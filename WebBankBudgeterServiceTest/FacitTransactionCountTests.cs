using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebBankBudgeterTests.Facit;

namespace WebBankBudgeterServiceTest;

/// <summary>
/// M0 (del): antal transaktioner i fryst facit-JSON ska stämma med
/// <c>WebBankBudgeterTests.Facit/Facit/README.md</c> (samma underlag som Excel-källan).
/// Full inläsning via <c>TransactionHandler</c> från <c>.xls</c> kräver lokal fil — se <c>plan.md</c>.
/// </summary>
[TestClass]
public class FacitTransactionCountTests
{
    [TestMethod]
    public void FacitJson_2014_Has809Transactions()
    {
        Assert.HasCount(809, FacitLoader.LoadTransactions(2014));
    }

    [TestMethod]
    public void FacitJson_2015_Has845Transactions()
    {
        Assert.HasCount(845, FacitLoader.LoadTransactions(2015));
    }

    [TestMethod]
    public void FacitJson_Combined2014And2015_Has1654Transactions()
    {
        var n2014 = FacitLoader.LoadTransactions(2014).Count;
        var n2015 = FacitLoader.LoadTransactions(2015).Count;
        Assert.AreEqual(1654, n2014 + n2015, "809 + 845 enligt facit-README");
    }
}
