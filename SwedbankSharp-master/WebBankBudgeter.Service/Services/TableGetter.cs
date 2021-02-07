using System;
using System.Collections.Generic;
using System.Linq;
using WebBankBudgeter.Service.Model;
using WebBankBudgeter.Service.Model.ViewModel;
using WebBankBudgeter.Service.Services.Helpers;

namespace WebBankBudgeter.Service.Services
{
    public class TableGetter
    {
        public bool AddAverageColumn { get; set; }

        public TextToTableOutPuter GetTextTableFromGroupedTransactions(IEnumerable<IGrouping<TransGrouping, Transaction>> grouped)
        {
            var transactionsGrouped = grouped as IList<IGrouping<TransGrouping, Transaction>> ?? grouped.ToList();

            var table = new TextToTableOutPuter
            {
                SelectedStartYear = transactionsGrouped.FirstOrDefault()?
                    .Key.Year.ToString()
            };

            AddColumnHeaderMonths(table, transactionsGrouped);

            var rows = GetRowsFromGroupedRecords(transactionsGrouped);
            table.BudgetRows = rows;

            return table;
        }

        private void AddColumnHeaderMonths(TextToTableOutPuter table, IEnumerable<IGrouping<TransGrouping, Transaction>> grouped)
        {
            table.ColumnHeaders.Add(TextToTableOutPuter.CategoryNameColumnDescription);

            if (AddAverageColumn)
            {
                table.ColumnHeaders.Add(TextToTableOutPuter.AverageColumnDescription);
            }

            // Add April, May etc
            var months = new Dictionary<string, string>();
            foreach (var g in grouped)
            {
                var first = g.FirstOrDefault();
                if (first == null) continue;

                var monthNameName = first.DateAsYearMothText;

                if (months.ContainsKey(monthNameName))
                    continue;

                table.ColumnHeaders.Add(monthNameName);
                months.Add(monthNameName, monthNameName);
            }
        }

        public static IReadOnlyCollection<BudgetRow> GetRowsFromGroupedRecords(
            IEnumerable<IGrouping<TransGrouping, Transaction>> transactionsGrouped)
        {
            // loop group
            // For each uniqe date
            // Add it to row1
            // For each sum for that date
            // Loop cats (ordered by value)
            // Add it to row 2 for type 2 etc

            var catChartModelRowList =
                new Dictionary<string, BudgetRow>();
            //var amountsForMonth = new Dictionary<string, double>();
            foreach (var dateAndCatTransGroup in transactionsGrouped)
            {
                var rowFactory = new BudgetRowFactory(
                    dateAndCatTransGroup, catChartModelRowList);

                if (rowFactory.RecordOne == null)
                {
                    continue;
                }

                var row = rowFactory.GetOrAddRow();

                rowFactory.AddSummedAmounts(row);
            }

            return catChartModelRowList.Values.ToList()
                .OrderByDescending(row => row.CategoryText).ToList();
        }

        public static string GetMonthAsFullString(TransGrouping key)
        {
            return Transaction.GetMonthAsFullString(new DateTime(key.Year, key.Month, 1));
        }

        public static IEnumerable<IGrouping<TransGrouping, Transaction>> GroupOnMonthAndCategory(List<Transaction> transactions)
        {
            var g = transactions.GroupBy(t =>
                //t.DateAsDate.Month + "_" + t.Categories.Name
                new TransGrouping
                {
                    Year = t.DateAsDate.Year,
                    Month = t.DateAsDate.Month,
                    Category = t.CategoryName
                }
            );

            return g;
        }

        public TextToTableOutPuter GetTextTableFromTransactions(List<Transaction> transactions)
        {
            var grouped = GroupOnMonthAndCategory(transactions);
            return GetTextTableFromGroupedTransactions(grouped);
        }
    }
}