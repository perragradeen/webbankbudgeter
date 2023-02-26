using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestBudgetterarn
{
    [TestClass]
    public class KontoEntryTest
    {
        [TestMethod]
        public void RowThatExistsTest()
        {
            var inArray = new object[] {"test1", "223"};
            const int columnNumber = 1;
            const string expected = "223";

            var ke = new Budgeter.Core.Entities.KontoEntry();
            var actual = ke.RowThatExists(inArray, columnNumber);

            Assert.AreEqual(expected, actual);
        }
    }
}