using BudgeterCore.Entities;

namespace BudgetterarnDAL.EntryLogicSetFlags
{
    public static class KontoEntryKeyChecker
    {
        public static bool CheckIfKeyExistsInKontoEntries(
            this List<KontoEntry> listToSearchIn,
            string keyToSearchFor)
        {
            return listToSearchIn.GetEntryFromKontoEntries(keyToSearchFor) != null;
        }

        private static KontoEntry GetEntryFromKontoEntries(
            this IEnumerable<KontoEntry> listToSearchIn,
            string keyToSearchFor)
        {
            return listToSearchIn.FirstOrDefault(viewItem =>
                viewItem.KeyForThis.Equals(keyToSearchFor));
        }
    }
}