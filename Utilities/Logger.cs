using System;
using System.Collections;
using System.Drawing;
using System.IO;
using Microsoft.Office.Interop.Excel;

namespace Utilities
{
    public delegate void MessageHandler(string message);

    /// <summary>
    /// Summary description for Logger.
    /// </summary>
    public class Logger
    {
        public delegate int OperationToPerformOnBook(Worksheet sheet, object[] logRows);

        public const int excelMaxNoRows = 65536;
                         // håller reda på hur många rader som kan finnas i ett Excelark, (<=Ex2003 har max 65536 (2^16)rader)
        private static readonly Hashtable _uniqueLoggerErrorMessages = new Hashtable();
        private readonly Hashtable _sheets = new Hashtable();
                                   // blir en lista med sheetName = namnet på arket, lastRow = sista raden i arket, subSheet = hur många delark som har samma början på namnet det finns Ex. Prov_part2
        private readonly Hashtable _testInfo = new Hashtable();
        private bool _GeneralSheetCreated;

        private Application _app;

        private Workbook _book;

        private Worksheet _last;

        // Temp for addrow
        private Worksheet _nextSheet;
        private Worksheet _sheet;

        public Logger()
            : this(true)
        {
        }

        public Logger(bool changeCulture)
        {
            if (changeCulture)
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            }

            // Sökvägen till alla loggar
            var logPath = string.Format(@"{0}Logs", AppDomain.CurrentDomain.BaseDirectory);

            Directory.CreateDirectory(logPath);

            logPath = string.Format(
                @"{0}Logs\{1}-{2:00}-{3:00} {4:00}-{5:00}-{6:00}.xls", 
                AppDomain.CurrentDomain.BaseDirectory, 
                DateTime.Now.Year, 
                DateTime.Now.Month, 
                DateTime.Now.Day, 
                DateTime.Now.Hour, 
                DateTime.Now.Minute, 
                DateTime.Now.Second);

            _app = new Microsoft.Office.Interop.Excel.ApplicationClass();
            _app.WorkbookDeactivate += Application_WorkbookDeactivate;

            _book = _app.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);

            _sheet = _book.Sheets[1] as Microsoft.Office.Interop.Excel.Worksheet; // Excel.Worksheet 
            _sheet.Name = "Info";
            _sheets.Add("Info", new object[] { _sheet, 1, 0 });
            _last = _sheet;

