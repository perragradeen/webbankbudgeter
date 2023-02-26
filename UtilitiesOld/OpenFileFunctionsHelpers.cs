using Microsoft.Office.Interop.Excel;
using System;

namespace Utilities
{
    internal static class OpenFileFunctionsHelpers
    {

        internal static Workbook OpenExcelBook(
            string excelBookPath,
            Application excelApp)
        {
            return excelApp.Workbooks._Open(
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
        }
    }
}