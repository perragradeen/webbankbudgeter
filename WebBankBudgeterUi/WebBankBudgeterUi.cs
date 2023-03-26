#nullable disable
using InbudgetHandler;
using InbudgetHandler.Model;
using WebBankBudgeterService;
using WebBankBudgeterService.Model;
using WebBankBudgeterService.Model.ViewModel;
using WebBankBudgeterService.MonthAvarages;
using WebBankBudgeterUi.UiBinders;

namespace WebBankBudgeterUi
{
    public partial class WebBankBudgeterUi : Form
    {
        private readonly WebBankBudgeter webBankBudgeter;
        private readonly UtgiftsHanterareUiBinder _utgiftsHanterareUiBinder;
        private readonly InBudgetUiHandler _inBudgetUiHandler;

        public WebBankBudgeterUi()
        {
            try
            {
                InitializeComponent();

                webBankBudgeter = new WebBankBudgeter(
                    WriteToOutput,
                    WriteLineToOutputAndScrollDown);

                _inBudgetUiHandler = new InBudgetUiHandler(
                    webBankBudgeter.InBudgetHandler,
                    gv_incomes,
                    WriteLineToOutputAndScrollDown);

                _utgiftsHanterareUiBinder = new UtgiftsHanterareUiBinder(
                    gv_budget);

                txtYearFilter.Text = TransFilterer.LastYear().ToString();

                ReloadButton.Click += async (s, e) =>
                    await ReloadButton_ClickAsync(s, e);

                Load += async (s, e) =>
                    await Form1_LoadAsync(s, e);

                SkapaTomRad.Click += async (s, e) =>
                    await SkapaTomRad_Click(s, e);
            }
            catch (Exception e)
            {
                WriteLineToOutputAndScrollDown(e.Message);
            }
        }

