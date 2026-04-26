using WebBankBudgeterService.Model;
using WebBankBudgeterService.Model.ViewModel;
using WebBankBudgeterService.Services;

namespace WebBankBudgeterServiceTest;

[TestClass]
public class BudgetStructureBuilderExpenseRowsTests
{
    [TestMethod]
    public void GetExpenseRowsBeforeFirstSummary_StopsAtFirstSummary()
    {
        var headers = new List<string>
        {
            TextToTableOutPuter.CategoryNameColumnDescription,
            "2014 January"
        };

        var el = new BudgetRow { CategoryText = "el" };
        el.AmountsForMonth["2014 January"] = -100;

        var structured = new BudgetStructureBuilder().BuildStructuredBudget(new List<BudgetRow> { el }, headers);
        var expenseOnly = BudgetStructureBuilder.GetExpenseRowsBeforeFirstSummary(structured);

        Assert.AreEqual(1, expenseOnly.Count);
        Assert.AreEqual("el", expenseOnly[0].CategoryText);
    }
}
