using System.Globalization;
using System.Text.Json;
using ClosedXML.Excel;

const string KontoutdragSheet = "Kontoutdrag_officiella";
const string Budget2014Sheet = "Budget (2014)";
const string Budget2015Sheet = "Budget (2015)";

var repoRoot = FindRepoRoot();
var xlsxPath = args.Length > 0
    ? args[0]
    : Path.Combine(repoRoot, "Pelles-budget-slim-2014-2015-gform.xlsx");
var outDir = args.Length > 1
    ? args[1]
    : Path.Combine(repoRoot, "WebBankBudgeterTests.Facit", "Facit");

if (!File.Exists(xlsxPath))
{
    Console.Error.WriteLine($"Excel saknas: {xlsxPath}");
    return 1;
}

Directory.CreateDirectory(outDir);

using var wb = new XLWorkbook(xlsxPath);

ExtractTransactions(wb.Worksheet(KontoutdragSheet), 2014, Path.Combine(outDir, "transactions-2014.json"));
ExtractTransactions(wb.Worksheet(KontoutdragSheet), 2015, Path.Combine(outDir, "transactions-2015.json"));

ExtractBudgetIn(wb.Worksheet(Budget2014Sheet), 2014, skipJanuary: true,
    Path.Combine(outDir, "budget-in-2014.json"));
ExtractBudgetIn(wb.Worksheet(Budget2015Sheet), 2015, skipJanuary: false,
    Path.Combine(outDir, "budget-in-2015.json"));

var t2014 = LoadTransactionsFromWorkbook(wb, 2014);
var t2015 = LoadTransactionsFromWorkbook(wb, 2015);

WriteExpectedUtAndTransfers(t2014, 2014, outDir);
WriteExpectedUtAndTransfers(t2015, 2015, outDir);

var in2014 = LoadBudgetIn(Path.Combine(outDir, "budget-in-2014.json"));
var in2015 = LoadBudgetIn(Path.Combine(outDir, "budget-in-2015.json"));
var ut2014 = LoadUt(Path.Combine(outDir, "expected-ut-2014.json"));
var ut2015 = LoadUt(Path.Combine(outDir, "expected-ut-2015.json"));

WriteExpectedKvar(in2014, ut2014, 2014, Path.Combine(outDir, "expected-kvar-2014.json"));
WriteExpectedKvar(in2015, ut2015, 2015, Path.Combine(outDir, "expected-kvar-2015.json"));

Console.WriteLine($"Facit skriven till: {outDir}");
return 0;

static string FindRepoRoot()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir != null)
    {
        if (File.Exists(Path.Combine(dir.FullName, "Budgetterarn.sln")))
            return dir.FullName;
        dir = dir.Parent;
    }

    return Directory.GetCurrentDirectory();
}

static void ExtractTransactions(IXLWorksheet ws, int year, string outPath)
{
    var list = new List<TransactionDto>();
    var row = ws.FirstRowUsed()?.RowNumber() ?? 1;
    var last = ws.LastRowUsed()?.RowNumber() ?? row;
    for (var r = row + 1; r <= last; r++)
    {
        var y = (int)Math.Round(ws.Cell(r, 1).GetDouble());
        if (y != year)
            continue;

        var month = (int)Math.Round(ws.Cell(r, 2).GetDouble());
        var day = (int)Math.Round(ws.Cell(r, 3).GetDouble());
        var desc = ws.Cell(r, 4).GetString().Trim();
        var amount = ws.Cell(r, 5).GetDouble();
        var category = ws.Cell(r, 8).GetString();
        var flagCell = ws.Cell(r, 12);
        var flag = flagCell.IsEmpty() ? "Regular" : flagCell.GetString().Trim();
        if (string.IsNullOrEmpty(flag))
            flag = "Regular";

        list.Add(new TransactionDto(y, month, day, desc, amount, category, flag));
    }

    list.Sort(CompareTransaction);
    WriteJson(outPath, list);
    Console.WriteLine($"{outPath} ({list.Count} rader)");
}

