using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
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
                            value = CalcMonthAvaragesPerRow(
                                row.AmountsForMonth, table.ColumnHeaders); //row.AmountAverageText;
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
        }


        private static object CalcMonthAvaragesPerRow(Dictionary<string, double> rowAmountsForMonth, List<string> tableColumnHeaders)
        {
            var amounts = new List<double>();
            foreach (var columHeader in tableColumnHeaders)
            {
                switch (columHeader)
                {
                    case TextToTableOutPuter.AverageColumnDescription:
                        break;
                    case TextToTableOutPuter.CategoryNameColumnDescription:
                        break;
                    default:
                        if (rowAmountsForMonth.ContainsKey(columHeader))
                        {
                            amounts.Add(rowAmountsForMonth[columHeader]);
                        }
                        else
                        {
                            amounts.Add(0);
                        }
                        break;
                }
            }

            return amounts.Average(d => d).ToString("N");
        }
    }
}
