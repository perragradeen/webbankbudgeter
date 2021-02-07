using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestBudgetterarn
{
    [TestClass]
    public class KontoEntryTest
    {
        [TestMethod]
        public void RowThatExistsTest()
        {
            var inArray = new[] { "test1", "223" };
            var columnNumber = 1;
            var expected = "223";
            string actual;

            var ke = new Budgeter.Core.Entities.KontoEntry();
            actual = ke.RowThatExists(inArray, columnNumber);

            Assert.AreEqual(expected, actual);
        }
    }
}