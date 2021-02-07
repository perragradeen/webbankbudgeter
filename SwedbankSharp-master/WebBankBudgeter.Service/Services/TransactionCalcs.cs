using System.Collections.Generic;
using System.Linq;
using WebBankBudgeter.Service.Model;

namespace WebBankBudgeter.Service.Services
{
    public class TransactionCalcs
    {
        public TransactionList TransactionList { get; }

        public TransactionCalcs(TransactionList transactionListdata)
        {
            TransactionList = transactionListdata;
        }

        public static double SumAllAmounts(IEnumerable<Transaction> incomes)
        {
            return incomes.Sum(t => t.AmountAsDouble);
        }

        public IEnumerable<Transaction> FilterToCategory(IEnumerable<string> groupNames)
        {
            var amounts = new List<Transaction>();
            var names = groupNames as IList<string> ?? groupNames.ToList();

            foreach (var transaction in TransactionList.Transactions)
            {
                if (transaction?.Categorizations == null) continue;

                amounts.AddRange(
                    from category in transaction.Categorizations.Categories
                    where names.Any(name => name == category.Group)
                    select transaction);
            }

            return amounts;
        }

        public List<Transaction> SortTrans()
        {
            return TransactionList.Transactions =
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
