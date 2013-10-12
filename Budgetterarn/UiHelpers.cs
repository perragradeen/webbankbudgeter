using Budgeter.Core.Entities;
using System.Windows.Forms;

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

        public static string ClearSpacesAndReplaceCommas(this string inString)
        {
            return !string.IsNullOrEmpty(inString) ? inString.Replace(" ", string.Empty).Replace(".", ",") : inString;
        }
    }
}
