using System.Text.Json;
using BudgeterCore.Entities;
using WebBankBudgeterTests.Facit;

/// <summary>
/// Skriver <c>BudgetIns.json</c> ( befintligt UI-schema ) genom att slå ihop
/// <c>budget-in-2014.json</c> och <c>budget-in-2015.json</c> från facit-projektets output.
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        var outPath = args.Length > 0
            ? args[0]
            : Path.GetFullPath(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..",
                "WebBankBudgeterUi", "TestData", "BudgetIns.json"));

        var list = new List<InBudget>();
        foreach (var year in new[] { 2014, 2015 })
        {
            foreach (var b in FacitLoader.LoadBudgetIn(year))
            {
                list.Add(new InBudget
                {
                    CategoryDescription = b.Category,
                    BudgetValue = b.BudgetAmount,
                    YearAndMonth = new DateTime(b.Year, b.Month, 1, 0, 0, 0, DateTimeKind.Unspecified)
                });
            }
        }

        var options = new JsonSerializerOptions { WriteIndented = true };
        Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
        File.WriteAllText(outPath, JsonSerializer.Serialize(list, options));
        Console.WriteLine($"Wrote {list.Count} rows to {outPath}");
        return 0;
    }
}
