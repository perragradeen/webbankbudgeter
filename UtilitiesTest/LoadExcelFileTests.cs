using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utilities;

namespace UtilitiesTest
{
    [TestClass]
    public class LoadExcelFileTests
    {
        private static string ExcelBookPath => Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            //@"\Temp\pelles budget.xls"
            @"..\..\..\Budgetterarn\bin\pelles budget.xls"
        );
        public const string SheetName = "Kontoutdrag_officiella";

        [TestMethod]
        public void ReadWorkBookTest()
        {
            var book = OpenFileFunctions.GetHashTableFromExcelSheet(
                ExcelBookPath,
                SheetName);
            var table = (Hashtable)book[SheetName];

            Assert.IsTrue(table.Count > 0);
        }

        [TestMethod, Ignore]
        public void ReadWorkBookPerformanceTest()
        {
            const string sheetName = SheetName;
            var timeTakenLadda1Blad = Load1Sheet(sheetName);
            var foo = "Time taken: " + timeTakenLadda1Blad.ToString(@"m\:ss\.fff");

            var timeTakenLaddaAllt = LoadAllSheets();
            var foo2 = "Time taken: " + timeTakenLaddaAllt.ToString(@"m\:ss\.fff");

            Assert.IsTrue(timeTakenLaddaAllt > timeTakenLadda1Blad);
        }

        private static TimeSpan Load1Sheet(string sheetName)
        {
            var timer = new Stopwatch();

            timer.Start();
            OpenFileFunctions.GetHashTableFromExcelSheet(
                ExcelBookPath,
                sheetName);

            timer.Stop();

            return timer.Elapsed;
        }

        private static TimeSpan LoadAllSheets()
        {
            var timer = new Stopwatch();

            timer.Start();
            OpenFileFunctions.GetHashTableFromExcelSheet(
                ExcelBookPath,
                onlyLoadSelectedSheetName: false);

            timer.Stop();

            return timer.Elapsed;
        }
    }
}