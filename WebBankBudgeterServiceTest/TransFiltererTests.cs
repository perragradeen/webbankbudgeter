using WebBankBudgeterService;
using WebBankBudgeterService.Model;

namespace WebBankBudgeterServiceTest
{
    [TestClass]
    public class TransFiltererTests
    {
        [TestMethod]
        public void FilterTransactions_ByYear_ExcludesAdjacentYearEvenIfInCalendarRange()
        {
            var list = new TransactionList
            {
                Transactions =
                [
                    new Transaction
                    {
                        DateAsDate = new DateTime(2014, 12, 31),
                        Description = "same year end",
                        Amount = -1.0,
                        Categorizations = new Categorizations { Categories = [] }
                    },
                    new Transaction
                    {
                        DateAsDate = new DateTime(2013, 12, 31),
                        Description = "dec previous year",
                        Amount = -2.0,
                        Categorizations = new Categorizations { Categories = [] }
                    },
                    new Transaction
                    {
                        DateAsDate = new DateTime(2015, 1, 1),
                        Description = "jan next year",
                        Amount = -3.0,
                        Categorizations = new Categorizations { Categories = [] }
                    }
                ]
            };

            var filtered = TransFilterer.FilterTransactions(list, 2014);

            Assert.AreEqual(1, filtered.Transactions.Count);
            Assert.AreEqual("same year end", filtered.Transactions[0].Description);
        }
    }
}
