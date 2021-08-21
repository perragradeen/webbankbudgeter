using Microsoft.Office.Interop.Excel;
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo

namespace Utilities
{
    /// <summary>
    /// Summary description for Logger.
    /// </summary>
    public class Logger
    {
        public delegate int OperationToPerformOnBook(Worksheet sheet, object[] logRows);

        public const int excelMaxNoRows = 65536;
        // h�ller reda p� hur m�nga rader som kan finnas i ett Excelark, (<=Ex2003 har max 65536 (2^16)rader)
        private static readonly Hashtable _uniqueLoggerErrorMessages = new Hashtable();
        private readonly Hashtable _sheets = new Hashtable();
        // blir en lista med sheetName = namnet p� arket, lastRow = sista raden i arket, subSheet = hur m�nga delark som har samma b�rjan p� namnet det finns Ex. Prov_part2
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

            // S�kv�gen till alla loggar
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

            _app = new ApplicationClass();
            _app.WorkbookDeactivate += Application_WorkbookDeactivate;

            _book = _app.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);

            _sheet = _book.Sheets[1] as Worksheet;
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
                XlFileFormat.xlWorkbookNormal,
                // FileFormat
                Type.Missing,
                // Password
                Type.Missing,
                // WriteResPassword
                false,
                // ReadOnlyRecommended
                Type.Missing,
                XlSaveAsAccessMode.xlExclusive,
                XlSaveConflictResolution.xlLocalSessionChanges,
                // ConflictResolution. Spara �ver �ndringar med lokala (man har ju tryckt p� att spara.
                Type.Missing,
                Type.Missing,
                Type.Missing,
                Type.Missing);
        }

        private void Application_WorkbookDeactivate(Workbook wb)
        {
            // St�ng och sl�pp excel
            _app.Quit();

            while (Marshal.ReleaseComObject(_app) != 0)
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
        /// Loggar rader i tabellen till filen p� angiven path
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
            // Skapa klass, med tabell �ver Sheets som nyklar och specialklass f�r det som finns i sheetet, som ska inneh�lla; sheetet, tabbell med arrayer med cellinneh�llet (helst str�ngarrayer med unika nyklar) (man kan �ven ha formatering lagrat f�r varje rad eller cell, men den informationen ska ligga separat, och s�ttas sist, n�r alla rader skrivits) , antalet dubbleter vid radoverflow, (redan skrivna rader => kan man f� fr�n sheetet sj�lv)
            // Hantera radoverflow
            // Ta bort det som returneras eller returnera sista raden skriven till
            // Optimera genom att skriva flera rader p� en g�ng

            // Done:
            // Ge m�jlighet till att v�lja sheet 
            #endregion

            var excelApp = new Application();

            Workbook excelBook;

            #region �ppna

            try
            {
                // Todo, ha denna som egen fkn , som returnerar en bok
                #region �ppna filen

                // �ppna filen
                excelBook = excelApp.Workbooks._Open(
                    excelBookPath,
                    // filename,
                    Type.Missing,
                    0,
                    Type.Missing,
                    XlPlatform.xlWindows,
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
                excelApp.Calculation = XlCalculation.xlCalculationManual;

                // get the collection of sheets in the workbook
                var Sheets = excelBook.Worksheets;
                var numOfSheets = excelBook.Worksheets.Count;

                var startSheetNumber = 1;

                // get the first worksheet from the collection of worksheets
                var workSheet = (Worksheet)Sheets.get_Item(startSheetNumber);
                if (sheetName != "")
                {
                    #region H�mta ut r�tt sheet

                    workSheet = null;

                    // H�mta ut ett ark med inskickat namn
                    foreach (Worksheet currentSheet in Sheets)
                    {
                        if (currentSheet.Name == sheetName)
                        {
                            workSheet = currentSheet;

                            break;
                        }

                        startSheetNumber++;
                    }

                    numOfSheets = startSheetNumber; // +1 beh�vs ej eftersom loopen har  <= numOfSheets

                    if (workSheet == null)
                    {
                        throw new Exception("Sheet not found: " + sheetName + ". In: " + excelBookPath);
                    }

                    // D� tas f�rsta? nej, avsluta is�fall//return -1; 
                    #endregion
                }

                var orgRowCount = overWrite ? 0 : workSheet.UsedRange.Rows.Count;

                // Rensa sheet s� det inte blir kvar gammalt om antalet rader �r mindre
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

                    var oa = new object[] { workSheet, orgRowCount + 1, 0 }; // +1 s� den sista raden inte skrivs �ver

                    if (rowToWrite != null) // Skriver en rad
                    {
                        AddRow(workSheet, "", ref oa, null, false, Color.Empty, 0, 0, rowToWrite);
                    }
                    else if (rowsToWrite != null) // Skriver flera rader
                    {
                        foreach (var currentRow in rowsToWrite.Values)
                        {
                            AddRow(workSheet, "", ref oa, null, false, Color.Empty, 0, 0, currentRow);
                        }
                    }

                    #endregion
                }

                // Enable calculation after writing is done
                excelApp.Calculation = XlCalculation.xlCalculationAutomatic;
            }
            catch (Exception e)
            {
                #region Exception

                excelApp.Quit(); // St�ng excel
                Marshal.ReleaseComObject(excelApp);

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

            excelApp.Quit(); // St�ng Excel
            Marshal.ReleaseComObject(excelApp);

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
                // F�r felmeddelanden
                _sheet =
                    _book.Worksheets.Add(Type.Missing, _last, Type.Missing, Type.Missing) as
                    Worksheet;
                _sheet.Name = "General";
                _sheets.Add("General", new object[] { _sheet, 1, 0 });
                _last = _sheet;

                _GeneralSheetCreated = true;

                AddRow("General", true, 0, "General exception messages");
            }

            return _GeneralSheetCreated;
        }

        public void Close()
        {
            if (_app == null)
            {
                return;
            }

            // n�got strul med stoppknappen
            try
            {
                _book.Close(false, Type.Missing, Type.Missing);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in closing excel." + e);
            }

            try
            {
                foreach (object[] sheetWInfo in _sheets.Values)
                {
                    if (sheetWInfo[0] is Worksheet workSheetToRelease)
                        Marshal.ReleaseComObject(
                            workSheetToRelease);
                }

                Marshal.ReleaseComObject(_book);
                _book = null;
            }
            catch
            {
                // ignored
            }

            Marshal.ReleaseComObject(_last);
            if (_sheet != null)
            {
                Marshal.ReleaseComObject(_sheet);
            }

            if (_nextSheet != null)
            {
                Marshal.ReleaseComObject(_nextSheet);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            _app.Quit();

            while (Marshal.ReleaseComObject(_app) != 0)
            {
            }

            _app = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void AddTest(string name, TestInfo testInfo)
        {
            _sheet =
                _book.Worksheets.Add(Type.Missing, _last, Type.Missing, Type.Missing) as
                Worksheet;
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

            if (_testInfo[name] is TestInfo testInfo)
            {
                var cellLayout = new Hashtable
                {
                    {CellLayOutSettings.Bold, true}
                };

                if (testInfo.Columns[0] == "Total number of:")
                {
                    AddRow(name, cellLayout, true, 3, testInfo.Columns);
                }
                else
                {
                    AddRow(name, cellLayout, true, 3, testInfo.Columns);
                }

                AddRow(name, false, 1, testInfo.Description); // skriver beskrivningen sist s� inte den autofittas
            }
        }

        public void Log(string sheetName, int insertInRow, params object[] args)
        {
            Log(sheetName, false, insertInRow, args);
        }

        public void Log(
            string sheetName, Hashtable cellLayOutSettings, bool autofit, int insertInRow, params object[] args)
        {
            AddRow(sheetName, cellLayOutSettings, autofit, insertInRow, args);
        }

        public void Log(string sheetName, bool autofit, int insertInRow, params object[] args)
        {
            AddRow(sheetName, autofit, insertInRow, args);
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
            // Stor prestandaf�rlust om man autofittar f�r varje ny rad som skrivs, detta g�rs f�r �verskriften (kolumnnamnen sen)
            AddRow(sheetName, cellLayOutSettings, false, insertInRow, args); // true

            if (OnLog == null || !(_testInfo[sheetName] is TestInfo ti) || ti.InfoText == "") return;

            var s = "";
            for (var i = 0; i < args.Length; i++)
            {
                s += $"{(ti.Columns.Length <= i ? "" : ti.Columns[i] + "=")}{args[i]}, ";
            }

            if (s.Length > 0)
            {
                s = s.Substring(0, s.Length - 2);
            }

            s = string.Format("{0}: {1}", ti.InfoText, s);
            OnLog(s);
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

            AddRow(type, false, insertInRow, logMessages);

            OnLog?.Invoke(message);
        }

        public void LogMessage(string type, string message, params object[] args)
        {
            var logMessages = new object[args.Length + 1];
            logMessages[0] = message;
            if (args.Length > 0 && args[0].GetType() == typeof(string))
            {
                var argNr = 1;
                message = type + " " + message + " ";
                foreach (string arg in args)
                {
                    logMessages[argNr++] = arg;
                    message += arg + " ";
                }
            }

            AddRow(type, true, 0, logMessages);

            OnLog?.Invoke(message);
        }

        public void LogMessage(string type, bool autofit, string message, params object[] args) // For time messages
        {
            message = string.Format(message, args);

            AddRow(type, autofit, 0, message);

            OnLog?.Invoke(message);
        }

        // PG, L�ser ett excelark och r�knar unika rader inklusive testinfo, rubrikraden etc
        public int UniqeCountId(Type t, string inString) // flytta ev. denna till en mer passande klass
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
            _sheet = oa[0] as Worksheet;
            var maxRows = (int)oa[1];
            var maxCols = 7;

            if (maxRows > 1)
            {
                for (var i = 1; i < maxRows + 1; i++)
                {
                    for (var j = 1; j < maxCols + 1; j++)
                    {
                        var stemp = _sheet.Cells[i, j].ToString();

                        // f�r bara ut typen som str�ng system.object...kanske ska k�ra med VCC-r�kning under k�rning, g�r det nu
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

        public void AddRow(string sheetName, bool autofit, int insertInRow, params object[] args)
        {
            AddRow(sheetName, null, autofit, insertInRow, args);
        }

        /// <summary>
        /// Adds a row, For autofit to be good, the rows without autofit shold be written last.
        /// </summary>
        /// <param name="sheetName">Name of the sheet to add in</param>
        /// <param name="cellLayOutSettings">Design of cell pretty</param>
        /// <param name="autofit">For autofit to be good, the rows without autofit shold be written last.</param>
        /// <param name="insertInRow">Row to set in cell</param>
        /// <param name="args">What to fill cells with</param>
        private void AddRow(
            string sheetName, Hashtable cellLayOutSettings, bool autofit, int insertInRow, params object[] args)
        // Done vad som va m�let inte det som st�r th.: returnera cellen eller cellrange o g�r det m�jligt att i efterhand g�ra autoFitColumnWidth. //Excel.Range addRow
        {
            try
            {
                if (sheetName == "General")
                {
                    GeneralSheetCreated();
                }

                if (_app == null)
                {
                    return; // null;
                }

                _sheet = null;

                if (!_sheets.Contains(sheetName))
                {
                    return; // null;
                }

                var oa = _sheets[sheetName] as object[];

                // h�r ska koll p� arknummer g�ras
                /*
                om o[2] �r >1 s� ska det numrets delark laddas
 
                    sheet ska vara = sheetName + o[2].tostring()
                    leta bland sheets efter Arket med det namnet
                    sheet = HL[ sheetName + o[2].tostring()]
 
                */
                string saveAsSheetName;
                if ((int)oa[2] > 1)
                {
                    saveAsSheetName = sheetName + "_part" + oa[2];
                    oa = _sheets[sheetName + "_part" + oa[2]] as object[];
                }
                else
                {
                    saveAsSheetName = sheetName;
                }

                _sheet = (Worksheet) oa?[0];

                var nextRow = AddRow(
                    _sheet,
                    saveAsSheetName,
                    ref oa,
                    cellLayOutSettings,
                    autofit,
                    Color.Empty,
                    insertInRow,
                    args);

                if (nextRow > (excelMaxNoRows - 1)) // tar sista raden ox� //(excelMaxNoRows-2) )//tar �nte allra sista raden f�r s�kerhets skull
                {
                    // G�r ett nytt ark med samma namn + siffra (EX. Prov_part2)

                    // Ev. skriv n�got p� sista raden typ: "Forts�ttning p� n�sta ark _part2...
                    _nextSheet =
                        _book.Worksheets.Add(Type.Missing, _last, Type.Missing, Type.Missing) as
                        Worksheet;
                    _last = _nextSheet;

                    var orgOa = _sheets[sheetName] as object[];
                    orgOa[2] = (int)orgOa[2] + 1; // �kar antal delark i en log
                    var newSheetName = sheetName + "_part" + orgOa[2]; // Ex. Prov_part2
                    _nextSheet.Name = newSheetName;
                    _sheets.Add(newSheetName, new object[] { _nextSheet, 4, 0 });
                }

                _sheets[saveAsSheetName] = oa;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Logger, may be Excel error: " + e.Message);
            }
        }

        public static int AddRow(
            Worksheet sheet,
            string saveAsSheetName,
            ref object[] oa,
            Hashtable cellLayOutSettings,
            bool autofit,
            Color color,
            int insertInRow,
            params object[] args)
        {
            return AddRow(sheet, saveAsSheetName, ref oa, cellLayOutSettings, autofit, color, insertInRow, 0, args);
        }

        public static int AddRow(
            Worksheet sheet,
            string saveAsSheetName,
            ref object[] oa,
            Hashtable cellLayOutSettings,
            bool autofit,
            Color color,
            int insertInRow,
            int insertInColumn,
            params object[] args)
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

                // spara cellerna som det skrivs till i en str�ng-array, skr sedan alla p� en g�ng
                object[,] cellsToWrite = null;
                if (args.Length == 1) // on�dig?, hmm nej, inte om det �r special f�r 1 grejj, ska man inte kunna skriva enradsgrejjer till andra kolumnerm, hm det har med DbInfos new och +/- kolumn att g�ra troligt
                {
                    cellsToWrite = new object[1, args.Length];
                }
                else
                {
                    cellsToWrite = new object[1, args.Length + insertInColumn];
                }

                #region Write each cell at a time to temp variable

                var rowWrittenTo = 0;
                for (var i = insertInColumn; i < args.Length + insertInColumn; i++)
                {
                    // string toWriteIncell = args[i - insertInColumn].ToString();

                    // Om det inte finns n�got att skriva, g� till n�sta
                    if (args[i - insertInColumn] == null)
                    {
                        continue;
                    }

                    // Str�ngar l�ngre �n ca912 kan inte skrivas till en cell, uten ger ett exception med lite info i. S� l�ngder �ver 900 tecken klipps bort.
                    var toWriteIncell = args[i - insertInColumn];

                    // Det blir problem med celler som b�rjar med "=", och sedan inte ger en riktig formel, s� detta s�tts till
                    // TODO: Fixa n�got allm�nt test f�r formler som kan g� fel, eller formatera rangen som text, men det vill man iofs inte alltid...
                    rowWrittenTo = nextRow; // Vilken rad som verkligen skrivits till, anv�nds f�r layout av cellen
                    if (insertInRow > 0)
                    {
                        cellsToWrite[0, i - insertInColumn] = toWriteIncell;
                        rowWrittenTo = insertInRow;
                    }
                    else
                    {
                        cellsToWrite[0, i] = toWriteIncell; // (toWriteIncell as string);//.Length > 900 ?
                    }
                }

                #endregion

                // Write cells several at a tiem, Fill A2:B6 with an array of values (First and Last Names).
                var fromColumn = ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);

                // nextRow.ToString();
                var toColumn = ExcelLogRowComparer.GetStandardExcelColumnName(args.Length + insertInColumn);

                // nextRow.ToString();
                var cellRange = sheet.get_Range(
                    fromColumn + rowWrittenTo.ToString(), toColumn + rowWrittenTo.ToString());

                // Write to excel sheet
                cellRange.Value2 = cellsToWrite; // "A"

                #region Layout (f�rg, autofit column etc)

                if ((cellLayOutSettings != null && cellLayOutSettings.Count > 0) || autofit
                    || (color != Color.Empty))
                {
                    if (cellLayOutSettings != null && cellLayOutSettings.Count > 0)
                    {
                        EditCellLayOut(cellLayOutSettings, cellRange);
                    }

                    if (autofit)
                    {
                        cellRange.EntireColumn.AutoFit(); // autofittar hela columnen f�r all som loggas
                    }

                    if (color != Color.Empty)
                    {
                        cellRange.Interior.Color = ColorTranslator.ToOle(color);
                    }
                }

                #endregion

                Marshal.ReleaseComObject(cellRange);
                cellRange = null;

                if (insertInRow > 0)
                {
                    nextRow--;
                }

                oa[1] = nextRow + 1; // efter detta ska det kollas om maxrader �r uppn�tt

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

                        // Kolla s� inte samma skrivs ut hela tiden
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
                                ColorTranslator.ToOle((Color)currentSetting.Value);
                            break;
                        case CellLayOutSettings.InteriorColorSysDrawingType:
                            cellRange.Interior.Color =
                                ColorTranslator.ToOle((Color)currentSetting.Value);
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
}
