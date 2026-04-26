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

        // Alla kategorirader (inkl. "+", " -", "-", …) — samma union som facit expected-kvar (IN + UT).
        // Inte bara "utgiftsblocket" före första summeringsrad i en strukturerad vy.
        var utgiftRader = mergedExpenseTable.BudgetRows.ToList();
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

            // Facit placeholder-rad "-" (saldoplaceholder) ska inte visas i Kvar-vyn.
            if (string.Equals(rad.RadNamnY?.Trim(), "-", StringComparison.Ordinal))
            {
                continue;
            }

            var row = new BudgetRow { CategoryText = rad.RadNamnY ?? string.Empty };
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
