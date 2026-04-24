using ClosedXML.Excel;
using System.Globalization;
using System.Text.Json;

namespace FacitExtractor;

class Program
{
    static void Main(string[] args)
    {
        var excelPath = args.Length > 0 ? args[0] : "../../pelles budget.xls";
        var outputDir = args.Length > 1 ? args[1] : "../../WebBankBudgeterUiTest/Facit";

        Console.WriteLine($"Reading Excel file: {excelPath}");
        Console.WriteLine($"Output directory: {outputDir}");

        Directory.CreateDirectory(outputDir);

        using var workbook = new XLWorkbook(excelPath);

        // Extract transactions
        Console.WriteLine("Extracting transactions...");
        var transactions2014 = ExtractTransactions(workbook, 2014);
        var transactions2015 = ExtractTransactions(workbook, 2015);

        WriteJson(Path.Combine(outputDir, "transactions-2014.json"), transactions2014);
        WriteJson(Path.Combine(outputDir, "transactions-2015.json"), transactions2015);

        Console.WriteLine($"  2014: {transactions2014.Count} transactions");
        Console.WriteLine($"  2015: {transactions2015.Count} transactions");

        // Extract budget-in
        Console.WriteLine("Extracting budget-in...");
        var budgetIn2014 = ExtractBudgetIn(workbook, 2014);
        var budgetIn2015 = ExtractBudgetIn(workbook, 2015);

        WriteJson(Path.Combine(outputDir, "budget-in-2014.json"), budgetIn2014);
        WriteJson(Path.Combine(outputDir, "budget-in-2015.json"), budgetIn2015);

        Console.WriteLine($"  2014: {budgetIn2014.Count} budget rows");
        Console.WriteLine($"  2015: {budgetIn2015.Count} budget rows");

        // Generate expected-ut (excluding transfers)
        Console.WriteLine("Generating expected-ut...");
        var expectedUt2014 = GenerateExpectedUt(transactions2014, excludeTransfers: true);
        var expectedUt2015 = GenerateExpectedUt(transactions2015, excludeTransfers: true);

        WriteJson(Path.Combine(outputDir, "expected-ut-2014.json"), expectedUt2014);
        WriteJson(Path.Combine(outputDir, "expected-ut-2015.json"), expectedUt2015);

        // Generate expected-transfers
        Console.WriteLine("Generating expected-transfers...");
        var expectedTransfers2014 = GenerateExpectedTransfers(transactions2014);
        var expectedTransfers2015 = GenerateExpectedTransfers(transactions2015);

        WriteJson(Path.Combine(outputDir, "expected-transfers-2014.json"), expectedTransfers2014);
        WriteJson(Path.Combine(outputDir, "expected-transfers-2015.json"), expectedTransfers2015);

        // Generate expected-kvar
        Console.WriteLine("Generating expected-kvar...");
        var expectedKvar2014 = GenerateExpectedKvar(budgetIn2014, expectedUt2014);
        var expectedKvar2015 = GenerateExpectedKvar(budgetIn2015, expectedUt2015);

        WriteJson(Path.Combine(outputDir, "expected-kvar-2014.json"), expectedKvar2014);
        WriteJson(Path.Combine(outputDir, "expected-kvar-2015.json"), expectedKvar2015);

        // Write README
        WriteReadme(outputDir, transactions2014.Count, transactions2015.Count, 
                    budgetIn2014.Count, budgetIn2015.Count);

        Console.WriteLine("Done!");
    }

    static List<TransactionFacit> ExtractTransactions(XLWorkbook workbook, int year)
    {
        var worksheet = workbook.Worksheet("Kontoutdrag_officiella");
        var transactions = new List<TransactionFacit>();

        var row = 2; // Skip header
        while (!worksheet.Cell(row, 1).IsEmpty())
        {
            var yearVal = (int)worksheet.Cell(row, 1).GetValue<double>();
            if (yearVal != year)
            {
                row++;
                continue;
            }

            var month = (int)worksheet.Cell(row, 2).GetValue<double>();
            var day = (int)worksheet.Cell(row, 3).GetValue<double>();
            var description = worksheet.Cell(row, 4).GetString();
            var amount = worksheet.Cell(row, 5).GetValue<double>();
            var category = worksheet.Cell(row, 8).GetString();
            var flag = worksheet.Cell(row, 12).GetString();

            transactions.Add(new TransactionFacit
            {
                Year = yearVal,
                Month = month,
                Day = day,
                Description = description,
                Amount = Math.Round(amount, 2),
                Category = category,
                Flag = string.IsNullOrWhiteSpace(flag) ? "Regular" : flag
            });

            row++;
        }

        return transactions.OrderBy(t => t.Category)
                          .ThenBy(t => t.Year)
                          .ThenBy(t => t.Month)
                          .ThenBy(t => t.Day)
                          .ToList();
    }

