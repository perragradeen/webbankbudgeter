using WebBankBudgeterService.Model;
using WebBankBudgeterService.Services;

namespace WebBankBudgeterService.MonthAvarages
{
    public class MonthAvaragesCalcs
    {
        private readonly TransactionList _transactionDatalist;

        public static readonly string[] ReoccurringCatGroups =
        {
            "ID_ACCOMMODATION",
            "ID_HOUSEHOLD",
            "ID_OTHER",
            "ID_TRANSPORT"
        };

        private static readonly IEnumerable<string> IncomesCatGroup = new[] { "ID_INCOME" };

        public MonthAvaragesCalcs(TransactionList transactionDatalist)
        {
            _transactionDatalist = transactionDatalist;
        }

        public MonthAvarages GetMonthAvarages()
        {
            var gropuedTrans = TableGetter.GroupOnMonthAndCategory(_transactionDatalist.Transactions);

            //transform to fit ui
            return GetMonthAveragesFromGroupTransactions(gropuedTrans);
        }

        private static MonthAvarages GetMonthAveragesFromGroupTransactions(
            IEnumerable<IGrouping<TransGrouping, Transaction>> gropuedTrans)
        {
            var averages = new MonthAvarages();
            var averagesReoccurringCosts = new List<double>();
            var averagesIncomes = new List<double>();

            var rows = TableGetter.GetRowsFromGroupedRecords(gropuedTrans);
            foreach (var row in rows)
            {
                var averageValue = row.AmountsForMonth.Values.ToList().Average(d => d);

                var catGroupIsIncome = IncomesCatGroup.Any(row.CategoryText.Contains);
                var catGroupIsReoccurring = ReoccurringCatGroups.Any(row.CategoryText.Contains);
                if (catGroupIsReoccurring)
                {
                    averagesReoccurringCosts.Add(averageValue);
                }
                else if (catGroupIsIncome)
                {
                    averagesIncomes.Add(averageValue);
                }
            }

            averages.ReccuringCosts = averagesReoccurringCosts.Sum(d => d);
            averages.Incomes = averagesIncomes.Sum(d => d);

            averages.IncomeDiffCosts = averages.Incomes + averages.ReccuringCosts;

            return averages;
        }
    }
}