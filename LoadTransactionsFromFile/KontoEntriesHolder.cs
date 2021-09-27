using System.Collections;

namespace LoadTransactionsFromFile
{
    public class KontoEntriesHolder : KontoEntriesHolderForLoad
    {
        public KontoEntriesHolder() : base()
        {
            NewKontoEntries = new SortedList();
        }

        public SortedList NewKontoEntries { get; set; }
    }
}