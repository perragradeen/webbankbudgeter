using WebBankBudgeterService;
using WebBankBudgeterService.Services;

namespace WebBankBudgeterServiceTest;

/// <summary>
/// M0: Verifierar <see cref="TransactionHandler"/> mot Excel i repot.
/// <list type="bullet">
/// <item><c>pelles budget.xls</c> — arbetskopia (nyare år, ej 2014-facit).</item>
/// <item><c>Pelles-budget-slim-2014-2015-gform.xlsx</c> — samma källa som facit-JSON (809/845).</item>
/// </list>
/// </summary>
[TestClass]
public sealed class TransactionHandlerM0Tests
{
    private static string RepoRoot =>
        Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));

    private static string PellesBudgetXlsPath =>
        Path.Combine(RepoRoot, "pelles budget.xls");

    private static string FacitSlimXlsxPath =>
        Path.Combine(RepoRoot, "Pelles-budget-slim-2014-2015-gform.xlsx");

    private static string CategoryPath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Categories.xml");

    private static TransactionHandler CreateHandler(string excelPath)
    {
        if (!File.Exists(excelPath))
            throw new AssertInconclusiveException($"Saknar transaktionsfil: {excelPath}");
        if (!File.Exists(CategoryPath))
            throw new AssertInconclusiveException($"Saknar Categories.xml: {CategoryPath}");

        return new TransactionHandler(
            _ => { },
            new TableGetter { AddAverageColumn = true },
            CategoryPath,
            excelPath);
    }

    [TestMethod]
    public void M0_PellesBudgetXls_ExistsAtRepoRoot()
    {
        Assert.IsTrue(File.Exists(PellesBudgetXlsPath), PellesBudgetXlsPath);
    }

    /// <summary>
    /// <c>pelles budget.xls</c> i repot är en levande export (2018–2023), inte slim-facit 2014–2015.
    /// </summary>
    [TestMethod]
    public async Task M0_PellesBudgetXls_WorkingCopy_Loads_WithExpectedYearSpan()
    {
        var handler = CreateHandler(PellesBudgetXlsPath);
        var ok = await handler.GetTransactionsAsync();
        Assert.IsTrue(ok, "GetTransactionsAsync ska lyckas");
        var all = handler.TransactionList!.Transactions.ToList();
        Assert.IsTrue(all.Count > 1000);

        var years = all.Select(t => t.DateAsDate.Year).Distinct().OrderBy(y => y).ToList();
        var minY = years.First();
        var maxY = years.Last();

        // Uppdatera intervallet om arbetsfilen byts till annan period.
        Assert.IsTrue(minY <= 2018 && maxY >= 2022,
            $"Förväntat ungefärligt spann 2018–2023, fick {minY}–{maxY}. " +
            $"Histogram: {string.Join(", ", years.Select(y => $"{y}×{all.Count(t => t.DateAsDate.Year == y)}"))}");

        Assert.AreEqual(0, all.Count(t => t.DateAsDate.Year == 2014),
            "Arbetsfilen ska inte innehålla 2014 om den inte bytts ut mot facit-export.");
    }

    /// <summary>
    /// Facit (JSON under <c>WebBankBudgeterTests.Facit</c>) byggdes ur denna xlsx.
    /// </summary>
    [TestMethod]
    public async Task M0_SlimGformXlsx_MatchesFacitTransactionCounts()
    {
        if (!File.Exists(FacitSlimXlsxPath))
        {
            Assert.Inconclusive($"Saknas (lägg facit-filen i repo-root): {FacitSlimXlsxPath}");
        }

        var handler = CreateHandler(FacitSlimXlsxPath);
        var ok = await handler.GetTransactionsAsync();
        Assert.IsTrue(ok, "GetTransactionsAsync ska lyckas för slim xlsx");
        var all = handler.TransactionList!.Transactions.ToList();

        var y2014 = all.Count(t => t.DateAsDate.Year == 2014);
        var y2015 = all.Count(t => t.DateAsDate.Year == 2015);
        var hist = string.Join(", ",
            all.GroupBy(t => t.DateAsDate.Year).OrderBy(g => g.Key)
                .Select(g => $"{g.Key}×{g.Count()}"));

        const int facit2014 = 809;
        const int facit2015 = 845;

        Assert.AreEqual(facit2014, y2014,
            $"2014: förväntat {facit2014} (transactions-2014.json), fick {y2014}. Histogram: {hist}");
        Assert.AreEqual(facit2015, y2015,
            $"2015: förväntat {facit2015} (transactions-2015.json), fick {y2015}.");
    }
}
