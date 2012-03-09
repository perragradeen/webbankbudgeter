using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using System.Collections;
using System.Threading;
using System.IO;
using Budgetterarn.Operations;
using Budgetterarn.Application_Settings_and_constants;
using Budgetterarn.InternalUtilities;

namespace Budgetterarn
{
    //Todos se Data/Todos.txt

    /// <summary>
    /// Xls-fil som läses in förutsätts ha Kontoutdrag_officiella som ark med 6 celler ejämte varann enligt ex. nedan:
    /// 2007-09-05	SkyDDat belopp	-120,00	100 991,24	127,02	telefonsamtal
    /// </summary>
    public partial class Budgeter : Form
    {
        public const string VersionNumber = "1.0.1.9";//IF CHANGED VERSION. DOCUMENT CHANGES!!!AND COMMIT See Changes summary below.
        //1.0.1.9 Gjort anpassningar till Excel engelsk version map datumformat etc. Nu loggas celler som objekt istället för strängar. Div omstrukturering i testProjekt och tillagda projekt.
        //1.0.1.8 För handelsbanken mobil. Autonavigera med inloggning etc. Så allt nytt laddas in automatiskt. + Snabbkanapp Ctrl+L för att ladda entries.
        //1.0.1.7 Kan nu ladda entries från Handelsbanken mobilsida. Som har enklare inloggning. +Buggfix med sortering av nya entries.
        //1.0.1.6 Enklare sparning utan prompt. Autosave som val när man lagt till nya entries. Även ändrat för funktioner som sparar och laddar. Fixat så det går att anv designervyn i VS.
        //1.0.1.5 Fixed new saldos for SHB. Double-mbox clearified, better handling of uniques and double-entries.
        //1.0.1.4 Fixed autocat set, so it is less unneccesary popups to user. Started to Add functionality for Swedbank.
        //1.0.1.3 Addad exception catch att Exclel close. Now user selects if autocat shold overwrite existing choices. Added info about how to set several cats at the same time. Added sorting on listviews. Columns from excel should now be correct in listviews.
        //1.0.1.2 Fixed so tag is also set when just selecting cat on new entries from web, also a halfsmart (not full proof, bu probably never gonna err...) doublechecker added.
        //1.0.1.1 Changed way Version number is set
        //1.0.1.0 PopupComboboxOfCaytegories had a bugg with wrong colwidth added when checking postion, only noticable if not all columns have same length. Nicer set autocat and popup. användaren kan sätta autokat.
        //1.0.0.1 Nothing new yet, Later singleclick in newlist etc.
        //1.0.0.0 Everything before, see Svn. Even Added mulitiselect etc.

        #region Members
        //Settings (mostly debug)
        readonly bool debugGlobal = false;//For useSaveCheck

        static string bankUrl = "http://www.handelsbanken.se/247igaa.nsf/default/LoginBankId?opendocument&redir=privelegsv";
        //static string BankUrlHandelsBanken = "http://www.handelsbanken.se/247igaa.nsf/default/LoginBankId?opendocument&redir=privelegsv";

        readonly SortedList _kontoEntries = new SortedList(new DescendingComparer());//Ev. byta denna mot en klass med innehåll och nyckel, för att behålla orginalordningen på posterna. Sorteras med nyaste först
        readonly SortedList _newKontoEntries = new SortedList();

        //private static string _excelFileSavePathWithoutFileName;// = @"C:\stuff\budget\";//Hårdkodad sökväg utan dialog
        //private static string _excelFileSaveFileName;// = @"Test LG Budget.xls";//Pelles Budget.xls";//Hårdkodad sökväg utan dialog
        //private string _excelFileSavePath;// = _excelFileSavePathWithoutFileName + _excelFileSaveFileName;//Hårdkodad sökväg utan dialog
        ////string _excelFileSavePath = @"C:\Documents and Settings\hu\My Documents\CoNy kolumn of Test Pelles kontoutdrag.xls";//Hårdkodad sökväg utan dialog

        //const string m_s_newEntriesXlsDebug = @"C:\Documents and Settings\hu\My Documents\NYA entries test Pelles kontoutdrag.xls";

        string _sheetName = "Kontoutdrag_officiella";// "Kontoutdrag f.o.m. 0709 bot.up.";
        public static string CategoryPath = @"Data\Categories.xml";
        bool _somethingChanged = false;

        //To do, sätt alla medlemmar i en egen klass etc.
        string saldoLöne = "";
        string saldoAllkort = "";
        string saldoAllkortKreditEjFakturerat = "";
        string saldoAllkortKreditFakturerat = "";

        Dictionary<string, string> saldon = new Dictionary<string, string>();//Key = description, Value= amount

        //Tread handling
        Thread _mainThread;
        Thread _workerThread;

        //Excel.Application _excelApp = new Excel.Application();//Denna ligger här för att kunna släppa objektet i delegat nedan (Application_WorkbookDeactivate)

        //Navigering i browser
        public delegate void DoneNavigationAction();

