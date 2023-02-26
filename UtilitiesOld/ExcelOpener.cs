using System;
using System.IO;
using System.Threading;
using Microsoft.Office.Interop.Excel;

namespace Utilities
{
    public static class ExcelOpener
    {
        // Contains Excel-app to use for opening file after save, in Excel mode. And auto close it when close...
        private static Application _excelAppOpen;

        public static void LoadExcelFileInExcel(string excelFileSavePath)
        {
            try
            {
                var fileOkToOpen = true;

                #region check file

                try
                {
                    var newFile = new FileInfo(excelFileSavePath);
                    if (File.Exists(excelFileSavePath))
                    {
                        using (newFile.Open(FileMode.Open))
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

                if (!fileOkToOpen)
                    return;

                #region Open log in Exel //before: tab window

                // Start new Excel-instance
                _excelAppOpen = new Application();
                _excelAppOpen.WorkbookDeactivate += ApplicationWorkbookDeactivate;

                var oldCi = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                _excelAppOpen.Workbooks?._Open(
                    excelFileSavePath,
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

                _excelAppOpen.Visible = true;

                Thread.CurrentThread.CurrentCulture = oldCi;

                #endregion
            }
            catch (Exception fileExp)
            {
                Console.WriteLine(@"Error in LoadComparedLogIn: " + fileExp.Message);
            }
        }

        private static void ApplicationWorkbookDeactivate(Workbook wb)
        {
            // Stäng och släpp excel
            _excelAppOpen.Quit();

            while (System.Runtime.InteropServices.Marshal.ReleaseComObject(_excelAppOpen) != 0)
            {
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            // ReSharper disable RedundantAssignment
            // Wants to be sure excelAppOpen is cleared
            _excelAppOpen = null;

            // ReSharper restore RedundantAssignment
        }
    }
}