using WebBankBudgeter.Service.Model.ViewModel;

namespace WebBankBudgeterUi.UiBinders {
    public class UtgiftsHanterareUiBinder {
        private readonly DataGridView _gv_budget;

        public UtgiftsHanterareUiBinder(DataGridView gv_budget) {
            _gv_budget = gv_budget;
        }

        public void BindToBudgetTableUi(TextToTableOutPuter table) {
            if (_gv_budget == null)
                return;

            foreach (var column in table.ColumnHeaders) {
                _gv_budget.Columns.Add(column, column);
            }

            _gv_budget.Columns[0].CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;

            foreach (var row in table.BudgetRows) {
                var n = _gv_budget.Rows.Add();

                // Skriv ut 0 i de kolumner där det inte finns värde för månad
                var i = 0;
                foreach (var header in table.ColumnHeaders) {
                    var categoryName = row.CategoryText;
                    object value;
                    switch (header) {
                        case TextToTableOutPuter.AverageColumnDescription:
                            value = table.GetAverageForCategory(categoryName);
                            value = DoubleTo1000SeparatedNoDecimals(value);
                            break;

                        case TextToTableOutPuter.AverageColumnDescriptionNotFormatted:
                            value = table.GetAverageForCategory(categoryName);
                            break;

                        case TextToTableOutPuter.CategoryNameColumnDescription:
                            value = row.CategoryText;
                            break;

                        default:
                            value = row.AmountsForMonth.ContainsKey(header)
                                ? row.AmountsForMonth[header]
                                : 0;
                            value = DoubleTo1000SeparatedNoDecimals(value);
                            break;
                    }

                    _gv_budget.Rows[n].Cells[i++].Value = value;
                }
            }
        }

        private object DoubleTo1000SeparatedNoDecimals(object value) {
            return ((double)value).ToString("N0");
        }
    }
}