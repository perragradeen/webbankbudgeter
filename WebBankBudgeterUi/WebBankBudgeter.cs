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
            Action<string> writeLineToOutputAndScrollDown)
        {
            if (utgifter == null)
            {
                throw new ArgumentNullException(nameof(utgifter));
            }

            var kvarrader = new List<Rad>();
            foreach (var inBudget in inData)
            {
                // Synka med kategori och månad.
                // Hitta motsvarande utgift
                var motsvarandeUtgifterRader = utgifter
                    .Where(u => u.CategoryText.Trim() == inBudget.RadNamnY.Trim()
                    );

                var nuvarandeRad = new Rad { RadNamnY = inBudget.RadNamnY };
                foreach (var motsvarandeUtgiftsRad in motsvarandeUtgifterRader)
                {
                    foreach (var utgiftsMånad in motsvarandeUtgiftsRad.AmountsForMonth)
                    {
                        if (inBudget.Kolumner.ContainsKey(utgiftsMånad.Key))
                        {
                            // och räkna ut diff.
                            var kvar =
                                // inkomst - utgift
                                inBudget.Kolumner[utgiftsMånad.Key]
                                + utgiftsMånad.Value; // Utgifter är negativa ie -1200

                            if (!nuvarandeRad.Kolumner.ContainsKey(utgiftsMånad.Key))
                            {
                                nuvarandeRad.Kolumner.Add(utgiftsMånad.Key, 0);
                            }

                            nuvarandeRad.Kolumner[utgiftsMånad.Key] += kvar;
                        }
                        else
                        {
                            // Fel
                            var message = "Hittar ingen motsvarande inpost för utgift i :"
                                          + utgiftsMånad.Key + " och kategori: " + inBudget.RadNamnY;

                            writeLineToOutputAndScrollDown(message);
                        }
                    }
                }

                kvarrader.Add(nuvarandeRad);
            }

            return kvarrader;
        }

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

        /// <summary>
        /// Slår in budget-IN i samma tabell som transaktions-UT (plan M5.1 / G1).
        /// </summary>
        internal static void MergeBudgetInsIntoBudgetTextTable(TextToTableOutPuter table, List<Rad> inPosterRader)
        {
            if (table == null || inPosterRader == null || inPosterRader.Count == 0)
            {
                return;
            }

            BudgetTableInMerger.MergeInRows(table, inPosterRader);
        }

        /// <summary>
        /// Kvar = IN + UT per kategori (befintlig <see cref="SnurraIgenom"/>), som <see cref="TextToTableOutPuter"/> för grid-bindning.
        /// </summary>
        internal TextToTableOutPuter BuildKvarTextTable(TextToTableOutPuter mergedExpenseTable, List<Rad> inPosterRader,
            Action<string> logLine)
        {
            if (mergedExpenseTable?.BudgetRows == null)
            {
                return new TextToTableOutPuter();
            }

            var builder = new BudgetStructureBuilder();
            var structured = builder.BuildStructuredBudget(
                mergedExpenseTable.BudgetRows,
                mergedExpenseTable.ColumnHeaders);

            var utgiftRader = BudgetStructureBuilder.GetExpenseRowsBeforeFirstSummary(structured);
            var kvarRader = SnurraIgenom(inPosterRader, utgiftRader, logLine);

            var kvarTable = new TextToTableOutPuter
            {
                UtgiftersStartYear = mergedExpenseTable.UtgiftersStartYear,
                ColumnHeaders = new List<string>(mergedExpenseTable.ColumnHeaders),
                AveragesForTransactions = mergedExpenseTable.AveragesForTransactions
            };

            var monthKeys = BudgetStructureBuilder.MonthColumnKeys(mergedExpenseTable.ColumnHeaders);
            var budgetRows = new List<BudgetRow>();

            foreach (var rad in kvarRader)
            {
                if (string.Equals(rad.RadNamnY, InBudgetHandler.SummaText, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var row = new BudgetRow { CategoryText = rad.RadNamnY };
                foreach (var mk in monthKeys)
                {
                    if (rad.Kolumner.TryGetValue(mk, out var v))
                    {
                        row.AmountsForMonth[mk] = v;
                    }
                }

                budgetRows.Add(row);
            }

            kvarTable.BudgetRows = budgetRows;
            return kvarTable;
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
