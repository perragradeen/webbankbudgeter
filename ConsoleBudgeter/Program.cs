using System.Globalization;
using System.Text;
using BudgeterCore.Entities;
using GeneralSettingsHandler;
using InbudgetHandler;
using InbudgetHandler.Model;
using WebBankBudgeterService;
using WebBankBudgeterService.Model;
using WebBankBudgeterService.Model.ViewModel;
using WebBankBudgeterService.MonthAvarages;
using WebBankBudgeterService.Services;

namespace ConsoleBudgeter;

internal static class Program
{
    private static readonly CultureInfo SvSe = CultureInfo.GetCultureInfo("sv-SE");

    private static async Task<int> Main(string[] args)
    {
        var years = new List<int>();
        string? outPath = null;
        string? transactionFileOverride = null;
        for (var i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--year", StringComparison.OrdinalIgnoreCase)
                && i + 1 < args.Length
                && int.TryParse(args[++i], out var y))
            {
                years.Add(y);
            }
            else if (string.Equals(args[i], "--out", StringComparison.OrdinalIgnoreCase)
                     && i + 1 < args.Length)
            {
                outPath = args[++i];
            }
            else if (string.Equals(args[i], "--transaction-file", StringComparison.OrdinalIgnoreCase)
                     && i + 1 < args.Length)
            {
                transactionFileOverride = args[++i];
            }
        }

        if (years.Count == 0)
        {
            years.Add(2014);
            years.Add(2015);
        }

        var sb = new StringBuilder();
        void W(string s) => sb.AppendLine(s);

        var appDir = AppContext.BaseDirectory;
        var settingsPath = Path.Combine(appDir, "Data", "GeneralSettings.xml");
        if (!File.Exists(settingsPath))
        {
            Console.Error.WriteLine("Saknar Data/GeneralSettings.xml i output. Bygg projektet så att filen kopieras.");
            return 1;
        }

        var getter = new GeneralSettingsGetter(settingsPath);
        var transactionPath = getter.GetStringSetting("TransactionTestFilePath");
        var categoryRelative = getter.GetStringSetting("CategoryPath");
        if (string.IsNullOrWhiteSpace(transactionPath) || string.IsNullOrWhiteSpace(categoryRelative))
        {
            Console.Error.WriteLine("GeneralSettings.xml saknar TransactionTestFilePath eller CategoryPath.");
            return 1;
        }

        var categoryPath = Path.GetFullPath(Path.Combine(appDir, categoryRelative));
        var resolvedTransactionPath = string.IsNullOrWhiteSpace(transactionFileOverride)
            ? Path.GetFullPath(Path.Combine(appDir, transactionPath))
            : Path.GetFullPath(transactionFileOverride);
        if (!File.Exists(resolvedTransactionPath))
        {
            Console.Error.WriteLine("Transaktionsfil finns inte: " + resolvedTransactionPath);
            Console.Error.WriteLine(
                "Ange sökväg i ConsoleBudgeter/Data/GeneralSettings.xml eller flaggan --transaction-file.");
            return 1;
        }

        if (!File.Exists(categoryPath))
        {
            Console.Error.WriteLine("Kategorifil finns inte: " + categoryPath);
            return 1;
        }

        var inBudgetPath = Path.Combine(appDir, "TestData", "BudgetIns.json");
        if (!File.Exists(inBudgetPath))
        {
            Console.Error.WriteLine("Saknar TestData/BudgetIns.json i output.");
            return 1;
        }

        var noop = static (string _) => { };
        var tableGetter = new TableGetter { AddAverageColumn = true };
        var transactionHandler = new TransactionHandler(
            noop,
            tableGetter,
            categoryPath,
            resolvedTransactionPath);

        var inBudgetHandler = new InBudgetHandler(inBudgetPath);

        if (!await transactionHandler.GetTransactionsAsync())
        {
            Console.Error.WriteLine("Kunde inte läsa transaktioner.");
            return 1;
        }

        transactionHandler.SortTransactions();
        transactionHandler.RemoveDuplicates();

        var allTransactionsBackup = new TransactionList
        {
            Transactions = transactionHandler.TransactionList!.Transactions.ToList(),
            Account = transactionHandler.TransactionList.Account
        };

