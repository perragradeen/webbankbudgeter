using InbudgetHandler;
using InbudgetHandler.Model;
using WebBankBudgeterService.Model;
using WebBankBudgeterService.Services;
using WebBankBudgeterTests.Facit;

namespace WebBankBudgeterServiceTest;

[TestClass]
public class InBudgetMathSnurraIgenomTests
{
    private const double Tolerance = 0.01;

    [TestMethod]
    public void SnurraIgenom_IncludesCategory_WhenOnlyUtHasRows()
    {
        var monthKey = FacitBudgetTextTableFactory.MonthKey(2014, 3);
        var utRow = new BudgetRow { CategoryText = "only-ut" };
        utRow.AmountsForMonth[monthKey] = -100;
        var ut = new List<BudgetRow> { utRow };

        var inRader = new List<Rad>
        {
            new() { RadNamnY = "has-budget", Kolumner = { [monthKey] = 50 } }
        };

        var log = new List<string>();
        var result = InBudgetMath.SnurraIgenom(inRader, ut, log.Add);

        var byCat = result.ToDictionary(r => r.RadNamnY.Trim(), StringComparer.Ordinal);
        Assert.IsTrue(byCat.ContainsKey("only-ut"));
        Assert.AreEqual(-100, byCat["only-ut"].Kolumner[monthKey], Tolerance);

        Assert.IsTrue(byCat.ContainsKey("has-budget"));
        Assert.AreEqual(50, byCat["has-budget"].Kolumner[monthKey], Tolerance);
        Assert.IsTrue(log.Count == 0);
    }

    [TestMethod]
    [DataRow(2014)]
    [DataRow(2015)]
    public void SnurraIgenom_PreInMergeUt_MatchesExpectedKvar_Facit(int year)
    {
        var budgetIn = FacitLoader.LoadBudgetIn(year);
        var ut = FacitLoader.LoadExpectedUt(year);
        var transfers = FacitLoader.LoadExpectedTransfers(year);
        var expectedKvar = FacitLoader.LoadExpectedKvar(year);

        var utAmounts = ut
            .Select(u => (u.Category, u.Year, u.Month, u.ActualAmount))
            .Concat(transfers.Select(t => (t.Category, t.Year, t.Month, t.ActualAmount)));

        var table = FacitBudgetTextTableFactory.Build(year, utAmounts, addAverageColumns: true);
        var utRows = table.BudgetRows!.ToList();

        var inRader = BudgetInRowsFromFacit(budgetIn);
        var log = new List<string>();
        var kvarRader = InBudgetMath.SnurraIgenom(inRader, utRows, log.Add);

        var actual = new Dictionary<(string Cat, string MonthKey), double>();
        foreach (var rad in kvarRader)
        {
            var cat = rad.RadNamnY.Trim();
            foreach (var (mk, v) in rad.Kolumner)
            {
                actual[(cat, mk)] = v;
            }
        }

        foreach (var k in expectedKvar)
        {
            var mk = FacitBudgetTextTableFactory.MonthKey(k.Year, k.Month);
            var key = (k.Category.Trim(), mk);
            Assert.IsTrue(actual.TryGetValue(key, out var got),
                $"Saknar Kvar för {k.Category} {k.Year}-{k.Month}");
            Assert.AreEqual(k.Remaining, got, Tolerance,
                $"Kvar mismatch {k.Category} {k.Year}-{k.Month}");
        }

        Assert.IsTrue(log.Count == 0, "Oväntade loggrader: " + string.Join("; ", log));
    }

    private static List<Rad> BudgetInRowsFromFacit(IEnumerable<BudgetInFacit> budgetIn)
    {
        var list = new List<Rad>();
        foreach (var g in budgetIn.GroupBy(b => b.Category, StringComparer.Ordinal))
        {
            var rad = new Rad { RadNamnY = g.Key };
            foreach (var item in g)
            {
                var mk = FacitBudgetTextTableFactory.MonthKey(item.Year, item.Month);
                rad.Kolumner.TryGetValue(mk, out var cur);
                rad.Kolumner[mk] = cur + (double)item.BudgetAmount;
            }

            list.Add(rad);
        }

        return list.OrderBy(r => r.RadNamnY, StringComparer.Ordinal).ToList();
    }
}