    static List<BudgetInFacit> ExtractBudgetIn(XLWorkbook workbook, int year)
    {
        var worksheetName = $"Budget ({year})";
        var worksheet = workbook.Worksheet(worksheetName);
        var budgetRows = new List<BudgetInFacit>();

        // IN section starts at row 25 (approximately, need to find "IN" marker)
        // Columns F-Q are months (Feb-Dec for 2014, Jan-Dec for 2015)
        
        var startRow = 25; // Approximate, adjust based on actual file
        var endRow = 57;   // Approximate
        
        for (int row = startRow; row <= endRow; row++)
        {
            var categoryCell = worksheet.Cell(row, 5); // Column E
            if (categoryCell.IsEmpty()) continue;
            
            var category = categoryCell.GetString().Trim();
            if (string.IsNullOrWhiteSpace(category)) continue;
            if (category.Contains("===")) continue; // Skip summary rows

            // Columns F (6) through Q (17) are months
            var monthColumns = year == 2014 
                ? Enumerable.Range(7, 11).ToList() // Feb-Dec (columns G-Q, 11 months)
                : Enumerable.Range(6, 12).ToList(); // Jan-Dec (columns F-Q, 12 months)

            foreach (var colIndex in monthColumns)
            {
                var cell = worksheet.Cell(row, colIndex);
                if (cell.IsEmpty() || !cell.TryGetValue(out double value)) continue;
                if (value == 0) continue;

                var monthIndex = year == 2014 
                    ? colIndex - 6  // Feb=2, Mar=3, ..., Dec=12
                    : colIndex - 5; // Jan=1, Feb=2, ..., Dec=12

                budgetRows.Add(new BudgetInFacit
                {
                    Category = category,
                    Year = year,
                    Month = monthIndex,
                    MonthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(monthIndex),
                    BudgetAmount = Math.Round(value, 2)
                });
            }
        }

        return budgetRows.OrderBy(b => b.Category)
                        .ThenBy(b => b.Year)
                        .ThenBy(b => b.Month)
                        .ToList();
    }

    static List<BudgetUtFacit> GenerateExpectedUt(List<TransactionFacit> transactions, bool excludeTransfers)
    {
        var filtered = transactions
            .Where(t => t.Flag != "Ignore")
            .Where(t => !excludeTransfers || t.Category != " -");

        var grouped = filtered.GroupBy(t => new { t.Category, t.Year, t.Month })
                             .Select(g => new BudgetUtFacit
                             {
                                 Category = g.Key.Category,
                                 Year = g.Key.Year,
                                 Month = g.Key.Month,
                                 MonthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                                 ActualAmount = Math.Round(g.Sum(t => t.Amount), 2)
                             })
                             .OrderBy(b => b.Category)
                             .ThenBy(b => b.Year)
                             .ThenBy(b => b.Month)
                             .ToList();

        return grouped;
    }

    static List<BudgetUtFacit> GenerateExpectedTransfers(List<TransactionFacit> transactions)
    {
        return transactions
            .Where(t => t.Flag != "Ignore")
            .Where(t => t.Category == " -")
            .GroupBy(t => new { t.Category, t.Year, t.Month })
            .Select(g => new BudgetUtFacit
            {
                Category = g.Key.Category,
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                ActualAmount = Math.Round(g.Sum(t => t.Amount), 2)
            })
            .OrderBy(b => b.Category)
            .ThenBy(b => b.Year)
            .ThenBy(b => b.Month)
            .ToList();
    }

