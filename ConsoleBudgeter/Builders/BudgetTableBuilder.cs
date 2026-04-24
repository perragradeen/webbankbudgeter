using System.Globalization;
using WebBankBudgeterTests.Facit;

namespace ConsoleBudgeter.Builders;

/// <summary>
/// Bygger den strukturerade budgetvyn (IN, UT, KVAR) som används för både
/// "Budget Total" och "Kvar" i WinForms-UI:t. Bygger samma radordning och
/// summeringsrader som <c>WebBankBudgeterService.Services.BudgetStructureBuilder</c>.
/// </summary>
public static class BudgetTableBuilder
{
    public const string CategoryNameColumnDescription = "Category . Month->";
    public const string AverageColumnDescription = "Average";
    public const string AverageColumnDescriptionNotFormatted = "Average-nf";
    public const string SummaColumnDescription = "Summa";

    public const string ExpensesSummaryRowName = "=== Summa utgifter ===";
    public const string IncomesSummaryRowName = "=== Summa inkomster ===";
    public const string TransfersSummaryRowName = "=== Summa förflyttningar ===";
    public const string TotalBudgetRowName = "=== BUDGET (Inkomster - Utgifter) ===";

    private const string IncomeMarker = "+";
    private const string TransferMarker = " -";

    public static BudgetTable BuildExpensesTable(int year,
        IEnumerable<BudgetUtFacit> expenses,
        IEnumerable<BudgetUtFacit> transfers)
    {
        var rows = new List<BudgetRow>();

        foreach (var item in expenses)
        {
            rows.Add(new BudgetRow
            {
                CategoryText = item.Category,
                AmountsForMonth =
                {
                    [MonthKey(item.Year, item.Month)] = item.ActualAmount
                }
            });
        }

        foreach (var item in transfers)
        {
            rows.Add(new BudgetRow
            {
                CategoryText = item.Category,
                AmountsForMonth =
                {
                    [MonthKey(item.Year, item.Month)] = item.ActualAmount
                }
            });
        }

        var merged = MergeRows(rows);
        return BuildStructuredBudget(year, merged);
    }

    /// <summary>
    /// Bygger IN-tabellen (enbart budgetrader, ingen summering).
    /// </summary>
    public static BudgetTable BuildBudgetInTable(int year, IEnumerable<BudgetInFacit> budgetIn)
    {
        var rows = new List<BudgetRow>();
        foreach (var item in budgetIn)
        {
            rows.Add(new BudgetRow
            {
                CategoryText = item.Category,
                AmountsForMonth =
                {
                    [MonthKey(item.Year, item.Month)] = item.BudgetAmount
                }
            });
        }

        var merged = MergeRows(rows).OrderBy(r => r.CategoryText, StringComparer.Ordinal).ToList();
        var headers = BuildHeaders(year, merged, includeAverage: true);

        var table = new BudgetTable { Year = year };
        table.ColumnHeaders.AddRange(headers);
        table.Rows.AddRange(merged);
        return table;
    }

    /// <summary>
    /// Bygger KVAR-tabellen (IN + UT = Remaining per kategori per månad).
    /// </summary>
    public static BudgetTable BuildKvarTable(int year, IEnumerable<BudgetKvarFacit> kvar)
    {
        var rows = new List<BudgetRow>();
        foreach (var item in kvar)
        {
            rows.Add(new BudgetRow
            {
                CategoryText = item.Category,
                AmountsForMonth =
                {
                    [MonthKey(item.Year, item.Month)] = item.Remaining
                }
            });
        }

        var merged = MergeRows(rows);
        return BuildStructuredBudget(year, merged);
    }

    private static BudgetTable BuildStructuredBudget(int year, List<BudgetRow> rows)
    {
        var incomeRows = rows.Where(r => r.CategoryText.Contains(IncomeMarker)).ToList();
        var transferRows = rows.Where(r => r.CategoryText.Contains(TransferMarker)).ToList();
        var expenseRows = rows
            .Where(r => !r.CategoryText.Contains(IncomeMarker) && !r.CategoryText.Contains(TransferMarker))
            .OrderBy(r => r.CategoryText, StringComparer.Ordinal)
            .ToList();

        var table = new BudgetTable { Year = year };
        var allRows = new List<BudgetRow>();
        allRows.AddRange(expenseRows);
        allRows.AddRange(incomeRows);
        allRows.AddRange(transferRows);
        var headers = BuildHeaders(year, allRows, includeAverage: true);
        table.ColumnHeaders.AddRange(headers);

        table.Rows.AddRange(expenseRows);
        if (expenseRows.Count > 0)
            table.Rows.Add(CreateSummaryRow(ExpensesSummaryRowName, expenseRows, headers));
        table.Rows.Add(new BudgetRow { CategoryText = string.Empty });

        table.Rows.AddRange(incomeRows);
        if (incomeRows.Count > 0)
            table.Rows.Add(CreateSummaryRow(IncomesSummaryRowName, incomeRows, headers));
        table.Rows.Add(new BudgetRow { CategoryText = string.Empty });

        table.Rows.AddRange(transferRows);
        if (transferRows.Count > 0)
            table.Rows.Add(CreateSummaryRow(TransfersSummaryRowName, transferRows, headers));
        table.Rows.Add(new BudgetRow { CategoryText = string.Empty });

        table.Rows.Add(CreateBudgetTotalRow(incomeRows, expenseRows, headers));
        return table;
    }

