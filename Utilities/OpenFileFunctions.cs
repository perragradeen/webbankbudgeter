using System;
using System.Collections;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Office.Interop.Excel;
using Application = Microsoft.Office.Interop.Excel.Application;

namespace Utilities
{
    public class OpenFileFunctions
    {
        #region Open file functions

        public static Hashtable UsedFileTypesFilterNames = InitInfoToolUsedFileTypesFilterNames();

        private static Application _excelApp;

        private static Hashtable InitInfoToolUsedFileTypesFilterNames()
        {
            var returnNames = new Hashtable();
            returnNames.Add(FileType.xls, "Excel XLS Log File");
            returnNames.Add(FileType.xml, "XML Setting File");

            return returnNames;
        }

        /// <summary>
        /// Lagrar ett Excelark i en Hashtabell, man kan välja ut ett nummer på kolumn som ska sparas, dubbletter lagras ej, alltså en rad eller cell lagras som unik endast en gång
        /// </summary>
        /// <param name="excelBookPath">Sökväg till Excelfilen</param>
        /// <param name="sheetName">Namn på ark, =theonlyonein ger specialfall med en kolumn</param>
        /// <param name="book">Hashtabell som ALLA celler eller rader lagras i, inte bara en kolumn även om man valt att få det, den returneras av funktionen istället</param>
        /// <param name="selectedRow">Rad som ska sparas, 0 för alla</param>
        /// <returns>Om selectedRow är annat än 0 och sheetName inte är tom sträng, så returneras en Hashtabell med den angivna raden </returns>
        public static Hashtable OpenExcelSheet(string excelBookPath, string sheetName, Hashtable book, int selectedRow)
            // ev. returnera en bool om det lyckades, ev. lägg en Arraylist som innehåller allt inkl. dubletter
        {
            var returnHashtable = new Hashtable();
            var oldCI = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            #region read Old Log

            _excelApp = new Microsoft.Office.Interop.Excel.ApplicationClass();
            _excelApp.WorkbookDeactivate += Application_WorkbookDeactivate;

            Workbook ExcelBook = null;

            // Hashtable book = new Hashtable();
            try
            {
                // new ExcelLogRowComparer();//För progress

                // ExcelLogRowComparer._compareProgress.StartTotal("Loading Log...", 0);

                // Öppna den gamla loggen
                ExcelBook = _excelApp.Workbooks._Open(
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

                // get the collection of sheets in the workbook
                var Sheets = ExcelBook.Worksheets;
                var numOfSheets = ExcelBook.Worksheets.Count;
                var startSheetNumber = 1;

                //// get the first and only worksheet from the collection of worksheets
                Worksheet worksheet = null;
                if (sheetName == "")
                {
                    worksheet = (Microsoft.Office.Interop.Excel.Worksheet)Sheets.get_Item(1);
                }
                else if (sheetName != "=theonlyonein")
                {
                    // Hämta ut ett ark med inskickat namn
                    foreach (Worksheet currentSheet in Sheets)
                    {
                        if (currentSheet.Name == sheetName)
                        {
                            worksheet = currentSheet;

                            break;
                        }

                        startSheetNumber++;
                    }

                    numOfSheets = startSheetNumber; // +1 behövs ej eftersom loopen har  <= numOfSheets
                }
                else if (sheetName == "=theonlyonein") // bör det ju vara då...hehe
                {
                    worksheet = (Microsoft.Office.Interop.Excel.Worksheet)Sheets.get_Item(1);
                    sheetName = worksheet.Name;

                    numOfSheets = 1;
                    startSheetNumber = 1;
                }

                /// loop through 10 rows of the spreadsheet and place each row in the list view
                var rows = new Hashtable(); // Behöver ej göras new, kan sättas till null eg.

                // Progress, görs ej nu, för de e fel comparer... ExcelLogRowComparer._compareProgress.StartTotal("Loading Log sheets...", numOfSheets);//Progress
                // int sheetsDone = 0;//För progress

                // Store old rows
                for (var sheetNr = startSheetNumber; sheetNr <= numOfSheets; sheetNr++)
                {
                    var localSheetName = ((Microsoft.Office.Interop.Excel.Worksheet)Sheets.get_Item(sheetNr)).Name;
                        
                        // Excelarknamnet
                    worksheet = (Microsoft.Office.Interop.Excel.Worksheet)Sheets.get_Item(sheetNr);
                        
                        // Här byts ju worksheet ändå, så att sätta worksheet ovan blir verkningslöst
                    rows = new Hashtable();
                    ExcelLogRowComparer.GetExcelRows(worksheet, rows);
                        
                        // Hämta ut rader och lägg i rows från Excel arket worksheet
                    book.Add(localSheetName, rows); // Lägg till i arbetsboken

                    // Progress, görs ej nu, för de e fel comparer... ExcelLogRowComparer._compareProgress.SetTotal(++sheetsDone);//Progress

                    // if (MainForm.StopGracefully)
                    // break;
                }

                // throw new Exception("TESTEXEPTION");
                if (sheetName != "" && selectedRow != 0) // ha detta som en annan fkn, för att kunna använda ovan som en mer generell fkn, och ev. ha en som kör båda sen, för MissingCSC
                {
                    // book = book[sheetName]
                    foreach (ExcelRowEntry var in (book[sheetName] as Hashtable).Values) // string[]
                    {
                        returnHashtable.Add(var.Args[selectedRow - 1], 1);
                    }
                }

                // Stäng worbook utan att spara (man har ju bara läst nu)
                ExcelBook.Close(false, Type.Missing, Type.Missing);
            }
            catch (Exception e)
            {
                _excelApp.Quit(); // Stäng excel
                System.Runtime.InteropServices.Marshal.ReleaseComObject(_excelApp);

                if (returnHashtable != null && returnHashtable.Count > 0)
                {
                    return returnHashtable;
                }

                // MessageBox.Show("Error in retrieving old log. Was the log opened in Excel during compare processing?\r\n\r\n(Sys err: " + e.Message + ").");
                throw new Exception(
                    "Error in retrieving log. Was the log opened in Excel during compare processing?\r\n\r\n(Sys err: "
                    + e.Message + ").", 
                    e);
            }

            // Stängt boken oven
            // _excelApp.Quit();//Stäng excel
            System.Runtime.InteropServices.Marshal.ReleaseComObject(_excelApp);

            return returnHashtable;

            #endregion
        }

        // För att stänga Excel efter användandet.
        private static void Application_WorkbookDeactivate(Workbook wb)
        {
            // Stäng och släpp excel
            var appToCloseEtc = _excelApp;
            appToCloseEtc.Quit();

            while (System.Runtime.InteropServices.Marshal.ReleaseComObject(appToCloseEtc) != 0)
            {
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            // ReSharper disable RedundantAssignment
            // Wants to be sure excelAppOpen is cleared
            appToCloseEtc = null;

            // ReSharper restore RedundantAssignment
        }

        #endregion

        #region LoadXmlSettings

        public Hashtable ReadSettings()
        {
            return ReadSettings("settings.xml", "//pretentioussettings"); // AppDomain.CurrentDomain.BaseDirectory + 
        }

        /// <summary>
        /// Returns a HasHtable with the nodes form nodeName, and some attributes from a xml file
        /// </summary>
        /// <param name="settingsFile">Xml file</param>
        /// <param name="nodeName">Node to read from</param>
        /// <returns></returns>
        public Hashtable ReadSettings(string settingsFile, string nodeName)
        {
            var returnTable = new Hashtable();
            try
            {
                var __doc = new XmlDocument();
                __doc.Load(settingsFile); // "settings.xml");//AppDomain.CurrentDomain.BaseDirectory + 

                var items = __doc.SelectSingleNode(nodeName); // "//pretentioussettings");
                foreach (XmlNode item in items.ChildNodes) // __doc.FirstChild.ChildNodes)
                {
                    var settingCurrentElem = item as XmlElement;
                    var settingCurrent = settingCurrentElem.Name;

                    returnTable.Add(
                        settingCurrent, ""
                        
                        // new DaySettings(
                        // int.Parse(settingCurrentElem.GetAttribute("dagintervall"))
                        // , settingCurrent + " Time!")
                        );
                }

                return returnTable;
            }
            catch (Exception e)
            {
                throw new Exception("Fel vid inläsning av settings-fil: " + e.Message, e);
            }
        }

        #endregion
    }

    public class WinFormsChecks
    {
        public delegate void SaveFunction();

        /// <summary>
        /// Saves if user wants to
        /// </summary>
        /// <param name="somethingChanged">bool indicating if something has changed</param>
        /// <param name="saveFunc">The function that will perform the actual saving.</param>
        /// <returns>True if something was saved</returns>
        public static DialogResult SaveCheck(bool somethingChanged, SaveFunction saveFunc)
        {
            var saveOr = DialogResult.None;
            if (somethingChanged)
            {
                saveOr = MessageBox.Show("Läget ej sparat! Spara nu?", "Spara?", MessageBoxButtons.YesNoCancel);
                    
                    // Cancel
                if (saveOr == DialogResult.Yes)
                {
                    saveFunc();
                }
            }

            return saveOr;
        }
    }

    public enum FileType
    {
        xls, 
        xml, 
    }
}