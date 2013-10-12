using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Budgeter.Core.Entities;
using Budgeter.Winforms;
using Budgetterarn.Application_Settings_and_constants;
using Budgetterarn.InternalUtilities;
using Budgetterarn.Operations;

// Budgeter.Winforms
namespace Budgetterarn
{
    // Todos se Data/Todos.txt

    /// <summary>
    /// Xls-fil som läses in förutsätts ha Kontoutdrag_officiella som ark med 6 celler ejämte varann enligt ex. nedan:
    /// 2007-09-05 SkyDDat belopp -120,00 100 991,24 127,02 telefonsamtal
    /// </summary>
    public partial class BudgeterForm : Form
    {
        public const string VersionNumber = "1.0.1.9";

        #region Members

        private const string SheetName = "Kontoutdrag_officiella"; // "Kontoutdrag f.o.m. 0709 bot.up.";
        private static string bankUrl =
            "http://www.handelsbanken.se/247igaa.nsf/default/LoginBankId?opendocument&redir=privelegsv";

        private static string categoryPath = @"Data\Categories.xml";
        private readonly bool debugGlobal; // For useSaveCheck

        // static string BankUrlHandelsBanken = "http://www.handelsbanken.se/247igaa.nsf/default/LoginBankId?opendocument&redir=privelegsv";
        private readonly SortedList kontoEntries = new SortedList(new DescendingComparer());

        // Ev. byta denna mot en klass med innehåll och nyckel, för att behålla orginalordningen på posterna. Sorteras med nyaste först
        private readonly Thread mainThread;
        private readonly Stack<DoneNavigationAction> navigatedNextActionIsStack = new Stack<DoneNavigationAction>();
        private readonly SortedList newKontoEntries = new SortedList();
        private readonly Dictionary<string, string> saldon = new Dictionary<string, string>();

        // Key = description, Value= amount
        // private static string _excelFileSavePathWithoutFileName;// = @"C:\stuff\budget\";//Hårdkodad sökväg utan dialog
        // private static string _excelFileSaveFileName;// = @"Test LG Budget.xls";//Pelles Budget.xls";//Hårdkodad sökväg utan dialog
        // private string _excelFileSavePath;// = _excelFileSavePathWithoutFileName + _excelFileSaveFileName;//Hårdkodad sökväg utan dialog
        // string _excelFileSavePath = @"C:\Documents and Settings\hu\My Documents\CoNy kolumn of Test Pelles kontoutdrag.xls";//Hårdkodad sökväg utan dialog
        // const string m_s_newEntriesXlsDebug = @"C:\Documents and Settings\hu\My Documents\NYA entries test Pelles kontoutdrag.xls";
        private bool somethingChanged;

        // To do, sätt alla medlemmar i en egen klass etc.
        // string saldoLöne = "";
        // string saldoAllkort = "";
        // string saldoAllkortKreditEjFakturerat = "";
        // string saldoAllkortKreditFakturerat = "";
        private Thread workerThread;

        // Excel.Application _excelApp = new Excel.Application();//Denna ligger här för att kunna släppa objektet i delegat nedan (Application_WorkbookDeactivate)
        // Navigering i browser
        #endregion

