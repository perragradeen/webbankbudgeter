using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Budgeter.Core.Entities;
using Budgetterarn.EntryLogicSetFlags;
using LoadTransactionsFromFile;

namespace Budgetterarn
{
    public class BudgeterFormHelper
    {
        private readonly Action<string> writeToOutput;
        private readonly KontoEntriesHolder kontoEntriesHolder;

        public BudgeterFormHelper(
            Action<string> writeToOutput
            , Action<string> WriteToUiStatusLog
            , KontoEntriesHolder kontoEntriesHolder
        )
        {
            this.writeToOutput = writeToOutput;
            this.kontoEntriesHolder = kontoEntriesHolder;
        }

        internal void LoadOldEntries()
        {
            // Sätt de gamla inlästa transaktionerna i minnet in i nya lista för redigering av kategori
            kontoEntriesHolder.NewKontoEntries = GetOldEntriesWithoutCategory();

            //CheckAndAddNewItems(true); // Lägg till gamla i GuiLista för redigering

            //somethingChanged = kontoEntriesHolder.NewKontoEntries.Count > 0;
        }

        // TODO: Flytta
        private SortedList GetOldEntriesWithoutCategory()
        {
            var size = kontoEntriesHolder.KontoEntries.Count;
            KontoEntry[] tempOldEntries = new KontoEntry[size];
            kontoEntriesHolder.KontoEntries.Values.CopyTo(tempOldEntries, 0);
            var filteredOldEntries = tempOldEntries
                .Where(el => string.IsNullOrEmpty(el.TypAvKostnad));
            var dict = filteredOldEntries.ToDictionary(ell => ell.KeyForThis);
            var sortedList = new SortedList(dict);
            return sortedList;
        }

        /// <summary>Uppdatera UI för nya entries, gör gisningar av dubbletter, typ av kostnad etc
        /// </summary>
        internal void CheckAndAddNewItems(
            KontoEntriesChecker kontoEntriesChecker,
            List<KontoEntry> itemsAsKontoEntries)
        {
            // Flagga och se vad som är nytt etc.
            kontoEntriesChecker.CheckAndAddNewItemsForLists();

            //// Lägg till i org
            //lists.NewItemsListOrg.ForEach(k =>
            //    ViewUpdateUi.AddToListview(newIitemsListOrgGrid, k));

            // Filtrera ut de som inte redan ligger i UI
            var inUiListAlready = itemsAsKontoEntries;
            kontoEntriesChecker.AddInUiListAlreadyToAddList(inUiListAlready);

            kontoEntriesChecker.CheckSkyddatBelopp(kontoEntriesHolder);
        }
    }
}