            SaveWorkBook(_book, logPath);
        }

        public Application ExcelApplication
        {
            get
            {
                return _app;
            }

            // set { _app = value; }
        }
        public Workbook Book
        {
            get
            {
                return _book;
            }
        }
        public event MessageHandler OnLog;

        private static void SaveWorkBook(Workbook book, string logPath)
        {
            book.SaveAs(
                logPath, 
                // Filename
                Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal, 
                // FileFormat
                Type.Missing, 
                // Password
                Type.Missing, 
                // WriteResPassword
                false, 
                // ReadOnlyRecommended
                Type.Missing, 
                Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, 
                Microsoft.Office.Interop.Excel.XlSaveConflictResolution.xlLocalSessionChanges, 
                // ConflictResolution. Spara över ändringar med lokala (man har ju tryckt på att spara.
                Type.Missing, 
                Type.Missing, 
                Type.Missing, 
                Type.Missing);
        }

        // Sub SaveAs( _
        // <InAttribute()> Optional ByVal Filename As Object, _
        // <InAttribute()> Optional ByVal FileFormat As Object, _
        // <InAttribute()> Optional ByVal Password As Object, _
        // <InAttribute()> Optional ByVal WriteResPassword As Object, _
        // <InAttribute()> Optional ByVal ReadOnlyRecommended As Object, _
        // <InAttribute()> Optional ByVal CreateBackup As Object, _
        // <InAttribute()> Optional ByVal AccessMode As XlSaveAsAccessMode, _
        // <InAttribute()> Optional ByVal ConflictResolution As Object, _
        // <InAttribute()> Optional ByVal AddToMru As Object, _
        // <InAttribute()> Optional ByVal TextCodepage As Object, _
        // <InAttribute()> Optional ByVal TextVisualLayout As Object, _
        // <InAttribute()> Optional ByVal Local As Object _
        // )
        private void Application_WorkbookDeactivate(Workbook wb)
        {
            // Stäng och släpp excel
            _app.Quit();

            while (System.Runtime.InteropServices.Marshal.ReleaseComObject(_app) != 0)
            {
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            // ReSharper disable RedundantAssignment
            // Wants to be sure excelAppOpen is cleared
            _app = null;

            // ReSharper restore RedundantAssignment
        }

        /// <summary>
        /// Loggar rader i tabellen till filen på angiven path
        /// </summary>
        /// <param name="excelBookPath">Path to Excel file</param>
        /// <param name="rowsToWrite">Table with rows to write to file</param>
        /// <param name="sheetName">Name of sheet to write to</param>
        /// <returns>Todo, return lastrow written to</returns>
        public static int WriteToWorkBook(string excelBookPath, string sheetName, bool overWrite, Hashtable rowsToWrite)
        {
            return WriteToWorkBook(excelBookPath, sheetName, null, null, overWrite, rowsToWrite);
        }

        /// <summary>
        /// Gets a workbook for saving purposes
        /// </summary>
        /// <param name="excelBookPath">path to Excel file</param>
        /// <returns></returns>
        public static int WriteToWorkBook(
            string excelBookPath, 
            string sheetName, 
            OperationToPerformOnBook operation, 
            object[] rowToWrite, 
            bool overWrite, 
            Hashtable rowsToWrite)
        {
            #region Todo

            // Todo:
            // Skapa klass, med tabell över Sheets som nyklar och specialklass för det som finns i sheetet, som ska innehålla; sheetet, tabbell med arrayer med cellinnehållet (helst strängarrayer med unika nyklar) (man kan även ha formatering lagrat för varje rad eller cell, men den informationen ska ligga separat, och sättas sist, när alla rader skrivits) , antalet dubbleter vid radoverflow, (redan skrivna rader => kan man få från sheetet själv)
            // Hantera radoverflow
            // Ta bort det som returneras eller returnera sista raden skriven till
            // Optimera genom att skriva flera rader på en gång

            // Done:
            // Ge möjlighet till att välja sheet 
            #endregion

            var excelApp = new Microsoft.Office.Interop.Excel.Application();

            Workbook excelBook = null;

            #region Öppna

            try
            {
                // Todo, ha denna som egen fkn , som returnerar en bok
                #region Öppna filen

                // Öppna filen
                excelBook = excelApp.Workbooks._Open(
                    excelBookPath, 
                    // filename,
                    Type.Missing, 
                    0, 
                    Type.Missing, 
                    Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, 
                    // XlTextQualifier.xlTextQualifierNone,
                    Type.Missing, 
                    Type.Missing, 
                    Type.Missing, 
                    false, 
                    // COMMA
                    Type.Missing, 
                    Type.Missing, 
                    Type.Missing, 
                    Type.Missing);

                #endregion

                // Disable calculation while writing
                excelApp.Calculation = Microsoft.Office.Interop.Excel.XlCalculation.xlCalculationManual;

                // get the collection of sheets in the workbook
                var Sheets = excelBook.Worksheets;
                var numOfSheets = excelBook.Worksheets.Count;

                var startSheetNumber = 1;

                // get the first worksheet from the collection of worksheets
                var workSheet = (Microsoft.Office.Interop.Excel.Worksheet)Sheets.get_Item(startSheetNumber);
                if (sheetName != "")
                {
                    #region Hämta ut rätt sheet

                    workSheet = null;

                    // Hämta ut ett ark med inskickat namn
                    foreach (Worksheet currentSheet in Sheets)
                    {
                        if (currentSheet.Name == sheetName)
                        {
                            workSheet = currentSheet;

                            break;
                        }

                        startSheetNumber++;
                    }

                    numOfSheets = startSheetNumber; // +1 behövs ej eftersom loopen har  <= numOfSheets

                    // string localSheetName = ((Excel.Worksheet)Sheets.get_Item(sheetNr)).Name;//Excelarknamnet
                    // workSheet = (Excel.Worksheet)Sheets.get_Item(sheetNr);//Här byts ju worksheet ändå, så att sätta worksheet ovan blir verkningslöst
                    if (workSheet == null)
                    {
                        throw new Exception("Sheet not found: " + sheetName + ". In: " + excelBookPath);
                    }

                    // Då tas första? nej, avsluta isåfall//return -1; 
                    #endregion
                }

                var orgRowCount = overWrite ? 0 : workSheet.UsedRange.Rows.Count;

                // Rensa sheet så det inte blir kvar gammalt om antalet rader är mindre
                if (overWrite)
                {
                    workSheet.Cells.Clear();
                }

                if (operation != null)
                {
                    return operation(workSheet, rowToWrite);
                }
                else
                {
                    #region Skriv en eller flera rader

                    var oa = new object[] { workSheet, orgRowCount + 1, 0 }; // +1 så den sista raden inte skrivs över

                    if (rowToWrite != null) // Skriver en rad
                    {
                        addRow(workSheet, "", ref oa, null, false, System.Drawing.Color.Empty, 0, 0, rowToWrite);
                    }
                    else if (rowsToWrite != null) // Skriver flera rader
                    {
                        foreach (var currentRow in rowsToWrite.Values)
                        {
                            addRow(workSheet, "", ref oa, null, false, System.Drawing.Color.Empty, 0, 0, currentRow);
                        }
                    }

                    #endregion
                }

                // Enable calculation after writing is done
                excelApp.Calculation = Microsoft.Office.Interop.Excel.XlCalculation.xlCalculationAutomatic;
            }
            catch (Exception e)
            {
                #region Exception

                excelApp.Quit(); // Stäng excel
                System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);

                // MessageBox.Show("Error in retrieving old log. Was the log opened in Excel during compare processing?\r\n\r\n(Sys err: " + e.Message + ").");
                throw new Exception(
                    "Error in retrieving log. Was the log opened in Excel during compare processing?\r\n\r\n(Sys err: "
                    + e.Message + ").", 
                    e);

                #endregion
            }

            // Spara
            if (overWrite)
            {
                excelApp.DisplayAlerts = false;
            }

            SaveWorkBook(excelBook, excelBookPath);
            if (overWrite)
            {
                excelApp.DisplayAlerts = true;
            }

            excelApp.Quit(); // Stäng Excel
            System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);

            #endregion

            return -1;
        }

        public int WriteToWorkBookInstantiatedNotStatic(
            string excelBookPath, string sheetName, bool overWrite, Hashtable rowsToWrite)
        {
            return WriteToWorkBook(excelBookPath, sheetName, null, null, overWrite, rowsToWrite);
        }

        private bool GeneralSheetCreated()
        {
            if (!_GeneralSheetCreated)
            {
                // För felmeddelanden
                // Excel.Worksheet sheet
                _sheet =
                    _book.Worksheets.Add(Type.Missing, _last, Type.Missing, Type.Missing) as
                    Microsoft.Office.Interop.Excel.Worksheet;
                _sheet.Name = "General";
                _sheets.Add("General", new object[] { _sheet, 1, 0 });
                _last = _sheet;

                _GeneralSheetCreated = true;

                addRow("General", true, 0, "General exception messages");

                // LogMessage("General", "---", "");
            }

            return _GeneralSheetCreated;
        }

        public void Close()
        {
            if (_app == null)
            {
                return;
            }

            // något strul med stoppknappen
            try
            {
                // _book.Save();
                _book.Close(false, Type.Missing, Type.Missing);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in closing excel." + e);
            }

            // _app. blir den 

            // ---
            try
            {
                foreach (object[] sheetWInfo in _sheets.Values)
                {
                    // _sheet = oa[0] as Excel.Worksheet;
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(
                        sheetWInfo[0] as Microsoft.Office.Interop.Excel.Worksheet);
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(_book);
                _book = null;
            }
            catch
            {
            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(_last);
            if (_sheet != null)
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(_sheet);
            }

            if (_nextSheet != null)
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(_nextSheet);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            _app.Quit();

            while (System.Runtime.InteropServices.Marshal.ReleaseComObject(_app) != 0)
            {
            }

            _app = null;

            // _book.Close(0, "","");//PG
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void AddTest(string name, TestInfo testInfo)
        {
            // Excel.Worksheet 
            _sheet =
                _book.Worksheets.Add(Type.Missing, _last, Type.Missing, Type.Missing) as
                Microsoft.Office.Interop.Excel.Worksheet;
            _last = _sheet;
            _sheet.Name = name;
            _sheets.Add(name, new object[] { _sheet, 4, 1 });

            _sheet.Cells[1, 1] = "";
            _sheet.Cells[3, 1] = "";

            _testInfo.Add(name, testInfo);
        }

        public void AddTestColumnNamesEtc(string name)
        {
            // Skippa Allprofiles
            if (name.StartsWith("AllProfiles"))
            {
                return;
            }

            var testInfo = _testInfo[name] as TestInfo;
            if (testInfo != null)
            {
                var cellLayout = new Hashtable();
                cellLayout.Add(CellLayOutSettings.Bold, true);

                if (testInfo.Columns[0] == "Total number of:") // if(name = "DataBaseInfo")
                {
                    addRow(name, cellLayout, true, 3, testInfo.Columns);
                }
                else
                {
                    addRow(name, cellLayout, true, 3, testInfo.Columns);
                }

                addRow(name, false, 1, testInfo.Description); // skriver beskrivningen sist så inte den autofittas
            }
        }

        // public void Log(string sheetName, object autofitAlwaysTrue, int insertInRow, params object[] args)
        // {
        // Log(sheetName , null, insertInRow, args);//Används aldrig
        // }
        public void Log(string sheetName, int insertInRow, params object[] args)
        {
            Log(sheetName, false, insertInRow, args);
        }

        public void Log(
            string sheetName, Hashtable cellLayOutSettings, bool autofit, int insertInRow, params object[] args)
        {
            addRow(sheetName, cellLayOutSettings, autofit, insertInRow, args);
        }

        public void Log(string sheetName, bool autofit, int insertInRow, params object[] args)
        {
            // Excel.Range newCell = 
            addRow(sheetName, autofit, insertInRow, args);
        }

        public void Log(Type t, int insertInRow, params object[] args)
        {
            var sheetName = t.Name;
            Log(sheetName, null, insertInRow, args);
        }

        public void Log(Type type_in, Hashtable cellLayOutSettings, int insertInRow, params object[] args)
        {
            Log(type_in.Name, cellLayOutSettings, insertInRow, args);
        }

        public void Log(string sheetName, Hashtable cellLayOutSettings, int insertInRow, params object[] args)
        {
            // Excel.Range newCell = 
            // Stor prestandaförlust om man autofittar för varje ny rad som skrivs, detta görs för överskriften (kolumnnamnen sen)
            addRow(sheetName, cellLayOutSettings, false, insertInRow, args); // true

            var ti = _testInfo[sheetName] as TestInfo;

            if (OnLog != null && ti != null && ti.InfoText != "")
            {
                // if (args.Length != ti.Columns.Length)
                // {
                // Console.WriteLine("Faulty TestInfo for {0}.", sheetName);
                // return;
                // }
                var s = "";
                for (var i = 0; i < args.Length; i++)
                {
                    s += string.Format("{0}{1}, ", ti.Columns.Length <= i ? "" : ti.Columns[i] + "=", args[i]);
                }

                if (s.Length > 0)
                {
                    s = s.Substring(0, s.Length - 2);
                }

                s = string.Format("{0}: {1}", ti.InfoText, s);
                OnLog(s);
            }
        }

        public void LogMessage(string type, int insertInRow, string message, params object[] args)
        {
            var logMessages = new object[args.Length + 1];
            logMessages[0] = message;
            var argNr = 1;
            message = type + " " + message + " ";
            foreach (string arg in args)
            {
                logMessages[argNr++] = arg;
                message += arg + " ";
            }

            addRow(type, false, insertInRow, logMessages);

            if (OnLog != null)
            {
                OnLog(message);
            }
        }

        public void LogMessage(string type, string message, params object[] args)
        {
            var logMessages = new object[args.Length + 1];
            logMessages[0] = message;
            if (args.Length > 0 && args[0].GetType() == typeof(string)) // .ToString().StartsWith("{"))
            {
                var argNr = 1;
                message = type + " " + message + " ";
                foreach (string arg in args)
                {
                    logMessages[argNr++] = arg;
                    message += arg + " ";
                }
            }

            addRow(type, true, 0, logMessages);

            if (OnLog != null)
            {
                OnLog(message);
            }
        }

        public void LogMessage(string type, bool autofit, string message, params object[] args) // For time messages
        {
            message = string.Format(message, args);

            addRow(type, autofit, 0, message);

            if (OnLog != null)
            {
                OnLog(message);
            }
        }

        // PG, Läser ett excelark och räknar unika rader inklusive testinfo, rubrikraden etc
        public int uniqeCountId(Type t, string inString) // flytta ev. denna till en mer passande klass
        {
            var sheetName = t.Name;

            if (_app == null)
            {
                return 0;
            }

            _sheet = null;

            if (!_sheets.Contains(sheetName))
            {
                return 0;
            }

            var numberOfUniqes = 0;
            var checkedIds = new Hashtable();

            var oa = _sheets[sheetName] as object[];
            _sheet = oa[0] as Microsoft.Office.Interop.Excel.Worksheet;
            var maxRows = (int)oa[1];
            var maxCols = 7;

            if (maxRows > 1)
            {
                for (var i = 1; i < maxRows + 1; i++)
                {
                    for (var j = 1; j < maxCols + 1; j++)
                    {
                        var stemp = _sheet.Cells[i, j].ToString();
                            
                            // får bara ut typen som sträng system.object...kanske ska köra med VCC-räkning under körning, gör det nu
                        if (stemp.StartsWith(inString) && checkedIds[stemp] == null)
                        {
                            checkedIds.Add(stemp, 1);
                            numberOfUniqes++;
                        }
                    }
                }
            }

            return numberOfUniqes;
        }

        public void addRow(string sheetName, bool autofit, int insertInRow, params object[] args)
        {
            addRow(sheetName, null, autofit, insertInRow, args);
        }

        /// <summary>
        /// Adds a row, For autofit to be good, the rows without autofit shold be written last.
        /// </summary>
        /// <param name="sheetName">Name of the sheet to add in</param>
        /// <param name="autofit">For autofit to be good, the rows without autofit shold be written last.</param>
        /// <param name="insertInRow">Row to set in cell</param>
        /// <param name="args">What to fill cells with</param>
        private void addRow(
            string sheetName, Hashtable cellLayOutSettings, bool autofit, int insertInRow, params object[] args)
            // Done vad som va målet inte det som står th.: returnera cellen eller cellrange o gör det möjligt att i efterhand göra autoFitColumnWidth. //Excel.Range addRow
        {
            try
            {
                if (sheetName == "General")
                {
                    GeneralSheetCreated();
                }

                // Excel.Range cellRange = null;
                if (_app == null)
                {
                    return; // null;
                }

                _sheet = null;

                // Excel.Worksheet sheet = null;
                if (!_sheets.Contains(sheetName))
                {
                    return; // null;
                }

                var oa = _sheets[sheetName] as object[];

                // här ska koll på arknummer göras
                /*
                om o[2] är >1	så ska det numrets delark laddas
	
                    sheet ska vara = sheetName + o[2].tostring()
                    leta bland sheets efter Arket med det namnet
                    sheet = HL[ sheetName + o[2].tostring()]
 
                */
                string saveAsSheetName;
                if ((int)oa[2] > 1)
                {
                    saveAsSheetName = sheetName + "_part" + oa[2];
                    oa = _sheets[sheetName + "_part" + oa[2]] as object[]; // oa[0] as Excel.Worksheet;	
                }
                else
                {
                    saveAsSheetName = sheetName;
                }

                _sheet = oa[0] as Microsoft.Office.Interop.Excel.Worksheet;

                var nextRow = addRow(
                    _sheet, 
                    saveAsSheetName, 
                    ref oa, 
                    cellLayOutSettings, 
                    autofit, 
                    System.Drawing.Color.Empty, 
                    insertInRow, 
                    args);

                if (nextRow > (excelMaxNoRows - 1)) // tar sista raden oxå //(excelMaxNoRows-2) )//tar ínte allra sista raden för säkerhets skull
                {
                    // Gör ett nytt ark med samma namn + siffra (EX. Prov_part2)

                    // Ev. skriv något på sista raden typ: "Fortsättning på nästa ark _part2...
                    _nextSheet =
                        _book.Worksheets.Add(Type.Missing, _last, Type.Missing, Type.Missing) as
                        Microsoft.Office.Interop.Excel.Worksheet;
                    _last = _nextSheet;

                    var orgOa = _sheets[sheetName] as object[];
                    orgOa[2] = (int)orgOa[2] + 1; // ökar antal delark i en log
                    var newSheetName = sheetName + "_part" + orgOa[2]; // Ex. Prov_part2
                    _nextSheet.Name = newSheetName;
                    _sheets.Add(newSheetName, new object[] { _nextSheet, 4, 0 });
                }

                // _sheets[sheetName] = oa;
                _sheets[saveAsSheetName] = oa;

                // return cellRange;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Logger, may be Excel error: " + e.Message);

                // throw e;
                // return null;
            }
        }

        public static int addRow(
            Worksheet sheet, 
            string saveAsSheetName, 
            ref object[] oa, 
            Hashtable cellLayOutSettings, 
            bool autofit, 
            Color color, 
            int insertInRow, 
            params object[] args)
            // Done vad som va målet inte det som står th.: returnera cellen eller cellrange o gör det möjligt att i efterhand göra autoFitColumnWidth. //Excel.Range addRow
        {
            return addRow(sheet, saveAsSheetName, ref oa, cellLayOutSettings, autofit, color, insertInRow, 0, args);
        }

        public static int addRow(
            Worksheet sheet, 
            string saveAsSheetName, 
            ref object[] oa, 
            Hashtable cellLayOutSettings, 
            bool autofit, 
            Color color, 
            int insertInRow, 
            int insertInColumn, 
            params object[] args)
            // Done vad som va målet inte det som står th.: returnera cellen eller cellrange o gör det möjligt att i efterhand göra autoFitColumnWidth. //Excel.Range addRow
        {
            try
            {
                if (args.Length == 0)
                {
                    return -1;
                }
                else if (args.Length.Equals(1) && (args[0] as object[]) != null)
                {
                    args = args[0] as object[];
                }

                var nextRow = (int)oa[1];

                // spara cellerna som det skrivs till i en sträng-array, skr sedan alla på en gång
                // string[,] cellsToWrite = new string[1, args.Length + insertInColumn];
                object[,] cellsToWrite = null;
                if (args.Length == 1) // onödig?, hmm nej, inte om det är special för 1 grejj, ska man inte kunna skriva enradsgrejjer till andra kolumnerm, hm det har med DbInfos new och +/- kolumn att göra troligt
                {
                    cellsToWrite = new object[1, args.Length]; // + insertInColumn
                }
                else
                {
                    cellsToWrite = new object[1, args.Length + insertInColumn];
                }

                // string toWriteIncells = args;
                #region Write each cell at a time to temp variable

                var rowWrittenTo = 0;
                for (var i = insertInColumn; i < args.Length + insertInColumn; i++)
                {
                    // string toWriteIncell = args[i - insertInColumn].ToString();

                    // Om det inte finns något att skriva, gå till nästa
                    if (args[i - insertInColumn] == null)
                    {
                        continue;
                    }

                    // Strängar längre än ca912 kan inte skrivas till en cell, uten ger ett exception med lite info i. Så längder över 900 tecken klipps bort.
                    // const int maxCellLength = 900;
                    // foreach (var arg in args[0] as object[])
                    // {
                    // if (arg.ToString().Length > maxCellLength)
                    // {
                    // arg = arg.ToString().Substring(0, 900);
                    // }
                    // }
                    var toWriteIncell = args[i - insertInColumn];

                    // (args[i - insertInColumn].ToString()).Length > 900 ?
                    // (args[i - insertInColumn].ToString()).Substring(0, 900)
                    // : args[i - insertInColumn].ToString();

                    // Det blir problem med celler som börjar med "=", och sedan inte ger en riktig formel, så detta sätts till
                    // TODO: Fixa något allmänt test för formler som kan gå fel, eller formatera rangen som text, men det vill man iofs inte alltid...
                    // if (toWriteIncell.ToString().StartsWith("=") && toWriteIncell.ToString().Contains("x")) toWriteIncell = " " + toWriteIncell;
                    rowWrittenTo = nextRow; // Vilken rad som verkligen skrivits till, används för layout av cellen
                    if (insertInRow > 0)
                    {
                        // sheet.Cells[insertInRow, i + 1] = toWriteIncell;
                        cellsToWrite[0, i - insertInColumn] = toWriteIncell;
                        rowWrittenTo = insertInRow;
                    }
                    else // if (args[i] != null)
                    {
                        // sheet.Cells[nextRow, i + 1] = toWriteIncell;
                        // cellsToWrite[0, i] = toWriteIncell;
                        cellsToWrite[0, i] = toWriteIncell; // (toWriteIncell as string);//.Length > 900 ?

                        // (toWriteIncell as string).Substring(0, 900)
                        // : toWriteIncell;
                    }

                    #region old Exceltester

                    // cellRange.Interior.ColorIndex = 36;//36 = Gul//Fungerar
                    // cellRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Green);//Fungerar

                    // rng.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Red);
                    // ((Excel.Worksheet)sheet.Activate())
                    // objRange.Font.Background = 4.0;
                    // string strData = objRange.get_Value(Type.Missing).ToString();
                    // objRange.Select();
                    // objRange.Style =  
                    // object tempObject = objRange.Borders.Color;

                    // objRange.Borders.Color = 5.0;//Ändrar faktiskt ramen för cellen

                    // ((Range)sheet.Cells[nextRow, i + 1]).AutoFit();
                    // sheet.Cells[nextRow, i + 1].AutoFit();
                    #endregion
                }

                #endregion

                // Write cells several at a tiem, Fill A2:B6 with an array of values (First and Last Names).
                var fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);
                    
                    // nextRow.ToString();
                var toColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(args.Length + insertInColumn);
                    
                    // nextRow.ToString();
                var cellRange = sheet.get_Range(
                    fromColumn + rowWrittenTo.ToString(), toColumn + rowWrittenTo.ToString());

                // Write to excel sheet
                cellRange.Value2 = cellsToWrite; // "A"

                #region buggtest Skriv till ExcelSheet

                // Excel.Range //cellRange = null;

                // object[,] tempp = new object[2, 5] { "a", "f", "g", "h" };
                // cellRange.Value2 = cellsToWrite;//"A"
                // object to = new object[] { cellsToWrite };

                // fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();
                // toColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(cellsToWrite.Length + insertInColumn);//nextRow.ToString();

                // cellRange = sheet.get_Range(fromColumn + rowWrittenTo.ToString(), toColumn + rowWrittenTo.ToString());

                // buggtest
                // object[,] tempp = new object[2, 5] { "a", "f", "g", "h" };
                // cellRange.Value2 = cellsToWrite;//"A"
                // object to = new object[] { cellsToWrite };
                #endregion

                #region Layout (färg, autofit column etc)

                if ((cellLayOutSettings != null && cellLayOutSettings.Count > 0) || autofit
                    || (color != System.Drawing.Color.Empty))
                {
                    // Excel.Range //cellRange = null;
                    // cellRange =
                    // sheet.get_Range(fromColumn + rowWrittenTo.ToString(), toColumn + rowWrittenTo.ToString());//"A" 
                    // (Excel.Range)sheet.Cells[rowWrittenTo, i + 1];
                    if (cellLayOutSettings != null && cellLayOutSettings.Count > 0)
                    {
                        EditCellLayOut(cellLayOutSettings, cellRange);
                    }

                    // Det hade med insertrow att göra, så det va fel range hela tiden...Inte ens detta ger bold på columnnamnen
                    // Excel.Range cellRanges = (Excel.Range)sheet.Cells[nextRow, i + 1];
                    // cellRanges.Font.Bold = true;
                    // cellRange.Font.Color = System.Drawing.ColorTranslator.ToOle(color);
                    // color = System.Drawing.Color.Empty;
                    if (autofit)
                    {
                        cellRange.EntireColumn.AutoFit(); // autofittar hela columnen för all som loggas

                        // cellRange.Font.Bold = true;
                    }

                    if (color != System.Drawing.Color.Empty)
                    {
                        cellRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(color); // Fungerar
                    }
                }

                #endregion

                System.Runtime.InteropServices.Marshal.ReleaseComObject(cellRange);
                cellRange = null;

                if (insertInRow > 0)
                {
                    nextRow--;
                }

                oa[1] = nextRow + 1; // efter detta ska det kollas om maxrader är uppnått

                return nextRow;
            }
            catch (Exception e)
            {
                var allArgs = "";
                foreach (var item in args)
                {
                    allArgs += ";" + item;
                }

                if (allArgs == "")
                {
                    allArgs = "<empty>";
                }

                var errMess = "Error in Logger. In sheet; " + saveAsSheetName + ", may be Excel error: " + e.Message
                              + "\r\n" + "Tried to Log" + allArgs;
                Console.WriteLine(errMess);

                try
                {
                    if (!_uniqueLoggerErrorMessages.ContainsKey(errMess))
                    {
                        _uniqueLoggerErrorMessages.Add(errMess, 1);

                        // Kolla så inte samma skrivs ut hela tiden
                        TextWriter tW =
                            new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"Logs\LoggerExceptions.txt");

                        var toLogerrMess = "";
                        foreach (var item in _uniqueLoggerErrorMessages.Keys)
                        {
                            toLogerrMess += item;
                        }

                        tW.Write(toLogerrMess);

                        tW.Close();
                    }
                }
                catch (Exception excExcp)
                {
                    Console.WriteLine(
                        "Error in Logger in sheet; " + saveAsSheetName + ", error with error reporting: "
                        + excExcp.Message);
                }

                // throw e;
                // return null;
                return -1;
            }
        }

        public static void EditCellLayOut(Hashtable settings, Range cellRange)
        {
            try
            {
                foreach (DictionaryEntry currentSetting in settings)
                {
                    var settingType = (CellLayOutSettings)currentSetting.Key;

                    switch (settingType)
                    {
                        case CellLayOutSettings.Bold:
                            cellRange.Font.Bold = (bool)currentSetting.Value;
                            break;
                        case CellLayOutSettings.UnderLined:
                            cellRange.Font.Underline = (bool)currentSetting.Value;
                            break;
                        case CellLayOutSettings.FontStyle:
                            cellRange.Font.FontStyle =
                                (currentSetting.Value as Microsoft.Office.Interop.Excel.Font).FontStyle;
                            break;
                        case CellLayOutSettings.TextColor:
                            cellRange.Font.Color =
                                System.Drawing.ColorTranslator.ToOle((System.Drawing.Color)currentSetting.Value);
                            break;
                        case CellLayOutSettings.InteriorColorSysDrawingType:
                            cellRange.Interior.Color =
                                System.Drawing.ColorTranslator.ToOle((System.Drawing.Color)currentSetting.Value);
                                
                                // System.Drawing.ColorTranslator.ToOle(color);
                            break;
                        case CellLayOutSettings.InteriorColorColorIndexType:
                            cellRange.Interior.ColorIndex = (int)currentSetting.Value;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in EditCellLayOut in Logger: " + e.Message);
            }
        }
    }

    public enum CellLayOutSettings
    {
        Bold, 
        UnderLined, 
        FontStyle, 
        TextColor, 
        InteriorColorSysDrawingType, // System.Drawing.Color.GreenYellow
        InteriorColorColorIndexType // range.Interior.ColorIndex = 36
    }

    public enum XlWBATemplate
    {
        xlWBATWorksheet = -4167, 
        xlWBATChart = -4109, 
        xlWBATExcel4MacroSheet = 3, 
        xlWBATExcel4IntlMacroSheet = 4, 
    }

    /// <summary>
    /// Summary description for TestInfo.
    /// </summary>
    public class TestInfo : Attribute
    {
        public string[] Columns = null;
        public string Description = "";
        public string InfoText = "";

        public TestInfo(string description, string infoText, params string[] columns)
        {
            Description = description;
            InfoText = infoText;
            Columns = columns;
        }
    }
}

