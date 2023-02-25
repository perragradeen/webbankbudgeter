using GeneralSettingsHandler;
using InbudgetHandler.Model;
using InbudgetHandler;
using WebBankBudgeter.Service.Model;
using WebBankBudgeter.Service.Model.ViewModel;
using WebBankBudgeter.Service.MonthAvarages;
using WebBankBudgeter.Service.Services;
using WebBankBudgeter.Service;
using WebBankBudgeterUi.UiBinders;

namespace WebBankBudgeterUi {
    public partial class WebBankBudgeterUi : Form
    {
        private readonly GeneralSettingsGetter generalSettingsGetter;
        private readonly TransactionHandler _transactionHandler;
        private readonly InBudgetUiHandler _inBudgetUiHandler;
        private readonly UtgiftsHanterareUiBinder _utgiftsHanterareUiBinder;
        private readonly SkapaInPosterHanterare _inPosterHanterare;

        private string TransactionFilePath =>
            generalSettingsGetter.GetStringSetting("TransactionTestFilePath");
        private string GetGeneralSettingsPath() {
            var path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"Data\"
            );
            return Path.Combine(path, @"GeneralSettings.xml");
        }

        private string CategoryFilePath => GetCategoryFilePath();
        private string GetCategoryFilePath() {
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(
                appPath,
                generalSettingsGetter.GetStringSetting("CategoryPath")
            );
        }

        public WebBankBudgeterUi() {
            try {
                generalSettingsGetter = new GeneralSettingsGetter(
                    GetGeneralSettingsPath());
                _transactionHandler = GetTransactionHandler();

                InitializeComponent();

                var inBudgetHandler = new InBudgetHandler(InBudgetFilePath);
                _inBudgetUiHandler = new InBudgetUiHandler(inBudgetHandler, gv_incomes, WriteLineToOutputAndScrollDown);

                _utgiftsHanterareUiBinder = new UtgiftsHanterareUiBinder(gv_budget);

                _inPosterHanterare = new SkapaInPosterHanterare(inBudgetHandler, _transactionHandler);


                ReloadButton.Click += async (s, e) =>
                    await ReloadButton_ClickAsync(s, e);

                Load += async (s, e) =>
                    await Form1_LoadAsync(s, e);

                SkapaTomRad.Click += async (s, e) =>
                    await SkapaTomRad_Click(s, e);
            }
            catch (Exception e) {
                WriteLineToOutputAndScrollDown(e.Message);
            }
        }

        private TransactionHandler GetTransactionHandler() {
            var tableGetter = new TableGetter { AddAverageColumn = true };
            return new TransactionHandler(
                WriteToOutput,
                tableGetter,
                CategoryFilePath,
                TransactionFilePath
            );
        }

        private static string InBudgetFilePath => GetInBudgetFilePath();

        private static string GetInBudgetFilePath() {
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(appPath, @"TestData\BudgetIns.json");
        }

        private async Task Form1_LoadAsync(object sender, EventArgs e) {
            try {
                ReloadButton.Show();

                await FillTablesAsync();
            }
            catch (Exception ex) {
                MessageBox.Show(@"Error: " + ex.Message);
                ReloadButton.Show();
            }
        }

        private async Task FillTablesAsync() {
            InitIncomesUi();
            InitTotalsUi();

            // Hämta, behandla och koppla data till UI
            // var inPosterTask = BindInPosterToUiAsync();

            // Hämta utgifter (transactioner) data ---
            var loadSuccess =
                await GetTransactionsAsync();
            if (!loadSuccess) return;
            // --- Hämta data

            // Behandla data ---
            SortTransactions();
            RemoveDuplicates();
            var summedAvaragesForCalc = CalculateMonthlyAvarages();
            var table = TransformToTextTableFromTransactions();
            AddAveragesToTable(table);
            // --- Behandla data

            // Koppla data till UI ---
            WriteMetaAsSaldoEtcToUi();
            BindTransactionListToUi();
            DescribeReoccurringGroups();
            DescribeStartYear(table);

            BindToBudgetTableUi(table);
            BindMonthAveragesToUi(summedAvaragesForCalc);
            // --- Koppla data till UI

            //await inPosterTask;

            //BindTotalsToUi();

            // Ta ut in-data och utgifter.
            var inDataRader = await _inBudgetUiHandler.HämtaRaderFörUiBindningAsync();
            var utgiftsRader = table.BudgetRows.ToList();
            var månadsRubriker = await _inBudgetUiHandler.HämtaRubrikePåInPosterAsync();

            // Presentera tabell för kvar budget.
            VisaKvarRader_BindInPosterRaderTillUi(inDataRader, utgiftsRader, månadsRubriker);

            // Presentera tabell för inkomst i varje kategori budget.
            VisaInRader_BindInPosterRaderTillUi(inDataRader, månadsRubriker);

            // Presentera summor för varje kat.
        }

