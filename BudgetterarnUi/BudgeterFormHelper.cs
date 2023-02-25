using Budgeter.Core.Entities;
using Budgetterarn.Application_Settings_and_constants;
using Budgetterarn.DAL;
using Budgetterarn.EntryLogicSetFlags;
using Budgetterarn.InternalUtilities;
using GeneralSettingsHandler;
using LoadTransactionsFromFile;
using System.Collections;
using Utilities;

namespace Budgetterarn
{
    public class BudgeterFormHelper
    {
        private readonly Action<string> writeToOutput;
        private readonly Action<string> writeToUiStatusLog;
        private readonly Action<bool> checkAndAddNewItems;
        private readonly KontoEntriesHolder kontoEntriesHolder;
        private bool somethingChanged;

        private KontoutdragExcelFileInfo KontoutdragExcelFileInfo { get; }

        public BudgeterFormHelper(
            Action<string> writeToOutput
            , Action<string> writeToUiStatusLog
            , Action<bool> checkAndAddNewItems
            , KontoEntriesHolder kontoEntriesHolder
            , GeneralSettingsGetter generalSettingsGetter)
        {
            this.writeToOutput = writeToOutput;
            this.writeToUiStatusLog = writeToUiStatusLog;
            this.checkAndAddNewItems = checkAndAddNewItems;
            this.kontoEntriesHolder = kontoEntriesHolder;

            KontoutdragExcelFileInfo = GetExcelFileReferences(generalSettingsGetter);
        }

        private KontoutdragExcelFileInfo GetExcelFileReferences(
            GeneralSettingsGetter generalSettingsGetter)
        {
            var fileReferences = new FileReferences(generalSettingsGetter);

            return new KontoutdragExcelFileInfo
            {
                ExcelFileSaveFileName = fileReferences.ExcelFileSaveFileName,
                ExcelFileSavePath = fileReferences.ExcelFileSavePath,
                ExcelFileSavePathWithoutFileName =
                      fileReferences.ExcelFileSavePathWithoutFileName,
                SheetName = FileReferences.SheetName
            };
        }

        internal void LoadOldEntries()
        {
            // Sätt de gamla inlästa transaktionerna i minnet in i nya lista för redigering av kategori
            kontoEntriesHolder.NewKontoEntries = GetOldEntriesWithoutCategory();

            //CheckAndAddNewItems(true); // Lägg till gamla i GuiLista för redigering

            //somethingChanged = kontoEntriesHolder.NewKontoEntries.Count > 0;
        }

        internal void CountNewKontoEntries()
        {
            somethingChanged = kontoEntriesHolder.NewKontoEntries.Count > 0;
        }

        /// <summary>Uppdatera UI för nya entries, gör gisningar av dubbletter, typ av kostnad etc
        /// </summary>
        internal void CheckAndAddNewItems(
            KontoEntriesChecker kontoEntriesChecker,
            List<KontoEntry> itemsAsKontoEntries)
        {
            // Flagga och se vad som är nytt etc.
            kontoEntriesChecker.CheckAndAddNewItemsForLists();

            //// Lägg till i org
            //lists.NewItemsListOrg.ForEach(k =>
            //    ViewUpdateUi.AddToListview(newIitemsListOrgGrid, k));

            // Filtrera ut de som inte redan ligger i UI
            var inUiListAlready = itemsAsKontoEntries;
            kontoEntriesChecker.AddInUiListAlreadyToAddList(inUiListAlready);

            kontoEntriesChecker.CheckSkyddatBelopp(kontoEntriesHolder);
        }

        internal void Save(string uiText)
        {
            var saveResult = SaveKonton.Save(
                KontoutdragExcelFileInfo,
                kontoEntriesHolder,
                writeToOutput);

            somethingChanged = saveResult.SomethingLoadedOrSaved;

            // Räkna inte överskriften, den skrivs alltid om

            CheckIfUserWantsToOpenExcel(KontoutdragExcelFileInfo);

            //Precis sparat, så här har inget hunnit ändras 
            var statusText = uiText
                             + " Saving done, saved entries; "
                             + saveResult.SkippedOrSaved;
            writeToUiStatusLog(statusText);
        }

