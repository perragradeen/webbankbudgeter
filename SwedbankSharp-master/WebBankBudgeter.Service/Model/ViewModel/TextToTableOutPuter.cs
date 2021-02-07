using System.Collections.Generic;

namespace WebBankBudgeter.Service.Model.ViewModel
{
    public class TextToTableOutPuter
    {
        public const string CategoryNameColumnDescription = "Category . Month->";
        public const string AverageColumnDescription = "Average";

        public TextToTableOutPuter()
        {
            ColumnHeaders = new List<string>();
        }

        public string SelectedStartYear { get; set; }

        public List<string> ColumnHeaders { get; set; }

        public IEnumerable<BudgetRow> BudgetRows { get; set; }

        //Rows = new List<List<object>>();
        //public List<List<object>> Rows { get; set; }
        //public List<List<string>> RowsIncludingHeaders
        //{
        //    get
        //    {
        //        var allRows = new List<List<string>>();
        //        allRows.Add(ColumnHeaders);
        //        allRows.AddRange(Rows);

        //        return allRows;
        //    }
        //}
    }
}