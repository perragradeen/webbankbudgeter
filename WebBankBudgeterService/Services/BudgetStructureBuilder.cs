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

        public const string ExpensesSummaryRowName = "=== Summa utgifter ===";
        public const string IncomesSummaryRowName = "=== Summa inkomster ===";
        public const string TransfersSummaryRowName = "=== Summa förflyttningar ===";
        public const string TotalBudgetRowName = "=== BUDGET (Inkomster - Utgifter) ===";

        /// <summary>
        /// Månadskolumner (exkl. kategori och Average) i samma ordning som i <paramref name="columnHeaders"/>.
        /// </summary>
        public static List<string> MonthColumnKeys(IEnumerable<string> columnHeaders) =>
            columnHeaders
                .Where(h => !h.Contains("Category", StringComparison.Ordinal) &&
                            !h.Contains("Average", StringComparison.Ordinal))
                .ToList();

        /// <summary>
        /// Bygger om strukturerad vy från en platt radlista (t.ex. efter att IN slagits in i utgiftsrader).
        /// </summary>
        public StructuredBudgetTable RebuildStructuredBudget(IEnumerable<BudgetRow> budgetRows, List<string> columnHeaders) =>
            BuildStructuredBudget(budgetRows, columnHeaders);

        /// <summary>
        /// Rader som räknas som utgifter i strukturen (före första summeringsrad), för att slå ihop med budget-IN.
        /// </summary>
        public static List<BudgetRow> GetExpenseRowsBeforeFirstSummary(StructuredBudgetTable structured)
        {
            var result = new List<BudgetRow>();
            foreach (var r in structured.Rows)
            {
                var c = r.CategoryText ?? string.Empty;
                if (string.IsNullOrWhiteSpace(c))
                {
                    continue;
                }

                if (c.Contains("===", StringComparison.Ordinal))
                {
                    break;
                }

                result.Add(r);
            }

            return result;
        }

        public StructuredBudgetTable BuildStructuredBudget(IEnumerable<BudgetRow> budgetRows, List<string> columnHeaders)
        {
            var result = new StructuredBudgetTable();
            var rows = budgetRows.ToList();

            // Inkomster är raden med namn "+" (trimmat), inte kategorier som råkar innehålla '+' (t.ex. "värnamoresor+övriga").
            // Förflyttning är exakt trimmat " -" (inte Contains), samma skäl.
            var incomeRows = rows.Where(r => IsIncomeCategoryRow(r.CategoryText)).ToList();
            var transferRows = rows.Where(r => IsTransferCategoryRow(r.CategoryText)).ToList();
            var expenseRows = rows
                .Where(r => !IsIncomeCategoryRow(r.CategoryText) && !IsTransferCategoryRow(r.CategoryText))
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

        private static bool IsIncomeCategoryRow(string categoryText) =>
            string.Equals(categoryText.Trim(), IncomeCategoryName, StringComparison.Ordinal);

        private static bool IsTransferCategoryRow(string categoryText) =>
            string.Equals(categoryText?.Trim(), TransferCategoryName, StringComparison.Ordinal);

        private BudgetRow CreateSummaryRow(string rowName, List<BudgetRow> rows, List<string> columnHeaders)
        {
            var summaryRow = new BudgetRow { CategoryText = rowName };

            // Filtrera bort kategorikolumnen och eventuella genomsnittskolumner
            var monthColumns = MonthColumnKeys(columnHeaders);

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

            var monthColumns = MonthColumnKeys(columnHeaders);

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

