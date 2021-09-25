using System.Collections.Generic;

namespace WebBankBudgeter.Service.Model
{
    public class BudgetRow
    {
        public BudgetRow()
        {
            AmountsForMonth = new Dictionary<string, double>();
        }

        /// <summary>
        /// Ex. "el"
        /// </summary>
        public string CategoryText { get; set; }

        /// <summary>
        /// Ex. "2016 January", -2215,22
        /// </summary>
        public Dictionary<string, double> AmountsForMonth { get; }

        public override string ToString()
        {
            return CategoryText + " " + AmountsForMonth.Count.ToString();
        }
    }
}