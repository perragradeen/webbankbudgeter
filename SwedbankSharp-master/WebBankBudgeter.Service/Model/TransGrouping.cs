namespace WebBankBudgeter.Service.Model
{
    public class TransGrouping
    {
        public int Year { get; set; }
        public int Month { get; set; }

        public string Category { get; set; }

        public double SummedAmount { get; set; }

        public override string ToString()
        {
            return $"{Year}-{Month}_{Category}";
        }

        public override bool Equals(object obj)
        {
            return obj.ToString().Equals(ToString());
        }

        public override int GetHashCode()
        {
            return
                //CategoriesList.GetHasIntForCategory(Category) + 
                Year + Month;
        }
    }
}