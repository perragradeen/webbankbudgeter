using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Budgeter.Core.Entities;
using Budgetterarn.Operations;
using Microsoft.Office.Interop.Excel;
using RefLesses;
using Utilities;
using Application = Microsoft.Office.Interop.Excel.Application;

namespace Budgetterarn
{
    public class LoadNSave
    {
        #region Load&Save, TODO: ha dessa funktioner i egen fil

        // internal static void Save(params object[] args)
        // {
        // //Fuul kod
        // var refArg1 = args[1] as string;
        // var outArg1 = args[2] as Thread;
        // var refArg2 = (bool)args[6];
        // Save(
        // //args[0] as Thread, 
        // ref refArg1 
        // //out outArg1, args[3] as SortedList
        // , args[4] as string
        // , args[5] as string
        // , ref refArg2
        // , args[7] as string
        // , args[8] as string
        // , args[9] as Dictionary<string, string>);
        // }
        private const string LÖNEKONTOName = "LÖNEKONTO";
        private const string AllkortName = "Allkort";
        private const string EjFaktureratEtcName = "ejFaktureratEtc";
        private static readonly List<string> swedbankSaldonames = new List<string>
                                                                  {
                                                                      "Privatkonto 8417-8,4 751 687-7", 
                                                                      "Servicekonto 8417-8,4 778 356-8", 
                                                                      "Servicekonto 8417-8,914 636 458-4", 
                                                                      "e-sparkonto 8417-8,983 306 619-5"
                                                                  };

        internal static LoadOrSaveResult Save(
            KontoutdragInfoForSave kontoutdragInfoForSave,
            // Thread mainThread, 
            // ref string statusLabel,
            // , out Thread workerThread,
            SortedList kontoEntries,
            // , string excelFileSavePath
            // , string sheetName
            // , ref bool somethingChanged
            // , string excelFileSavePathWithoutFileName, string excelFileSaveFileName
            Dictionary<string, string> saldon)
        {
            #region Thread handling

            // if (Thread.CurrentThread == mainThread) {
            // toolStripStatusLabel1.Text = "Saving... number of entries; " + kontoEntries.Count;

            // workerThread = new Thread(new ThreadStart(Save));
            // workerThread.CurrentCulture = mainThread.CurrentCulture;
            // workerThread.CurrentUICulture = mainThread.CurrentUICulture;
            // workerThread.Start();
            // return;
            // }
            #endregion

            try
            {
                // If nothing to save, return
                if (kontoEntries == null || kontoEntries.Count == 0)
                {
                    return new LoadOrSaveResult();
                }

                var logArray = GetTopRowWithHeaders(saldon);
                var logThis = GetWhatToLogWithHeaders(ProgramSettings.BankType, logArray, kontoEntries);

                ReIndexKontoentriesToLatestOnTop(kontoEntries, logThis);

                // Gör någon backup el. likn. för att inte förlora data. Backupa dynamiskt. Så att om man skickar in en fil så backas den upp istället för huvudfilen...men de e rätt ok att backa huvudfilen
                BackupOrginialFile(
                    "Before.Save",
                    kontoutdragInfoForSave.excelFileSavePath,
                    kontoutdragInfoForSave.excelFileSavePathWithoutFileName,
                    kontoutdragInfoForSave.excelFileSaveFileName);

                // spara över gammalt, innan skrevs det på sist
                Logger.WriteToWorkBook(
                    kontoutdragInfoForSave.excelFileSavePath, kontoutdragInfoForSave.sheetName, true, logThis);

                return new LoadOrSaveResult { skippedOrSaved = logThis.Count - 1, somethingLoadedOrSaved = false };
            }
            catch (Exception savExcp)
            {
                MessageBox.Show("Error: " + savExcp.Message);
                return new LoadOrSaveResult();
            }
        }

        private static Hashtable GetWhatToLogWithHeaders(BankType bankType, object[] logArray, SortedList kontoEntries)
        {
            // Gör om till Arraylist för ordning, det blir i omvänd ordning, alltså först överst. Ex 2009-04-01 sen 2009-04-02 osv.
            Hashtable logThis = null;

            // Lägg till överskrifter
            // y	m	d	n	t	g	s	b				c
            if (ProgramSettings.BankType.Equals(BankType.Swedbank)
                || ProgramSettings.BankType.Equals(BankType.Mobilhandelsbanken))
            {
                logThis = new Hashtable { { kontoEntries.Count + 1, logArray } };
            }
            else
            {
                throw new Exception("Bank type not allowed: " + ProgramSettings.BankType.ToString());
            }

            return logThis;
        }

        private static void ReIndexKontoentriesToLatestOnTop(SortedList kontoEntries, Hashtable logThis)
        {
            var indexKey = kontoEntries.Count;
            foreach (DictionaryEntry currentRow in kontoEntries)
            {
                // string key = currentRow.Key as string;
                var currentKeEntry = currentRow.Value as KontoEntry;
                if (currentKeEntry != null)
                {
                    logThis.Add(indexKey--, currentKeEntry.RowToSaveForThis); // Använd int som nyckel
                }
            }
        }

        private static object[] GetTopRowWithHeaders(Dictionary<string, string> saldon)
        {
            // saldon
            var saldoColumnNumber = 11 + 1;
            var columnNames = new object[] { "y", "m", "d", "n", "t", "g", "s", "b", "", "", "", "c" };