static int CompareTransaction(TransactionDto a, TransactionDto b)
{
    var c = string.Compare(a.Category, b.Category, StringComparison.Ordinal);
    if (c != 0) return c;
    c = a.Year.CompareTo(b.Year);
    if (c != 0) return c;
    c = a.Month.CompareTo(b.Month);
    if (c != 0) return c;
    c = a.Day.CompareTo(b.Day);
    if (c != 0) return c;
    return string.Compare(a.Description, b.Description, StringComparison.Ordinal);
}

static List<TransactionDto> LoadTransactionsFromWorkbook(XLWorkbook wb, int year)
{
    var ws = wb.Worksheet(KontoutdragSheet);
    var list = new List<TransactionDto>();
    var last = ws.LastRowUsed()?.RowNumber() ?? 2;
    for (var r = 2; r <= last; r++)
    {
        var y = (int)Math.Round(ws.Cell(r, 1).GetDouble());
        if (y != year)
            continue;

        var month = (int)Math.Round(ws.Cell(r, 2).GetDouble());
        var day = (int)Math.Round(ws.Cell(r, 3).GetDouble());
        var desc = ws.Cell(r, 4).GetString().Trim();
        var amount = ws.Cell(r, 5).GetDouble();
        var category = ws.Cell(r, 8).GetString();
        var flagCell = ws.Cell(r, 12);
        var flag = flagCell.IsEmpty() ? "Regular" : flagCell.GetString().Trim();
        if (string.IsNullOrEmpty(flag))
            flag = "Regular";

        list.Add(new TransactionDto(y, month, day, desc, amount, category, flag));
    }

    return list;
}

static void ExtractBudgetIn(IXLWorksheet ws, int year, bool skipJanuary, string outPath)
{
    var list = new List<BudgetInDto>();
    var last = ws.LastRowUsed()?.RowNumber() ?? 1;
    var monthStartCol = 6; // F = januari

    for (var r = 1; r <= last; r++)
    {
        var block = ws.Cell(r, 1).GetString().Trim();
        if (!string.Equals(block, "In", StringComparison.Ordinal))
            continue;

        var category = ws.Cell(r, 2).GetString().Trim();
        if (string.IsNullOrEmpty(category) ||
            category.Equals("IN", StringComparison.OrdinalIgnoreCase) ||
            category.Equals("Summa", StringComparison.OrdinalIgnoreCase))
            continue;

        for (var m = 1; m <= 12; m++)
        {
            if (skipJanuary && m == 1)
                continue;

            var col = monthStartCol + m - 1;
            var raw = ws.Cell(r, col);
            double v;
            if (raw.IsEmpty())
                v = 0;
            else
                v = RoundMoney(raw.GetDouble());

            var dt = new DateTime(year, m, 1);
            var monthName = dt.ToString("MMMM", CultureInfo.InvariantCulture);
            list.Add(new BudgetInDto(category, year, m, monthName, v));
        }
    }

    list.Sort(CompareBudgetIn);
    WriteJson(outPath, list);
    Console.WriteLine($"{outPath} ({list.Count} rader)");
}

static int CompareBudgetIn(BudgetInDto a, BudgetInDto b)
{
    var c = string.Compare(a.Category, b.Category, StringComparison.Ordinal);
    if (c != 0) return c;
    c = a.Year.CompareTo(b.Year);
    if (c != 0) return c;
    return a.Month.CompareTo(b.Month);
}

