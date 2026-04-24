using WebBankBudgeterService.Model;

namespace WebBankBudgeterService.Model.ViewModel;

public static class TextToTableOutPuterClone
{
    public static TextToTableOutPuter Clone(TextToTableOutPuter? source)
    {
        if (source == null)
        {
            return new TextToTableOutPuter();
        }

        var copy = new TextToTableOutPuter
        {
            UtgiftersStartYear = source.UtgiftersStartYear,
            AveragesForTransactions = source.AveragesForTransactions
        };

        copy.ColumnHeaders.AddRange(source.ColumnHeaders);

        if (source.BudgetRows != null)
        {
            var rows = new List<BudgetRow>();
            foreach (var r in source.BudgetRows)
            {
                var nr = new BudgetRow { CategoryText = r.CategoryText };
                foreach (var kv in r.AmountsForMonth)
                {
                    nr.AmountsForMonth[kv.Key] = kv.Value;
                }

                rows.Add(nr);
            }

            copy.BudgetRows = rows;
        }

        return copy;
    }
}