            var logArray = new object[columnNames.Length + saldon.Count];

            var index = 0;
            foreach (var s in columnNames)
            {
                logArray[index++] = s;
            }

            foreach (var s in saldon.Values)
            {
                logArray[saldoColumnNumber++] = s;
            }

            return logArray;
        }

        /// <summary>
        /// Sparar till Excel-fil
        /// </summary>
        public static LoadOrSaveResult GetAllEntriesFromExcelFile(
            KontoutdragInfoForLoad kontoutdragInfoForLoad,
            SortedList saveToTable,
            Dictionary<string, string> saldon,
            Hashtable entriesLoadedFromDataStore)
        {
            // Töm alla tidigare entries i minnet om det ska laddas helt ny fil el. likn. 
            if (kontoutdragInfoForLoad.clearContentBeforeReadingNewFile)
            {
                saveToTable.Clear();
            }

            // Görs i Ui-handling, UpdateEntriesToSaveMemList();
            // Skapa kontoentries
            // För att se om det laddats något, så UI-uppdateras etc. Så returneras bool om det...
            return SkapaKontoEntries(saveToTable, entriesLoadedFromDataStore, saldon);
        }

        public static Hashtable LoadEntriesFromFile(
            KontoutdragInfoForLoad kontoutdragInfoForLoad, ref Thread workerThread)
        {
            // Backa inte upp filen innan laddning, eftersom filen inte ändras vid laddning...
            // BackupOrginialFile("Before.Load");

            // Öppna fil först, och ladda, sen ev. spara ändringar, som inte ändrats av laddningen, av filöpnningen
            var kontoUtdragXls = new Hashtable();

            // Todo: Gör om till arraylist, eller lista av dictionary items, för att kunna välja ordning
            #region Öppna fil och hämta rader

            try
            {
                var filePath = kontoutdragInfoForLoad.filePath;
                if (filePath == "")
                {
                    kontoutdragInfoForLoad.excelFileSavePath =
                        filePath = FileOperations.OpenFileOfType("Open file", FileType.xls, ""); // Öppnar dialog
                }

                if (string.IsNullOrEmpty(filePath))
                {
                    workerThread = null;
                    return null;
                }

                if (!System.IO.File.Exists(filePath))
                {
                    MessageBox.Show("File: " + filePath + " does not exist.", "File error");
                    workerThread = null;
                    return null;
                }

                OpenFileFunctions.OpenExcelSheet(filePath, kontoutdragInfoForLoad.sheetName, kontoUtdragXls, 0);
            }
            catch (Exception fileOpneExcp)
            {
                Console.WriteLine("User cancled or other error: " + fileOpneExcp.Message);

                if (kontoUtdragXls.Count < 1)
                {
                    workerThread = null;
                    return null;
                }
            }

            #endregion

            workerThread = null;

            return (Hashtable)kontoUtdragXls[kontoutdragInfoForLoad.sheetName];
        }

        private static LoadOrSaveResult SkapaKontoEntries(
            SortedList saveToTable, Hashtable entriesLoadedFromDataStore, Dictionary<string, string> saldon)
        {
            var loadResult = new LoadOrSaveResult();

            // var skipped = 0;
            // var somethingLoaded = false;
            foreach (DictionaryEntry item in entriesLoadedFromDataStore)
            {
                if (item.Value != null)
                {
                    var entryArray = ((ExcelRowEntry)item.Value).args;

                    // Om det är tomt
                    if (entryArray == null)
                    {
                        continue;
                    }

                    // Om det är kolumnbeskrivning, skippa...
                    if (entryArray[0] == "y")
                    {
                        // Spara saldon, använd det gamla värdet om inget nytt hittats från fil.
                        var saldoLöne = saldon.SafeGetStringFromDictionary(LÖNEKONTOName);
                        var saldoAllkort = saldon.SafeGetStringFromDictionary(AllkortName);
                        var saldoAllkortKreditEjFakturerat = saldon.SafeGetStringFromDictionary(EjFaktureratEtcName);

                        saldoLöne = entryArray.Length > 12 ? entryArray[12] ?? saldoLöne : saldoLöne;
                        saldoAllkort = entryArray.Length > 13 ? entryArray[13] ?? saldoAllkort : saldoAllkort;
                        saldoAllkortKreditEjFakturerat = entryArray.Length > 14
                                                             ? entryArray[14] ?? saldoAllkortKreditEjFakturerat
                                                             : saldoAllkortKreditEjFakturerat;

                        // var saldoAllkortKreditFakturerat = entryArray.Length > 15 ? entryArray[15] ?? saldoAllkortKreditFakturerat : saldoAllkortKreditFakturerat;
                        var saldoColumnNumber = 11;
                        if (ProgramSettings.BankType == BankType.Swedbank)
                        {
                            foreach (var s in swedbankSaldonames)
                            {
                                var saldot = entryArray.Length > saldoColumnNumber
                                                 ? entryArray[saldoColumnNumber + 1] ?? string.Empty
                                                 : string.Empty; // Todo, byt empty mot värden i saldon

                                if (!saldon.ContainsKey(s))
                                {
                                    saldon.Add(s, saldot);
                                }
                                else
                                {
                                    saldon[s] = saldot;
                                }

                                saldoColumnNumber++;
                            }
                        }
                        else if (ProgramSettings.BankType == BankType.Mobilhandelsbanken)
                        {
                            saldon.AddToOrChangeValueInDictionaryForKey(LÖNEKONTOName, saldoLöne.GetValueFromEntry());
                            saldon.AddToOrChangeValueInDictionaryForKey(AllkortName, saldoAllkort.GetValueFromEntry());
                            saldon.AddToOrChangeValueInDictionaryForKey(
                                EjFaktureratEtcName, saldoAllkortKreditEjFakturerat.GetValueFromEntry()

                                // + saldoAllkortKreditFakturerat.GetValueFromEntry()
                                );
                        }

                        // Hoppa över
                        continue;
                    }

                    var newKe = new KontoEntry(entryArray, true);
                    var key = newKe.KeyForThis; // item.Key as string;

                    // Lägg till orginalraden, gör i UI-hanterare
                    if (!saveToTable.ContainsKey(key))
                    {
                        #region old debug

                        // AddToRichTextBox(richTextBox1, newKE.RowToSaveForThis);

                        // test debug
                        // if (_newKontoEntries.Count < 6)
                        // {
                        // if (!_newKontoEntries.ContainsKey(key))
                        // {
                        // _newKontoEntries.Add(key, newKE);
                        // //AddToListview(m_newIitemsListOrg, newKE);
                        // }
                        // }
                        // else 
                        #endregion

                        saveToTable.Add(key, newKe); // CreateKE(entryArray, true)

                        loadResult.somethingLoadedOrSaved = true;
                    }
                    else
                    {
                        // Detta ordnar sig, så länge saldot är med i nyckeln, det är den, så det gäller bara att ha rätt saldo i xls //Om man tagit utt t.ex. 100kr 2 ggr samma dag, från samma bankomat. hm, sätt 1 etta efteråt, men det göller ju bara det som är såna, hm, får ta dem manuellt

                        // skulle kunna tillåta någon inläsning här ev. 
                        // om man kan förutsätta att xls:en är kollad, 
                        // det får bli här man lägger till specialdubbletter manuellt
                        Console.WriteLine("Entry Double found. Key = " + key);

                        // meddela detta till usern, man ser de på skipped...
                        loadResult.skippedOrSaved++;
                    }
                }
            }

            return loadResult;
        }

