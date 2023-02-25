using Budgeter.Core.BudgeterConstants;
using GeneralSettingsHandler;

namespace Budgetterarn.Application_Settings_and_constants
{
    internal class FileReferences
    {
        /// <summary>
        /// Kontoutdrag_officiella
        /// </summary>
        internal static readonly string SheetName = BankConstants.SheetName;

        internal FileReferences(GeneralSettingsGetter generalSettingsGetter)
        {
            // Get file names from settings file
            var appPath = AppDomain.CurrentDomain.BaseDirectory; // TODO: fixa en allmän funktion som ger sökväg relativ

            ExcelFileSaveFileName =
                generalSettingsGetter.GetStringSetting("ExcelFileSavePathFileName");
            ExcelFileSavePathWithoutFileName =
                generalSettingsGetter.GetStringSetting("ExcelFileSaveDirPath");
            ExcelFileSavePath =
                ExcelFileSavePathWithoutFileName + ExcelFileSaveFileName;
            ExcelFileSavePath = Path.Combine(appPath, ExcelFileSavePath);
        }

        public string ExcelFileSavePathWithoutFileName { get; }
        public string ExcelFileSaveFileName { get; }
        public string ExcelFileSavePath { get; }
    }
}