/*
"
fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();lx32131616516516516uuuuuuuuuuu255
"fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();l32131616516516516uuuuuuyyuuuuutt255"
"fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1)xxxnextRow.ToString()xfromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1)XXxnextRow.ToString()Xl32131616516516516uuuuuuyyuuuuutt255"
"fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1)xxxnextRow.ToString()xfromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1)XXxnextRow.ToString()Xl32131616516516516uuuuuuyyuuuuutt255fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1)xxxnextRow.ToString()xfromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1)XXxnextRow.ToString()Xl32131616516516516uuuuuuyyuuuuutt255"
"fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();lx32131616516516516uuuuuuuuuuu255fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();lx32131616516516516uuuuuuuuuuu255"
"fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();lx32131616516516516uuuuuuuuuuu255fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();lx32131616516516516uuuuuuuuuuu255fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();lx32131616516516516uuuuuuuuuuu255"
"fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();lx32131616516516516uuuuuuuuuuu255fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();lx32131616516516516uuuuuuuuuuu255fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();lx32131616516516516uuuuuuuuuuu255fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();lx32131616516516516uuuuuuuuuuutr255"

"fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();lx32131616516516516uuuuuuuuuuu255fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();lx32131616516516516uuuuuuuuuuu255fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();lx32131616516516516uuuuuuuusdfsfgsfgsfgsfggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggguuxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxuxx255"
dsd 
 * 
 * 
 * Gräns vid 912 tecken
"
 * 
fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();lx32131616516516516uuuuuuuuuuu255fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();lx32131616516516516uuuuuuuuuuu255fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();fromColumn = Utilities.ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);//nextRow.ToString();lx32131616516516516uuuuuuuusdfsfgsfgsfgsfgggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggpppgggggggxxxuxx255"
 */