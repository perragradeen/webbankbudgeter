using System;
using System.IO;
using System.Threading.Tasks;
using InbudgetHandler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebBankBudgeter.Service.TransactionTests
{
    [TestClass]
    public class GetTests
    {
        private readonly string FilePath;
        //@"C:\Files\Dropbox\budget\Program\webbankbudgeter\SwedbankSharp-master\WebBankBudgeter\TestData\BudgetInsRiktigaExempel.json"

        public GetTests()
        {
            var baseDir = Environment.CurrentDirectory;
            FilePath = Path.Combine(baseDir, @"Data\BudgetIns.json");
            // FilePath = Path.Combine(baseDir, @"TestData\BudgetInsRiktigaExempel.json");
            // BudgetIns
        }

        private InBudgetHandler Target =>
            new InBudgetHandler(FilePath);

        [TestMethod]
        public async Task H�mtaRaderF�rUiBindningAsyncTestAsync()
        {
            var results = await Target.H�mtaRaderF�rUiBindningAsync();

            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task H�mtaRubrikerP�InPosterAsyncTestAsync()
        {
            var results = await Target.H�mtaRubrikerP�InPosterAsync();
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task GetInPosterTestAsync()
        {
            var results = await Target.GetInPoster();
            Assert.IsNotNull(results);
        }
    }
}