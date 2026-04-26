using InbudgetHandler;

namespace WebBankBudgeterServiceTest;

[TestClass]
public class FacitBudgetInLoaderTests
{
    [TestMethod]
    public void Load_FiltersByYear()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Facit", "budget-in-2014.json");
        if (!File.Exists(path))
        {
            Assert.Inconclusive($"Saknar {path} (kör från test-output med facit kopierad).");
        }

        var list = FacitBudgetInLoader.Load(path, year: 2014);
        Assert.IsTrue(list.Count > 0);
        Assert.IsTrue(list.All(x => x.YearAndMonth.Year == 2014));
    }
}
