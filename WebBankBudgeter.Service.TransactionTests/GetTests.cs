using InbudgetToTable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace InbudgetToTableTests
{
    [TestClass]
    public class GetTests
    {
        private static string FilePath =>
            //@"C:\Files\Dropbox\budget\Program\webbankbudgeter\SwedbankSharp-master\WebBankBudgeter\TestData\BudgetIns.json"
            @"C:\Files\Dropbox\budget\Program\webbankbudgeter\SwedbankSharp-master\WebBankBudgeter\TestData\BudgetInsRiktigaExempel.json"
            ;

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