        internal void LoadCurrentEntriesFromBrowser(string text)
        {
            writeToUiStatusLog(@"Processing");

            try
            {

                var loadFromWeb = new LoadKontonFromWebBrowser(kontoEntriesHolder);
                var somethingLoadeded = loadFromWeb.GetAllVisibleEntriesFromWebBrowser(text);

                // Meddela på nåt sätt att det är klart, och antal inlästa, i tex. statusbar
                writeToUiStatusLog(@"Done processing  no new entries fond from html.");

                if (!somethingLoadeded) return;

                checkAndAddNewItems(false); // Lägg till nya i GuiLista FromBrowser
                writeToUiStatusLog(@"Done processing entries from html. New Entries found; "
                                   + kontoEntriesHolder.NewKontoEntries.Count
                                   + @".");
            }
            catch (Exception e)
            {
                writeToUiStatusLog(@"Error! " + e.Message);
            }
        }

        internal bool UserSaveQuestionResultedInCancel(string uiText)
        {

            var saveCheckResults = SaveCheckResults();
            if (saveCheckResults == DialogResult.Cancel)
            {
                return true;
            }
            else if (saveCheckResults == DialogResult.Yes)
            {
                Save(uiText);
            }

            return false;
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
        internal void EntriesFromFileLoadedOk(bool clearContentBeforeReadingNewFile)
        {
            if (clearContentBeforeReadingNewFile)
                ClearUiContents();

            CheckFileIfEmptyPromptUserIfEmptyPath();

            LoadFromFileHelper.SetEntriesFromFile();
        }

        internal void ClearNewOnes(Action ClearNewOnesFnc)
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

        internal void CheckIfSomethingWasChanged(AddedAndReplacedEntriesCounter changeInfo)
        {
            somethingChanged = CheckIfSomethingWasChanged(
                somethingChanged,
                changeInfo.SomethingChanged);
        }

        internal DialogResult SaveCheckResults()
        {
            return WinFormsChecks.SaveCheck(somethingChanged);
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

        private LoadFromFileHelper LoadFromFileHelper =>
            new LoadFromFileHelper(
                KontoutdragExcelFileInfo,
                kontoEntriesHolder,
                writeToOutput,
                writeToUiStatusLog);

        private void ClearUiContents()
        {
            // Töm alla tidigare entries i minnet om det ska laddas
            // helt ny fil el. likn. 
            kontoEntriesHolder.KontoEntries.Clear();
        }

        private void CheckFileIfEmptyPromptUserIfEmptyPath()
        {
            if (FilePathAlreadySet) return;

            // Öppnar dialog
            var filePath = FileOperations.OpenFileOfType(writeToOutput);

            // Ev. har pathen ändrats.
            // Har man däremot laddat in nya så ska den sökvägen gälla för sparningar
            KontoutdragExcelFileInfo.ExcelFileSavePath =
                filePath;
        }

        private bool FilePathAlreadySet => !string.IsNullOrWhiteSpace(
            KontoutdragExcelFileInfo.ExcelFileSavePath);

        private static bool CheckIfSomethingWasChanged(
            bool oldSomethingChanged,
            bool newSomethingChanged)
        {
            return oldSomethingChanged || newSomethingChanged;
        }

        private static void CheckIfUserWantsToOpenExcel(
            KontoutdragExcelFileInfo kontoutdragExcelFileInfo)
        {
            // Fråga om man vill öppna Excel
            var question = @"Open budget file (wait a litte while first)?";
            var userWantsToOpen = MessageBox.Show(
                question,
                @"Open file",
                MessageBoxButtons.YesNo);

            if (userWantsToOpen == DialogResult.Yes)
            {
                ExcelOpener.LoadExcelFileInExcel(
                    kontoutdragExcelFileInfo.ExcelFileSavePath);
            }
        }

    }
}