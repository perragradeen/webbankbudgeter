using InbudgetHandler.Model;
using WebBankBudgeterService.Services;
using WebBankBudgeterTests.Facit;

namespace ConsoleBudgeter;

/// <summary>
/// Mappar facit <c>budget-in-*.json</c> till <see cref="Rad"/> för samma merge som WinForms (<see cref="InbudgetHandler.BudgetTableInMerger"/>).
/// </summary>
public static class FacitInBudgetRows
{
    public static List<Rad> FromFacit(IEnumerable<BudgetInFacit> budgetIn)
    {
        var list = new List<Rad>();
        foreach (var g in budgetIn.GroupBy(b => b.Category, StringComparer.Ordinal))
        {
            var rad = new Rad { RadNamnY = g.Key };
            foreach (var item in g)
            {
                var mk = FacitBudgetTextTableFactory.MonthKey(item.Year, item.Month);
                rad.Kolumner.TryGetValue(mk, out var cur);
                rad.Kolumner[mk] = cur + item.BudgetAmount;
            }

            list.Add(rad);
        }

        return list.OrderBy(r => r.RadNamnY, StringComparer.Ordinal).ToList();
    }
}