        private Stack<DoneNavigationAction> navigatedNextActionIsSatck = new Stack<DoneNavigationAction>();
        #endregion
        public Budgeter()//Konstruktor
        {
            //Todo senast:
            #region Inits

            try
            {
                //Get file names from settings file
                CategoryPath = GeneralSettings.GetStringSetting("CategoryPath");
                bankUrl = GeneralSettings.GetTextfileStringSetting("BankUrl");
                #region Old
                //if (!string.IsNullOrEmpty(bankUrl)) {
                //    if (bankUrl == @"http://www.handelsbanken.se") {
                //        bankUrl = BankUrlHandelsBanken;
                //    }
                //}

                #endregion

                //var t = new CategoriesHolder();
                //Ladda kategorier som man har till att flagga olika kontohändelser
                CategoriesHolder.DeserializeObject(CategoryPath);

                //Initiera UI-objekt
                InitializeComponent();
                InitSpecialGenericUIElements();
                SetStatusLabelProps();

                //Sätt nuvarande tråd som main
                _mainThread = Thread.CurrentThread;

                //läs in xls...
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
                else debugGlobal = false;

                if (debug)
                {
                    //TODO: GetAllEntriesFromExcelFile(m_s_newEntriesXlsDebug, _newKontoEntries, false, null);
                    //CheckAndAddNewItems();//Debug: Lägg till nya i GuiLista

                    debugbtn.Visible = true;
                    DebugAddoNewList();
                }
                #endregion
                else
                {
                    //Öpnna banksidan direkt
                    OpenBankSiteInBrowser();

                    AutoNavigateToKontonEtc();

                    //Thread.Sleep(100);
                    //webBrowser1.Navigate(
                    //        "https://secure.handelsbanken.se" +
                    //        "/bb/seip/servlet/UASipko?appAction=ShowAccountOverview&appName=ipko"
                    //    );
                }

                #region Old
                //string sheetName = "Kontoutdrag_officiella";// "Kontoutdrag f.o.m. 0709 bot.up.";
                //2009	3	2009-03-26	 	JohaMsMatBio	 	-10	 	50 951,93	spara till russel övrigt
                //string[] temp1 = new string[] { "2009", "3",    "2009-03-26", "JohaMsMatBio", "-10", "50 951,93", "spara till russel övrigt" };
                //2009	3	2009-03-25	 	LÖN	 	17 969,00	 	50 961,93	+	

                //Utilities.ExcelRowEntry newE = new Utilities.ExcelRowEntry(0, temp1);

                //if (!_kontoUtdragXLS.ContainsKey(mergeStringArrayToString(temp1)))
                //    _kontoUtdragXLS.Add(mergeStringArrayToString(temp1), newE);
                //webBrowser1.Url = "";

                //läs in html...
                //OpenBankSiteInBrowser();//Gör ej som default.

                #endregion

                //Sätt versionsnummer i titel
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
            //var ifHb = ProgramSettings.BankType.Equals(BankType.Handelsbanken);

            #endregion

        }

        private void AutoNavigateToKontonEtc()
        {

            if (!AutoLoadEtce())
            {
                return;
            }

            //Korttransaktioner
            navigatedNextActionIsSatck.Push(loadCurrentEntriesFromBrowser);
            //navigatedNextActionIsSatck.Push(NavigateToFirstItemInVisibleList);

            ////Allkonto
            navigatedNextActionIsSatck.Push(LoadEntriesAndGoToFirst);
            navigatedNextActionIsSatck.Push(NavigateToAllKonto);
            //navigatedNextActionIsSatck.Push(BrowserGoBack);

            //Löne
            navigatedNextActionIsSatck.Push(LoadEntriesAndGoBack);
            navigatedNextActionIsSatck.Push(NavigateToLöneKonto);

            //Inlogg
            navigatedNextActionIsSatck.Push(NavigateToFirstItemInVisibleList);
            navigatedNextActionIsSatck.Push(SetLoginUserEtc);
            navigatedNextActionIsSatck.Push(NavigateToFirstItemInVisibleList);
        }

        private bool AutoLoadEtce()
        {
            var s = GeneralSettings.GetStringSetting("AutonavigateEtc");

            var b = false;
            if (bool.TryParse(s, out b))
            {
                return b;
            }

            return false;
        }

        private void LoadEntriesAndGoBack()
        {
            loadCurrentEntriesFromBrowser();
            BrowserGoBack();
        }

        private void LoadEntriesAndGoToFirst()
        {
            loadCurrentEntriesFromBrowser();
            NavigateToFirstItemInVisibleList();
        }

        #region Generic types for Designer
        private ListViewWithComboBox m_newIitemsListEdited;
        private KontoEntryListView m_newIitemsListOrg;
        private KontoEntryListView m_XlsOrginalEntries;
        private KontoEntryListView m_EntriesInToBeSaved;

        #endregion


        private void InitSpecialGenericUIElements()
        {
            this.m_newIitemsListEdited = new Budgetterarn.ListViewWithComboBox();
            this.m_newIitemsListOrg = new KontoEntryListView();
            this.m_EntriesInToBeSaved = new Budgetterarn.KontoEntryListView();
            this.m_XlsOrginalEntries = new Budgetterarn.KontoEntryListView();

            // 
            // tp_NewItemsEdited
            // 
            this.tp_NewItemsEdited.Controls.Add(this.m_newIitemsListEdited);
            this.tp_NewItemsEdited.Location = new System.Drawing.Point(4, 22);
            this.tp_NewItemsEdited.Name = "tp_NewItemsEdited";
            this.tp_NewItemsEdited.Padding = new System.Windows.Forms.Padding(3);
            this.tp_NewItemsEdited.Size = new System.Drawing.Size(1161, 551);
            this.tp_NewItemsEdited.TabIndex = 0;
            this.tp_NewItemsEdited.Text = "New items edited";
            this.tp_NewItemsEdited.UseVisualStyleBackColor = true;
            // 
            // m_newIitemsListEdited
            // 
            this.m_newIitemsListEdited.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.m_newIitemsListEdited.FullRowSelect = true;
            this.m_newIitemsListEdited.GridLines = true;
            this.m_newIitemsListEdited.Location = new System.Drawing.Point(3, 3);
            this.m_newIitemsListEdited.Name = "m_newIitemsListEdited";
            this.m_newIitemsListEdited.Size = new System.Drawing.Size(855, 545);
            this.m_newIitemsListEdited.TabIndex = 0;
            this.m_newIitemsListEdited.UseCompatibleStateImageBehavior = false;
            this.m_newIitemsListEdited.View = System.Windows.Forms.View.Details;
            // 
            // tp_NewItemsOrg
            // 
            this.tp_NewItemsOrg.Controls.Add(this.m_newIitemsListOrg);
            this.tp_NewItemsOrg.Location = new System.Drawing.Point(4, 22);
            this.tp_NewItemsOrg.Name = "tp_NewItemsOrg";
            this.tp_NewItemsOrg.Padding = new System.Windows.Forms.Padding(3);
            this.tp_NewItemsOrg.Size = new System.Drawing.Size(1161, 551);
            this.tp_NewItemsOrg.TabIndex = 1;
            this.tp_NewItemsOrg.Text = "New items orginal";
            this.tp_NewItemsOrg.UseVisualStyleBackColor = true;

            // 
            // m_newIitemsListOrg
            // 
            this.m_newIitemsListOrg.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.c_Date,
            this.c_Info,
            this.c_KostnadEllerInkomst,
            this.c_SaldoOrginal,
            this.c_AckumuleratSaldo,
            this.c_TypAvKostnad});
            this.m_newIitemsListOrg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_newIitemsListOrg.FullRowSelect = true;
            this.m_newIitemsListOrg.GridLines = true;
            this.m_newIitemsListOrg.Location = new System.Drawing.Point(3, 3);
            this.m_newIitemsListOrg.Name = "m_newIitemsListOrg";
            this.m_newIitemsListOrg.Size = new System.Drawing.Size(1155, 545);
            this.m_newIitemsListOrg.TabIndex = 0;
            this.m_newIitemsListOrg.UseCompatibleStateImageBehavior = false;
            this.m_newIitemsListOrg.View = System.Windows.Forms.View.Details;

