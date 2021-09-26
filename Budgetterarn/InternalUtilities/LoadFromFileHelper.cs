using Budgeter.Core.Entities;
using LoadTransactionsFromFile;
using LoadTransactionsFromFile.DAL;
using System;
using System.Collections;

namespace Budgetterarn.InternalUtilities
{
    public class LoadFromFileHelper
    {
        private readonly ExcelFileKontoutdragInfoForLoad excelFileKontoutdragInfoForLoad;
        private readonly KontoEntriesHolder kontoEntriesHolder;
        private readonly Action<string> writeToOutput;
        private readonly Action<string> writeToUiStatusLog;

        public LoadFromFileHelper(
            ExcelFileKontoutdragInfoForLoad kontoutdragInfoForLoad,
            KontoEntriesHolder kontoEntriesHolder,
            Action<string> writeToOutput,
            Action<string> writeToUiStatusLog)
        {
            this.excelFileKontoutdragInfoForLoad = kontoutdragInfoForLoad;
            this.kontoEntriesHolder = kontoEntriesHolder;
            this.writeToOutput = writeToOutput;
            this.writeToUiStatusLog = writeToUiStatusLog;
        }

        internal void SetEntriesFromFile(
            bool clearContentBeforeReadingNewFile)
        {
            // Ladda från fil
            var entriesLoadedFromDataStore = GetEntriesFromFile();
            if (entriesLoadedFromDataStore == null) return;

            VisaAnvändarenAttIngetLaddatsÄn(
                excelFileKontoutdragInfoForLoad,
                entriesLoadedFromDataStore);

            if (clearContentBeforeReadingNewFile)
                ClearUiContents();

            var loadResult =
                EntriesFromExcelTransFormer.TransformFromExcelFileToList(
                    kontoEntriesHolder,
                    entriesLoadedFromDataStore);

            VisaFörAnvändarenHurDetGick(
                excelFileKontoutdragInfoForLoad,
                loadResult);
        }

        private void ClearUiContents()
        {
            // Töm alla tidigare entries i minnet om det ska laddas
            // helt ny fil el. likn. 
            kontoEntriesHolder.KontoEntries.Clear();
        }

        private void VisaAnvändarenAttIngetLaddatsÄn(
            ExcelFileKontoutdragInfoForLoad kontoutdragInfoForLoad,
            Hashtable entriesLoadedFromDataStore)
        {
            var statusText = @"Nothing loaded.";
            writeToUiStatusLog(statusText);

            if (entriesLoadedFromDataStore == null)
            {
                statusText += kontoutdragInfoForLoad.ExcelFileSavePath;
            }

            writeToUiStatusLog(statusText);
        }

        private void VisaFörAnvändarenHurDetGick(
            ExcelFileKontoutdragInfoForLoad kontoutdragInfoForLoad,
            LoadOrSaveResult loadResult)
        {
            string statusText = "No. rows loaded; "
                         + kontoEntriesHolder.KontoEntries.Count
                         + " . Skpped: "
                         + loadResult.SkippedOrSaved
                         + ". File loaded; "
                         + kontoutdragInfoForLoad.ExcelFileSavePath;
            writeToUiStatusLog(statusText);
        }

        private Hashtable GetEntriesFromFile()
        {
            try
            {
                return LoadEntriesFromFileHandler
                    .LoadEntriesFromFile(excelFileKontoutdragInfoForLoad);
            }
            catch (Exception)
            {
                writeToOutput(
                    @"File: " +
                    excelFileKontoutdragInfoForLoad.ExcelFileSavePath +
                    @" does not exist.");
                return null;
            }
        }
    }
}