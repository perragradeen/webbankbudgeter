using InbudgetToTable;
using InbudgetToTable.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebBankBudgeter.Service;
using WebBankBudgeter.Service.Model.ViewModel;
using WebBankBudgeter.Service.MonthAvarages;
using WebBankBudgeter.Service.Services;
using WebBankBudgeter.UiBinders;

namespace WebBankBudgeter
{
    public partial class WebBankBudgeterUi : Form
    {
        private readonly TransactionHandler _transactionHandler;
        private readonly InBudgetUiHandler _inBudgetUiHandler;
        private readonly UtgiftsHanterareUiBinder _utgiftsHanterareUiBinder;

        public WebBankBudgeterUi()
        {
            var tableGetter = new TableGetter { AddAverageColumn = true };
            _transactionHandler = new TransactionHandler(
                WriteToOutput,
                tableGetter,
                GetCategoryFilePath()
                );

            InitializeComponent();

            var inBudgetHandler = new InBudgetHandler(InBudgetFilePath);
            _inBudgetUiHandler = new InBudgetUiHandler(inBudgetHandler, gv_incomes);

            _utgiftsHanterareUiBinder = new UtgiftsHanterareUiBinder(gv_budget);

            ReloadButton.Click += new EventHandler(async (s, e) =>
                await ReloadButton_ClickAsync(s, e));
            Load += new EventHandler(async (s, e) =>
                 await Form1_LoadAsync(s, e));
        }

        private string GetCategoryFilePath()
        {
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(
                //< property name = "CategoryPath" value = "Data\Categories.xml" />
                Path.Combine(appPath, @"..\..\..\..\Budgetterarn\Data"),
                //@"\Files\Dropbox\budget\Budgeterarn Release\Data", //TODO:Viktig: fixa riktig sökväg, ev slå ihop winforms-apparna
                @"Categories.xml"
            );
        }

        private string InBudgetFilePath => GetInBudgetFilePath();
        private string GetInBudgetFilePath()
        {
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(appPath, @"TestData\BudgetIns.json");
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
                MessageBox.Show("Error: " + ex.Message);
                ReloadButton.Show();
            }
        }

