using WebBankBudgeter.Service.Model;

namespace WebBankBudgeter.Service.TransactionTests
{
    [TestClass]
    public class TransactionMonthTests
    {
        [TestMethod]
        public void DateTest()
        {
            var f�rv�ntatDatum = new DateTime(2021, 1, 1);
            var datum = Transaction.GetDateFromYearMonthName("2021 January");

            Assert.AreEqual(f�rv�ntatDatum.Year, datum.Year);
            Assert.AreEqual(f�rv�ntatDatum.Month, datum.Month);
        }
    }
}