        internal static bool GetAllVisibleEntriesFromWebBrowser(
            SortedList kontoEntries,
            WebBrowser webBrowser1,
            SortedList newKontoEntries,
            ref bool somethingChanged,
            Dictionary<string, string> saldon)
        {
            if (webBrowser1 == null || webBrowser1.Document == null)
            {
                return false;
            }

            var noKe = kontoEntries.Count; // Se om något ändras sen...
            var noNewKontoEntriesBeforeLoading = newKontoEntries.Count;

            // Kolla browser efter entries.
            if (webBrowser1.Document.Window != null)
            {
                switch (ProgramSettings.BankType)
                {
                    case BankType.Handelsbanken:

                        #region Handelsbanken

                        // var nextIsAllkreditFaktureratEtc = false;

                        // Kolla även huvuddocet
                        // CheckDocForEntries(webBrowser1.Document.Window.Document, kontoEntries, ref saldoAllkortKreditEjFakturerat, ref saldoAllkortKreditFakturerat, newKontoEntries, ref saldoLöne, ref saldoAllkort, ref somethingChanged, saldon);

                        // if (webBrowser1.Document.Window.Frames != null) {
                        // foreach (HtmlWindow currentWindow in webBrowser1.Document.Window.Frames)
                        // {
                        // //break;//Debug
                        // var doc = currentWindow.Document;
                        // CheckDocForEntries(doc, kontoEntries, ref saldoAllkortKreditEjFakturerat, ref saldoAllkortKreditFakturerat, newKontoEntries, ref saldoLöne, ref saldoAllkort, ref somethingChanged, saldon);
                        // }

                        // }
                        #endregion

                        break;
                    case BankType.Swedbank:

                        #region Swedbank

                        if (webBrowser1.Document.Body != null)
                        {
                            // Get saldo
                            GetSwedbankSaldo(webBrowser1.Document.Body, saldon);

                            var saldoTable =
                                webBrowser1.Document.Body.FirstChild.NextSibling.NextSibling.FirstChild.NextSibling
                                           .FirstChild.FirstChild.FirstChild.FirstChild.FirstChild.FirstChild.FirstChild
                                           .FirstChild.NextSibling.NextSibling.FirstChild.NextSibling.FirstChild
                                           .NextSibling.NextSibling;

                            // ReSharper disable ConditionIsAlwaysTrueOrFalse
                            if (saldoTable != null

                                // webBrowser1.Document.Body.FirstChild.NextSibling.NextSibling.FirstChild.NextSibling.
                                // FirstChild.FirstChild.FirstChild.FirstChild.FirstChild.FirstChild.FirstChild.FirstChild.NextSibling.
                                // NextSibling.FirstChild.NextSibling.FirstChild.NextSibling.NextSibling != null
                                )
                            {
                                // ReSharper restore ConditionIsAlwaysTrueOrFalse

                                // Get Entries
                                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                                if (saldoTable.NextSibling == null)
                                {
                                    // ReSharper restore ConditionIsAlwaysTrueOrFalse
                                    GetHtmlEntriesFromSwedBank(
                                        saldoTable.FirstChild.FirstChild.NextSibling.Children,
                                        kontoEntries,
                                        newKontoEntries);
                                }

                                    // ReSharper disable ConditionIsAlwaysTrueOrFalse
                                else if (saldoTable.NextSibling != null)
                                {
                                    // ReSharper restore ConditionIsAlwaysTrueOrFalse
                                    GetHtmlEntriesFromSwedBank(
                                        saldoTable.NextSibling.FirstChild.FirstChild.NextSibling.Children,
                                        kontoEntries,
                                        newKontoEntries);
                                }
                            }
                            else if (
                                webBrowser1.Document.Body.FirstChild.NextSibling.NextSibling.FirstChild.NextSibling
                                           .FirstChild.FirstChild.FirstChild.FirstChild.FirstChild.FirstChild
                                           .FirstChild.FirstChild.NextSibling.NextSibling.FirstChild.NextSibling
                                           .FirstChild.NextSibling.FirstChild.FirstChild.NextSibling != null)
                            {
                                // Get Entries
                                GetHtmlEntriesFromSwedBank(
                                    webBrowser1.Document.Body.FirstChild.NextSibling.NextSibling.FirstChild
                                               .NextSibling.FirstChild.FirstChild.FirstChild.FirstChild.FirstChild
                                               .FirstChild.FirstChild.FirstChild.NextSibling.NextSibling.FirstChild
                                               .NextSibling.FirstChild.NextSibling.FirstChild.FirstChild.NextSibling
                                               .FirstChild.FirstChild.NextSibling.Children,
                                    kontoEntries,
                                    newKontoEntries);
                            }
                        }

                        #endregion

                        break;
                    case BankType.Mobilhandelsbanken:
                        var htmlBody = webBrowser1.Document.Body;
                        if (htmlBody != null)
                        {
                            GetAllEntriesFromMobileHandelsBanken(htmlBody, kontoEntries, newKontoEntries, saldon);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (kontoEntries.Count != noKe)
            {
                somethingChanged = true; // Här har man tagit in nytt som inte är sparat
            }

            // Returnera aom något ändrats. Är de nya inte samma som innan laddning, så är det sant att något ändrats.
            return newKontoEntries.Count != noNewKontoEntriesBeforeLoading;
        }

        private static void GetAllEntriesFromMobileHandelsBanken(
            HtmlElement htmlBody, SortedList kontoEntries, SortedList newKontoEntries, Dictionary<string, string> saldon)
        {
            var baseElement = htmlBody.FirstChild.FirstChild.FirstChild.FirstChild.NextSibling.NextSibling.FirstChild;

            var saldoElement = baseElement;

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

            GetMobileHandelsBankenSaldo(saldoElement, saldon);

            var kontoEntriesElement = baseElement.NextSibling;
            if (kontoEntriesElement.TagName.Equals("UL")) // .GetAttribute("link-list") != null)
            {
            }
            else
            {
                kontoEntriesElement = kontoEntriesElement.NextSibling;
            }

            GetHtmlEntriesFromMobileHandelsbanken(kontoEntriesElement, kontoEntries, newKontoEntries);
        }

        /// <summary>
        /// Körs flera gånger en per sida och får då ut flera olika konton och uppdaterar dess värde i saldo-tabellen.
        /// </summary>
        /// <param name="saldoElement"></param>
        /// <param name="saldon"></param>
        private static void GetMobileHandelsBankenSaldo(HtmlElement saldoElement, Dictionary<string, string> saldon)
        {
            var saldoName = saldoElement.FirstChild.FirstChild.InnerText;
            var saldoValueElem = saldoElement.FirstChild.NextSibling.NextSibling;

            var saldoValue = 0.0;

            // var allkortHas = false;
            if (saldon.ContainsKey(AllkortName) || saldon.ContainsKey("Allkortskonto"))
            {
                if (saldoName.Contains(AllkortName))
                {
                    // allkortHas = true;
                    saldoName = AllkortName;
                }
            }

            if (saldoElement != null)
            {
                saldoValue = RemoveSekFromMoneyString(saldoValueElem.InnerText).GetValueFromEntry();
                saldon.AddToOrChangeValueInDictionaryForKey(saldoName, saldoValue);
            }

            // Kolla disp. belopp
            var saldoNameDispBelopp = EjFaktureratEtcName;
            saldoValueElem = saldoElement.FirstChild.NextSibling.NextSibling.NextSibling.FirstChild.NextSibling;

            var saldoValueDisp = 0.0;
            if (saldoElement != null && saldoName != LÖNEKONTOName)
            {
                saldoValueDisp = RemoveSekFromMoneyString(saldoValueElem.InnerText).GetValueFromEntry();

                // Räkna ut mellanskillnaden som motsvarar fakturerat och ej förfallet etc
                const int KreditBelopp = 10000;

                saldoValueDisp = saldoValue + KreditBelopp - saldoValueDisp;

                saldon.AddToOrChangeValueInDictionaryForKey(saldoNameDispBelopp, -saldoValueDisp);
            }
        }

        private static void GetHtmlEntriesFromMobileHandelsbanken(
            HtmlElement kontoEntriesElement, SortedList kontoEntries, SortedList newKontoEntries)
        {
            var newBatchOfKontoEntriesAlreadyRed = GetNewBatchOfKontoEntriesAlreadyRed(kontoEntries, newKontoEntries);

            foreach (HtmlElement htmlElement in kontoEntriesElement.GetElementsByTagName("LI"))
            {
                AddNewEntryFromStringArray(
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
            entryStrings.BeloppValue = RemoveSekFromMoneyString(beloppVal);
            entryStrings.SaldoValue = string.Empty;

            return entryStrings;
        }

        private static string RemoveSekFromMoneyString(string beloppVal)
        {
            return beloppVal.Replace("SEK", string.Empty).Trim().Replace(" ", string.Empty);
        }

        // private static void CheckDocForEntries(HtmlDocument doc, SortedList kontoEntries, ref string saldoAllkortKreditEjFakturerat, ref string saldoAllkortKreditFakturerat, SortedList newKontoEntries, ref string saldoLöne, ref string saldoAllkort, ref bool somethingChanged, Dictionary<string, string> saldon)
        // {
        // //Leta upp: "För period fr o m:t o m:"
        // const string toFind = "Reskontradatum Transaktionsdatum Text Belopp Saldo";//"För period fr o m:t o m:"; // : "De senaste transaktionerna";
        // if (doc == null || doc.Body == null) { }
        // else
        // {
        // foreach (HtmlElement currentElement in doc.Body.Children)
        // {
        // #region Gå igenom alla element för denna ram
        // if (currentElement.OuterText == null)
        // continue;

        // var allkortKredit = (currentElement.OuterText != null &&
        // currentElement.OuterText.Trim().Contains("Konto: 629 011 192"));

        // #region Old
        // //if (allkortKredit) {
        // //    if (currentElement.OuterText.Trim().StartsWith("Kontonummer:629 010")) {
        // //        allkortKredit = false;
        // //        löneKonto = true;
        // //    }
        // //} 
        // #endregion

        // //Om man är i lönekontot, den har lite annan struktur
        // var löneKonto = (currentElement.OuterText != null &&
        // currentElement.OuterText.Trim().Contains("Konto: 629 010 552"));

        // var nuKreditKonton = (currentElement.OuterText != null
        // &&
        // (
        // currentElement.OuterText.Trim().Contains("Urval:Ej fakturerat") ||
        // currentElement.OuterText.Trim().Contains("Urval:Fakturerat, ej förfallet")
        // )
        // );//Urval:Ej fakturerat

        // var löneKontoEndTextIdentifier = "Clearingnummer";
        // //Kolla saldo löne
        // if (currentElement.OuterText != null &&
        // currentElement.OuterText.Trim().Contains("Saldo:")
        // && currentElement.OuterText.Trim().Contains(löneKontoEndTextIdentifier)
        // )
        // {
        // var elemText = currentElement.OuterText.Trim();
        // var saldo =
        // //elemText.Substring(elemText.IndexOf("Saldo:") + 6, elemText.IndexOf("Information och villkor om kontot")).Trim();

        // elemText.Substring(elemText.IndexOf("Saldo:") + 6,
        // elemText.IndexOf(löneKontoEndTextIdentifier) -
        // (elemText.IndexOf("Saldo:") + 6)).Trim().Replace(
        // " ", string.Empty);
        // //Saldo:44 476,09 Information och villkor om kontot
        // if (löneKonto)
        // {
        // saldoLöne = saldo;
        // }
        // }

        // //Ej fakturerat:-713,81
        // //Fakturerat, ej förfallet:-3 585,77
        // //Disponibelt belopp:36 535,53 Totalt utbetald bonus 160 kr

        // var startText = "Ej fakturerat:";
        // var endText = "Fakturerat, ej förfallet";
        // var extraText = "Kontovillkor och IBAN";
        // //Kolla saldo
        // if (currentElement.OuterText != null &&
        // currentElement.OuterText.Trim().Contains(startText)
        // && currentElement.OuterText.Trim().Contains(endText)
        // )
        // {
        // var elemText = currentElement.OuterText.Trim();
        // var startIndex = elemText.IndexOf(startText) + startText.Length;
        // var endIndex = elemText.IndexOf(endText);
        // //
        // var saldo =
        // //elemText.Substring(elemText.IndexOf("Saldo:") + 6, elemText.IndexOf("Information och villkor om kontot")).Trim();

        // elemText.Substring(startIndex, endIndex - startIndex).Trim().Replace(extraText, string.Empty).Replace(" ", string.Empty);
        // //Saldo:44 476,09 Information och villkor om kontot
        // saldoAllkortKreditEjFakturerat = saldo;
        // }

        // startText = "Fakturerat, ej förfallet:";
        // endText = "Disponibelt belopp:";
        // //Kolla saldo
        // if (currentElement.OuterText != null &&
        // currentElement.OuterText.Trim().Contains(startText)
        // && currentElement.OuterText.Trim().Contains(endText)
        // )
        // {
        // var elemText = currentElement.OuterText.Trim();
        // var startIndex = elemText.IndexOf(startText) + startText.Length;
        // var endIndex = elemText.IndexOf(endText);
        // //
        // var saldo =
        // //elemText.Substring(elemText.IndexOf("Saldo:") + 6, elemText.IndexOf("Information och villkor om kontot")).Trim();

        // elemText.Substring(startIndex, endIndex - startIndex).Trim().Replace(" ", string.Empty);
        // //Saldo:44 476,09 Information och villkor om kontot
        // saldoAllkortKreditFakturerat = saldo;
        // }

        // //Saldo på kontot:30 835,11 Information och villkor om kontot
        // //Ej fakturerat:-713,81   

        // //Hämta allkort
        // startText = "Saldo på kontot:";
        // endText = "Clearingnummer";//"Information och villkor om kontot";
        // //Kolla saldo
        // if (currentElement.OuterText != null &&
        // currentElement.OuterText.Trim().Contains(startText)
        // && currentElement.OuterText.Trim().Contains(endText)
        // )
        // {
        // var elemText = currentElement.OuterText.Trim();
        // var startIndex = elemText.IndexOf(startText) + startText.Length;
        // var endIndex = elemText.IndexOf(endText);
        // //
        // var saldo =
        // //elemText.Substring(elemText.IndexOf("Saldo:") + 6, elemText.IndexOf("Information och villkor om kontot")).Trim();

        // elemText.Substring(startIndex, endIndex - startIndex).Trim().Replace(" ", string.Empty);
        // //Saldo:44 476,09 Information och villkor om kontot
        // saldoAllkort = saldo;
        // }

        // #region Old
        // //Kolla saldo
        // //if (currentElement.OuterText != null &&
        // //    currentElement.OuterText.Trim().Contains("Saldo")
        // //    && currentElement.OuterText.Trim().Contains("Beviljad kredit")
        // //    ) {
        // //    saldoAllkort =
        // //        currentElement.OuterText.Trim().Substring(
        // //            currentElement.OuterText.Trim().LastIndexOf(":") +
        // //            ":".Length);

        // //    if (!löneKonto) {
        // //        nextIsAllkreditFaktureratEtc = true;
        // //    }
        // //} 

        // #endregion
        // var noTables = 0;

        // if (nuKreditKonton)
        // {
        // foreach (HtmlElement subElement in currentElement.All)
        // {
        // #region Leta upp den andra tabellen

        // if (subElement.TagName.ToLower() != "table")
        // {
        // continue;
        // }

        // noTables++;
        // if (noTables == 4) //Hoppa över 1:a tabellen
        // {
        // CheckHtmlTr(subElement, kontoEntries, newKontoEntries, true);
        // }

        // #endregion
        // }
        // }

        // if (((currentElement.OuterText == null ||
        // (!currentElement.OuterText.Trim().StartsWith(toFind) &&
        // !currentElement.OuterText.Trim().Contains(toFind))) && !allkortKredit))
        // {
        // continue;
        // }

        // //Leta upp den andra tabellen
        // noTables = 0;
        // foreach (HtmlElement subElement in currentElement.All)
        // {
        // #region Leta upp den andra tabellen

        // if (subElement.TagName.ToLower() != "table")
        // {
        // continue;
        // }

        // #region Old
        // //if (allkortKredit) {
        // //    noTables++;
        // //    if (noTables > 1) //Hoppa över 1:a tabellen
        // //    {
        // //        CheckHtmlTr(subElement, kontoEntries, newKontoEntries);
        // //    }
        // //} else {

        // #endregion
        // noTables++; //va fel här innan...
        // #region Old
        // if (noTables == 13 && !löneKonto && allkortKredit)
        // {
        // CheckHtmlTr(subElement, kontoEntries, newKontoEntries);
        // }
        // else
        // #endregion
        // if (noTables == 12)
        // {
        // CheckHtmlTr(subElement, kontoEntries, newKontoEntries);
        // }
        // //}

        // #endregion
        // }

        // #endregion
        // }
        // }
        // }
        private static void GetSwedbankSaldo(HtmlElement htmlElement, Dictionary<string, string> saldon)
        {
            // foreach (HtmlElement currentElem in htmlElement.Children) {
            // if (currentElem.InnerText != null && currentElem.InnerText.Contains("Saldo")) {
            var nextIsSaldo = false;

            // var nextIsLöne = false;
            var saldoNameNnumber = "";

            foreach (HtmlElement currentSubElem in htmlElement.All)
            {
                if (nextIsSaldo)
                {
                    if (saldoNameNnumber != string.Empty)
                    {
                        if (!saldon.ContainsKey(saldoNameNnumber))
                        {
                            saldon.Add(saldoNameNnumber, currentSubElem.InnerText);
                        }
                        else
                        {
                            saldon[saldoNameNnumber] = currentSubElem.InnerText;
                        }

                        break;
                    }
                }

                if (currentSubElem.InnerText != null)
                {
                    if (!currentSubElem.TagName.Equals("OPTION") && // h3
                        currentSubElem.TagName.Equals("H3"))
                    {
                        foreach (var s in swedbankSaldonames)
                        {
                            if (currentSubElem.InnerText.Contains(s))
                            {
                                saldoNameNnumber = s;
                            }
                        }

                        // if (currentSubElem.InnerText.Contains("8417-8,4 751 687-7"))
                        // nextIsLöne = true;
                        // else if (currentSubElem.InnerText.Contains("Privatkonto 8417-8,4 751 687-7"))
                        // saldoNameNnumber = "Privatkonto 8417-8,4 751 687-7";
                    }

                    if (currentSubElem.InnerText.Equals("Saldo"))
                    {
                        nextIsSaldo = true;
                    }
                }

                // foreach (HtmlElement currentSubSubElem in currentSubElem.Children) {
                // foreach (HtmlElement currentSubSubSubElem in currentSubSubElem.Children) {

                // }

                // }
            }

            // }
            // }
        }

        private static void GetHtmlEntriesFromSwedBank(
            HtmlElementCollection htmlElements, SortedList kontoEntries, SortedList newKontoEntries)
        {
            // Spara en batch, dyker det upp dubletter i samma, så ska de ses som unika
            var newBatchOfKontoEntriesAlreadyRed = GetNewBatchOfKontoEntriesAlreadyRed(kontoEntries, newKontoEntries);

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
                    AddNewEntryFromStringArray(
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

            var entryStrings = new BankRow();
            entryStrings.DateValue = htmlElement.Children[dateColNum] != null
                                         ? "20" + htmlElement.Children[dateColNum].InnerText
                                         : string.Empty;
            entryStrings.EventValue = htmlElement.Children[eventColNum] != null
                                          ? htmlElement.Children[eventColNum].InnerText
                                          : string.Empty;

            var beloppVal = htmlElement.Children.Count > beloppColNum
                                ? (htmlElement.Children[beloppColNum] != null
                                       ? htmlElement.Children[beloppColNum].InnerText
                                       : string.Empty)
                                : (htmlElement.Children[3] != null ? htmlElement.Children[3].InnerText : string.Empty);
            entryStrings.BeloppValue = RemoveSekFromMoneyString(beloppVal);

            entryStrings.SaldoValue = htmlElement.Children.Count > saldoColNum
                                      && htmlElement.Children[saldoColNum] != null
                                          ? htmlElement.Children[saldoColNum].InnerText
                                          : string.Empty;

            return entryStrings;
        }

        // private static void CheckSwedBankHtml(HtmlElement element, SortedList kontoEntries, SortedList newKontoEntries) {

        // }

        // private static void CheckHtmlTr(HtmlElement subElement, SortedList kontoEntries, SortedList newKontoEntries) {
        // CheckHtmlTr(subElement, kontoEntries, newKontoEntries, false);
        // }

        // private static void CheckHtmlTr(HtmlElement subElement, SortedList kontoEntries, SortedList newKontoEntries, bool kreditEtc3Columns) {
        // var noTRs = 0;

        // if (newKontoEntries == null || kontoEntries == null)
        // return;

        // //Spara en batch, dyker det upp dubletter i samma, så ska de ses som unika
        // var newBatchOfKontoEntriesAlreadyRed = GetNewBatchOfKontoEntriesAlreadyRed(kontoEntries, newKontoEntries);

        // //Hoppa över första TR
        // foreach (HtmlElement transacion in subElement.All) {
        // if (transacion.TagName.ToLower() != "tr") {
        // continue;
        // }

        // noTRs++;
        // if (noTRs <= 1) {
        // continue;
        // }

        // var entryStrings = new string[4];
        // var fieldNo = 0;
        // var noTrans = 0;
        // foreach (HtmlElement transEntry in transacion.All) {
        // //hoppa över de 2 första
        // noTrans++;
        // if (noTrans <= 2 && !kreditEtc3Columns) {
        // continue;
        // }

        // //TODO: ta in varannan eller få tag i namngivna tabbar, för att kunna läsa in tomma värden, så att varje värde får rätt typ
        // if (transEntry.InnerHtml == null || transEntry.InnerHtml.StartsWith("&") || transEntry.InnerHtml.StartsWith("<")) {
        // continue;
        // }

        // if (entryStrings.Length > fieldNo)
        // entryStrings[fieldNo++] = transEntry.InnerText;
        // }

        // //Lägg till ny
        // AddNewEntryFromStringArray(entryStrings, kontoEntries, newKontoEntries, newBatchOfKontoEntriesAlreadyRed);
        // }
        // }
        private static SortedList GetNewBatchOfKontoEntriesAlreadyRed(
            SortedList kontoEntries, SortedList newKontoEntries)
        {
            var newBatchOfKontoEntriesAlreadyRed = new SortedList();
            foreach (DictionaryEntry entry in newKontoEntries)
            {
                if (!newBatchOfKontoEntriesAlreadyRed.ContainsKey(entry.Key))
                {
                    newBatchOfKontoEntriesAlreadyRed.Add(entry.Key, entry.Value);
                }
            }

            foreach (DictionaryEntry entry in kontoEntries)
            {
                if (!newBatchOfKontoEntriesAlreadyRed.ContainsKey(entry.Key))
                {
                    newBatchOfKontoEntriesAlreadyRed.Add(entry.Key, entry.Value);
                }
            }

            return newBatchOfKontoEntriesAlreadyRed;
        }

        private static void AddNewEntryFromStringArray(
            BankRow entryStrings,
            SortedList kontoEntries,
            SortedList newKontoEntries,
            SortedList newBatchOfKontoEntriesAlreadyRed)
        {
            var newKeyFromHtml = new KontoEntry(entryStrings);
            var key = newKeyFromHtml.KeyForThis;

            if (!kontoEntries.ContainsKey(key) && !newKontoEntries.ContainsKey(key)) // Kollas även senare
            {
                // if (newKontoEntries != null) {// && !newKontoEntries.ContainsKey(key)) {
                if (key != null)
                {
                    newKontoEntries.Add(key, newKeyFromHtml);

                    // }
                    // else {
                    // //Dubblett
                    // }
                }

                // Handle Doubles
            }
            else if (!newBatchOfKontoEntriesAlreadyRed.ContainsKey(key))
            {
                // Om man hade entryn i Excel, innan laddning, och innan man gick igenom nya, så kan man (förutsätter att man då det inte finns saldo (i allkort-kredit), så läses hela listan in i ett svep, det är inte en lista, det kan ev. bli dubblet om två datum hamnar på olika allkort-kredit-fakturor)
                var userDecision = MessageBox.Show(
                    "Found potential double: " + newKeyFromHtml.KeyForThis,
                    "Double, SaveThisEntry?",
                    MessageBoxButtons.YesNo);

                if (userDecision.Equals(DialogResult.Yes))
                {
                    // Detta är en dubblett, men om det finns fler än 2 dubbletter så måste man se till att nyckeln är unik
                    while (newKontoEntries.ContainsKey(newKeyFromHtml.KeyForThis))
                    {
                        // Stega upp saldo, tills en unik nyckel skapats
                        newKeyFromHtml.SaldoOrginal += newKeyFromHtml.KostnadEllerInkomst != 0
                                                           ? newKeyFromHtml.KostnadEllerInkomst
                                                           : 1;
                    }

                    newKontoEntries.Add(newKeyFromHtml.KeyForThis, newKeyFromHtml);
                }

                // För annat än Allkortskredit, så ordnar Detta sig, så länge saldot är med i nyckeln, det är den, så det gäller bara att ha rätt saldo i xls //Om man tagit utt t.ex. 100kr 2 ggr samma dag, från samma bankomat. hm, sätt 1 etta efteråt, men det göller ju bara det som är såna, hm, får ta dem manuellt
            }
        }

        private static void BackupOrginialFile(
            string typeOfBackup,
            string excelFileSavePath,
            string excelFileSavePathWithoutFileName,
            string excelFileSaveFileName)
        {
            BackupOrginialFile(
                excelFileSavePath, excelFileSavePathWithoutFileName, typeOfBackup + "." + excelFileSaveFileName);
        }

        private static void BackupOrginialFile(
            string orgfilePath, string newFilePathWithoutFileName, string newFileName)
        {
            // TODO: check that dir exists and path etc
            System.IO.File.Copy(
                orgfilePath,
                newFilePathWithoutFileName + @"bak\" + newFileName + "."
                + DateTime.Now.ToString(new System.Globalization.CultureInfo("sv-SE")).Replace(":", ".") + ".bak.xls",
                true);
        }

        #endregion

        // Extrafunktioner

        // Contains Excelapp to use for opening file after save, in Excel mode. And auto close it when close...
        private static Application excelAppOpen;


        public static void LoadExcelFileInExcel(string excelFileSavePath)
        {
            // SetStatusBar(EStatusBar.eProcessing);
            try
            {
                var filePath = excelFileSavePath;
                Cursor.Current = Cursors.WaitCursor;
                var fileOkToOpen = true;

                #region check file

                try
                {
                    var newFile = new System.IO.FileInfo(filePath);
                    if (System.IO.File.Exists(filePath))
                    {
                        using (newFile.Open(System.IO.FileMode.Open))
                        {
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception fileExp)
                {
                    fileOkToOpen = false;
                    Console.WriteLine("File already open or other error: " + fileExp.Message);
                }

                #endregion

                if (fileOkToOpen)
                {
                    #region Old öppna med reflect proccess

                    // System.Diagnostics.Process proc = new System.Diagnostics.Process();

                    // string processPath = @"C:\Program Files\Microsoft Office\OFFICE11\";
                    // proc.StartInfo = new System.Diagnostics.ProcessStartInfo(processPath + "Excel" + ".exe", filePath);//C:\\windows\\system32\\
                    #endregion

                    #region Open log in Exel //before: tab window

                    // Start new Excel-instance
                    excelAppOpen = new Application();
                    excelAppOpen.WorkbookDeactivate += Application_WorkbookDeactivate;

                    var oldCi = Thread.CurrentThread.CurrentCulture;
                    Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                    if (excelAppOpen.Workbooks != null)
                    {
                        excelAppOpen.Workbooks._Open(
                            filePath,
                            Type.Missing,
                            0,
                            Type.Missing,
                            XlPlatform.xlWindows,
                            Type.Missing,
                            Type.Missing,
                            Type.Missing,
                            false, // COMMA
                            Type.Missing,
                            Type.Missing,
                            Type.Missing,
                            Type.Missing);
                    }

                    excelAppOpen.Visible = true;

                    Thread.CurrentThread.CurrentCulture = oldCi;

                    #endregion
                }
            }
            catch (Exception fileExp)
            {
                Console.WriteLine("Error in LoadComparedLogIn: " + fileExp.Message);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            return;
        }

        private static void Application_WorkbookDeactivate(Workbook wb)
        {
            try
            {
                // Stäng och släpp excel
                excelAppOpen.Quit();

                while (System.Runtime.InteropServices.Marshal.ReleaseComObject(excelAppOpen) != 0)
                {
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();

                // ReSharper disable RedundantAssignment
                // Wants to be sure excelAppOpen is cleared
                excelAppOpen = null;

                // ReSharper restore RedundantAssignment
            }
            catch (Exception e)
            {
                MessageBox.Show("Error while closing Excel: " + e.Message);
            }
        }
    }
}