using Budgeter.Core.Entities;
using LoadTransactionsFromFile;
using LoadTransactionsFromFile.DAL;
using System;
using System.Collections;

namespace Budgetterarn.InternalUtilities
{
    public class LoadFromFileHelper
    {
        private readonly KontoutdragExcelFileInfo kontoutdragExcelFileInfo;
        private readonly KontoEntriesHolder kontoEntriesHolder;
        private readonly Action<string> writeToOutput;
        private readonly Action<string> writeToUiStatusLog;

        public LoadFromFileHelper(
            KontoutdragExcelFileInfo kontoutdragExcelFileInfo,
            KontoEntriesHolder kontoEntriesHolder,
            Action<string> writeToOutput,
            Action<string> writeToUiStatusLog)
        {
            this.kontoutdragExcelFileInfo = kontoutdragExcelFileInfo;
            this.kontoEntriesHolder = kontoEntriesHolder;
            this.writeToOutput = writeToOutput;
            this.writeToUiStatusLog = writeToUiStatusLog;
        }

        internal void SetEntriesFromFile()
        {
            // Ladda från fil
            var entriesLoadedFromDataStore = GetEntriesFromFile();
            if (entriesLoadedFromDataStore == null) return;

            VisaAnvändarenAttIngetLaddatsÄn(entriesLoadedFromDataStore);

            var loadResult =
                EntriesFromExcelTransFormer.TransformFromExcelFileToList(
                    kontoEntriesHolder,
                    entriesLoadedFromDataStore);

            VisaFörAnvändarenHurDetGick(loadResult);
        }

        private void VisaAnvändarenAttIngetLaddatsÄn(Hashtable entriesLoadedFromDataStore)
        {
            var statusText = @"Nothing loaded.";
            writeToUiStatusLog(statusText);

            if (entriesLoadedFromDataStore == null)
            {
                statusText += kontoutdragExcelFileInfo.ExcelFileSavePath;
            }

            writeToUiStatusLog(statusText);
        }

        private void VisaFörAnvändarenHurDetGick(LoadOrSaveResult loadResult)
        {
            string statusText = "No. rows loaded; "
                         + kontoEntriesHolder.KontoEntries.Count
                         + " . Skpped: "
                         + loadResult.SkippedOrSaved
                         + ". File loaded; "
                         + kontoutdragExcelFileInfo.ExcelFileSavePath;
            writeToUiStatusLog(statusText);
        }

        private Hashtable GetEntriesFromFile()
        {
            try
            {
                return LoadEntriesFromFileHandler.LoadEntriesFromFile(
                    kontoutdragExcelFileInfo);
            }
            catch (Exception)
            {
                writeToOutput(
                    @"File: " +
                    kontoutdragExcelFileInfo.ExcelFileSavePath +
                    @" does not exist.");
                return null;
            }
        }
    }
}