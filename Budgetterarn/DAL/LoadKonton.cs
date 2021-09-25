using System;
using System.Collections;
using System.Collections.Generic;
using Budgeter.Core.BudgeterConstants;
using Budgeter.Core.Entities;
using Budgetterarn.WebCrawlers;
using LoadTransactionsFromFile;
using LoadTransactionsFromFile.DAL;

namespace Budgetterarn.DAL
{
    internal class LoadKonton
    {
        /// <summary>
        /// Sparar till Excel-fil
        /// </summary>
        public static LoadOrSaveResult GetAllEntriesFromExcelFile(
            SortedList saveToTable,
            SaldoHolder saldoHolder,
            Hashtable entriesLoadedFromDataStore)
        {
            // Görs i Ui-handling, UpdateEntriesToSaveMemList();
            // Skapa kontoentries
            // För att se om det laddats något, så UI-uppdateras etc. Så returneras bool om det...
            return LoadKontonDal.SkapaKontoEntries(saveToTable, entriesLoadedFromDataStore, saldoHolder);
        }

        public static Hashtable LoadEntriesFromFile(
            KontoutdragInfoForLoad kontoutdragInfoForLoad)
        {
            return LoadEntriesFromFileHandler.LoadEntriesFromFile(kontoutdragInfoForLoad);
        }

        internal static bool GetAllVisibleEntriesFromWebBrowser(
            KontoEntriesHolder kontoEntriesHolder,
            string text)
        {
            var noNewKontoEntriesBeforeLoading =
                kontoEntriesHolder.NewKontoEntries.Count;

            // Kolla browser efter entries.
            if (text != null
                && ProgramSettings.BankType == BankType.Swedbank)
            {
                // TODO: läs saldon Get saldo
                //GetSwedbankSaldo(webBrowser1.Document.Body, kontoEntriesHolder.SaldoHolder);

                // Get Entries
                GetHtmlEntriesFromSwedBankv2(
                    text,
                    kontoEntriesHolder.KontoEntries,
                    kontoEntriesHolder.NewKontoEntries);
            }

            // Returnera aom något ändrats. Är de nya inte samma som innan laddning, så är det sant att något ändrats.
            return kontoEntriesHolder.NewKontoEntries.Count != noNewKontoEntriesBeforeLoading;
        }

        private static void GetHtmlEntriesFromSwedBankv2(
            string text, SortedList kontoEntries, SortedList newKontoEntries)
        {
            // Spara en batch, dyker det upp dubletter i samma, så ska de ses som unika
            var newBatchOfKontoEntriesAlreadyRed = EntryAdder.GetNewBatchOfKontoEntriesAlreadyRed(kontoEntries, newKontoEntries);

            var entryBlob = text.Substring(text.IndexOf("\nTransaktioner\nTransaktionsdatum\nBokföringsdatum\nBelopp\nSaldo\n", StringComparison.Ordinal) +
                                           "\nTransaktioner\nTransaktionsdatum\nBokföringsdatum\nBelopp\nSaldo\n".Length);
            var entries = entryBlob.Split('\n');
            var currentColumnCount = 0;
            var rows = new List<List<string>>();
            var currentEntriesColumns = new List<string>();
            foreach (var textPart in entries)
            {
                currentColumnCount++;
                currentEntriesColumns.Add(textPart);

                if (currentColumnCount <= 4) continue;
                currentColumnCount = 0;
                rows.Add(new List<string>(currentEntriesColumns));
                currentEntriesColumns = new List<string>();

            }

            foreach (var htmlElement in rows)
            {
                // Lägg till ny
                EntryAdder.AddNewEntryFromStringArray(
                    GetSwedBankTableRowv2(htmlElement),
                    kontoEntries,
                    newKontoEntries,
                    newBatchOfKontoEntriesAlreadyRed);
            }
        }

        /// <summary>
        /// TRADERA
        /// 2021-08-06
        /// 2021-08-06
        /// -160,00
        /// 194 122,84
        /// </summary>
        /// <param name="htmlElement"></param>
        /// <returns></returns>
        private static BankRow GetSwedBankTableRowv2(IReadOnlyList<string> htmlElement)
        {
            const int eventColNum = 1;
            const int dateColNum = 2;
            const int beloppColNum = 4;
            const int saldoColNum = 5;

            var entry = new BankRow
            {
                DateValue = htmlElement[dateColNum - 1] ?? string.Empty,
                EventValue = htmlElement[eventColNum - 1] ?? string.Empty,
                BeloppValue = htmlElement[beloppColNum - 1] ?? string.Empty,
                SaldoValue = htmlElement[saldoColNum - 1] ?? string.Empty
            };

            return entry;
        }
    }
}