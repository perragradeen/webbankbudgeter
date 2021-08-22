namespace WebBankBudgeter.Service.Model
{
    public class TransGrouping
    {
        public int Year { get; set; }
        public int Month { get; set; }

        public string Category { get; set; }

        public override string ToString()
        {
            return $"{Year}-{Month}_{Category}";
        }
    }
}