using System.Collections.Generic;

namespace WebBankBudgeter.Service.Model
{
    public class TransactionList
    {
        public List<Transaction> Transactions { get; set; }
        public Account Account { get; set; }
        public int NumberOfTransactions { get; set; }
        public List<ReservedTransaction> ReservedTransactions { get; set; }
        public int NumberOfReservedTransactions { get; set; }
        public bool MoreTransactionsAvailable { get; set; }
    }
}
