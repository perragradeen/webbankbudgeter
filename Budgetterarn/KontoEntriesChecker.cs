﻿using System.Collections;
using System.Drawing;
using System.Linq;
using Budgeter.Core;
using Budgeter.Core.Entities;
using CategoryHandler;

// ReSharper disable CommentTypo

namespace Budgetterarn
{
    public static class KontoEntriesChecker
    {
        const string Skb = "skyddat belopp";
        const string Pkk = "PREL. KORTKÖP";
        private static bool okToAddFromOld;

        public static bool OkToAddFromOld { get => okToAddFromOld; set => okToAddFromOld = value; }

        /// <summary>
        /// Sets color if some conditions exists
        /// </summary>
        /// <param name="newKe">Entry to change</param>
        /// <param name="entry">CurrentEntry to read</param>
        /// <returns></returns>
        private static bool DidSatKeyForEntry(KontoEntry newKe, KontoEntry entry)
        {
            if (entry.Date != newKe.Date
                || !entry.KostnadEllerInkomst.Equals(newKe.KostnadEllerInkomst)) return true;

            // Ersätt skb
            if (entry.Info?.ToLower() == Skb.ToLower() || entry.Info?.ToLower() == Pkk.ToLower())
            {
                newKe.FontFrontColor = entry.FontFrontColor = Color.DeepSkyBlue;

                // Ta de gamla saldot
                newKe.SaldoOrginal = entry.SaldoOrginal;
                newKe.AckumuleratSaldo = entry.AckumuleratSaldo;

                // Vid senare ersättande, så kommer typen vara den nya, eftersom det är den som autokattats, och då stämmer det nog bättre än den som kan vara skyddat belopp. Anv. kan ju även alltid sätta själv innan sparning
                // Är inget autokattat, så ta den gamla, man har säkert gissat rätt
                if (string.IsNullOrEmpty(newKe.TypAvKostnad))
                {
                    newKe.TypAvKostnad = entry.TypAvKostnad;
                }

                newKe.ReplaceThisKey = entry.KeyForThis;
            }
            else
            {
                // Det är kanske en dubblett
                newKe.FontFrontColor = entry.FontFrontColor = Color.Red;
                newKe.ThisIsDoubleDoNotAdd = true;
            }

            return false; // En entry ska bara kunna ersätta En annan entry
        }

        /// <summary>Hjäpfunnktion till CheckAndAddNewItems
        /// SweEnglish rules!
        /// Prestandainfo. Loop i loop...
        /// </summary>
        /// <param name="newKe"></param>
        /// <param name="kontoEntries"></param>
        public static void CheckForSkyddatBeloppMatcherAndGuessDouble(
            KontoEntry newKe,
            SortedList kontoEntries)
        {
            foreach (KontoEntry entry in kontoEntries.Values)
            {
                // Om entryn inte är av typen regulär skippa jämförelser av den.
                // Det kan t.ex. vara mathandling, som delas upp i hemlagat o hygien, eller Periodens köp, som inte ska räknas med som vanlgt och ej heller jämföras
                if (entry.EntryType != KontoEntryType.Regular
                    || string.IsNullOrEmpty(entry.Info))
                {
                    continue;
                }

                if (!DidSatKeyForEntry(newKe, entry))
                {
                    return;
                }

            }
        }

        public static void CheckAndAddNewItemsForLists(KontoEntriesViewModelListUpdater lists)
        {
            // TODO: flytta denna till annan fil, ev. skicka med fkn som delegat
            // Skriv in nya entries i textrutan
            if (lists.NewKontoEntriesIn.Count <= 0) return;

            foreach (DictionaryEntry item in lists.NewKontoEntriesIn)
            {
                if (!(item.Value is KontoEntry newKe))
                {
                    continue;
                }

                var foundDoubleInUList = lists.NewItemsListEdited
                                             .CheckIfKeyExistsInKontoEntries(newKe.KeyForThis)
                                         || lists.NewItemsListEdited
                                             .Any(
                                                 viewItem =>
                                                     (viewItem).KeyForThis.Equals(
                                                         newKe.KeyForThis));

                // Om man laddar html-entries 2 gånger i rad, så ska det inte skapas dubletter
                if (foundDoubleInUList)
                {
                    continue;
                }

                // Lägg till i org
                lists.NewItemsListOrg?.Add(newKe);

                // Kolla om det är en dubblet eller om det är finns ett motsvarade "skyddat belopp"
                if (lists.KontoEntries.ContainsKey(newKe.KeyForThis) && !okToAddFromOld)
                {
                    continue;
                }

                // kolla om det är "Skyddat belopp", dubblett o likn. innan man ändrar entryn, med autokat

                // Slå upp autokategori
                var lookedUpCat = CategoriesHolder.AutocategorizeType(newKe.Info);
                if (lookedUpCat != null)
                {
                    newKe.TypAvKostnad = lookedUpCat;
                }

                // Lägg till i edited
                lists.NewItemsListEdited.Add(newKe);
            }

            okToAddFromOld = false;
        }
    }
}
