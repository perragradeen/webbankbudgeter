namespace WebBankBudgeter.Service.Model.ViewModel
{
    public class TextToTableOutPuter
    {
        public const string CategoryNameColumnDescription = "Category . Month->";
        public const string AverageColumnDescription = "Average";
        public const string AverageColumnDescriptionNotFormatted = "Average-nf";

        public TextToTableOutPuter()
        {
            ColumnHeaders = new List<string>();
        }

        public string UtgiftersStartYear { get; set; }

        public List<string> ColumnHeaders { get; }

        public IEnumerable<BudgetRow> BudgetRows { get; set; }
        public List<BudgetRow> AveragesForTransactions { get; set; }

        public double GetAverageForCategory(string categoryName)
        {
            var trans = AveragesForTransactions
                .FirstOrDefault(a =>
                    a.CategoryText.ToLower().Trim() ==
                    categoryName.ToLower().Trim());

            if (trans == null)
            {
                return 0;
            }

            return trans
                .AmountsForMonth
                .Values
                .FirstOrDefault();
        }
    }
}