        // IF CHANGED VERSION. DOCUMENT CHANGES!!!AND COMMIT See Changes summary below.
        // 1.0.1.9 Gjort anpassningar till Excel engelsk version map datumformat etc. Nu loggas celler som objekt istället för strängar. Div omstrukturering i testProjekt och tillagda projekt.
        // 1.0.1.8 För handelsbanken mobil. Autonavigera med inloggning etc. Så allt nytt laddas in automatiskt. + Snabbkanapp Ctrl+L för att ladda entries.
        // 1.0.1.7 Kan nu ladda entries från Handelsbanken mobilsida. Som har enklare inloggning. +Buggfix med sortering av nya entries.
        // 1.0.1.6 Enklare sparning utan prompt. Autosave som val när man lagt till nya entries. Även ändrat för funktioner som sparar och laddar. Fixat så det går att anv designervyn i VS.
        // 1.0.1.5 Fixed new saldos for SHB. Double-mbox clearified, better handling of uniques and double-entries.
        // 1.0.1.4 Fixed autocat set, so it is less unneccesary popups to user. Started to Add functionality for Swedbank.
        // 1.0.1.3 Addad exception catch att Exclel close. Now user selects if autocat shold overwrite existing choices. Added info about how to set several cats at the same time. Added sorting on listviews. Columns from excel should now be correct in listviews.
        // 1.0.1.2 Fixed so tag is also set when just selecting cat on new entries from web, also a halfsmart (not full proof, bu probably never gonna err...) doublechecker added.
        // 1.0.1.1 Changed way Version number is set
        // 1.0.1.0 PopupComboboxOfCaytegories had a bugg with wrong colwidth added when checking postion, only noticable if not all columns have same length. Nicer set autocat and popup. användaren kan sätta autokat.
        // 1.0.0.1 Nothing new yet, Later singleclick in newlist etc.
        // 1.0.0.0 Everything before, see Svn. Even Added mulitiselect etc.
        public BudgeterForm() // Konstruktor
        {
            // Todo senast:
            #region Inits

            try
            {
                // Get file names from settings file
                categoryPath = GeneralSettings.GetStringSetting("CategoryPath");
                bankUrl = GeneralSettings.GetTextfileStringSetting("BankUrl");

                #region Old

                // if (!string.IsNullOrEmpty(bankUrl)) {
                // if (bankUrl == @"http://www.handelsbanken.se") {
                // bankUrl = BankUrlHandelsBanken;
                // }
                // }
                #endregion

                // var t = new CategoriesHolder();
                // Ladda kategorier som man har till att flagga olika kontohändelser
                CategoriesHolder.DeserializeObject(categoryPath);

                // Initiera UI-objekt
                InitializeComponent();
                InitSpecialGenericUIElements();
                SetStatusLabelProps();

                // Sätt nuvarande tråd som main
                mainThread = Thread.CurrentThread;

                // läs in xls...
                GetAllEntriesFromExcelFile(Filerefernces._excelFileSavePath, true);

                #region Debug

                var debug = false;
#if DEBUG
                debug = true;
#endif

                if (!debugGlobal)
                {
                    debug = debugGlobal;
                }

                if (debugGlobal && debug)
                {
                    debugGlobal = true;
                }
                else
                {
                    debugGlobal = false;
                }

                if (debug)
                {
                    // TODO: GetAllEntriesFromExcelFile(m_s_newEntriesXlsDebug, _newKontoEntries, false, null);
                    // CheckAndAddNewItems();//Debug: Lägg till nya i GuiLista
                    debugbtn.Visible = true;
                    DebugAddoNewList();
                }

                    #endregion
                else
                {
                    // Öpnna banksidan direkt
                    OpenBankSiteInBrowser();

                    AutoNavigateToKontonEtc();

                    // Thread.Sleep(100);
                    // webBrowser1.Navigate(
                    // "https://secure.handelsbanken.se" +
                    // "/bb/seip/servlet/UASipko?appAction=ShowAccountOverview&appName=ipko"
                    // );
                }

                #region Old

                // string sheetName = "Kontoutdrag_officiella";// "Kontoutdrag f.o.m. 0709 bot.up.";
                // 2009 3 2009-03-26   JohaMsMatBio   -10   50 951,93 spara till russel övrigt
                // string[] temp1 = new string[] { "2009", "3",    "2009-03-26", "JohaMsMatBio", "-10", "50 951,93", "spara till russel övrigt" };
                // 2009 3 2009-03-25   LÖN   17 969,00   50 961,93 + 

                // Utilities.ExcelRowEntry newE = new Utilities.ExcelRowEntry(0, temp1);

                // if (!_kontoUtdragXLS.ContainsKey(mergeStringArrayToString(temp1)))
                // _kontoUtdragXLS.Add(mergeStringArrayToString(temp1), newE);
                // webBrowser1.Url = "";

                // läs in html...
                // OpenBankSiteInBrowser();//Gör ej som default.
                #endregion

                // Sätt versionsnummer i titel
                if (Text != null)
                {
                    Text += VersionNumber;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error! : " + e.Message);
            }

            #endregion

            #region Test

            // var ifHb = ProgramSettings.BankType.Equals(BankType.Handelsbanken);
            #endregion
        }

        // Settings (mostly debug)
        public delegate void DoneNavigationAction();

        /// <summary>
        /// Titeltexten för fönstret
        /// </summary>
        public override sealed string Text
        {
            get
            {
                return base.Text;
            }

            set
            {
                base.Text = value;
            }
        }

        public bool AutoLoadEtce()
        {
            var s = GeneralSettings.GetStringSetting("AutonavigateEtc");

            bool b;
            return bool.TryParse(s, out b) && b;
        }

        private void AutoNavigateToKontonEtc()
        {
            if (!AutoLoadEtce())
            {
                return;
            }

            // Korttransaktioner
            navigatedNextActionIsStack.Push(LoadCurrentEntriesFromBrowser);

            // navigatedNextActionIsSatck.Push(NavigateToFirstItemInVisibleList);

            ////Allkonto
            navigatedNextActionIsStack.Push(LoadEntriesAndGoToFirst);
            navigatedNextActionIsStack.Push(NavigateToAllKonto);

            // navigatedNextActionIsSatck.Push(BrowserGoBack);

            // Löne
            navigatedNextActionIsStack.Push(LoadEntriesAndGoBack);
            navigatedNextActionIsStack.Push(NavigateToLöneKonto);

            // Inlogg
            navigatedNextActionIsStack.Push(NavigateToFirstItemInVisibleList);
            navigatedNextActionIsStack.Push(SetLoginUserEtc);
            navigatedNextActionIsStack.Push(NavigateToFirstItemInVisibleList);
        }

        private void LoadEntriesAndGoBack()
        {
            LoadCurrentEntriesFromBrowser();
            BrowserGoBack();
        }

        private void LoadEntriesAndGoToFirst()
        {
            LoadCurrentEntriesFromBrowser();
            NavigateToFirstItemInVisibleList();
        }

        private void InitSpecialGenericUIElements()
        {
            newIitemsListEdited = new Budgetterarn.ListViewWithComboBox();
            newIitemsListOrg = new KontoEntryListView();
            entriesInToBeSaved = new Budgetterarn.KontoEntryListView();
            xlsOrginalEntries = new Budgetterarn.KontoEntryListView();

            // tp_NewItemsEdited
            tp_NewItemsEdited.Controls.Add(newIitemsListEdited);
            tp_NewItemsEdited.Location = new System.Drawing.Point(4, 22);
            tp_NewItemsEdited.Name = "tp_NewItemsEdited";
            tp_NewItemsEdited.Padding = new System.Windows.Forms.Padding(3);
            tp_NewItemsEdited.Size = new System.Drawing.Size(1161, 551);
            tp_NewItemsEdited.TabIndex = 0;
            tp_NewItemsEdited.Text = "New items edited";
            tp_NewItemsEdited.UseVisualStyleBackColor = true;

            // m_newIitemsListEdited
            newIitemsListEdited.Anchor = (((System.Windows.Forms.AnchorStyles.Top
                                              | System.Windows.Forms.AnchorStyles.Bottom)
                                             | System.Windows.Forms.AnchorStyles.Left)
                                            | System.Windows.Forms.AnchorStyles.Right);
            newIitemsListEdited.FullRowSelect = true;
            newIitemsListEdited.GridLines = true;
            newIitemsListEdited.Location = new Point(3, 3);
            newIitemsListEdited.Name = "m_newIitemsListEdited";
            newIitemsListEdited.Size = new Size(855, 545);
            newIitemsListEdited.TabIndex = 0;
            newIitemsListEdited.UseCompatibleStateImageBehavior = false;
            newIitemsListEdited.View = View.Details;

            // tp_NewItemsOrg
            tp_NewItemsOrg.Controls.Add(newIitemsListOrg);
            tp_NewItemsOrg.Location = new Point(4, 22);
            tp_NewItemsOrg.Name = "tp_NewItemsOrg";
            tp_NewItemsOrg.Padding = new Padding(3);
            tp_NewItemsOrg.Size = new Size(1161, 551);
            tp_NewItemsOrg.TabIndex = 1;
            tp_NewItemsOrg.Text = @"New items orginal";
            tp_NewItemsOrg.UseVisualStyleBackColor = true;

            // m_newIitemsListOrg
            newIitemsListOrg.Columns.AddRange(
                new[] { c_Date, c_Info, c_KostnadEllerInkomst, c_SaldoOrginal, c_AckumuleratSaldo, c_TypAvKostnad });
            newIitemsListOrg.Dock = DockStyle.Fill;
            newIitemsListOrg.FullRowSelect = true;
            newIitemsListOrg.GridLines = true;
            newIitemsListOrg.Location = new Point(3, 3);
            newIitemsListOrg.Name = "m_newIitemsListOrg";
            newIitemsListOrg.Size = new Size(1155, 545);
            newIitemsListOrg.TabIndex = 0;
            newIitemsListOrg.UseCompatibleStateImageBehavior = false;
            newIitemsListOrg.View = View.Details;

            m_inMemoryList.Controls.Add(entriesInToBeSaved);

            // m_EntriesInToBeSaved
            entriesInToBeSaved.Dock = DockStyle.Fill;
            entriesInToBeSaved.FullRowSelect = true;
            entriesInToBeSaved.GridLines = true;
            entriesInToBeSaved.Location = new Point(3, 3);
            entriesInToBeSaved.Name = "m_EntriesInToBeSaved";
            entriesInToBeSaved.Size = new Size(288, 577);
            entriesInToBeSaved.TabIndex = 0;
            entriesInToBeSaved.UseCompatibleStateImageBehavior = false;
            entriesInToBeSaved.View = View.Details;

            // m_originalXls
            m_originalXls.Controls.Add(xlsOrginalEntries);
            m_originalXls.Location = new Point(4, 22);
            m_originalXls.Name = "m_originalXls";
            m_originalXls.Padding = new Padding(3);
            m_originalXls.Size = new Size(294, 583);
            m_originalXls.TabIndex = 0;
            m_originalXls.Text = @"Xls Original";
            m_originalXls.UseVisualStyleBackColor = true;

            // m_XlsOrginalEntries
            xlsOrginalEntries.Dock = DockStyle.Fill;
            xlsOrginalEntries.FullRowSelect = true;
            xlsOrginalEntries.GridLines = true;
            xlsOrginalEntries.Location = new Point(3, 3);
            xlsOrginalEntries.Name = "m_XlsOrginalEntries";
            xlsOrginalEntries.Size = new Size(288, 577);
            xlsOrginalEntries.TabIndex = 0;
            xlsOrginalEntries.UseCompatibleStateImageBehavior = false;
            xlsOrginalEntries.View = View.Details;

            entriesInToBeSaved.ListViewItemSorter = new ListViewColumnSorter();
            xlsOrginalEntries.ListViewItemSorter = new ListViewColumnSorter();
            newIitemsListEdited.ListViewItemSorter = new ListViewColumnSorter();
            newIitemsListOrg.ListViewItemSorter = new ListViewColumnSorter();
        }

        /// <summary>
        /// Uses members in this class
        /// </summary>
        /// <param name="excelFileSavePath">
        /// </param>
        /// <param name="clearContentBeforeReadingNewFile">
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool GetAllEntriesFromExcelFile(string excelFileSavePath, bool clearContentBeforeReadingNewFile)
        {
            var statusText = toolStripStatusLabel1.Text = "Nothing loaded.";
            var changed_excelFileSavePath = Filerefernces._excelFileSavePath;
            var kontoutdragInfoForLoad = new KontoutdragInfoForLoad
                                         {
                                             filePath = Filerefernces._excelFileSavePath, 
                                             excelFileSavePath = changed_excelFileSavePath, 
                                             excelFileSavePathWithoutFileName =
                                                 Filerefernces.ExcelFileSavePathWithoutFileName, 
                                             
                                             
                                             // Todo: gör en funktion för denna eller refa med en filnamns och sökvägsklass....
                                             excelFileSaveFileName = Filerefernces._excelFileSaveFileName, 
                                             sheetName = SheetName, 
                                             clearContentBeforeReadingNewFile = clearContentBeforeReadingNewFile, 
                                             somethingChanged = somethingChanged, 
                                         };

            // Ladda från fil
            var entriesLoadedFromDataStore = LoadNSave.LoadEntriesFromFile(kontoutdragInfoForLoad, ref workerThread);

            // För att se om något laddats från fil
            var somethingLoadedFromFile = false;
            somethingLoadedFromFile = entriesLoadedFromDataStore.Count > 0;

            // kolla om något laddades från Excel
            if (!somethingLoadedFromFile) // kontoUtdragXls.Count < 1)
            {
                return false;
            }

            var checkforUnsavedChanges = true;
            var userCanceled = SaveFirstCheck(
                kontoutdragInfoForLoad, checkforUnsavedChanges, somethingLoadedFromFile);

            if (userCanceled)
            {
                return false;
            }

            var loadResult = LoadNSave.GetAllEntriesFromExcelFile(
                kontoutdragInfoForLoad, 
                kontoEntries, 
                // _mainThread,
                // ref _workerThread,
                saldon, 
                entriesLoadedFromDataStore);

            statusText = "No. rows loaded; " + kontoEntries.Count + " . Skpped: " + loadResult.skippedOrSaved
                         + ". File loaded; " + kontoutdragInfoForLoad.filePath;

            // Visa text för anv. om hur det gick etc.
            if (checkforUnsavedChanges)
            {
                kontoutdragInfoForLoad.somethingChanged = false; // Nu har det precis rensats och laddats in nytt
            }

            // Ev. har pathen ändrats.
            if (excelFileSavePath == string.Empty)
            {
                // Om man lagt till nya rader från annan fil, så spara i den gamla.
            }
            else
            {
                // Har man däremot laddat in nya så ska den sökvägen gälla för sparningar
                Filerefernces._excelFileSavePath = changed_excelFileSavePath;

                // Todo: sätt denna tidigare så att LoadNsave bara gör vad den ska utan UI etc
            }

            // toolStripStatusLabel1.Text = statusText + " Saldon: Allkort:" + saldoAllkort + ", Löne:" + saldoLöne + ", Kredit Ej fakt.:" + saldoAllkortKreditEjFakturerat + ", Kredit fakt.:" + saldoAllkortKreditFakturerat;

            // If nothing loaded return
            if (!loadResult.somethingLoadedOrSaved)
            {
                return false;
            }

            // Lägg till orginalraden, gör i UI-hanterare
            // Lägg in det som är satt att sparas till minnet (viasa alla _kontoEntries i listview). Även uppdatera färg på text.
            ViewUpdateUi.UpdateListViewFromSortedList(xlsOrginalEntries, kontoEntries);
            ViewUpdateUi.UpdateListViewFromSortedList(entriesInToBeSaved, kontoEntries);

            return true;
        }

        public static DialogResult SaveCheckWithArgs(
            KontoutdragInfoForLoad kontoutdragInfoForSave, SortedList kontoEntries, Dictionary<string, string> saldon)
        {
            var saveOr = DialogResult.None;
            if (kontoutdragInfoForSave.somethingChanged)
            {
                saveOr = MessageBox.Show("Läget ej sparat! Spara nu?", "Spara?", MessageBoxButtons.YesNoCancel);

                // Cancel
                if (saveOr == DialogResult.Yes)
                {
                    LoadNSave.Save(kontoutdragInfoForSave, kontoEntries, saldon);
                }
            }

            return saveOr;
        }

        private bool SaveFirstCheck(
            KontoutdragInfoForLoad kontoutdragInfoForLoad, 
            bool checkforUnsavedChanges, 
            bool somethingLoadedFromFile)
        {
            // Nu har något laddats från fil, kolla då om något ska sparas
            #region Save check

            // Save check
            if (checkforUnsavedChanges && somethingLoadedFromFile)
            {
                if (kontoEntries.Count > 0)
                {
                    // somethingChanged är alltid false här
                    var userResponse = SaveCheckWithArgs(kontoutdragInfoForLoad, kontoEntries, saldon);
                    if (userResponse == DialogResult.Cancel)
                    {
                        return true;
                    }
                }
                else
                {
                    kontoutdragInfoForLoad.somethingChanged = false;
                }
            }

            #endregion

            return false;
        }

        public static void VoidFunc()
        {
            // Do nothing
        }

        private static ToolStripStatusLabel toolStripStatusLabel1;
        public static string StatusLabelText
        {
            get
            {
                return toolStripStatusLabel1.Text;
            }

            set
            {
                toolStripStatusLabel1.Text = value;
            }
        }

        public HtmlElement FindChildWithId(HtmlElement htmlElement, string idToFind)
        {
            if (htmlElement.Id != null && htmlElement.Id.Equals(idToFind))
            {
                return htmlElement;
            }

            HtmlElement returnHtmlElement = null;
            foreach (HtmlElement item in htmlElement.Children)
            {
                returnHtmlElement = FindChildWithId(item, idToFind);
            }

            if (returnHtmlElement != null)
            {
                return returnHtmlElement;
            }

            while ((htmlElement = htmlElement.NextSibling) != null)
            {
                returnHtmlElement = FindChildWithId(htmlElement, idToFind);
            }

            if (returnHtmlElement != null)
            {
                return returnHtmlElement;
            }

            return null;
        }

        // private void UpdateXlsOrginal()
        // UpdateEntriesToSaveMemList
        // private static void GetAllEntriesFromExcelFile(string excelFileSavePath, SortedList entries, bool b, object o) {
        // throw new NotImplementedException();
        // }
        private void OpenUrlToolStripMenuItemClick(object sender, EventArgs e)
        {
            var url = InputBoxDialog.InputBox("Skirv url", "Navigera till", webBrowser1.Url.AbsolutePath);

            // var httpText = "http://";
            webBrowser1.Navigate(url); // httpText + url.Replace(httpText, string.Empty));
        }

        private void NavigeraToolStripMenuItemClick(object sender, EventArgs e)
        {
            NavigateToFirstItemInVisibleList();
        }

        private void NavigateToFirstItemInVisibleList()
        {
            if (webBrowser1.Document != null)
            {
                if (webBrowser1.Document.Body != null)
                {
                    // ReSharper disable PossibleNullReferenceException
                    if (webBrowser1.Document.Body.FirstChild.FirstChild.FirstChild.FirstChild.NextSibling.NextSibling != null)
                    {
                        var baseElem =
                            webBrowser1.Document.Body.FirstChild.FirstChild.FirstChild.FirstChild.NextSibling.FirstChild.FirstChild
                            ?? webBrowser1.Document.Body.FirstChild.FirstChild.FirstChild.FirstChild.NextSibling.NextSibling
                                          .FirstChild.FirstChild;

                        // ReSharper restore PossibleNullReferenceException
                        if (baseElem == null)
                        {
                            return;
                        }

                        var logginElem = baseElem.FirstChild;

                        NavigateToAsHref(logginElem);
                    }
                }
            }
        }

        private void NavigateToAsHref(HtmlElement navigateAElem)
        {
            var href = navigateAElem.GetAttribute("href");

            var url = href;
            webBrowser1.Navigate(url);
        }

        // private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        // {

        // }
        private void SetLoginToolStripMenuItemClick(object sender, EventArgs e)
        {
            SetLoginUserEtc();
        }

        private void SetLoginUserEtc()
        {
            if (webBrowser1.Document != null && webBrowser1.Document.Body != null)
            {
                var baselogginElem =
// ReSharper disable PossibleNullReferenceException
                    webBrowser1.Document.Body.FirstChild.FirstChild.FirstChild.FirstChild.NextSibling.NextSibling
// ReSharper restore PossibleNullReferenceException
                               .FirstChild.FirstChild.NextSibling;

                if (baselogginElem != null)
                {
                    var userNameElem = baselogginElem.FirstChild;
// ReSharper disable PossibleNullReferenceException
                    var passElem = baselogginElem.NextSibling.NextSibling.FirstChild;
// ReSharper restore PossibleNullReferenceException

                    // System.Web.UI.HtmlControls.HtmlInputControl;

                    // Set attrib. Login
                    if (userNameElem != null)
                    {
                        userNameElem.SetAttribute("value", string.Empty);
                    }

                    if (passElem != null)
                    {
                        passElem.SetAttribute("value", string.Empty);
                    }
                }

                // (webBrowser1.FindForm() as HtmlElement)
                // .InvokeMember("submit");
                Submit(webBrowser1);
            }
        }

        private void Submit(WebBrowser inWebBrowserControl)
        {
            if (inWebBrowserControl.Document != null)
            {
                var elements = inWebBrowserControl.Document.GetElementsByTagName("Form");

                foreach (HtmlElement currentElement in elements)
                {
                    currentElement.InvokeMember("submit");
                }
            }
        }

        private void NavigateToAllKonto()
        {
            if (webBrowser1.Document != null)
            {
                var oneElem = FindChildWithId(webBrowser1.Document.Body, "item_1").FirstChild;
                if (oneElem == null)
                {
                    throw new ArgumentNullException("one" + "Elem");
                }

                NavigateToAsHref(oneElem);
            }
        }

        private void NavigateToLöneKonto()
        {
            // item_2

            // webBrowser1

            // var Dd = webBrowser1.Document.Body.FirstChild;
            // Dd = FindChildWithId(webBrowser1.Document.Body, "item_2");
            if (webBrowser1.Document == null)
            {
                return;
            }

            var oneElem = FindChildWithId(webBrowser1.Document.Body, "item_2").FirstChild;
            NavigateToAsHref(oneElem);

            // var href = elem.FirstChild.GetAttribute("href");

            // var url = href;
            // webBrowser1.Navigate(url);
        }

        private void NavigateToLöneToolStripMenuItemClick(object sender, EventArgs e)
        {
            NavigateToLöneKonto();
        }

        private void TestNav1ToolStripMenuItemClick(object sender, EventArgs e)
        {
        }

        private void TestBackNavToolStripMenuItemClick(object sender, EventArgs e)
        {
            BrowserGoBack();
        }

        private void BrowserGoBack()
        {
            if (webBrowser1.CanGoBack)
            {
                webBrowser1.GoBack();
            }
        }

        #region Koppling av data till UI

        private void UpdateEntriesToSaveMemList()
        {
            ViewUpdateUi.UpdateListViewFromSortedList(entriesInToBeSaved, kontoEntries);
        }

        #endregion

        #region Events (button clicks etc)

        private void LoadToolStripMenuItem1Click(object sender, EventArgs e)
        {
            // _kontoEntries.Clear();//Töm här kan ge att det inte kommer in något nytt...

            // Helt ny fil ska laddas, töm gammalt
            // Ev. Todo: Rensa UI också, eller lita på att funktionen klarar det iom laddning kan avbrytas etc.
            // Man vill öppna en annan fil som man ska välja och som man ska hämta värden ifrån. Sen spara som den filen man valt. Att börja om med annan fil
            GetAllEntriesFromExcelFile(debugGlobal ? Filerefernces._excelFileSavePath : string.Empty, true);
        }

        private void FileMenuLoadNewFromXlsClick(object sender, EventArgs e)
        {
            // Adding entries here, no clear
            // Man vill lägga till fler värden ifrån en annan fil som man ska välja. Sen spara som den tidigare filen man valt. Att börja om med annan fil
            var somethingLoadeded = GetAllEntriesFromExcelFile(string.Empty, false);
            if (somethingLoadeded)
            {
                CheckAndAddNewItems(); // Lägg till nya i GuiLista
            }
        }

        private void WebBrowser1DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            toolStripStatusLabel1.Text = @"Done";

            // statusStrip1//_browserStatusMessage.Text = "Done";
            if (navigatedNextActionIsStack.Count > 0)
            {
                // SetLoginUserEtc();
                var navigatedNextActionIs = navigatedNextActionIsStack.Pop();
                navigatedNextActionIs.Invoke();
            }
        }

