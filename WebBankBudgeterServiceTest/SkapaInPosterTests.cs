using InbudgetHandler;
using WebBankBudgeterService;
using WebBankBudgeterService.Model;
using WebBankBudgeterService.Services;

namespace WebBankBudgeterServiceTest
{
    [TestClass]
    public class SkapaInPosterTests
    {
        private static string _transactionTestFilePath => Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            //@"\Temp\pelles budget.xls"
            @"..\..\..\..\BudgetterarnUi\bin\Debug\pelles budget.xls" //TODO: Byt fr�n joxiga relativa s�kv�gar.ev v�lj fr�n UI och spara...
        //C:\files\Dropbox\budget\Program\webbankbudgeter\BudgetterarnUi\bin\Debug
        );

        private const string _categoryRelativeDirPath = @"Data";
        //private const string _categoryRelativeDirPath = @"..\..\..\Budgetterarn\Data";

        private static readonly string _budgetInsRelativeFilePath = @"Data\BudgetIns.json";
        private static string? _budgetInsFilePath;
        //@"C:\Files\Dropbox\budget\Program\webbankbudgeter\SwedbankSharp-master\WebBankBudgeter\TestData\BudgetIns.json";

        private static string? _globalLog;

        private static InBudgetHandler InBudgetHandler =>
            new(
                _budgetInsFilePath);

        private TransactionHandler TransactionHandler
        {
            get
            {
                if (!File.Exists(_transactionTestFilePath))
                {
                    throw new FileNotFoundException(_transactionTestFilePath);
                }

                var tableGetter = new TableGetter { AddAverageColumn = true };
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
            _budgetInsFilePath = Path.Combine(baseDir, _budgetInsRelativeFilePath);
        }

        [TestMethod]
        public void TransFileExistsTest()
        {
            if (!File.Exists(_transactionTestFilePath))
            {
                throw new FileNotFoundException(_transactionTestFilePath);
            }
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
        public async Task SkapaInPosterTestAsync2()
        {
            var handler = new SkapaInPosterHanterare(
                InBudgetHandler,
                TransactionHandler);

            var nuDatum = SkapaInPosterHanterare.Fr�n�rTillDatum("2022");
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

            Assert.AreEqual("1�151", actual);
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

        [TestMethod]
        public void FilterTransactionsTest()
        {
            // Arrange
            var expectedEntries = 2;
            var transactionList = GetDefaultTransactions();

            // Act
            var actual = TransFilterer.FilterTransactions(
                transactionList,
                new DateTime(2019, 02, 01),
                new DateTime(2019, 03, 01));

            // Assert
            Assert.IsTrue(actual.Transactions.Any());
            Assert.AreEqual(expectedEntries,
                //TransactionHandler.TransactionList.Transactions
                actual.Transactions.Count);
        }

        [TestMethod]
        public void FilterTransactions_OneYear_Test()
        {
            // Arrange
            var expectedEntries = 5;
            var transactionList = GetDefaultTransactions();
            transactionList.Transactions.Add(
                new Transaction { DateAsDate = new DateTime(2020, 1, 1), Amount = 10, Categorizations = GetDefatultCat() }
            );
            transactionList.Transactions.Add(
                new Transaction { DateAsDate = new DateTime(2019, 12, 31), Amount = 10, Categorizations = GetDefatultCat() }
            );

            // Act
            var actual = TransFilterer.FilterTransactions(
                transactionList,
                2019);

            // Assert
            Assert.IsTrue(actual.Transactions.Any());
            Assert.AreEqual(expectedEntries,
                //TransactionHandler.TransactionList.Transactions
                actual.Transactions.Count);
        }

        [TestMethod]
        public void FilterTransactionsTest2s()
        {
            var results = SkapaInPosterHanterare
                .Fr�n�rTillDatum("2023");

            Assert.AreEqual(new DateTime(2023, 01, 01), results);
        }

        [TestMethod]
        public void FilterTransactionsTest2()
        {
            // Arrange
            var expectedEntries = 0;
            var transactionList = GetDefaultTransactions();

            // Act
            var actual = TransFilterer.FilterTransactions(
                transactionList);

            // Assert
            Assert.IsFalse(actual.Transactions.Any());
            Assert.AreEqual(expectedEntries,
                //TransactionHandler.TransactionList.Transactions
                actual.Transactions.Count);
        }

        private static TransactionList GetDefaultTransactions()
        {
            var cat1 = GetDefatultCat();
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

        private static Categorizations GetDefatultCat()
        {
            var cat1 = new Categorizations();
            var cats1 = new List<Categories>
            {
                new Categories {Name = "kat1"},
            };
            cat1.Categories = cats1;
            return cat1;
        }

        private static string GetCategoryFilePath()
        {
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(
                Path.Combine(appPath, _categoryRelativeDirPath),
                @"Categories.xml"
            );
        }

        private static void WriteToOutput(string message)
        {
            _globalLog += message;
        }
    }
}