using Budgetterarn;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestBudgetterarn
{
    /// <summary>
    ///This is a test class for KontoEntryTest and is intended
    ///to contain all KontoEntryTest Unit Tests
    ///</summary>
    [TestClass]
    public class KontoEntryTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext)
        // {
        // }
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup()
        // {
        // }
        // Use TestInitialize to run code before running each test
        // [TestInitialize()]
        // public void MyTestInitialize()
        // {
        // }
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup()
        // {
        // }
        #endregion

        /// <summary>
        ///A test for RowThatExists
        ///</summary>
        [TestMethod]
        [DeploymentItem("Budgetterarn.exe")]
        public void RowThatExistsTest()
        {
            var target = new BudgeterForm(); // TODO: Initialize to an appropriate value
            var inArray = new[] { "test1", "223" };
            var columnNumber = 1; // TODO: Initialize to an appropriate value
            var expected = "223"; // TODO: Initialize to an appropriate value
            string actual;

            var ke = new Budgeter.Core.Entities.KontoEntry();
            actual = ke.RowThatExists(inArray, columnNumber);

            Assert.AreEqual(expected, actual);
        }
    }
}