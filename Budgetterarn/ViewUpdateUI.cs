using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using Budgeter.Core.Entities;
using Budgetterarn.Application_Settings_and_constants;

namespace Budgetterarn
{
    internal class ViewUpdateUi
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
            if (rowCounter > UISettings.MaxRowDisplay)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #region Container adds

        private void AddToListview(ListView list, string[] items)
        {
            for (var itemIndex = 0; itemIndex < items.Length; itemIndex++)
            {
                if (string.IsNullOrEmpty(items[itemIndex]))
                {
                    items[itemIndex] = " "; // kanska kan göras på annat ställe
                }
            }

            list.Items.Add(new ListViewItem(items));
                
                // Overkill? hehe, anal. Trodde jag...nu fick jag ju nytta av det så det så
        }

        internal static void AddToListview(ListView list, KontoEntry entry)
        {
            // Sätt mellanslagstecken ifall en strän i listan kommer att bli tom eller null, så att det finns något att klicka på och så det inte uppstår exception senare.
            entry.ForUi = true;
            var kontoEntryElements = entry.RowToSaveToUiSwitched; // RowToSaveForThis;
            entry.ForUi = false;

            // for (var itemIndex = 0; itemIndex < kontoEntryElements.Length; itemIndex++)//hm, denna kan man nog inte ha här o räkna med bra resultat, men o andra sidan så är det bara för att comboboxen ska dyka upp visuellt, så detta e lugnt
            // {
            // if (string.IsNullOrEmpty(kontoEntryElements[itemIndex])) {
            // kontoEntryElements[itemIndex] = " ";//kanska kan göras på annat ställe
            // }
            // }

            // System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            // "Neww"}, -1, System.Drawing.Color.Lime, System.Drawing.Color.Empty, null);

            // byt plats på typavkat och kostnad
            // var kostnad = kontoEntryElements[2];
            // var typAvkostnad = kontoEntryElements[5];

            // kontoEntryElements[2] = typAvkostnad;
            // kontoEntryElements[5] = typAvkostnad;

            list.Items.Add(new ListViewItem(kontoEntryElements, -1, entry.FontFrontColor, Color.Empty, null)).Tag =
                entry;
                
                // man slipper lite tecken och castningarna o likhetstecknet, iom att detta är en fkn//Overkill? hehe, anal. Trodde jag...nu fick jag ju nytta av det så det så
        }

        #endregion
    }
}