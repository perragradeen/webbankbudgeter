using InbudgetToTable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace InbudgetToTableTests
{
    [TestClass]
    public class GetTests
    {
        private static string _filePath =>
            @"C:\Files\Dropbox\budget\Program\webbankbudgeter\SwedbankSharp-master\WebBankBudgeter\TestData\BudgetIns.json";
        private InBudgetHandler Target =>
            new InBudgetHandler(
                _filePath);

        [TestMethod]
        public async Task HämtaRaderFörUiBindningAsyncTest()
        {
            var results = await Target.HämtaRaderFörUiBindningAsync();

            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task HämtaRubrikerPåInPosterAsyncTestAsync()
        {
            var results = await Target.HämtaRubrikerPåInPosterAsync();
            Assert.IsNotNull(results);

        }

        [TestMethod]
'        public async Task GetInPosterTestAsync()
        {
            var results = await Target.GetInPoster();
            Assert.IsNotNull(results);
        }
    }
}
