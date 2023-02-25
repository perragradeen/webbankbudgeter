using Budgetterarn.InternalUtilities;
using CefSharp;
using CefSharp.WinForms;

namespace BudgetterarnUi
{
    public partial class BudgeterGui
    {
        private KontoEntryListView entriesInToBeSavedGrid;

        private ListViewWithComboBox newIitemsListEditedGrid;
        private ChromiumWebBrowser webBrowser1;

        private void InitSpecialGenericUiElements()
        {
            newIitemsListEditedGrid = new ListViewWithComboBox();
            //newIitemsListOrgGrid = new KontoEntryListView();
            entriesInToBeSavedGrid = new KontoEntryListView();
            //xlsOrginalEntriesGrid = new KontoEntryListView();

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
            //m_originalXls.Controls.Add(xlsOrginalEntriesGrid);
            m_originalXls.Location = new Point(4, 22);
            m_originalXls.Name = "m_originalXls";
            m_originalXls.Padding = new Padding(3);
            m_originalXls.Size = new Size(294, 583);
            m_originalXls.TabIndex = 0;
            m_originalXls.Text = @"Xls Original";
            m_originalXls.UseVisualStyleBackColor = true;

            // m_XlsOrginalEntries
            //xlsOrginalEntriesGrid.Dock = DockStyle.Fill;
            //xlsOrginalEntriesGrid.FullRowSelect = true;
            //xlsOrginalEntriesGrid.GridLines = true;
            //xlsOrginalEntriesGrid.Location = new Point(3, 3);
            //xlsOrginalEntriesGrid.Name = "m_XlsOrginalEntries";
            //xlsOrginalEntriesGrid.Size = new Size(288, 577);
            //xlsOrginalEntriesGrid.TabIndex = 0;
            //xlsOrginalEntriesGrid.UseCompatibleStateImageBehavior = false;
            //xlsOrginalEntriesGrid.View = View.Details;

            entriesInToBeSavedGrid.ListViewItemSorter = new ListViewColumnSorter();
            //xlsOrginalEntriesGrid.ListViewItemSorter = new ListViewColumnSorter();
            newIitemsListEditedGrid.ListViewItemSorter = new ListViewColumnSorter();
            //newIitemsListOrgGrid.ListViewItemSorter = new ListViewColumnSorter();
        }

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
    }
}
