using System.Collections;
using System.Windows.Forms;
using Budgeter.Core.Entities;

namespace Budgetterarn.WebCrawlers
{
    public static class EntryAdder
    {
        public static void AddNewEntryFromStringArray(
            BankRow entryStrings,
            SortedList kontoEntries,
            SortedList newKontoEntries,
            SortedList newBatchOfKontoEntriesAlreadyRed)
        {
            var newKeyFromHtml = new KontoEntry(entryStrings);
            var key = newKeyFromHtml.KeyForThis;

            if (!kontoEntries.ContainsKey(key) && !newKontoEntries.ContainsKey(key)) // Kollas även senare
            {
                if (key != null)
                {
                    newKontoEntries.Add(key, newKeyFromHtml);
                }

                // Handle Doubles
            }
            else if (!newBatchOfKontoEntriesAlreadyRed.ContainsKey(key))
            {
                // Om man hade entryn i Excel, innan laddning, och innan man gick igenom nya, så kan man (förutsätter att man då det inte finns saldo (i allkort-kredit), så läses hela listan in i ett svep, det är inte en lista, det kan ev. bli dubblet om två datum hamnar på olika allkort-kredit-fakturor)
                var userDecision = MessageBox.Show(
                    @"Found potential double: " + newKeyFromHtml.KeyForThis,
                    @"Double, SaveThisEntry?",
                    MessageBoxButtons.YesNo);

                if (!userDecision.Equals(DialogResult.Yes)) return;
                // Detta är en dubblett, men om det finns fler än 2 dubbletter så måste man se till att nyckeln är unik
                while (newKontoEntries.ContainsKey(newKeyFromHtml.KeyForThis))
                {
                    // Stega upp saldo, tills en unik nyckel skapats
                    newKeyFromHtml.SaldoOrginal += newKeyFromHtml.KostnadEllerInkomst != 0
                        ? newKeyFromHtml.KostnadEllerInkomst
                        : 1;
                }

                newKontoEntries.Add(newKeyFromHtml.KeyForThis, newKeyFromHtml);

                // För annat än Allkortskredit, så ordnar Detta sig, så länge saldot är med i nyckeln, det är den, så det gäller bara att ha rätt saldo i xls //Om man tagit utt t.ex. 100kr 2 ggr samma dag, från samma bankomat. hm, sätt 1 etta efteråt, men det göller ju bara det som är såna, hm, får ta dem manuellt
            }
        }

        public static SortedList GetNewBatchOfKontoEntriesAlreadyRed(
            SortedList kontoEntries, SortedList newKontoEntries)
        {
            var newBatchOfKontoEntriesAlreadyRed = new SortedList();
            foreach (DictionaryEntry entry in newKontoEntries)
            {
                if (!newBatchOfKontoEntriesAlreadyRed.ContainsKey(entry.Key))
                {
                    newBatchOfKontoEntriesAlreadyRed.Add(entry.Key, entry.Value);
                }
            }

            foreach (DictionaryEntry entry in kontoEntries)
            {
                if (!newBatchOfKontoEntriesAlreadyRed.ContainsKey(entry.Key))
                {
                    newBatchOfKontoEntriesAlreadyRed.Add(entry.Key, entry.Value);
                }
            }

            return newBatchOfKontoEntriesAlreadyRed;
        }
    }
}
