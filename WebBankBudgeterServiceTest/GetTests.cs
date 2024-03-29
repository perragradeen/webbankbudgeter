using InbudgetHandler;

namespace WebBankBudgeterServiceTest
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
            new(FilePath);

        [TestMethod]
        public async Task HämtaRaderFörUiBindningAsyncTestAsync()
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