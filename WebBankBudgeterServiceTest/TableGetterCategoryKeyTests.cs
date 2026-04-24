using BudgeterCore.Entities;
using WebBankBudgeterService.Model;
using WebBankBudgeterService.Services;

namespace WebBankBudgeterServiceTest;

[TestClass]
public class TableGetterCategoryKeyTests
{
    [TestMethod]
    public void BudgetTableCategoryKey_UsesPlainName_WhenGroupIsEmpty()
    {
        var t = new Transaction
        {
            DateAsDate = new DateTime(2014, 3, 1),
            AmountAsDouble = -100,
            Categorizations = new Categorizations
            {
                Categories = new List<Categories>
                {
                    new Categories { Group = "", Name = "el" }
                }
            }
        };

        Assert.AreEqual("el", t.BudgetTableCategoryKey);
        StringAssert.StartsWith(t.CategoryName, " ");
    }

    [TestMethod]
    public void GroupOnMonthAndCategory_ExcludesIgnoreEntryType()
    {
        var march2014 = new DateTime(2014, 3, 15);
        var transactions = new List<Transaction>
        {
            MakeTx(march2014, "el", group: "", amount: -100, KontoEntryType.Regular),
            MakeTx(march2014, "el", group: "", amount: -50, KontoEntryType.Ignore)
        };

        var grouped = TableGetter.GroupOnMonthAndCategory(transactions)!.ToList();
        Assert.AreEqual(1, grouped.Count);
        var sum = grouped[0].Sum(x => x.AmountAsDouble);
        Assert.AreEqual(-100, sum, 0.001);
    }

    [TestMethod]
    public void GetMonthAsFullString_UsesInvariantEnglishMonth()
    {
        var dt = new DateTime(2014, 1, 15);
        var s = Transaction.GetMonthAsFullString(dt);
        Assert.AreEqual("January", s);
    }

    private static Transaction MakeTx(DateTime date, string name, string group, double amount,
        KontoEntryType entryType)
    {
        return new Transaction
        {
            DateAsDate = date,
            AmountAsDouble = amount,
            SourceEntryType = entryType,
            Categorizations = new Categorizations
            {
                Categories = new List<Categories>
                {
                    new Categories { Group = group, Name = name }
                }
            }
        };
    }
}
