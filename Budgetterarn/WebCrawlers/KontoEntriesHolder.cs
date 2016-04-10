using System.Collections;
using System.Windows.Forms;
using Budgeter.Core.Entities;
using Budgetterarn.Model;

namespace Budgetterarn.WebCrawlers
{
    public class KontoEntriesHolder
    {
        public KontoEntriesHolder()
        {
            KontoEntries = new SortedList(new DescendingComparer());

            NewKontoEntries = new SortedList();
            SaldoHolder = new SaldoHolder();
        }

        public HtmlDocument Doc { get; set; }
        public SortedList KontoEntries { get; set; }
        public SortedList NewKontoEntries { get; set; }
        public bool SomethingChanged { get; set; }
        public SaldoHolder SaldoHolder { get; set; }
    }
}