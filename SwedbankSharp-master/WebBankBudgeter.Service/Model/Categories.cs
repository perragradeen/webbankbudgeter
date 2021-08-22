namespace WebBankBudgeter.Service.Model
{
    public class Categories
    {
        public string Name { get; set; }
        public string Group { get; set; }

        public override string ToString()
        {
            return $"{Group} {Name}";
        }
    }
}