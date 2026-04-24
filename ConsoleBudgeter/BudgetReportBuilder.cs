using System.Text;
using ConsoleBudgeter.Rendering;
using InbudgetHandler;
using WebBankBudgeterService.Model.ViewModel;
using WebBankBudgeterService.Services;
using WebBankBudgeterTests.Facit;

namespace ConsoleBudgeter;

/// <summary>
/// Bygger rapport från facit-JSON med samma service-lager som WinForms:
/// <see cref="FacitBudgetTextTableFactory"/>, <see cref="BudgetStructureBuilder"/>,
/// <see cref="BudgetTableInMerger"/>, <see cref="KvarTextTableBuilder"/>.
/// </summary>
public static class BudgetReportBuilder
{
    public static string BuildReport(int year, int? transactionLimit = 20)
    {
        var transactions = FacitLoader.LoadTransactions(year);
        var budgetIn = FacitLoader.LoadBudgetIn(year);
        var expectedUt = FacitLoader.LoadExpectedUt(year);
        var transfers = FacitLoader.LoadExpectedTransfers(year);
        var expectedKvar = FacitLoader.LoadExpectedKvar(year);

        var sb = new StringBuilder();
        sb.AppendLine($"# WebBankBudgeter – rapport för {year}");
        sb.AppendLine();

        sb.AppendLine("## Incomes (gv_incomes)");
        sb.AppendLine(IncomesRenderer.Render(year, budgetIn));

        var utAmounts = expectedUt
            .Select(u => (u.Category, u.Year, u.Month, u.ActualAmount))
            .Concat(transfers.Select(t => (t.Category, t.Year, t.Month, t.ActualAmount)));

        var expenseTable = FacitBudgetTextTableFactory.Build(year, utAmounts, addAverageColumns: true);
        var inRader = FacitInBudgetRows.FromFacit(budgetIn);
        var tableBeforeIn = TextToTableOutPuterClone.Clone(expenseTable);
        BudgetTableInMerger.MergeInRows(expenseTable, inRader);

        sb.AppendLine("## Utgifter aka - Budget Total (gv_budget)");
        sb.AppendLine(TableRenderer.Render(expenseTable));

        var kvarTable = KvarTextTableBuilder.Build(tableBeforeIn, inRader);
        sb.AppendLine("## Kvar (gv_Kvar)");
        sb.AppendLine(TableRenderer.Render(kvarTable));

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
