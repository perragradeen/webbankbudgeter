using BudgeterCore.Entities;

namespace BudgetterarnUiTest
{
    [TestClass]
    public class KontoEntryTest
    {
        [TestMethod]
        public void RowThatExistsTest()
        {
            var inArray = new object[] { "test1", "223" };
            const int columnNumber = 1;
            const string expected = "223";

            var ke = new KontoEntry();
            var actual = ke.RowThatExists(inArray, columnNumber);

            Assert.AreEqual(expected, actual);
        }
    }
}