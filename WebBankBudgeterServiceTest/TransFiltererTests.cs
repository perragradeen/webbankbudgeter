using WebBankBudgeterService;
using WebBankBudgeterService.Model;

namespace WebBankBudgeterServiceTest;

[TestClass]
public class TransFiltererTests
{
    [TestMethod]
    public void FilterTransactions_SelectedYear_ExcludesAdjacentCalendarYears()
    {
        var list = new TransactionList
        {
            Transactions =
            [
                new Transaction { DateAsDate = new DateTime(2013, 12, 31), AmountAsDouble = -100 },
                new Transaction { DateAsDate = new DateTime(2014, 6, 15), AmountAsDouble = -50 },
                new Transaction { DateAsDate = new DateTime(2015, 1, 1), AmountAsDouble = -20 }
            ]
        };

        var filtered = TransFilterer.FilterTransactions(list, 2014);

        Assert.AreEqual(1, filtered.Transactions.Count);
        Assert.AreEqual(2014, filtered.Transactions[0].DateAsDate.Year);
        Assert.AreEqual(6, filtered.Transactions[0].DateAsDate.Month);
    }
}
