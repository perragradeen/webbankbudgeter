using System.Collections;

namespace Budgeter.Core.Entities
{
    public class KontoutdragInfo
    {
        /// <summary>
        /// Key = description, Value= amount
        /// </summary>
        //private readonly SaldoHolder saldoHolder;

        public KontoutdragInfo()
        {
            KontoEntries = new SortedList(new DescendingComparer());
            //saldoHolder = new SaldoHolder();
        }

        /// <summary>
        /// Även vid sparning (saveToTable)
        /// </summary>
        public SortedList KontoEntries { get; set; }
        public SortedList NewKontoEntries { get; set; }
    }

    // Tagit från nätet: http://www.codeproject.com/KB/cs/Descending_Sorted_List.aspx?fid=1353560&df=90&mpp=25&noise=3&sort=Position&view=Quick&select=2570977#xx2570977xx
}