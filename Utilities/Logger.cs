using ClosedXML.Excel;
using System.Collections;

namespace Utilities
{
    /// <summary>
    /// Handles writing data to Excel workbooks
    /// </summary>
    public static class Logger
    {
        public delegate int OperationToPerformOnBook(IXLWorksheet sheet, object[] logRows);

        /// <summary>
        /// Skriver data till en Excel-fil
        /// </summary>
        /// <param name="excelBookPath">Sökväg till Excel-filen</param>
        /// <param name="sheetName">Namn på arket att skriva till</param>
        /// <param name="rowsToWrite">Hashtable med rader att skriva</param>
        /// <param name="rowToWrite">Enkel rad att skriva (om rowsToWrite är null)</param>
        /// <param name="overWrite">Om filen ska skrivas över</param>
        /// <param name="operation">Custom operation att utföra</param>
        /// <returns>Antal rader som skrevs</returns>
        public static int WriteToWorkBook(
            string excelBookPath,
            string sheetName,
            Hashtable? rowsToWrite,
            object[]? rowToWrite = null,
            bool overWrite = true,
            OperationToPerformOnBook? operation = null)
        {
            try
            {
                XLWorkbook workbook;
                
                // Öppna befintlig fil eller skapa ny
                if (File.Exists(excelBookPath) && !overWrite)
                {
                    workbook = new XLWorkbook(excelBookPath);
                }
                else
                {
                    workbook = new XLWorkbook();
                }

                using (workbook)
                {
                    // Hämta eller skapa worksheet
                    IXLWorksheet worksheet;
                    if (workbook.Worksheets.TryGetWorksheet(sheetName, out var existingSheet))
                    {
                        worksheet = existingSheet;
                    }
                    else
                    {
                        worksheet = workbook.Worksheets.Add(sheetName);
                    }

                    // Rensa om vi ska skriva över
                    if (overWrite)
                    {
                        worksheet.Clear();
                    }

                    // Om vi har en custom operation
                    if (operation != null && rowToWrite != null)
                    {
                        return operation(worksheet, rowToWrite);
                    }

                    var currentRow = overWrite ? 1 : worksheet.LastRowUsed()?.RowNumber() + 1 ?? 1;

                    // Skriv en rad
                    if (rowToWrite != null)
                    {
                        WriteRow(worksheet, currentRow, rowToWrite);
                        currentRow++;
                    }
                    // Skriv flera rader
                    else if (rowsToWrite != null)
                    {
                        // Sortera efter nyckel för att få rätt ordning
                        var sortedKeys = new ArrayList(rowsToWrite.Keys);
                        sortedKeys.Sort();

                        foreach (var key in sortedKeys)
                        {
                            var row = rowsToWrite[key];
                            if (row is object[] objArray)
                            {
                                WriteRow(worksheet, currentRow, objArray);
                                currentRow++;
                            }
                        }
                    }

                    // Spara filen
                    workbook.SaveAs(excelBookPath);
                    
                    return currentRow - 1;
                }
            }
            catch (Exception e)
            {
                throw new Exception(
                    "Error writing to Excel file. Was the file opened during processing?\r\n\r\n" +
                    "(Sys err: " + e.Message + ").", e);
            }
        }

        private static void WriteRow(IXLWorksheet worksheet, int rowNumber, object[] data)
        {
            for (var i = 0; i < data.Length; i++)
            {
                var cell = worksheet.Cell(rowNumber, i + 1);
                
                if (data[i] == null)
                {
                    cell.Value = string.Empty;
                }
                else if (data[i] is string strValue)
                {
                    // Hantera strängar som börjar med '=' (formler)
                    if (strValue.StartsWith("="))
                    {
                        cell.Value = "'" + strValue; // Force text
                    }
                    else
                    {
                        cell.Value = strValue;
                    }
                }
                else if (data[i] is double || data[i] is float || data[i] is decimal)
                {
                    cell.Value = Convert.ToDouble(data[i]);
                }
                else if (data[i] is int || data[i] is long)
                {
                    cell.Value = Convert.ToInt64(data[i]);
                }
                else if (data[i] is DateTime dateTime)
                {
                    cell.Value = dateTime;
                }
                else
                {
                    cell.Value = data[i].ToString();
                }
            }
        }

        /// <summary>
        /// Helper method for getting Excel column name from number
        /// </summary>
        public static string GetStandardExcelColumnName(int columnNumberOneBased)
        {
            var baseValue = Convert.ToInt32('A') - 1;
            var ret = string.Empty;

            if (columnNumberOneBased > 26)
            {
                ret = GetStandardExcelColumnName(columnNumberOneBased / 26);
            }

            var remainder = columnNumberOneBased % 26;
            if (remainder == 0)
            {
                remainder = 26;
            }

            return ret + Convert.ToChar(baseValue + remainder);
        }
    }
}

