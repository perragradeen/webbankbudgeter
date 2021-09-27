using System.Collections;
using Budgeter.Core.Entities;

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
}