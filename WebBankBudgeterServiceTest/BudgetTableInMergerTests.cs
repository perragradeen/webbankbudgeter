using InbudgetHandler;
using InbudgetHandler.Model;
using WebBankBudgeterService.Model;
using WebBankBudgeterService.Model.ViewModel;

namespace WebBankBudgeterServiceTest;

[TestClass]
public class BudgetTableInMergerTests
{
    [TestMethod]
    public void MergeInRows_AddsBudgetToMatchingExpenseRow()
    {
        var table = new TextToTableOutPuter();
        table.ColumnHeaders.Add(TextToTableOutPuter.CategoryNameColumnDescription);
        table.ColumnHeaders.Add("2014 January");

        var utRow = new BudgetRow { CategoryText = "el" };
        utRow.AmountsForMonth["2014 January"] = -433;
        table.BudgetRows = new List<BudgetRow> { utRow };

        var inRader = new List<Rad>
        {
            new Rad
            {
                RadNamnY = "el",
                Kolumner = new Dictionary<string, double> { ["2014 January"] = 200 }
            }
        };

        BudgetTableInMerger.MergeInRows(table, inRader);

        var merged = table.BudgetRows!.Single();
        Assert.AreEqual(-233, merged.AmountsForMonth["2014 January"], 0.001);
    }

    [TestMethod]
    public void MergeInRows_SkipsSummaRowFromInHandler()
    {
        var table = new TextToTableOutPuter();
        table.ColumnHeaders.Add(TextToTableOutPuter.CategoryNameColumnDescription);
        table.ColumnHeaders.Add("2014 January");
        var utRows = new List<BudgetRow> { new BudgetRow { CategoryText = "el" } };
        utRows[0].AmountsForMonth["2014 January"] = -100;
        table.BudgetRows = utRows;

        var inRader = new List<Rad>
        {
            new Rad { RadNamnY = InBudgetHandler.SummaText, Kolumner = { ["2014 January"] = 999 } }
        };

        BudgetTableInMerger.MergeInRows(table, inRader);

        var rows = table.BudgetRows!.ToList();
        Assert.AreEqual(-100, rows[0].AmountsForMonth["2014 January"], 0.001);
    }
}
