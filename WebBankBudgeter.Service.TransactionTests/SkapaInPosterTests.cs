using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InbudgetHandler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebBankBudgeter.Service.Model;
using WebBankBudgeter.Service.Services;

namespace WebBankBudgeter.Service.TransactionTests
{
    [TestClass]
    public class SkapaInPosterTests
    {
        private const string _transactionTestFilePath = @"C:\Temp";
        private const string _categoryRelativeDirPath = @"..\..\..\Budgetterarn\Data";

        private static string _budgetInsFilePath;
        //@"C:\Files\Dropbox\budget\Program\webbankbudgeter\SwedbankSharp-master\WebBankBudgeter\TestData\BudgetIns.json";

        private static string _globalLog;

        private static InBudgetHandler InBudgetHandler =>
            new InBudgetHandler(
                _budgetInsFilePath);

        private TransactionHandler TransactionHandler
        {
            get
            {
                var tableGetter = new TableGetter {AddAverageColumn = true};
                return new TransactionHandler(
                    WriteToOutput,
                    tableGetter,
                    GetCategoryFilePath(),
                    _transactionTestFilePath
                );
            }
        }

        public SkapaInPosterTests()
        {
            var baseDir = Environment.CurrentDirectory;
            //@"C:\Files\Dropbox\budget\Program\webbankbudgeter\SwedbankSharp-master\WebBankBudgeter\TestData\BudgetIns.json";
            _budgetInsFilePath = Path.Combine(baseDir, @"TestData\BudgetIns.json");
        }

        [TestMethod]
        public async Task SkapaInPosterTestAsync()
        {
            var handler = new SkapaInPosterHanterare(
                InBudgetHandler,
                TransactionHandler);

            var nuDatum = new DateTime(2021, 09, 01);
            var results = await handler.SkapaInPoster(nuDatum);

            Assert.IsTrue(results.Any());
            Assert.IsNull(_globalLog);
        }

        [TestMethod]
        public void GetCategoryFilePathTest()
        {
            var path = GetCategoryFilePath();

            Assert.IsTrue(File.Exists(path));
        }

        [TestMethod]
        public void DoubleToStringFormat_Test()
        {
            var value = 1151.23;
            var actual =
                value.ToString("N0");

            Assert.AreEqual("1 151", actual);
        }

        [TestMethod]
        public void GetLowestDateTest()
        {
            // Arrange
            var transactions = GetDefaultTransactions().Transactions;
            var expectedDate = new DateTime(2019, 1, 1);

            // Act
            var resultDate =
                SkapaInPosterHanterare.GetLowestDate(transactions);

            // Assert
            Assert.AreEqual(expectedDate, resultDate);
        }

        [TestMethod]
        public void GetHighestDateTest()
        {
            // Arrange
            var transactions = GetDefaultTransactions().Transactions;
            var expectedDate = new DateTime(2019, 4, 1);

            // Act
            var resultDate =
                SkapaInPosterHanterare.GetHighestDate(transactions);

            // Assert
            Assert.AreEqual(expectedDate, resultDate);
        }

        [TestMethod]
        public void GetNrMonthsBetweenDates_Test()
        {
            // Arrange
            var date1 = new DateTime(2018, 6, 1);
            var date2 = new DateTime(2020, 7, 1);
            var expectedMonths = 25;

            // Act
            var result = SkapaInPosterHanterare.GetNrMonthsBetweenDates(
                date2,
                date1);

            // Assert
            Assert.AreEqual(expectedMonths, result);
        }

        [TestMethod]
        public void GetAvaragesTest()
        {
            // Arrange
            var expectedAverage = 3.75;
            var transactionList = GetDefaultTransactions();

            // Act
            var results = SkapaInPosterHanterare
                .GetAvarages(transactionList, DateTime.Today);

            // Assert
            Assert.IsTrue(results.Any());
            Assert.AreEqual(expectedAverage,
                results[0].AmountsForMonth.FirstOrDefault().Value);
        }

        private static TransactionList GetDefaultTransactions()
        {
            var cat1 = new Categorizations();
            var cats1 = new List<Categories>
            {
                new Categories {Name = "kat1"},
            };
            cat1.Categories = cats1;
            var transactions = new List<Transaction>
            {
                new Transaction {Amount = 0, Categorizations = cat1, DateAsDate = new DateTime(2019, 1, 1)},
                new Transaction {Amount = 0, Categorizations = cat1, DateAsDate = new DateTime(2019, 2, 1)},
                new Transaction {Amount = 5, Categorizations = cat1, DateAsDate = new DateTime(2019, 3, 1)},
                new Transaction {Amount = 10, Categorizations = cat1, DateAsDate = new DateTime(2019, 4, 1)}
            };

            return new TransactionList
            {
                Transactions = transactions
            };
        }

        private static string GetCategoryFilePath()
        {
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(
                //< property name = "CategoryPath" value = "Data\Categories.xml" />
                Path.Combine(appPath, _categoryRelativeDirPath),
                //@"\Files\Dropbox\budget\Budgeterarn Release\Data", //TODO:Viktig: fixa riktig sökväg, ev slå ihop winforms-apparna
                @"Categories.xml"
            );
        }

        private static void WriteToOutput(string message)
        {
            _globalLog += message;
        }
    }
}