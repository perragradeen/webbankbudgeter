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

            ExcelFileSaveFileName =
                GeneralSettings.GetStringSetting("ExcelFileSavePathFileName");
            ExcelFileSavePathWithoutFileName =
                GeneralSettings.GetStringSetting("ExcelFileSaveDirPath");
            ExcelFileSavePath =
                ExcelFileSavePathWithoutFileName + ExcelFileSaveFileName;
            ExcelFileSavePath = Path.Combine(appPath, ExcelFileSavePath);
        }

        public static string ExcelFileSavePathWithoutFileName { get; set; }
        public static string ExcelFileSaveFileName { get; set; }
        public static string ExcelFileSavePath { get; set; }
    }
}