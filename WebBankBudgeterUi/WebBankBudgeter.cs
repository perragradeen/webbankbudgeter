using GeneralSettingsHandler;
using InbudgetHandler;
using InbudgetHandler.Model;
using RefLesses;
using WebBankBudgeterService;
using WebBankBudgeterService.Model;
using WebBankBudgeterService.Model.ViewModel;
using WebBankBudgeterService.MonthAvarages;
using WebBankBudgeterService.Services;

namespace WebBankBudgeterUi
{
    internal class WebBankBudgeter
    {
        internal const string CategoryNameColumnDescription = "Category . Month->";

        private readonly GeneralSettingsGetter generalSettingsGetter;
        private readonly TransactionHandler _transactionHandler;
        private readonly InBudgetHandler _inBudgetHandler;
        private readonly SkapaInPosterHanterare _inPosterHanterare;
        private readonly Action<string> writeToOutput;
        private readonly Action<string> writeLineToOutputAndScrollDown;

        internal SkapaInPosterHanterare InPosterHanterare =>
            _inPosterHanterare;
        internal InBudgetHandler InBudgetHandler =>
            _inBudgetHandler;
        internal TransactionHandler TransactionHandler =>
            _transactionHandler;

        private string TransactionFilePath =>
            generalSettingsGetter?.GetStringSetting("TransactionTestFilePath");
        private static string GetGeneralSettingsPath()
        {
            var path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"Data\"
            );
            return Path.Combine(path, @"GeneralSettings.xml");
        }

        private string CategoryFilePath => GetCategoryFilePath();
        private string GetCategoryFilePath()
        {
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(
                appPath,
                generalSettingsGetter?.GetStringSetting("CategoryPath")!
            );
        }

        public WebBankBudgeter(
            Action<string> writeToOutput,
            Action<string> writeLineToOutputAndScrollDown)
        {
            generalSettingsGetter = new GeneralSettingsGetter(
                    GetGeneralSettingsPath());
            _transactionHandler = GetTransactionHandler();

            _inBudgetHandler = new InBudgetHandler(InBudgetFilePath);

            _inPosterHanterare = new SkapaInPosterHanterare(
                _inBudgetHandler,
                _transactionHandler);
            this.writeToOutput = writeToOutput;
            this.writeLineToOutputAndScrollDown = writeLineToOutputAndScrollDown;
        }

        private TransactionHandler GetTransactionHandler()
        {
            var tableGetter = new TableGetter { AddAverageColumn = true };
            return new TransactionHandler(
                writeToOutput,
                tableGetter,
                CategoryFilePath,
                TransactionFilePath
            );
        }

        private static string InBudgetFilePath => GetInBudgetFilePath();

        private static string GetInBudgetFilePath()
        {
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(appPath, @"TestData\BudgetIns.json");
        }

        internal async Task FillTablesAsync()
        {
            // Hämta, behandla och koppla data till UI
            // var inPosterTask = BindInPosterToUiAsync();

            // Hämta utgifter (transactioner) data ---
            var loadSuccess =
                await GetTransactionsAsync();
            if (!loadSuccess)
            {
                return;
            }
            // --- Hämta data

            // Behandla data ---
            SortTransactions();
            RemoveDuplicates();
        }

        internal void FilterTransactions(string txtYearFilter)
        {
            var selectedYear = MiscFunctions
                .SafeGetIntFromString(txtYearFilter);
            var transactionList = _transactionHandler.TransactionList;

            var filteredTrans = string.IsNullOrWhiteSpace(txtYearFilter)
                ? TransFilterer.FilterTransactions(
                    transactionList)
                : TransFilterer.FilterTransactions(
                    transactionList, selectedYear);

            _transactionHandler.SetTransactionList(filteredTrans);
        }

        internal void AddAveragesToTable(TextToTableOutPuter table)
        {
            table.AveragesForTransactions = SkapaInPosterHanterare.GetAvarages(
                _transactionHandler?.TransactionList,
                DateTime.Today);
        }

        internal static List<Rad> SnurraIgenom(
            IEnumerable<Rad> inData,
            List<BudgetRow> utgifter,
            Action<string> writeLineToOutputAndScrollDown) =>
            InBudgetKvarCalculator.SnurraIgenom(inData, utgifter, writeLineToOutputAndScrollDown);

        internal MonthAvarages CalculateMonthlyAvarages()
        {
            var monthAveragesCalcer = new MonthAvaragesCalcs(
                _transactionHandler?.TransactionList);
            var summedAvaragesForCalc = monthAveragesCalcer.GetMonthAvarages();
            return summedAvaragesForCalc;
        }

        private async Task<bool> GetTransactionsAsync()
        {
            return await _transactionHandler?.GetTransactionsAsync()!;
        }

        private void RemoveDuplicates()
        {
            _transactionHandler?.RemoveDuplicates();
        }

        private void SortTransactions()
        {
            _transactionHandler?.SortTransactions();
        }

        internal TextToTableOutPuter TransformToTextTableFromTransactions()
        {
            return _transactionHandler?.GetTextTableFromTransactions();
        }

        internal void DescribeReoccurringGroups()
        {
            foreach (var group in MonthAvaragesCalcs.ReoccurringCatGroups)
            {
                writeToOutput(group + ". ");
            }
        }


    }
}
