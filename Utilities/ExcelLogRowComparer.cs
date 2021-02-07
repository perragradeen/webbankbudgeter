using Microsoft.Office.Interop.Excel;
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo

namespace Utilities
{
    internal class ExcelLogRowComparer
    {
        // There is one other line you will have to change which is switching XlWBATemplate.xlWBATWorksheet to Excel.XlWBATemplate.xlWBATWorksheet.
        private static Application excelApp;

        private static Workbook newLog;
        private static Workbook oldLog;

        // public static Progress _compareProgress = new Progress();

        // public static void CompareLogs(string oldLogFileName)
        // {
        // #region CheckLast
        // if (MainForm.LastLogPath == "")//MainForm.LastLog == null && 
        // {
        // //done, öppna fildialog och välj ny fil
        // //return;

        // OpenFileDialog dlg = OpenLog("Open NEW log");
        // if (dlg.ShowDialog() != DialogResult.OK)
        // return;

        // //dlg.FileName = "C:\\Infotool-Projekt\\Senaste\\MSVS 2008\\InfoTool\\trunk\\src\\InfoTool\\bin\\Debug\\Logs\\new 2008-10-27 14-41-55.xls";

        // MainForm.LastLogPath = dlg.FileName;
        // }
        // else
        // {
        // //if (MainForm.LastLogPath == "")
        // //{
        // //    _newLog = MainForm.LastLog;
        // //}
        // }

        // #endregion

