using Budgeter.Core.Entities;
using System.Collections;
using System.Drawing;

namespace Budgetterarn
{
    public class SkyddatBeloppChecker
    {
        const string Skb = "skyddat belopp";
        const string Pkk = "PREL. KORTKÖP";

        private static bool SetColorOnGridRowDependingOnCondition(
            KontoEntry entryNew,
            KontoEntry entryOld)
        {
            if (EntriesHasDiffrentBaseData(entryNew, entryOld))
            {
                return true;
            }

            // Ersätt skb
            if (entryOld.Info?.ToLower() == Skb.ToLower()
                || entryOld.Info?.ToLower() == Pkk.ToLower())
            {
                entryNew.FontFrontColor =
                entryOld.FontFrontColor =
                    Color.DeepSkyBlue;

                // Ta de gamla saldot
                entryNew.SaldoOrginal = entryOld.SaldoOrginal;
                entryNew.AckumuleratSaldo = entryOld.AckumuleratSaldo;

                // Vid senare ersättande, så kommer typen vara den nya,
                // eftersom det är den som autokattats, och då stämmer
                // det nog bättre än den som kan vara skyddat belopp.
                // Anv. kan ju även alltid sätta själv innan sparning
                // Är inget autokattat, så ta den gamla, man har säkert
                // gissat rätt
                if (string.IsNullOrEmpty(entryNew.TypAvKostnad))
                {
                    entryNew.TypAvKostnad = entryOld.TypAvKostnad;
                }

                entryNew.ReplaceThisKey = entryOld.KeyForThis;
            }
            else
            {
                // Det är kanske en dubblett
                entryNew.FontFrontColor =
                entryOld.FontFrontColor =
                    Color.Red;

                entryNew.ThisIsDoubleDoNotAdd = true;
            }

            return false; // En entry ska bara kunna ersätta En annan entry
        }

        private static bool EntriesHasDiffrentBaseData(
            KontoEntry entryNew,
            KontoEntry entryOld)
        {
            return
                entryOld.Date != entryNew.Date
                
                || !entryOld.KostnadEllerInkomst.Equals(
                        entryNew.KostnadEllerInkomst);
        }

        /// <summary> Hjälpfunnktion till CheckAndAddNewItems
        /// Prestandainfo. Loop i loop...
        /// </summary>
        /// <param name="entryNew"></param>
        /// <param name="kontoEntries"></param>
        public static void CheckForSkyddatBeloppMatcherAndGuessDouble(
            KontoEntry entryNew,
            SortedList kontoEntries)
        {
            // kolla om det är "Skyddat belopp", dubblett o likn.
            // innan man ändrar entryn, med autokat
            foreach (KontoEntry entry in kontoEntries.Values)
            {
                // Om entryn inte är av typen regulär skippa jämförelser
                // av den.
                // Det kan t.ex. vara mathandling, som delas upp i
                // hemlagat o hygien, eller Periodens köp, som inte
                // ska räknas med som vanlgt och ej heller jämföras
                if (entry.EntryType != KontoEntryType.Regular
                    || string.IsNullOrEmpty(entry.Info))
                {
                    continue;
                }

                if (!SetColorOnGridRowDependingOnCondition(entryNew, entry))
                {
                    return;
                }

            }
        }
    }
}