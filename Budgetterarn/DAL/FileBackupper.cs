using System;
using System.Globalization;
using System.IO;
using Budgeter.Core.Entities;

namespace Budgetterarn.DAL
{
    public class FileBackupper
    {
        private const string _typeOfBackup = "Before.Save";
        private readonly KontoutdragExcelFileInfo excelFileInfo;

        public FileBackupper(KontoutdragExcelFileInfo excelFileInfo)
        {
            this.excelFileInfo = excelFileInfo;
        }

        public void BackupOrginialFile()
        {
            var destinationPath = Path.Combine(
                excelFileInfo.ExcelFileSavePathWithoutFileName,
                @"bak\");

            Directory.CreateDirectory(destinationPath);

            File.Copy(
                sourceFileName:
                excelFileInfo.ExcelFileSavePath,
                destFileName:
                GetFullDestinationFileName(
                    GetFullDestinationFileName(),
                    destinationPath),
                overwrite: true);
        }

        private string GetFullDestinationFileName(
            string fileName,
            string destinationPath)
        {
            return Path.Combine(destinationPath, fileName);
        }

        private string GetFullDestinationFileName()
        {
            return
                _typeOfBackup + "." +
                excelFileInfo.ExcelFileSaveFileName + "." +
                GetTimeNowString +
                ".bak.xls";
        }

        private static string GetTimeNowString =>
            DateTime.Now
                .ToString(new CultureInfo("sv-SE"))
                .Replace(":", ".");
    }
}