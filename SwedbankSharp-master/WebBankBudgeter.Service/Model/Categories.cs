namespace WebBankBudgeter.Service.Model
{
    public class Categories
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }
        public string Amount { get; set; }
        public override string ToString()
        {
            return $"{Group} {Name}";
        }
    }
}