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
        /// Görs i Ui-handling, UpdateEntriesToSaveMemList();
        /// Skapa kontoentries
        /// För att se om det laddats något, så UI-uppdateras etc.
        /// Så returneras bool om det...
        /// </summary>
        public static LoadOrSaveResult TransFormEntriesFromExcelFileToTable(
            KontoEntriesHolder kontoEntriesHolder,
            Hashtable entriesLoadedFromDataStore)
        {
            var saveToTable = kontoEntriesHolder.KontoEntries;
            var loadResult = new LoadOrSaveResult();

            foreach (DictionaryEntry item in entriesLoadedFromDataStore)
            {
                if (item.Value == null) continue;

                var entryArray = ((ExcelRowEntry)item.Value).Args;
                if (entryArray == null) continue; // Om det är tomt

                if (DetÄrInteKolumnbeskrivning(entryArray))
                {
                    SparaNyKontoRad(saveToTable, loadResult, entryArray);
                }
                else
                {
                    UpdateraSaldo(kontoEntriesHolder.SaldoHolder, entryArray);
                }
            }

            return loadResult;
        }

        private static bool DetÄrInteKolumnbeskrivning(object[] entryArray)
        {
            return (string)entryArray[0] != "y";
        }

        private static void SparaNyKontoRad(
            SortedList saveToTable,
            LoadOrSaveResult loadResult,
            object[] entryArray)
        {
            var entryNew = new KontoEntry(entryArray, true);
            var key = entryNew.KeyForThis;

            // Lägg till orginalraden, gör i UI-hanterare
            if (!saveToTable.ContainsKey(key))
            {
                saveToTable.Add(key, entryNew);

                loadResult.SomethingLoadedOrSaved = true;
            }
            else
            {
                HanteraEvOviktigDubblett(loadResult, key);
            }
        }

        private static void HanteraEvOviktigDubblett(
            LoadOrSaveResult loadResult,
            string key)
        {
            // Detta ordnar sig, så länge saldot är med i nyckeln, det är den,
            // så det gäller bara att ha rätt saldo i xls
            // Om man tagit utt t.ex. 100kr 2 ggr samma dag, från samma bankomat.
            // hm, sätt 1 etta efteråt, men det göller ju bara det som är såna,
            // hm, får ta dem manuellt

            // skulle kunna tillåta någon inläsning här ev. 
            // om man kan förutsätta att xls:en är kollad, 
            // det får bli här man lägger till specialdubbletter manuellt
            Console.WriteLine("Entry Double found. Key = " + key);

            // meddela detta till usern, man ser de på skipped...
            loadResult.SkippedOrSaved++;
        }

        private static void UpdateraSaldo(
            SaldoHolder saldoHolder,
            object[] entryArray)
        {
            var saldoColumnNumber = 11;

            foreach (var saldoName in BankConstants.SwedbankSaldonames)
            {
                var saldot = entryArray.Length > saldoColumnNumber + 1
                    ? entryArray[saldoColumnNumber + 1] ?? string.Empty
                    : string.Empty; // Todo: byt empty mot värden i saldon

                var saldoValue = saldot.ToString().GetDoubleValueFromStringEntry();
                saldoHolder.AddToOrChangeValueInDictionaryForKey(
                         saldoName,
                         saldoValue);

                saldoColumnNumber++;
            }
        }
    }
}