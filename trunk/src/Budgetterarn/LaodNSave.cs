using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Budgetterarn.Operations;
using Microsoft.Office.Interop.Excel;
using Utilities;
//using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;

namespace Budgetterarn {
    class LoadNSave {
        #region Load&Save, TODO: ha dessa funktioner i egen fil

        internal static void Save(params object[] args)
        {
            //Fuul kod
            var refArg1 = args[1] as string;
            var outArg1 = args[2] as Thread;
            var refArg2 = (bool) args[10];
            Save(args[0] as Thread, ref refArg1 , out outArg1, args[3] as SortedList
                , args[4] as string
                , args[5] as string
                , args[6] as string
                , args[7] as string
                , args[8] as string
                , args[9] as string
                , ref refArg2
                , args[11] as string
                , args[12] as string
                , args[13] as Dictionary<string,string>);
        }

        /// <summary>
        /// Sparar till Excel-fil
        /// </summary>
        internal static bool Save(Thread mainThread, ref string statusLabel
            , out Thread workerThread, SortedList kontoEntries, string excelFileSavePath, string saldoLöne
            , string saldoAllkort, string saldoAllkortKreditEjFakturerat, string saldoAllkortKreditFakturerat
            , string sheetName, ref bool somethingChanged, string excelFileSavePathWithoutFileName, string excelFileSaveFileName
            , Dictionary<string, string> saldon) {
            #region Thread handling
            //if (Thread.CurrentThread == mainThread) {
            //    toolStripStatusLabel1.Text = "Saving... number of entries; " + kontoEntries.Count;

            //    workerThread = new Thread(new ThreadStart(Save));
            //    workerThread.CurrentCulture = mainThread.CurrentCulture;
            //    workerThread.CurrentUICulture = mainThread.CurrentUICulture;
            //    workerThread.Start();
            //    return;
            //}
            #endregion

            try {
                //If nothing to save, return
                if (kontoEntries == null || kontoEntries.Count == 0) {
                    workerThread = null;
                    return false;
                }

                var logThis = new Hashtable
                              {
                                  {
                                      kontoEntries.Count + 1,
                                      new object[]
                                      {
                                          "y", "m", "d", "n", "t", "g", "s", "b", "", "", "", "c", saldoLöne, saldoAllkort,
                                          saldoAllkortKreditEjFakturerat, saldoAllkortKreditFakturerat
                                      }
                                    }
                              };//Gör om till Arraylist för ordning, det blir i omvänd ordning, alltså först överst. Ex 2009-04-01 sen 2009-04-02 osv.

                if (ProgramSettings.BankType.Equals(BankType.Swedbank)
                    || ProgramSettings.BankType.Equals(BankType.Mobilhandelsbanken))
                {
                    //saldon
                    var saldoColumnNumber = 11 + 1;
                    var columnNames = new object[]
                                      {
                                          "y", "m", "d", "n", "t", "g", "s", "b", "", "", "", "c"
                                      };

                    var logArray = new object[columnNames.Length + saldon.Count];

                    var index = 0;
                    foreach (var s in columnNames) {
                        logArray[index++] = s;
                    }

                    foreach (var s in saldon.Values) {
                        logArray[saldoColumnNumber++] = s;
                    }

                    logThis = new Hashtable
                              {
                                  {
                                      kontoEntries.Count + 1,
                                      logArray
                                      }
                              };

                }
                //Lägg till överskrifter
                //y	m	d	n	t	g	s	b				c

                //int currRow = 1;
                var indexKey = kontoEntries.Count;
                foreach (DictionaryEntry currentRow in kontoEntries) {
                    //string key = currentRow.Key as string;
                    var currentKeEntry = currentRow.Value as KontoEntry;
                    if (currentKeEntry != null) {
                        logThis.Add(indexKey--, currentKeEntry.RowToSaveForThis);//Använd int som nyckel
                    }
                }

                //Done: Backup, backupa dynamiskt. Så att om man skickar in en fil så backas den upp istället för huvudfilen...men de e rätt ok att backa huvudfilen
                //Done: spara över gammalt, innan skrevs det på sist
                //Done: Gör någon backup el. likn. för att inte förlora data
                BackupOrginialFile("Before.Save", excelFileSavePath
                    , excelFileSavePathWithoutFileName, excelFileSaveFileName);
                Logger.WriteToWorkBook(excelFileSavePath, sheetName, true, logThis);

                //somethingChanged = false;//Precis sparat, så här har inget hunnit ändras 

                statusLabel = "Saving done, saved entries; " + (logThis.Count - 1);//Räkna inte överskriften, den skrivs alltid om
                //toolStripStatusLabel1.Text = "Saving done, saved entries; " + (logThis.Count - 1);//Räkna inte överskriften, den skrivs alltid om

                //Fråga om man vill öppna Excel

                if (MessageBox.Show("Open budget file (wait a litte while first)?", "Open file", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                    LoadExcelFileInExcel(excelFileSavePath);
                }
            } catch (Exception savExcp) {

                MessageBox.Show("Error: " + savExcp.Message);
            }

            workerThread = null;
            somethingChanged = false;

            return true;
        }

        public static bool GetAllEntriesFromExcelFile(string filePath, SortedList kontoEntries
            , Thread mainThread, ref string statusLabel, out Thread workerThread, ref string excelFileSavePath
            , ref string saldoLöne, ref string saldoAllkort
            , ref string saldoAllkortKreditEjFakturerat, ref string saldoAllkortKreditFakturerat
            , string sheetName, ref bool somethingChanged
            , string excelFileSavePathWithoutFileName, string excelFileSaveFileName
            , bool clearContentBeforeReadingNewFile
            , Dictionary<string, string> saldon) {

            //Call overload
            return GetAllEntriesFromExcelFile(filePath, kontoEntries, true, mainThread, ref statusLabel
                , out workerThread, kontoEntries, ref excelFileSavePath
                , ref saldoLöne, ref saldoAllkort
                , ref saldoAllkortKreditEjFakturerat, ref saldoAllkortKreditFakturerat
                , sheetName, ref somethingChanged
                , excelFileSavePathWithoutFileName, excelFileSaveFileName
                , clearContentBeforeReadingNewFile, saldon);
        }

        static bool GetAllEntriesFromExcelFile(string filePath, SortedList saveToTable, bool checkforUnsavedChanges,
            Thread mainThread, ref string statusLabel, out Thread workerThread, SortedList kontoEntries
            , ref string excelFileSavePath
            , ref string saldoLöne, ref string saldoAllkort, ref string saldoAllkortKreditEjFakturerat
            , ref string saldoAllkortKreditFakturerat
            , string sheetName, ref bool somethingChanged
            , string excelFileSavePathWithoutFileName, string excelFileSaveFileName
            , bool clearContentBeforeReadingNewFile
            , Dictionary<string, string> saldon)//showOrgInThis)
        {
            //För att se om det laddats något, så UI-uppdateras etc
            var somethingLoaded = false;


            //Backa inte upp filen innan laddning, eftersom filen inte ändras vid laddning...
            //BackupOrginialFile("Before.Load");

            //För att se om något laddats från fil
            var somethingLoadedFromFile = false;
            //Öppna fil först, och ladda, sen ev. spara ändringar, som inte ändrats av laddningen, av filöpnningen
            var kontoUtdragXls = new Hashtable();//Todo: Gör om till arraylist, eller lista av dictionary items, för att kunna välja ordning
            #region Öppna fil och hämta rader
            try {
                if (filePath == "")
                    excelFileSavePath = filePath = FileOperations.OpenFileOfType("Open file", FileType.xls, "");//Öppnar dialog

                if (string.IsNullOrEmpty(filePath)) {
                    workerThread = null;
                    return false;
                }

                if (!System.IO.File.Exists(filePath)) {
                    MessageBox.Show("File: " + filePath + " does not exist.", "File error");
                    workerThread = null;
                    return false;
                }
                OpenFileFunctions.OpenExcelSheet(filePath, sheetName, kontoUtdragXls, 0);

                somethingLoadedFromFile = kontoUtdragXls.Count > 0;
            } catch (Exception fileOpneExcp) {
                Console.WriteLine("User cancled or other error: " + fileOpneExcp.Message);

                if (kontoUtdragXls.Count < 1) {
                    workerThread = null;
                    return false;
                }
            }

            #endregion

            //kolla om något laddades från Excel
            if (kontoUtdragXls.Count < 1) {
                workerThread = null;
                return false;
            }

            //Nu har något laddats från fil, kolla då om något ska sparas
            #region Save check
            //Save check
            if (checkforUnsavedChanges && somethingLoadedFromFile) {
                if (kontoEntries.Count > 0)
                {
                    workerThread = workerThread = new Thread(new ThreadStart(voidFunc));
                    //somethingChanged är alltid fals här
                    var userResponse = WinFormsChecks.SaveCheckWithArgs(somethingChanged
                            , Save, mainThread, statusLabel, workerThread, kontoEntries
                                , excelFileSavePath, saldoLöne, saldoAllkort, saldoAllkortKreditEjFakturerat
                                , saldoAllkortKreditFakturerat, sheetName, somethingChanged
                                , excelFileSavePathWithoutFileName, excelFileSaveFileName, saldon
                        );
                    workerThread = null;
                    if (userResponse == DialogResult.Cancel)
                        return false;
                }
                else
                {
                    somethingChanged = false;
                }
            }

            #endregion


            //Töm alla tidigare entries i minnet om det ska laddas helt ny fil el. likn. 
            if (clearContentBeforeReadingNewFile) {
                kontoEntries.Clear();
            }

            #region Skapa kontoentries
            var skipped = 0;
            foreach (DictionaryEntry item in ((Hashtable)kontoUtdragXls[sheetName])) {
                if (item.Value != null) {
                    var entryArray = ((ExcelRowEntry)item.Value).args;
                    //Om det är tomt
                    if (entryArray == null)
                        continue;

                    //Om det är kolumnbeskrivning, skippa...
                    if (entryArray[0] == "y") {
                        //Spara saldon, använd det gamla värdet om inget nytt hittats från fil.
                        saldoLöne = entryArray.Length > 12 ? entryArray[12] ?? saldoLöne : saldoLöne;
                        saldoAllkort = entryArray.Length > 13 ? entryArray[13] ?? saldoAllkort : saldoAllkort;
                        saldoAllkortKreditEjFakturerat = entryArray.Length > 14 ? entryArray[14] ?? saldoAllkortKreditEjFakturerat : saldoAllkortKreditEjFakturerat;
                        saldoAllkortKreditFakturerat = entryArray.Length > 15 ? entryArray[15] ?? saldoAllkortKreditFakturerat : saldoAllkortKreditFakturerat;

                        var saldoColumnNumber = 11;
                        if (ProgramSettings.BankType == BankType.Swedbank) {
                            foreach (var s in swedbankSaldonames) {
                                var saldot = entryArray.Length > saldoColumnNumber ?
                                    entryArray[saldoColumnNumber + 1] ?? string.Empty : string.Empty;//Todo, byt empty mot värden i saldon

                                if (!saldon.ContainsKey(s)) {
                                    saldon.Add(s, saldot);
                                }
                                else {
                                    saldon[s] = saldot;
                                }

                                saldoColumnNumber++;
                            }

                        }
                        else if (ProgramSettings.BankType== BankType.Mobilhandelsbanken)
                        {
                            AddToDictionary(saldon, "LÖNEKONTO", KontoEntry.GetValueFromEntry(saldoLöne));
                            AddToDictionary(saldon, "Allkort", KontoEntry.GetValueFromEntry(saldoAllkort));
                            AddToDictionary(saldon, "ejFaktureratEtc",
                                    KontoEntry.GetValueFromEntry(saldoAllkortKreditEjFakturerat) + KontoEntry.GetValueFromEntry(saldoAllkortKreditFakturerat)
                                    );
                        }

                        //Hoppa över
                        continue;
                    }

                    var newKe = new KontoEntry(entryArray, true);
                    var key = newKe.KeyForThis;//item.Key as string;

                    //Lägg till orginalraden, gör i UI-hanterare

                    if (!saveToTable.ContainsKey(key)) {
                        #region old debug
                        //AddToRichTextBox(richTextBox1, newKE.RowToSaveForThis);

                        //test debug
                        //if (_newKontoEntries.Count < 6)
                        //{
                        //    if (!_newKontoEntries.ContainsKey(key))
                        //    {
                        //        _newKontoEntries.Add(key, newKE);
                        //        //AddToListview(m_newIitemsListOrg, newKE);
                        //    }
                        //}
                        //else 
                        #endregion
                        saveToTable.Add(key, newKe);//CreateKE(entryArray, true)

                        somethingLoaded = true;
                    } else {
                        //Detta ordnar sig, så länge saldot är med i nyckeln, det är den, så det gäller bara att ha rätt saldo i xls //Om man tagit utt t.ex. 100kr 2 ggr samma dag, från samma bankomat. hm, sätt 1 etta efteråt, men det göller ju bara det som är såna, hm, får ta dem manuellt

                        //skulle kunna tillåta någon inläsning här ev. 
                        //om man kan förutsätta att xls:en är kollad, 
                        //det får bli här man lägger till specialdubbletter manuellt
                        Console.WriteLine("Entry Double found. Key = " + key);//meddela detta till usern, man ser de på skipped...
                        skipped++;
                    }
                }
            }

            #endregion
            //Görs i Ui-handling, UpdateEntriesToSaveMemList();

            statusLabel = "No. rows loaded; " + saveToTable.Count + " . Skpped: " + skipped + ". File loaded; " + filePath;//Visa text för anv. om hur det gick etc.

            if (checkforUnsavedChanges)
                somethingChanged = false;//Nu har det precis rensats och laddats in nytt

            workerThread = null;

            return somethingLoaded;
        }

        internal static bool GetAllVisibleEntriesFromWebBrowser(SortedList kontoEntries, WebBrowser webBrowser1
            , ref string saldoAllkortKreditEjFakturerat, ref string saldoAllkortKreditFakturerat
            , SortedList newKontoEntries, ref string saldoLöne, ref string saldoAllkort, ref bool somethingChanged
            , Dictionary<string, string> saldon) {
            if (webBrowser1 == null || webBrowser1.Document == null)
                return false;

            var noKe = kontoEntries.Count;//Se om något ändras sen...
            var noNewKontoEntriesBeforeLoading = newKontoEntries.Count;

            //Kolla browser efter entries.
            if (webBrowser1.Document.Window != null) {
                switch (ProgramSettings.BankType) {
                    case BankType.Handelsbanken:
                        #region Handelsbanken

                        //var nextIsAllkreditFaktureratEtc = false;

                        //Kolla även huvuddocet
                        CheckDocForEntries(webBrowser1.Document.Window.Document, kontoEntries, ref saldoAllkortKreditEjFakturerat, ref saldoAllkortKreditFakturerat, newKontoEntries, ref saldoLöne, ref saldoAllkort, ref somethingChanged, saldon);
                            
                        if (webBrowser1.Document.Window.Frames != null) {
                            foreach (HtmlWindow currentWindow in webBrowser1.Document.Window.Frames)
                            {
                                //break;//Debug
                                var doc = currentWindow.Document;
                                CheckDocForEntries(doc, kontoEntries, ref saldoAllkortKreditEjFakturerat, ref saldoAllkortKreditFakturerat, newKontoEntries, ref saldoLöne, ref saldoAllkort, ref somethingChanged, saldon);
                            }

                        }
                        #endregion
                        break;
                    case BankType.Swedbank:
                        #region Swedbank
                        if (webBrowser1.Document.Body != null) {
                            //Get saldo
                            GetSwedbankSaldo(webBrowser1.Document.Body, saldon);

                            var saldoTable =
                                webBrowser1.Document.Body.FirstChild.NextSibling.NextSibling.FirstChild.NextSibling.
                                    FirstChild.FirstChild.FirstChild.FirstChild.FirstChild.FirstChild.FirstChild.
                                    FirstChild.NextSibling.
                                    NextSibling.FirstChild.NextSibling.FirstChild.NextSibling.NextSibling;
                            // ReSharper disable ConditionIsAlwaysTrueOrFalse
                            if (
                                saldoTable != null
                                //webBrowser1.Document.Body.FirstChild.NextSibling.NextSibling.FirstChild.NextSibling.
                                //    FirstChild.FirstChild.FirstChild.FirstChild.FirstChild.FirstChild.FirstChild.FirstChild.NextSibling.
                                //    NextSibling.FirstChild.NextSibling.FirstChild.NextSibling.NextSibling != null
                                ) {
                                // ReSharper restore ConditionIsAlwaysTrueOrFalse

                                //Get Entries
                                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                                if (saldoTable.NextSibling == null) {
                                    // ReSharper restore ConditionIsAlwaysTrueOrFalse
                                    GetHtmlEntriesFromSwedBank(
                                        saldoTable
                                            .FirstChild.FirstChild.NextSibling.Children
                                        , kontoEntries, newKontoEntries);

                                }
                                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                                else if (saldoTable.NextSibling != null) {
                                    // ReSharper restore ConditionIsAlwaysTrueOrFalse
                                    GetHtmlEntriesFromSwedBank(
                                        saldoTable.NextSibling
                                            .FirstChild.FirstChild.NextSibling.Children
                                        , kontoEntries, newKontoEntries);
                                }
                            } else if (
                                webBrowser1.Document.Body.FirstChild.NextSibling.NextSibling.FirstChild.NextSibling.
                                    FirstChild.FirstChild.FirstChild.FirstChild.FirstChild.FirstChild.FirstChild.
                                    FirstChild.NextSibling.NextSibling.FirstChild.NextSibling.FirstChild.NextSibling.
                                    FirstChild.FirstChild.NextSibling != null) {
                                //Get Entries
                                GetHtmlEntriesFromSwedBank(
                                    webBrowser1.Document.Body.FirstChild.NextSibling.NextSibling.FirstChild.NextSibling.
                                        FirstChild.FirstChild.FirstChild.FirstChild.FirstChild.FirstChild.FirstChild.
                                        FirstChild.NextSibling.NextSibling.FirstChild.NextSibling.FirstChild.NextSibling.
                                        FirstChild.FirstChild.NextSibling
                                        .FirstChild.FirstChild.NextSibling.Children
                                    , kontoEntries, newKontoEntries);
                            }
                        }
                        #endregion
                        break;
                    case BankType.Mobilhandelsbanken:
                        var htmlBody =webBrowser1.Document.Body;
                        if (htmlBody != null) {
                            GetAllEntriesFromMobileHandelsBanken(htmlBody, kontoEntries, newKontoEntries, saldon);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (kontoEntries.Count != noKe)
                somethingChanged = true;//Här har man tagit in nytt som inte är sparat

            //Returnera aom något ändrats. Är de nya inte samma som innan laddning, så är det sant att något ändrats.
            return newKontoEntries.Count != noNewKontoEntriesBeforeLoading;
        }

        private static void GetAllEntriesFromMobileHandelsBanken(HtmlElement htmlBody, SortedList kontoEntries, SortedList newKontoEntries, Dictionary<string, string> saldon)
        {
            var baseElement = htmlBody.FirstChild.FirstChild.FirstChild
                .FirstChild.NextSibling.NextSibling.FirstChild;

            var saldoElement = baseElement;

            if (saldoElement.TagName.Equals("DIV")) //.GetAttribute("link-list") != null)
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
            if (kontoEntriesElement.TagName.Equals("UL")) //.GetAttribute("link-list") != null)
            {

            }
            else
                kontoEntriesElement = kontoEntriesElement.NextSibling;

            GetHtmlEntriesFromMobileHandelsbanken(kontoEntriesElement, kontoEntries, newKontoEntries);

        }

        private static void GetMobileHandelsBankenSaldo(HtmlElement saldoElement, Dictionary<string, string> saldon)
        {
            var saldoName = saldoElement.FirstChild.FirstChild.InnerText;
            var saldoValueElem = saldoElement.FirstChild.NextSibling.NextSibling;

            var saldoValue = 0.0;

            //var allkortHas = false;
            if (saldon.ContainsKey("Allkort")
                || saldon.ContainsKey("Allkortskonto")
                )
            {
                if (saldoName.Contains("Allkort"))
                {
                    //allkortHas = true;
                    saldoName = "Allkort";
                }
            }

            if (saldoElement != null)
            {

                saldoValue = KontoEntry.GetValueFromEntry(RemoveSekFromMoneyString(saldoValueElem.InnerText));
                AddToDictionary(saldon, saldoName, saldoValue);
            }


            //Kolla disp. belopp
            var saldoNameDispBelopp = "ejFaktureratEtc";
            saldoValueElem = saldoElement.FirstChild.NextSibling.NextSibling.NextSibling.FirstChild.NextSibling;

            var saldoValueDisp = 0.0;
            if (saldoElement != null && saldoName != "LÖNEKONTO")
            {
                saldoValueDisp = KontoEntry.GetValueFromEntry(RemoveSekFromMoneyString(saldoValueElem.InnerText));

                //Räkna ut mellanskillnaden som motsvarar fakturerat och ej förfallet etc
                const int KreditBelopp = 10000;

                saldoValueDisp = saldoValue + KreditBelopp - saldoValueDisp;

                AddToDictionary(saldon, saldoNameDispBelopp, -saldoValueDisp);
            }


        }

        private static void AddToDictionary(Dictionary<string, string> saldon, string saldoName, double saldoValue)
        {
            if (saldon.ContainsKey(saldoName))
            {
                saldon[saldoName] = saldoValue.ToString();
            }
            else
            {
                saldon.Add(saldoName, saldoValue.ToString());
            }
        }

        private static void GetHtmlEntriesFromMobileHandelsbanken(HtmlElement kontoEntriesElement, SortedList kontoEntries, SortedList newKontoEntries)
        {
            var newBatchOfKontoEntriesAlreadyRed = GetNewBatchOfKontoEntriesAlreadyRed(kontoEntries, newKontoEntries);

            //var firstKontoEnrty = kontoEntriesElement.FirstChild;
            //AddNewEntryFromStringArray(GetMobileHandelsbankenTableRow(firstKontoEnrty), kontoEntries, newKontoEntries, newBatchOfKontoEntriesAlreadyRed);
            //HtmlElement htmlElement = firstKontoEnrty.NextSibling;
            foreach (HtmlElement htmlElement in kontoEntriesElement.GetElementsByTagName("LI"))
            {
                AddNewEntryFromStringArray(GetMobileHandelsbankenTableRow(htmlElement), kontoEntries, newKontoEntries, newBatchOfKontoEntriesAlreadyRed);
                
            }
            //while ((htmlElement) != null)
            //{
            //    //Lägg till ny
            //    AddNewEntryFromStringArray(GetMobileHandelsbankenTableRow(htmlElement), kontoEntries, newKontoEntries, newBatchOfKontoEntriesAlreadyRed);

            //    htmlElement = htmlElement.NextSibling;
            //}
            //string[] GetSwedBankTableRow(HtmlElement htmlElement) {
        }

        private static string[] GetMobileHandelsbankenTableRow(HtmlElement htmlElement)
        {
            var entryStrings = new string[4];

            var dateVal = htmlElement.FirstChild.InnerText.Trim();
            var infoEventVal = htmlElement.FirstChild.NextSibling.FirstChild.InnerText.Trim();
            var beloppVal = htmlElement.FirstChild.NextSibling.FirstChild.NextSibling.InnerText.Trim();

            entryStrings[0] = dateVal;
            entryStrings[1] = infoEventVal;
            entryStrings[2] = RemoveSekFromMoneyString(beloppVal);
            entryStrings[3] = string.Empty;


            return entryStrings;
        }

        private static string RemoveSekFromMoneyString(string beloppVal)
        {
            return beloppVal.Replace("SEK", string.Empty).Trim().Replace(" ", string.Empty);
        }

        private static void CheckDocForEntries(HtmlDocument doc, SortedList kontoEntries, ref string saldoAllkortKreditEjFakturerat, ref string saldoAllkortKreditFakturerat, SortedList newKontoEntries, ref string saldoLöne, ref string saldoAllkort, ref bool somethingChanged, Dictionary<string, string> saldon)
        {
            //Leta upp: "För period fr o m:t o m:"
            const string toFind = "Reskontradatum Transaktionsdatum Text Belopp Saldo";//"För period fr o m:t o m:"; // : "De senaste transaktionerna";
            if (doc == null || doc.Body == null) { }
            else
            {
                foreach (HtmlElement currentElement in doc.Body.Children)
                {
                    #region Gå igenom alla element för denna ram
                    if (currentElement.OuterText == null)
                        continue;

                    var allkortKredit = (currentElement.OuterText != null &&
                                         currentElement.OuterText.Trim().Contains("Konto: 629 011 192"));

                    #region Old
                    //if (allkortKredit) {
                    //    if (currentElement.OuterText.Trim().StartsWith("Kontonummer:629 010")) {
                    //        allkortKredit = false;
                    //        löneKonto = true;
                    //    }
                    //} 
                    #endregion

                    //Om man är i lönekontot, den har lite annan struktur
                    var löneKonto = (currentElement.OuterText != null &&
                                     currentElement.OuterText.Trim().Contains("Konto: 629 010 552"));

                    var nuKreditKonton = (currentElement.OuterText != null
                            &&
                            (
                                currentElement.OuterText.Trim().Contains("Urval:Ej fakturerat") ||
                                currentElement.OuterText.Trim().Contains("Urval:Fakturerat, ej förfallet")
                            )
                        );//Urval:Ej fakturerat

                    var löneKontoEndTextIdentifier = "Clearingnummer";
                    //Kolla saldo löne
                    if (currentElement.OuterText != null &&
                        currentElement.OuterText.Trim().Contains("Saldo:")
                        && currentElement.OuterText.Trim().Contains(löneKontoEndTextIdentifier)
                        )
                    {
                        var elemText = currentElement.OuterText.Trim();
                        var saldo =
                            //elemText.Substring(elemText.IndexOf("Saldo:") + 6, elemText.IndexOf("Information och villkor om kontot")).Trim();

                            elemText.Substring(elemText.IndexOf("Saldo:") + 6,
                                               elemText.IndexOf(löneKontoEndTextIdentifier) -
                                               (elemText.IndexOf("Saldo:") + 6)).Trim().Replace(
                                " ", string.Empty);
                        //Saldo:44 476,09 Information och villkor om kontot
                        if (löneKonto)
                        {
                            saldoLöne = saldo;
                        }
                    }

                    //Ej fakturerat:-713,81
                    //Fakturerat, ej förfallet:-3 585,77
                    //Disponibelt belopp:36 535,53 Totalt utbetald bonus 160 kr

                    var startText = "Ej fakturerat:";
                    var endText = "Fakturerat, ej förfallet";
                    var extraText = "Kontovillkor och IBAN";
                    //Kolla saldo
                    if (currentElement.OuterText != null &&
                        currentElement.OuterText.Trim().Contains(startText)
                        && currentElement.OuterText.Trim().Contains(endText)
                        )
                    {
                        var elemText = currentElement.OuterText.Trim();
                        var startIndex = elemText.IndexOf(startText) + startText.Length;
                        var endIndex = elemText.IndexOf(endText);
                        //
                        var saldo =
                            //elemText.Substring(elemText.IndexOf("Saldo:") + 6, elemText.IndexOf("Information och villkor om kontot")).Trim();

                            elemText.Substring(startIndex, endIndex - startIndex).Trim().Replace(extraText, string.Empty).Replace(" ", string.Empty);
                        //Saldo:44 476,09 Information och villkor om kontot
                        saldoAllkortKreditEjFakturerat = saldo;
                    }

                    startText = "Fakturerat, ej förfallet:";
                    endText = "Disponibelt belopp:";
                    //Kolla saldo
                    if (currentElement.OuterText != null &&
                        currentElement.OuterText.Trim().Contains(startText)
                        && currentElement.OuterText.Trim().Contains(endText)
                        )
                    {
                        var elemText = currentElement.OuterText.Trim();
                        var startIndex = elemText.IndexOf(startText) + startText.Length;
                        var endIndex = elemText.IndexOf(endText);
                        //
                        var saldo =
                            //elemText.Substring(elemText.IndexOf("Saldo:") + 6, elemText.IndexOf("Information och villkor om kontot")).Trim();

                            elemText.Substring(startIndex, endIndex - startIndex).Trim().Replace(" ", string.Empty);
                        //Saldo:44 476,09 Information och villkor om kontot
                        saldoAllkortKreditFakturerat = saldo;
                    }

                    //Saldo på kontot:30 835,11 Information och villkor om kontot
                    //Ej fakturerat:-713,81   

                    //Hämta allkort
                    startText = "Saldo på kontot:";
                    endText = "Clearingnummer";//"Information och villkor om kontot";
                    //Kolla saldo
                    if (currentElement.OuterText != null &&
                        currentElement.OuterText.Trim().Contains(startText)
                        && currentElement.OuterText.Trim().Contains(endText)
                        )
                    {
                        var elemText = currentElement.OuterText.Trim();
                        var startIndex = elemText.IndexOf(startText) + startText.Length;
                        var endIndex = elemText.IndexOf(endText);
                        //
                        var saldo =
                            //elemText.Substring(elemText.IndexOf("Saldo:") + 6, elemText.IndexOf("Information och villkor om kontot")).Trim();

                            elemText.Substring(startIndex, endIndex - startIndex).Trim().Replace(" ", string.Empty);
                        //Saldo:44 476,09 Information och villkor om kontot
                        saldoAllkort = saldo;
                    }

                    #region Old
                    //Kolla saldo
                    //if (currentElement.OuterText != null &&
                    //    currentElement.OuterText.Trim().Contains("Saldo")
                    //    && currentElement.OuterText.Trim().Contains("Beviljad kredit")
                    //    ) {
                    //    saldoAllkort =
                    //        currentElement.OuterText.Trim().Substring(
                    //            currentElement.OuterText.Trim().LastIndexOf(":") +
                    //            ":".Length);

                    //    if (!löneKonto) {
                    //        nextIsAllkreditFaktureratEtc = true;
                    //    }
                    //} 

                    #endregion
                    var noTables = 0;

                    if (nuKreditKonton)
                    {
                        foreach (HtmlElement subElement in currentElement.All)
                        {
                            #region Leta upp den andra tabellen

                            if (subElement.TagName.ToLower() != "table")
                            {
                                continue;
                            }

                            noTables++;
                            if (noTables == 4) //Hoppa över 1:a tabellen
                            {
                                CheckHtmlTr(subElement, kontoEntries, newKontoEntries, true);
                            }

                            #endregion
                        }
                    }

                    if (((currentElement.OuterText == null ||
                          (!currentElement.OuterText.Trim().StartsWith(toFind) &&
                           !currentElement.OuterText.Trim().Contains(toFind))) && !allkortKredit))
                    {
                        continue;
                    }

                    //Leta upp den andra tabellen
                    noTables = 0;
                    foreach (HtmlElement subElement in currentElement.All)
                    {
                        #region Leta upp den andra tabellen

                        if (subElement.TagName.ToLower() != "table")
                        {
                            continue;
                        }

                        #region Old
                        //if (allkortKredit) {
                        //    noTables++;
                        //    if (noTables > 1) //Hoppa över 1:a tabellen
                        //    {
                        //        CheckHtmlTr(subElement, kontoEntries, newKontoEntries);
                        //    }
                        //} else {

                        #endregion
                        noTables++; //va fel här innan...
                        #region Old
                        if (noTables == 13 && !löneKonto && allkortKredit)
                        {
                            CheckHtmlTr(subElement, kontoEntries, newKontoEntries);
                        }
                        else
                        #endregion
                            if (noTables == 12)
                            {
                                CheckHtmlTr(subElement, kontoEntries, newKontoEntries);
                            }
                        //}

                        #endregion
                    }

                    #endregion
                }
            }
        }

        private static readonly List<string> swedbankSaldonames = new List<string>
                                                  {
                                                      "Privatkonto 8417-8,4 751 687-7"
                                                      ,"Servicekonto 8417-8,4 778 356-8"
                                                      ,"Servicekonto 8417-8,914 636 458-4"
                                                      ,"e-sparkonto 8417-8,983 306 619-5"
                                                  };

        private static void GetSwedbankSaldo(HtmlElement htmlElement
            , Dictionary<string, string> saldon
            ) {
            //foreach (HtmlElement currentElem in htmlElement.Children) {
            //    if (currentElem.InnerText != null && currentElem.InnerText.Contains("Saldo")) {
            var nextIsSaldo = false;
            //var nextIsLöne = false;
            var saldoNameNnumber = "";

            foreach (HtmlElement currentSubElem in htmlElement.All) {


                if (nextIsSaldo) {
                    if (saldoNameNnumber != string.Empty) {

                        if (!saldon.ContainsKey(saldoNameNnumber)) {
                            saldon.Add(saldoNameNnumber, currentSubElem.InnerText);
                        }
                        else {
                            saldon[saldoNameNnumber] = currentSubElem.InnerText;
                        }

                        break;
                    }
                }
                if (currentSubElem.InnerText != null) {
                    if (!currentSubElem.TagName.Equals("OPTION") &&
                        //h3
                        currentSubElem.TagName.Equals("H3")
                        ) {

                        foreach (var s in swedbankSaldonames) {
                            if (currentSubElem.InnerText.Contains(s))
                                saldoNameNnumber = s;
                        }

                        //if (currentSubElem.InnerText.Contains("8417-8,4 751 687-7"))
                        //    nextIsLöne = true;
                        //else if (currentSubElem.InnerText.Contains("Privatkonto 8417-8,4 751 687-7"))
                        //    saldoNameNnumber = "Privatkonto 8417-8,4 751 687-7";

                    }

                    if (currentSubElem.InnerText.Equals("Saldo")) {
                        nextIsSaldo = true;
                    }
                }
                //foreach (HtmlElement currentSubSubElem in currentSubElem.Children) {
                //    foreach (HtmlElement currentSubSubSubElem in currentSubSubElem.Children) {

                //    }

                //}
            }
            //    }
            //}

        }

        private static void GetHtmlEntriesFromSwedBank(HtmlElementCollection htmlElements, SortedList kontoEntries, SortedList newKontoEntries) {
            //Spara en batch, dyker det upp dubletter i samma, så ska de ses som unika
            var newBatchOfKontoEntriesAlreadyRed = GetNewBatchOfKontoEntriesAlreadyRed(kontoEntries, newKontoEntries);

            var first = true;
            foreach (HtmlElement htmlElement in htmlElements) {
                //Skip first, column descriptons
                if (first) {
                    first = false;
                } else {
                    if (htmlElement.Children.Count < 2)
                        continue;

                    if (htmlElement.InnerText.StartsWith("Föregående"))
                        break;

                    //Lägg till ny
                    AddNewEntryFromStringArray(GetSwedBankTableRow(htmlElement), kontoEntries, newKontoEntries, newBatchOfKontoEntriesAlreadyRed);
                }
            }
        }

        private static string[] GetSwedBankTableRow(HtmlElement htmlElement) {
            var entryStrings = new string[4];
            var fieldNo = 0;
            //foreach (HtmlElement transEntry in htmlElement.Children) {
            //    //TODO: ta in varannan eller få tag i namngivna tabbar, för att kunna läsa in tomma värden, så att varje värde får rätt typ
            //    if (transEntry.InnerHtml.StartsWith("&") || transEntry.InnerHtml.StartsWith("<")) {
            //        continue;
            //    }

            //    if (entryStrings.Length > fieldNo)
            //        entryStrings[fieldNo++] = transEntry.InnerText;
            //}

            const int dateColNum = 1;
            const int eventColNum = 2;
            const int beloppColNum = 4;
            const int saldoColNum = 5;

            entryStrings[fieldNo++] = htmlElement.Children[dateColNum] != null
                                          ? "20" + htmlElement.Children[dateColNum].InnerText
                                          : string.Empty;
            entryStrings[fieldNo++] = htmlElement.Children[eventColNum] != null
                                          ? htmlElement.Children[eventColNum].InnerText
                                          : string.Empty;

            entryStrings[fieldNo++] = htmlElement.Children.Count > beloppColNum ?
                                    (htmlElement.Children[beloppColNum] != null
                                    ? htmlElement.Children[beloppColNum].InnerText
                                    : string.Empty)
                                :
                                    (htmlElement.Children[3] != null
                                    ? htmlElement.Children[3].InnerText
                                    : string.Empty);

            entryStrings[fieldNo] = htmlElement.Children.Count > saldoColNum && htmlElement.Children[saldoColNum] != null
                                        ? htmlElement.Children[saldoColNum].InnerText
                                        : string.Empty;

            return entryStrings;
        }

        private static void CheckSwedBankHtml(HtmlElement element, SortedList kontoEntries, SortedList newKontoEntries) {

        }

        private static void CheckHtmlTr(HtmlElement subElement, SortedList kontoEntries, SortedList newKontoEntries) {
            CheckHtmlTr(subElement, kontoEntries, newKontoEntries, false);
        }

        private static void CheckHtmlTr(HtmlElement subElement, SortedList kontoEntries, SortedList newKontoEntries, bool kreditEtc3Columns) {
            var noTRs = 0;

            if (newKontoEntries == null || kontoEntries == null)
                return;

            //Spara en batch, dyker det upp dubletter i samma, så ska de ses som unika
            var newBatchOfKontoEntriesAlreadyRed = GetNewBatchOfKontoEntriesAlreadyRed(kontoEntries, newKontoEntries);

            //Hoppa över första TR
            foreach (HtmlElement transacion in subElement.All) {
                if (transacion.TagName.ToLower() != "tr") {
                    continue;
                }

                noTRs++;
                if (noTRs <= 1) {
                    continue;
                }

                var entryStrings = new string[4];
                var fieldNo = 0;
                var noTrans = 0;
                foreach (HtmlElement transEntry in transacion.All) {
                    //hoppa över de 2 första
                    noTrans++;
                    if (noTrans <= 2 && !kreditEtc3Columns) {
                        continue;
                    }

                    //TODO: ta in varannan eller få tag i namngivna tabbar, för att kunna läsa in tomma värden, så att varje värde får rätt typ
                    if (transEntry.InnerHtml == null || transEntry.InnerHtml.StartsWith("&") || transEntry.InnerHtml.StartsWith("<")) {
                        continue;
                    }

                    if (entryStrings.Length > fieldNo)
                        entryStrings[fieldNo++] = transEntry.InnerText;
                }

                //Lägg till ny
                AddNewEntryFromStringArray(entryStrings, kontoEntries, newKontoEntries, newBatchOfKontoEntriesAlreadyRed);
            }
        }

        private static SortedList GetNewBatchOfKontoEntriesAlreadyRed(SortedList kontoEntries, SortedList newKontoEntries) {
            var newBatchOfKontoEntriesAlreadyRed = new SortedList();
            foreach (DictionaryEntry entry in newKontoEntries) {
                if (!newBatchOfKontoEntriesAlreadyRed.ContainsKey(entry.Key))
                    newBatchOfKontoEntriesAlreadyRed.Add(entry.Key, entry.Value);
            }
            foreach (DictionaryEntry entry in kontoEntries) {
                if (!newBatchOfKontoEntriesAlreadyRed.ContainsKey(entry.Key))
                    newBatchOfKontoEntriesAlreadyRed.Add(entry.Key, entry.Value);
            }

            return newBatchOfKontoEntriesAlreadyRed;
        }

        private static void AddNewEntryFromStringArray(string[] entryStrings, SortedList kontoEntries, SortedList newKontoEntries, SortedList newBatchOfKontoEntriesAlreadyRed) {
            var newKeFromHtml = new KontoEntry(entryStrings);
            var key = newKeFromHtml.KeyForThis;

            if (!kontoEntries.ContainsKey(key) && !newKontoEntries.ContainsKey(key)) //Kollas även senare
                {
                //if (newKontoEntries != null) {// && !newKontoEntries.ContainsKey(key)) {
                if (key != null) {
                    newKontoEntries.Add(key, newKeFromHtml);
                    //}
                    //else {
                    //    //Dubblett
                    //}
                }
                //Handle Doubles
            } else if (!newBatchOfKontoEntriesAlreadyRed.ContainsKey(key)) {//Om man hade entryn i Excel, innan laddning, och innan man gick igenom nya, så kan man (förutsätter att man då det inte finns saldo (i allkort-kredit), så läses hela listan in i ett svep, det är inte en lista, det kan ev. bli dubblet om två datum hamnar på olika allkort-kredit-fakturor)
                var userDecision = MessageBox.Show("Found potential double: " + newKeFromHtml.KeyForThis,
                                                   "Double, SaveThisEntry?", MessageBoxButtons.YesNo);

                if (userDecision.Equals(DialogResult.Yes)) {
                    //Detta är en dubblett, men om det finns fler än 2 dubbletter så måste man se till att nyckeln är unik
                    while (newKontoEntries.ContainsKey(newKeFromHtml.KeyForThis)) {
                        //Stega upp saldo, tills en unik nyckel skapats
                        newKeFromHtml.SaldoOrginal += newKeFromHtml.KostnadEllerInkomst != 0
                                                          ? newKeFromHtml.KostnadEllerInkomst
                                                          : 1;
                    }

                    newKontoEntries.Add(newKeFromHtml.KeyForThis, newKeFromHtml);
                }
                //För annat än Allkortskredit, så ordnar Detta sig, så länge saldot är med i nyckeln, det är den, så det gäller bara att ha rätt saldo i xls //Om man tagit utt t.ex. 100kr 2 ggr samma dag, från samma bankomat. hm, sätt 1 etta efteråt, men det göller ju bara det som är såna, hm, får ta dem manuellt
            }
        }

        static void BackupOrginialFile(string typeOfBackup, string excelFileSavePath, string excelFileSavePathWithoutFileName, string excelFileSaveFileName) {
            BackupOrginialFile(excelFileSavePath, excelFileSavePathWithoutFileName, typeOfBackup + "." + excelFileSaveFileName);
        }
        static void BackupOrginialFile(string orgfilePath, string newFilePathWithoutFileName, string newFileName) {
            //TODO: check that dir exists and path etc
            System.IO.File.Copy(orgfilePath, newFilePathWithoutFileName + @"bak\" + newFileName + "." + DateTime.Now.ToString(new System.Globalization.CultureInfo("sv-SE")).Replace(":", ".") + ".bak.xls", true);
        }
        #endregion


        //Extrafunktioner

        //Contains Excelapp to use for opening file after save, in Excel mode. And auto close it when close...
        static Excel.Application excelAppOpen;

        static void LoadExcelFileInExcel(string excelFileSavePath) {
            //SetStatusBar(EStatusBar.eProcessing);

            try {
                var filePath = excelFileSavePath;
                Cursor.Current = Cursors.WaitCursor;
                var fileOkToOpen = true;

                #region check file

                try {
                    var newFile = new System.IO.FileInfo(filePath);
                    if (System.IO.File.Exists(filePath)) {
                        using (newFile.Open(System.IO.FileMode.Open)) {

                        }
                    } else {
                        return;
                    }
                } catch (Exception fileExp) {
                    fileOkToOpen = false;
                    Console.WriteLine("File already open or other error: " + fileExp.Message);
                }
                #endregion

                if (fileOkToOpen) {
                    #region Old öppna med reflect proccess
                    //System.Diagnostics.Process proc = new System.Diagnostics.Process();

                    //string processPath = @"C:\Program Files\Microsoft Office\OFFICE11\";
                    //proc.StartInfo = new System.Diagnostics.ProcessStartInfo(processPath + "Excel" + ".exe", filePath);//C:\\windows\\system32\\

                    #endregion
                    #region Open log in Exel //before: tab window

                    //Start new Excel-instance
                    excelAppOpen = new ApplicationClass();
                    excelAppOpen.WorkbookDeactivate += Application_WorkbookDeactivate;


                    var oldCi = Thread.CurrentThread.CurrentCulture;
                    Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                    if (excelAppOpen.Workbooks != null) {
                        excelAppOpen.Workbooks._Open(filePath,
                                                 Type.Missing,
                                                 0,
                                                 Type.Missing,
                                                 XlPlatform.xlWindows,
                                                 Type.Missing,
                                                 Type.Missing,
                                                 Type.Missing,
                                                 false, //COMMA
                                                 Type.Missing,
                                                 Type.Missing,
                                                 Type.Missing,
                                                 Type.Missing
                            );
                    }

                    excelAppOpen.Visible = true;

                    Thread.CurrentThread.CurrentCulture = oldCi;
                    #endregion
                }

            } catch (Exception fileExp) {
                Console.WriteLine("Error in LoadComparedLogIn: " + fileExp.Message);
            } finally {
                Cursor.Current = Cursors.Default;
            }

            return;
        }

        static void Application_WorkbookDeactivate(Workbook wb) {
            try {
                //Stäng och släpp excel
                excelAppOpen.Quit();

                while (System.Runtime.InteropServices.Marshal.ReleaseComObject(excelAppOpen) != 0) { }

                GC.Collect();
                GC.WaitForPendingFinalizers();

                // ReSharper disable RedundantAssignment
                //Wants to be sure excelAppOpen is cleared
                excelAppOpen = null;
                // ReSharper restore RedundantAssignment

            } catch (Exception e) {
                MessageBox.Show("Error while closing Excel: " + e.Message);
            }
        }



        public static void voidFunc() {
            //Do nothing
        }

    }
}
