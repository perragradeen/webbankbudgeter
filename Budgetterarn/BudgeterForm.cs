﻿using System;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Budgeter.Core;
using Budgeter.Core.Entities;
using Budgetterarn.Application_Settings_and_constants;
using Budgetterarn.AutoNavigateBrowser;
using Budgetterarn.DAL;
using Budgetterarn.InternalUtilities;
using CategoryHandler;
using CefSharp;
using CefSharp.WinForms;
using LoadTransactionsFromFile;
using LoadTransactionsFromFile.DAL;
using Utilities;

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
        // TODO: Bugg när ke laddas från web. Dubbletter kommenr in i nya listan

        // Ändra i \Budgetterarn\Properties\AssemblyInfo.cs
        private const string VersionNumber = "1.0.1.16";

        #region Members

        private static ToolStripStatusLabel toolStripStatusLabel1;
        private static string bankUrl = "LoadsVia_xml_settings";

        private static string categoryPath = @"Data\Categories.xml";
        private bool debugGlobal = false; // For useSaveCheck

        private readonly KontoEntriesHolder kontoEntriesHolder
            = new KontoEntriesHolder();
        private bool somethingChanged;

        // Generic types for Designer
        private KontoEntryListView entriesInToBeSavedGrid;
        private ListViewWithComboBox newIitemsListEditedGrid;
        //private KontoEntryListView newIitemsListOrgGrid;
        private KontoEntryListView xlsOrginalEntriesGrid;

        private ProgramSettings programSettings;
        private AutoGetEntriesHbMobil autoGetEntriesHbMobilHandler;
        private ChromiumWebBrowser webBrowser1;

        #endregion

        public BudgeterForm()
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
                WriteExceptionToOutput(e, @"Init Error! :");
            }
        }

        #region Write to Output functions
        private static void WriteExceptionToOutput(Exception e, string message = "")
        {
            MessageBox.Show(message + " " + e.Message);
        }

        private static void WriteToOutput(string message)
        {
            MessageBox.Show(message);
        }

        private static void WriteToUiStatusLog(string statusInfo)
        {
            toolStripStatusLabel1.Text = statusInfo;
        }

        private static void AddToUiStatusLog(string statusInfo)
        {
            toolStripStatusLabel1.Text += statusInfo;
        }

        /// <summary>
        /// Settings (mostly debug)
        /// </summary>
        public static string StatusLabelText
        {
            set => WriteToUiStatusLog(value);
        }

        /// <summary>
        /// Titeltexten för fönstret
        /// </summary>
        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }
        #endregion

        #region Inits
        private void InitChromiumWebBrowser()
        {
            var settingsBrowse = new CefSettings();

            Cef.Initialize(settingsBrowse);

            webBrowser1 = new ChromiumWebBrowser(string.Empty);
            Controls.Add(webBrowser1);

            // 
            // webBrowser1
            // 
            webBrowser1.Dock = DockStyle.Fill;
            webBrowser1.Location = new Point(0, 0);
            webBrowser1.MinimumSize = new Size(20, 20);
            webBrowser1.Name = "webBrowser1";
            webBrowser1.Size = new Size(80, 609);
            webBrowser1.TabIndex = 0;
            //this.webBrowser1.IsLoading.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.WebBrowser1DocumentCompleted);

            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(webBrowser1);
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
                bankUrl = GeneralSettings.GetTextFileStringSetting("BankUrl");

                // Ladda kategorier som man har till att flagga olika kontohändelser
                CategoriesHolder.LoadAllCategoriesAndCreateHandler(categoryPath);

                // Initiera UI-objekt
                InitializeComponent();
                InitSpecialGenericUiElements();
                SetStatusLabelProps();

                // Sätt nuvarande tråd som main

                // läs in xls...
                EntriesFromFileLoadedOk(true); // after ctor
            }
            catch (Exception e)
            {
                WriteExceptionToOutput(e);
            }
        }

        private void InitSpecialGenericUiElements()
        {
            newIitemsListEditedGrid = new ListViewWithComboBox();
            //newIitemsListOrgGrid = new KontoEntryListView();
            entriesInToBeSavedGrid = new KontoEntryListView();
            xlsOrginalEntriesGrid = new KontoEntryListView();

            // tp_NewItemsEdited
            tp_NewItemsEdited.Controls.Add(newIitemsListEditedGrid);
            tp_NewItemsEdited.Location = new Point(4, 22);
            tp_NewItemsEdited.Name = "tp_NewItemsEdited";
            tp_NewItemsEdited.Padding = new Padding(3);
            tp_NewItemsEdited.Size = new Size(1161, 551);
            tp_NewItemsEdited.TabIndex = 0;
            tp_NewItemsEdited.Text = @"New items edited";
            tp_NewItemsEdited.UseVisualStyleBackColor = true;

            // m_newIitemsListEdited
            newIitemsListEditedGrid.Anchor = ((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left)
                                         | AnchorStyles.Right;
            newIitemsListEditedGrid.FullRowSelect = true;
            newIitemsListEditedGrid.GridLines = true;
            newIitemsListEditedGrid.Location = new Point(3, 3);
            newIitemsListEditedGrid.Name = "m_newIitemsListEdited";
            newIitemsListEditedGrid.Size = new Size(855, 545);
            newIitemsListEditedGrid.TabIndex = 0;
            newIitemsListEditedGrid.UseCompatibleStateImageBehavior = false;
            newIitemsListEditedGrid.View = View.Details;

            // tp_NewItemsOrg
            //tp_NewItemsOrg.Controls.Add(newIitemsListOrgGrid);
            tp_NewItemsOrg.Location = new Point(4, 22);
            tp_NewItemsOrg.Name = "tp_NewItemsOrg";
            tp_NewItemsOrg.Padding = new Padding(3);
            tp_NewItemsOrg.Size = new Size(1161, 551);
            tp_NewItemsOrg.TabIndex = 1;
            tp_NewItemsOrg.Text = @"New items orginal";
            tp_NewItemsOrg.UseVisualStyleBackColor = true;

            //// m_newIitemsListOrg
            //newIitemsListOrgGrid.Columns.AddRange(
            //    new[] { c_Date, c_Info, c_KostnadEllerInkomst, c_SaldoOrginal, c_AckumuleratSaldo, c_TypAvKostnad });
            //newIitemsListOrgGrid.Dock = DockStyle.Fill;
            //newIitemsListOrgGrid.FullRowSelect = true;
            //newIitemsListOrgGrid.GridLines = true;
            //newIitemsListOrgGrid.Location = new Point(3, 3);
            //newIitemsListOrgGrid.Name = "m_newIitemsListOrg";
            //newIitemsListOrgGrid.Size = new Size(1155, 545);
            //newIitemsListOrgGrid.TabIndex = 0;
            //newIitemsListOrgGrid.UseCompatibleStateImageBehavior = false;
            //newIitemsListOrgGrid.View = View.Details;

            m_inMemoryList.Controls.Add(entriesInToBeSavedGrid);

            // m_EntriesInToBeSaved
            entriesInToBeSavedGrid.Dock = DockStyle.Fill;
            entriesInToBeSavedGrid.FullRowSelect = true;
            entriesInToBeSavedGrid.GridLines = true;
            entriesInToBeSavedGrid.Location = new Point(3, 3);
            entriesInToBeSavedGrid.Name = "m_EntriesInToBeSaved";
            entriesInToBeSavedGrid.Size = new Size(288, 577);
            entriesInToBeSavedGrid.TabIndex = 0;
            entriesInToBeSavedGrid.UseCompatibleStateImageBehavior = false;
            entriesInToBeSavedGrid.View = View.Details;

            // m_originalXls
            m_originalXls.Controls.Add(xlsOrginalEntriesGrid);
            m_originalXls.Location = new Point(4, 22);
            m_originalXls.Name = "m_originalXls";
            m_originalXls.Padding = new Padding(3);
            m_originalXls.Size = new Size(294, 583);
            m_originalXls.TabIndex = 0;
            m_originalXls.Text = @"Xls Original";
            m_originalXls.UseVisualStyleBackColor = true;

            // m_XlsOrginalEntries
            xlsOrginalEntriesGrid.Dock = DockStyle.Fill;
            xlsOrginalEntriesGrid.FullRowSelect = true;
            xlsOrginalEntriesGrid.GridLines = true;
            xlsOrginalEntriesGrid.Location = new Point(3, 3);
            xlsOrginalEntriesGrid.Name = "m_XlsOrginalEntries";
            xlsOrginalEntriesGrid.Size = new Size(288, 577);
            xlsOrginalEntriesGrid.TabIndex = 0;
            xlsOrginalEntriesGrid.UseCompatibleStateImageBehavior = false;
            xlsOrginalEntriesGrid.View = View.Details;

            entriesInToBeSavedGrid.ListViewItemSorter = new ListViewColumnSorter();
            xlsOrginalEntriesGrid.ListViewItemSorter = new ListViewColumnSorter();
            newIitemsListEditedGrid.ListViewItemSorter = new ListViewColumnSorter();
            //newIitemsListOrgGrid.ListViewItemSorter = new ListViewColumnSorter();
        }
        #endregion

        #region Load&Save

        private void Save()
        {
            var kontoutdragInfoForSave = new ExcelFileKontoutdragInfoForSave
            {
                ExcelFileSaveFileName = FileReferences.ExcelFileSaveFileName,
                ExcelFileSavePath = FileReferences.ExcelFileSavePath,
                ExcelFileSavePathWithoutFileName =
                    FileReferences.ExcelFileSavePathWithoutFileName,
                SheetName = FileReferences.SheetName
            };

            var saveResult = SaveKonton.Save(
                kontoutdragInfoForSave,
                kontoEntriesHolder,
                WriteToOutput);

            somethingChanged = saveResult.SomethingLoadedOrSaved;

            // Räkna inte överskriften, den skrivs alltid om

            CheckIfUserWantsToOpenExcel(kontoutdragInfoForSave);

            //Precis sparat, så här har inget hunnit ändras 
            var statusText = toolStripStatusLabel1.Text
                             + " Saving done, saved entries; "
                             + saveResult.SkippedOrSaved;
            WriteToUiStatusLog(statusText);
        }

        private static void CheckIfUserWantsToOpenExcel(ExcelFileKontoutdragInfoForSave kontoutdragInfoForSave)
        {
            // Fråga om man vill öppna Excel
            var question = @"Open budget file (wait a litte while first)?";
            var userWantsToOpen = MessageBox.Show(
                question,
                @"Open file",
                MessageBoxButtons.YesNo);

            if (userWantsToOpen == DialogResult.Yes)
            {
                ExcelOpener.LoadExcelFileInExcel(kontoutdragInfoForSave.ExcelFileSavePath);
            }
        }

        private void LoadCurrentEntriesFromBrowser()
        {
            WriteToUiStatusLog(@"Processing");

            var loadKontonHandler = new LoadKontonFromWebBrowser(kontoEntriesHolder);
            var somethingLoadeded = loadKontonHandler.GetAllVisibleEntriesFromWebBrowser(
                webBrowser1.GetTextAsync().Result
            );

            // Meddela på nåt sätt att det är klart, och antal inlästa, i tex. statusbar
            WriteToUiStatusLog(@"Done processing  no new entries fond from html.");

            if (!somethingLoadeded) return;

            CheckAndAddNewItems(); // Lägg till nya i GuiLista FromBrowser
            WriteToUiStatusLog(@"Done processing entries from html. New Entries found; "
                               + kontoEntriesHolder.NewKontoEntries.Count
                               + @".");
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
        private void EntriesFromFileLoadedOk(bool clearContentBeforeReadingNewFile)
        {
            SetAllEntriesFromExcelFile(clearContentBeforeReadingNewFile);

            DisplayEntriesInUiGrids();
        }

        private void SetAllEntriesFromExcelFile(bool clearContentBeforeReadingNewFile)
        {
            var kontoutdragInfoForLoad = InitKontoInfo();

            CheckFileIfEmptyPromptUserIfEmptyPath(kontoutdragInfoForLoad);

            GetLoadFromFileHelper(kontoutdragInfoForLoad)
                .SetEntriesFromFile(clearContentBeforeReadingNewFile);
        }

        private LoadFromFileHelper GetLoadFromFileHelper(
            ExcelFileKontoutdragInfoForLoad kontoutdragInfoForLoad)
        {
            return new LoadFromFileHelper(
                kontoutdragInfoForLoad,
                kontoEntriesHolder,
                WriteToOutput,
                WriteToUiStatusLog);
        }

        private void DisplayEntriesInUiGrids()
        {
            // Lägg till orginalraden, gör i UI-hanterare
            // Lägg in det som är satt att sparas till minnet
            // (viasa alla _kontoEntries i listview). Även uppdatera färg på text.
            ViewUpdateUi.ClearListAndSetEntriesToListView(
                xlsOrginalEntriesGrid,
                kontoEntriesHolder.KontoEntries);
            ViewUpdateUi.ClearListAndSetEntriesToListView(
                entriesInToBeSavedGrid,
                kontoEntriesHolder.KontoEntries);
        }

        private ExcelFileKontoutdragInfoForLoad InitKontoInfo()
        {
            return new ExcelFileKontoutdragInfoForLoad
            {
                ExcelFileSavePath = FileReferences.ExcelFileSavePath,
                ExcelFileSavePathWithoutFileName =
                    FileReferences.ExcelFileSavePathWithoutFileName,
                ExcelFileSaveFileName = FileReferences.ExcelFileSaveFileName,
                SheetName = FileReferences.SheetName,
            };
        }

        // TODO: Kolla om denna ska användas...
        private static bool UserWantsToSave()
        {
            return MessageBox.Show(
                    @"Läget ej sparat! Spara nu?",
                    @"Spara?",
                    MessageBoxButtons.YesNoCancel)
                
                 == DialogResult.Yes;
        }
        #endregion

        #region Koppling av data till UI

        private void UpdateEntriesToSaveMemList()
        {
            ViewUpdateUi.ClearListAndSetEntriesToListView(
                entriesInToBeSavedGrid,
                kontoEntriesHolder.KontoEntries);
        }

        private void CheckAndAddNewItems()
        {
            CheckAndAddNewItems(
                new KontoEntriesViewModelListUpdater
                {
                    KontoEntries = kontoEntriesHolder.KontoEntries,
                    NewItemsListEdited = newIitemsListEditedGrid.ItemsAsKontoEntries,
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

            //// Lägg till i org
            //lists.NewItemsListOrg.ForEach(k =>
            //    ViewUpdateUi.AddToListview(newIitemsListOrgGrid, k));

            // Filtrera ut de som inte redan ligger i UI
            var inUiListAlready = newIitemsListEditedGrid.ItemsAsKontoEntries;
            foreach (var entry in lists.NewItemsListEdited)
            {
                if (inUiListAlready.All(e => e.KeyForThis != entry.KeyForThis))
                {
                    lists.ToAddToListview.Add(entry);
                }
            }

            foreach (var entry in lists.ToAddToListview)
            {
                // kolla om det är "Skyddat belopp", och se om det finns några
                // gamla som matchar.
                SkyddatBeloppChecker.CheckForSkyddatBeloppMatcherAndGuessDouble(
                    entry,
                    kontoEntriesHolder.KontoEntries);
            }

            // Lägg till i edited
            ViewUpdateUi.AddEntriesToListView(newIitemsListEditedGrid, lists.ToAddToListview);

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
            //autoGetEntriesHbMobilHandler.BrowserNavigator.NavigateToFirstItemInVisibleList();
        }

        private void SetLoginToolStripMenuItemClick(object sender, EventArgs e)
        {
            //autoGetEntriesHbMobilHandler.BrowserNavigator.SetLoginUserEtc();
        }

        private void NavigateToLöneToolStripMenuItemClick(object sender, EventArgs e)
        {
            //autoGetEntriesHbMobilHandler.BrowserNavigator.NavigateToLöneKonto();
        }

        private void LoadNewAndClearOld_FileMenuClick(object sender, EventArgs e)
        {
            // Helt ny fil ska laddas, töm gammalt
            // Ev. Todo: Rensa UI också, eller lita på att funktionen klarar det iom laddning kan avbrytas etc.
            // Man vill öppna en annan fil som man ska välja och som man ska hämta värden ifrån. Sen spara som den filen man valt. Att börja om med annan fil
            EntriesFromFileLoadedOk(true); // LoadNewAndClearOld_FileMenuClick
        }

        private void AddNew_FileMenuClick(object sender, EventArgs e)
        {
            // Adding entries here, no clear
            // Man vill lägga till fler värden ifrån en annan fil som man ska välja. Sen spara som den tidigare filen man valt. Att börja om med annan fil
            EntriesFromFileLoadedOk(false); // AddNew_FileMenuClick
        }

        private void WebBrowser1DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (!programSettings.AutoLoadEtc)
            {
                return;
            }

            WriteToUiStatusLog(@"Done");

            try
            {
                autoGetEntriesHbMobilHandler.LoadingCompleted();
            }
            catch (Exception browseExp)
            {
                WriteToOutput(@"Error in WebBrowser1DocumentCompleted! : "
                                + browseExp.Message);
            }
        }

        private void DebugToolStripMenuItemClick(object sender, EventArgs e)
        {
            webBrowser1.Load(
                "https://secure.handelsbanken.se"
                + "/bb/seip/servlet/UASipko?appAction=ShowAccountOverview&appName=ipko");
        }

        private void LoadCurrentEntriesToolStripMenuItemClick(object sender, EventArgs e)
        {
            LoadCurrentEntriesFromBrowser(); // MenuItemClick
        }

        private void BtnLoadCurrentEntriesClick(object sender, EventArgs e)
        {
            LoadCurrentEntriesFromBrowser(); // BtnClick
        }

        // Todo: change name
        private void BudgeterFormClosing(object sender, FormClosingEventArgs e)
        {
            if (debugGlobal) return;
            if (WinFormsChecks.SaveCheck(somethingChanged, Save) == DialogResult.Cancel)
            {
                e.Cancel = true;
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
            var userSure = MessageBox.Show(
                @"Delete new entries",
                @"Are u sure?",
                MessageBoxButtons.YesNo);

            if (userSure == DialogResult.Yes)
            {
                ClearNewOnesFnc();
            }
        }

        private void BtnRecheckAutocatClick(object sender, EventArgs e)
        {
            ListViewWithComboBox.UpdateCategoriesWithAutoCatList(newIitemsListEditedGrid.Items);
        }

        private void AddCatergoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CategoriesHolder.LoadAllCategoriesAndCreateHandler(categoryPath);
            newIitemsListEditedGrid.LoadCategoriesToSelectBox();
        }

        private void LoadOldEntriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Sätt de gamla inlästa transaktionerna i minnet in i nya lista för redigering av kategori
            kontoEntriesHolder.NewKontoEntries = GetOldEntriesWithoutCategory();

            KontoEntriesChecker.OkToAddFromOld = true;
            CheckAndAddNewItems(); // Lägg till gamla i GuiLista för redigering
            KontoEntriesChecker.OkToAddFromOld = false;
        }

        private SortedList GetOldEntriesWithoutCategory()
        {
            var size = kontoEntriesHolder.KontoEntries.Count;
            KontoEntry[] tempOldEntries = new KontoEntry[size];
            kontoEntriesHolder.KontoEntries.Values.CopyTo(tempOldEntries, 0);
            var filteredOldEntries = tempOldEntries
                .Where(el => string.IsNullOrEmpty(el.TypAvKostnad));
            var dict = filteredOldEntries.ToDictionary(ell => ell.KeyForThis);
            var sortedList = new SortedList(dict);
            return sortedList;
        }

        #endregion

        #region Funktioner, TODO: ha en del av dessa funktioner i egen fil

        #region Har med UIobjekt i denna klass att göra

        private static string GetBankUrl()
        {
            return
                //@"C:\Files\Dropbox\budget\Program\TestData\x.html"
                bankUrl
            ;
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
            WriteToUiStatusLog(@"Loading");

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
            //newIitemsListOrgGrid.Items.Clear();
            newIitemsListEditedGrid.Items.Clear();
            kontoEntriesHolder.NewKontoEntries.Clear();

            // Rensa även listan som är en kopia av Guilistan för nya ke
        }

        #endregion

        private void RunAutoLoadIfItIsEnabled()
        {
            if (!programSettings.AutoLoadEtc) return;

            autoGetEntriesHbMobilHandler = new AutoGetEntriesHbMobil(
                LoadCurrentEntriesFromBrowser, // AutoLoad
                null);

            autoGetEntriesHbMobilHandler.AutoNavigateToKontonEtc();
        }

        private static void CheckFileIfEmptyPromptUserIfEmptyPath(
            ExcelFileKontoutdragInfoForLoad kontoutdragInfoForLoad)
        {
            if (!string.IsNullOrWhiteSpace(
                kontoutdragInfoForLoad.ExcelFileSavePath))
            {
                return;
            }

            var filePath = FileOperations.OpenFileOfType(
                        @"Open file",
                        FileType.Xls,
                        string.Empty,
                        WriteToOutput); // Öppnar dialog

            // Ev. har pathen ändrats.
            // Har man däremot laddat in nya så ska den sökvägen gälla för sparningar
            FileReferences.ExcelFileSavePath =
            kontoutdragInfoForLoad.ExcelFileSavePath =
                filePath;
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
            WriteToUiStatusLog(@"toolStripStatusLabel1");
        }

        /// <summary>
        /// Accessar status etikett-ui-elementet.
        /// </summary>
        /// <param name="autoSave"></param>
        private void AddNewEntriesToUiListsAndMem(bool autoSave)
        {
            WriteToUiStatusLog(@"Trying to add; " +
                kontoEntriesHolder.NewKontoEntries.Count + @"items");

            // Hämta nya entries från Ui.
            // (slipper man om man binder ui-kontroller med de som är
            // sparade och ändrade i minnet.)
            var newEntriesFromUi = GetNewEntriesFromUI(newIitemsListEditedGrid);

            // Lägg till/Updatera nya
            var changeInfoHandler = new EntryAdderAndReplacer(
                kontoEntriesHolder.KontoEntries,
                WriteToOutput,
                AddToUiStatusLog);
            var changeInfo = changeInfoHandler.AddNewEntries(
                newEntriesFromUi);

            somethingChanged = CheckIfSomethingWasChanged(
                somethingChanged,
                changeInfo.SomethingChanged);

            UpdateEntriesToSaveMemList();

            WriteToUiStatusLog(@"Entries in memory updated. " +
                @"Added entries; " + changeInfo.Added + ". " +
                @"Replaced entries; " + changeInfo.Replaced);

            if (autoSave)
            {
                Save();
            }
        }

        private static bool CheckIfSomethingWasChanged(bool oldSomethingChanged, bool newSomethingChanged)
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

        private void TestNav1ToolStripMenuItemClick(object sender, EventArgs e)
        {
        }

        private void TestBackNavToolStripMenuItemClick(object sender, EventArgs e)
        {
            //autoGetEntriesHbMobilHandler.BrowserNavigator.BrowserGoBack();
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
                if (kontoEntriesHolder.NewKontoEntries.ContainsKey(testKey)) continue;
                var newInfo = "test" + (i % 2 == 0 ? i.ToString(CultureInfo.InvariantCulture) : string.Empty);
                kontoEntriesHolder.NewKontoEntries.Add(testKey, new KontoEntry { Date = DateTime.Now.AddDays(i), Info = newInfo });
                sometheingadded = true;
            }

            if (sometheingadded)
            {
                CheckAndAddNewItems(); // Debug
            }
        }
        #endregion
    }
}