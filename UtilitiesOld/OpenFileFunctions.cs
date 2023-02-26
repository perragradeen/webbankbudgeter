using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Office.Interop.Excel;

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
                {FileType.Xls, "Excel XLS Log File"},
                {FileType.Xml, "XML Setting File"}
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
        public static Hashtable GetHashTableFromExcelSheet(
                string excelBookPath,
                string sheetName = "",
                bool onlyLoadSelectedSheetName = true)
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var excelApp = new Application();

            var book = new Hashtable();
            try
            {
                var excelBook = OpenFileFunctionsHelpers.OpenExcelBook(excelBookPath, excelApp);

                // get the collection of sheets in the workbook
                var sheets = excelBook.Worksheets;
                var numOfSheets = excelBook.Worksheets.Count;

                if (onlyLoadSelectedSheetName && !string.IsNullOrWhiteSpace(sheetName))
                {
                    LoadOneSheet(book, sheets, sheetName);
                }
                else
                {
                    LoadAllSheets(book, sheets, numOfSheets);
                }

                // Stäng worbook utan att spara (man har ju bara läst nu)
                excelBook.Close(false, Type.Missing, Type.Missing);
            }
            catch (Exception e)
            {
                excelApp.Quit(); // Stäng excel
                Marshal.ReleaseComObject(excelApp);

                throw new Exception(
                    "Error in retrieving log. Was the log opened in Excel during compare processing?\r\n\r\n" +
                        "(Sys err: " + e.Message + ")."
                    , e);
            }

            // Stäng boken oven
            CloseApp(excelApp);

            return book;
        }

        private static void LoadOneSheet(Hashtable book, Sheets sheets, string sheetName)
        {
            var worksheet = (Worksheet)sheets[sheetName];

            // Här byts ju worksheet ändå, så att sätta worksheet ovan blir verkningslöst
            var rows = new Hashtable(); // Behöver ej göras new, kan sättas till null eg.
            ExcelLogRowComparer.GetExcelRows(worksheet, rows);

            // Hämta ut rader och lägg i rows från Excel arket worksheet
            book.Add(sheetName, rows); // Lägg till i arbetsboken
        }

        private static void LoadAllSheets(Hashtable book, Sheets sheets, int numOfSheets)
        {
            const int startSheetNumber = 1;
            for (var sheetNr = startSheetNumber; sheetNr <= numOfSheets; sheetNr++)
            {
                // Excelarknamnet
                var localSheetName = ((Worksheet)sheets.Item[sheetNr]).Name;

                LoadOneSheet(book, sheets, localSheetName);
            }
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