        private async Task FillTablesAsync()
        {
            InitIncomesUi();
            InitTotalsUi();

            // Hämta, behandla och koppla data till UI
            // var inPosterTask = BindInPosterToUiAsync();

            // Hämta data ---
            var loadSuccess =
                await GetTransactionsAsync();
            if (!loadSuccess) return;
            // --- Hämta data

            // Behandla data ---
            SortTransactions();
            RemoveDuplicates();
            var summedAvaragesForCalc = CalculateMonthlyAvarages();
            var table = TransformToTextTableFromTransactions();
            // --- Behandla data

            // Koppla data till UI ---
            WriteMetaAsSaldoEtcToUi();
            BindTransactionListToUi();
            DescribeReoccuringGroups();
            DescribeStartYear(table);

            BindToBudgetTableUi(table); // TODO: presentera utan decimaler och med tusen avgränsare (iofs nackdel om man ska kopiera till excel el. likn.)
            BindMonthAvaragesToUi(summedAvaragesForCalc);
            // --- Koppla data till UI

            //await inPosterTask;

            //BindTotalsToUi();

            // Ta ut in-data och utgifter.
            var inData = await _inBudgetUiHandler.HämtaRaderFörUiBindningAsync();
            var utgifter = table.BudgetRows.ToList();
            var månadsRubriker = await _inBudgetUiHandler.HämtaRubrikePåInPosterAsync();

            var kvarrader = new List<Rad>();
            // Snurra igenom
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

                            WriteLineToOutputAndScrollDown(message);
                        }
                    }
                }
                kvarrader.Add(nuvarandeRad);
            }
            // Presentera tabell för kvar budget.
            _inBudgetUiHandler.BindInPosterRaderTillUi(
                kvarrader,
                månadsRubriker,
                gv_incomes);

            // Presentera summor för varje kat.
        }

        private MonthAvarages CalculateMonthlyAvarages()
        {
            var monthAvaragesCalcer = new MonthAvaragesCalcs(
                _transactionHandler.TransactionList);
            var summedAvaragesForCalc = monthAvaragesCalcer.GetMonthAvarages();
            return summedAvaragesForCalc;
        }

        private async Task<bool> GetTransactionsAsync()
        {
            return await _transactionHandler.GetTransactionsAsync();
        }

        private void RemoveDuplicates()
        {
            _transactionHandler.RemoveDuplicates();
        }

        private void SortTransactions()
        {
            _transactionHandler.SortTransactions();
        }

        private TextToTableOutPuter TransformToTextTableFromTransactions()
        {
            return _transactionHandler.GetTextTableFromTransactions();
        }
        private const string CategoryNameColumnDescription = "Category . Month->";

        private void InitIncomesUi()
        {
            gv_incomes.Columns.Add("1", CategoryNameColumnDescription);

            //gv_incomes.SortCompare += Gv_incomes_SortCompare;
            //gv_incomes.SortCompare += new DataGridViewSortCompareEventHandler(nisse);
        }

        //private void Gv_incomes_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        //{

        //}

        //private void nisse(object sender, DataGridViewSortCompareEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        private void InitTotalsUi()
        {
            var cNo = gv_Totals.Columns.Add("1", "Description");
            gv_Totals.Columns[cNo].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            gv_Totals.Columns[cNo].CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;

            gv_Totals.Columns.Add("2", "Amount");
        }

        private void DescribeStartYear(TextToTableOutPuter table)
        {
            label1.Text += @"Börjar på år: " + table.SelectedStartYear;
        }

        private void DescribeReoccuringGroups()
        {
            foreach (var group in MonthAvaragesCalcs.RecurringCatGroups)
            {
                WriteToOutput(group + ". ");
            }
        }

        private void BindMonthAvaragesToUi(MonthAvarages summedAvaragesForCalc)
        {
            // bind to ui gv_totals
            AddRowWith2Cells(gv_Totals, "Återkommande snitt", summedAvaragesForCalc.ReccuringCosts);
            AddRowWith2Cells(gv_Totals, "Inkomster snitt", summedAvaragesForCalc.Incomes);
            AddRowWith2Cells(gv_Totals, "Diff snitt", summedAvaragesForCalc.IncomeDiffCosts);
        }

        //private async Task BindInPosterToUiAsync()
        //{
        //    await _inBudgetUiHandler.BindInPosterToUiAsync();
        //}

        private void SparaInPosterPåDisk()
        {
            _inBudgetUiHandler.SparaInPosterPåDisk(
                _inBudgetUiHandler.HämtaInPosterFrånUi());

            WriteLineToOutputAndScrollDown("Sparat.");
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
                _transactionHandler.TransactionList.Account.AvailableAmount;
        }

        public void BindToBudgetTableUi(TextToTableOutPuter table)
        {
            _utgiftsHanterareUiBinder.BindToBudgetTableUi(table);
        }

        private void BindTransactionListToUi()
        {
            dg_Transactions.Columns.Add("1", "Date");
            dg_Transactions.Columns.Add("2", "Amount");
            dg_Transactions.Columns.Add("3", "Description");
            dg_Transactions.Columns.Add("4", "Category");

            foreach (var row in _transactionHandler.TransactionList.Transactions)
            {
                var n = dg_Transactions.Rows.Add();

                var i = 0;
                dg_Transactions.Rows[n].Cells[i++].Value = row.DateAsDate.ToShortDateString();
                dg_Transactions.Rows[n].Cells[i++].Value = row.AmountAsDouble;
                dg_Transactions.Rows[n].Cells[i++].Value = row.Description;
                dg_Transactions.Rows[n].Cells[i].Value = row.CategoryName;
            }
        }

        public void WriteToOutput(string message)
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
                MessageBox.Show("Error: " + ex.Message);
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
    }
}