            this.m_inMemoryList.Controls.Add(this.m_EntriesInToBeSaved);

            // 
            // m_EntriesInToBeSaved
            // 
            this.m_EntriesInToBeSaved.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_EntriesInToBeSaved.FullRowSelect = true;
            this.m_EntriesInToBeSaved.GridLines = true;
            this.m_EntriesInToBeSaved.Location = new System.Drawing.Point(3, 3);
            this.m_EntriesInToBeSaved.Name = "m_EntriesInToBeSaved";
            this.m_EntriesInToBeSaved.Size = new System.Drawing.Size(288, 577);
            this.m_EntriesInToBeSaved.TabIndex = 0;
            this.m_EntriesInToBeSaved.UseCompatibleStateImageBehavior = false;
            this.m_EntriesInToBeSaved.View = System.Windows.Forms.View.Details;
            // 
            // m_originalXls
            // 
            this.m_originalXls.Controls.Add(this.m_XlsOrginalEntries);
            this.m_originalXls.Location = new System.Drawing.Point(4, 22);
            this.m_originalXls.Name = "m_originalXls";
            this.m_originalXls.Padding = new System.Windows.Forms.Padding(3);
            this.m_originalXls.Size = new System.Drawing.Size(294, 583);
            this.m_originalXls.TabIndex = 0;
            this.m_originalXls.Text = "Xls Original";
            this.m_originalXls.UseVisualStyleBackColor = true;
            // 
            // m_XlsOrginalEntries
            // 
            this.m_XlsOrginalEntries.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_XlsOrginalEntries.FullRowSelect = true;
            this.m_XlsOrginalEntries.GridLines = true;
            this.m_XlsOrginalEntries.Location = new System.Drawing.Point(3, 3);
            this.m_XlsOrginalEntries.Name = "m_XlsOrginalEntries";
            this.m_XlsOrginalEntries.Size = new System.Drawing.Size(288, 577);
            this.m_XlsOrginalEntries.TabIndex = 0;
            this.m_XlsOrginalEntries.UseCompatibleStateImageBehavior = false;
            this.m_XlsOrginalEntries.View = System.Windows.Forms.View.Details;


