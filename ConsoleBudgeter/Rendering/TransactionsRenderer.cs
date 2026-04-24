using System.Globalization;
using System.Text;
using WebBankBudgeterTests.Facit;

namespace ConsoleBudgeter.Rendering;

/// <summary>
/// Renderar transaktionslistan (motsvarar <c>dg_Transactions</c>).
/// Kolumner: Date, Amount, Description, Category.
/// </summary>
public static class TransactionsRenderer
{
    public static string Render(IEnumerable<TransactionFacit> transactions, int? limit = null, string? title = null)
    {
        var headers = new[] { "Date", "Amount", "Description", "Category" };
        var list = transactions.ToList();
        if (limit.HasValue) list = list.Take(limit.Value).ToList();

        var rows = list.Select(t => new[]
        {
            new DateTime(t.Year, t.Month, t.Day).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            t.Amount.ToString("0.##", CultureInfo.InvariantCulture),
            t.Description ?? string.Empty,
            t.Category ?? string.Empty,
        }).ToList();

        return IncomesRenderer.RenderSimpleTable(headers.ToList(), rows, title);
    }
}

/// <summary>
/// Renderar totals-fliken (Återkommande snitt, Inkomster snitt, Diff snitt).
/// </summary>
public static class TotalsRenderer
{
    public static string Render(double recurringAverage, double incomeAverage, double diffAverage, string? title = null)
    {
        var headers = new[] { "Description", "Amount" };
        var rows = new List<string[]>
        {
            new[] { "Återkommande snitt", recurringAverage.ToString("# ##0", CultureInfo.InvariantCulture) },
            new[] { "Inkomster snitt",    incomeAverage.ToString("# ##0", CultureInfo.InvariantCulture) },
            new[] { "Diff snitt",         diffAverage.ToString("# ##0", CultureInfo.InvariantCulture) },
        };
        return IncomesRenderer.RenderSimpleTable(headers.ToList(), rows, title);
    }
}

internal static class StringBuilderExt
{
    public static StringBuilder AppendSection(this StringBuilder sb, string content)
    {
        sb.AppendLine();
        sb.AppendLine(content);
        return sb;
    }
}
