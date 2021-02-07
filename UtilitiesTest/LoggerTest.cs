using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.IO;
using Utilities;

namespace UtilitiesTest
{
    [TestClass]
    public class LoggerTest
    {
        public string ExcelBookPath => Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"TestFiles\1.xls"
                );

        [TestMethod]
        public void WriteToWorkBookTest()
        {
            var sheetName = "Blad1";
            Logger.OperationToPerformOnBook operation = null;
            var rowToWrite = new object[] { "Testar", "kl:", DateTime.Now }; // .ToString()
            var overWrite = true;
            Hashtable rowsToWrite = null;
            var expected = -1;
            int actual;
            actual = Logger.WriteToWorkBook(ExcelBookPath, sheetName, operation, rowToWrite, overWrite, rowsToWrite);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void WriteToWorkBookTest1()
        {
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
            actual = Logger.WriteToWorkBook(ExcelBookPath, sheetName, overWrite, rowsToWrite);
            Assert.AreEqual(expected, actual);
        }
    }
}