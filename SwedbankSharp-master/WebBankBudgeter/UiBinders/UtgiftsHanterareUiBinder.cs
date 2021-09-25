using System.Windows.Forms;
using WebBankBudgeter.Service;
using WebBankBudgeter.Service.Model.ViewModel;

namespace WebBankBudgeter.UiBinders
{
    public class UtgiftsHanterareUiBinder
    {
        private readonly DataGridView _gv_budget;

        public UtgiftsHanterareUiBinder(DataGridView gv_budget)
        {
            _gv_budget = gv_budget;
        }

        public void BindToBudgetTableUi(TextToTableOutPuter table)
        {
            if (_gv_budget == null)
                return;
            //throw new ArgumentNullException(nameof(_gv_budget));

            foreach (var column in table.ColumnHeaders)
            {
                _gv_budget.Columns.Add(column, column);
            }

            _gv_budget.Columns[0].CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;

            //TODO: hämta från ny hanterare för averages...var averagesForTransactions = new List<BudgetRow>();
            foreach (var row in table.BudgetRows)
            {
                var n = _gv_budget.Rows.Add();

                // Skriv ut 0 i de kolumner där det inte finns värde för månad
                var i = 0;
                foreach (var header in table.ColumnHeaders)
                {
                    object value;
                    switch (header)
                    {
                        case TextToTableOutPuter.AverageColumnDescription:
                            var amounts = AverageCalcer.CalcMonthAveragesPerRow(
                                row.AmountsForMonth, table.ColumnHeaders);
                            value = AverageCalcer.GetAverageValueAsText(amounts);

                            //amounts.ForEach(a =>
                            //{
                            //    var aomuntsForAverageRow = new BudgetRow
                            //    {
                            //        CategoryText = row.CategoryText
                            //    };
                            //    aomuntsForAverageRow.AmountsForMonth.Add(header, a);
                            //    averagesForTransactions.Add(aomuntsForAverageRow);
                            //});

                            break;
                        case TextToTableOutPuter.CategoryNameColumnDescription:
                            value = row.CategoryText;
                            break;
                        default:
                            value = row.AmountsForMonth.ContainsKey(header)
                                ? row.AmountsForMonth[header] : 0;
                            break;
                    }

                    _gv_budget.Rows[n].Cells[i++].Value = value;
                }
            }

            //table.AveragesForTransactions = averagesForTransactions;
        }
    }
}
