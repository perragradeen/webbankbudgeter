using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Budgeter.Core.Entities;

namespace Budgetterarn
{
    internal static class ViewUpdateUi
    {
        public const int MaxRowDisplay = 1000;

        internal static void ClearListAndSetEntriesToListView(
            ListView showEntriesInThisUiList,
            SortedList kontoEntries)
        {
            if (showEntriesInThisUiList != null)
            {
                showEntriesInThisUiList.Items.Clear();
            }
            else
            {
                throw new Exception("New EntryList is null");
            }

            AddEntriesToListView(showEntriesInThisUiList, kontoEntries);
        }

        internal static void AddEntriesToListView(
            ListView showEntriesInThisUiList,
            IEnumerable<KontoEntry> kontoEntries)
        {
            var list = new SortedList(
                kontoEntries.ToDictionary(d => d.KeyForThis));

            AddEntriesToListView(
                showEntriesInThisUiList,
                list
            );
        }

        internal static void AddEntriesToListView(
            ListView showEntriesInThisUiList,
            SortedList kontoEntries)
        {
            // For performance
            showEntriesInThisUiList.BeginUpdate();

            var rowCounter = 0;
            foreach (KontoEntry kontoEntry in kontoEntries.Values)
            {
                AddToListview(showEntriesInThisUiList, kontoEntry);
                // TODO: bestäm max antal om något
                if (false && RowsExceedMax(ref rowCounter))
                {
                    break; // Begränsa antal synliga rader
                }
            }

            // For performance
            showEntriesInThisUiList.EndUpdate();
        }

        private static bool RowsExceedMax(ref int rowCounter)
        {
            // Begränsa antal synliga rader
            rowCounter++;
            return rowCounter > MaxRowDisplay;
        }

        #region Container adds

        private static void AddToListview(ListView list, KontoEntry entry)
        {
            // Sätt mellanslagstecken ifall en sträng i listan kommer att bli tom eller null,
            // så att det finns något att klicka på och så det inte uppstår exception senare.
            entry.ForUi = true;
            var kontoEntryElements = entry.RowToSaveToUiSwitched;
            entry.ForUi = false;

            var item = new ListViewItem(
                kontoEntryElements,
                -1,
                entry.FontFrontColor,
                Color.Empty,
                null)
            {
                Tag = entry
            };

            list.Items.Add(item);

            // man slipper lite tecken och castningarna o likhetstecknet, iom att detta är en
            // fkn//Overkill? hehe, anal. Trodde jag...nu fick jag ju nytta av det så det så
        }

        #endregion
    }
}