using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using Budgeter.Core.Entities;
using Budgetterarn.Application_Settings_and_constants;

namespace Budgetterarn
{
    internal static class ViewUpdateUi
    {
        internal static void SetNewItemsListViewFromSortedList(ListView showEntriesInThis, SortedList kontoEntries)
        {
            if (showEntriesInThis != null)
            {
                showEntriesInThis.Items.Clear();
            }
            else
            {
                throw new Exception("New EntryList is null");
            }

            var rowCounter = 0;
            foreach (KontoEntry kontoEntry in kontoEntries.Values)
            {
                AddToListview(showEntriesInThis, kontoEntry);

                if (RowsExceedMax(ref rowCounter))
                {
                    break; // Begränsa antal synliga rader
                }
            }
        }

        private static bool RowsExceedMax(ref int rowCounter)
        {
            // Begränsa antal synliga rader
            rowCounter++;
            return rowCounter > UISettings.MaxRowDisplay;
        }

        #region Container adds

        internal static void AddToListview(ListView list, KontoEntry entry)
        {
            // Sätt mellanslagstecken ifall en strän i listan kommer att bli tom eller null, så att det finns något att klicka på och så det inte uppstår exception senare.
            entry.ForUi = true;
            var kontoEntryElements = entry.RowToSaveToUiSwitched; // RowToSaveForThis;
            entry.ForUi = false;

            list.Items.Add(new ListViewItem(kontoEntryElements, -1, entry.FontFrontColor, Color.Empty, null)).Tag =
                entry;

            // man slipper lite tecken och castningarna o likhetstecknet, iom att detta är en fkn//Overkill? hehe, anal. Trodde jag...nu fick jag ju nytta av det så det så
        }

        #endregion
    }
}