        // CompareLogs(oldLogFileName, MainForm.LastLogPath);
        // }
        // private static OpenFileDialog OpenLog(string title)
        // {
        // OpenFileDialog dlg = new OpenFileDialog();
        // dlg.Title = title;
        // dlg.Multiselect = false;
        // dlg.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "Logs";
        // //dlg.Filter = "XML Log File|*.xml";
        // dlg.Filter = "Excel XLS Log File|*.xls";
        // return dlg;
        // }
        public static void CompareLogs(string oldLogFileName, string newLogFileName) // filename
        {
            // bool orgProgresSetting = false;//Håller på vad progress vad satt till innan
            // if (MainForm.StopGracefully)
            // return;
            excelApp = new ApplicationClass();

            // Skapa instansen här istället för globalt i denna klass, för att det inte ska skapas en Excelprocess om man bara kör en funktion i denna klassen
            #region Progress

            // Progress
            // orgProgresSetting = MainForm.ShowProgress;//Håller på vad progress vad satt till innan
            // if (MainForm.ShowProgress)
            // MainForm.ShowProgress = false;

            // MainForm.ShowTextProgress = true;//Visar texten bara 
            #endregion

            var oldCi = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            #region read Old Log

            var oldBook = new Hashtable();

            try
            {
                // _compareProgress.StartTotal("Loading old Log...", 0);//-1 );

                // Öppna den gamla loggen
                oldLog = excelApp.Workbooks._Open(
                    oldLogFileName,
                    Type.Missing, // filename,
                    0,
                    Type.Missing,
                    XlPlatform.xlWindows, // XlTextQualifier.xlTextQualifierNone,
                    Type.Missing,
                    Type.Missing,
                    Type.Missing,
                    false, // COMMA
                    Type.Missing,
                    Type.Missing,
                    Type.Missing,
                    Type.Missing);

                // get the collection of sheets in the workbook
                var oldSheets = oldLog.Worksheets;
                var numOfOldSheets = oldLog.Worksheets.Count;

                //// get the first and only worksheet from the collection of worksheets

                //// loop through 10 rows of the spreadsheet and place each row in the list view

                // _compareProgress.StartTotal("Loading old Log sheets...", numOfOldSheets);//-1 );
                // int sheetsDone = 0;//För progress

                // Store old rows
                for (var sheetNr = 1; sheetNr <= numOfOldSheets; sheetNr++)
                {
                    var name = ((Worksheet)oldSheets.Item[sheetNr]).Name;

                    var oldWorksheet = (Worksheet)oldSheets.Item[sheetNr];

                    var oldRows = new Hashtable();
                    GetExcelRows(oldWorksheet, oldRows);

                    oldBook.Add(name, oldRows);

                    // _compareProgress.SetTotal(++sheetsDone);

                    // if (MainForm.StopGracefully)
                    // break;
                }
            }
            catch (Exception e)
            {
                // MessageBox.Show("Error in retrieving old log. Was the log opened in Excel during compare processing?\r\n\r\n(Sys err: " + e.Message + ").");
                throw new Exception(
                    "Error in retrieving old log. Was the log opened in Excel during compare processing?\r\n\r\n(Sys err: "
                    + e.Message + ").",
                    e);
            }

            #endregion

            #region read New Log

            try
            {
                // if (MainForm.StopGracefully)
                // return;

                // _compareProgress.StartTotal("Loading new Log...", 0);//-1 );

                // Excel.Workbook tempWB = newLog;
                // Ev. ha If(_newLog ==null)...
                newLog = excelApp.Workbooks._Open(
                    newLogFileName, // filename, filename,
                    Type.Missing,
                    0,
                    Type.Missing,
                    XlPlatform.xlWindows, // XlTextQualifier.xlTextQualifierNone,
                    Type.Missing,
                    Type.Missing,
                    Type.Missing,
                    false, // COMMA
                    Type.Missing,
                    Type.Missing,
                    Type.Missing,
                    Type.Missing);

                var newSheets = newLog.Worksheets;
                var numOfNewSheets = newLog.Worksheets.Count;

                //// get the first worksheet from the collection of worksheets

                //// loop through 10 rows of the spreadsheet and place each row in the list view
                // Hashtable newBook = new Hashtable();
                // Hashtable oldRows = new Hashtable();

                // _compareProgress.StartTotal("Loading new Log sheets and compares...", numOfNewSheets);//-1 );
                // int sheetsDone = 0;//För progress
                var specialCaseForAllProfilesHandled = false;

                // Compare to old rows
                for (var sheetNr = 1; sheetNr <= numOfNewSheets; sheetNr++)
                {
                    // if (MainForm.StopGracefully)
                    // break;
                    var name = ((Worksheet)newSheets.Item[sheetNr]).Name;

                    var newWorksheet = (Worksheet)newSheets.Item[sheetNr];

                    // Specialfall för AllProfiles-flikar
                    if (name.StartsWith("AllProfiles") && !specialCaseForAllProfilesHandled)
                    {
                        CheckAllProfiles(oldBook, newSheets);

                        specialCaseForAllProfilesHandled = true;
                    }
                    else if (name.StartsWith("AllProfiles") && specialCaseForAllProfilesHandled)
                    {
                    }
                    else if (oldBook.ContainsKey(name))
                    {
                        // if (MainForm.StopGracefully)
                        // break;

                        // Läs in hela nuv. nya arket till en HT
                        var newRows = new Hashtable();
                        GetExcelRows(newWorksheet, newRows);

                        var rows = 0;
                        var colums = 0; // newWorksheet.UsedRange.Columns.Count;
                        if (CompareExcelRows(newWorksheet, oldBook[name] as Hashtable, newRows, ref rows, ref colums))
                        {
                            #region Sortera

                            // if (MainForm.StopGracefully)
                            // break;

                            // Sortera på new
                            var column = GetStandardExcelColumnName(colums + 1);
                            var range = newWorksheet.Range["A4", column + rows.ToString(CultureInfo.InvariantCulture)];

                            // "IV"
                            if (name != "DatabaseInfo")
                            {
                                range.Sort(
                                    range.Columns[colums + 1, Type.Missing],
                                    XlSortOrder.xlDescending,
                                    // För att felsöka Excelprogrammering, använd macroEdit for VB i excel...
                                    // range.Columns[2,Type.Missing], Type.Missing, Excel.XlSortOrder.xlDescending
                                    Type.Missing,
                                    Type.Missing,
                                    XlSortOrder.xlDescending,
                                    Type.Missing,
                                    XlSortOrder.xlDescending,
                                    XlYesNoGuess.xlNo,
                                    Type.Missing,
                                    Type.Missing,
                                    XlSortOrientation.xlSortColumns,
                                    // ReSharper disable RedundantArgumentDefaultValue
                                    XlSortMethod.xlPinYin,
                                    XlSortDataOption.xlSortNormal,
                                    XlSortDataOption.xlSortNormal,
                                    XlSortDataOption.xlSortNormal);

                                // ReSharper restore RedundantArgumentDefaultValue
                            }

                            // Ta bort "new"-kolumnen
                            range = newWorksheet.Range[column + 1, column + rows.ToString(CultureInfo.InvariantCulture)];

                            // "IV"
                            range.Delete(Type.Missing); // false//(object)false);//

                            // För old o diff överskriften...
                            if (name == "DatabaseInfo")
                            {
                                range = newWorksheet.Range["C:D", "C:D"];
                                range.EntireColumn.AutoFit(); // autofittar hela columnen för all som loggas
                            }

                            // newWorksheet.set 
                            #endregion
                        }

                        // if (MainForm.StopGracefully)
                        // break;
                    }
                    else
                    {
                        Console.WriteLine(name + " didn't exist in old Excel book.");
                    }

                    // oldBook.Add(name, oldRows);

                    // _compareProgress.SetTotal(++sheetsDone);
                }

                // Spara en ny fil
                // newLog.FullName = newLog.FullName + "-Compared";
                // newLog.Save();// .Save();
                newLogFileName = newLog.FullName.Replace(".xls", string.Empty) + "-Compared" + ".xls";
                newLog.SaveCopyAs(newLogFileName); // MainForm.LastLogPath);//newLog.FullName + "-Compared");
                newLog.Close(false, Type.Missing, Type.Missing);

                // _book.Close(false, Type.Missing, Type.Missing);
                excelApp.Quit();
                Marshal.ReleaseComObject(excelApp);
                excelApp = null;
            }
            catch (Exception e)
            {
                // MessageBox.Show("Error in comparing old log with new log. Was the log opened in Excel during compare processing?\r\n\r\n(Sys err: " + e.Message + ").");
                // throw e;
                throw new Exception(
                    "Error in comparing old log with new log. Was the log opened in Excel during compare processing?\r\n\r\n(Sys err: "
                    + e.Message + ").",
                    e);
            }

            #endregion - read new

            System.Threading.Thread.CurrentThread.CurrentCulture = oldCi;

            // MainForm.ShowTextProgress = false;//Visar inte ens texten längre
            // MainForm.ShowProgress = orgProgresSetting;//Håller på vad progress vad satt till innan

            // Stäng Excel
            if (excelApp != null)
            {
                excelApp.Quit();
                Marshal.ReleaseComObject(excelApp);
                excelApp = null;
            }
        }

