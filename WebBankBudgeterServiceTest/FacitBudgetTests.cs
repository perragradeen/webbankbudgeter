using System.Globalization;
using InbudgetHandler;
using WebBankBudgeterService;
using InbudgetHandler.Model;
using WebBankBudgeterService.Model;
using WebBankBudgeterService.Model.ViewModel;
using WebBankBudgeterService.Services;
using WebBankBudgeterTests.Facit;

namespace WebBankBudgeterServiceTest;

[TestClass]
public class FacitBudgetTests
{
    private const double Tolerance = 0.02;

    [TestMethod]
    public void Facit_FileCounts_AreStable()
    {
        Assert.AreEqual(809, FacitLoader.LoadTransactions(2014).Count);
        Assert.AreEqual(845, FacitLoader.LoadTransactions(2015).Count);
        Assert.AreEqual(374, FacitLoader.LoadBudgetIn(2014).Count);
        Assert.AreEqual(408, FacitLoader.LoadBudgetIn(2015).Count);
        Assert.AreEqual(222, FacitLoader.LoadExpectedUt(2014).Count);
        Assert.AreEqual(214, FacitLoader.LoadExpectedUt(2015).Count);
        Assert.AreEqual(9, FacitLoader.LoadExpectedTransfers(2014).Count);
        Assert.AreEqual(6, FacitLoader.LoadExpectedTransfers(2015).Count);
        Assert.AreEqual(402, FacitLoader.LoadExpectedKvar(2014).Count);
        Assert.AreEqual(420, FacitLoader.LoadExpectedKvar(2015).Count);
    }

    [TestMethod]
    public void AggregationFromTransactions_MatchesExpectedUt_2014() =>
        AssertAggregationMatchesExpectedUt(2014);

    [TestMethod]
    public void AggregationFromTransactions_MatchesExpectedUt_2015() =>
        AssertAggregationMatchesExpectedUt(2015);

    [TestMethod]
    public void IgnoreFlag_IsExcludedFromAggregation()
    {
        var all = FacitLoader.LoadTransactions(2014);
        var withIgnore = all.Select(Clone).ToList();
        var firstRegular = withIgnore.First(t =>
            string.Equals(t.Flag, "Regular", StringComparison.OrdinalIgnoreCase));
        var extra = Clone(firstRegular);
        extra = extra with { Description = "SYNTH_IGNORE_TEST", Flag = "Ignore", Amount = 9999.99 };
        withIgnore.Add(extra);

        var tableFull = BuildTableFromFacitTransactions(withIgnore);
        var tableFiltered = BuildTableFromFacitTransactions(all);

        CollectionAssert.AreEqual(
            SortedFlatten(tableFiltered),
            SortedFlatten(tableFull));
    }

    [TestMethod]
    public void KvarCalculation_InPlusUt_EqualsExpectedKvar_2014() =>
        AssertKvarMatchesFacit(2014);

    [TestMethod]
    public void KvarCalculation_InPlusUt_EqualsExpectedKvar_2015() =>
        AssertKvarMatchesFacit(2015);