        private void AddAveragesToTable(TextToTableOutPuter table) {
            table.AveragesForTransactions = SkapaInPosterHanterare.GetAvarages(
                _transactionHandler.TransactionList,
                DateTime.Today);
        }

        private void VisaInRader_BindInPosterRaderTillUi(List<Rad> inDataRader, List<string> månadsRubriker) {
            _inBudgetUiHandler.BindInPosterRaderTillUi(
                inDataRader,
                månadsRubriker,
                gv_incomes
            );
        }

        private void VisaKvarRader_BindInPosterRaderTillUi(List<Rad> inDataRader,
            List<BudgetRow> utgiftsRader, List<string> månadsRubriker) {
            _inBudgetUiHandler.BindInPosterRaderTillUi(
                SnurraIgenom(inDataRader, utgiftsRader, WriteLineToOutputAndScrollDown),
                månadsRubriker,
                gv_Kvar);
        }

        private static List<Rad> SnurraIgenom(
            IEnumerable<Rad> inData,
            List<BudgetRow> utgifter,
            Action<string> writeLineToOutputAndScrollDown) {
            if (utgifter == null) throw new ArgumentNullException(nameof(utgifter));
            var kvarrader = new List<Rad>();
            foreach (var inBudget in inData) {
                // Synka med kategori och månad.
                // Hitta motsvarande utgift
                var motsvarandeUtgifterRader = utgifter
                    .Where(u => u.CategoryText.Trim() == inBudget.RadNamnY.Trim()
                    );

                var nuvarandeRad = new Rad { RadNamnY = inBudget.RadNamnY };
                foreach (var motsvarandeUtgiftsRad in motsvarandeUtgifterRader) {
                    foreach (var utgiftsMånad in motsvarandeUtgiftsRad.AmountsForMonth) {
                        if (inBudget.Kolumner.ContainsKey(utgiftsMånad.Key)) {
                            // och räkna ut diff.
                            var kvar =
                                // inkomst - utgift
                                inBudget.Kolumner[utgiftsMånad.Key]
                                + utgiftsMånad.Value; // Utgifter är negativa ie -1200

                            if (!nuvarandeRad.Kolumner.ContainsKey(utgiftsMånad.Key)) {
                                nuvarandeRad.Kolumner.Add(utgiftsMånad.Key, 0);
                            }

                            nuvarandeRad.Kolumner[utgiftsMånad.Key] += kvar;
                        }
                        else {
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

        private MonthAvarages CalculateMonthlyAvarages() {
            var monthAveragesCalcer = new MonthAvaragesCalcs(
                _transactionHandler.TransactionList);
            var summedAvaragesForCalc = monthAveragesCalcer.GetMonthAvarages();
            return summedAvaragesForCalc;
        }

        private async Task<bool> GetTransactionsAsync() {
            return await _transactionHandler.GetTransactionsAsync();
        }

        private void RemoveDuplicates() {
            _transactionHandler.RemoveDuplicates();
        }

        private void SortTransactions() {
            _transactionHandler.SortTransactions();
        }

        private TextToTableOutPuter TransformToTextTableFromTransactions() {
            return _transactionHandler.GetTextTableFromTransactions();
        }

        private const string CategoryNameColumnDescription = "Category . Month->";

        private void InitIncomesUi() {
            gv_incomes.Columns.Add("1", CategoryNameColumnDescription);
            gv_Kvar.Columns.Add("1", CategoryNameColumnDescription);
        }

        private void InitTotalsUi() {
            var cNo = gv_Totals.Columns.Add("1", "Description");
            gv_Totals.Columns[cNo].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            gv_Totals.Columns[cNo].CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;

            gv_Totals.Columns.Add("2", "Amount");
        }

        private void DescribeStartYear(TextToTableOutPuter table) {
            label1.Text += @"Utgifter börjar på år: " + table.UtgiftersStartYear;
        }

        private void DescribeReoccurringGroups() {
            foreach (var group in MonthAvaragesCalcs.ReoccurringCatGroups) {
                WriteToOutput(group + ". ");
            }
        }

        private void BindMonthAveragesToUi(MonthAvarages summedAvaragesForCalc) {
            // bind to ui gv_totals
            AddRowWith2Cells(gv_Totals, "Återkommande snitt", summedAvaragesForCalc.ReccuringCosts);
            AddRowWith2Cells(gv_Totals, "Inkomster snitt", summedAvaragesForCalc.Incomes);
            AddRowWith2Cells(gv_Totals, "Diff snitt", summedAvaragesForCalc.IncomeDiffCosts);
        }

        private void SparaInPosterPåDisk() {
            _inBudgetUiHandler.SparaInPosterPåDisk();

            WriteLineToOutputAndScrollDown("Sparat.");
        }

        private static void AddRowWith2Cells(DataGridView gridView, string description, double amount) {
            var columnNumber = 0;
            var rowNumber = gridView.Rows.Add();
            gridView.Rows[rowNumber].Cells[columnNumber++].Value = description;
            gridView.Rows[rowNumber].Cells[columnNumber].Value = amount.ToString("# ##0");
        }

        private void WriteMetaAsSaldoEtcToUi() {
            label1.Text += @" Saldo: " +
                           _transactionHandler.TransactionList.Account.AvailableAmount;
        }

        private void BindToBudgetTableUi(TextToTableOutPuter table) {
            _utgiftsHanterareUiBinder.BindToBudgetTableUi(table);
        }

        private void BindTransactionListToUi() {
            dg_Transactions.Columns.Add("1", "Date");
            dg_Transactions.Columns.Add("2", "Amount");
            dg_Transactions.Columns.Add("3", "Description");
            dg_Transactions.Columns.Add("4", "Category");

            foreach (var row in _transactionHandler.TransactionList.Transactions) {
                var n = dg_Transactions.Rows.Add();

                var i = 0;
                dg_Transactions.Rows[n].Cells[i++].Value = row.DateAsDate.ToShortDateString();
                dg_Transactions.Rows[n].Cells[i++].Value = row.AmountAsDouble;
                dg_Transactions.Rows[n].Cells[i++].Value = row.Description;
                dg_Transactions.Rows[n].Cells[i].Value = row.CategoryName;
            }
        }

        private void WriteToOutput(string message) {
            LogTexts.AppendText(message);
        }

        private void WriteLineToOutputAndScrollDown(string message) {
            WriteToOutput(Environment.NewLine);
            WriteToOutput(message);
            LogTexts.ScrollToCaret();
        }

        private async Task ResetUtgifterAsync() {
            gv_budget.Rows.Clear();
            gv_budget.Columns.Clear();
            LogTexts.Clear();
            LogTexts.AppendText($"Reloading at {DateTime.Now}");

            try {
                await FillTablesAsync();
            }
            catch (Exception ex) {
                MessageBox.Show($@"Error: {ex.Message}");
                ReloadButton.Show();
            }
        }

        private async Task ReloadButton_ClickAsync(object sender, EventArgs e) {
            await ResetUtgifterAsync();
        }

        private void SaveInPosterButton_Click(object sender, EventArgs e) {
            SparaInPosterPåDisk();
        }

        private async Task SkapaTomRad_Click(object sender, EventArgs e) {
            await FyllIMedDefaultInposterFörSenasteMånadAsync();
        }

        private async Task FyllIMedDefaultInposterFörSenasteMånadAsync() {
            // Skapa en rad med alla valbara kategorier
            //      för nuvarande månad
            //          om det inte redan finns

            // Hämta en lista på exempel inposter. Baserat på snitt för utgifter i varje kat
            var inPosterDefault = await _inPosterHanterare.SkapaInPoster(
                transactionList: _transactionHandler.TransactionList);

            // Merga med föregående inposter.
            var inDataRaderTidigare = await _inBudgetUiHandler.GetInPoster();
            inPosterDefault.AddRange(inDataRaderTidigare);
            _inBudgetUiHandler.SetInPoster(inPosterDefault);

            // Hämta rader i Ui-format
            var inDataRader = await _inBudgetUiHandler
                .HämtaRaderFörUiBindningAsync();

            try {
                //Skriv ut i Ui
                gv_incomes.Columns.Clear();
                gv_incomes.Rows.Clear();
                var månadsRubriker = await _inBudgetUiHandler.HämtaRubrikePåInPosterAsync();
                _inBudgetUiHandler.BindInPosterRaderTillUi(
                    inDataRader,
                    månadsRubriker,
                    gv_incomes
                );
            }
            catch (Exception e) {
                WriteLineToOutputAndScrollDown(e.Message);
            }
        }
    }
}