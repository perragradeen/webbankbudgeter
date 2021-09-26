using System.Linq;
using Budgeter.Core;
using Budgeter.Core.Entities;
using CategoryHandler;

// ReSharper disable CommentTypo

namespace Budgetterarn
{
    public static class KontoEntriesChecker
    {
        public static bool OkToAddFromOld { get; set; }

        public static void CheckAndAddNewItemsForLists(
            KontoEntriesViewModelListUpdater lists)
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

                // Lägg till i org
                lists.NewItemsListOrg?.Add(entryNew);

                // Kolla om det är en dubblet eller om det är finns ett
                // motsvarade "skyddat belopp"
                if (lists.KontoEntries.ContainsKey(entryNew.KeyForThis)
                    && !OkToAddFromOld)
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

            OkToAddFromOld = false;
        }

        private static bool EntryMatchesKeyForEntryInList(KontoEntriesViewModelListUpdater lists, KontoEntry entryNew)
        {
            return lists.NewItemsListEdited.Any(
                viewItem => viewItem.KeyEqauls(entryNew));
        }
    }
}
