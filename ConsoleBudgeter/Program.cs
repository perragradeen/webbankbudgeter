using System.Globalization;
using ConsoleBudgeter;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var years = new List<int>();
string? outputFile = null;
int? transactionLimit = 20;

for (var i = 0; i < args.Length; i++)
{
    var a = args[i];
    switch (a)
    {
        case "--year" when i + 1 < args.Length:
            if (int.TryParse(args[++i], NumberStyles.Integer, CultureInfo.InvariantCulture, out var y)) years.Add(y);
            break;
        case "--out" when i + 1 < args.Length:
            outputFile = args[++i];
            break;
        case "--transactions" when i + 1 < args.Length:
            if (int.TryParse(args[++i], NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
                transactionLimit = n <= 0 ? null : n;
            break;
        case "-h":
        case "--help":
            PrintHelp();
            return 0;
    }
}

if (years.Count == 0) years.AddRange(new[] { 2014, 2015 });

using var writer = outputFile == null
    ? Console.Out
    : new StreamWriter(outputFile, append: false, System.Text.Encoding.UTF8);

foreach (var year in years)
{
    writer.Write(BudgetReportBuilder.BuildReport(year, transactionLimit));
    writer.WriteLine();
}

return 0;

static void PrintHelp()
{
    Console.WriteLine("ConsoleBudgeter – textbaserad motsvarighet till WebBankBudgeterUi");
    Console.WriteLine();
    Console.WriteLine("Usage: ConsoleBudgeter [--year 2014] [--year 2015] [--transactions N] [--out file.txt]");
    Console.WriteLine();
    Console.WriteLine("Utan --year skrivs rapporter för både 2014 och 2015 ut.");
    Console.WriteLine("--transactions 0 = skriv ut alla transaktioner.");
}
