namespace WebBankBudgeter.Service.Model
{
    public class Account
    {
        public string AvailableAmount { get; set; }
        public string CreditGranted { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public string Currency { get; set; }
        public string AccountNumber { get; set; }
        public string ClearingNumber { get; set; }
        public object Balance { get; set; }
        public string FullyFormattedNumber { get; set; }
    }
}