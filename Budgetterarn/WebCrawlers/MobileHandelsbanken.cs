using Budgeter.Core.BudgeterConstants;
using Budgeter.Core.Entities;
using RefLesses;
using System.Collections;
using System.Windows.Forms;
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo

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
            var entryStrings = new BankRow
            {
                DateValue = htmlElement.FirstChild?.InnerText.Trim(),
                EventValue = htmlElement.FirstChild?.NextSibling?.FirstChild?.InnerText.Trim()
            };


            var beloppVal =
                htmlElement.FirstChild?.NextSibling?.FirstChild?.NextSibling?.InnerText.Trim();
            entryStrings.BeloppValue = StringFuncions.RemoveSekFromMoneyString(beloppVal);
            entryStrings.SaldoValue = string.Empty;

            return entryStrings;
        }


        /// <summary>
        /// Körs flera gånger en per sida och får då ut flera olika konton och uppdaterar dess värde i saldo-tabellen.
        /// </summary>
        /// <param name="saldoElement"></param>
        /// <param name="saldoHolder"></param>
        private static void GetMobileHandelsBankenSaldo(HtmlElement saldoElement, SaldoHolder saldoHolder)
        {
            var saldoName = saldoElement.FirstChild?.FirstChild?.InnerText;
            var saldoValueElem = saldoElement.FirstChild?.NextSibling?.NextSibling;

            if (saldoHolder.HasSaldoName(AllkortName)
                || saldoHolder.HasSaldoName("Allkortskonto"))
            {
                if (saldoName != null
                    && saldoName.Contains(AllkortName))
                {
                    // allkortHas = true;
                    saldoName = AllkortName;
                }
            }

            var saldoValue = StringFuncions.RemoveSekFromMoneyString(saldoValueElem?.InnerText).
                GetDoubleValueFromStringEntry();
            saldoHolder.AddToOrChangeValueInDictionaryForKey(saldoName, saldoValue);

            // Kolla disp. belopp
            const string saldoNameDispBelopp = AllkortEjFaktureratName;
            saldoValueElem =
                saldoElement.FirstChild?.NextSibling?.NextSibling?.NextSibling?.FirstChild?.NextSibling;

            if (saldoName == LönekontoName) return;

            var saldoValueDisp = StringFuncions.RemoveSekFromMoneyString(saldoValueElem?.InnerText).
                GetDoubleValueFromStringEntry();

            // Räkna ut mellanskillnaden som motsvarar fakturerat och ej förfallet etc
            const int kreditBelopp = 10000;

            saldoValueDisp = saldoValue + kreditBelopp - saldoValueDisp;

            saldoHolder.AddToOrChangeValueInDictionaryForKey(saldoNameDispBelopp, -saldoValueDisp);
        }
    }
}