        private void DebugToolStripMenuItemClick(object sender, EventArgs e)
        {
            webBrowser1.Navigate(
                "https://secure.handelsbanken.se"
                + "/bb/seip/servlet/UASipko?appAction=ShowAccountOverview&appName=ipko");

            // const string clickUrl = @"javascript:showOrHideMenu('/shb/Inet/ICentSv.nsf/default/q1525F8FCB98E7B02C12571E60031D5A7?opendocument&frame=0','id4')";

            // var obj = webBrowser1.ObjectForScripting;//.Navigate(clickUrl);
        }

        private void LoadCurrentEntriesToolStripMenuItemClick(object sender, EventArgs e)
        {
            LoadCurrentEntriesFromBrowser();
        }

        private void BtnLoadCurrentEntriesClick(object sender, EventArgs e)
        {
            LoadCurrentEntriesFromBrowser();
        }

        // Todo: change name
        private void BudgeterFormClosing(object sender, FormClosingEventArgs e)
        {
            if (!debugGlobal)
            {
                if (Utilities.WinFormsChecks.SaveCheck(somethingChanged, Save) == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void OpenBankSiteToolStripMenuItemClick(object sender, EventArgs e)
        {
            OpenBankSiteInBrowser();
        }

        private void SaveToolStripMenuItemClick(object sender, EventArgs e)
        {
            Save();
        }

        private void AddNewToMemClick(object sender, EventArgs e)
        {
            AddNewEntriesToUiListsAndMem();
        }

        private void MbClearNewOnesClick(object sender, EventArgs e)
        {
            var userSure = MessageBox.Show(@"Delete new entries", @"Are u sure?", MessageBoxButtons.YesNo);
            if (userSure == DialogResult.Yes)
            {
                ClearNewOnesFnc();
            }
        }

        private void BtnRecheckAutocatClick(object sender, EventArgs e)
        {
            ListViewWithComboBox.UpdateCategoriesWithAutoCatList(newIitemsListEdited.Items);
        }

        #endregion

        #region Funktioner, TODO: ha en del av dessa funktioner i egen fil

        #region Har med UIobjekt i denna klass att göra

        private void AddNewEntriesToUiListsAndMem()
        {
            AddNewEntriesToUiListsAndMem(AoutSaveWhenAddClick());
        }

        private bool AoutSaveWhenAddClick()
        {
            return menuItemAutoSaveCheck.Checked;
        }

        private void OpenBankSiteInBrowser()
        {
            toolStripStatusLabel1.Text = @"Loading";

            // Set spitter so webpage gets more room.
            var halfWindowWidth = (int)(Width * 0.5);
            var hWw = halfWindowWidth;
            var oldSd = splitContainer1.SplitterDistance; // Save pos.
            splitContainer1.SplitterDistance = hWw > oldSd ? hWw : oldSd;
            splitContainer1.ResumeLayout(false);
            PerformLayout();

            // läs in html...
            webBrowser1.Navigate(bankUrl);
        }

        // Rensa minnet och m_newIitemsListOrg
        private void ClearNewOnesFnc()
        {
            newIitemsListOrg.Items.Clear();
            newIitemsListEdited.Items.Clear();
            newKontoEntries.Clear();

            // Rensa även listan som är en kopia av Guilistan för nya ke
        }

        #endregion

        /// <summary>Uppdatera UI för nya entries, gör gisningar av dubbletter, typ av kostnad etc
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1123:DoNotPlaceRegionsWithinElements", Justification = "Reviewed. Suppression is OK here.")]
        private void CheckAndAddNewItems()
        {
            // TODO: flytta denna till annan fil, ev. skicka med fkn som delegat
            // Skriv in nya entries i textrutan
            if (newKontoEntries.Count > 0)
            {
                foreach (DictionaryEntry item in newKontoEntries)
                {
                    var newKe = item.Value as KontoEntry;

                    if (newKe == null)
                    {
                        continue;
                    }

                    var foundDoubleInUList = newIitemsListEdited
                                                 .CheckIfKeyExistsInUiControl(newKe.KeyForThis)
                                                 || newIitemsListEdited.Items.Cast<ListViewItem>()
                                                 .Any(viewItem => ((KontoEntry)viewItem.Tag)
                                                    .KeyForThis.Equals(newKe.KeyForThis));

                    // Om man laddar html-entries 2 gånger i rad, så ska det inte skapas dubletter
                    if (foundDoubleInUList)
                    {
                        continue;
                    }

                    // Lägg till i org
                    if (newIitemsListOrg != null)
                    {
                        ViewUpdateUi.AddToListview(newIitemsListOrg, newKe);
                    }

                    // Kolla om det är en dubblet eller om det är finns ett motsvarade "skyddat belopp"
                    if (kontoEntries.ContainsKey(newKe.KeyForThis))
                    {
                        continue;
                    }

                    // kolla om det är "Skyddat belopp", dubblett o likn. innan man ändrar entryn, med autokat

                    // Slå upp autokategori
                    var lookedUpCat = CategoriesHolder.AllCategories.AutocategorizeType(newKe.Info);
                    if (lookedUpCat != null)
                    {
                        newKe.TypAvKostnad = lookedUpCat;
                    }

                    #region Old
                    // markera de som är dubblet eller skb, och flagga dem för ersättning av de som redan finns i minnet
                    // Gissa om det är en dublett, jmfr på datum, info och kost
                    // if (GuessedDouble(newKE))
                    // {
                    // continue;
                    // } 
                    #endregion

                    // kolla om det är "Skyddat belopp", och se om det finns några gamla som matchar.
                    CheckForSkyddatBeloppMatcherAndGuesseDouble(newKe);

                    // Lägg till i edited
                    ViewUpdateUi.AddToListview(newIitemsListEdited, newKe);
                }
            }

            // Updatera memlistan för att se om någon entry fått ny färg
            UpdateEntriesToSaveMemList();
        }

        /// <summary>Hjäpfunnktion till CheckAndAddNewItems
        /// SweEnglish rules!
        /// Prestandainfo. Loop i loop...
        /// </summary>
        /// <param name="newKe"></param>
        private void CheckForSkyddatBeloppMatcherAndGuesseDouble(KontoEntry newKe) // , bool skipInfo)
        {
            const string Skb = "skyddat belopp";
            const string Pkk = "PREL. KORTKÖP";

            foreach (KontoEntry entry in kontoEntries.Values)
            {
                // Om entryn inte är av typen regulär skippa jämförelser av den.
                // Det kan t.ex. vara mathandling, som delas upp i hemlagat o hygien, eller Periodens köp, som inte ska räknas med som vanlgt och ej heller jämföras
                if (entry.EntryType != KontoEntryType.Regular)
                {
                    continue;
                }

                if (entry.Date == newKe.Date && entry.KostnadEllerInkomst.Equals(newKe.KostnadEllerInkomst))
                {
                    // Ersätt skb
                    if (entry.Info.ToLower() == Skb.ToLower() || entry.Info.ToLower() == Pkk.ToLower())
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

                    return; // En entry ska bara kunna ersätta En annan entry
                }
            }
        }

        private void SetStatusLabelProps()
        {
            toolStripStatusLabel1 = new ToolStripStatusLabel();

            // statusStrip1
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            statusStrip1.Location = new Point(0, 644);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1284, 22);
            statusStrip1.TabIndex = 3;
            statusStrip1.Text = @"statusStrip1ryhjjhj";

            // toolStripStatusLabel1
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(109, 17);
            toolStripStatusLabel1.Text = @"toolStripStatusLabel1";
        }

        // Ger tillgång till status etiketten.
        private void AddNewEntriesToUiListsAndMem(bool autoSave)
        {
            toolStripStatusLabel1.Text = @"Trying to add; " + newKontoEntries.Count + @"items";

            // Hämta nya entries från Ui. (slipper man om man binder ui-kontroller med de som är sparade och ändrade i minnet.)
            var newEntriesFromUi = GetNewEntriesFromUI(newIitemsListEdited);

            // Lägg till/Updatera nya
            var changeInfo = UiHelpersDependant.AddNewEntries(kontoEntries, newEntriesFromUi);
            somethingChanged = CheckIfSomethingWasChanged(somethingChanged, changeInfo.SomethingChanged);

            UpdateEntriesToSaveMemList();

            toolStripStatusLabel1.Text = @"Entries in memory updated. Added entries; " + changeInfo.Added
                                         + @". Replaced entries; " + changeInfo.Replaced;

            if (autoSave)
            {
                Save();
            }
        }

        private bool CheckIfSomethingWasChanged(bool oldSomethingChanged, bool newSomethingChanged)
        {
            if (oldSomethingChanged)
            {
                return true;
            }

            if (newSomethingChanged)
            {
                return true;
            }

            return false;
        }

        private SortedList GetNewEntriesFromUI(ListView mineNewIitemsListEdited)
        {
            // Hämta nya entries från Ui. (slipper man om man binder ui-kontroller med de som är sparade och ändrade i minnet.)
            var newEntriesFromUi = new SortedList();
            foreach (ListViewItem item in mineNewIitemsListEdited.Items)
            {
                var newKe = item.Tag as KontoEntry;
                if (newKe != null && !newEntriesFromUi.ContainsKey(newKe.KeyForThis))
                {
                    newEntriesFromUi.Add(newKe.KeyForThis, newKe);
                }
            }

            return newEntriesFromUi;
        }

        private void Save()
        {
            var statusText = toolStripStatusLabel1.Text;
            var kontoutdragInfoForSave = new KontoutdragInfoForSave
                                         {
                                             excelFileSaveFileName = Filerefernces._excelFileSaveFileName, 
                                             excelFileSavePath = Filerefernces._excelFileSavePath, 
                                             excelFileSavePathWithoutFileName =
                                                 Filerefernces.ExcelFileSavePathWithoutFileName, 
                                             sheetName = SheetName
                                         };

            var saveResult = LoadNSave.Save(kontoutdragInfoForSave, kontoEntries, saldon);

            somethingChanged = saveResult.somethingLoadedOrSaved;

            // somethingChanged = false;//Precis sparat, så här har inget hunnit ändras 
            statusText += "Saving done, saved entries; " + saveResult.skippedOrSaved;

            // Räkna inte överskriften, den skrivs alltid om

            // toolStripStatusLabel1.Text = "Saving done, saved entries; " + (logThis.Count - 1);//Räkna inte överskriften, den skrivs alltid om

            // Fråga om man vill öppna Excel
            if (MessageBox.Show(@"Open budget file (wait a litte while first)?", @"Open file", MessageBoxButtons.YesNo)
                == DialogResult.Yes)
            {
                LoadNSave.LoadExcelFileInExcel(kontoutdragInfoForSave.excelFileSavePath);
            }

            toolStripStatusLabel1.Text = statusText;
        }

        private void LoadCurrentEntriesFromBrowser()
        {
            toolStripStatusLabel1.Text = @"Processing";

            // var oldSaldoAllkort = saldoAllkort;
            // var oldSaldoLöne = saldoLöne;
            var somethingLoadeded = LoadNSave.GetAllVisibleEntriesFromWebBrowser(
                kontoEntries, webBrowser1, newKontoEntries, ref somethingChanged, saldon);

            // Done, meddela på nåt sätt att det är klart, och antal inlästa, i tex. statusbar
            toolStripStatusLabel1.Text = @"Done processing  no new entries fond from html.";

            if (somethingLoadeded)
            {
                CheckAndAddNewItems();
                toolStripStatusLabel1.Text = @"Done processing entries from html. New Entries found; "
                                             + newKontoEntries.Count + @".";
            }

            // if ((!string.IsNullOrEmpty(oldSaldoAllkort) &&
            // !oldSaldoAllkort.ClearSpacesAndReplaceCommas().Equals(saldoAllkort.ClearSpacesAndReplaceCommas()))
            // ||
            // (!string.IsNullOrEmpty(oldSaldoLöne) &&
            // !oldSaldoLöne.ClearSpacesAndReplaceCommas().Equals(saldoLöne.ClearSpacesAndReplaceCommas()))
            // )
            // {
            // toolStripStatusLabel1.Text += " Saldon: Allkort:" + (saldoAllkort ?? string.Empty) + ", Löne:" +
            // (saldoLöne ?? string.Empty) + ", Kredit Ej fakt.:" +
            // (saldoAllkortKreditEjFakturerat ?? string.Empty) +
            // ", Kredit fakt.:" + (saldoAllkortKreditFakturerat ?? string.Empty);
            // }
        }

        #endregion

        #region Test&Debug

        private void DebugbtnClick(object sender, EventArgs e)
        {
            DebugAddoNewList();
        }

        private void DebugAddoNewList()
        {
            var sometheingadded = false;
            for (var i = 0; i < 8; i++)
            {
                if (newKontoEntries == null)
                {
                    continue;
                }

                var testKey = "testkey" + i;
                if (!newKontoEntries.ContainsKey(testKey))
                {
                    var newInfo = "test" + (i % 2 == 0 ? i.ToString(CultureInfo.InvariantCulture) : string.Empty);
                    newKontoEntries.Add(testKey, new KontoEntry { Date = DateTime.Now.AddDays(i), Info = newInfo });
                    sometheingadded = true;
                }
            }

            if (sometheingadded)
            {
                CheckAndAddNewItems(); // Debug
            }
        }

        #endregion

        #region Generic types for Designer

        private KontoEntryListView entriesInToBeSaved;
        private KontoEntryListView xlsOrginalEntries;
        private ListViewWithComboBox newIitemsListEdited;
        private KontoEntryListView newIitemsListOrg;

        #endregion
    }
}