            this.m_EntriesInToBeSaved.ListViewItemSorter = new ListViewColumnSorter();
            this.m_XlsOrginalEntries.ListViewItemSorter = new ListViewColumnSorter();
            this.m_newIitemsListEdited.ListViewItemSorter = new ListViewColumnSorter();
            this.m_newIitemsListOrg.ListViewItemSorter = new ListViewColumnSorter();

        }

        /// <summary>
        /// Titeltexten för fönstret
        /// </summary>
        public override sealed string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        /// <summary>
        /// Uses members in this class
        /// </summary>
        /// <param name="excelFileSavePath"></param>
        /// <param name="clearContentBeforeReadingNewFile"></param>
        private bool GetAllEntriesFromExcelFile(string excelFileSavePath, bool clearContentBeforeReadingNewFile)
        {
            var statusText = toolStripStatusLabel1.Text = "Nothing loaded.";

            var changed_excelFileSavePath = Filerefernces._excelFileSavePath;
            var loadedSomething = LoadNSave.GetAllEntriesFromExcelFile(excelFileSavePath, _kontoEntries, _mainThread,
                                                 ref statusText, out _workerThread, ref changed_excelFileSavePath
                                                 , ref saldoLöne, ref saldoAllkort, ref saldoAllkortKreditEjFakturerat
                                                 , ref saldoAllkortKreditFakturerat
                                                 , _sheetName, ref _somethingChanged
                                                 , Filerefernces.ExcelFileSavePathWithoutFileName//Todo: gör en funktion för denna eller refa med en filnamns och sökvägsklass....
                                                 , Filerefernces._excelFileSaveFileName
                                                 , clearContentBeforeReadingNewFile
                                                 , saldon
                                                 );

            //Ev. har pathen ändrats.
            if (excelFileSavePath == string.Empty)
            {
                //Om man lagt till nya rader från annan fil, så spara i den gamla.
            }
            else
            {
                //Har man däremot laddat in nya så ska den sökvägen gälla för sparningar
                Filerefernces._excelFileSavePath = changed_excelFileSavePath;
                //Todo: sätt denna tidigare så att LoadNsave bara gör vad den ska utan UI etc
            }

            toolStripStatusLabel1.Text = statusText + " Saldon: Allkort:" + saldoAllkort + ", Löne:" + saldoLöne + ", Kredit Ej fakt.:" + saldoAllkortKreditEjFakturerat + ", Kredit fakt.:" + saldoAllkortKreditFakturerat;

            //If nothing loaded return
            if (!loadedSomething)
            {
                return false;
            }

            //Lägg till orginalraden, gör i UI-hanterare
            //Lägg in det som är satt att sparas till minnet (viasa alla _kontoEntries i listview). Även uppdatera färg på text.
            ViewUpdateUi.UpdateListViewFromSortedList(m_XlsOrginalEntries, _kontoEntries);
            ViewUpdateUi.UpdateListViewFromSortedList(m_EntriesInToBeSaved, _kontoEntries);

            return true;
        }

        //private void UpdateXlsOrginal()
        //UpdateEntriesToSaveMemList
        //private static void GetAllEntriesFromExcelFile(string excelFileSavePath, SortedList entries, bool b, object o) {
        //    throw new NotImplementedException();
        //}


        #region Events (button clicks etc)
        private void loadToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //_kontoEntries.Clear();//Töm här kan ge att det inte kommer in något nytt...

