using Microsoft.Office.Interop.Excel;
using System;
using System.Threading;

namespace Utilities
{
    public class ExcelOpener
    {
        // Contains Excelapp to use for opening file after save, in Excel mode. And auto close it when close...
        private static Application excelAppOpen;

        public static void LoadExcelFileInExcel(string excelFileSavePath)
        {
            // SetStatusBar(EStatusBar.eProcessing);
            try
            {
                var filePath = excelFileSavePath;
                // Cursor.Current = Cursors.WaitCursor;
                var fileOkToOpen = true;

                #region check file

                try
                {
                    var newFile = new System.IO.FileInfo(filePath);
                    if (System.IO.File.Exists(filePath))
                    {
                        using (newFile.Open(System.IO.FileMode.Open))
                        {
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception fileExp)
                {
                    fileOkToOpen = false;
                    Console.WriteLine("File already open or other error: " + fileExp.Message);
                }

                #endregion

                if (fileOkToOpen)
                {
                    #region Old öppna med reflect proccess

                    // System.Diagnostics.Process proc = new System.Diagnostics.Process();

                    // string processPath = @"C:\Program Files\Microsoft Office\OFFICE11\";
                    // proc.StartInfo = new System.Diagnostics.ProcessStartInfo(processPath + "Excel" + ".exe", filePath);//C:\\windows\\system32\\
                    #endregion

                    #region Open log in Exel //before: tab window

                    // Start new Excel-instance
                    excelAppOpen = new Application();
                    excelAppOpen.WorkbookDeactivate += ApplicationWorkbookDeactivate;

                    var oldCi = Thread.CurrentThread.CurrentCulture;
                    Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                    if (excelAppOpen.Workbooks != null)
                    {
                        excelAppOpen.Workbooks._Open(
                            filePath,
                            Type.Missing,
                            0,
                            Type.Missing,
                            XlPlatform.xlWindows,
                            Type.Missing,
                            Type.Missing,
                            Type.Missing,
                            false, // COMMA
                            Type.Missing,
                            Type.Missing,
                            Type.Missing,
                            Type.Missing);
                    }

                    excelAppOpen.Visible = true;

                    Thread.CurrentThread.CurrentCulture = oldCi;

                    #endregion
                }
            }
            catch (Exception fileExp)
            {
                Console.WriteLine(@"Error in LoadComparedLogIn: " + fileExp.Message);
            }
            finally
            {
                // Cursor.Current = Cursors.Default;
            }

        }

        private static void ApplicationWorkbookDeactivate(Workbook wb)
        {
            // Stäng och släpp excel
            excelAppOpen.Quit();

            while (System.Runtime.InteropServices.Marshal.ReleaseComObject(excelAppOpen) != 0)
            {
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            // ReSharper disable RedundantAssignment
            // Wants to be sure excelAppOpen is cleared
            excelAppOpen = null;

            // ReSharper restore RedundantAssignment
        }
    }
}