        private async Task FillTablesAsync()
        {
            InitIncomesUi();
            InitTotalsUi();

            await webBankBudgeter.FillTablesAsync();

            // filtrera
            webBankBudgeter.FilterTransactions(txtYearFilter.Text);

            var table = webBankBudgeter.TransformToTextTableFromTransactions();
            webBankBudgeter.AddAveragesToTable(table);
            // --- Behandla data

            // Koppla data till UI ---
            WriteMetaAsSaldoEtcToUi();
            BindTransactionListToUi();
            webBankBudgeter.DescribeReoccurringGroups();
            DescribeStartYear(table);

            BindToBudgetTableUi(table);
            var summedAvaragesForCalc = webBankBudgeter.CalculateMonthlyAvarages();
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

        private void VisaInRader_BindInPosterRaderTillUi(List<Rad> inDataRader, List<string> månadsRubriker)
        {
            _inBudgetUiHandler.BindInPosterRaderTillUi(
                inDataRader,
                månadsRubriker,
                gv_incomes
            );
        }

        private void VisaKvarRader_BindInPosterRaderTillUi(List<Rad> inDataRader,
            List<BudgetRow> utgiftsRader, List<string> månadsRubriker)
        {
            _inBudgetUiHandler.BindInPosterRaderTillUi(
                WebBankBudgeter.SnurraIgenom(
                    inDataRader,
                    utgiftsRader,
                    WriteLineToOutputAndScrollDown),
                månadsRubriker,
                gv_Kvar);
        }

        private void SparaInPosterPåDisk()
        {
            _inBudgetUiHandler.SparaInPosterPåDisk();

            WriteLineToOutputAndScrollDown("Sparat.");
        }

        private async Task Form1_LoadAsync(object sender, EventArgs e)
        {
            try
            {
                ReloadButton.Show();

                await FillTablesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Error: " + ex.Message);
                ReloadButton.Show();
            }
        }

        private void InitIncomesUi()
        {
            gv_incomes.Columns.Add("1", WebBankBudgeter.CategoryNameColumnDescription);
            gv_Kvar.Columns.Add("1", WebBankBudgeter.CategoryNameColumnDescription);
        }

        private void InitTotalsUi()
        {
            var cNo = gv_Totals.Columns.Add("1", "Description");
            gv_Totals.Columns[cNo].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            gv_Totals.Columns[cNo].CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;

            gv_Totals.Columns.Add("2", "Amount");
        }

        private void DescribeStartYear(TextToTableOutPuter table)
        {
            label1.Text += @"Utgifter börjar på år: " + table.UtgiftersStartYear;
        }

        private void BindMonthAveragesToUi(MonthAvarages summedAvaragesForCalc)
        {
            // bind to ui gv_totals
            AddRowWith2Cells(gv_Totals, "Återkommande snitt", summedAvaragesForCalc.ReccuringCosts);
            AddRowWith2Cells(gv_Totals, "Inkomster snitt", summedAvaragesForCalc.Incomes);
            AddRowWith2Cells(gv_Totals, "Diff snitt", summedAvaragesForCalc.IncomeDiffCosts);
        }

        private static void AddRowWith2Cells(DataGridView gridView, string description, double amount)
        {
            var columnNumber = 0;
            var rowNumber = gridView.Rows.Add();
            gridView.Rows[rowNumber].Cells[columnNumber++].Value = description;
            gridView.Rows[rowNumber].Cells[columnNumber].Value = amount.ToString("# ##0");
        }

        private void WriteMetaAsSaldoEtcToUi()
        {
            label1.Text += @" Saldo: " +
                webBankBudgeter.TransactionHandler?
                    .TransactionList?.Account?.AvailableAmount;
        }

        private void BindToBudgetTableUi(TextToTableOutPuter table)
        {
            _utgiftsHanterareUiBinder.BindToBudgetTableUi(table);
        }

        private void BindTransactionListToUi()
        {
            dg_Transactions.Columns.Add("1", "Date");
            dg_Transactions.Columns.Add("2", "Amount");
            dg_Transactions.Columns.Add("3", "Description");
            dg_Transactions.Columns.Add("4", "Category");

            foreach (var row in webBankBudgeter.TransactionHandler?
                .TransactionList?.Transactions!)
            {
                var n = dg_Transactions.Rows.Add();

                var i = 0;
                dg_Transactions.Rows[n].Cells[i++].Value = row.DateAsDate.ToShortDateString();
                dg_Transactions.Rows[n].Cells[i++].Value = row.AmountAsDouble;
                dg_Transactions.Rows[n].Cells[i++].Value = row.Description;
                dg_Transactions.Rows[n].Cells[i].Value = row.CategoryName;
            }
        }

        private void WriteToOutput(string message)
        {
            LogTexts.AppendText(message);
        }

        private void WriteLineToOutputAndScrollDown(string message)
        {
            WriteToOutput(Environment.NewLine);
            WriteToOutput(message);
            LogTexts.ScrollToCaret();
        }

        private async Task ResetUtgifterAsync()
        {
            gv_budget.Rows.Clear();
            gv_budget.Columns.Clear();
            LogTexts.Clear();
            LogTexts.AppendText($"Reloading at {DateTime.Now}");

            try
            {
                await FillTablesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Error: {ex.Message}");
                ReloadButton.Show();
            }
        }

        private async Task ReloadButton_ClickAsync(object sender, EventArgs e)
        {
            await ResetUtgifterAsync();
        }

        private void SaveInPosterButton_Click(object sender, EventArgs e)
        {
            SparaInPosterPåDisk();
        }

        private async Task SkapaTomRad_Click(object sender, EventArgs e)
        {
            await FyllIMedDefaultInposterFörSenasteMånadAsync();
        }

        private async Task FyllIMedDefaultInposterFörSenasteMånadAsync()
        {
            // TODO: Sätt Incomes fliken som fokus när knappen trycks...

            // Skapa en rad med alla valbara kategorier
            //      för nuvarande månad
            //          om det inte redan finns

            var inDataRader = await HämtaIndataRader();

            try
            {
                //Skriv ut i Ui
                ÅterställInkomstGrid();

                var månadsRubriker = await _inBudgetUiHandler
                    .HämtaRubrikePåInPosterAsync();

                _inBudgetUiHandler.BindInPosterRaderTillUi(
                    inDataRader,
                    månadsRubriker,
                    gv_incomes
                );
            }
            catch (Exception e)
            {
                WriteLineToOutputAndScrollDown(e.Message);
            }
        }

        private async Task<List<Rad>> HämtaIndataRader()
        {
            var nuDatum = SkapaInPosterHanterare.FrånÅrTillDatum(
                            txtYearFilter.Text);

            // Hämta en lista på exempel inposter. Baserat på snitt för utgifter i varje kat
            var inPosterDefault = await webBankBudgeter.InPosterHanterare
                .SkapaInPoster(
                    nuDatum,
                    transactionList: webBankBudgeter.TransactionHandler?
                        .TransactionList);

            // Merga med föregående inposter.
            var inDataRaderTidigare = await _inBudgetUiHandler.GetInPoster();
            inPosterDefault.AddRange(inDataRaderTidigare);
            _inBudgetUiHandler.SetInPoster(inPosterDefault);

            // Hämta rader i Ui-format
            var inDataRader = await _inBudgetUiHandler
                .HämtaRaderFörUiBindningAsync();
            return inDataRader;
        }

        private void ÅterställInkomstGrid()
        {
            gv_incomes.Columns.Clear();
            gv_incomes.Rows.Clear();

            gv_incomes.Columns.Add("1", WebBankBudgeter.CategoryNameColumnDescription);
        }
    }
}