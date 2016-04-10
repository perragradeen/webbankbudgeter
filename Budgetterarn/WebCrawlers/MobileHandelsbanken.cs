using Budgeter.Core.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RefLesses;
using Budgetterarn.DAL;
using Budgetterarn.Model;
using Budgeter.Core;

namespace Budgetterarn.WebCrawlers
{
    public class MobileHandelsbanken : ShbConstants
    {
        public static void GetAllEntriesFromMobileHandelsBanken(
            HtmlElement htmlBody,
            SortedList kontoEntries,
            SortedList newKontoEntries,
            SaldoHolder saldoHolder)
        {
            var baseElement = htmlBody.FirstChild.FirstChild.FirstChild.FirstChild.NextSibling.NextSibling.FirstChild;

            var saldoElement = GetSaldoElement(baseElement);
            GetMobileHandelsBankenSaldo(saldoElement, saldoHolder);

            var kontoEntriesElement = GetKontoEntriesElement(baseElement);
            GetHtmlEntriesFromMobileHandelsbanken(kontoEntriesElement, kontoEntries, newKontoEntries);
        }

        private static HtmlElement GetKontoEntriesElement(HtmlElement baseElement)
        {
            var kontoEntriesElement = baseElement.NextSibling;
            if (kontoEntriesElement.TagName.Equals("UL")) // .GetAttribute("link-list") != null)
            {
            }
            else
            {
                kontoEntriesElement = kontoEntriesElement.NextSibling;
            }

            return kontoEntriesElement;
        }

        private static HtmlElement GetSaldoElement(HtmlElement saldoElement)
        {
           if (saldoElement.TagName.Equals("DIV")) // .GetAttribute("link-list") != null)
            {
            }
            else
            {
                saldoElement = saldoElement.NextSibling;
            }

            if (saldoElement.InnerText.Equals("Korttransaktioner"))
            {
                saldoElement = saldoElement.NextSibling;
            }

            return saldoElement;
        }


        private static void GetHtmlEntriesFromMobileHandelsbanken(
    HtmlElement kontoEntriesElement, SortedList kontoEntries, SortedList newKontoEntries)
        {
            var newBatchOfKontoEntriesAlreadyRed = EntryAdder.GetNewBatchOfKontoEntriesAlreadyRed(kontoEntries, newKontoEntries);

            foreach (HtmlElement htmlElement in kontoEntriesElement.GetElementsByTagName("LI"))
            {
                EntryAdder.AddNewEntryFromStringArray(
                    GetMobileHandelsbankenTableRow(htmlElement),
                    kontoEntries,
                    newKontoEntries,
                    newBatchOfKontoEntriesAlreadyRed);
            }
        }

        private static BankRow GetMobileHandelsbankenTableRow(HtmlElement htmlElement)
        {
            var entryStrings = new BankRow();

            entryStrings.DateValue = htmlElement.FirstChild.InnerText.Trim();
            entryStrings.EventValue = htmlElement.FirstChild.NextSibling.FirstChild.InnerText.Trim();

            var beloppVal = htmlElement.FirstChild.NextSibling.FirstChild.NextSibling.InnerText.Trim();
            entryStrings.BeloppValue = StringFuncions.RemoveSekFromMoneyString(beloppVal);
            entryStrings.SaldoValue = string.Empty;

            return entryStrings;
        }


        /// <summary>
        /// Körs flera gånger en per sida och får då ut flera olika konton och uppdaterar dess värde i saldo-tabellen.
        /// </summary>
        /// <param name="saldoElement"></param>
        /// <param name="saldon"></param>
        private static void GetMobileHandelsBankenSaldo(HtmlElement saldoElement, SaldoHolder saldoHolder)
        {
            var saldoName = saldoElement.FirstChild.FirstChild.InnerText;
            var saldoValueElem = saldoElement.FirstChild.NextSibling.NextSibling;

            var saldoValue = 0.0;

            if (saldoHolder.HasSaldoName(AllkortName)
                || saldoHolder.HasSaldoName("Allkortskonto"))
            {
                if (saldoName.Contains(AllkortName))
                {
                    // allkortHas = true;
                    saldoName = AllkortName;
                }
            }

            if (saldoElement != null)
            {
                saldoValue = StringFuncions.RemoveSekFromMoneyString(saldoValueElem.InnerText).
                    GetDoubleValueFromStringEntry();
                saldoHolder.AddToOrChangeValueInDictionaryForKey(saldoName, saldoValue);
            }

            // Kolla disp. belopp
            var saldoNameDispBelopp = AllkortEjFaktureratName;
            saldoValueElem = saldoElement.FirstChild.NextSibling.NextSibling.NextSibling.FirstChild.NextSibling;

            var saldoValueDisp = 0.0;
            if (saldoElement != null && saldoName != LönekontoName)
            {
                saldoValueDisp = StringFuncions.RemoveSekFromMoneyString(saldoValueElem.InnerText).
                    GetDoubleValueFromStringEntry();

                // Räkna ut mellanskillnaden som motsvarar fakturerat och ej förfallet etc
                const int KreditBelopp = 10000;

                saldoValueDisp = saldoValue + KreditBelopp - saldoValueDisp;

                saldoHolder.AddToOrChangeValueInDictionaryForKey(saldoNameDispBelopp, -saldoValueDisp);
            }
        }
    }
}
