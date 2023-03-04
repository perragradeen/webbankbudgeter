namespace BudgeterCore.Entities
{
    public class InBudget
    {
        public string CategoryDescription { get; set; }
        public double BudgetValue { get; set; }
        public DateTime YearAndMonth { get; set; }

        public override string ToString()
        {
            return YearAndMonth.ToShortDateString() + " " + CategoryDescription + " " + BudgetValue;
        }
    }
}