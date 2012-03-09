using Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;

namespace UtilitiesTest
{
    
    
    /// <summary>
    ///This is a test class for LoggerTest and is intended
    ///to contain all LoggerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LoggerTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for WriteToWorkBook
        ///</summary>
        [TestMethod()]
        public void WriteToWorkBookTest()
        {
            string excelBookPath = @"C:\1.xls";
            string sheetName = "Blad1";
            Logger.OperationToPerformOnBook operation = null;
            var rowToWrite = new object[] { "Testar", "kl:", DateTime.Now };//.ToString()
            bool overWrite = true;
            Hashtable rowsToWrite = null;
            int expected = -1;
            int actual;
            actual = Logger.WriteToWorkBook(excelBookPath, sheetName, operation, rowToWrite, overWrite, rowsToWrite);
            Assert.AreEqual(expected, actual);
        }


        /// <summary>
        ///A test for WriteToWorkBook
        ///</summary>
        [TestMethod()]
        public void WriteToWorkBookTest1()
        {
            string excelBookPath = @"C:\1.xls";
            string sheetName = "Sheet1";
            //Logger.OperationToPerformOnBook operation = null;
            //var rowToWrite = new object[] { "Testar", "kl:", DateTime.Now };//.ToString()
            bool overWrite = true;

            Hashtable rowsToWrite = new Hashtable { 
                {1, new object[] {"afdf", "test", 7, DateTime.Now}}
                , {2, new object[] {"tvåan", "test", 2, DateTime.Now.AddHours(1)}}
            };
            int expected = -1;
            int actual;
            actual = Logger.WriteToWorkBook(excelBookPath, sheetName, overWrite, rowsToWrite);
            Assert.AreEqual(expected, actual);
        }
    }
}
