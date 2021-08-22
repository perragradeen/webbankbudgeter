using System.Collections.Generic;
using System.Linq;
using WebBankBudgeter.Service.Model;

namespace WebBankBudgeter.Service.Services.Helpers
{
    internal class BudgetRowFactory
    {
        private readonly IGrouping<TransGrouping, Transaction> _dateAndCatTransGroup;
        private readonly Dictionary<string, BudgetRow> _catChartModelRowList;

        public BudgetRowFactory(IGrouping<TransGrouping, Transaction> dateAndCatTransGroup, Dictionary<string, BudgetRow> catChartModelRowList)
        {
            _dateAndCatTransGroup = dateAndCatTransGroup;
            _catChartModelRowList = catChartModelRowList;
        }

        public Transaction RecordOne => _dateAndCatTransGroup.FirstOrDefault();

        public BudgetRow GetOrAddRow()
        {
            BudgetRow row;
            // Lägg till
            var categoryText_AsKey = RecordOne.CategoryName;
            if (_catChartModelRowList.ContainsKey(categoryText_AsKey))
            {
                row = _catChartModelRowList[categoryText_AsKey];
            }
            else
            {
                row = new BudgetRow
                {
                    CategoryText = categoryText_AsKey,
                };

                _catChartModelRowList.Add(row.CategoryText, row);
            }

            return row;
        }

        public void AddSummedAmounts(BudgetRow row)
        {
            var summedAmountsExpenses = _dateAndCatTransGroup
                .Sum(r => r.AmountAsDouble);

            // Lägg till summa av Amount
            var monthNameNameAsKey = RecordOne.DateAsYearMothText;
            if (row.AmountsForMonth.ContainsKey(monthNameNameAsKey))
            {
                row.AmountsForMonth[monthNameNameAsKey] += summedAmountsExpenses;
            }
            else
            {
                row.AmountsForMonth.Add(monthNameNameAsKey, summedAmountsExpenses);
            }
        }
    }
}