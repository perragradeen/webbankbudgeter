using System.Collections.Generic;
using System.Linq;
using WebBankBudgeter.Service.Model;

namespace WebBankBudgeter.Service.Services
{
    public class TransactionCalcs
    {
        public TransactionList TransactionList { get; }

        public TransactionCalcs(TransactionList transactionDatalist)
        {
            TransactionList = transactionDatalist;
        }

        public void SortTrans()
        {
            TransactionList.Transactions =
                TransactionList.Transactions.OrderBy(t => t.DateAsDate).ToList();
        }

        public void RemoveDuplicates()
        {
            var newDataUnique = new Dictionary<string, Transaction>();
            foreach (var item in TransactionList.Transactions)
            {
                var id = item?.Id;

                if (id == null
                    || item.ExpenseControlIncludedAsEnum
                        == ExpenseControlIncludedAlternatives.OUTDATED)
                {
                    continue;
                }

                if (!newDataUnique.ContainsKey(id))
                {
                    newDataUnique.Add(id, item);
                }
            }

            TransactionList.Transactions = newDataUnique.Values.ToList();
        }
    }
}
