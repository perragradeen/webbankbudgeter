using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;
using RefLesses;

namespace Utilities
{
    /// <summary>
    /// Summary description for Logger.
    /// </summary>
    public static class Logger
    {
        public delegate int OperationToPerformOnBook(Worksheet sheet, object[] logRows);

        // h�ller reda p� hur m�nga rader som kan finnas i ett Excelark, (<=Ex2003 har max 65536 (2^16)rader)
        private static readonly Hashtable UniqueLoggerErrorMessages = new Hashtable();

        private static void SaveWorkBook(_Workbook book, string logPath)
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

        /// <summary>
        /// Gets a workbook for saving purposes
        /// </summary>
        /// <param name="excelBookPath">path to Excel file</param>
        /// <param name="sheetName"></param>
        /// <param name="rowsToWrite"></param>
        /// <param name="rowToWrite"></param>
        /// <param name="overWrite"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        public static int WriteToWorkBook(
            string excelBookPath,
            string sheetName,
            Hashtable rowsToWrite,
            object[] rowToWrite = null,
            bool overWrite = true,
            OperationToPerformOnBook operation = null)
        {
            #region Todo

            // Todo:
            // Skapa klass, med tabell �ver Sheets som nyklar och specialklass f�r det som finns i sheetet, som ska inneh�lla; sheetet, tabbell med arrayer med cellinneh�llet (helst str�ngarrayer med unika nyklar) (man kan �ven ha formatering lagrat f�r varje rad eller cell, men den informationen ska ligga separat, och s�ttas sist, n�r alla rader skrivits) , antalet dubbleter vid radoverflow, (redan skrivna rader => kan man f� fr�n sheetet sj�lv)
            // Hantera radoverflow
            // Ta bort det som returneras eller returnera sista raden skriven till
            // Optimera genom att skriva flera rader p� en g�ng

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
                var sheets = excelBook.Worksheets;

                var startSheetNumber = 1;

                // get the first worksheet from the collection of worksheets
                var workSheet = (Worksheet) sheets.Item[startSheetNumber];
                if (sheetName != "")
                {
                    #region H�mta ut r�tt sheet

                    workSheet = null;

                    // H�mta ut ett ark med inskickat namn
                    foreach (Worksheet currentSheet in sheets)
                    {
                        if (currentSheet.Name == sheetName)
                        {
                            workSheet = currentSheet;

                            break;
                        }

                        startSheetNumber++;
                    }

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

                #region Skriv en eller flera rader

                var oa = new object[] {workSheet, orgRowCount + 1, 0}; // +1 s� den sista raden inte skrivs �ver

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

        private static void AddRow(_Worksheet sheet,
            string saveAsSheetName,
            ref object[] oa,
            ICollection cellLayOutSettings,
            bool autofit,
            Color color,
            int insertInRow,
            int insertInColumn,
            params object[] args)
        {
            try
            {
                switch (args.Length)
                {
                    case 0:
                        return;
                    case 1 when (args[0] as object[]) != null:
                        args = args[0] as object[];
                        break;
                }

                var nextRow = (int) oa[1];

                // spara cellerna som det skrivs till i en str�ng-array, skr sedan alla p� en g�ng
                if (args != null)
                {
                    var cellsToWrite = args.Length == 1
                        ? new object[1, args.Length]
                        : new object[1, args.Length + insertInColumn];

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

                    // Write cells several at a time, Fill A2:B6 with an array of values (First and Last Names).
                    var fromColumn = ExcelLogRowComparer.GetStandardExcelColumnName(insertInColumn + 1);

                    // nextRow.ToString();
                    var toColumn = ExcelLogRowComparer.GetStandardExcelColumnName(args.Length + insertInColumn);

                    // nextRow.ToString();
                    var cellRange = sheet.Range[fromColumn + rowWrittenTo, toColumn + rowWrittenTo];

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
                }

                if (insertInRow > 0)
                {
                    nextRow--;
                }

                oa[1] = nextRow + 1; // efter detta ska det kollas om maxrader �r uppn�tt
            }
            catch (Exception e)
            {
                var allArgs = args.Aggregate(string.Empty, (current, item) => current + (";" + item));

                if (allArgs == string.Empty)
                {
                    allArgs = "<empty>";
                }

                var errMess = "Error in Logger. In sheet; " + saveAsSheetName + ", may be Excel error: " + e.Message
                              + "\r\n" + "Tried to Log" + allArgs;
                Console.WriteLine(errMess);

                try
                {
                    if (!UniqueLoggerErrorMessages.ContainsKey(errMess))
                    {
                        UniqueLoggerErrorMessages.Add(errMess, 1);

                        // Kolla s� inte samma skrivs ut hela tiden
                        var streamWriter =
                            new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"Logs\LoggerExceptions.txt");

                        var toLoggerMess = StringFunctions.MergeStringArrayToString(
                            (IEnumerable<string>) UniqueLoggerErrorMessages.Keys);

                        streamWriter.Write(toLoggerMess);

                        streamWriter.Close();
                    }
                }
                catch (Exception excExcp)
                {
                    Console.WriteLine(
                        "Error in Logger in sheet; " + saveAsSheetName + ", error with error reporting: "
                        + excExcp.Message);
                }
            }
        }

        private static void EditCellLayOut(IEnumerable settings, Range cellRange)
        {
            try
            {
                foreach (DictionaryEntry currentSetting in settings)
                {
                    var settingType = (CellLayOutSettings) currentSetting.Key;

                    switch (settingType)
                    {
                        case CellLayOutSettings.Bold:
                            cellRange.Font.Bold = (bool) currentSetting.Value;
                            break;
                        case CellLayOutSettings.UnderLined:
                            cellRange.Font.Underline = (bool) currentSetting.Value;
                            break;
                        case CellLayOutSettings.FontStyle:
                            cellRange.Font.FontStyle =
                                (currentSetting.Value as Microsoft.Office.Interop.Excel.Font)?.FontStyle;
                            break;
                        case CellLayOutSettings.TextColor:
                            cellRange.Font.Color =
                                ColorTranslator.ToOle((Color) currentSetting.Value);
                            break;
                        case CellLayOutSettings.InteriorColorSysDrawingType:
                            cellRange.Interior.Color =
                                ColorTranslator.ToOle((Color) currentSetting.Value);
                            break;
                        case CellLayOutSettings.InteriorColorColorIndexType:
                            cellRange.Interior.ColorIndex = (int) currentSetting.Value;
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