        var countsByYear = allTransactionsBackup.Transactions
            .GroupBy(t => t.DateAsDate.Year)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Count());

        W("## Diagnostik");
        W("Transaktionsfil: " + resolvedTransactionPath);
        W("Antal transaktioner efter inläsning: " + allTransactionsBackup.Transactions.Count);
        W("Antal per kalenderår: " + string.Join(
            ", ",
            countsByYear.Select(kv => $"{kv.Key}={kv.Value}")));
        if (years.Any(y => !countsByYear.ContainsKey(y) || countsByYear[y] == 0))
        {
            W("Varning: minst ett begärt år saknar transaktioner i denna fil.");
            W("Byt källa med --transaction-file eller uppdatera GeneralSettings.xml.");
        }

        foreach (var year in years.OrderBy(y => y))
        {
            W("");
            W(new string('=', 72));
            W($" År {year} ");
            W(new string('=', 72));

            transactionHandler.SetTransactionList(new TransactionList
            {
                Transactions = allTransactionsBackup.Transactions.ToList(),
                Account = allTransactionsBackup.Account
            });
            var filtered = TransFilterer.FilterTransactions(transactionHandler.TransactionList!, year);
            transactionHandler.SetTransactionList(filtered);

            var table = transactionHandler.GetTextTableFromTransactions();
            if (table?.BudgetRows == null)
            {
                W("(ingen tabelldata)");
                continue;
            }

            table.AveragesForTransactions = SkapaInPosterHanterare.GetAvarages(
                filtered,
                new DateTime(year, 1, 1));

            W("## Inkomster / in-budget (gv_incomes) — rådata från BudgetIns.json");
            inBudgetHandler.SetInPosterFilter(
                new DateTime(year, 1, 1),
                new DateTime(year, 12, 31));
            var inPoster = await inBudgetHandler.GetInPoster();
            inPoster = inPoster
                .Where(i => i.YearAndMonth.Year == year)
                .OrderBy(i => i.YearAndMonth)
                .ToList();
            var inRubriker = HämtaInKolumnRubriker(inPoster)
                .OrderBy(Transaction.GetDateFromYearMonthName)
                .ToList();
            var inRader = await inBudgetHandler.HämtaRaderFörUiBindningAsync();
            AppendIncomeSection(W, inRubriker, inRader);

            W("");
            W("## Utgifter (strukturerad tabell som gv_budget)");
            AppendStructuredBudgetTable(W, table);

            var utgifterFörKvar = GetExpenseBudgetRowsForKvar(table);
            W("");
            W("## Kvar (gv_Kvar) — IN + UT per kategori (döljer kategori \"-\")");
            var inFörKvar = inRader
                .Where(r => !string.Equals(r.RadNamnY?.Trim(), InBudgetHandler.SummaText, StringComparison.Ordinal))
                .Where(r => !string.Equals(r.RadNamnY?.Trim(), "-", StringComparison.Ordinal))
                .ToList();
            var kvarRader = InBudgetKvarCalculator.SnurraIgenom(inFörKvar, utgifterFörKvar, msg => W(msg));
            AppendIncomeStyleTable(W, inRubriker, kvarRader, skipCategoryDash: true);

            W("");
            W("## Totals (gv_Totals)");
            var averages = new MonthAvaragesCalcs(filtered).GetMonthAvarages();
            W($"Återkommande snitt\t{averages.ReccuringCosts.ToString("# ##0", CultureInfo.InvariantCulture)}");
            W($"Inkomster snitt\t{averages.Incomes.ToString("# ##0", CultureInfo.InvariantCulture)}");
            W($"Diff snitt\t{averages.IncomeDiffCosts.ToString("# ##0", CultureInfo.InvariantCulture)}");

            W("");
            W("## Transaktioner (alla rader för året)");
            foreach (var t in filtered.Transactions.OrderBy(t => t.DateAsDate))
            {
                W($"{t.DateAsDate:yyyy-MM-dd}\t{t.AmountAsDouble}\t{t.Description}\t{t.CategoryName}");
            }
        }

        var text = sb.ToString();
        if (!string.IsNullOrEmpty(outPath))
        {
            var fullOut = Path.GetFullPath(outPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullOut)!);
            await File.WriteAllTextAsync(fullOut, text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            Console.WriteLine("Skrev: " + fullOut);
        }
        else
        {
            Console.Write(text);
        }

        return 0;
    }

    private static List<string> HämtaInKolumnRubriker(List<InBudget> inPoster)
    {
        var keys = new List<string>();
        foreach (var p in inPoster.OrderBy(x => x.YearAndMonth))
        {
            var key = Transaction.GetYearMonthName(p.YearAndMonth);
            if (!keys.Contains(key))
            {
                keys.Add(key);
            }
        }

        return keys;
    }

    private static void AppendIncomeSection(
        Action<string> w,
        List<string> månadsRubriker,
        List<Rad> rader)
    {
        foreach (var rad in rader)
        {
            if (string.Equals(rad.RadNamnY?.Trim(), InBudgetHandler.SummaText, StringComparison.Ordinal))
            {
                continue;
            }

            var cells = new List<string> { rad.RadNamnY ?? "" };
            foreach (var h in månadsRubriker.OrderBy(Transaction.GetDateFromYearMonthName))
            {
                rad.Kolumner.TryGetValue(h, out var v);
                cells.Add(v.ToString("# ##0", CultureInfo.InvariantCulture));
            }

            w(string.Join("\t", cells));
        }
    }

    private static void AppendIncomeStyleTable(
        Action<string> w,
        List<string> månadsRubriker,
        List<Rad> rader,
        bool skipCategoryDash)
    {
        foreach (var rad in rader)
        {
            if (skipCategoryDash && string.Equals(rad.RadNamnY?.Trim(), "-", StringComparison.Ordinal))
            {
                continue;
            }

            var cells = new List<string> { rad.RadNamnY ?? "" };
            foreach (var h in månadsRubriker.OrderBy(Transaction.GetDateFromYearMonthName))
            {
                rad.Kolumner.TryGetValue(h, out var v);
                cells.Add(v.ToString("# ##0", CultureInfo.InvariantCulture));
            }

            w(string.Join("\t", cells));
        }
    }

    private static void AppendStructuredBudgetTable(Action<string> w, TextToTableOutPuter table)
    {
        var builder = new BudgetStructureBuilder();
        var structured = builder.BuildStructuredBudget(table.BudgetRows, table.ColumnHeaders);
        var monthCols = table.ColumnHeaders
            .Where(h => !h.Contains("Category", StringComparison.OrdinalIgnoreCase)
                        && !h.Contains("Average", StringComparison.OrdinalIgnoreCase))
            .ToList();

        w(string.Join("\t", new[] { "Kategori" }.Concat(monthCols).Concat(new[] { "Summa" })));

        foreach (var row in structured.Rows)
        {
            var cells = new List<string> { row.CategoryText ?? "" };
            double rowTotal = 0;
            foreach (var col in monthCols)
            {
                row.AmountsForMonth.TryGetValue(col, out var v);
                rowTotal += v;
                cells.Add(v.ToString("N0", SvSe));
            }

            cells.Add(rowTotal.ToString("N0", SvSe));
            w(string.Join("\t", cells));
        }
    }

    /// <summary>
    /// Utgiftsrader som i UI-tabellen före första summering / tomrad (samma ordning som strukturbyggaren).
    /// </summary>
    private static List<BudgetRow> GetExpenseBudgetRowsForKvar(TextToTableOutPuter table)
    {
        var builder = new BudgetStructureBuilder();
        var structured = builder.BuildStructuredBudget(table.BudgetRows, table.ColumnHeaders);
        var list = new List<BudgetRow>();
        foreach (var row in structured.Rows)
        {
            if (string.IsNullOrWhiteSpace(row.CategoryText))
            {
                break;
            }

            if (row.CategoryText.Contains("===", StringComparison.Ordinal))
            {
                break;
            }

            if (string.Equals(row.CategoryText.Trim(), "-", StringComparison.Ordinal))
            {
                continue;
            }

            list.Add(row);
        }

        return list;
    }
}
