using System.Collections.Generic;
using System.Linq;
using WebBankBudgeter.Service.Model;
using WebBankBudgeter.Service.Services;

namespace WebBankBudgeter.Service.MonthAvarages
{
    public class MonthAvaragesCalcs
    {
        private readonly TransactionList _transactionListdata;

        public static readonly string[] RecurringCatGroups = {
            "ID_ACCOMMODATION",
            "ID_HOUSEHOLD",
            "ID_OTHER",
            "ID_TRANSPORT"
        };

        public static readonly IEnumerable<string> IncomesCatGroup = new[] { "ID_INCOME" };

        public MonthAvaragesCalcs(TransactionList transactionListdata)
        {
            _transactionListdata = transactionListdata;
        }

        public MonthAvarages GetMonthAvarages()
        {
            var gropuedTrans = TableGetter.GroupOnMonthAndCategory(_transactionListdata.Transactions);

            //transform to fit ui
            return GetMonthAvaragesFromGroupTransactions(gropuedTrans);
        }

        private MonthAvarages GetMonthAvaragesFromGroupTransactions(IEnumerable<IGrouping<TransGrouping, Transaction>> gropuedTrans)
        {
            var avarages = new MonthAvarages();
            var avaragesReccuringCosts = new List<double>();
            var avaragesIncomes = new List<double>();

            var rows = TableGetter.GetRowsFromGroupedRecords(gropuedTrans);
            foreach (var row in rows)
            {
                var averageValue = row.AmountsForMonth.Values.ToList().Average(d => d);

                var catGroupIsIncome = IncomesCatGroup.Any(c => row.CategoryText.Contains(c));
                var catGroupIsReccuring = RecurringCatGroups.Any(c => row.CategoryText.Contains(c));
                if (catGroupIsReccuring)
                {
                    avaragesReccuringCosts.Add(averageValue);
                }
                else if (catGroupIsIncome)
                {
                    avaragesIncomes.Add(averageValue);
                }
            }

            avarages.ReccuringCosts = avaragesReccuringCosts.Sum(d => d);
            avarages.Incomes = avaragesIncomes.Sum(d => d);

            avarages.IncomeDiffCosts = avarages.Incomes + avarages.ReccuringCosts;

            return avarages;
        }
    }
}