    [TestMethod]
    public void MonthKey_MatchesFacitFormat()
    {
        var prev = Thread.CurrentThread.CurrentCulture;
        var prevUi = Thread.CurrentThread.CurrentUICulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("sv-SE");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("sv-SE");
            var key = Transaction.GetYearMonthName(new DateTime(2014, 1, 1));
            Assert.AreEqual("2014 January", key);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = prev;
            Thread.CurrentThread.CurrentUICulture = prevUi;
        }
    }

    [TestMethod]
    public void CategoryNormalization_BudgetTableCategoryKey_UsesNameWhenGroupEmpty()
    {
        var t = new Transaction
        {
            DateAsDate = new DateTime(2014, 3, 1),
            Description = "test",
            Amount = -10.0,
            Categorizations = new Categorizations
            {
                Categories = new List<Categories>
                {
                    new() { Group = "", Name = "el" }
                }
            }
        };

        Assert.AreEqual("el", t.BudgetTableCategoryKey);
        Assert.AreEqual(" el", t.CategoryName);
    }

    [TestMethod]
    public void FilterTransactions_SelectedYear_ExcludesOtherCalendarYears()
    {
        var list = new TransactionList
        {
            Transactions =
            [
                new Transaction
                {
                    DateAsDate = new DateTime(2018, 12, 31),
                    Amount = 1,
                    Categorizations = new Categorizations { Categories = new List<Categories> { new() { Name = "a", Group = "" } } }
                },
                new Transaction
                {
                    DateAsDate = new DateTime(2019, 1, 1),
                    Amount = 2,
                    Categorizations = new Categorizations { Categories = new List<Categories> { new() { Name = "b", Group = "" } } }
                }
            ]
        };

        var f = TransFilterer.FilterTransactions(list, 2019);
        Assert.AreEqual(1, f.Transactions.Count);
        Assert.AreEqual(2, f.Transactions[0].AmountAsDouble);
    }

    private static void AssertAggregationMatchesExpectedUt(int year)
    {
        var transactions = FacitLoader.LoadTransactions(year)
            .Where(t => string.Equals(t.Flag, "Regular", StringComparison.OrdinalIgnoreCase))
            .Where(t => !string.Equals(t.Category.Trim(), " -", StringComparison.Ordinal))
            .Where(t => !string.Equals(t.Category.Trim(), "-", StringComparison.Ordinal))
            .Select(ToTransaction)
            .ToList();

        var table = new TableGetter { AddAverageColumn = false }
            .GetTextTableFromTransactions(transactions);

        var flat = FlattenTable(table);

        foreach (var expected in FacitLoader.LoadExpectedUt(year))
        {
            var monthKey = Transaction.GetYearMonthName(new DateTime(expected.Year, expected.Month, 1));
            var cat = expected.Category.Trim();
            var key = (cat, monthKey);
            flat.TryGetValue(key, out var actual);
            Assert.AreEqual(
                expected.ActualAmount,
                actual,
                Tolerance,
                $"Kat {cat} {monthKey}");
        }

        foreach (var kv in flat)
        {
            var (cat, monthKey) = kv.Key;
            var dt = Transaction.GetDateFromYearMonthName(monthKey);
            var match = FacitLoader.LoadExpectedUt(year).Any(e =>
                e.Category.Trim() == cat &&
                e.Year == dt.Year &&
                e.Month == dt.Month &&
                Math.Abs(e.ActualAmount - kv.Value) <= Tolerance);
            Assert.IsTrue(match, $"Extra cell i tabell: {cat} {monthKey} = {kv.Value}");
        }
    }

    private static void AssertKvarMatchesFacit(int year)
    {
        var inRader = BudgetInToRader(FacitLoader.LoadBudgetIn(year));
        var utRows = ExpectedUtToBudgetRows(FacitLoader.LoadExpectedUt(year));
        var kvar = InBudgetKvarCalculator.SnurraIgenom(inRader, utRows);

        foreach (var exp in FacitLoader.LoadExpectedKvar(year))
        {
            var monthKey = Transaction.GetYearMonthName(new DateTime(exp.Year, exp.Month, 1));
            var rad = kvar.FirstOrDefault(r => r.RadNamnY.Trim() == exp.Category.Trim());
            Assert.IsNotNull(rad, $"Saknar kvar-rad för {exp.Category}");
            rad!.Kolumner.TryGetValue(monthKey, out var actual);
            Assert.AreEqual(
                exp.Remaining,
                actual,
                Tolerance,
                $"Kvar {exp.Category} {monthKey}");
        }

        foreach (var rad in kvar)
        {
            foreach (var kv in rad.Kolumner)
            {
                var dt = Transaction.GetDateFromYearMonthName(kv.Key);
                var exists = FacitLoader.LoadExpectedKvar(year).Any(e =>
                    e.Category.Trim() == rad.RadNamnY.Trim() &&
                    e.Year == dt.Year &&
                    e.Month == dt.Month);
                Assert.IsTrue(exists, $"Extra kvar: {rad.RadNamnY} {kv.Key}");
            }
        }
    }

    private static List<Rad> BudgetInToRader(List<BudgetInFacit> rows)
    {
        var list = new List<Rad>();
        foreach (var g in rows.GroupBy(r => r.Category.Trim()))
        {
            var rad = new Rad { RadNamnY = g.Key };
            foreach (var b in g)
            {
                var mk = Transaction.GetYearMonthName(new DateTime(b.Year, b.Month, 1));
                rad.Kolumner[mk] = b.BudgetAmount;
            }

            list.Add(rad);
        }

        return list;
    }

    private static List<BudgetRow> ExpectedUtToBudgetRows(List<BudgetUtFacit> rows)
    {
        var byCat = new Dictionary<string, BudgetRow>(StringComparer.Ordinal);
        foreach (var e in rows)
        {
            var cat = e.Category.Trim();
            if (!byCat.TryGetValue(cat, out var row))
            {
                row = new BudgetRow { CategoryText = cat };
                byCat[cat] = row;
            }

            var mk = Transaction.GetYearMonthName(new DateTime(e.Year, e.Month, 1));
            row.AmountsForMonth[mk] = e.ActualAmount;
        }

        return byCat.Values.ToList();
    }

    private static TextToTableOutPuter BuildTableFromFacitTransactions(List<TransactionFacit> facit) =>
        new TableGetter { AddAverageColumn = false }
            .GetTextTableFromTransactions(
                facit
                    .Where(t => string.Equals(t.Flag, "Regular", StringComparison.OrdinalIgnoreCase))
                    .Select(ToTransaction)
                    .ToList());

    private static Dictionary<(string Cat, string MonthKey), double> FlattenTable(TextToTableOutPuter table)
    {
        var d = new Dictionary<(string, string), double>();
        foreach (var row in table.BudgetRows)
        {
            var cat = row.CategoryText.Trim();
            foreach (var kv in row.AmountsForMonth)
                d[(cat, kv.Key)] = kv.Value;
        }

        return d;
    }

    private static List<KeyValuePair<(string Cat, string MonthKey), double>> SortedFlatten(TextToTableOutPuter t) =>
        FlattenTable(t).OrderBy(x => x.Key.Cat).ThenBy(x => x.Key.MonthKey).ToList();

    private static Transaction ToTransaction(TransactionFacit f) =>
        new()
        {
            DateAsDate = new DateTime(f.Year, f.Month, f.Day),
            Description = f.Description,
            Amount = f.Amount,
            Categorizations = new Categorizations
            {
                Categories = new List<Categories>
                {
                    new()
                    {
                        Group = "",
                        Name = f.Category
                    }
                }
            }
        };

    private static TransactionFacit Clone(TransactionFacit f) =>
        new(f.Year, f.Month, f.Day, f.Description, f.Amount, f.Category, f.Flag);
}
