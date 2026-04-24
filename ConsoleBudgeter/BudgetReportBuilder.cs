using System.Text;
using ConsoleBudgeter.Builders;
using ConsoleBudgeter.Rendering;
using WebBankBudgeterTests.Facit;

namespace ConsoleBudgeter;

/// <summary>
/// Bygger en komplett textbaserad rapport för ett år baserat på facit-data.
/// Speglar vad WinForms-UI:t (flikarna Kvar, Incomes, Budget Total, Totals,
/// Transactions) visar.
/// </summary>
public static class BudgetReportBuilder
{
    public static string BuildReport(int year, int? transactionLimit = 20)
    {
        var transactions = FacitLoader.LoadTransactions(year);
        var budgetIn     = FacitLoader.LoadBudgetIn(year);
        var expectedUt   = FacitLoader.LoadExpectedUt(year);
        var transfers    = FacitLoader.LoadExpectedTransfers(year);
        var expectedKvar = FacitLoader.LoadExpectedKvar(year);

        var sb = new StringBuilder();
        sb.AppendLine($"# WebBankBudgeter – rapport för {year}");
        sb.AppendLine();

        var budgetTotal = BudgetTableBuilder.BuildExpensesTable(year, expectedUt, transfers);
        sb.AppendLine("## Budget Total (gv_budget)");
        sb.AppendLine(TableRenderer.Render(budgetTotal));

        var kvar = BudgetTableBuilder.BuildKvarTable(year, expectedKvar);
        sb.AppendLine("## Kvar (gv_Kvar)");
        sb.AppendLine(TableRenderer.Render(kvar));

        sb.AppendLine("## Incomes (gv_incomes)");
        sb.AppendLine(IncomesRenderer.Render(year, budgetIn));

        var recurringAvg = expectedUt
            .Where(u => !string.IsNullOrEmpty(u.Category))
            .GroupBy(u => u.Category)
            .Select(g => g.Average(x => x.ActualAmount))
            .DefaultIfEmpty(0)
            .Sum();
        var incomeAvg = 0.0;
        var diffAvg = incomeAvg + recurringAvg;

        sb.AppendLine("## Totals (gv_Totals)");
        sb.AppendLine(TotalsRenderer.Render(recurringAvg, incomeAvg, diffAvg));

        sb.AppendLine($"## Transactions (dg_Transactions) – topp {transactionLimit?.ToString() ?? "alla"}");
        sb.AppendLine(TransactionsRenderer.Render(transactions, transactionLimit));

        return sb.ToString();
    }
}