static void WriteExpectedUtAndTransfers(List<TransactionDto> all, int year, string outDir)
{
    var utAgg = new Dictionary<(string Cat, int Y, int M), double>();
    var trAgg = new Dictionary<(string Cat, int Y, int M), double>();

    foreach (var t in all)
    {
        if (!string.Equals(t.Flag, "Regular", StringComparison.OrdinalIgnoreCase))
            continue;

        var key = (t.Category, t.Year, t.Month);
        if (string.Equals(t.Category, " -", StringComparison.Ordinal))
        {
            trAgg.TryGetValue(key, out var s);
            trAgg[key] = RoundMoney(s + t.Amount);
            continue;
        }

        // Saldorad "-" i kontoutdrag — inte samma som förflyttning " -"; räknas inte i expected-ut
        if (string.Equals(t.Category.Trim(), "-", StringComparison.Ordinal))
            continue;

        utAgg.TryGetValue(key, out var u);
        utAgg[key] = RoundMoney(u + t.Amount);
    }

    var utList = utAgg
        .Where(kv => kv.Key.Y == year)
        .Select(kv => new BudgetUtDto(
            kv.Key.Cat,
            kv.Key.Y,
            kv.Key.M,
            MonthName(kv.Key.Y, kv.Key.M),
            kv.Value))
        .OrderBy(x => x.Category, StringComparer.Ordinal)
        .ThenBy(x => x.Year)
        .ThenBy(x => x.Month)
        .ToList();

    var trList = trAgg
        .Where(kv => kv.Key.Y == year)
        .Select(kv => new BudgetUtDto(
            kv.Key.Cat,
            kv.Key.Y,
            kv.Key.M,
            MonthName(kv.Key.Y, kv.Key.M),
            kv.Value))
        .OrderBy(x => x.Category, StringComparer.Ordinal)
        .ThenBy(x => x.Year)
        .ThenBy(x => x.Month)
        .ToList();

    WriteJson(Path.Combine(outDir, $"expected-ut-{year}.json"), utList);
    WriteJson(Path.Combine(outDir, $"expected-transfers-{year}.json"), trList);
    Console.WriteLine($"expected-ut-{year}.json ({utList.Count}), expected-transfers-{year}.json ({trList.Count})");
}

static string MonthName(int year, int month) =>
    new DateTime(year, month, 1).ToString("MMMM", CultureInfo.InvariantCulture);

static double RoundMoney(double v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

static void WriteExpectedKvar(
    List<BudgetInDto> budgetIn,
    List<BudgetUtDto> expectedUt,
    int year,
    string outPath)
{
    var inMap = new Dictionary<(string Cat, int M), double>();
    foreach (var b in budgetIn.Where(x => x.Year == year))
        inMap[(b.Category, b.Month)] = b.BudgetAmount;

    var utMap = new Dictionary<(string Cat, int M), double>();
    foreach (var u in expectedUt.Where(x => x.Year == year))
        utMap[(u.Category, u.Month)] = u.ActualAmount;

    var keys = new HashSet<(string Cat, int M)>();
    foreach (var k in inMap.Keys)
        keys.Add(k);
    foreach (var k in utMap.Keys)
        keys.Add(k);

    var list = new List<BudgetKvarDto>();
    foreach (var (cat, m) in keys.OrderBy(x => x.Cat, StringComparer.Ordinal).ThenBy(x => x.M))
    {
        inMap.TryGetValue((cat, m), out var bi);
        utMap.TryGetValue((cat, m), out var ua);
        var rem = RoundMoney(bi + ua);
        list.Add(new BudgetKvarDto(cat, year, m, MonthName(year, m), bi, ua, rem));
    }

    WriteJson(outPath, list);
    Console.WriteLine($"{outPath} ({list.Count} rader)");
}

static List<BudgetInDto> LoadBudgetIn(string path)
{
    var json = File.ReadAllText(path);
    return JsonSerializer.Deserialize<List<BudgetInDto>>(json,
               new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
           ?? [];
}

static List<BudgetUtDto> LoadUt(string path)
{
    var json = File.ReadAllText(path);
    return JsonSerializer.Deserialize<List<BudgetUtDto>>(json,
               new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
           ?? [];
}

static void WriteJson<T>(string path, T data)
{
    var opts = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    File.WriteAllText(path, JsonSerializer.Serialize(data, opts));
}

internal sealed record TransactionDto(
    int Year,
    int Month,
    int Day,
    string Description,
    double Amount,
    string Category,
    string Flag);

internal sealed record BudgetInDto(
    string Category,
    int Year,
    int Month,
    string MonthName,
    double BudgetAmount);

internal sealed record BudgetUtDto(
    string Category,
    int Year,
    int Month,
    string MonthName,
    double ActualAmount);

internal sealed record BudgetKvarDto(
    string Category,
    int Year,
    int Month,
    string MonthName,
    double BudgetAmount,
    double ActualAmount,
    double Remaining);
