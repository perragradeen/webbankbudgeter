using System.Windows.Forms;

namespace Budgetterarn
{
    partial class BudgeterForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._MainMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.FileMenuLoadNewFromXls = new System.Windows.Forms.ToolStripMenuItem();
            this.openBankSiteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openUrlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.navigeraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setLoginToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.navigateToLöneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testNav1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testBackNavToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadCurrentEntriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemAutoSaveCheck = new System.Windows.Forms.ToolStripMenuItem();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.m_inMemoryList = new System.Windows.Forms.TabPage();
            this.m_originalXls = new System.Windows.Forms.TabPage();
            this.InfoNewEntries = new System.Windows.Forms.Label();
            this.m_b_ClearNewOnes = new System.Windows.Forms.Button();
            this.m_newItemsTab = new System.Windows.Forms.TabControl();
            this.tp_NewItemsEdited = new System.Windows.Forms.TabPage();
            this.tp_NewItemsOrg = new System.Windows.Forms.TabPage();
            this.btn_RecheckAutocat = new System.Windows.Forms.Button();
            this.m_b_AddNewToMem = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.c_Date = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.c_Info = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.c_KostnadEllerInkomst = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.c_SaldoOrginal = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.c_AckumuleratSaldo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.c_TypAvKostnad = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.debugbtn = new System.Windows.Forms.Button();
            this.btnLoadCurrentEntries = new System.Windows.Forms.Button();
            this._MainMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.m_newItemsTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // _MainMenu
            // 
            this._MainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.testToolStripMenuItem,
            this.loadToolStripMenuItem,
            this.optionsToolStripMenuItem});
            this._MainMenu.Location = new System.Drawing.Point(0, 0);
            this._MainMenu.Name = "_MainMenu";
            this._MainMenu.Size = new System.Drawing.Size(1284, 24);
            this._MainMenu.TabIndex = 2;
            this._MainMenu.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem1,
            this.FileMenuLoadNewFromXls,
            this.openBankSiteToolStripMenuItem,
            this.openUrlToolStripMenuItem,
            this.saveToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadToolStripMenuItem1
            // 
            this.loadToolStripMenuItem1.Name = "loadToolStripMenuItem1";
            this.loadToolStripMenuItem1.Size = new System.Drawing.Size(197, 22);
            this.loadToolStripMenuItem1.Text = "Load new entries from xls";
            this.loadToolStripMenuItem1.Click += new System.EventHandler(this.LoadToolStripMenuItem1Click);
            // 
            // FileMenuLoadNewFromXls
            // 
            this.FileMenuLoadNewFromXls.Name = "FileMenuLoadNewFromXls";
            this.FileMenuLoadNewFromXls.Size = new System.Drawing.Size(197, 22);
            this.FileMenuLoadNewFromXls.Text = "Add new entries from xls";
            this.FileMenuLoadNewFromXls.Click += new System.EventHandler(this.FileMenuLoadNewFromXlsClick);
            // 
            // openBankSiteToolStripMenuItem
            // 
            this.openBankSiteToolStripMenuItem.Name = "openBankSiteToolStripMenuItem";
            this.openBankSiteToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.openBankSiteToolStripMenuItem.Text = "Open bank site";
            this.openBankSiteToolStripMenuItem.Click += new System.EventHandler(this.OpenBankSiteToolStripMenuItemClick);
            // 
            // openUrlToolStripMenuItem
            // 
            this.openUrlToolStripMenuItem.Name = "openUrlToolStripMenuItem";
            this.openUrlToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.openUrlToolStripMenuItem.Text = "Open Url";
            this.openUrlToolStripMenuItem.Click += new System.EventHandler(this.OpenUrlToolStripMenuItemClick);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.SaveToolStripMenuItemClick);
            // 
            // testToolStripMenuItem
            // 
            this.testToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.debugToolStripMenuItem,
            this.navigeraToolStripMenuItem,
            this.setLoginToolStripMenuItem,
            this.navigateToLöneToolStripMenuItem,
            this.testNav1ToolStripMenuItem,
            this.testBackNavToolStripMenuItem});
            this.testToolStripMenuItem.Name = "testToolStripMenuItem";
            this.testToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.testToolStripMenuItem.Text = "Test";
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.debugToolStripMenuItem.Text = "Debug";
            this.debugToolStripMenuItem.Click += new System.EventHandler(this.DebugToolStripMenuItemClick);
            // 
            // navigeraToolStripMenuItem
            // 
            this.navigeraToolStripMenuItem.Name = "navigeraToolStripMenuItem";
            this.navigeraToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.navigeraToolStripMenuItem.Text = "Navigera";
            this.navigeraToolStripMenuItem.Click += new System.EventHandler(this.NavigeraToolStripMenuItemClick);
            // 
            // setLoginToolStripMenuItem
            // 
            this.setLoginToolStripMenuItem.Name = "setLoginToolStripMenuItem";
            this.setLoginToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.setLoginToolStripMenuItem.Text = "Set Login";
            this.setLoginToolStripMenuItem.Click += new System.EventHandler(this.SetLoginToolStripMenuItemClick);
            // 
            // navigateToLöneToolStripMenuItem
            // 
            this.navigateToLöneToolStripMenuItem.Name = "navigateToLöneToolStripMenuItem";
            this.navigateToLöneToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.navigateToLöneToolStripMenuItem.Text = "NavigateToLöne";
            this.navigateToLöneToolStripMenuItem.Click += new System.EventHandler(this.NavigateToLöneToolStripMenuItemClick);
            // 
            // testNav1ToolStripMenuItem
            // 
            this.testNav1ToolStripMenuItem.Name = "testNav1ToolStripMenuItem";
            this.testNav1ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.testNav1ToolStripMenuItem.Text = "Test nav1";
            this.testNav1ToolStripMenuItem.Click += new System.EventHandler(this.TestNav1ToolStripMenuItemClick);
            // 
            // testBackNavToolStripMenuItem
            // 
            this.testBackNavToolStripMenuItem.Name = "testBackNavToolStripMenuItem";
            this.testBackNavToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.testBackNavToolStripMenuItem.Text = "TestBackNav";
            this.testBackNavToolStripMenuItem.Click += new System.EventHandler(this.TestBackNavToolStripMenuItemClick);
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadCurrentEntriesToolStripMenuItem});
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(42, 20);
            this.loadToolStripMenuItem.Text = "Load";
            // 
            // loadCurrentEntriesToolStripMenuItem
            // 
            this.loadCurrentEntriesToolStripMenuItem.Name = "loadCurrentEntriesToolStripMenuItem";
            this.loadCurrentEntriesToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.loadCurrentEntriesToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.loadCurrentEntriesToolStripMenuItem.Text = "Load current entries";
            this.loadCurrentEntriesToolStripMenuItem.Click += new System.EventHandler(this.LoadCurrentEntriesToolStripMenuItemClick);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemAutoSaveCheck});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // menuItemAutoSaveCheck
            // 
            this.menuItemAutoSaveCheck.Checked = true;
            this.menuItemAutoSaveCheck.CheckOnClick = true;
            this.menuItemAutoSaveCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.menuItemAutoSaveCheck.Name = "menuItemAutoSaveCheck";
            this.menuItemAutoSaveCheck.Size = new System.Drawing.Size(170, 22);
            this.menuItemAutoSaveCheck.Text = "Autosave when add";
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(102, 609);
            this.webBrowser1.TabIndex = 0;
            this.webBrowser1.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.WebBrowser1DocumentCompleted);
            // this.webBrowser1.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(this.webBrowser1_Navigated);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 27);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.webBrowser1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1281, 609);
            this.splitContainer1.SplitterDistance = 102;
            this.splitContainer1.TabIndex = 1;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.tabControl1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.InfoNewEntries);
            this.splitContainer2.Panel2.Controls.Add(this.m_b_ClearNewOnes);
            this.splitContainer2.Panel2.Controls.Add(this.m_newItemsTab);
            this.splitContainer2.Panel2.Controls.Add(this.btn_RecheckAutocat);
            this.splitContainer2.Panel2.Controls.Add(this.m_b_AddNewToMem);
            this.splitContainer2.Size = new System.Drawing.Size(1175, 609);
            this.splitContainer2.SplitterDistance = 302;
            this.splitContainer2.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.m_inMemoryList);
            this.tabControl1.Controls.Add(this.m_originalXls);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(302, 609);
            this.tabControl1.TabIndex = 0;
            // 
            // m_inMemoryList
            // 
            this.m_inMemoryList.Location = new System.Drawing.Point(4, 22);
            this.m_inMemoryList.Name = "m_inMemoryList";
            this.m_inMemoryList.Padding = new System.Windows.Forms.Padding(3);
            this.m_inMemoryList.Size = new System.Drawing.Size(294, 583);
            this.m_inMemoryList.TabIndex = 1;
            this.m_inMemoryList.Text = "Memory";
            this.m_inMemoryList.UseVisualStyleBackColor = true;
            // 
            // m_originalXls
            // 
            this.m_originalXls.Location = new System.Drawing.Point(4, 22);
            this.m_originalXls.Name = "m_originalXls";
            this.m_originalXls.Size = new System.Drawing.Size(294, 583);
            this.m_originalXls.TabIndex = 2;
            // 
            // InfoNewEntries
            // 
            this.InfoNewEntries.AutoSize = true;
            this.InfoNewEntries.Location = new System.Drawing.Point(315, 8);
            this.InfoNewEntries.Name = "InfoNewEntries";
            this.InfoNewEntries.Size = new System.Drawing.Size(551, 13);
            this.InfoNewEntries.TabIndex = 6;
            this.InfoNewEntries.Text = "To delete one; select it and press del. To autocategorize press a. s = Set severa" +
                "l at once, all selected get the same.";
            // 
            // m_b_ClearNewOnes
            // 
            this.m_b_ClearNewOnes.Location = new System.Drawing.Point(116, 3);
            this.m_b_ClearNewOnes.Name = "m_b_ClearNewOnes";
            this.m_b_ClearNewOnes.Size = new System.Drawing.Size(61, 23);
            this.m_b_ClearNewOnes.TabIndex = 5;
            this.m_b_ClearNewOnes.Text = "Clear All";
            this.m_b_ClearNewOnes.UseVisualStyleBackColor = true;
            this.m_b_ClearNewOnes.Click += new System.EventHandler(this.MbClearNewOnesClick);
            // 
            // m_newItemsTab
            // 
            this.m_newItemsTab.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.m_newItemsTab.Controls.Add(this.tp_NewItemsEdited);
            this.m_newItemsTab.Controls.Add(this.tp_NewItemsOrg);
            this.m_newItemsTab.Location = new System.Drawing.Point(0, 32);
            this.m_newItemsTab.Name = "m_newItemsTab";
            this.m_newItemsTab.SelectedIndex = 0;
            this.m_newItemsTab.Size = new System.Drawing.Size(1169, 577);
            this.m_newItemsTab.TabIndex = 0;
            // 
            // tp_NewItemsEdited
            // 
            this.tp_NewItemsEdited.Location = new System.Drawing.Point(4, 22);
            this.tp_NewItemsEdited.Name = "tp_NewItemsEdited";
            this.tp_NewItemsEdited.Size = new System.Drawing.Size(1161, 551);
            this.tp_NewItemsEdited.TabIndex = 0;
            // 
            // tp_NewItemsOrg
            // 
            this.tp_NewItemsOrg.Location = new System.Drawing.Point(4, 22);
            this.tp_NewItemsOrg.Name = "tp_NewItemsOrg";
            this.tp_NewItemsOrg.Size = new System.Drawing.Size(1161, 551);
            this.tp_NewItemsOrg.TabIndex = 1;
            // 
            // btn_RecheckAutocat
            // 
            this.btn_RecheckAutocat.Location = new System.Drawing.Point(183, 3);
            this.btn_RecheckAutocat.Name = "btn_RecheckAutocat";
            this.btn_RecheckAutocat.Size = new System.Drawing.Size(126, 23);
            this.btn_RecheckAutocat.TabIndex = 4;
            this.btn_RecheckAutocat.Text = "Re check w auto cats";
            this.btn_RecheckAutocat.UseVisualStyleBackColor = true;
            this.btn_RecheckAutocat.Click += new System.EventHandler(this.BtnRecheckAutocatClick);
            // 
            // m_b_AddNewToMem
            // 
            this.m_b_AddNewToMem.Location = new System.Drawing.Point(4, 3);
            this.m_b_AddNewToMem.Name = "m_b_AddNewToMem";
            this.m_b_AddNewToMem.Size = new System.Drawing.Size(106, 23);
            this.m_b_AddNewToMem.TabIndex = 4;
            this.m_b_AddNewToMem.Text = "Add new to mem";
            this.m_b_AddNewToMem.UseVisualStyleBackColor = true;
            this.m_b_AddNewToMem.Click += new System.EventHandler(this.AddNewToMemClick);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(3, 3);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(288, 577);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // c_Date
            // 
            this.c_Date.Text = "Date";
            this.c_Date.Width = 62;
            // 
            // c_Info
            // 
            this.c_Info.Text = "Info";
            this.c_Info.Width = 85;
            // 
            // c_KostnadEllerInkomst
            // 
            this.c_KostnadEllerInkomst.Text = "KostnadEllerInkomst";
            this.c_KostnadEllerInkomst.Width = 79;
            // 
            // c_SaldoOrginal
            // 
            this.c_SaldoOrginal.Text = "SaldoOrginal";
            this.c_SaldoOrginal.Width = 75;
            // 
            // c_AckumuleratSaldo
            // 
            this.c_AckumuleratSaldo.Text = "AckumuleratSaldo";
            this.c_AckumuleratSaldo.Width = 75;
            // 
            // c_TypAvKostnad
            // 
            this.c_TypAvKostnad.Text = "TypAvKostnad";
            this.c_TypAvKostnad.Width = 192;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 644);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1284, 22);
            this.statusStrip1.TabIndex = 5;
            // 
            // debugbtn
            // 
            this.debugbtn.Location = new System.Drawing.Point(876, 1);
            this.debugbtn.Name = "debugbtn";
            this.debugbtn.Size = new System.Drawing.Size(106, 23);
            this.debugbtn.TabIndex = 4;
            this.debugbtn.Text = "Debug add new";
            this.debugbtn.UseVisualStyleBackColor = true;
            this.debugbtn.Visible = false;
            this.debugbtn.Click += new System.EventHandler(this.DebugbtnClick);
            // 
            // btnLoadCurrentEntries
            // 
            this.btnLoadCurrentEntries.Location = new System.Drawing.Point(215, 1);
            this.btnLoadCurrentEntries.Name = "btnLoadCurrentEntries";
            this.btnLoadCurrentEntries.Size = new System.Drawing.Size(146, 23);
            this.btnLoadCurrentEntries.TabIndex = 4;
            this.btnLoadCurrentEntries.Text = "Load current entries";
            this.btnLoadCurrentEntries.UseVisualStyleBackColor = true;
            this.btnLoadCurrentEntries.Click += new System.EventHandler(this.BtnLoadCurrentEntriesClick);
            // 
            // Budgeter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1284, 666);
            this.Controls.Add(this.btnLoadCurrentEntries);
            this.Controls.Add(this.debugbtn);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this._MainMenu);
            this.Controls.Add(this.splitContainer1);
            this.MainMenuStrip = this._MainMenu;
            this.Name = "Budgeter";
            this.Text = "The Budgeter, version ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BudgeterFormClosing);
            this._MainMenu.ResumeLayout(false);
            this._MainMenu.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.m_newItemsTab.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip _MainMenu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadCurrentEntriesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem openBankSiteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage m_originalXls;
        private System.Windows.Forms.TabPage m_inMemoryList;
        private System.Windows.Forms.TabControl m_newItemsTab;
        private System.Windows.Forms.TabPage tp_NewItemsEdited;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private Button m_b_AddNewToMem;
        private ColumnHeader c_Date;
        private ColumnHeader c_Info;
        private ColumnHeader c_KostnadEllerInkomst;
        private ColumnHeader c_SaldoOrginal;
        private ColumnHeader c_AckumuleratSaldo;
        private ColumnHeader c_TypAvKostnad;
        private TabPage tp_NewItemsOrg;
        private Button m_b_ClearNewOnes;
        private ToolStripMenuItem FileMenuLoadNewFromXls;
        private Label InfoNewEntries;
        private Button debugbtn;
        private Button btn_RecheckAutocat;
        private ToolStripMenuItem openUrlToolStripMenuItem;
        private Button btnLoadCurrentEntries;
        private ToolStripMenuItem optionsToolStripMenuItem;
        private ToolStripMenuItem menuItemAutoSaveCheck;
        private ToolStripMenuItem navigeraToolStripMenuItem;
        private ToolStripMenuItem setLoginToolStripMenuItem;
        private ToolStripMenuItem navigateToLöneToolStripMenuItem;
        private ToolStripMenuItem testNav1ToolStripMenuItem;
        private ToolStripMenuItem testBackNavToolStripMenuItem;
    }
}


