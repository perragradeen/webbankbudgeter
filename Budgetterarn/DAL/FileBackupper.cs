using System;

namespace Budgetterarn.DAL
{
    public class FileBackupper
    {
        private string typeOfBackup;
        private string excelFileSavePath;
        private string excelFileSavePathWithoutFileName;
        private string excelFileSaveFileName;

        public FileBackupper(string v, string excelFileSavePath, string excelFileSavePathWithoutFileName, string excelFileSaveFileName)
        {
            typeOfBackup = v;
            this.excelFileSavePath = excelFileSavePath;
            this.excelFileSavePathWithoutFileName = excelFileSavePathWithoutFileName;
            this.excelFileSaveFileName = excelFileSaveFileName;
        }

        public void BackupOrginialFile()
        {
            BackupOrginialFile(
                typeOfBackup + "." + excelFileSaveFileName);
        }

        private static string GetTimeNowString =>
            DateTime.Now.ToString(new System.Globalization.CultureInfo("sv-SE")).Replace(":", ".");

        private void BackupOrginialFile(string newFileName)
        {
            // TODO: check that dir exists and path etc
            System.IO.File.Copy(
                sourceFileName:
                    excelFileSavePath,
                destFileName:
                    excelFileSavePathWithoutFileName + @"bak\" + newFileName + "."
                    + GetTimeNowString + ".bak.xls",
                overwrite:
                    true);
        }

    }
}
