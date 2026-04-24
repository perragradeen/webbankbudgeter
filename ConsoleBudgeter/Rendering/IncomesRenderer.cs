using System.Globalization;
using System.Text;
using WebBankBudgeterTests.Facit;

namespace ConsoleBudgeter.Rendering;

/// <summary>
/// Renderar "Incomes"-fliken. Motsvarar det <c>InBudgetUiHandler</c> binder
/// till <c>gv_incomes</c>: första kolumn = kategorinamn, följande kolumner =
/// månader, värden formaterade med "# ##0" enligt InvariantCulture.
/// </summary>
public static class IncomesRenderer
{
    public const string CategoryNameColumnDescription = "Category . Month->";

    public static string Render(int year, IEnumerable<BudgetInFacit> budgetIn, string? title = null)
    {
        var byCategory = budgetIn
            .GroupBy(b => b.Category, StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .ToList();

        var months = budgetIn
            .Select(b => (b.Year, b.Month))
            .Distinct()
            .OrderBy(t => t.Year).ThenBy(t => t.Month)
            .ToList();

        var headers = new List<string> { CategoryNameColumnDescription };
        headers.AddRange(months.Select(m => MonthKey(m.Year, m.Month)));

        var rows = new List<string[]>();
        foreach (var g in byCategory)
        {
            var row = new string[headers.Count];
            row[0] = g.Key;
            var byMonth = g.ToDictionary(x => MonthKey(x.Year, x.Month), x => x.BudgetAmount);
            for (var i = 1; i < headers.Count; i++)
            {
                byMonth.TryGetValue(headers[i], out var v);
                row[i] = v.ToString("# ##0", CultureInfo.InvariantCulture);
            }
            rows.Add(row);
        }

        return RenderSimpleTable(headers, rows, title);
    }

    private static string MonthKey(int year, int month)
    {
        var date = new DateTime(year, month, 1);
        return date.Year.ToString(CultureInfo.InvariantCulture) + " "
               + date.ToString("MMMM", CultureInfo.InvariantCulture);
    }

    internal static string RenderSimpleTable(List<string> headers, List<string[]> rows, string? title)
    {
        var widths = headers.Select(h => h.Length).ToArray();
        foreach (var row in rows)
        {
            for (var i = 0; i < widths.Length; i++)
            {
                if (row[i].Length > widths[i]) widths[i] = row[i].Length;
            }
        }

        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(title))
        {
            sb.AppendLine(title);
            sb.AppendLine(new string('=', title.Length));
        }

        sb.AppendLine(BorderLine(widths));
        sb.AppendLine(RowLine(headers.ToArray(), widths, leftFirst: true));
        sb.AppendLine(BorderLine(widths));
        foreach (var row in rows)
        {
            sb.AppendLine(RowLine(row, widths, leftFirst: true));
        }
        sb.AppendLine(BorderLine(widths));
        return sb.ToString();
    }

    private static string BorderLine(int[] widths)
    {
        var sb = new StringBuilder();
        sb.Append('+');
        foreach (var w in widths)
        {
            sb.Append(new string('=', w + 2));
            sb.Append('+');
        }
        return sb.ToString();
    }

    private static string RowLine(string[] cells, int[] widths, bool leftFirst)
    {
        var sb = new StringBuilder();
        sb.Append('|');
        for (var i = 0; i < cells.Length; i++)
        {
            sb.Append(' ');
            if (leftFirst && i == 0) sb.Append((cells[i] ?? string.Empty).PadRight(widths[i]));
            else sb.Append((cells[i] ?? string.Empty).PadLeft(widths[i]));
            sb.Append(' ');
            sb.Append('|');
        }
        return sb.ToString();
    }
}
