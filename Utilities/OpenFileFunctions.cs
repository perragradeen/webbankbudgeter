using ExcelDataReader;
using System.Collections;
using System.Data;
using System.Globalization;
using System.Text;

namespace Utilities
{
    public static class OpenFileFunctions
    {
        static OpenFileFunctions()
        {
            // Registrera encoding providers för att stödja äldre .xls-filer
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public static readonly Hashtable UsedFileTypesFilterNames =
            InitInfoToolUsedFileTypesFilterNames();

        private static Hashtable InitInfoToolUsedFileTypesFilterNames()
        {
            var returnNames = new Hashtable
            {
                {FileType.Xls, "Excel XLS Log File"},
                {FileType.Xlsx, "Excel XLSX Log File"},
                {FileType.Xml, "XML Setting File"}
            };

            return returnNames;
        }

        /// <summary>
        /// Läser ett Excel-ark till en Hashtable
        /// Stödjer både .xls (Excel 97-2003) och .xlsx (Excel 2007+) format
        /// </summary>
        /// <param name="excelBookPath">Sökväg till Excel-filen</param>
        /// <param name="sheetName">Namn på ark att läsa</param>
        /// <param name="onlyLoadSelectedSheetName">Om endast valt ark ska laddas</param>
        /// <returns>Hashtable med data från Excel-filen</returns>
        public static Hashtable GetHashTableFromExcelSheet(
            string excelBookPath,
            string sheetName = "",
            bool onlyLoadSelectedSheetName = true)
        {
            var book = new Hashtable();

            try
            {
                using var stream = File.Open(excelBookPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = ExcelReaderFactory.CreateReader(stream);
                
                var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = false // Läs alla rader inklusive header
                    }
                });

                if (onlyLoadSelectedSheetName && !string.IsNullOrWhiteSpace(sheetName))
                {
                    LoadOneSheet(book, dataSet, sheetName);
                }
                else
                {
                    LoadAllSheets(book, dataSet);
                }
            }
            catch (Exception e)
            {
                throw new Exception(
                    "Error in retrieving Excel data. Was the file opened during processing?\r\n\r\n" +
                    "(Sys err: " + e.Message + ").", e);
            }

            return book;
        }

        private static void LoadOneSheet(Hashtable book, DataSet dataSet, string sheetName)
        {
            if (!dataSet.Tables.Contains(sheetName))
            {
                throw new Exception($"Sheet '{sheetName}' not found in workbook.");
            }

            var rows = new Hashtable();
            GetExcelRows(dataSet.Tables[sheetName]!, rows);
            book.Add(sheetName, rows);
        }

        private static void LoadAllSheets(Hashtable book, DataSet dataSet)
        {
            foreach (DataTable table in dataSet.Tables)
            {
                var rows = new Hashtable();
                GetExcelRows(table, rows);
                book.Add(table.TableName, rows);
            }
        }

        private static void GetExcelRows(DataTable table, Hashtable storeIn)
        {
            if (storeIn == null)
            {
                return;
            }

            try
            {
                foreach (DataRow row in table.Rows)
                {
                    var columnCount = table.Columns.Count;
                    var strArrayToSave = new object[columnCount];
                    var strArray = string.Empty;

                    for (var colIndex = 0; colIndex < columnCount; colIndex++)
                    {
                        var cellValue = row[colIndex];
                        
                        string stringValue;
                        if (cellValue == null || cellValue == DBNull.Value)
                        {
                            stringValue = string.Empty;
                        }
                        else if (cellValue is double || cellValue is float || cellValue is decimal)
                        {
                            stringValue = Convert.ToDouble(cellValue).ToString(CultureInfo.InvariantCulture);
                        }
                        else if (cellValue is DateTime dateTime)
                        {
                            stringValue = dateTime.ToString(CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            stringValue = cellValue.ToString() ?? string.Empty;
                        }

                        strArray += stringValue;
                        strArrayToSave[colIndex] = stringValue;
                    }

                    // Lägg till endast om det inte redan finns (undvik dubbletter)
                    if (!storeIn.ContainsKey(strArray))
                    {
                        storeIn.Add(strArray, new ExcelRowEntry(strArrayToSave));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading Excel rows: " + e.Message);
                throw;
            }
        }
    }
}

