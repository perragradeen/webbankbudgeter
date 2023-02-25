using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Budgeter.Core.Entities;
using WebBankBudgeter.Service;
using WebBankBudgeter.Service.Model;

namespace InbudgetHandler
{
    public class SkapaInPosterHanterare
    {
        private readonly TransactionHandler _transactionHandler;
        private readonly InBudgetHandler _inBudgetHandler;

        public SkapaInPosterHanterare(InBudgetHandler target, TransactionHandler transactionHandler)
        {
            _inBudgetHandler = target;
            _transactionHandler = transactionHandler;
        }

        public async Task<List<InBudget>> SkapaInPoster(
            DateTime? nuDatum = null,
            TransactionList transactionList = null)
        {
            if (!nuDatum.HasValue)
            {
                nuDatum = DateTime.Today;
            }

            var inPoster = await _inBudgetHandler.GetInPoster();

            var senasteDatum = _inBudgetHandler.HämtaSenasteDatum(inPoster, nuDatum.Value);
            senasteDatum = senasteDatum.AddMonths(1); // Den senaste finns redan så lägg till 1 månad.

            if (transactionList != null)
            {
                _transactionHandler.SetTransactionList(transactionList);
            }
            else
            {
                await _transactionHandler.GetTransactionsAsync();
            }

            // Räkna ut snitt
            // hyra 12k mat 6k etc	Snitt kostnad för alla tider finns ta den
            var averagesForTransactions = GetAvarages(
                _transactionHandler.TransactionList,
                senasteDatum);

            // Skapa en inrad för 1 månad med snitt
            // 2021-09	25 Hyra bla bla	hyra 12k mat 6k etc
            var inBudgetRows = new List<InBudget>();
            foreach (var row in averagesForTransactions)
            {
                var inBudgetRow = new InBudget
                {
                    CategoryDescription = row.CategoryText,
                    BudgetValue = row.AmountsForMonth.Any()
                        ? row.AmountsForMonth.FirstOrDefault().Value
                        : 0,
                    YearAndMonth = senasteDatum
                };
                inBudgetRows.Add(inBudgetRow);
            }

            // Fyll på med de kategorier som inte var med i utgifter
            // 25 Hyra bla bla	Hämta alla kategorier			
            var kategorier = _transactionHandler.AllCategories;
            foreach (var categoryName in kategorier.CategoryList)
            {
                var missingCategory = inBudgetRows
                    .FirstOrDefault(a =>
                        a.CategoryDescription != categoryName.Description);

                if (missingCategory != null)
                {
                    var inBudgetRow = new InBudget
                    {
                        CategoryDescription = categoryName.Description,
                        BudgetValue = 0,
                        YearAndMonth = senasteDatum
                    };
                    inBudgetRows.Add(inBudgetRow);
                }
            }

            return inBudgetRows;
        }

        public static List<BudgetRow> GetAvarages(
            TransactionList TransactionList,
            DateTime dateTime)
        {
            // Måste gruppera på år+mån+kat
            // Sen fylla i alla tomma måndader med 0
            // Sen räkna ut snittet på alla månader inkl. de med 0
            var transactions = TransactionList.Transactions
                .GroupBy(g => g.CategoryNameNoGroup);

            // Hämta högsta o lägsta datum
            var högstDatum = GetHighestDate(TransactionList.Transactions);
            var lägstDatum = GetLowestDate(TransactionList.Transactions);

            // Räkna ut antal månader emellan
            var månaderEmellan = GetNrMonthsBetweenDates(högstDatum, lägstDatum) + 1;

            var budgetRows = new List<BudgetRow>();
            foreach (var transactionGroup in transactions)
            {
                // Summera allt i en kategori
                var summKat =
                    transactionGroup.Sum(t => t.AmountAsDouble);

                // Dela summa med antal månader
                var averageFor1Kat = summKat / månaderEmellan;

                var row = new BudgetRow {CategoryText = transactionGroup.Key};
                var dateText = Transaction.GetYearMonthName(dateTime);
                row.AmountsForMonth.Add(dateText, averageFor1Kat);
                budgetRows.Add(row);
            }

            return budgetRows;
        }

        public static int GetNrMonthsBetweenDates(DateTime date1, DateTime date2)
        {
            return ((date1.Year - date2.Year) * 12)
                   + date1.Month
                   - date2.Month;
        }

        public static DateTime GetLowestDate(List<Transaction> transactions)
        {
            return transactions
                .OrderBy(t => t.DateAsDate)
                .FirstOrDefault()
                .DateAsDate;
        }

        public static DateTime GetHighestDate(List<Transaction> transactions)
        {
            return transactions
                .OrderByDescending(t => t.DateAsDate)
                .FirstOrDefault()
                .DateAsDate;
        }
    }
}