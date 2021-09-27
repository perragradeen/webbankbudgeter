using Budgetterarn.InternalUtilities;
using System;
using System.Windows.Forms;
using CategoryHandler;
using CefSharp;

namespace Budgetterarn
{
    public partial class BudgeterForm
    {
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

        private void LoadCurrentVisibleEntries_LoadMenuClick(object sender, EventArgs e)
        {
            LoadCurrentEntriesFromBrowser(); // MenuItemClick
        }

        private void LoadOldEntries_LoadMenuClick(object sender, EventArgs e)
        {
            budgeterFormHelper.LoadOldEntries();

            //TOOD: Move
            CheckAndAddNewItems(true); // Lägg till gamla i GuiLista för redigering

            budgeterFormHelper.CountNewKontoEntries();
        }

        private void BtnLoadCurrentEntriesClick(object sender, EventArgs e)
        {
            LoadCurrentEntriesFromBrowser(); // BtnClick
        }

        private void LoadCurrentEntriesFromBrowser()
        {
            budgeterFormHelper.LoadCurrentEntriesFromBrowser(webBrowser1.GetTextAsync().Result);

        }

        private void FormIsClosing(object sender, FormClosingEventArgs e)
        {
            if (debugGlobal) return;

            CheckIfUserWantsToSaveUnsavedChanges(e);
        }

        private void OpenBankSiteToolStripMenuItemClick(object sender, EventArgs e)
        {
            OpenBankSiteInBrowser();
        }

        private void SaveToolStripMenuItemClick(object sender, EventArgs e)
        {
            budgeterFormHelper.Save(toolStripStatusLabel1.Text);
        }

        private void AddNewToMemClick(object sender, EventArgs e)
        {
            AddNewEntriesToUiListsAndMem();
        }

        private void MbClearNewOnesClick(object sender, EventArgs e)
        {
            budgeterFormHelper.ClearNewOnes(ClearNewOnesFnc);
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

        #endregion
    }
}
