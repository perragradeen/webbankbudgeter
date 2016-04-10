using Budgeter.Core.Entities;
using Budgetterarn.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;

namespace Budgetterarn.DAL
{
    public class SaveKonton
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
                var logThis = GetWhatToLogWithHeaders(ProgramSettings.BankType, logArray, kontoEntries);

                ReIndexKontoentriesToLatestOnTop(kontoEntries, logThis);

                // Gör någon backup el. likn. för att inte förlora data. Backupa dynamiskt. Så att om man skickar in en fil så backas den upp istället för huvudfilen...men de e rätt ok att backa huvudfilen
                BackupOrginialFile(
                    "Before.Save",
                    kontoutdragInfoForSave.excelFileSavePath,
                    kontoutdragInfoForSave.excelFileSavePathWithoutFileName,
                    kontoutdragInfoForSave.excelFileSaveFileName);

                // spara över gammalt, innan skrevs det på sist
                Logger.WriteToWorkBook(
                    kontoutdragInfoForSave.excelFileSavePath, kontoutdragInfoForSave.sheetName, true, logThis);

                return new LoadOrSaveResult { skippedOrSaved = logThis.Count - 1, somethingLoadedOrSaved = false };
            }
            catch (Exception savExcp)
            {
                MessageBox.Show(@"Error: " + savExcp.Message);
                return new LoadOrSaveResult();
            }
        }

        private static Hashtable GetWhatToLogWithHeaders(BankType bankType, object[] logArray, SortedList kontoEntries)
        {
            // Gör om till Arraylist för ordning, det blir i omvänd ordning, alltså först överst. Ex 2009-04-01 sen 2009-04-02 osv.
            Hashtable logThis = null;

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
                throw new Exception("Bank type not allowed: " + ProgramSettings.BankType.ToString());
            }

            return logThis;
        }

        private static void ReIndexKontoentriesToLatestOnTop(SortedList kontoEntries, Hashtable logThis)
        {
            var indexKey = kontoEntries.Count;
            foreach (DictionaryEntry currentRow in kontoEntries)
            {
                // string key = currentRow.Key as string;
                var currentKeEntry = currentRow.Value as KontoEntry;
                if (currentKeEntry != null)
                {
                    logThis.Add(indexKey--, currentKeEntry.RowToSaveForThis); // Använd int som nyckel
                }
            }
        }

        private static object[] GetTopRowWithHeaders(SaldoHolder saldoHolder)
        {
            // saldon
            var saldoColumnNumber = 11 + 1;
            var columnNames = new object[] { "y", "m", "d", "n", "t", "g", "s", "b", "", "", "", "c" };

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


        private static void BackupOrginialFile(
            string typeOfBackup,
            string excelFileSavePath,
            string excelFileSavePathWithoutFileName,
            string excelFileSaveFileName)
        {
            BackupOrginialFile(
                excelFileSavePath, excelFileSavePathWithoutFileName, typeOfBackup + "." + excelFileSaveFileName);
        }

        private static void BackupOrginialFile(
            string orgfilePath, string newFilePathWithoutFileName, string newFileName)
        {
            // TODO: check that dir exists and path etc
            System.IO.File.Copy(
                orgfilePath,
                newFilePathWithoutFileName + @"bak\" + newFileName + "."
                + DateTime.Now.ToString(new System.Globalization.CultureInfo("sv-SE")).Replace(":", ".") + ".bak.xls",
                true);
        }
    }

}
