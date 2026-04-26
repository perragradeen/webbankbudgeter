using System.Globalization;
using WebBankBudgeterService.Model.ViewModel;
using WebBankBudgeterService.Services;

namespace WebBankBudgeterUi.UiBinders
{
    public class UtgiftsHanterareUiBinder
    {
        private static readonly CultureInfo NumberCulture = CultureInfo.GetCultureInfo("sv-SE");

        private readonly DataGridView _gv_budget;

        public UtgiftsHanterareUiBinder(DataGridView gv_budget)
        {
            _gv_budget = gv_budget;
        }

        public void BindToBudgetTableUi(TextToTableOutPuter table, DataGridView targetGrid = null)
        {
            var grid = targetGrid ?? _gv_budget;
            if (grid == null)
            {
                return;
            }

            grid.Visible = false;
            grid.SuspendLayout();

            try
            {
                grid.Columns.Clear();
                grid.Rows.Clear();

                // Använd den nya strukturerade budgetbyggaren
                var budgetBuilder = new BudgetStructureBuilder();
                var structuredBudget = budgetBuilder.BuildStructuredBudget(
                    table.BudgetRows,
                    table.ColumnHeaders);

                // Lägg till kolumnrubriker
                foreach (var column in table.ColumnHeaders)
                {
                    grid.Columns.Add(column, column);
                }

                // Lägg till "Summa"-kolumn efter alla andra kolumner
                var sumColumnIndex = grid.Columns.Add("Summa", "Summa");

                grid.Columns[0].CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;

                // Lägg till rader från den strukturerade budgeten
                foreach (var row in structuredBudget.Rows)
                {
                    var n = grid.Rows.Add();
                    var categoryName = row.CategoryText;

                    // Räkna ut radtotal och genomsnitt
                    double rowTotal = 0;
                    int monthCount = 0;

                    // Skriv ut värden för varje kolumn
                    var i = 0;
                    foreach (var header in table.ColumnHeaders)
                    {
                        object value;
                        switch (header)
                        {
                            case TextToTableOutPuter.AverageColumnDescription:
                                // Beräkna genomsnitt baserat på månadskolumner
                                var monthColumns = table.ColumnHeaders
                                    .Where(h => !h.Contains("Category") && !h.Contains("Average"))
                                    .ToList();

                                double sum = 0;
                                int count = 0;
                                foreach (var monthCol in monthColumns)
                                {
                                    if (row.AmountsForMonth.ContainsKey(monthCol))
                                    {
                                        sum += row.AmountsForMonth[monthCol];
                                        count++;
                                    }
                                }

                                value = count > 0 ? sum / count : 0;
                                value = DoubleTo1000SeparatedNoDecimals(value);
                                break;

                            case TextToTableOutPuter.AverageColumnDescriptionNotFormatted:
                                // Samma beräkning men oformaterad
                                var monthCols = table.ColumnHeaders
                                    .Where(h => !h.Contains("Category") && !h.Contains("Average"))
                                    .ToList();

                                double sumVal = 0;
                                int countVal = 0;
                                foreach (var monthCol in monthCols)
                                {
                                    if (row.AmountsForMonth.ContainsKey(monthCol))
                                    {
                                        sumVal += row.AmountsForMonth[monthCol];
                                        countVal++;
                                    }
                                }

                                value = countVal > 0 ? sumVal / countVal : 0;
                                break;

                            case TextToTableOutPuter.CategoryNameColumnDescription:
                                value = row.CategoryText;
                                break;

                            default:
                                // Månadsvärde
                                double monthValue = row.AmountsForMonth.ContainsKey(header)
                                    ? row.AmountsForMonth[header]
                                    : 0;

                                rowTotal += monthValue;
                                monthCount++;

                                value = DoubleTo1000SeparatedNoDecimals(monthValue);
                                break;
                        }

                        grid.Rows[n].Cells[i++].Value = value;

                        // Formatera summeringsrader med fet stil
                        if (categoryName.Contains("==="))
                        {
                            grid.Rows[n].Cells[i - 1].Style.Font =
                                new Font(grid.DefaultCellStyle.Font, FontStyle.Bold);
                            grid.Rows[n].Cells[i - 1].Style.BackColor = Color.LightGray;
                        }
                    }

                    // Lägg till radtotalen i sista kolumnen
                    grid.Rows[n].Cells[sumColumnIndex].Value = DoubleTo1000SeparatedNoDecimals(rowTotal);

                    // Formatera summakolumnen också för summeringsrader
                    if (categoryName.Contains("==="))
                    {
                        grid.Rows[n].Cells[sumColumnIndex].Style.Font =
                            new Font(grid.DefaultCellStyle.Font, FontStyle.Bold);
                        grid.Rows[n].Cells[sumColumnIndex].Style.BackColor = Color.LightGray;
                    }
                }
            }
            finally
            {
                grid.ResumeLayout();
                grid.Visible = true;
            }
        }

        private object DoubleTo1000SeparatedNoDecimals(object value)
        {
            return ((double)value).ToString("N0", NumberCulture);
        }
    }
}