            //Helt ny fil ska laddas, töm gammalt
            //Ev. Todo: Rensa UI också, eller lita på att funktionen klarar det iom laddning kan avbrytas etc.
            //Man vill öppna en annan fil som man ska välja och som man ska hämta värden ifrån. Sen spara som den filen man valt. Att börja om med annan fil
            if (debugGlobal) GetAllEntriesFromExcelFile(Filerefernces._excelFileSavePath, true);
            else GetAllEntriesFromExcelFile(string.Empty, true);//_excelFileSavePath
        }

        private void fileMenuLoadNewFromXls_Click(object sender, EventArgs e)
        {
            //Adding entries here, no clear
            //Man vill lägga till fler värden ifrån en annan fil som man ska välja. Sen spara som den tidigare filen man valt. Att börja om med annan fil
            var somethingLoadeded = GetAllEntriesFromExcelFile(string.Empty, false);
            if (somethingLoadeded)
                CheckAndAddNewItems();//Lägg till nya i GuiLista
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            toolStripStatusLabel1.Text = "Done";
            //statusStrip1//_browserStatusMessage.Text = "Done";

            if (navigatedNextActionIsSatck.Count > 0)
            {
                //SetLoginUserEtc();

                var navigatedNextActionIs = navigatedNextActionIsSatck.Pop();
                navigatedNextActionIs.Invoke();
                navigatedNextActionIs = null;
            }
        }

        private void debugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            webBrowser1.Navigate
                (
                "https://secure.handelsbanken.se" +
                "/bb/seip/servlet/UASipko?appAction=ShowAccountOverview&appName=ipko"
                );

            //const string clickUrl = @"javascript:showOrHideMenu('/shb/Inet/ICentSv.nsf/default/q1525F8FCB98E7B02C12571E60031D5A7?opendocument&frame=0','id4')";

            //var obj = webBrowser1.ObjectForScripting;//.Navigate(clickUrl);

        }

        private void loadCurrentEntriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadCurrentEntriesFromBrowser();
        }

        private void btnLoadCurrentEntries_Click(object sender, EventArgs e)
        {
            loadCurrentEntriesFromBrowser();
        }

        //Todo: change name
        private void Budgeter_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!debugGlobal)
                if (Utilities.WinFormsChecks.SaveCheck(_somethingChanged, Save) == DialogResult.Cancel)
                    e.Cancel = true;
        }

        private void openBankSiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenBankSiteInBrowser();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void AddNewToMem_Click(object sender, EventArgs e)
        {
            AddNewEntriesToUIListsAndMem();
        }

        private void m_b_ClearNewOnes_Click(object sender, EventArgs e)
        {
            DialogResult userSure = MessageBox.Show("Delete new entries", "Are u sure?", MessageBoxButtons.YesNo);
            if (userSure == DialogResult.Yes)
            {
                ClearNewOnesFnc();
            }
        }

        private void btn_RecheckAutocat_Click(object sender, EventArgs e)
        {
            ListViewWithComboBox.UpdateCategoriesWithAutoCatList(m_newIitemsListEdited.Items);
        }

        #endregion

        #region Extra funktioner, DONE; Flytta denna till .dll eller likn.
        #endregion

        #region Funktioner, TODO: ha en del av dessa funktioner i egen fil
        #region Har med UIobjekt i denna klass att göra
        private void AddNewEntriesToUIListsAndMem()
        {
            AddNewEntriesToUIListsAndMem(AoutSaveWhenAddClick());
        }

        private bool AoutSaveWhenAddClick()
        {
            return menuItemAutoSaveCheck.Checked;
        }
        
        void OpenBankSiteInBrowser()
        {
            toolStripStatusLabel1.Text = "Loading";

            #region Set spitter so webpage gets more room.
            //Set spitter so webpage gets more room.
            var halfWindowWidth = (int)(Width * 0.5);
            var hWw = halfWindowWidth;
            var oldSd = splitContainer1.SplitterDistance;//Save pos.
            splitContainer1.SplitterDistance = hWw > oldSd ? hWw : oldSd;
            splitContainer1.ResumeLayout(false);
            PerformLayout();

            #endregion
            //läs in html...
            webBrowser1.Navigate(bankUrl);
        }


        //Rensa minnet och m_newIitemsListOrg
        void ClearNewOnesFnc()
        {
            m_newIitemsListOrg.Items.Clear();
            m_newIitemsListEdited.Items.Clear();
            _newKontoEntries.Clear();

            //Rensa även listan som är en kopia av Guilistan för nya ke
        }
        #endregion

        /// <summary>Uppdatera UI för nya entries, gör gisningar av dubbletter, typ av kostnad etc
        /// </summary>
        private void CheckAndAddNewItems()
        {//TODO: flytta denna till annan fil, ev. skicka med fkn som delegat
            //Skriv in nya entries i textrutan
            if (_newKontoEntries.Count > 0)
            {
                foreach (DictionaryEntry item in _newKontoEntries)
                {
                    var newKe = item.Value as KontoEntry;

                    if (newKe == null)
                    {
                        continue;
                    }

                    var foundDoubleInUList = CheckIfKeyExistsInUiControl(newKe.KeyForThis, m_newIitemsListEdited);
                    //Om man laddar html-entries 2 gånger i rad, så ska det inte skapas dubletter

                    foreach (ListViewItem viewItem in m_newIitemsListEdited.Items)
                    {
                        if (!((KontoEntry)viewItem.Tag).KeyForThis.Equals(newKe.KeyForThis))
                        {
                            continue;
                        }

                        foundDoubleInUList = true;
                        break;
                    }

                    if (foundDoubleInUList)
                        continue;

                    #region Old
                    //Om man laddar html-entries 2 gånger i rad, så ska det inte skapas dubletter
                    //if (_kontoEntries.ContainsKey(newKe.KeyForThis)) {
                    //    continue;
                    //}
                    //else
                    //    _kontoEntries.Add(newKE.KeyForThis, newKE); 
                    #endregion

                    //Lägg till i org
                    if (m_newIitemsListOrg != null)
                    {
                        ViewUpdateUi.AddToListview(m_newIitemsListOrg, newKe);
                    }

                    //Kolla om det är en dubblet eller om det är finns ett motsvarade "skyddat belopp"
                    if (_kontoEntries.ContainsKey(newKe.KeyForThis))
                    {
                        continue;
                    }

                    //kolla om det är "Skyddat belopp", dubblett o likn. innan man ändrar entryn, med autokat

                    //Slå upp autokategori
                    var lookedUpCat = CategoriesHolder.AllCategories.AutocategorizeType(newKe.Info);
                    if (lookedUpCat != null)
                    {
                        newKe.TypAvKostnad = lookedUpCat;
                    }
                    #region Old
                    //markera de som är dubblet eller skb, och flagga dem för ersättning av de som redan finns i minnet
                    //Gissa om det är en dublett, jmfr på datum, info och kost
                    //if (GuessedDouble(newKE))
                    //{
                    //    continue;
                    //} 
                    #endregion

                    //kolla om det är "Skyddat belopp", och se om det finns några gamla som matchar.
                    CheckForSkyddatBeloppMatcherAndGuesseDouble(newKe);

                    //Lägg till i edited
                    ViewUpdateUi.AddToListview(m_newIitemsListEdited, newKe);
                }
            }

            //Updatera memlistan för att se om någon entry fått ny färg
            UpdateEntriesToSaveMemList();
        }

        private static bool CheckIfKeyExistsInUiControl(string keyToSearchFor, ListView listToSearchIn)
        {
            return GetEntryFromUiControl(keyToSearchFor, listToSearchIn) != null;
        }

        private static ListViewItem GetEntryFromUiControl(string keyToSearchFor, ListView listToSearchIn)
        {
            foreach (ListViewItem viewItem in listToSearchIn.Items)
            {
                if (((KontoEntry)viewItem.Tag).KeyForThis.Equals(keyToSearchFor))
                {
                    return viewItem;
                }
            }

            return null;
        }

        /// <summary>Hjäpfunnktion till CheckAndAddNewItems
        /// SweEnglish rules!
        /// </summary>
        /// <param name="newKe"></param>
        private void CheckForSkyddatBeloppMatcherAndGuesseDouble(KontoEntry newKe)//, bool skipInfo)
        {
            const string skb = "skyddat belopp";
            const string pkk = "PREL. KORTKÖP";

            foreach (KontoEntry entry in _kontoEntries.Values)
            {
                //Om entryn inte är av typen regulär skippa jämförelser av den.
                //Det kan t.ex. vara mathandling, som delas upp i hemlagat o hygien, eller Periodens köp, som inte ska räknas med som vanlgt och ej heller jämföras
                if (entry.EntryType != KontoEntryType.Regular)
                {
                    continue;
                }

                if (entry.Date == newKe.Date
                    && entry.KostnadEllerInkomst == newKe.KostnadEllerInkomst
                    )
                {
                    if (entry.Info.ToLower() == skb.ToLower()
                        || entry.Info.ToLower() == pkk.ToLower()
                        )//Ersätt skb
                    {
                        newKe.FontFrontColor = entry.FontFrontColor = newKe.FontFrontColor = Color.DeepSkyBlue;

                        //Ta de gamla saldot
                        newKe.SaldoOrginal = entry.SaldoOrginal;
                        newKe.AckumuleratSaldo = entry.AckumuleratSaldo;

                        //Vid senare ersättande, så kommer typen vara den nya, eftersom det är den som autokattats, och då stämmer det nog bättre än den som kan vara skyddat belopp. Anv. kan ju även alltid sätta själv innan sparning
                        //Är inget autokattat, så ta den gamla, man har säkert gissat rätt
                        if (string.IsNullOrEmpty(newKe.TypAvKostnad))
                            newKe.TypAvKostnad = entry.TypAvKostnad;

                        newKe.ReplaceThisKey = entry.KeyForThis;
                    }
                    else //Det är kanske en dubblett
                    {
                        newKe.FontFrontColor = entry.FontFrontColor = newKe.FontFrontColor = Color.Red;
                        newKe.ThisIsDoubleDoNotAdd = true;
                    }


                    return;//En entry ska bara kunna ersätta En annan entry
                }
            }
        }

        private static ToolStripStatusLabel toolStripStatusLabel1;

        void SetStatusLabelProps()
        {
            toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 644);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1284, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1ryhjjhj";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new System.Drawing.Size(109, 17);
            toolStripStatusLabel1.Text = "toolStripStatusLabel1";
        }

        //Ger tillgång till status etiketten.
        public static string StatusLabelText
        {
            get { return toolStripStatusLabel1.Text; }
            set { toolStripStatusLabel1.Text = value; }
        }


        private void AddNewEntriesToUIListsAndMem(bool autoSave)
        {
            toolStripStatusLabel1.Text = "Trying to add; " + _newKontoEntries.Count + "items";


            //Hämta nya entries från Ui. (slipper man om man binder ui-kontroller med de som är sparade och ändrade i minnet.)
            var newEntriesFromUi = GetNewEntriesFromUI(m_newIitemsListEdited);

            //Lägg till/Updatera nya
            var changeInfo = AddNewEntries(_kontoEntries, newEntriesFromUi);
            _somethingChanged = CheckIfSomethingWasChanged(_somethingChanged, changeInfo.SomethingChanged);

            UpdateEntriesToSaveMemList();

            toolStripStatusLabel1.Text = "Entries in memory updated. Added entries; " + changeInfo.Added
                + ". Replaced entries; " + changeInfo.Replaced;

            if (autoSave)
            {
                Save();
            }
        }

        private bool CheckIfSomethingWasChanged(bool oldSomethingChanged, bool newSomethingChanged)
        {
            if (oldSomethingChanged)
            {
                return oldSomethingChanged;
            }

            if (newSomethingChanged)
            {
                return newSomethingChanged;
            }
            else
                return oldSomethingChanged;
        }

        internal class AddedAndReplacedEntriesCounter
        {
            public int Added { get; set; }
            public int Replaced { get; set; }

            public bool SomethingChanged { get; set; }
        }

        private static AddedAndReplacedEntriesCounter AddNewEntries(SortedList oldKontoEntries, SortedList newEntries)
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

        private SortedList GetNewEntriesFromUI(ListViewWithComboBox m_newIitemsListEdited)
        {
            //Hämta nya entries från Ui. (slipper man om man binder ui-kontroller med de som är sparade och ändrade i minnet.)
            var newEntriesFromUi = new SortedList();
            foreach (ListViewItem item in m_newIitemsListEdited.Items)
            {
                var newKe = item.Tag as KontoEntry;
                if (newKe != null && !newEntriesFromUi.ContainsKey(newKe.KeyForThis))
                    newEntriesFromUi.Add(newKe.KeyForThis, newKe);
            }

            return newEntriesFromUi;
        }

        private void Save()
        {
            var statusText = toolStripStatusLabel1.Text;
            LoadNSave.Save(_mainThread, ref statusText, out _workerThread, _kontoEntries, Filerefernces._excelFileSavePath, saldoLöne,
                           saldoAllkort, saldoAllkortKreditEjFakturerat,
                           saldoAllkortKreditFakturerat, _sheetName, ref _somethingChanged
                           , Filerefernces.ExcelFileSavePathWithoutFileName, Filerefernces._excelFileSaveFileName, saldon);
            toolStripStatusLabel1.Text = statusText;
        }


        private void loadCurrentEntriesFromBrowser()
        {
            toolStripStatusLabel1.Text = "Processing";
            var oldSaldoAllkort = saldoAllkort;
            var oldSaldoLöne = saldoLöne;
            var somethingLoadeded = LoadNSave.GetAllVisibleEntriesFromWebBrowser(_kontoEntries, webBrowser1, ref saldoAllkortKreditEjFakturerat,
                                                         ref saldoAllkortKreditFakturerat, _newKontoEntries, ref saldoLöne,
                                                         ref saldoAllkort, ref _somethingChanged, saldon);

            //Done, meddela på nåt sätt att det är klart, och antal inlästa, i tex. statusbar
            toolStripStatusLabel1.Text = "Done processing  no new entries fond from html.";

            if (somethingLoadeded)
            {
                CheckAndAddNewItems();
                toolStripStatusLabel1.Text = "Done processing entries from html. New Entries found; " +
                                             _newKontoEntries.Count + ".";
            }

            if ((!string.IsNullOrEmpty(oldSaldoAllkort) &&
                    !ClearSpacesAndReplaceCommas(oldSaldoAllkort).Equals(ClearSpacesAndReplaceCommas(saldoAllkort)))
                    ||
                 (!string.IsNullOrEmpty(oldSaldoLöne) &&
                    !ClearSpacesAndReplaceCommas(oldSaldoLöne).Equals(ClearSpacesAndReplaceCommas(saldoLöne)))
                )
            {
                toolStripStatusLabel1.Text += " Saldon: Allkort:" + (saldoAllkort ?? string.Empty) + ", Löne:" +
                                              (saldoLöne ?? string.Empty) + ", Kredit Ej fakt.:" +
                                              (saldoAllkortKreditEjFakturerat ?? string.Empty) +
                                              ", Kredit fakt.:" + (saldoAllkortKreditFakturerat ?? string.Empty);
            }
        }

        static string ClearSpacesAndReplaceCommas(string inString)
        {
            return !string.IsNullOrEmpty(inString) ? inString.Replace(" ", string.Empty).Replace(".", ",") : inString;
        }

        #endregion

        #region Interna klasser, nu bara sortedlist descendingklass
        //Tagit från nätet: http://www.codeproject.com/KB/cs/Descending_Sorted_List.aspx?fid=1353560&df=90&mpp=25&noise=3&sort=Position&view=Quick&select=2570977#xx2570977xx
        internal class DescendingComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                try
                {
                    if (x.GetType() == typeof(string))
                    {
                        return x.ToString().CompareTo(y.ToString()) * -1;
                    }
                    else
                        return System.Convert.ToInt32(x).CompareTo
                            (System.Convert.ToInt32(y)) * -1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("No real exception in DescendingComparer.Compare(obj x, obj y): " + ex.Message);
                    return x.ToString().CompareTo(y.ToString());
                }
            }
        }
        #endregion

        #region Test&Debug
        private void debugbtn_Click(object sender, EventArgs e)
        {
            DebugAddoNewList();
        }


        void DebugAddoNewList()
        {
            var sometheingadded = false;
            for (var i = 0; i < 8; i++)
            {
                if (_newKontoEntries == null)
                {
                    continue;
                }


                var testKey = "testkey" + i;
                if (!_newKontoEntries.ContainsKey(testKey))
                {
                    var newInfo = "test" + (i % 2 == 0 ? i.ToString() : string.Empty);
                    _newKontoEntries.Add(testKey, new KontoEntry { Date = DateTime.Now.AddDays(i), Info = newInfo });
                    sometheingadded = true;
                }
            }
            if (sometheingadded)
                CheckAndAddNewItems();//Debug
        }

        #endregion

        private void openUrlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var url = InputBoxDialog.InputBox("Skirv url", "Navigera till", webBrowser1.Url.AbsolutePath);
            //var httpText = "http://";
            webBrowser1.Navigate(url);// httpText + url.Replace(httpText, string.Empty));
        }

        #region Koppling av data till UI
        private void UpdateEntriesToSaveMemList()
        {
            ViewUpdateUi.UpdateListViewFromSortedList(m_EntriesInToBeSaved, _kontoEntries);
        }
        #endregion

        private void navigeraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NavigateToFirstItemInVisibleList();
        }

        private void NavigateToFirstItemInVisibleList()
        {
            var baseElem = webBrowser1.Document.Body.FirstChild.FirstChild.FirstChild
                .FirstChild.NextSibling.FirstChild.FirstChild;
            if (baseElem == null)
            {
                baseElem = webBrowser1.Document.Body.FirstChild.FirstChild.FirstChild.FirstChild
                    .NextSibling.NextSibling.FirstChild.FirstChild;//.FirstChild;

            }

            if (baseElem == null)
            {
                return;
            }

            var logginElem = baseElem.FirstChild;

            NavigateToAsHref(logginElem);
        }

        private void NavigateToAsHref(HtmlElement navigateAElem)
        {
            var href = navigateAElem.GetAttribute("href");

            var url = href;
            webBrowser1.Navigate(url);
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            

        }

        private void setLoginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetLoginUserEtc();
        }

        private void SetLoginUserEtc()
        {
            if (webBrowser1.Document.Body != null)
            {
                var baselogginElem = webBrowser1.Document.Body.FirstChild.FirstChild
                    .FirstChild.FirstChild.NextSibling.NextSibling
                    .FirstChild.FirstChild.NextSibling
                    ;

                var userNameElem = baselogginElem.FirstChild;
                var passElem = baselogginElem.NextSibling.NextSibling.FirstChild;// System.Web.UI.HtmlControls.HtmlInputControl;

                //Set attrib
                userNameElem.SetAttribute("value", "xxx");
                passElem.SetAttribute("value", "xxxx");


                //(webBrowser1.FindForm() as HtmlElement)
                //    .InvokeMember("submit");

                Submit(webBrowser1);
            }

        }

        private void Submit(WebBrowser inWebBrowserControl)
        {
            var elements = inWebBrowserControl.Document.GetElementsByTagName("Form");

            foreach (HtmlElement currentElement in elements)
            {
                currentElement.InvokeMember("submit");
            }
        }

        private void NavigateToAllKonto()
        {
            var aElem = FindChildWithId(webBrowser1.Document.Body, "item_1").FirstChild;
            NavigateToAsHref(aElem);

        }
        private void NavigateToLöneKonto()
        {
            //item_2

            //webBrowser1

            //var Dd = webBrowser1.Document.Body.FirstChild;
            //Dd = FindChildWithId(webBrowser1.Document.Body, "item_2");


            var aElem = FindChildWithId(webBrowser1.Document.Body, "item_2").FirstChild;
            NavigateToAsHref(aElem);

            //var href = elem.FirstChild.GetAttribute("href");

            //var url = href;
            //webBrowser1.Navigate(url);
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

            while( (htmlElement = htmlElement.NextSibling) !=null)
            {
                returnHtmlElement = FindChildWithId(htmlElement, idToFind);
            }

            if (returnHtmlElement != null)
            {
                return returnHtmlElement;
            }

            return null;
        }

        private void navigateToLöneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NavigateToLöneKonto();
        }

        private void testNav1ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void testBackNavToolStripMenuItem_Click(object sender, EventArgs e)
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

    }

}
