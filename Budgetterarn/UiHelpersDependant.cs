using Budgeter.Core.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Budgeter.Winforms
{
    public class UiHelpersDependant
    {
        public static AddedAndReplacedEntriesCounter AddNewEntries(SortedList oldKontoEntries, SortedList newEntries)
        {
            var somethingChanged = false;

            var addedEntries = 0;
            var replacedEntries = 0;
            foreach (KontoEntry entry in newEntries.Values)//_newKontoEntries.Values)
            {
                if (!entry.ThisIsDoubleDoNotAdd)
                    if (!oldKontoEntries.ContainsKey(entry.KeyForThis))//(detta ska redan vara kollat)
                    {
                        if (string.IsNullOrEmpty(entry.ReplaceThisKey))//Add new
                        {
                            entry.FontFrontColor = Color.Lime;
                            oldKontoEntries.Add(entry.KeyForThis, entry);
                            addedEntries++;
                        }
                        else //Replace old
                        {
                            entry.FontFrontColor = Color.Blue;//ev. skulle man sätta replacethiskey till den gamla keyn med den som ersatte, för att kunna spåra förändringar
                            if (oldKontoEntries.ContainsKey(entry.ReplaceThisKey)) oldKontoEntries[entry.ReplaceThisKey] = entry;
                            else MessageBox.Show("Error: key not found! : " + entry.ReplaceThisKey);
                            replacedEntries++;
                        }
                        somethingChanged = true;//Här har man tagit in nytt som inte är sparat
                    }
                    else
                    {
                        Console.WriteLine("Double key found!: " + entry.KeyForThis);
                    }
            }
            return new AddedAndReplacedEntriesCounter { SomethingChanged = somethingChanged, Added = addedEntries, Replaced = replacedEntries };
        }
    }

    public class AddedAndReplacedEntriesCounter
    {
        public int Added { get; set; }
        public int Replaced { get; set; }

        public bool SomethingChanged { get; set; }
    }

}
