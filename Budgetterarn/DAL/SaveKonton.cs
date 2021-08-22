using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Budgeter.Core.BudgeterConstants;
using Budgeter.Core.Entities;
using Utilities;

// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo

namespace Budgetterarn.DAL
{
    public static class SaveKonton
    {
        internal static LoadOrSaveResult Save(
            KontoutdragInfoForSave kontoutdragInfoForSave,
            SortedList kontoEntries,
            SaldoHolder saldoHolder)
        {
            try
            {
                // If nothing to save, return
                if (kontoEntries == null || kontoEntries.Count == 0)
                {
                    return new LoadOrSaveResult();
                }

                var logArray = GetTopRowWithHeaders(saldoHolder);
                var logThis = GetWhatToLogWithHeaders(logArray, kontoEntries);

                ReIndexKontoentriesToLatestOnTop(kontoEntries, logThis);

                BackupOldFile(kontoutdragInfoForSave);

                // spara över gammalt, innan skrevs det på sist
                Logger.WriteToWorkBook(
                    kontoutdragInfoForSave.ExcelFileSavePath, kontoutdragInfoForSave.SheetName, true, logThis);

                return new LoadOrSaveResult { SkippedOrSaved = logThis.Count - 1, SomethingLoadedOrSaved = false };
            }
            catch (Exception savExcp)
            {
                MessageBox.Show(@"Error: " + savExcp.Message);
                return new LoadOrSaveResult();
            }
        }

        private static void BackupOldFile(KontoutdragInfoForSave kontoutdragInfoForSave)
        {
            // Gör någon backup el. likn. för att inte förlora data. Backupa dynamiskt. Så att om man skickar in en fil så backas den upp istället för huvudfilen...men de e rätt ok att backa huvudfilen
            new FileBackupper(
                "Before.Save",
                kontoutdragInfoForSave.ExcelFileSavePath,
                kontoutdragInfoForSave.ExcelFileSavePathWithoutFileName,
                kontoutdragInfoForSave.ExcelFileSaveFileName
            ).BackupOrginialFile();
        }

        private static Hashtable GetWhatToLogWithHeaders(IEnumerable logArray, ICollection kontoEntries)
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

        private static void ReIndexKontoentriesToLatestOnTop(ICollection kontoEntries, IDictionary logThis)
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
