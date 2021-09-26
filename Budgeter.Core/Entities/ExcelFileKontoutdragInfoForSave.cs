namespace Budgeter.Core.Entities
{
    public class ExcelFileKontoutdragInfoForSave
    {
        public string ExcelFileSavePath { get; set; }
        public string ExcelFileSavePathWithoutFileName { get; set; }
        public string ExcelFileSaveFileName { get; set; }

        public string SheetName { get; set; }
    }
}