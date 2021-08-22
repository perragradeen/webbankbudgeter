using Budgeter.Core.BudgeterConstants;
using Budgeter.Core.Entities;
using RefLesses;
using System;
using System.Collections;
using Utilities;

namespace LoadTransactionsFromFile.DAL
{
    public static class LoadKontonDal
    {
        /// <summary>
        /// Sparar till Excel-fil
        /// </summary>
        public static LoadOrSaveResult TransFormEntriesFromExcelFileToTable(
            KontoutdragInfoForLoad kontoutdragInfoForLoad,
            SortedList saveToTable,
            SaldoHolder saldoHolder,
            Hashtable entriesLoadedFromDataStore)
        {
            // Töm alla tidigare entries i minnet om det ska laddas helt ny fil el. likn. 
            if (kontoutdragInfoForLoad.ClearContentBeforeReadingNewFile)
            {
                saveToTable.Clear();
            }

            // Görs i Ui-handling, UpdateEntriesToSaveMemList();
            // Skapa kontoentries
            // För att se om det laddats något, så UI-uppdateras etc. Så returneras bool om det...
            return SkapaKontoEntries(saveToTable, entriesLoadedFromDataStore, saldoHolder);
        }

        public static LoadOrSaveResult SkapaKontoEntries(
            SortedList saveToTable,
            Hashtable entriesLoadedFromDataStore,
            SaldoHolder saldoHolder)
        {
            var loadResult = new LoadOrSaveResult();

            foreach (DictionaryEntry item in entriesLoadedFromDataStore)
            {
                if (item.Value != null)
                {
                    var entryArray = ((ExcelRowEntry)item.Value).Args;

                    // Om det är tomt
                    if (entryArray == null)
                    {
                        continue;
                    }

                    // Om det är kolumnbeskrivning, skippa...
                    if ((string)entryArray[0] == "y")
                    {
                        var saldoColumnNumber = 11;

                        foreach (var saldoName in BankConstants.SwedbankSaldonames)
                        {
                            var saldot = entryArray.Length > saldoColumnNumber + 1
                                ? entryArray[saldoColumnNumber + 1] ?? string.Empty
                                : string.Empty; // Todo: byt empty mot värden i saldon

                            saldoHolder.AddToOrChangeValueInDictionaryForKey(saldoName,
                                saldot.ToString().GetDoubleValueFromStringEntry());

                            saldoColumnNumber++;
                        }

                        // Hoppa över
                        continue;
                    }

                    var newKe = new KontoEntry(entryArray, true);
                    var key = newKe.KeyForThis;

                    // Lägg till orginalraden, gör i UI-hanterare
                    if (!saveToTable.ContainsKey(key))
                    {
                        saveToTable.Add(key, newKe);

                        loadResult.SomethingLoadedOrSaved = true;
                    }
                    else
                    {
                        // Detta ordnar sig, så länge saldot är med i nyckeln, det är den, så det gäller bara att ha rätt saldo i xls //Om man tagit utt t.ex. 100kr 2 ggr samma dag, från samma bankomat. hm, sätt 1 etta efteråt, men det göller ju bara det som är såna, hm, får ta dem manuellt

                        // skulle kunna tillåta någon inläsning här ev. 
                        // om man kan förutsätta att xls:en är kollad, 
                        // det får bli här man lägger till specialdubbletter manuellt
                        Console.WriteLine("Entry Double found. Key = " + key);

                        // meddela detta till usern, man ser de på skipped...
                        loadResult.SkippedOrSaved++;
                    }
                }
            }

            return loadResult;
        }
    }
}