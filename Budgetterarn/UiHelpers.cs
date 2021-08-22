using System.Collections.Generic;
using System.Linq;
using Budgeter.Core.Entities;

namespace Budgetterarn
{
    public static class UiHelpers
    {
        public static bool CheckIfKeyExistsInKontoEntries(this List<KontoEntry> listToSearchIn, string keyToSearchFor)
        {
            return listToSearchIn.GetEntryFromKontoEntries(keyToSearchFor) != null;
        }

        private static KontoEntry GetEntryFromKontoEntries(this IEnumerable<KontoEntry> listToSearchIn, string keyToSearchFor)
        {
            return listToSearchIn.FirstOrDefault(viewItem =>
                viewItem.KeyForThis.Equals(keyToSearchFor));
        }
    }
}