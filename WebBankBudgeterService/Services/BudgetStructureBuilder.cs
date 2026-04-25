using WebBankBudgeterService.Model;
using WebBankBudgeterService.Model.ViewModel;

namespace WebBankBudgeterService.Services
{
    /// <summary>
    /// Bygger en strukturerad budgetvy med kategorier grupperade enligt typ
    /// (utgifter, inkomster '+', förflyttningar '-') och med månadssammanfattningar
    /// </summary>
    public class BudgetStructureBuilder
    {
        private const string IncomeCategoryName = "+";
        private const string TransferCategoryName = " -";
        private const string ExpensesSummaryRowName = "=== Summa utgifter ===";
        private const string IncomesSummaryRowName = "=== Summa inkomster ===";
        private const string TransfersSummaryRowName = "=== Summa förflyttningar ===";
        private const string TotalBudgetRowName = "=== BUDGET (Inkomster - Utgifter) ===";

        public StructuredBudgetTable BuildStructuredBudget(IEnumerable<BudgetRow> budgetRows, List<string> columnHeaders)
        {
            var result = new StructuredBudgetTable();
            var rows = budgetRows.ToList();

            // Inkomst = exakt "+"; förflyttning = mellanslag före minus (Excel " -"), inte "-" eller "+ i text"
            var incomeRows = rows.Where(r =>
                string.Equals(r.CategoryText?.Trim(), IncomeCategoryName, StringComparison.Ordinal)).ToList();
            var transferRows = rows.Where(r =>
                r.CategoryText != null &&
                r.CategoryText.Contains(TransferCategoryName, StringComparison.Ordinal)).ToList();
            var expenseRows = rows
                .Where(r =>
                    !string.Equals(r.CategoryText?.Trim(), IncomeCategoryName, StringComparison.Ordinal) &&
                    !(r.CategoryText?.Contains(TransferCategoryName, StringComparison.Ordinal) ?? false))
                .OrderBy(r => r.CategoryText)
                .ToList();

            // Bygg strukturerade rader
            result.Rows.AddRange(expenseRows);
            
            // Lägg till summeringsrad för utgifter
            if (expenseRows.Any())
            {
                result.Rows.Add(CreateSummaryRow(ExpensesSummaryRowName, expenseRows, columnHeaders));
            }

            // Lägg till en tom rad för separering
            result.Rows.Add(new BudgetRow { CategoryText = string.Empty });

            // Lägg till inkomster
            result.Rows.AddRange(incomeRows);
            if (incomeRows.Any())
            {
                result.Rows.Add(CreateSummaryRow(IncomesSummaryRowName, incomeRows, columnHeaders));
            }

            // Lägg till en tom rad för separering
            result.Rows.Add(new BudgetRow { CategoryText = string.Empty });

            // Lägg till förflyttningar
            result.Rows.AddRange(transferRows);
            if (transferRows.Any())
            {
                result.Rows.Add(CreateSummaryRow(TransfersSummaryRowName, transferRows, columnHeaders));
            }

            // Lägg till en tom rad för separering
            result.Rows.Add(new BudgetRow { CategoryText = string.Empty });

            // Lägg till total budgetrad (Inkomster - Utgifter)
            result.Rows.Add(CreateBudgetTotalRow(incomeRows, expenseRows, columnHeaders));

            return result;
        }

        private BudgetRow CreateSummaryRow(string rowName, List<BudgetRow> rows, List<string> columnHeaders)
        {
            var summaryRow = new BudgetRow { CategoryText = rowName };

            // Filtrera bort kategorikolumnen och eventuella genomsnittskolumner
            var monthColumns = columnHeaders
                .Where(h => !h.Contains("Category") && !h.Contains("Average"))
                .ToList();

            foreach (var monthColumn in monthColumns)
            {
                double total = 0;
                foreach (var row in rows)
                {
                    if (row.AmountsForMonth.ContainsKey(monthColumn))
                    {
                        total += row.AmountsForMonth[monthColumn];
                    }
                }

                summaryRow.AmountsForMonth[monthColumn] = total;
            }

            return summaryRow;
        }

        private BudgetRow CreateBudgetTotalRow(List<BudgetRow> incomeRows, List<BudgetRow> expenseRows, List<string> columnHeaders)
        {
            var budgetRow = new BudgetRow { CategoryText = TotalBudgetRowName };

            // Filtrera bort kategorikolumnen och eventuella genomsnittskolumner
            var monthColumns = columnHeaders
                .Where(h => !h.Contains("Category") && !h.Contains("Average"))
                .ToList();

            foreach (var monthColumn in monthColumns)
            {
                double incomeTotal = 0;
                double expenseTotal = 0;

                foreach (var row in incomeRows)
                {
                    if (row.AmountsForMonth.ContainsKey(monthColumn))
                    {
                        incomeTotal += row.AmountsForMonth[monthColumn];
                    }
                }

                foreach (var row in expenseRows)
                {
                    if (row.AmountsForMonth.ContainsKey(monthColumn))
                    {
                        expenseTotal += row.AmountsForMonth[monthColumn];
                    }
                }

                // Budget = Inkomster - Utgifter (observera att utgifter är negativa)
                var budget = incomeTotal + expenseTotal; // expenseTotal är redan negativt
                budgetRow.AmountsForMonth[monthColumn] = budget;
            }

            return budgetRow;
        }
    }

    public class StructuredBudgetTable
    {
        public List<BudgetRow> Rows { get; set; } = new List<BudgetRow>();
    }
}