    static List<BudgetKvarFacit> GenerateExpectedKvar(
        List<BudgetInFacit> budgetIn, 
        List<BudgetUtFacit> expectedUt)
    {
        var result = new List<BudgetKvarFacit>();

        // Union of all (category, year, month) combinations
        var allKeys = budgetIn.Select(b => (b.Category, b.Year, b.Month))
                             .Union(expectedUt.Select(u => (u.Category, u.Year, u.Month)))
                             .Distinct()
                             .OrderBy(k => k.Category)
                             .ThenBy(k => k.Year)
                             .ThenBy(k => k.Month);

        foreach (var (category, year, month) in allKeys)
        {
            var inRow = budgetIn.FirstOrDefault(b => 
                b.Category == category && b.Year == year && b.Month == month);
            var utRow = expectedUt.FirstOrDefault(u => 
                u.Category == category && u.Year == year && u.Month == month);

            var budgetAmount = inRow?.BudgetAmount ?? 0;
            var actualAmount = utRow?.ActualAmount ?? 0;

            result.Add(new BudgetKvarFacit
            {
                Category = category,
                Year = year,
                Month = month,
                MonthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month),
                BudgetAmount = budgetAmount,
                ActualAmount = actualAmount,
                Remaining = Math.Round(budgetAmount + actualAmount, 2)
            });
        }

        return result;
    }

    static void WriteJson<T>(string path, T data)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(data, options);
        File.WriteAllText(path, json);
        Console.WriteLine($"  Wrote {path}");
    }

    static void WriteReadme(string outputDir, int trans2014, int trans2015, int budget2014, int budget2015)
    {
        var readme = @"# Facit-data (utdragen ur pelles-budget-slim-2014-2015.xlsx)

## Ursprung
- Källa: `pelles budget.xls`
- Filen är ett fryst snapshot av användarens riktiga budget 2014–2015.
- Extrakt gjort av `tools/FacitExtractor/` (engångskörning, inte en del av bygget).

## Filer
| Fil | Innehåll | Källrad i Excel |
|-----|----------|-----------------|
| transactions-YYYY.json | En rad per transaktion | `Kontoutdrag_officiella` rad 2+ |
| budget-in-YYYY.json    | Budget per kategori per månad | `Budget (YYYY)` rad 25–57 |
| expected-ut-YYYY.json  | Summa transaktioner per (kat, mån) | Beräknat ur transaktioner |
| expected-kvar-YYYY.json| Budget + utfall per (kat, mån) | Beräknat (IN + UT) |

## Invarianter som testas
1. `sum(transactions.amount where Flag != ""Ignore"") per kategori per månad == expected-ut` (tolerans ±0.01)
2. `budget-in + expected-ut == expected-kvar` (per kategori per månad)
3. Transaktioner med `Flag == ""Ignore""` räknas **inte** med i UT.
4. Antal transaktioner per år: 2014 = " + trans2014 + @", 2015 = " + trans2015 + @".
5. IN 2014 har " + budget2014 + @" rader.
6. IN 2015 har " + budget2015 + @" rader.
7. Transfers (`"" -""`) ingår i egen fil `expected-transfers-YYYY.json`.
";

        File.WriteAllText(Path.Combine(outputDir, "README.md"), readme);
        Console.WriteLine($"  Wrote README.md");
    }
}

// Models
record TransactionFacit
{
    public int Year { get; init; }
    public int Month { get; init; }
    public int Day { get; init; }
    public string Description { get; init; } = "";
    public double Amount { get; init; }
    public string Category { get; init; } = "";
    public string Flag { get; init; } = "";
}

record BudgetInFacit
{
    public string Category { get; init; } = "";
    public int Year { get; init; }
    public int Month { get; init; }
    public string MonthName { get; init; } = "";
    public double BudgetAmount { get; init; }
}

record BudgetUtFacit
{
    public string Category { get; init; } = "";
    public int Year { get; init; }
    public int Month { get; init; }
    public string MonthName { get; init; } = "";
    public double ActualAmount { get; init; }
}

record BudgetKvarFacit
{
    public string Category { get; init; } = "";
    public int Year { get; init; }
    public int Month { get; init; }
    public string MonthName { get; init; } = "";
    public double BudgetAmount { get; init; }
    public double ActualAmount { get; init; }
    public double Remaining { get; init; }
}
