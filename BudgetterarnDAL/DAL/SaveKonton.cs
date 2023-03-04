using BudgeterCore;
using BudgeterCore.BudgeterConstants;
using BudgeterCore.Entities;
using LoadTransactionsFromFile;
using System.Collections;
using Utilities;

// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo

namespace BudgetterarnDAL.DAL
{
    public static class SaveKonton
    {
        public static LoadOrSaveResult Save(
            KontoutdragExcelFileInfo kontoutdragExcelFileInfo,
            KontoEntriesHolder kontoEntriesHolder,
            Action<string> writeToOutput)
        {
            var kontoEntries = kontoEntriesHolder.KontoEntries;

            try
            {
                // If nothing to save, return
                if (kontoEntries == null || kontoEntries.Count == 0)
                {
                    return new LoadOrSaveResult();
                }

                var logArray = GetTopRowWithHeaders(kontoEntriesHolder.SaldoHolder);
                var logThis = GetWhatToLogWithHeaders(logArray, kontoEntries);

                ReIndexKontoentriesToLatestOnTop(kontoEntries, logThis);

                BackupOldFile(kontoutdragExcelFileInfo);

                // spara över gammalt, innan skrevs det på sist
                Logger.WriteToWorkBook(
                    kontoutdragExcelFileInfo.ExcelFileSavePath,
                    kontoutdragExcelFileInfo.SheetName,
                    logThis);

                return new LoadOrSaveResult
                {
                    SkippedOrSaved = logThis.Count - 1,
                    SomethingLoadedOrSaved = false
                };
            }
            catch (Exception savExcp)
            {
                writeToOutput(@"Error: " + savExcp.Message);
                return new LoadOrSaveResult();
            }
        }

        private static void BackupOldFile(KontoutdragExcelFileInfo kontoutdragExcelFileInfo)
        {
            // Gör någon backup el. likn. för att inte förlora data. Backupa dynamiskt.
            // Så att om man skickar in en fil så backas den upp istället för huvudfilen...
            // men de e rätt ok att backa huvudfilen
            new FileBackupper(kontoutdragExcelFileInfo)
                .BackupOrginialFile();
        }

        private static Hashtable GetWhatToLogWithHeaders(
            IEnumerable logArray,
            ICollection kontoEntries)
        {
            // Gör om till Arraylist för ordning, det blir i omvänd ordning, alltså först överst. Ex 2009-04-01 sen 2009-04-02 osv.
            Hashtable logThis;

            // Lägg till överskrifter
            // y m d n t g s b    c
            if (ProgramSettings.BankType.Equals(BankType.Swedbank)
                || ProgramSettings.BankType.Equals(BankType.Mobilhandelsbanken)
                || ProgramSettings.BankType.Equals(BankType.Handelsbanken)
            )
            {
                logThis = new Hashtable { { kontoEntries.Count + 1, logArray } };
            }
            else
            {
                throw new Exception("Bank type not allowed: " + ProgramSettings.BankType);
            }

            return logThis;
        }

        private static void ReIndexKontoentriesToLatestOnTop(
            ICollection kontoEntries,
            IDictionary logThis)
        {
            var indexKey = kontoEntries.Count;
            foreach (DictionaryEntry currentRow in kontoEntries)
            {
                if (currentRow.Value is KontoEntry currentKeEntry)
                {
                    logThis.Add(indexKey--, currentKeEntry.RowToSaveForThis); // Använd int som nyckel
                }
            }
        }

        private static IEnumerable<object> GetTopRowWithHeaders(SaldoHolder saldoHolder)
        {
            var columnNames = new object[] { "y", "m", "d", "n", "t", "g", "s", "b", "", "", "", "c" };

            var saldoColumnNumber = 11 + 1;

            var logArray = new object[columnNames.Length + saldoHolder.Saldon.Count];

            var index = 0;
            foreach (var s in columnNames)
            {
                logArray[index++] = s;
            }

            foreach (var s in saldoHolder.Saldon)
            {
                logArray[saldoColumnNumber++] = s.SaldoValue;
            }

            return logArray;
        }
    }
}