using System;
using System.Globalization;
using System.IO;

namespace Budgetterarn.DAL
{
    public class FileBackupper
    {
        private readonly string _typeOfBackup;
        private readonly string _excelFileSavePath;
        private readonly string _excelFileSavePathWithoutFileName;
        private readonly string _excelFileSaveFileName;

        public FileBackupper(string v, string excelFileSavePath, string excelFileSavePathWithoutFileName, string excelFileSaveFileName)
        {
            _typeOfBackup = v;
            _excelFileSavePath = excelFileSavePath;
            _excelFileSavePathWithoutFileName = excelFileSavePathWithoutFileName;
            _excelFileSaveFileName = excelFileSaveFileName;
        }

        public void BackupOrginialFile()
        {
            BackupOrginialFile(
                _typeOfBackup + "." + _excelFileSaveFileName);
        }

        private static string GetTimeNowString =>
            DateTime.Now.ToString(new CultureInfo("sv-SE")).Replace(":", ".");

        private void BackupOrginialFile(string newFileName)
        {
            // TODO: check that dir exists and path etc
            File.Copy(
                sourceFileName:
                    _excelFileSavePath,
                destFileName:
                    _excelFileSavePathWithoutFileName + @"bak\" + newFileName + "."
                    + GetTimeNowString + ".bak.xls",
                overwrite:
                    true);
        }

    }
}
