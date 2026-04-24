using InbudgetHandler.Model;
using WebBankBudgeterService.Model;

namespace InbudgetHandler;

/// <summary>
/// Delad budget-matematik (IN + UT per månad) för WinForms och konsol.
/// </summary>
public static class InBudgetMath
{
    public static List<Rad> SnurraIgenom(
        IEnumerable<Rad> inData,
        List<BudgetRow> utgifter,
        Action<string> writeLineToOutputAndScrollDown)
    {
        if (utgifter == null)
        {
            throw new ArgumentNullException(nameof(utgifter));
        }

        static string CatKey(string? s) => (s ?? string.Empty).Trim();

        var inList = (inData ?? Enumerable.Empty<Rad>())
            .Where(r => !string.Equals(r.RadNamnY, InBudgetHandler.SummaText, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var inByCat = new Dictionary<string, Rad>(StringComparer.Ordinal);
        foreach (var r in inList)
        {
            var key = CatKey(r.RadNamnY);
            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            if (!inByCat.TryGetValue(key, out var existing))
            {
                inByCat[key] = new Rad { RadNamnY = r.RadNamnY };
                existing = inByCat[key];
            }

            foreach (var (mk, v) in r.Kolumner)
            {
                existing.Kolumner.TryGetValue(mk, out var sum);
                existing.Kolumner[mk] = sum + v;
            }
        }

        var utByCat = utgifter
            .GroupBy(u => CatKey(u.CategoryText))
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

        var allCats = new HashSet<string>(StringComparer.Ordinal);
        foreach (var k in inByCat.Keys)
        {
            if (!string.IsNullOrEmpty(k))
            {
                allCats.Add(k);
            }
        }

        foreach (var k in utByCat.Keys)
        {
            if (!string.IsNullOrEmpty(k))
            {
                allCats.Add(k);
            }
        }

        var kvarrader = new List<Rad>();
        foreach (var cat in allCats.OrderBy(c => c, StringComparer.Ordinal))
        {
            inByCat.TryGetValue(cat, out var inBudget);
            utByCat.TryGetValue(cat, out var utRowsForCat);

            var nuvarandeRad = new Rad { RadNamnY = cat };

            var monthKeys = new HashSet<string>(StringComparer.Ordinal);
            if (inBudget != null)
            {
                foreach (var k in inBudget.Kolumner.Keys)
                {
                    monthKeys.Add(k);
                }
            }

            if (utRowsForCat != null)
            {
                foreach (var utRow in utRowsForCat)
                {
                    foreach (var k in utRow.AmountsForMonth.Keys)
                    {
                        monthKeys.Add(k);
                    }
                }
            }

            foreach (var monthKey in monthKeys.OrderBy(k => k, StringComparer.Ordinal))
            {
                var inAmount = 0.0;
                if (inBudget != null && inBudget.Kolumner.TryGetValue(monthKey, out var iv))
                {
                    inAmount = iv;
                }

                var utAmount = 0.0;
                if (utRowsForCat != null)
                {
                    foreach (var utRow in utRowsForCat)
                    {
                        if (utRow.AmountsForMonth.TryGetValue(monthKey, out var uv))
                        {
                            utAmount += uv;
                        }
                    }
                }

                if (inBudget != null && !inBudget.Kolumner.ContainsKey(monthKey) && utRowsForCat != null &&
                    utRowsForCat.Any(r => r.AmountsForMonth.ContainsKey(monthKey)))
                {
                    var message = "Hittar ingen motsvarande inpost för utgift i :"
                                  + monthKey + " och kategori: " + cat;
                    writeLineToOutputAndScrollDown(message);
                }

                nuvarandeRad.Kolumner[monthKey] = inAmount + utAmount;
            }

            kvarrader.Add(nuvarandeRad);
        }

        return kvarrader;
    }
}
