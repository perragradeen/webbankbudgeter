using Budgeter.Core;
using Budgeter.Core.Entities;
using CategoryHandler;
using LoadTransactionsFromFile;

// ReSharper disable CommentTypo

namespace Budgetterarn.EntryLogicSetFlags
{
    public class KontoEntriesChecker
    {
        private readonly KontoEntriesViewModelListUpdater lists;
        private bool okToAddFromOld;

        public KontoEntriesChecker(
            KontoEntriesViewModelListUpdater lists,
            bool okToAddFromOld = false)
        {
            this.lists = lists;
            this.okToAddFromOld = okToAddFromOld;
        }

        public void CheckAndAddNewItemsForLists()
        {
            if (lists.NewKontoEntriesIn.Count <= 0) return;

            foreach (var item in lists.NewKontoEntriesIn.Values)
            {
                if (!(item is KontoEntry))
                {
                    continue;
                }

                var entryNew = item as KontoEntry;

                var foundDoubleInUList =
                    lists.NewItemsListEdited.CheckIfKeyExistsInKontoEntries(
                        entryNew.KeyForThis)
                    || EntryMatchesKeyForEntryInList(lists, entryNew);

                // Om man laddar html-entries 2 gånger i rad, så ska det
                // inte skapas dubletter
                if (foundDoubleInUList)
                {
                    continue;
                }

                //// Lägg till i org
                //lists.NewItemsListOrg?.Add(entryNew);

                // Kolla om det är en dubblet eller om det är finns ett
                // motsvarade "skyddat belopp"
                if (lists.KontoEntries.ContainsKey(entryNew.KeyForThis)
                    && !okToAddFromOld)
                {
                    continue;
                }

                // Slå upp autokategori
                var lookedUpCat = CategoriesHolder.AutocategorizeType(entryNew.Info);
                if (lookedUpCat != null)
                {
                    entryNew.TypAvKostnad = lookedUpCat;
                }

                // Lägg till i edited
                lists.NewItemsListEdited.Add(entryNew);
            }

            okToAddFromOld = false;
        }

        private static bool EntryMatchesKeyForEntryInList(KontoEntriesViewModelListUpdater lists, KontoEntry entryNew)
        {
            return lists.NewItemsListEdited.Any(
                viewItem => viewItem.KeyEqauls(entryNew));
        }

        public void AddInUiListAlreadyToAddList(
            List<KontoEntry> inUiListAlready)
        {
            foreach (var entry in lists.NewItemsListEdited)
            {
                // TODO: Kolla prestanda?
                if (inUiListAlready.All(e => e.KeyForThis != entry.KeyForThis))
                {
                    lists.ToAddToListview.Add(entry);
                }
            }
        }

        public void CheckSkyddatBelopp(KontoEntriesHolder kontoEntriesHolder)
        {
            foreach (var entry in lists.ToAddToListview)
            {
                // kolla om det är "Skyddat belopp", och se om det finns några
                // gamla som matchar.
                SkyddatBeloppChecker.CheckForSkyddatBeloppMatcherAndGuessDouble(
                    entry,
                    kontoEntriesHolder.KontoEntries);
            }
        }
    }
}