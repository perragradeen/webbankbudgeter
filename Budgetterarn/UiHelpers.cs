using System.Windows.Forms;
using Budgeter.Core.Entities;
using System.Collections.Generic;

namespace Budgeter.Winforms
{
    public static class UiHelpers
    {
        public static bool CheckIfKeyExistsInUiControl(this ListView listToSearchIn, string keyToSearchFor)
        {
            return listToSearchIn.GetEntryFromUiControl(keyToSearchFor) != null;
        }

        private static ListViewItem GetEntryFromUiControl(this ListView listToSearchIn, string keyToSearchFor)
        {
            foreach (ListViewItem viewItem in listToSearchIn.Items)
            {
                if (((KontoEntry)viewItem.Tag).KeyForThis.Equals(keyToSearchFor))
                {
                    return viewItem;
                }
            }

            return null;
        }

        public static bool CheckIfKeyExistsInKontoEntries(this List<KontoEntry> listToSearchIn, string keyToSearchFor)
        {
            return listToSearchIn.GetEntryFromKontoEntries(keyToSearchFor) != null;
        }

        private static KontoEntry GetEntryFromKontoEntries(this List<KontoEntry> listToSearchIn, string keyToSearchFor)
        {
            foreach (KontoEntry viewItem in listToSearchIn)
            {
                if (viewItem.KeyForThis.Equals(keyToSearchFor))
                {
                    return viewItem;
                }
            }

            return null;
        }

        public static string ClearSpacesAndReplaceCommas(this string inString)
        {
            return !string.IsNullOrEmpty(inString) ? inString.Replace(" ", string.Empty).Replace(".", ",") : inString;
        }
    }
}