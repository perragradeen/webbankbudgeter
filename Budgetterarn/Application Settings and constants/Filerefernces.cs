using System;
using System.IO;

namespace Budgetterarn.Application_Settings_and_constants
{
    internal class Filerefernces
    {
        static Filerefernces()
        {
            // Get file names from settings file
            var appPath = AppDomain.CurrentDomain.BaseDirectory;// TODO: fixa en allmän funktion som ger sökväg relativ

            _excelFileSaveFileName =
                GeneralSettings.GetStringSetting("ExcelFileSavePathFileName");
            ExcelFileSavePathWithoutFileName =
                GeneralSettings.GetStringSetting("ExcelFileSaveDirPath");
            _excelFileSavePath =
                ExcelFileSavePathWithoutFileName + _excelFileSaveFileName;
            _excelFileSavePath = Path.Combine(appPath, _excelFileSavePath);
        }

        public static string ExcelFileSavePathWithoutFileName { get; set; }
        public static string _excelFileSaveFileName { get; set; }
        public static string _excelFileSavePath { get; set; }
    }
}