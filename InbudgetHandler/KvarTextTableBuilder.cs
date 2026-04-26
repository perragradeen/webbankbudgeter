using InbudgetHandler.Model;
using WebBankBudgeterService.Model;
using WebBankBudgeterService.Model.ViewModel;
using WebBankBudgeterService.Services;

namespace InbudgetHandler;

/// <summary>
/// Bygger Kvar-tabell som <see cref="TextToTableOutPuter"/> via <see cref="InBudgetMath.SnurraIgenom"/>
/// (samma logik som WinForms efter IN+UT-merge).
/// </summary>
public static class KvarTextTableBuilder
{
    public static TextToTableOutPuter Build(
        TextToTableOutPuter mergedExpenseTable,
        IReadOnlyList<Rad> inPosterRader,
        Action<string>? logLine = null)
    {
        if (mergedExpenseTable.BudgetRows == null)
        {
            return new TextToTableOutPuter();
        }

        var builder = new BudgetStructureBuilder();
        var structured = builder.BuildStructuredBudget(
            mergedExpenseTable.BudgetRows,
            mergedExpenseTable.ColumnHeaders);

        var utgiftRader = BudgetStructureBuilder.GetExpenseRowsBeforeFirstSummary(structured);
        var kvarRader = InBudgetMath.SnurraIgenom(inPosterRader, utgiftRader, logLine ?? (_ => { }));

        var kvarTable = new TextToTableOutPuter
        {
            UtgiftersStartYear = mergedExpenseTable.UtgiftersStartYear,
            AveragesForTransactions = mergedExpenseTable.AveragesForTransactions
        };
        kvarTable.ColumnHeaders.AddRange(mergedExpenseTable.ColumnHeaders);

        var monthKeys = BudgetStructureBuilder.MonthColumnKeys(mergedExpenseTable.ColumnHeaders);
        var budgetRows = new List<BudgetRow>();

        foreach (var rad in kvarRader)
        {
            if (string.Equals(rad.RadNamnY, InBudgetHandler.SummaText, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var row = new BudgetRow { CategoryText = rad.RadNamnY };
            foreach (var mk in monthKeys)
            {
                if (rad.Kolumner.TryGetValue(mk, out var v))
                {
                    row.AmountsForMonth[mk] = v;
                }
            }

            budgetRows.Add(row);
        }

        kvarTable.BudgetRows = budgetRows;
        return kvarTable;
    }
}
