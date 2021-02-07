using Budgeter.Core.BudgeterConstants;
using Budgeter.Core.Entities;
using Budgetterarn.WebCrawlers;
using LoadTransactionsFromFile;
using LoadTransactionsFromFile.DAL;
using RefLesses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

//using Microsoft.Office.Interop.Excel;
//using Application = Microsoft.Office.Interop.Excel.Application;

namespace Budgetterarn.DAL
{
    public class LoadKonton : ShbConstants
    {
        /// <summary>
        /// Sparar till Excel-fil
        /// </summary>
        public static LoadOrSaveResult GetAllEntriesFromExcelFile(
            KontoutdragInfoForLoad kontoutdragInfoForLoad,
            SortedList saveToTable,
            SaldoHolder saldoHolder,
            Hashtable entriesLoadedFromDataStore)
        {
            // Töm alla tidigare entries i minnet om det ska laddas helt ny fil el. likn. 
            if (kontoutdragInfoForLoad.ClearContentBeforeReadingNewFile)
            {
                saveToTable.Clear();
            }

            // Görs i Ui-handling, UpdateEntriesToSaveMemList();
            // Skapa kontoentries
            // För att se om det laddats något, så UI-uppdateras etc. Så returneras bool om det...
            return LoadKontonDal.SkapaKontoEntries(saveToTable, entriesLoadedFromDataStore, saldoHolder);
        }

        public static Hashtable LoadEntriesFromFile(
            KontoutdragInfoForLoad kontoutdragInfoForLoad)
        {
            return LoadEntriesFromFileHandler.LoadEntriesFromFile(kontoutdragInfoForLoad);
        }

        //private static LoadOrSaveResult SkapaKontoEntries(
        //    SortedList saveToTable, Hashtable entriesLoadedFromDataStore, SaldoHolder saldoHolder)
        //{
        //    var loadResult = new LoadOrSaveResult();

        //    foreach (DictionaryEntry item in entriesLoadedFromDataStore)
        //    {
        //        if (item.Value != null)
        //        {
        //            var entryArray = ((ExcelRowEntry)item.Value).Args;

        //            // Om det är tomt
        //            if (entryArray == null)
        //            {
        //                continue;
        //            }

        //            // Om det är kolumnbeskrivning, skippa...
        //            if ((string)entryArray[0] == "y")
        //            {
        //                // var saldoAllkortKreditFakturerat = entryArray.Length > 15 ? entryArray[15] ?? saldoAllkortKreditFakturerat : saldoAllkortKreditFakturerat;
        //                var saldoColumnNumber = 11;
        //                if (ProgramSettings.BankType == BankType.Swedbank)
        //                {
        //                    foreach (var saldoName in SwedbankSaldonames)
        //                    {
        //                        var saldot = entryArray.Length > saldoColumnNumber + 1
        //                                         ? entryArray[saldoColumnNumber + 1] ?? string.Empty
        //                                         : string.Empty; // Todo, byt empty mot värden i saldon

        //                        saldoHolder.AddToOrChangeValueInDictionaryForKey(saldoName,
        //                            saldot.ToString().GetDoubleValueFromStringEntry());

        //                        saldoColumnNumber++;
        //                    }
        //                }
        //                else if (ProgramSettings.BankType == BankType.Mobilhandelsbanken)
        //                {
        //                    // Spara saldon, använd det gamla värdet om inget nytt hittats från fil.
        //                    //var saldoLöne = saldoHolder.GetSaldoForName(LönekontoName);
        //                    //var saldoAllkort = saldoHolder.GetSaldoForName(AllkortName);
        //                    //var saldoAllkortKreditEjFakturerat = saldoHolder.GetSaldoForName(EjFaktureratEtcName);

        //                    //var saldoLöne = GetValueIfNotEmpty(entryArray, 12);
        //                    //var saldoAllkort = (string)(entryArray.Length > 13 ? entryArray[13] ?? saldoAllkort : saldoAllkort);
        //                    //var saldoAllkortKreditEjFakturerat = (string)(entryArray.Length > 14
        //                    //                                              ? entryArray[14] ?? saldoAllkortKreditEjFakturerat
        //                    //                                              : saldoAllkortKreditEjFakturerat);


