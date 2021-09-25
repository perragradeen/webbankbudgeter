using Budgeter.Core.Entities;
using System.Collections;

namespace LoadTransactionsFromFile
{
    public class KontoEntriesHolder
    {
        public KontoEntriesHolder()
        {
            KontoEntries = new SortedList(new DescendingComparer());

            NewKontoEntries = new SortedList();
            SaldoHolder = new SaldoHolder();
        }

        /// <summary>
        /// saveToTable
        /// </summary>
        public SortedList KontoEntries { get; }

        public SortedList NewKontoEntries { get; set; }
        public SaldoHolder SaldoHolder { get; }
    }
}