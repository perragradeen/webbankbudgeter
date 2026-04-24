using System.Globalization;
using InbudgetHandler.Model;
using WebBankBudgeterService.Model;
using WebBankBudgeterService.Model.ViewModel;

namespace InbudgetHandler;

/// <summary>
/// Slår ihop budget-IN (<see cref="Rad"/>) med utgiftstabellen från transaktioner (plan M5.1 / G1).
/// </summary>
public static class BudgetTableInMerger
{
    /// <summary>
    /// Lägger IN-belopp per månad på befintliga <see cref="BudgetRow"/> (match på kategorinamn, trim),
    /// eller skapar ny rad om kategorin saknas. Månadskolumner som saknas i tabellen läggs till sist.
    /// Summeringsraden från <see cref="InBudgetHandler.SummaText"/> hoppas över.
    /// </summary>
    public static void MergeInRows(TextToTableOutPuter table, IReadOnlyList<Rad> inRader)
    {
        if (table.BudgetRows == null || inRader.Count == 0)
        {
            return;
        }

        var rows = table.BudgetRows.Select(CloneRow).ToList();

        foreach (var inRad in inRader)
        {
            if (string.Equals(inRad.RadNamnY, InBudgetHandler.SummaText, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var cat = inRad.RadNamnY?.Trim() ?? string.Empty;
            var row = rows.FirstOrDefault(r =>
                string.Equals(r.CategoryText?.Trim(), cat, StringComparison.Ordinal));

            if (row == null)
            {
                row = new BudgetRow { CategoryText = inRad.RadNamnY ?? string.Empty };
                rows.Add(row);
            }

            foreach (var (monthKey, amount) in inRad.Kolumner)
            {
                if (!IsMonthColumn(table.ColumnHeaders, monthKey))
                {
                    continue;
                }

                EnsureMonthHeader(table.ColumnHeaders, monthKey);

                row.AmountsForMonth.TryGetValue(monthKey, out var existing);
                row.AmountsForMonth[monthKey] = existing + amount;
            }
        }

        table.BudgetRows = rows;
    }

    private static bool IsMonthColumn(IReadOnlyList<string> headers, string key)
    {
        if (string.IsNullOrWhiteSpace(key) ||
            key.Contains(TextToTableOutPuter.CategoryNameColumnDescription, StringComparison.Ordinal) ||
            key.Contains("Average", StringComparison.Ordinal))
        {
            return false;
        }

        return headers.Contains(key) || TryParseYearMonthKey(key);
    }

    private static bool TryParseYearMonthKey(string key)
    {
        var parts = key.Split(' ', 2);
        if (parts.Length != 2) return false;
        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
            return false;
        return DateTime.TryParseExact(parts[1], "MMMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }

    private static void EnsureMonthHeader(List<string> headers, string monthKey)
    {
        if (headers.Contains(monthKey))
        {
            return;
        }

        headers.Add(monthKey);
    }

    private static BudgetRow CloneRow(BudgetRow source)
    {
        var row = new BudgetRow { CategoryText = source.CategoryText };
        foreach (var kv in source.AmountsForMonth)
        {
            row.AmountsForMonth[kv.Key] = kv.Value;
        }

        return row;
    }
}