        //                    saldoHolder.AddToOrChangeValueInDictionaryForKey(LönekontoName, GetValueIfNotEmpty(entryArray, 12));
        //                    saldoHolder.AddToOrChangeValueInDictionaryForKey(AllkortName, GetValueIfNotEmpty(entryArray, 13));
        //                    saldoHolder.AddToOrChangeValueInDictionaryForKey(
        //                        AllkortEjFaktureratName, GetValueIfNotEmpty(entryArray, 14)

        //                        // + saldoAllkortKreditFakturerat.GetValueFromEntry()
        //                        );
        //                }

        //                // Hoppa över
        //                continue;
        //            }

        //            var newKe = new KontoEntry(entryArray, true);
        //            var key = newKe.KeyForThis; // item.Key as string;

        //            // Lägg till orginalraden, gör i UI-hanterare
        //            if (!saveToTable.ContainsKey(key))
        //            {
        //                #region old debug

        //                // AddToRichTextBox(richTextBox1, newKE.RowToSaveForThis);

        //                // test debug
        //                // if (_newKontoEntries.Count < 6)
        //                // {
        //                // if (!_newKontoEntries.ContainsKey(key))
        //                // {
        //                // _newKontoEntries.Add(key, newKE);
        //                // //AddToListview(m_newIitemsListOrg, newKE);
        //                // }
        //                // }
        //                // else 
        //                #endregion

        //                saveToTable.Add(key, newKe); // CreateKE(entryArray, true)

        //                loadResult.somethingLoadedOrSaved = true;
        //            }
        //            else
        //            {
        //                // Detta ordnar sig, så länge saldot är med i nyckeln, det är den, så det gäller bara att ha rätt saldo i xls //Om man tagit utt t.ex. 100kr 2 ggr samma dag, från samma bankomat. hm, sätt 1 etta efteråt, men det göller ju bara det som är såna, hm, får ta dem manuellt

        //                // skulle kunna tillåta någon inläsning här ev. 
        //                // om man kan förutsätta att xls:en är kollad, 
        //                // det får bli här man lägger till specialdubbletter manuellt
        //                Console.WriteLine("Entry Double found. Key = " + key);

        //                // meddela detta till usern, man ser de på skipped...
        //                loadResult.skippedOrSaved++;
        //            }
        //        }
        //    }

        //    return loadResult;
        //}

        //private static string GetValueIfNotEmpty(object[] entryArray, int p)
        //{
        //    if (entryArray.Length <= p)
        //    {
        //        return null;
        //    }

        //    var textValue = (string)entryArray[p];

        //    return (string.IsNullOrEmpty(textValue) ? textValue : null);
        //}

        public static IEnumerable<HtmlWindow> ToIEnumerableHtmlWindows(IEnumerator enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return (HtmlWindow)enumerator.Current;
            }
        }

