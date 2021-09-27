using System;
using System.Collections;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utilities;

namespace UtilitiesTest
{
    [TestClass]
    public class LoggerTest
    {
        private static string ExcelBookPath => Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            @"TestFiles\1.xls"
        );

        [TestMethod]
        public void WriteToWorkBookTest()
        {
            const string sheetName = "Blad1";
            var rowToWrite = new object[]
                {"Testar", "kl:", DateTime.Now};

            const int expected = -1;

            var actual =
                Logger.WriteToWorkBook(
                    ExcelBookPath,
                    sheetName,
                    operation: null,
                    rowToWrite,
                    overWrite: true,
                    rowsToWrite: null);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void WriteToWorkBookTest1()
        {
            const string sheetName = "Sheet1";

            var rowsToWrite = new Hashtable
            {
                {1, new object[] {"afdf", "test", 7, DateTime.Now}},
                {2, new object[] {"tvåan", "test", 2, DateTime.Now.AddHours(1)}}
            };

            const int expected = -1;

            var actual =
                Logger.WriteToWorkBook(
                    ExcelBookPath,
                    sheetName,
                    overWrite: true,
                    rowsToWrite);

            Assert.AreEqual(expected, actual);
        }
    }
}