    private static List<BudgetRow> MergeRows(IEnumerable<BudgetRow> rows)
    {
        var byCat = new Dictionary<string, BudgetRow>(StringComparer.Ordinal);
        foreach (var row in rows)
        {
            if (!byCat.TryGetValue(row.CategoryText, out var existing))
            {
                existing = new BudgetRow { CategoryText = row.CategoryText };
                byCat[row.CategoryText] = existing;
            }

            foreach (var (k, v) in row.AmountsForMonth)
            {
                existing.AmountsForMonth.TryGetValue(k, out var current);
                existing.AmountsForMonth[k] = current + v;
            }
        }

        return byCat.Values.ToList();
    }

    private static List<string> BuildHeaders(int year, IEnumerable<BudgetRow> rows, bool includeAverage)
    {
        var headers = new List<string> { CategoryNameColumnDescription };
        if (includeAverage)
        {
            headers.Add(AverageColumnDescription);
            headers.Add(AverageColumnDescriptionNotFormatted);
        }

        var monthKeys = new SortedSet<(int y, int m)>();
        foreach (var row in rows)
        {
            foreach (var key in row.AmountsForMonth.Keys)
            {
                if (TryParseMonthKey(key, out var ym))
                {
                    monthKeys.Add(ym);
                }
            }
        }

        if (monthKeys.Count == 0)
        {
            for (var m = 1; m <= 12; m++) monthKeys.Add((year, m));
        }

        foreach (var (y, m) in monthKeys)
        {
            headers.Add(MonthKey(y, m));
        }

        return headers;
    }

    private static BudgetRow CreateSummaryRow(string rowName, List<BudgetRow> rows, List<string> headers)
    {
        var sum = new BudgetRow { CategoryText = rowName };
        var monthColumns = MonthColumns(headers);
        foreach (var col in monthColumns)
        {
            double total = 0;
            foreach (var row in rows)
            {
                if (row.AmountsForMonth.TryGetValue(col, out var v)) total += v;
            }
            sum.AmountsForMonth[col] = total;
        }
        return sum;
    }

    private static BudgetRow CreateBudgetTotalRow(List<BudgetRow> incomeRows, List<BudgetRow> expenseRows, List<string> headers)
    {
        var row = new BudgetRow { CategoryText = TotalBudgetRowName };
        var monthColumns = MonthColumns(headers);
        foreach (var col in monthColumns)
        {
            double income = 0, expense = 0;
            foreach (var r in incomeRows) if (r.AmountsForMonth.TryGetValue(col, out var v)) income += v;
            foreach (var r in expenseRows) if (r.AmountsForMonth.TryGetValue(col, out var v)) expense += v;
            row.AmountsForMonth[col] = income + expense;
        }
        return row;
    }

    public static List<string> MonthColumns(IEnumerable<string> headers) =>
        headers.Where(h => !h.Contains("Category") && !h.Contains("Average") && h != SummaColumnDescription).ToList();

    public static string MonthKey(int year, int month)
    {
        var date = new DateTime(year, month, 1);
        return date.Year.ToString(CultureInfo.InvariantCulture)
               + " "
               + date.ToString("MMMM", CultureInfo.InvariantCulture);
    }

    private static bool TryParseMonthKey(string key, out (int y, int m) ym)
    {
        ym = default;
        var parts = key.Split(' ', 2);
        if (parts.Length != 2) return false;
        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var y)) return false;
        if (!DateTime.TryParseExact(parts[1], "MMMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)) return false;
        ym = (y, dt.Month);
        return true;
    }
}

public sealed class BudgetTable
{
    public int Year { get; set; }
    public List<string> ColumnHeaders { get; } = new();
    public List<BudgetRow> Rows { get; } = new();
}

public sealed class BudgetRow
{
    public string CategoryText { get; set; } = string.Empty;
    public Dictionary<string, double> AmountsForMonth { get; } = new(StringComparer.Ordinal);
}
