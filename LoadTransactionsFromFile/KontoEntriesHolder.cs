using System.Collections;

namespace LoadTransactionsFromFile
{
    public class KontoEntriesHolder : KontoEntriesHolderForLoad
    {
        public KontoEntriesHolder()
        {
            NewKontoEntries = new SortedList();
        }

        public SortedList NewKontoEntries { get; set; }
    }
}