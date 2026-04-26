using System.Globalization;
using System.Text.Json;
using BudgeterCore.Entities;

namespace InbudgetHandler;

/// <summary>
/// Läser <c>budget-in-YYYY.json</c> (samma schema som facit-extraktorn) till <see cref="InBudget"/>.
/// </summary>
public static class FacitBudgetInLoader
{
    private sealed class RowDto
    {
        public string? Category { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public double BudgetAmount { get; set; }
    }

    /// <summary>
    /// Läser alla rader från filen och filtrerar på <paramref name="year"/> om satt.
    /// </summary>
    public static List<InBudget> Load(string filePath, int? year = null)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Facit budget-in hittades inte: {filePath}");
        }

        var json = File.ReadAllText(filePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var rows = JsonSerializer.Deserialize<List<RowDto>>(json, options)
                   ?? throw new InvalidOperationException($"Ogiltig JSON: {filePath}");

        var list = new List<InBudget>();
        foreach (var r in rows)
        {
            if (string.IsNullOrWhiteSpace(r.Category))
            {
                continue;
            }

            if (year.HasValue && r.Year != year.Value)
            {
                continue;
            }

            list.Add(new InBudget
            {
                CategoryDescription = r.Category!,
                BudgetValue = r.BudgetAmount,
                YearAndMonth = new DateTime(r.Year, r.Month, 1, 0, 0, 0, DateTimeKind.Unspecified)
            });
        }

        return list;
    }

    /// <summary>
    /// Förväntad fil i katalogen: <c>budget-in-{year}.json</c>.
    /// </summary>
    public static string GetDefaultFilePath(string facitDirectory, int year)
    {
        var name = string.Format(CultureInfo.InvariantCulture, "budget-in-{0}.json", year);
        return Path.Combine(facitDirectory, name);
    }
}
