using Microsoft.Office.Interop.Excel;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using Application = Microsoft.Office.Interop.Excel.Application;

namespace Utilities
{
    public static class OpenFileFunctions
    {
        #region Open file functions

        public static readonly Hashtable UsedFileTypesFilterNames =
            InitInfoToolUsedFileTypesFilterNames();

        private static Hashtable InitInfoToolUsedFileTypesFilterNames()
        {
            var returnNames = new Hashtable
            {
                { FileType.Xls, "Excel XLS Log File" },
                { FileType.Xml, "XML Setting File" }
            };

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
        public static void OpenExcelSheet(
                string excelBookPath,
                string sheetName,
                Hashtable book,
                int selectedRow)
            // ev. returnera en bool om det lyckades, ev. lägg en Arraylist som innehåller allt inkl. dubletter
        {
            var returnHashtable = new Hashtable();
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            #region read Old Log

            var excelApp = new ApplicationClass();

            try
            {
                // Öppna den gamla loggen
                var excelBook = excelApp.Workbooks._Open(
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

                // get the collection of sheets in the workbook
                var sheets = excelBook.Worksheets;
                var numOfSheets = excelBook.Worksheets.Count;

                // Store old rows
                const int startSheetNumber = 1;
                for (var sheetNr = startSheetNumber; sheetNr <= numOfSheets; sheetNr++)
                {
                    var localSheetName = ((Worksheet)sheets.Item[sheetNr]).Name;

                    // Excelarknamnet
                    var worksheet = (Worksheet)sheets.Item[sheetNr];

                    // Här byts ju worksheet ändå, så att sätta worksheet ovan blir verkningslöst
                    var rows = new Hashtable(); // Behöver ej göras new, kan sättas till null eg.
                    ExcelLogRowComparer.GetExcelRows(worksheet, rows);

                    // Hämta ut rader och lägg i rows från Excel arket worksheet
                    book.Add(localSheetName, rows); // Lägg till i arbetsboken
                }

                if (sheetName != "" && selectedRow != 0) // ha detta som en annan fkn, för att kunna använda ovan som en mer generell fkn, och ev. ha en som kör båda sen, för MissingCSC
                {
                    var rows = (book[sheetName] as Hashtable)?.Values
                               ?? throw new ArgumentNullException(nameof(excelBookPath));
                    foreach (ExcelRowEntry var in rows)
                    {
                        returnHashtable.Add(var.Args[selectedRow - 1], 1);
                    }
                }

                // Stäng worbook utan att spara (man har ju bara läst nu)
                excelBook.Close(false, Type.Missing, Type.Missing);
            }
            catch (Exception e)
            {
                excelApp.Quit(); // Stäng excel
                Marshal.ReleaseComObject(excelApp);

                if (returnHashtable.Count > 0)
                {
                    return;
                }

                throw new Exception(
                    "Error in retrieving log. Was the log opened in Excel during compare processing?\r\n\r\n(Sys err: "
                    + e.Message + ").",
                    e);
            }

            // Stäng boken oven
            CloseApp(excelApp);

            #endregion
        }

        // För att stänga Excel efter användandet.
        private static void CloseApp(Application appToCloseEtc)
        {
            // Stäng och släpp excel
            appToCloseEtc.Quit();

            while (Marshal.ReleaseComObject(appToCloseEtc) != 0)
            {
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        #endregion
    }
}