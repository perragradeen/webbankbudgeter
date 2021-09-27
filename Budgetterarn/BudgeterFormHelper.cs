using Budgeter.Core.Entities;
using LoadTransactionsFromFile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
