using InbudgetHandler;

namespace InbudgetHandlerTest
{
    [TestClass]
    public class GetTests
    {
        private static string _filePath;

        private InBudgetHandler Target =>
            new(
                _filePath);

        public GetTests()
        {
            var baseDir = AppContext.BaseDirectory;
            _filePath = Path.Combine(baseDir, "Data", "BudgetIns.json");
            //@"..\..\..\SwedbankSharp-master\WebBankBudgeter\TestData\BudgetIns.json");
            //BudgetInsRiktigaExempel.json
        }

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
        public async Task GetInPosterTestAsync()
        {
            var results = await Target.GetInPoster();
            Assert.IsNotNull(results);
        }
    }
}