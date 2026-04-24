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

        // Samma flikordning som UI:t: In (gv_incomes) → Budget Total (gv_budget) → Kvar (gv_Kvar).
        sb.AppendLine("## Incomes (gv_incomes)");
        sb.AppendLine(IncomesRenderer.Render(year, budgetIn));

        var budgetTotal = BudgetTableBuilder.BuildExpensesTable(year, expectedUt, transfers);
        sb.AppendLine("## Budget Total (gv_budget)");
        sb.AppendLine(TableRenderer.Render(budgetTotal));

        // Kvar-fliken ska inte visa transaktions-/saldo-raden "-" (facit använder den för annat än kategorier).
        var kvarFacitRows = expectedKvar.Where(k => k.Category.Trim() != "-").ToList();
        var kvar = BudgetTableBuilder.BuildKvarTable(year, kvarFacitRows);
        sb.AppendLine("## Kvar (gv_Kvar)");
        sb.AppendLine(TableRenderer.Render(kvar));

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
