using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Budgetterarn.Application_Settings_and_constants
{
    class Filerefernces
    {
        public static string ExcelFileSavePathWithoutFileName { get; set; }
        public static string _excelFileSaveFileName { get; set; }
        public static string _excelFileSavePath { get; set; }

        static Filerefernces()
        {
            //Get file names from settings file
            var appPath = AppDomain.CurrentDomain.BaseDirectory;

            _excelFileSaveFileName = GeneralSettings.GetStringSetting("ExcelFileSavePathFileName");
            ExcelFileSavePathWithoutFileName = GeneralSettings.GetStringSetting("ExcelFileSaveDirPath");
            _excelFileSavePath = ExcelFileSavePathWithoutFileName + _excelFileSaveFileName;
            _excelFileSavePath = Path.Combine(appPath, _excelFileSavePath);
        }
    }
}
