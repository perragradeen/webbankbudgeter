using Budgeter.Core.Entities;
using System.Collections;

namespace LoadTransactionsFromFile
{
    public class KontoEntriesHolderForLoad
    {
        public KontoEntriesHolderForLoad()
        {
            KontoEntries = new SortedList(new DescendingComparer());
            SaldoHolder = new SaldoHolder();
        }

        public SortedList KontoEntries { get; }

        public SaldoHolder SaldoHolder { get; }
    }

    public class KontoEntriesHolder : KontoEntriesHolderForLoad
    {
        public KontoEntriesHolder() : base()
        {
            NewKontoEntries = new SortedList();
        }

        public SortedList NewKontoEntries { get; set; }
    }
}