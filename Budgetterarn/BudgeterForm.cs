using Budgeter.Core;
using Budgeter.Core.Entities;
using Budgetterarn.Application_Settings_and_constants;
using Budgetterarn.DAL;
using Budgetterarn.InternalUtilities;
using CategoryHandler;
using LoadTransactionsFromFile;
using System;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Utilities;

using CefSharp;
using CefSharp.WinForms;
// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo

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
        private static string GetBankUrl()
        {
            return
                //@"C:\Files\Dropbox\budget\Program\TestData\x.html"
                bankUrl
            ;
        }

        // Bugg när ke laddas från web. Dubbletter kommenr in i nya listan
        public const string VersionNumber = "1.0.1.16"; // Ändra i \Budgetterarn\Properties\AssemblyInfo.cs

        #region Members

        private const string SheetName = "Kontoutdrag_officiella";
        private static ToolStripStatusLabel toolStripStatusLabel1;
        private static string bankUrl = "LoadsVia_xml_settings";

        private static string categoryPath = @"Data\Categories.xml";
        private bool debugGlobal; // For useSaveCheck

        private readonly KontoEntriesHolder kontoEntriesHolder = new KontoEntriesHolder();
        private bool somethingChanged;

        // Generic types for Designer
        private KontoEntryListView entriesInToBeSaved;
        private ListViewWithComboBox newIitemsListEdited;
        private KontoEntryListView newIitemsListOrg;
        private KontoEntryListView xlsOrginalEntries;

        private ProgramSettings programSettings;
        private AutoGetEntriesHbMobil autoGetEntriesHbMobilHandler;
        public ChromiumWebBrowser webBrowser1;
        #endregion

        public void InitChromiumWebBrowser()
        {
            var settingsBrowse = new CefSettings();

            Cef.Initialize(settingsBrowse);

            webBrowser1 = new ChromiumWebBrowser(string.Empty);
            this.Controls.Add(webBrowser1);

            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = DockStyle.Fill;
            this.webBrowser1.Location = new Point(0, 0);
            this.webBrowser1.MinimumSize = new Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new Size(80, 609);
            this.webBrowser1.TabIndex = 0;
            //this.webBrowser1.IsLoading.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.WebBrowser1DocumentCompleted);

            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.webBrowser1);
        }

        public BudgeterForm() // Konstruktor
        {
            try
            {
                InitFields();

                InitSettingsEtc();

                InitChromiumWebBrowser();

                #region Debug
                if (Debug())
                {
                    debugbtn.Visible = true;
                    DebugAddoNewList();
                }
                #endregion
                else
                {
                    // Öpnna banksidan direkt
                    OpenBankSiteInBrowser();

                    RunAutoLoadIfItIsEnabled();
                }

                SetVersionsnummerToWindowTitle();
            }
            catch (Exception e)
            {
                MessageBox.Show(@"Init Error! : " + e.Message);
            }
        }

        private void RunAutoLoadIfItIsEnabled()
        {
            if (programSettings.AutoLoadEtc)
            {
                autoGetEntriesHbMobilHandler = new AutoGetEntriesHbMobil(LoadCurrentEntriesFromBrowser, null);
                autoGetEntriesHbMobilHandler.AutoNavigateToKontonEtc();
            }
        }

        /// <summary>
        /// Sätt versionsnummer i titel
        /// </summary>
        private void SetVersionsnummerToWindowTitle()
        {
            if (Text != null)
            {
                Text += VersionNumber;
            }
        }

        private bool Debug()
        {
            // ReSharper disable JoinDeclarationAndInitializer
            bool debug = false;

            // ReSharper restore JoinDeclarationAndInitializer
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

            return debug;
        }

        /// <summary>
        /// Settings (mostly debug)
        /// </summary>
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

        private void InitFields()
        {
            programSettings = new ProgramSettings();
        }

        private void InitSettingsEtc()
        {
            try
            {
                // Get file names from settings file
                categoryPath = GeneralSettings.GetStringSetting("CategoryPath");
                bankUrl = GeneralSettings.GetTextfileStringSetting("BankUrl");

                // Ladda kategorier som man har till att flagga olika kontohändelser
                CategoriesHolder.LoadAllCategoriesAndCreateHandler(categoryPath);

                // Initiera UI-objekt
                InitializeComponent();
                InitSpecialGenericUiElements();
                SetStatusLabelProps();

                // Sätt nuvarande tråd som main

                // läs in xls...
                GetAllEntriesFromExcelFile(Filerefernces.ExcelFileSavePath, true);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void InitSpecialGenericUiElements()
        {
            newIitemsListEdited = new ListViewWithComboBox();
            newIitemsListOrg = new KontoEntryListView();
            entriesInToBeSaved = new KontoEntryListView();
            xlsOrginalEntries = new KontoEntryListView();

            // tp_NewItemsEdited
            tp_NewItemsEdited.Controls.Add(newIitemsListEdited);
            tp_NewItemsEdited.Location = new Point(4, 22);
            tp_NewItemsEdited.Name = "tp_NewItemsEdited";
            tp_NewItemsEdited.Padding = new Padding(3);
            tp_NewItemsEdited.Size = new Size(1161, 551);
            tp_NewItemsEdited.TabIndex = 0;
            tp_NewItemsEdited.Text = @"New items edited";
            tp_NewItemsEdited.UseVisualStyleBackColor = true;

            // m_newIitemsListEdited
            newIitemsListEdited.Anchor = ((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left)
                                         | AnchorStyles.Right;
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

        #region Load&Save

        public static DialogResult SaveCheckWithArgs(
           KontoutdragInfoForLoad kontoutdragInfoForSave, SortedList kontoEntries, SaldoHolder saldoHolder)
        {
            var saveOr = DialogResult.None;
            if (kontoutdragInfoForSave.SomethingChanged)
            {
                saveOr = MessageBox.Show(@"Läget ej sparat! Spara nu?", @"Spara?", MessageBoxButtons.YesNoCancel);

                // Cancel
                if (saveOr == DialogResult.Yes)
                {
                    SaveKonton.Save
                        (kontoutdragInfoForSave, kontoEntries, saldoHolder);
                }
            }

            return saveOr;
        }

        private bool SaveFirstCheck(
            KontoutdragInfoForLoad kontoutdragInfoForLoad, bool checkforUnsavedChanges, bool somethingLoadedFromFile)
        {
            // Nu har något laddats från fil, kolla då om något ska sparas
            // Save check
            if (checkforUnsavedChanges && somethingLoadedFromFile)
            {
                if (kontoEntriesHolder.KontoEntries.Count > 0)
                {
                    // somethingChanged är alltid false här
                    var userResponse = SaveCheckWithArgs(kontoutdragInfoForLoad, kontoEntriesHolder.KontoEntries,
                        kontoEntriesHolder.SaldoHolder);
                    if (userResponse == DialogResult.Cancel)
                    {
                        return true;
                    }
                }
                else
                {
                    kontoutdragInfoForLoad.SomethingChanged = false;
                }
            }

            return false;
        }

        private void Save()
        {
            var statusText = toolStripStatusLabel1.Text;
            var kontoutdragInfoForSave = new KontoutdragInfoForSave
            {
                ExcelFileSaveFileName = Filerefernces.ExcelFileSaveFileName,
                ExcelFileSavePath = Filerefernces.ExcelFileSavePath,
                ExcelFileSavePathWithoutFileName =
                    Filerefernces.ExcelFileSavePathWithoutFileName,
                SheetName = SheetName
            };

            var saveResult = SaveKonton.Save(kontoutdragInfoForSave, kontoEntriesHolder.KontoEntries,
                kontoEntriesHolder.SaldoHolder);

            somethingChanged = saveResult.SomethingLoadedOrSaved;

            //Precis sparat, så här har inget hunnit ändras 
            statusText += "Saving done, saved entries; " + saveResult.SkippedOrSaved;

            // Räkna inte överskriften, den skrivs alltid om

            // Fråga om man vill öppna Excel
            if (MessageBox.Show(@"Open budget file (wait a litte while first)?", @"Open file", MessageBoxButtons.YesNo)
                == DialogResult.Yes)
            {
                ExcelOpener.LoadExcelFileInExcel(kontoutdragInfoForSave.ExcelFileSavePath);
            }

            toolStripStatusLabel1.Text = statusText;
        }

        private void LoadCurrentEntriesFromBrowser()
        {
            toolStripStatusLabel1.Text = @"Processing";

            var somethingLoadeded = LoadKonton.GetAllVisibleEntriesFromWebBrowser(
                kontoEntriesHolder,
                webBrowser1.GetTextAsync().Result
            );

            // Meddela på nåt sätt att det är klart, och antal inlästa, i tex. statusbar
            toolStripStatusLabel1.Text = @"Done processing  no new entries fond from html.";

            if (somethingLoadeded)
            {
                CheckAndAddNewItems();
                toolStripStatusLabel1.Text = @"Done processing entries from html. New Entries found; "
                                             + kontoEntriesHolder.NewKontoEntries.Count + @".";
            }
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
            var statusText = toolStripStatusLabel1.Text = @"Nothing loaded.";
            var changedExcelFileSavePath = Filerefernces.ExcelFileSavePath;

            // Todo: gör en funktion för denna eller refa med en filnamns och sökvägsklass....
            var kontoutdragInfoForLoad = new KontoutdragInfoForLoad
            {
                FilePath = Filerefernces.ExcelFileSavePath,
                ExcelFileSavePath = changedExcelFileSavePath,
                ExcelFileSavePathWithoutFileName =
                                                 Filerefernces.ExcelFileSavePathWithoutFileName,
                ExcelFileSaveFileName = Filerefernces.ExcelFileSaveFileName,
                SheetName = SheetName,
                ClearContentBeforeReadingNewFile = clearContentBeforeReadingNewFile,
                SomethingChanged = somethingChanged,
            };

            CheckFileIfEmptyPromptUserIfEmptyPath(kontoutdragInfoForLoad);

            // Ladda från fil
            Hashtable entriesLoadedFromDataStore;
            try
            {
                entriesLoadedFromDataStore = LoadKonton.LoadEntriesFromFile(kontoutdragInfoForLoad);
            }
            catch (Exception)
            {
                MessageBox.Show("File: " + kontoutdragInfoForLoad?.FilePath + " does not exist.", "File error");
                return false;
            }

            // För att se om något laddats från fil
            var somethingLoadedFromFile = entriesLoadedFromDataStore != null && entriesLoadedFromDataStore.Count > 0;

            if (entriesLoadedFromDataStore == null)
            {
                statusText += kontoutdragInfoForLoad.FilePath;
            }

            toolStripStatusLabel1.Text = statusText;

            // kolla om något laddades från Excel
            if (!somethingLoadedFromFile)
            {
                return false;
            }

            const bool CheckforUnsavedChanges = true;
            var userCanceled = SaveFirstCheck(kontoutdragInfoForLoad, CheckforUnsavedChanges, true);

            if (userCanceled)
            {
                return false;
            }

            var loadResult = LoadKonton.GetAllEntriesFromExcelFile(
                kontoutdragInfoForLoad,
                kontoEntriesHolder.KontoEntries,
                kontoEntriesHolder.SaldoHolder,
                entriesLoadedFromDataStore);

            // Visa text för anv. om hur det gick etc.
            statusText = "No. rows loaded; " + kontoEntriesHolder.KontoEntries.Count + " . Skpped: " + loadResult.SkippedOrSaved
                         + ". File loaded; " + kontoutdragInfoForLoad.FilePath;

            // Nu har det precis rensats och laddats in nytt
            kontoutdragInfoForLoad.SomethingChanged = !CheckforUnsavedChanges;

            // Ev. har pathen ändrats.
            if (excelFileSavePath == string.Empty)
            {
                // Om man lagt till nya rader från annan fil, så spara i den gamla.
            }
            else
            {
                // Har man däremot laddat in nya så ska den sökvägen gälla för sparningar
                Filerefernces.ExcelFileSavePath = changedExcelFileSavePath;

                // Todo: sätt denna tidigare så att LoadNsave bara gör vad den ska utan UI etc
            }

            toolStripStatusLabel1.Text = statusText;

            // If nothing loaded return
            if (!loadResult.SomethingLoadedOrSaved)
            {
                return false;
            }

            // Lägg till orginalraden, gör i UI-hanterare
            // Lägg in det som är satt att sparas till minnet (viasa alla _kontoEntries i listview). Även uppdatera färg på text.
            ViewUpdateUi.SetNewItemsListViewFromSortedList(xlsOrginalEntries, kontoEntriesHolder.KontoEntries);
            ViewUpdateUi.SetNewItemsListViewFromSortedList(entriesInToBeSaved, kontoEntriesHolder.KontoEntries);

            return true;
        }

        #endregion

        #region Koppling av data till UI

        private void UpdateEntriesToSaveMemList()
        {
            ViewUpdateUi.SetNewItemsListViewFromSortedList(entriesInToBeSaved, kontoEntriesHolder.KontoEntries);
        }

        private void CheckAndAddNewItems()
        {
            CheckAndAddNewItems(
                new KontoEntriesViewModelListUpdater
                {
                    KontoEntries = kontoEntriesHolder.KontoEntries,
                    NewIitemsListEdited = newIitemsListEdited.ItemsAsKontoEntries,
                    NewKontoEntriesIn = kontoEntriesHolder.NewKontoEntries,
                }
            );
        }

        /// <summary>Uppdatera UI för nya entries, gör gisningar av dubbletter, typ av kostnad etc
        /// </summary>
        private void CheckAndAddNewItems(KontoEntriesViewModelListUpdater lists)
        {
            // Flagga och se vad som är nytt etc.
            KontoEntriesChecker.CheckAndAddNewItemsForLists(lists);

            // Lägg till i org
            if (lists.NewIitemsListOrg != null)
            {
                lists.NewIitemsListOrg.ForEach(k => ViewUpdateUi.AddToListview(newIitemsListOrg, k));
            }

            // Filtrera ut de som inte redan ligger i UI
            var inUiListAlready = newIitemsListEdited.ItemsAsKontoEntries;
            foreach (var entry in lists.NewIitemsListEdited)
            {
                if (!inUiListAlready.Any(e => e.KeyForThis == entry.KeyForThis))
                {
                    lists.ToAddToListview.Add(entry);
                }
            }

            foreach (var entry in lists.ToAddToListview)
            {
                // kolla om det är "Skyddat belopp", och se om det finns några gamla som matchar.
                KontoEntriesChecker.CheckForSkyddatBeloppMatcherAndGuesseDouble(entry, kontoEntriesHolder.KontoEntries);

                // Lägg till i edited
                ViewUpdateUi.AddToListview(newIitemsListEdited, entry);
            }

            // Updatera memlistan för att se om någon entry fått ny färg
            UpdateEntriesToSaveMemList();
        }

        #endregion

        #region Events (button clicks etc)

        private void OpenUrlToolStripMenuItemClick(object sender, EventArgs e)
        {
            var url = InputBoxDialog.InputBox(
                "Skirv url", "Navigera till", webBrowser1.Address);

            webBrowser1.Load(url);
        }

        private void NavigeraToolStripMenuItemClick(object sender, EventArgs e)
        {
            autoGetEntriesHbMobilHandler.BrowserNavigator.NavigateToFirstItemInVisibleList();
        }

        private void SetLoginToolStripMenuItemClick(object sender, EventArgs e)
        {
            autoGetEntriesHbMobilHandler.BrowserNavigator.SetLoginUserEtc();
        }

        private void NavigateToLöneToolStripMenuItemClick(object sender, EventArgs e)
        {
            autoGetEntriesHbMobilHandler.BrowserNavigator.NavigateToLöneKonto();
        }

        private void LoadToolStripMenuItem1Click(object sender, EventArgs e)
        {
            // _kontoEntries.Clear();//Töm här kan ge att det inte kommer in något nytt...

            // Helt ny fil ska laddas, töm gammalt
            // Ev. Todo: Rensa UI också, eller lita på att funktionen klarar det iom laddning kan avbrytas etc.
            // Man vill öppna en annan fil som man ska välja och som man ska hämta värden ifrån. Sen spara som den filen man valt. Att börja om med annan fil
            GetAllEntriesFromExcelFile(
                debugGlobal ? Filerefernces.ExcelFileSavePath : string.Empty, true);
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

        private void LoadCurrentEntriesToolStripMenuItemClick(object sender, EventArgs e)
        {
            LoadCurrentEntriesFromBrowser();
        }

        private void WebBrowser1DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (!programSettings.AutoLoadEtc)
            {
                return;
            }

            toolStripStatusLabel1.Text = @"Done";

            try
            {
                autoGetEntriesHbMobilHandler.LoadingCompleted();
            }
            catch (Exception browseExp)
            {
                MessageBox.Show(@"Error in WebBrowser1DocumentCompleted! : " + browseExp.Message);
            }
        }

        private void DebugToolStripMenuItemClick(object sender, EventArgs e)
        {
            webBrowser1.Load(
                "https://secure.handelsbanken.se"
                + "/bb/seip/servlet/UASipko?appAction=ShowAccountOverview&appName=ipko");
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
                if (WinFormsChecks.SaveCheck(somethingChanged, Save) == DialogResult.Cancel)
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
            webBrowser1.Load(GetBankUrl());
        }

        /// <summary>
        /// Rensa minnet och m_newIitemsListOrg
        /// </summary>
        private void ClearNewOnesFnc()
        {
            newIitemsListOrg.Items.Clear();
            newIitemsListEdited.Items.Clear();
            kontoEntriesHolder.NewKontoEntries.Clear();

            // Rensa även listan som är en kopia av Guilistan för nya ke
        }

        #endregion

        private void CheckFileIfEmptyPromptUserIfEmptyPath(KontoutdragInfoForLoad kontoutdragInfoForLoad)
        {
            if (string.IsNullOrWhiteSpace(kontoutdragInfoForLoad.FilePath))
            {
                kontoutdragInfoForLoad.ExcelFileSavePath =
                    kontoutdragInfoForLoad.FilePath =
                        FileOperations.OpenFileOfType("Open file", FileType.xls, ""); // Öppnar dialog
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

        /// <summary>
        /// Accessar status etikett-ui-elementet.
        /// </summary>
        /// <param name="autoSave"></param>
        private void AddNewEntriesToUiListsAndMem(bool autoSave)
        {
            toolStripStatusLabel1.Text = @"Trying to add; " + kontoEntriesHolder.NewKontoEntries.Count + @"items";

            // Hämta nya entries från Ui. (slipper man om man binder ui-kontroller med de som är sparade och ändrade i minnet.)
            var newEntriesFromUi = GetNewEntriesFromUI(newIitemsListEdited);

            // Lägg till/Updatera nya
            var changeInfo = UiHelpersDependant.AddNewEntries(kontoEntriesHolder.KontoEntries, newEntriesFromUi);
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
            return oldSomethingChanged || newSomethingChanged;
        }

        private static SortedList GetNewEntriesFromUI(ListView mineNewIitemsListEdited)
        {
            // Hämta nya entries från Ui. (slipper man om man binder ui-kontroller med de som är sparade och ändrade i minnet.)
            var newEntriesFromUi = new SortedList();
            foreach (ListViewItem item in mineNewIitemsListEdited.Items)
            {
                if (item.Tag is KontoEntry newKe 
                    && !newEntriesFromUi.ContainsKey(newKe.KeyForThis))
                {
                    newEntriesFromUi.Add(newKe.KeyForThis, newKe);
                }
            }

            return newEntriesFromUi;
        }

        #endregion

        #region Test&Debug
        // TODO: ta bort alla tester o flytta ev till unit/integrationstester...

        private void TestNav1ToolStripMenuItemClick(object sender, EventArgs e)
        {
        }

        private void TestBackNavToolStripMenuItemClick(object sender, EventArgs e)
        {
            autoGetEntriesHbMobilHandler.BrowserNavigator.BrowserGoBack();
        }

        private void DebugbtnClick(object sender, EventArgs e)
        {
            DebugAddoNewList();
        }

        private void DebugAddoNewList()
        {
            var sometheingadded = false;
            for (var i = 0; i < 8; i++)
            {
                if (kontoEntriesHolder.NewKontoEntries == null)
                {
                    continue;
                }

                var testKey = "testkey" + i;
                if (!kontoEntriesHolder.NewKontoEntries.ContainsKey(testKey))
                {
                    var newInfo = "test" + (i % 2 == 0 ? i.ToString(CultureInfo.InvariantCulture) : string.Empty);
                    kontoEntriesHolder.NewKontoEntries.Add(testKey, new KontoEntry { Date = DateTime.Now.AddDays(i), Info = newInfo });
                    sometheingadded = true;
                }
            }

            if (sometheingadded)
            {
                CheckAndAddNewItems(); // Debug
            }
        }

        #endregion
    }
}