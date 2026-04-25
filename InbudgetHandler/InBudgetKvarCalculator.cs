using InbudgetHandler.Model;
using WebBankBudgeterService.Model;

namespace InbudgetHandler;

/// <summary>
/// Kvar per kategori/månad: IN + UT (UT negativ). Union av kategorier i IN och UT.
/// </summary>
public static class InBudgetKvarCalculator
{
    public static List<Rad> SnurraIgenom(
        IEnumerable<Rad> inData,
        List<BudgetRow> utgifter,
        Action<string>? writeLineToOutputAndScrollDown = null)
    {
        if (utgifter == null)
            throw new ArgumentNullException(nameof(utgifter));

        _ = writeLineToOutputAndScrollDown;

        var inPerCat = new Dictionary<string, Dictionary<string, double>>(StringComparer.Ordinal);
        foreach (var inBudget in inData)
        {
            var cat = inBudget.RadNamnY.Trim();
            if (string.Equals(cat, InBudgetHandler.SummaText, StringComparison.Ordinal))
                continue;

            if (!inPerCat.TryGetValue(cat, out var months))
            {
                months = new Dictionary<string, double>(StringComparer.Ordinal);
                inPerCat[cat] = months;
            }

            foreach (var kv in inBudget.Kolumner)
            {
                months.TryGetValue(kv.Key, out var v);
                months[kv.Key] = v + kv.Value;
            }
        }

        var utPerCat = new Dictionary<string, Dictionary<string, double>>(StringComparer.Ordinal);
        foreach (var u in utgifter)
        {
            var cat = u.CategoryText.Trim();
            if (!utPerCat.TryGetValue(cat, out var months))
            {
                months = new Dictionary<string, double>(StringComparer.Ordinal);
                utPerCat[cat] = months;
            }

            foreach (var kv in u.AmountsForMonth)
            {
                months.TryGetValue(kv.Key, out var v);
                months[kv.Key] = Math.Round(v + kv.Value, 2, MidpointRounding.AwayFromZero);
            }
        }

        var allCats = new HashSet<string>(StringComparer.Ordinal);
        foreach (var c in inPerCat.Keys)
            allCats.Add(c);
        foreach (var c in utPerCat.Keys)
            allCats.Add(c);

        var kvarrader = new List<Rad>();
        foreach (var cat in allCats.OrderBy(c => c, StringComparer.Ordinal))
        {
            inPerCat.TryGetValue(cat, out var inMonths);
            utPerCat.TryGetValue(cat, out var utMonths);
            inMonths ??= new Dictionary<string, double>(StringComparer.Ordinal);
            utMonths ??= new Dictionary<string, double>(StringComparer.Ordinal);

            var monthKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var k in inMonths.Keys)
                monthKeys.Add(k);
            foreach (var k in utMonths.Keys)
                monthKeys.Add(k);

            var nuvarandeRad = new Rad { RadNamnY = cat };
            foreach (var mk in monthKeys.OrderBy(k => k, StringComparer.Ordinal))
            {
                inMonths.TryGetValue(mk, out var inVal);
                utMonths.TryGetValue(mk, out var utVal);
                nuvarandeRad.Kolumner[mk] = Math.Round(inVal + utVal, 2, MidpointRounding.AwayFromZero);
            }

            kvarrader.Add(nuvarandeRad);
        }

        return kvarrader
            .Where(r => r.RadNamnY.Trim() != "-")
            .ToList();
    }
}
