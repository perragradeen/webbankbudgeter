using System.Globalization;
using WebBankBudgeterService.Model;
using WebBankBudgeterService.Model.ViewModel;

namespace WebBankBudgeterService.Services;

/// <summary>
/// Bygger <see cref="TextToTableOutPuter"/> från platta (kategori, år, månad, belopp)-rader,
/// samma kolumnlayout som <see cref="TableGetter"/> med <see cref="TableGetter.AddAverageColumn"/> = true.
/// </summary>
public static class FacitBudgetTextTableFactory
{
    public static TextToTableOutPuter Build(
        int displayYear,
        IEnumerable<(string Category, int Year, int Month, double Amount)> monthAmounts,
        bool addAverageColumns = true)
    {
        var mergedByCat = new Dictionary<string, BudgetRow>(StringComparer.Ordinal);
        foreach (var (category, year, month, amount) in monthAmounts)
        {
            var key = category ?? string.Empty;
            if (!mergedByCat.TryGetValue(key, out var row))
            {
                row = new BudgetRow { CategoryText = key };
                mergedByCat[key] = row;
            }

            var monthCol = MonthKey(year, month);
            row.AmountsForMonth.TryGetValue(monthCol, out var cur);
            row.AmountsForMonth[monthCol] = cur + amount;
        }

        var monthKeys = mergedByCat.Values
            .SelectMany(r => r.AmountsForMonth.Keys)
            .Distinct(StringComparer.Ordinal)
            .Select(k => (Key: k, Ym: ParseMonthKey(k)))
            .Where(x => x.Ym.HasValue)
            .OrderBy(x => x.Ym!.Value.year)
            .ThenBy(x => x.Ym!.Value.month)
            .Select(x => x.Key)
            .ToList();

        var headers = new List<string> { TextToTableOutPuter.CategoryNameColumnDescription };
        if (addAverageColumns)
        {
            headers.Add(TextToTableOutPuter.AverageColumnDescription);
            headers.Add(TextToTableOutPuter.AverageColumnDescriptionNotFormatted);
        }

        headers.AddRange(monthKeys);

        var table = new TextToTableOutPuter
        {
            UtgiftersStartYear = displayYear.ToString(CultureInfo.InvariantCulture),
            BudgetRows = mergedByCat.Values.OrderBy(r => r.CategoryText, StringComparer.Ordinal).ToList()
        };

        table.ColumnHeaders.AddRange(headers);

        return table;
    }

    public static string MonthKey(int year, int month)
    {
        var date = new DateTime(year, month, 1);
        return date.Year.ToString(CultureInfo.InvariantCulture)
               + " "
               + date.ToString("MMMM", CultureInfo.InvariantCulture);
    }

    private static (int year, int month)? ParseMonthKey(string key)
    {
        var parts = key.Split(' ', 2);
        if (parts.Length != 2) return null;
        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var y))
            return null;
        if (!DateTime.TryParseExact(parts[1], "MMMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return null;
        return (y, dt.Month);
    }
}
