using System;
using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utilities;

namespace UtilitiesTest
{
    /// <summary>
    ///This is a test class for LoggerTest and is intended
    ///to contain all LoggerTest Unit Tests
    ///</summary>
    [TestClass]
    public class LoggerTest
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
        ///A test for WriteToWorkBook
        ///</summary>
        [TestMethod]
        public void WriteToWorkBookTest()
        {
            var excelBookPath = @"C:\1.xls";
            var sheetName = "Blad1";
            Logger.OperationToPerformOnBook operation = null;
            var rowToWrite = new object[] { "Testar", "kl:", DateTime.Now }; // .ToString()
            var overWrite = true;
            Hashtable rowsToWrite = null;
            var expected = -1;
            int actual;
            actual = Logger.WriteToWorkBook(excelBookPath, sheetName, operation, rowToWrite, overWrite, rowsToWrite);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for WriteToWorkBook
        ///</summary>
        [TestMethod]
        public void WriteToWorkBookTest1()
        {
            var excelBookPath = @"C:\1.xls";
            var sheetName = "Sheet1";

            // Logger.OperationToPerformOnBook operation = null;
            // var rowToWrite = new object[] { "Testar", "kl:", DateTime.Now };//.ToString()
            var overWrite = true;

            var rowsToWrite = new Hashtable
                              {
                                  { 1, new object[] { "afdf", "test", 7, DateTime.Now } }, 
                                  { 2, new object[] { "tvåan", "test", 2, DateTime.Now.AddHours(1) } }
                              };
            var expected = -1;
            int actual;
            actual = Logger.WriteToWorkBook(excelBookPath, sheetName, overWrite, rowsToWrite);
            Assert.AreEqual(expected, actual);
        }
    }
}