        public static string GetStandardExcelColumnName(int columnNumberOneBased)
        {
            var baseValue = Convert.ToInt32('A') - 1;
            var ret = string.Empty;

            if (columnNumberOneBased > 26)
            {
                ret = GetStandardExcelColumnName(columnNumberOneBased / 26);
            }

            return ret + Convert.ToChar(baseValue + (columnNumberOneBased % 26));
        }

        public static void GetExcelRows(Worksheet worksheet, Hashtable storeIn)
        {
            if (storeIn == null)
            {
                // MessageBox.Show("Hashtable empty.");
                return;
            }

            try
            {
                // Progress
                // _compareProgress.StartCurrent("Loading sheet: " + worksheet.Name + "...", worksheet.UsedRange.Rows.Count);
                // int currentProgress = 0;

                // worksheet.UsedRange.Count ger rader, worksheet.UsedRange.Columns.Count ger kolumner
                // 65536
                for (var i = 1; i <= worksheet.UsedRange.Rows.Count; i++)
                {
                    // if (MainForm.StopGracefully)
                    // return;

                    // break;//Debug
                    var numOfRowsToReadAtATime = 5000; // 10 blir 11
                    if (numOfRowsToReadAtATime > worksheet.UsedRange.Rows.Count)
                    {
                        numOfRowsToReadAtATime = worksheet.UsedRange.Rows.Count - 1;
                    }

                    // Todo: ta bara in resterande, räkna inte med de som redan lästs hittils
                    var column = GetStandardExcelColumnName(worksheet.UsedRange.Columns.Count + 1);
                    var range =
                        worksheet
                            .Range["A" + i.ToString(CultureInfo.InvariantCulture),
                            column + (i + numOfRowsToReadAtATime).ToString(CultureInfo.InvariantCulture)]; // "IV" 
                    var myvalues = (Array)range.Cells.Value[Type.Missing]; // Value;

                    // string strData = range.get_Value(Type.Missing).ToString();
                    // string[] strArray2 = ConvertToStringArray(myvalues);
                    string[] strArrayIn = null;
                    string[,] strArrayIn2D = null;

                    if (numOfRowsToReadAtATime > 1)
                    {
                        strArrayIn2D = ConvertToStringArray2Dimensional(myvalues);
                    }
                    else
                    {
                        strArrayIn = ConvertToStringArray(myvalues);
                    }

                    for (var ii = 0; ii < numOfRowsToReadAtATime + 1; ii++)
                    {
                        // strArrayIn2d = ConvertToStringArray2Dimensional(myvalues);

                        // string sdfa = "";
                        // for (int ji = 0; i < worksheet.UsedRange.Columns.Count; ji++)
                        // {
                        // strArrayIn = strArrayIn2d.GetValue(0, strArrayIn2d.GetUpperBound(1));
                        if (numOfRowsToReadAtATime > 1)
                        {
                            // Hämta ut en inläst rad
                            if (strArrayIn2D != null)
                            {
                                strArrayIn = new string[1 + strArrayIn2D.GetUpperBound(1)];
                                for (var ijj = 0; ijj < strArrayIn2D.GetUpperBound(1) + 1; ijj++)
                                {
                                    strArrayIn[ijj] = strArrayIn2D[ii, ijj];
                                }
                            }
                        }

                        var strArray = string.Empty;
                        var currentColumn = 0;

                        // Onödig ta strArrayIn direkt
                        if (strArrayIn == null)
                        {
                            continue;
                        }

                        var strArrayToSave = new object[strArrayIn.Length]; // - 1];

                        // TODO: skippa konverteringen till string och lagra object direkt istället
                        foreach (var arg in strArrayIn)
                        {
                            strArray += arg; // +" ";//ta object.ToString() här

                            // Onödig ta strArrayIn direkt
                            if (currentColumn < strArrayToSave.Length)
                            {
                                strArrayToSave[currentColumn++] = arg;
                            }
                        }

                        // if (worksheet.Name == "DatabaseInfo")
                        // {
                        // Logga inte om det finns dubletter
                        if (!storeIn.ContainsKey(strArray))
                        {
                            storeIn.Add(strArray, new ExcelRowEntry(i + ii, strArrayToSave));
                        }

                        // }
                        // else
                        // {//även
                        // if (!storeIn.ContainsKey(strArray))
                        // {
                        // storeIn.Add(strArray, strArrayToSave);
                        // }
                        // }

                        // _compareProgress.SetCurrent(++currentProgress);

                        // }
                    }

                    // if (MainForm.StopGracefully)
                    // return;
                    i += numOfRowsToReadAtATime > 1 ? numOfRowsToReadAtATime : 0;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }

        private static string[] ConvertToStringArray(Array values)
        {
            // create a new string array
            var theArray = new string[values.Length];

            // loop through the 2-D System.Array and populate the 1-D String Array
            for (var i = 1; i <= values.Length; i++)
            {
                if (values.GetValue(1, i) == null)
                {
                    theArray[i - 1] = string.Empty;
                }
                else
                {
                    theArray[i - 1] = values.GetValue(1, i).ToString();
                }

                // if (MainForm.StopGracefully)
                // return null;
            }

            return theArray;

            // string Str1= ((ExcelXptlb.Range ) ( (ExcelXptlb.Worksheet)
            // ExlApp.Workbooks[WorkBookName.ToString()].Worksheets[WorkSheetName.ToString()]
            // ).Cells[Row, Col]).Text.ToString()    
        }

        private static string[,] ConvertToStringArray2Dimensional(Array values)
        {
            // create a new string array
            var theArray = new string[values.GetUpperBound(0), values.GetUpperBound(1) - 1];

            // string[,] test = new string[11, 2];

            // loop through the 2-D System.Array and populate the 1-D String Array
            for (var i = 1; i <= values.GetUpperBound(0); i++)
            {
                for (var j = 1; j < values.GetUpperBound(1); j++)
                {
                    if (values.GetValue(i, j) == null)
                    {
                        theArray[i - 1, j - 1] = string.Empty;
                    }
                    else
                    {
                        theArray[i - 1, j - 1] = values.GetValue(i, j).ToString();
                    }

                    // if (MainForm.StopGracefully)
                    // return null;
                }
            }

            return theArray;
        }

        private static void CheckAllProfiles(Hashtable oldBook, Sheets newSheets)
        {
            try
            {
                var newRows = new Hashtable();
                Worksheet saveSheet = null;

                #region Läs in alla nya, och hitta sheetet "AllProfiles"

                // Läs in alla nya
                var numOfNewSheets = newLog.Worksheets.Count;

                //// get the first worksheet from the collection of worksheets

                //// loop through 10 rows of the spreadsheet and place each row in the list view

                // _compareProgress.StartTotal("Loading new AllProfiles Log sheets and compares...", numOfNewSheets);//-1 );
                // int sheetsDone = 0;//För progress

                // Compare to old rows
                for (var sheetNr = 1; sheetNr <= numOfNewSheets; sheetNr++)
                {
                    // if (MainForm.StopGracefully)
                    // break;
                    var name = ((Worksheet)newSheets.Item[sheetNr]).Name;

                    var newWorksheet = (Worksheet)newSheets.Item[sheetNr]; // (Excel.Worksheet)newSheets.get_Item(1);

                    // Läs in hela nuv. nya arket till en HT
                    // Specialfall för AllProfiles-flikar
                    if (name.StartsWith("AllProfiles"))
                    {
                        GetExcelRows(newWorksheet, newRows);
                    }

                    if (name == "AllProfiles")
                    {
                        saveSheet = newWorksheet;
                    }

                    // Ev. Rensa _part1...X

                    // _compareProgress.SetTotal(sheetsDone++);
                }

                #endregion

                #region Lägg ihop alla gamla

                var oldRows = oldBook["AllProfiles"] as Hashtable;

                foreach (DictionaryEntry item in oldBook)
                {

                    // if (name == "AllProfiles")//Specialfall för AllProfiles-flikar
                    // {
                    // oldRows = rows;
                    // }
                    // Specialfall för AllProfiles-flikar
                    if (!(item.Key is string name) || !name.StartsWith("AllProfiles_"))
                        continue;
                    if (!(item.Value is Hashtable rows))
                        continue;

                    foreach (DictionaryEntry innerItem in rows)
                    {
                        if (oldRows != null
                            // ReSharper disable AssignNullToNotNullAttribute
                            && oldRows.ContainsKey(innerItem.Key as string))
                            // ReSharper restore AssignNullToNotNullAttribute
                        {
                            // Här finns en dublett! Det ska ladrig inträffa, för det ska vara unika som läggs till, även om det iofs är olika blad
                            Console.WriteLine(
                                "Double fond in CheckAllProfiles old rows: " + name + ". Key: >" + innerItem.Key
                                + "<");
                        }
                        else
                        {
                            if (oldRows != null)
                            {
                                // ReSharper disable AssignNullToNotNullAttribute
                                if (!oldRows.ContainsKey(innerItem.Key as string))
                                {
                                    oldRows.Add(innerItem.Key as string, innerItem.Value as ExcelRowEntry);
                                    // ReSharper restore AssignNullToNotNullAttribute
                                }
                            }
                        }
                    }
                }

                #endregion

                CompareExcelRows(saveSheet, oldRows, newRows);
            }
            catch (Exception allPexcp)
            {
                throw new Exception(
                    "Error in comparing AllProfiles log with new log. Was the log opened in Excel during compare processing?\r\n\r\n(Sys err: "
                    + allPexcp.Message + ").",
                    allPexcp);
            }
        }

        private static bool TableAreNull(Hashtable storedOld, Hashtable storedNew)
        {
            if (storedOld == null || storedNew == null)
            {
                // MessageBox.Show("Hashtable loaded from log file is empty.");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Kolla returnerar de rader som skiljer mellan storedInFirst och storedInSecond, de rader i storedInFirst som inte finns med i storedInSecond
        /// </summary>
        /// <param name="storedInFirst"></param>
        /// <param name="storedInSecond"></param>
        /// <returns>HashTable</returns>
        private static Hashtable CheckForRowsIn(Hashtable storedInFirst, Hashtable storedInSecond)
        {
            var allFoundRows = new Hashtable();

            #region check efter nya rader

            // Progress
            // _compareProgress.StartCurrent("Checking for new rows. Comparing new sheet" + "" + "...", storedInFirst.Count);
            // int currentProgress = 0;
            #region Ta förinladdad ny log

            foreach (DictionaryEntry storedInFirstRow in storedInFirst)
            {
                // if (MainForm.StopGracefully)
                // return null;

                if (storedInFirstRow.Key is string concatedNewRowCells 
                    && !storedInSecond.ContainsKey(concatedNewRowCells))
                {
                    // Om den bara finns med i nya
                    // färglägg bara unika celler från stored.Value...
                    // Uniqe row found
                    allFoundRows.Add(concatedNewRowCells, storedInFirstRow.Value as ExcelRowEntry);

                    // _compareProgress.SetCurrent(++currentProgress);
                }

                // if (MainForm.StopGracefully)
                // return null;
            }

            #endregion - Ta förinladdad ny log

            #endregion - check efter nya rader

            return allFoundRows;
        }

        /// <summary>
        /// Loggar de rader som skiljer sig till arbetsbladet "saveWorksheet"
        /// </summary>
        /// <param name="saveWorksheet">
        /// </param>
        /// <param name="storedOld">
        /// </param>
        /// <param name="storedNew">
        /// </param>
        private static void CompareExcelRows(Worksheet saveWorksheet, Hashtable storedOld, Hashtable storedNew)
        {
            if (TableAreNull(storedOld, storedNew))
            {
                return;
            }

            try
            {
                // Hämtar ut alla nya rader
                var allNewRows = CheckForRowsIn(storedNew, storedOld);

                // if (MainForm.StopGracefully)
                // return false;

                // Hämtar ut alla deletade rader
                var allOldRows = CheckForRowsIn(storedOld, storedNew);

                // if (MainForm.StopGracefully)
                // return false;
                if (allOldRows.Count > 0 || allNewRows.Count > 0)
                {
                }
                else
                {
                    return;
                }

                #region Logga det som skiljer

                saveWorksheet.Cells.Clear(); // Rensa

                const string SheetName = "AllProfiles";

                // Logga det som skiljer (TODO; fixa så det inte skriver över antalet maxrader)
                // int nextRow = 1;
                var oa = new object[] { saveWorksheet, 4, 1 };

                // Logga new
                var cellLayOutSettings = new Hashtable { { CellLayOutSettings.InteriorColorColorIndexType, 36 } };
                foreach (DictionaryEntry item in allNewRows)
                {
                    if (item.Value is ExcelRowEntry excelRowEntry)
                    {
                        Logger.AddRow(
                            saveWorksheet,
                            SheetName,
                            ref oa,
                            cellLayOutSettings,
                            false,
                            System.Drawing.Color.Empty,
                            0,
                            excelRowEntry.Args); // as string[]//Green// as string[]
                    }

                    // Addr(sheetName, cellLayOutSettings, insertRow, (item.Value as DbInfoLogEntry).args);                    
                }

                // Logga deleted
                cellLayOutSettings.Clear(); // Rensa gamla settings
                cellLayOutSettings.Add(CellLayOutSettings.InteriorColorSysDrawingType, System.Drawing.Color.GreenYellow);
                foreach (DictionaryEntry item in allOldRows)
                {
                    if (item.Value is ExcelRowEntry excelRowEntry)
                    {
                        Logger.AddRow(
                            saveWorksheet,
                            SheetName,
                            ref oa,
                            cellLayOutSettings,
                            false,
                            System.Drawing.Color.Empty,
                            0,
                            excelRowEntry.Args); // as string[]//Green// as string[]
                    }

                    // Addr(sheetName, cellLayOutSettings, insertRow, (item.Value as DbInfoLogEntry).args);                    
                }

                #endregion
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in compareExcelRows: " + e.Message);
            }
        }

        private static bool CompareExcelRows(
            _Worksheet worksheet,
            Hashtable storedOld, 
            Hashtable storedNew, 
            ref int rows, 
            ref int orgColCount)
        {
            // if (storedOld == null)
            // {
            // MessageBox.Show("Hashtable empty.");
            // return false;
            // }
            // skulle ha new är oxo eg.
            if (TableAreNull(storedOld, storedOld))
            {
                return false;
            }

            var somethingDiffers = false;
            var orgRowCount = 0;
            orgColCount = 0; // int

            #region Old

            // Hashtable existsInBoth = new Hashtable();
            // Hashtable onlyExistInOld = new Hashtable();
            // Hashtable onlyExistInNew = new Hashtable();
            // Hashtable allInNew = new Hashtable();
            #endregion

            try
            {
                orgRowCount = worksheet.UsedRange.Rows.Count;
                orgColCount = worksheet.UsedRange.Columns.Count;

                #region check efter nya rader

                // worksheet.UsedRange.Count ger rader, worksheet.UsedRange.Columns.Count ger kolumner

                // Progress
                // _compareProgress.StartCurrent("Comparing sheet: " + worksheet.Name + "...", worksheet.UsedRange.Rows.Count);//Loading
                // int currentProgress = 0;
                #region Ta förinladdad ny log

                // int rowNumber = 0;
                foreach (DictionaryEntry newEntry in storedNew)
                {
                    // if (MainForm.StopGracefully)
                    // return false;

                    #region Old

                    // rowNumber++;
                    // if (!allInNew.ContainsKey(concatedNewRowCells))//TODO: ta bort allInNew o kör direkt på storedNew, ändra alla ställen som anv. det
                    // {
                    // string[] newRowCells = (newEntry.Value as DbInfoLogEntry).args;
                    // //(storedOld[oldrow] as DbInfoLogEntry)

                    // allInNew.Add(concatedNewRowCells, new object[2] { 
                    // newRowCells.Length > 1 ? newRowCells[1] : newRowCells[0]
                    // , (newEntry.Value as DbInfoLogEntry).row});//Lägg till hopklippt rad och andra cellen i raden och radnumret (för DB-info)
                    // } 
                    #endregion

                    if (newEntry.Key is string concatedNewRowCells && !storedOld.ContainsKey(concatedNewRowCells))
                    {
                        // Om den bara finns med i nya
                        // färglägg bara unika celler från stored.Value...
                        // Uniqe row found

                        // hämta radnumret
                        if (storedNew.ContainsKey(concatedNewRowCells))
                        {
                            if (storedNew[concatedNewRowCells] is ExcelRowEntry excelRowEntry)
                            {
                                var currentRowNumber = excelRowEntry.Row;

                                // int currentRowNumber = ((int)(allInNew[concatedNewRowCells] as object[])[1]);
                                var column = GetStandardExcelColumnName(orgColCount + 1);

                                // worksheet.UsedRange.Columns.Count + 1);
                                var range =
                                    worksheet.Range["A" + currentRowNumber.ToString(CultureInfo.InvariantCulture),
                                        column + currentRowNumber.ToString(CultureInfo.InvariantCulture)]; // "IV" 

                                range.Interior.ColorIndex = 36; // EntireRow
                                worksheet.Cells[currentRowNumber, orgColCount + 1] = "NEW"; // args[i].ToString();
                            }
                        }

                        somethingDiffers = true;

                        // if (!onlyExistInNew.ContainsKey(strArray))
                        // {
                        // onlyExistInNew.Add(strArray, 0);
                        // }

                        // _compareProgress.SetCurrent(++currentProgress);
                    }

                    // if (MainForm.StopGracefully)
                    // return false;
                }

                #endregion - Ta förinladdad ny log

                #region old

                // for (int i = 1; i <= orgRowCount; i++)//worksheet.Rows.Count; i++)//65536; i++)
                // {
                // if (MainForm.stopGracefully)
                // return false;

                // //TODO: Använd samma fkn för att läsa in old som new, alltså; läs in old först, sen läs in hela new, sen jmfr.  ta ut många rader på en gång från Excel, lägg dem i som nu görs i HT, viktigt med rätt radnr, sen kolla om det skiljer, om det skiljer, hämta ut den raden från excel och byt färg etc.
                // string column = GetStandardExcelColumnName(orgColCount + 1);//worksheet.UsedRange.Columns.Count + 1);
                // Excel.Range range = worksheet.get_Range("A" + i.ToString(), column + i.ToString());//"IV" 
                // System.Array myvalues = (System.Array)range.Cells.get_Value(Type.Missing);//Value;

                // string[] strArrayIn = ConvertToStringArray(myvalues);
                // string strArray = "";
                // foreach (string arg in strArrayIn)
                // {
                // //logMessages[argNr++] = arg;
                // strArray += arg;// +" ";
                // }

                // if (!storedOld.ContainsKey(strArray))
                // {
                // //Om den bara finns med i nya

                // //oldRows.Add(strArray, 0);
                // //Tdo: färglägg bara unika celler från stored.Value...strArrayIn

                // //Uniqe row found
                // range.Interior.ColorIndex = 36;//EntireRow
                // //((Excel.Range)worksheet.Cells[i, _orgColCount + 1]).Interior.ColorIndex = 36;
                // worksheet.Cells[i, orgColCount + 1] = "NEW";//args[i].ToString();

                // somethingDiffers = true;

                // //if (!onlyExistInNew.ContainsKey(strArray))
                // //{
                // //    onlyExistInNew.Add(strArray, 0);
                // //}
                // }
                // else
                // {
                // //Om den finns med i både gamla o nya
                // //if (!existsInBoth.ContainsKey(strArray))
                // //{
                // //    existsInBoth.Add(strArray, 0);
                // //}
                // }

                // //Spara de som finns med i nya till senare jämförelse
                // if (!allInNew.ContainsKey(strArray))
                // {
                // allInNew.Add(strArray, new object[2] { strArrayIn[1], i });//i);//Lägg till 
                // }

                // _compareProgress.SetCurrent(++currentProgress);

                // if (MainForm.stopGracefully)
                // return false;
                // } 
                #endregion

                #endregion - check efter nya rader

                #region check deleted

                var sheets = new Hashtable { { worksheet.Name, new object[] { worksheet, orgRowCount + 1, 1 } } };

                // +1 på rad för att det är var man ska skriva nästa cellrad.
                var somethingFoundDeletedAndExtraTitelsWritten = false;

                // Special för DatabaseInfo
                // int DbInsertrow = 4;//Om man ska skriva ut dbinfos deleted så ska de vara f.o.m rad 4

                // _compareProgress.StartCurrent("Checking for deleted rows. Comparing sheet: " + worksheet.Name + "...", storedOld.Count);//Loading //Progress
                // currentProgress = 0;
                // Kolla vilka som har tagis bort (delete), de som inte finns med i den nya, men som fanns med i den gamla
                // Kolla igenom den gamla, hittar man något som inte finns med i den nya så lägg till den i loggen och välj speciell färg (grön)
                foreach (string oldrow in storedOld.Keys)
                {
                    if (!storedNew.ContainsKey(oldrow))
                    {
                        #region hittat deleted

                        // lägg till raden i loggen och välj speciell färg (grön), TODO: ha funktioner när man skickar med wilken excelbok man har och vilken sheets-tabell, så all excelkod ligger i logger.cs
                        try
                        {
                            var sheetName = worksheet.Name;
                            var oa = sheets[sheetName] as object[];

                            string saveAsSheetName;

                            #region Check if rowcount exceeded maximum

                            if ((int)oa[2] > 1)
                            {
                                // Excel.Workbook _book = new Excel.Workbook();
                                // Excel.Worksheet nextSheet = _book.Worksheets.Add(Type.Missing, _last, Type.Missing, Type.Missing) as Excel.Worksheet;
                                // Excel.Worksheet nextSheet = new Excel.Worksheet() as Excel.Worksheet;//_book.Worksheets.Add(Type.Missing, _last, Type.Missing, Type.Missing) as Excel.Worksheet;//Type.Missing, _last, Type.Missing, Type.Missing

                                // nextSheet.Name = saveAsSheetName = "Prov_part2";
                                // sheets.Add(saveAsSheetName, new object[] { nextSheet, 1, 4 });
                                saveAsSheetName = sheetName + "_part" + oa[2];
                                oa = sheets[sheetName + "_part" + oa[2]] as object[]; // oa[0] as Excel.Worksheet; 
                            }
                            else
                            {
                                saveAsSheetName = sheetName;
                            }

                            #endregion

                            var sheet = oa[0] as Worksheet;

                            int nextRow;

                            // = Logger.addRow(sheet, saveAsSheetName, ref oa, null, false, System.Drawing.Color.Green, 0, stored[oldrow] as string[]);// as string[]
                            if (worksheet.Name != "DatabaseInfo")
                            {
                                nextRow = Logger.AddRow(
                                    sheet,
                                    saveAsSheetName,
                                    ref oa,
                                    null,
                                    false,
                                    System.Drawing.Color.GreenYellow,
                                    0,
                                    (storedOld[oldrow] as ExcelRowEntry).Args); // as string[]//Green// as string[]
                            }
                            else
                            {
                                #region om det är databaseinfo som ska jmfr

                                if (!somethingFoundDeletedAndExtraTitelsWritten)
                                {
                                    // var titleCellLayout = new Hashtable { { CellLayOutSettings.Bold, true } };
                                    // nextRow = Logger.addRow(
                                    // sheet,
                                    // saveAsSheetName,
                                    // ref oa,
                                    // titleCellLayout,
                                    // true,
                                    // System.Drawing.Color.Empty,
                                    // 3,
                                    // 3,
                                    // "Old quantity");
                                    // nextRow = Logger.addRow(
                                    // sheet,
                                    // saveAsSheetName,
                                    // ref oa,
                                    // titleCellLayout,
                                    // true,
                                    // System.Drawing.Color.Empty,
                                    // 3,
                                    // 4,
                                    // "Difference");
                                    somethingFoundDeletedAndExtraTitelsWritten = true;
                                }

                                // nextRow = Logger.addRow(
                                // sheet,
                                // saveAsSheetName,
                                // ref oa,
                                // null,
                                // false,
                                // System.Drawing.Color.GreenYellow,
                                // (storedOld[oldrow] as ExcelRowEntry).row,
                                // 3,
                                // (storedOld[oldrow] as ExcelRowEntry).args[1]);
                                var currRow = (storedOld[oldrow] as ExcelRowEntry).Row;
                                var cellLayout = new Hashtable();

                                #region Skriv med rött om antalet gått ner sen gamla loggen

                                var newQuantity = 0;

                                // foreach (object[] newRow in allInNew.Values)
                                foreach (DictionaryEntry newRow in storedNew)
                                {
                                    // string currRow = (storedOld[oldrow] as DbInfoLogEntry).row.ToString();
                                    if (currRow == (newRow.Value as ExcelRowEntry).Row)
                                    {
                                        // Hittat, ska nu hämta ut quantity, som finns i value 0
                                        newQuantity = int.Parse((string)(newRow.Value as ExcelRowEntry).Args[1]);

                                        // newQuantity = int.Parse(newRow[0].ToString());
                                        break;
                                    }
                                }

                                var oldQuantity = newQuantity
                                                  - int.Parse((string)(storedOld[oldrow] as ExcelRowEntry).Args[1]);

                                // Det som kommer stå i cellen sen. I.e nya-gamla quantity, blir bökigt att hämta ut
                                if (oldQuantity < 0)
                                {
                                    cellLayout.Add(CellLayOutSettings.TextColor, System.Drawing.Color.Red);
                                }

                                #endregion

                                nextRow = Logger.AddRow(
                                    sheet,
                                    saveAsSheetName,
                                    ref oa,
                                    cellLayout,
                                    false,
                                    Color.Empty,
                                    (storedOld[oldrow] as ExcelRowEntry).Row,
                                    4, 
                                    "=B" + currRow + "-D" + currRow);

                                // BC när den är färdig, men "new" finns som rad o då blird det B+D
                                #endregion
                            }

                            #region Check if rowcount exceeded maximum

                            // +1 fär oa[1] (nästa rad) är alltid 1 större än nextrow är här
                            // tar sista raden oxå //(excelMaxNoRows-2) )//tar ínte allra sista raden för säkerhets skull
                            if (nextRow + 1 > (Logger.excelMaxNoRows - 2))
                            {
                                // Gör ett nytt ark med samma namn + siffra (EX. Prov_part2)

                                // Ev. skriv något på sista raden typ: "Fortsättning på nästa ark _part2...

                                // Excel.Constants.xlMaximum
                                // Excel.Application _app = new Excel.ApplicationClass();                               
                                // Excel.Workbook _book = _app.Workbooks.Add(Excel.XlWBATemplate.xlWBATWorksheet) as Excel.Workbook;

                                // Använd newLog för att skapa nya ark
                                // Excel.Worksheet nextSheet = _newLog.Sheets[1] as Excel.Worksheet;
                                var orgOa = sheets[sheetName] as object[];
                                orgOa[2] = (int)orgOa[2] + 1; // ökar antal delark i en log
                                var newSheetName = sheetName + "_part" + orgOa[2]; // Ex. Prov_part2

                                // Excel.Worksheet nextSheet = new Excel.Worksheet() as Excel.Worksheet;//_book.Worksheets.Add(Type.Missing, _last, Type.Missing, Type.Missing) as Excel.Worksheet;//Type.Missing, _last, Type.Missing, Type.Missing
                                // Excel.Worksheet nextSheet = _book.Worksheets.Add(Type.Missing, _last, Type.Missing, Type.Missing) as Excel.Worksheet;
                                var last = sheet;
                                var nextSheet =
                                    newLog.Worksheets.Add(Type.Missing, last, Type.Missing, Type.Missing) as Worksheet;

                                nextSheet.Name = newSheetName;
                                sheets.Add(newSheetName, new object[] { nextSheet, 4, 0 });
                            }

                            #endregion

                            // _sheets[sheetName] = oa;
                            sheets[saveAsSheetName] = oa;

                            // return cellRange;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error in Logger, may be Excel error: " + e.Message);

                            // throw e;
                            // return null;
                        }

                        // Logger.addRow(worksheet,
                        #endregion - hitta deleted
                    }

                    // _compareProgress.SetCurrent(++currentProgress);
                }

                if (sheets.Count > 1)
                {
                    // Lägg till nya sheets
                }

                #endregion
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in compareExcelRows: " + e.Message);
            }

            rows = orgRowCount;

            return somethingDiffers;
        }
    }
}