        internal static bool GetAllVisibleEntriesFromWebBrowser(
            KontoEntriesHolder kontoEntriesHolder,
            WebBrowser webBrowser1
            //SortedList kontoEntries,
            //SortedList newKontoEntries,
            //ref bool somethingChanged,
            //SaldoHolder saldoHolder
            )
        {
            if (webBrowser1 == null || webBrowser1.Document == null)
            {
                return false;
            }

            var noKe = kontoEntriesHolder.KontoEntries.Count; // Se om något ändras sen...
            var noNewKontoEntriesBeforeLoading = kontoEntriesHolder.NewKontoEntries.Count;

            // Kolla browser efter entries.
            if (webBrowser1.Document.Window != null)
            {
                switch (ProgramSettings.BankType)
                {
                    case BankType.Handelsbanken:

                        #region Handelsbanken

                        // bool nextIsAllkreditFaktureratEtc;

                        // Kolla även huvuddocet
                        kontoEntriesHolder.Doc = webBrowser1.Document.Window.Document;
                        var docChecker = new DocChecker(kontoEntriesHolder);
                        //webBrowser1.Document.Window.Document,
                        //kontoEntries, newKontoEntries, ref somethingChanged, saldoHolder);
                        docChecker.CheckDocForEntries();

                        if (webBrowser1.Document.Window.Frames != null)
                        {
                            var enumerator = webBrowser1.Document.Window.Frames.GetEnumerator();
                            var frames = ToIEnumerableHtmlWindows(enumerator);

                            frames.ToList().ForEach(_ => docChecker.CheckDocForEntries());
                        }
                        #endregion

                        break;
                    case BankType.Swedbank:

                        #region Swedbank

                        if (webBrowser1.Document.Body != null)
                        {
                            // Get saldo
                            GetSwedbankSaldo(webBrowser1.Document.Body, kontoEntriesHolder.SaldoHolder);


                            // Get Entries
                            foreach (HtmlElement htmlElement in webBrowser1.Document.All)
                            {
                                //if (htmlElement.InnerText.Contains("Kontohändelse"))
                                //{
                                //}
                                //  Get kontoentriestable
                                if (htmlElement.InnerText != null &&
                                    htmlElement.TagName == "H3" &&
                                    htmlElement.InnerText == ("De senaste transaktionerna")) //Kontohändelse
                                {
                                    //return htmlElement.Parent.Parent.Children;
                                    GetHtmlEntriesFromSwedBank(
                                        htmlElement.Parent.Children[1].Children,
                                        kontoEntriesHolder.KontoEntries,
                                        kontoEntriesHolder.NewKontoEntries);

                                    if (htmlElement.Parent.Children.Count < 3)
                                    {
                                        break;
                                    }

                                    GetHtmlEntriesFromSwedBank(
                                      htmlElement.Parent.Children[3].Children[0].Children,
                                      kontoEntriesHolder.KontoEntries,
                                      kontoEntriesHolder.NewKontoEntries);

                                    if (htmlElement.Parent.Children.Count < 5)
                                    {
                                        break;
                                    }

                                    GetHtmlEntriesFromSwedBank(
                                      htmlElement.Parent.Children[5].Children[0].Children,
                                      kontoEntriesHolder.KontoEntries,
                                      kontoEntriesHolder.NewKontoEntries);


                                    break;
                                }
                            }
                            // Get Entries
                            //GetHtmlEntriesFromSwedBank(
                            //webBrowser1.Document.Body.FirstChild.NextSibling.NextSibling.NextSibling.FirstChild.NextSibling.FirstChild.FirstChild.FirstChild.FirstChild.FirstChild.FirstChild.FirstChild.FirstChild.NextSibling.NextSibling.FirstChild.NextSibling.FirstChild.NextSibling.NextSibling.NextSibling
                            //    .Children[1].Children,
                            //kontoEntriesHolder.KontoEntries,
                            //kontoEntriesHolder.NewKontoEntries);
                        }

                        #endregion

                        break;
                    case BankType.Mobilhandelsbanken:
                        var htmlBody = webBrowser1.Document.Body;
                        if (htmlBody != null)
                        {
                            MobileHandelsbanken.GetAllEntriesFromMobileHandelsBanken
                                    (htmlBody,
                                    kontoEntriesHolder.KontoEntries,
                                    kontoEntriesHolder.NewKontoEntries,
                                    kontoEntriesHolder.SaldoHolder);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (kontoEntriesHolder.KontoEntries.Count != noKe)
            {
                kontoEntriesHolder.SomethingChanged = true; // Här har man tagit in nytt som inte är sparat
            }

            // Returnera aom något ändrats. Är de nya inte samma som innan laddning, så är det sant att något ändrats.
            return kontoEntriesHolder.NewKontoEntries.Count != noNewKontoEntriesBeforeLoading;
        }

        #region Swedbank

        private static void GetSwedbankSaldo(HtmlElement htmlElement, SaldoHolder saldoHolder)
        {
            var saldoName = string.Empty;
            foreach (var currentSaldoName in SwedbankSaldonames)
            {
                if (htmlElement.InnerText.Contains(currentSaldoName))
                {
                    saldoName = currentSaldoName;
                }
            }

            if (string.IsNullOrEmpty(saldoName))
                return;

            var saldoValue = StringFuncions.GetTextBetweenStartAndEndText(
            htmlElement.InnerText, "Saldo", "Skyddade belopp");
            if (string.IsNullOrEmpty(saldoValue))
            {
                saldoValue = StringFuncions.GetTextBetweenStartAndEndText(
               htmlElement.InnerText, "Saldo", "Tillgängligt belopp");
            }

            saldoHolder.AddToOrChangeValueInDictionaryForKey(saldoName, saldoValue);
        }

        private static void GetHtmlEntriesFromSwedBank(
            HtmlElementCollection htmlElements, SortedList kontoEntries, SortedList newKontoEntries)
        {
            // Spara en batch, dyker det upp dubletter i samma, så ska de ses som unika
            var newBatchOfKontoEntriesAlreadyRed = EntryAdder.GetNewBatchOfKontoEntriesAlreadyRed(kontoEntries, newKontoEntries);

            var first = true;
            foreach (HtmlElement htmlElement in htmlElements)
            {
                // Skip first, column descriptons
                if (first)
                {
                    first = false;
                }
                else
                {
                    if (htmlElement.Children.Count < 2)
                    {
                        continue;
                    }

                    if (htmlElement.InnerText.StartsWith("Föregående"))
                    {
                        break;
                    }

                    // Lägg till ny
                    EntryAdder.AddNewEntryFromStringArray(
                        GetSwedBankTableRow(htmlElement),
                        kontoEntries,
                        newKontoEntries,
                        newBatchOfKontoEntriesAlreadyRed);
                }
            }
        }

        private static BankRow GetSwedBankTableRow(HtmlElement htmlElement)
        {
            const int dateColNum = 1;
            const int eventColNum = 2;
            const int beloppColNum = 4;
            const int saldoColNum = 5;

            var entryStrings = new BankRow
            {
                DateValue = htmlElement.Children[dateColNum] != null
                                         ? "20" + htmlElement.Children[dateColNum].InnerText
                                         : string.Empty,
                EventValue = htmlElement.Children[eventColNum] != null
                                          ? htmlElement.Children[eventColNum].InnerText
                                          : string.Empty
            };

            var beloppVal = htmlElement.Children.Count > beloppColNum
                                ? (htmlElement.Children[beloppColNum] != null
                                       ? htmlElement.Children[beloppColNum].InnerText
                                       : string.Empty)
                                : (htmlElement.Children[3] != null ? htmlElement.Children[3].InnerText : string.Empty);
            entryStrings.BeloppValue = StringFuncions.RemoveSekFromMoneyString(beloppVal);

            entryStrings.SaldoValue = htmlElement.Children.Count > saldoColNum
                                      && htmlElement.Children[saldoColNum] != null
                                          ? htmlElement.Children[saldoColNum].InnerText
                                          : string.Empty;

            return entryStrings;
        }
        #endregion

        public static void CheckHtmlTr(HtmlElement subElement, SortedList kontoEntries, SortedList newKontoEntries)
        {
            CheckHtmlTr(subElement, kontoEntries, newKontoEntries, false);
        }

        private static BankRow GetHandelsbankenTableRowForLoneAndAllkort(HtmlElement htmlElement)
        {
            var entryStrings = new BankRow
            {
                DateValue = htmlElement.FirstChild.NextSibling.NextSibling.InnerText.Trim(),

                EventValue = htmlElement.FirstChild
                    .NextSibling.NextSibling.NextSibling.NextSibling.InnerText.Trim()
            };

            var beloppVal = htmlElement.FirstChild
                .NextSibling.NextSibling.NextSibling.NextSibling.NextSibling.NextSibling
                .InnerText.Trim();
            entryStrings.BeloppValue = StringFuncions.RemoveSekFromMoneyString(beloppVal);

            entryStrings.SaldoValue = string.Empty;

            return entryStrings;
        }

        private static BankRow GetHandelsbankenTableRowForKredit(HtmlElement htmlElement)
        {
            var entryStrings = new BankRow();

            var firstColumn = htmlElement.FirstChild.InnerText.Trim().ToLower();
            entryStrings.DateValue = firstColumn;

            entryStrings.EventValue = htmlElement.Children[4]
                //entryStrings.EventValue = htmlElement.FirstChild
                //.NextSibling.NextSibling.NextSibling.NextSibling
                .InnerText.Trim();

            var beloppVal = htmlElement.Children[10]
                .InnerText.Trim();
            entryStrings.BeloppValue = StringFuncions.RemoveSekFromMoneyString(beloppVal);

            entryStrings.SaldoValue = string.Empty;

            return entryStrings;
        }


        public static void CheckHtmlTr(HtmlElement subElement, SortedList kontoEntries, SortedList newKontoEntries, bool kreditEtc3Columns)
        {

            // subElement =
            // <tr>
            //    <td class="SHBHeader"><a href="JavaScript:openHelpWindow('','http://www.handelsbanken.se/shb/Inet/ICentSv.nsf/Default/q45529328136C7DCBC12576E2004322C0?opendocument&amp;frame=0','500','500')">Reskontradatum</a></td>
            //    <td width="5" class="SHBHeader">&nbsp;</td>
            //    <td class="SHBHeader"><a href="JavaScript:openHelpWindow('','http://www.handelsbanken.se/shb/Inet/ICentSv.nsf/Default/q0983A0EBE3994B4AC12576E200434D76?opendocument&amp;frame=0','500','500')">Transaktionsdatum</a></td>
            //    <td width="5" class="SHBHeader">&nbsp;</td>
            //    <td class="SHBHeader">Text</td>
            //    <td width="5" class="SHBHeader">&nbsp;</td>
            //    <td align="right" class="SHBHeader">Belopp</td>
            //    <td width="5" class="SHBHeader">&nbsp;</td>
            //    <td align="right" class="SHBHeader">Saldo</td>
            //</tr>
            //<tr>

            var newBatchOfKontoEntriesAlreadyRed = EntryAdder.GetNewBatchOfKontoEntriesAlreadyRed(kontoEntries, newKontoEntries);

            foreach (HtmlElement tr in subElement.Children)
            {
                var firstColumnForTitleCheck = tr.FirstChild.InnerText.Trim().ToLower();
                // Skippa header-raden
                if (firstColumnForTitleCheck == "Reskontradatum".ToLower()
                    || (kreditEtc3Columns
                        && firstColumnForTitleCheck == "köpdatum".ToLower())
                    )
                {
                    continue;
                }

                var row = kreditEtc3Columns ? GetHandelsbankenTableRowForKredit(tr)
                    : GetHandelsbankenTableRowForLoneAndAllkort(tr);

                EntryAdder.AddNewEntryFromStringArray(
                    row,
                    kontoEntries,
                    newKontoEntries,
                    newBatchOfKontoEntriesAlreadyRed);
            }



            //var noTRs = 0;

            //if (newKontoEntries == null || kontoEntries == null)
            //    return;

            ////Spara en batch, dyker det upp dubletter i samma, så ska de ses som unika
            //var newBatchOfKontoEntriesAlreadyRed = EntryAdder.GetNewBatchOfKontoEntriesAlreadyRed(kontoEntries, newKontoEntries);

            ////Hoppa över första TR
            //foreach (HtmlElement transacion in subElement.All)
            //{
            //    if (transacion.TagName.ToLower() != "tr")
            //    {
            //        continue;
            //    }

            //    noTRs++;
            //    if (noTRs <= 1)
            //    {
            //        continue;
            //    }

            //    var entryStrings = new string[4];
            //    var fieldNo = 1;
            //    var noTrans = 0;
            //    foreach (HtmlElement transEntry in transacion.All)
            //    {
            //        //hoppa över de 2 första
            //        noTrans++;
            //        if (noTrans <= 1 && !kreditEtc3Columns)
            //        {
            //            continue;
            //        }

            //        //TODO: ta in varannan eller få tag i namngivna tabbar, för att kunna läsa in tomma värden, så att varje värde får rätt typ
            //        if (transEntry.InnerHtml == null || transEntry.InnerHtml.StartsWith("&") || transEntry.InnerHtml.StartsWith("<"))
            //        {
            //            continue;
            //        }

            //        if (entryStrings.Length > fieldNo)
            //            entryStrings[fieldNo++] = transEntry.InnerText;
            //    }

            //    var bankRow = new BankRow
            //    {
            //        BeloppValue = entryStrings[3],
            //        DateValue = entryStrings[1],
            //        EventValue = entryStrings[2],
            //        SaldoValue = "0",
            //    };

            //    //Lägg till ny
            //    EntryAdder.AddNewEntryFromStringArray(bankRow, kontoEntries, newKontoEntries, newBatchOfKontoEntriesAlreadyRed);
            //}
        }

        internal static bool GetAllEntriesFromPdfFile(KontoEntriesHolder kontoEntriesHolder, List<BankRow> rows)
        {
            var newEntriesStart = kontoEntriesHolder.NewKontoEntries.Count;

            var newBatchOfKontoEntriesAlreadyRed =
                EntryAdder.GetNewBatchOfKontoEntriesAlreadyRed(
                    kontoEntriesHolder.KontoEntries, kontoEntriesHolder.NewKontoEntries);

            foreach (var row in rows)
            {
                EntryAdder.AddNewEntryFromStringArray(
                 row,
                 kontoEntriesHolder.KontoEntries,
                 kontoEntriesHolder.NewKontoEntries,
                 newBatchOfKontoEntriesAlreadyRed);
            }

            return kontoEntriesHolder.NewKontoEntries.Count > newEntriesStart;
        }
    }
}