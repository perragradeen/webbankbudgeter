using InbudgetHandler.Model;
using WebBankBudgeterService.Model;

namespace InbudgetHandler;

/// <summary>
/// Kvar per kategori/månad: IN (budget) + UT (utfall; utgifter är negativa).
/// Delad mellan WinForms och konsol — ingen referens till UI.
/// </summary>
public static class InBudgetKvarCalculator
{
    public static List<Rad> SnurraIgenom(
        IEnumerable<Rad> inData,
        List<BudgetRow> utgifter,
        Action<string>? writeLine = null)
    {
        if (utgifter == null)
        {
            throw new ArgumentNullException(nameof(utgifter));
        }

        var kvarrader = new List<Rad>();
        foreach (var inBudget in inData)
        {
            var motsvarandeUtgifterRader = utgifter
                .Where(u => u.CategoryText.Trim() == inBudget.RadNamnY.Trim());

            var nuvarandeRad = new Rad { RadNamnY = inBudget.RadNamnY };
            foreach (var motsvarandeUtgiftsRad in motsvarandeUtgifterRader)
            {
                foreach (var utgiftsMånad in motsvarandeUtgiftsRad.AmountsForMonth)
                {
                    if (inBudget.Kolumner.ContainsKey(utgiftsMånad.Key))
                    {
                        var kvar = inBudget.Kolumner[utgiftsMånad.Key] + utgiftsMånad.Value;

                        if (!nuvarandeRad.Kolumner.ContainsKey(utgiftsMånad.Key))
                        {
                            nuvarandeRad.Kolumner.Add(utgiftsMånad.Key, 0);
                        }

                        nuvarandeRad.Kolumner[utgiftsMånad.Key] += kvar;
                    }
                    else
                    {
                        var message = "Hittar ingen motsvarande inpost för utgift i :"
                                      + utgiftsMånad.Key + " och kategori: " + inBudget.RadNamnY;
                        writeLine?.Invoke(message);
                    }
                }
            }

            kvarrader.Add(nuvarandeRad);
        }

        return kvarrader;
    }
}
