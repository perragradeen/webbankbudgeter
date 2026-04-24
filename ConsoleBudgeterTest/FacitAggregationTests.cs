using ConsoleBudgeter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebBankBudgeterService.Services;
using WebBankBudgeterTests.Facit;

namespace ConsoleBudgeterTest;

/// <summary>
/// Tester som verifierar att tabeller som byggs från facit-data matchar
/// förväntade summor (budget-in + expected-ut == expected-kvar) samt att
/// renderade texter inte är tomma.
/// </summary>
[TestClass]
public class FacitAggregationTests
{
    private const double Tolerance = 0.01;

    [DataTestMethod]
    [DataRow(2014)]
    [DataRow(2015)]
    public void BudgetInPlusExpectedUt_Equals_ExpectedKvar(int year)
    {
        var budgetIn = FacitLoader.LoadBudgetIn(year);
        var ut = FacitLoader.LoadExpectedUt(year);
        var kvar = FacitLoader.LoadExpectedKvar(year);

        var inByKey = budgetIn
            .ToDictionary(x => (x.Category, x.Year, x.Month), x => x.BudgetAmount);
        var utByKey = ut
            .ToDictionary(x => (x.Category, x.Year, x.Month), x => x.ActualAmount);

        foreach (var k in kvar)
        {
            inByKey.TryGetValue((k.Category, k.Year, k.Month), out var b);
            utByKey.TryGetValue((k.Category, k.Year, k.Month), out var a);
            var expected = b + a;
            Assert.AreEqual(k.Remaining, expected, Tolerance,
                $"Mismatch för {k.Category} {k.Year}-{k.Month}: facit {k.Remaining}, beräknat {expected}");
        }
    }

    [DataTestMethod]
    [DataRow(2014)]
    [DataRow(2015)]
    public void BudgetTotal_SummaryRow_EqualsSumOfExpenses(int year)
    {
        var ut = FacitLoader.LoadExpectedUt(year);
        var transfers = FacitLoader.LoadExpectedTransfers(year);
        var amounts = ut
            .Select(u => (u.Category, u.Year, u.Month, u.ActualAmount))
            .Concat(transfers.Select(t => (t.Category, t.Year, t.Month, t.ActualAmount)));

        var table = FacitBudgetTextTableFactory.Build(year, amounts, addAverageColumns: true);
        var structured = new BudgetStructureBuilder().BuildStructuredBudget(
            table.BudgetRows!.ToList(),
            table.ColumnHeaders);

        var summaryRow = structured.Rows.First(r => r.CategoryText == BudgetStructureBuilder.ExpensesSummaryRowName);
        var expenseRows = BudgetStructureBuilder.GetExpenseRowsBeforeFirstSummary(structured);

        foreach (var monthCol in BudgetStructureBuilder.MonthColumnKeys(table.ColumnHeaders))
        {
            var expected = expenseRows.Sum(r =>
                r.AmountsForMonth.TryGetValue(monthCol, out var v) ? v : 0);
            summaryRow.AmountsForMonth.TryGetValue(monthCol, out var actual);
            Assert.AreEqual(expected, actual, Tolerance,
                $"Fel summa för utgifter i {monthCol}");
        }
    }

    [DataTestMethod]
    [DataRow(2014, 809)]
    [DataRow(2015, 845)]
    public void FacitTransactions_HaveExpectedCount(int year, int expected)
    {
        var transactions = FacitLoader.LoadTransactions(year);
        Assert.AreEqual(expected, transactions.Count, $"Fel antal transaktioner för {year}");
    }

    [DataTestMethod]
    [DataRow(2014)]
    [DataRow(2015)]
    public void BuildReport_ProducesAllSections(int year)
    {
        var report = BudgetReportBuilder.BuildReport(year, transactionLimit: null);
        StringAssert.Contains(report, "Incomes (gv_incomes)");
        StringAssert.Contains(report, "Utgifter aka - Budget Total (gv_budget)");
        StringAssert.Contains(report, "Kvar (gv_Kvar)");
        StringAssert.Contains(report, "Totals (gv_Totals)");
        StringAssert.Contains(report, "Transactions (dg_Transactions)");
        StringAssert.Contains(report, "=== Summa utgifter ===");
        StringAssert.Contains(report, "=== BUDGET (Inkomster - Utgifter) ===");
        StringAssert.Contains(report, "Category . Month->");
        StringAssert.Contains(report, $"{year} January");
        StringAssert.Contains(report, $"{year} December");
    }
}
