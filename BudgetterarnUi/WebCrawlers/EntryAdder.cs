using Budgeter.Core.Entities;
using LoadTransactionsFromFile;
using System.Collections;

namespace Budgetterarn.WebCrawlers
{
    public class EntryAdder
    {
        private readonly KontoEntriesHolder kontoEntriesHolder;

        private SortedList newBatchOfKontoEntriesAlreadyRed;

        public EntryAdder(KontoEntriesHolder kontoEntriesHolder)
        {
            this.kontoEntriesHolder = kontoEntriesHolder;
        }

        public void SetKontoEntriesToNewList(List<KontoEntry> kontoEntriesFromHtml)
        {
            newBatchOfKontoEntriesAlreadyRed = GetNewBatchOfKontoEntriesAlreadyRed();

            kontoEntriesFromHtml.ForEach(AddNewEntryFromStringArray);
        }

        public void AddNewEntryFromStringArray(KontoEntry entryNewFromHtml)
        {
            var key = entryNewFromHtml.KeyForThis;

            if (NoListContainsKey(key))
            {
                if (key != null)
                {
                    kontoEntriesHolder.NewKontoEntries.Add(key, entryNewFromHtml);
                }
            }
            else if (NoPreviousEntriesContainsKey(key))
            {
                HandlePotientialDouble(entryNewFromHtml);
            }
        }

        private bool NoPreviousEntriesContainsKey(string key)
        {
            return !newBatchOfKontoEntriesAlreadyRed.ContainsKey(key);
        }

        private void HandlePotientialDouble(KontoEntry entryNewFromHtml)
        {
            var newKontoEntries = kontoEntriesHolder.NewKontoEntries;

            // Om man hade entryn i Excel, innan laddning, och innan man gick igenom nya,
            // så kan man (förutsätter att man då det inte finns saldo (i allkort-kredit),
            // så läses hela listan in i ett svep, det är inte en lista, det kan ev. bli
            // dubblet om två datum hamnar på olika allkort-kredit-fakturor)
            // TODO: Ev. bryt ut denna och lägg i en lista som gås igenom senare.
            // Eller skicka in hanterare/delegat som låter användare välja
            var userDecision = MessageBox.Show(
                @"Found potential double: " + entryNewFromHtml.KeyForThis,
                @"Double, SaveThisEntry?",
                MessageBoxButtons.YesNo);

            if (!userDecision.Equals(DialogResult.Yes)) return;

            // Detta är en dubblett, men om det finns fler än 2 dubbletter så måste man
            // se till att nyckeln är unik
            while (newKontoEntries.ContainsKey(entryNewFromHtml.KeyForThis))
            {
                // Stega upp saldo, tills en unik nyckel skapats
                entryNewFromHtml.SaldoOrginal += entryNewFromHtml.KostnadEllerInkomst != 0
                    ? entryNewFromHtml.KostnadEllerInkomst
                    : 1;
            }

            newKontoEntries.Add(entryNewFromHtml.KeyForThis, entryNewFromHtml);

            // För annat än Allkortskredit, så ordnar Detta sig, så länge saldot är med
            // i nyckeln, det är den, så det gäller bara att ha rätt saldo i xls

            // Om man tagit utt t.ex. 100kr 2 ggr samma dag, från samma bankomat.
            // hm, sätt 1 etta efteråt, men det göller ju bara det som är såna, hm,
            // får ta dem manuellt (se även HanteraEvOviktigDubblett(...))
        }

        private bool NoListContainsKey(string key)
        {
            // Kollas även senare
            return !kontoEntriesHolder.KontoEntries.ContainsKey(key)
                   && !kontoEntriesHolder.NewKontoEntries.ContainsKey(key);
        }

        /// <summary>
        /// Spara en batch, dyker det upp dubletter i samma, så ska de ses som unika
        /// </summary>
        /// <returns></returns>
        public SortedList GetNewBatchOfKontoEntriesAlreadyRed()
        {
            var newBatchOfKontoEntriesAlreadyRed = new SortedList();
            foreach (DictionaryEntry entry in kontoEntriesHolder.NewKontoEntries)
            {
                if (!newBatchOfKontoEntriesAlreadyRed.ContainsKey(entry.Key))
                {
                    newBatchOfKontoEntriesAlreadyRed.Add(entry.Key, entry.Value);
                }
            }

            foreach (DictionaryEntry entry in kontoEntriesHolder.KontoEntries)
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