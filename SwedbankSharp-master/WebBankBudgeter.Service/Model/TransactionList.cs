using System.Collections.Generic;

namespace WebBankBudgeter.Service.Model
{
    public class TransactionList
    {
        public List<Transaction> Transactions { get; set; }
        public Account Account { get; set; }
    }
}
