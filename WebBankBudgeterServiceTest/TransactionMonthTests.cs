using WebBankBudgeterService.Model;

namespace WebBankBudgeterServiceTest
{
    [TestClass]
    public class TransactionMonthTests
    {
        [TestMethod]
        public void DateTest()
        {
            var förväntatDatum = new DateTime(2021, 1, 1);
            var datum = Transaction.GetDateFromYearMonthName("2021 January");

            Assert.AreEqual(förväntatDatum.Year, datum.Year);
            Assert.AreEqual(förväntatDatum.Month, datum.Month);
        }
    }
}