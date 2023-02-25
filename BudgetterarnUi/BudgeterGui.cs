using Budgeter.Core;
using Budgeter.Core.Entities;
using Budgetterarn;
using Budgetterarn.EntryLogicSetFlags;
using CategoryHandler;
using GeneralSettingsHandler;
using LoadTransactionsFromFile;
using System.Collections;
using System.Globalization;

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo

// Budgeter.Winforms
namespace BudgetterarnUi
{
    // Todos se Data/Todos.txt
    /// <summary>
    /// Xls-fil som läses in förutsätts ha Kontoutdrag_officiella som ark med 6 celler ejämte varann enligt ex. nedan:
    /// 2007-09-05 SkyDDat belopp -120,00 100 991,24 127,02 telefonsamtal
    /// </summary>
    public partial class BudgeterGui : Form
    {
        // Ändra i \Budgetterarn\Properties\AssemblyInfo.cs
        private const string VersionNumber = "1.0.1.16";
        private readonly GeneralSettingsGetter generalSettingsGetter;
        private readonly BudgeterFormHelper budgeterFormHelper;

        #region Members

        private static ToolStripStatusLabel toolStripStatusLabel1;
        private static string bankUrl = "LoadsVia_xml_settings";

        private static string categoryPath = @"Data\Categories.xml";
        private bool debugGlobal; // For useSaveCheck

        private readonly KontoEntriesHolder kontoEntriesHolder
            = new KontoEntriesHolder();

        //private KontoEntryListView newIitemsListOrgGrid;
        //private KontoEntryListView xlsOrginalEntriesGrid;

        private ProgramSettings programSettings;

        private string GetGeneralSettingsPath()
        {
            var path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"Data\"
            );
            return Path.Combine(path, @"GeneralSettings.xml");
        }

        #endregion

        public BudgeterGui()
        {
            try
            {
                generalSettingsGetter = new GeneralSettingsGetter(GetGeneralSettingsPath());
                budgeterFormHelper = GetBudgetFormHelper();

                InitFields();

                InitSettingsEtc();

                InitChromiumWebBrowser();

                if (DebugModeOff())
                {
                    // Öpnna banksidan direkt
                    OpenBankSiteInBrowser();
                }

                SetVersionsnummerToWindowTitle();
            }
            catch (Exception e)
            {
                WriteExceptionToOutput(e, @"Init Error! :");
            }
        }

        private BudgeterFormHelper GetBudgetFormHelper()
        {
            return new BudgeterFormHelper(
                    WriteToOutput,
                    WriteToUiStatusLog,
                    CheckAndAddNewItems,
                    kontoEntriesHolder,
                    generalSettingsGetter
                );
        }

        #region Inits

        private void InitFields()
        {
            programSettings = new ProgramSettings();
        }

        private void InitSettingsEtc()
        {
            try
            {
                // Get file names from settings file
                categoryPath = generalSettingsGetter.GetStringSetting("CategoryPath");
                bankUrl = generalSettingsGetter.GetTextFileStringSetting("BankUrl");

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

        #endregion

        #region Koppling av data till UI

        private void EntriesFromFileLoadedOk(bool clearContentBeforeReadingNewFile)
        {
            budgeterFormHelper.EntriesFromFileLoadedOk(clearContentBeforeReadingNewFile);
            DisplayEntriesInUiGrids();
        }

        private void DisplayEntriesInUiGrids()
        {
            // Lägg in det som är satt att sparas till minnet
            // (viasa alla _kontoEntries i listview). Även uppdatera färg på text.
            ViewUpdateUi.ClearListAndSetEntriesToListView(
                entriesInToBeSavedGrid,
                kontoEntriesHolder.KontoEntries);

            // Lägg till orginalraden, gör i UI-hanterare
            //ViewUpdateUi.ClearListAndSetEntriesToListView(
            //    xlsOrginalEntriesGrid,
            //    kontoEntriesHolder.KontoEntries);
        }

        private void CheckAndAddNewItems(bool okToAddFromOld = false)
        {
            var lists = new KontoEntriesViewModelListUpdater
            {
                KontoEntries = kontoEntriesHolder.KontoEntries,
                NewItemsListEdited = newIitemsListEditedGrid.ItemsAsKontoEntries,
                NewKontoEntriesIn = kontoEntriesHolder.NewKontoEntries,
            };

            budgeterFormHelper.CheckAndAddNewItems(
                new KontoEntriesChecker(
                    lists,
                    okToAddFromOld),
                newIitemsListEditedGrid.ItemsAsKontoEntries);

            // Lägg till i edited
            ViewUpdateUi.AddEntriesToListView(
                newIitemsListEditedGrid,
                lists.ToAddToListview);

            // Updatera memlistan för att se om någon entry fått ny färg
            DisplayEntriesInUiGrids();
        }

        #endregion

        #region Funktioner

        private static string GetBankUrl()
        {
            return
                //@"C:\Files\Dropbox\budget\Program\TestData\x.html"
                bankUrl
                ;
        }

        private void CheckIfUserWantsToSaveUnsavedChanges(FormClosingEventArgs e)
        {
            if (budgeterFormHelper.UserSaveQuestionResultedInCancel(
                toolStripStatusLabel1.Text))
            {
                e.Cancel = true;
            }
        }

        #region Har med UIobjekt i denna klass att göra

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

        internal void AddNewEntriesToUiListsAndMem()
        {
            AddNewEntriesToUiListsAndMem(menuItemAutoSaveCheck.Checked);
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

        private void SetStatusLabelProps()
        {
            toolStripStatusLabel1 = new ToolStripStatusLabel();

            // TODO: Lägg alla UI-element i egen partial fil. Typ: ...Custom-elements.cs

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
            var changeInfo = changeInfoHandler.AddNewEntries(newEntriesFromUi);

            budgeterFormHelper.CheckIfSomethingWasChanged(changeInfo);

            DisplayEntriesInUiGrids();

            WriteToUiStatusLog(@"Entries in memory updated. " +
                               @"Added entries; " + changeInfo.Added + ". " +
                               @"Replaced entries; " + changeInfo.Replaced);

            if (autoSave)
                budgeterFormHelper.Save(toolStripStatusLabel1.Text);
        }

        private static SortedList GetNewEntriesFromUI(ListView mineNewIitemsListEdited)
        {
            // For performance
            mineNewIitemsListEdited.BeginUpdate();

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

            // For performance
            mineNewIitemsListEdited.EndUpdate();

            return newEntriesFromUi;
        }

        #endregion

        #region Test&Debug

        // TODO: Rensa all debug och commita i enskild commit. Ta fram i framtiden om det behövs då...

        private bool DebugModeOff()
        {
            #region Debug

            if (Debug())
            {
                debugbtn.Visible = true;
                DebugAddoNewList();
                return false;
            }
            else
            {
                return true;
            }

            #endregion
        }

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

        private void DebugToolStripMenuItemClick(object sender, EventArgs e)
        {
            webBrowser1.Load(
                "https://secure.handelsbanken.se"
                + "/bb/seip/servlet/UASipko?appAction=ShowAccountOverview&appName=ipko");
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
                kontoEntriesHolder.NewKontoEntries.Add(testKey,
                    new KontoEntry { Date = DateTime.Now.AddDays(i), Info = newInfo });
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