using System;
using System.IO;
using Budgeter.Core.BudgeterConstants;

namespace Budgetterarn.Application_Settings_and_constants
{
    internal static class FileReferences
    {
        /// <summary>
        /// Kontoutdrag_officiella
        /// </summary>
        internal static readonly string SheetName = BankConstants.SheetName;

        static FileReferences()
        {
            // Get file names from settings file
            var appPath = AppDomain.CurrentDomain.BaseDirectory; // TODO: fixa en allmän funktion som ger sökväg relativ

            ExcelFileSaveFileName =
                GeneralSettings.GetStringSetting("ExcelFileSavePathFileName");
            ExcelFileSavePathWithoutFileName =
                GeneralSettings.GetStringSetting("ExcelFileSaveDirPath");
            ExcelFileSavePath =
                ExcelFileSavePathWithoutFileName + ExcelFileSaveFileName;
            ExcelFileSavePath = Path.Combine(appPath, ExcelFileSavePath);
        }

        public static string ExcelFileSavePathWithoutFileName { get; }
        public static string ExcelFileSaveFileName { get; }
        public static string ExcelFileSavePath { get; }
    }
}