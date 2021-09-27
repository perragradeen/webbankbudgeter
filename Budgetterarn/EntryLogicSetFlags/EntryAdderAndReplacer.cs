using System;
using System.Collections;
using System.Drawing;
using Budgeter.Core.Entities;

// ReSharper disable LocalizableElement

namespace Budgetterarn.EntryLogicSetFlags
{
    public class EntryAdderAndReplacer
    {
        private readonly SortedList oldEntries;
        private readonly Action<string> writeToOutput;
        private readonly Action<string> addToUiStatusLog;

        private int addedEntries = 0;
        private int replacedEntries = 0;

        public EntryAdderAndReplacer(
            SortedList oldEntries,
            Action<string> writeToOutput,
            Action<string> addToUiStatusLog)
        {
            this.oldEntries = oldEntries;
            this.writeToOutput = writeToOutput;
            this.addToUiStatusLog = addToUiStatusLog;
        }

        public AddedAndReplacedEntriesCounter AddNewEntries(
            SortedList newEntries)
        {
            var somethingChanged = false;

            foreach (KontoEntry entry in newEntries.Values)
            {
                if (entry.ThisIsDoubleDoNotAdd) continue;

                // (Kanske är detta redan kollat?)
                if (oldEntries.ContainsKey(entry.KeyForThis))
                {
                    addToUiStatusLog(@"Double key found!: " + entry.KeyForThis);
                    continue;
                }

                if (string.IsNullOrEmpty(entry.ReplaceThisKey))
                {
                    AddNew(entry);
                }
                else
                {
                    ReplaceOld(entry);
                }

                somethingChanged = true; // Här har man tagit in nytt som inte är sparat
            }

            return new AddedAndReplacedEntriesCounter
            {
                SomethingChanged = somethingChanged,
                Added = addedEntries,
                Replaced = replacedEntries
            };
        }

        private void ReplaceOld(KontoEntry entry)
        {
            entry.FontFrontColor = Color.Blue;

            // ev. skulle man sätta replacethiskey till den gamla keyn med den
            // som ersatte, för att kunna spåra förändringar
            if (CheckIfExistsToReplace(entry))
            {
                oldEntries[entry.ReplaceThisKey] = entry;
                replacedEntries++;
            }
        }

        private bool CheckIfExistsToReplace(KontoEntry entry)
        {
            if (!oldEntries.ContainsKey(entry.ReplaceThisKey))
            {
                writeToOutput(
                    @"Error: key not found! : " + entry.ReplaceThisKey);

                return false;
            }

            return true;
        }

        private void AddNew(KontoEntry entry)
        {
            entry.FontFrontColor = Color.Lime;
            oldEntries.Add(entry.KeyForThis, entry);
            addedEntries++;
        }
    }
}