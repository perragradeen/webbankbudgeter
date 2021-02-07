using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SwedbankSharp.JsonSchemas;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBankBudgeter.Service;
using WebBankBudgeter.Service.Model;
using WebBankBudgeter.Service.Services;
using WebBankBudgeter.Service.Test;

namespace SwedbankSharpTests
{
    [TestClass]
    public class TransaktionTests
    {
        private const int NumberOfnewTestDatas = 60;
        private TableGetter _tableGetter;

        [TestMethod, Ignore]
        public async Task JuniÄrTom_MenBordeHaHandpenningTest()
        {
            var tableGetter = new TableGetter { AddAverageColumn = true };
            var transactionHandler = new TransactionHandler(
                NullWriteToOutput, tableGetter);

            await transactionHandler.GetTransactions();

            transactionHandler.SortTransactions();
            transactionHandler.RemoveDuplicates();

            var results = transactionHandler.TransactionList.Transactions;
            Assert.IsTrue(results.Any(t =>
                t.DateAsDate.Month == 6
                && t.DateAsDate.Year == 2016
                && t.AmountAsDouble > 0
            ));

            var junis = results.FirstOrDefault(
                t => t.DateAsDate.Month == 6
                && t.DateAsDate.Year == 2016
                && t.AmountAsDouble > 0);
            Assert.IsNotNull(junis);

            var table = transactionHandler.GetTextTableFromTransactions();

            Assert.IsTrue(table.BudgetRows.Any(row =>
                row.CategoryText == "No group No Category"
            ));

            var juniAmountsTotal = transactionHandler.TransactionList.Transactions.Where(
                    t => t.DateAsDate.Month == 6
                         && t.DateAsDate.Year == 2016)
                .Sum(t => t.AmountAsDouble);

            Assert.IsTrue(table.BudgetRows.Any(row =>
                row.AmountsForMonth.ContainsKey("2016 June") && 
                row.AmountsForMonth["2016 June"] == juniAmountsTotal
            ));
        }

        private void NullWriteToOutput(string obj)
        {
            return;
        }

        [TestMethod]
        public void TransaktionGroupingTest()
        {
            // Arrange
            var transactionList = GetTestDataFromCodeAndCheckDateFormat();

            // Gruppera på månad+categori
            var grouped = GetGroupedAndAssert(transactionList.Transactions).ToList();

            // Loopa grupper och skriv ut tabell x = datum-månad, y=grupp, summa amount i celler
            var table = _tableGetter.GetTextTableFromGroupedTransactions(grouped);

            Assert.AreEqual(14, grouped.Count);

            // 17 4 - 17 6 = 3 columner + 1 för info om cat
            Assert.AreEqual(3 + 1, table.ColumnHeaders.Count);

            // 6 typer av cats. (rad med headers för sig)
            //Assert.AreEqual(7, table.Rows.Count);

            //Assert.AreEqual("240.00", table.Rows[4][1]);
        }

        [TestMethod]
        public void TransaktionFromFileTest()
        {
            // Arrange
            var transactionList = GetTestDataFromFileAndCheckDateFormat();

            // Gruppera på månad+categori
            var grouped = GetGroupedAndAssert(transactionList.Transactions).ToList();

            // Loopa grupper och skriv ut tabell x = datum-månad, y=grupp, summa amount i celler
            var table = _tableGetter.GetTextTableFromGroupedTransactions(grouped);


            //Assert.AreEqual(29, grouped.Count());
            Assert.AreEqual(154, grouped.Count);

            // 17 4 - 17 6 = 3 columner + 1 för info om cat
            Assert.AreEqual(11 + 1, table.ColumnHeaders.Count);

            //// 6 typer av cats. (rad med headers för sig)
            //Assert.AreEqual(18, table.Rows.Count);

            //Assert.AreEqual("-1,925.00", table.Rows[4][1]);
        }

        private TransactionList GetTestDataFromFileAndCheckDateFormat()
        {
            _tableGetter = new TableGetter();

            //var dateTimeToday = DateTime.Today;
            //var testData = new TestData(dateTimeToday);

            // Hämta trans
            var transactionList =
                //testData.GetTestDataTransaktionList(numberOfnewTestDatas)
                TestData.GetTestDatasFromFiles()
                ;

            Assert.IsTrue(transactionList.Transactions.Any());
            Assert.IsTrue(transactionList.Transactions.Count > 50);

            //Expected:< 2017 - 02 - 22: buffert = -4 000,00.No group No Category >.
            //Actual:< 2017 - 04 - 12: Klarna = -453,66.ID_OTHER Okategoriserat >.

            Assert.AreEqual("2017-04-12: Klarna = -453,66. ID_OTHER Okategoriserat",
                //$"{dateTimeToday.ToShortDateString()}: Testöverföring0 = 0. ID_OTHER Försäkringar",
                transactionList.Transactions[0].ToString());

            return transactionList;
        }

        private TransactionList GetTestDataFromCodeAndCheckDateFormat()
        {
            _tableGetter = new TableGetter();

            var dateTimeToday = DateTime.Today;
            var testData = new TestData(dateTimeToday);

            // Hämta trans
            var transactionList = testData
                .GetTestDataTransaktionList(NumberOfnewTestDatas)
                ;

            Assert.IsTrue(transactionList.Transactions.Any());

            Assert.AreEqual(
                $"{dateTimeToday.ToShortDateString()}: Testöverföring0 = 0. ID_OTHER Försäkringar",
                transactionList.Transactions[0].ToString());

            return transactionList;
        }

        private static IEnumerable<IGrouping<TransGrouping, Transaction>> GetGroupedAndAssert(List<Transaction> transactions)
        {
            var grouped = TableGetter.GroupOnMonthAndCategory(transactions).ToList();
            Assert.AreNotEqual(NumberOfnewTestDatas, grouped.Count);

            var groupElementsNotNull = grouped.Sum(g => g.Count(e => e != null));
            Assert.AreEqual(transactions.Count,
                groupElementsNotNull
                );

            var groupElementsAlsoNull = grouped.Sum(g => g.Count());
            Assert.AreEqual(transactions.Count,
                groupElementsAlsoNull
                );

            return grouped;
        }

        [TestMethod]
        public void TransaktionBalanceTotalTest()
        {
            var transs1 = new TransactionList
            {
                Account = new Account
                {
                    Balance = "0,00"
                }
            };
            var transs2 = new TransactionList
            {
                Account = new Account
                {
                    AvailableAmount = "200.0"
                }
            };
            var target = new TransactionHandler(null, null);

            var allTrans = new List<TransactionList>();
            allTrans.Add(transs1);
            allTrans.Add(transs2);

            Assert.AreEqual(200.0, target.GetTotalBalanceForTransactions(allTrans));
        }
    }
}
