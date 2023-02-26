namespace WebBankBudgeterUi
{
    partial class WebBankBudgeterUi
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            var dataGridViewCellStyle2 = new DataGridViewCellStyle();
            gv_budget = new DataGridView();
            label1 = new Label();
            LogTexts = new RichTextBox();
            ReloadButton = new Button();
            BudgetTabs = new TabControl();
            tbl_Kvar = new TabPage();
            gv_Kvar = new DataGridView();
            tbl_INCOME = new TabPage();
            gv_incomes = new DataGridView();
            tbl_Budget = new TabPage();
            tbl_Totals = new TabPage();
            gv_Totals = new DataGridView();
            tbl_Transactions = new TabPage();
            dg_Transactions = new DataGridView();
            tbl_reccuringCosts = new TabPage();
            tbl_nonReccuringCosts = new TabPage();
            tbl_NoCategory = new TabPage();
            SaveInPosterButton = new Button();
            SkapaTomRad = new Button();
            label2 = new Label();
            txtYearFilter = new TextBox();
            ((System.ComponentModel.ISupportInitialize)gv_budget).BeginInit();
            BudgetTabs.SuspendLayout();
            tbl_Kvar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gv_Kvar).BeginInit();
            tbl_INCOME.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gv_incomes).BeginInit();
            tbl_Budget.SuspendLayout();
            tbl_Totals.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gv_Totals).BeginInit();
            tbl_Transactions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dg_Transactions).BeginInit();
            SuspendLayout();
            // 
            // gv_budget
            // 
            gv_budget.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            gv_budget.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            gv_budget.DefaultCellStyle = dataGridViewCellStyle2;
            gv_budget.Dock = DockStyle.Fill;
            gv_budget.Location = new Point(4, 3);
            gv_budget.Margin = new Padding(4, 3, 4, 3);
            gv_budget.Name = "gv_budget";
            gv_budget.RowTemplate.Resizable = DataGridViewTriState.True;
            gv_budget.Size = new Size(1005, 539);
            gv_budget.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(28, 15);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(34, 15);
            label1.TabIndex = 1;
            label1.Text = "Info: ";
            // 
            // LogTexts
            // 
            LogTexts.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            LogTexts.Location = new Point(31, 696);
            LogTexts.Margin = new Padding(4, 3, 4, 3);
            LogTexts.Name = "LogTexts";
            LogTexts.Size = new Size(1015, 27);
            LogTexts.TabIndex = 2;
            LogTexts.Text = "";
            // 
            // ReloadButton
            // 
            ReloadButton.Location = new Point(31, 61);
            ReloadButton.Margin = new Padding(4, 3, 4, 3);
            ReloadButton.Name = "ReloadButton";
            ReloadButton.Size = new Size(88, 27);
            ReloadButton.TabIndex = 3;
            ReloadButton.Text = "ReLoad";
            ReloadButton.UseVisualStyleBackColor = true;
            // 
            // BudgetTabs
            // 
            BudgetTabs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            BudgetTabs.Controls.Add(tbl_Kvar);
            BudgetTabs.Controls.Add(tbl_INCOME);
            BudgetTabs.Controls.Add(tbl_Budget);
            BudgetTabs.Controls.Add(tbl_Totals);
            BudgetTabs.Controls.Add(tbl_Transactions);
            BudgetTabs.Controls.Add(tbl_reccuringCosts);
            BudgetTabs.Controls.Add(tbl_nonReccuringCosts);
            BudgetTabs.Controls.Add(tbl_NoCategory);
            BudgetTabs.Location = new Point(31, 115);
            BudgetTabs.Margin = new Padding(4, 3, 4, 3);
            BudgetTabs.Name = "BudgetTabs";
            BudgetTabs.SelectedIndex = 0;
            BudgetTabs.Size = new Size(1021, 573);
            BudgetTabs.TabIndex = 4;
            // 
            // tbl_Kvar
            // 
            tbl_Kvar.Controls.Add(gv_Kvar);
            tbl_Kvar.Location = new Point(4, 24);
            tbl_Kvar.Margin = new Padding(4, 3, 4, 3);
            tbl_Kvar.Name = "tbl_Kvar";
            tbl_Kvar.Padding = new Padding(4, 3, 4, 3);
            tbl_Kvar.Size = new Size(1013, 545);
            tbl_Kvar.TabIndex = 7;
            tbl_Kvar.Text = "Kvar";
            tbl_Kvar.UseVisualStyleBackColor = true;
            // 
            // gv_Kvar
            // 
            gv_Kvar.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gv_Kvar.Dock = DockStyle.Fill;
            gv_Kvar.Location = new Point(4, 3);
            gv_Kvar.Margin = new Padding(4, 3, 4, 3);
            gv_Kvar.Name = "gv_Kvar";
            gv_Kvar.Size = new Size(1005, 539);
            gv_Kvar.TabIndex = 0;
            // 
            // tbl_INCOME
            // 
            tbl_INCOME.Controls.Add(gv_incomes);
            tbl_INCOME.Location = new Point(4, 24);
            tbl_INCOME.Margin = new Padding(4, 3, 4, 3);
            tbl_INCOME.Name = "tbl_INCOME";
            tbl_INCOME.Size = new Size(1013, 545);
            tbl_INCOME.TabIndex = 2;
            tbl_INCOME.Text = "Incomes";
            tbl_INCOME.UseVisualStyleBackColor = true;
            // 
            // gv_incomes
            // 
            gv_incomes.AllowDrop = true;
            gv_incomes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gv_incomes.Dock = DockStyle.Fill;
            gv_incomes.Location = new Point(0, 0);
            gv_incomes.Margin = new Padding(4, 3, 4, 3);
            gv_incomes.Name = "gv_incomes";
            gv_incomes.Size = new Size(1013, 545);
            gv_incomes.TabIndex = 0;
            // 
            // tbl_Budget
            // 
            tbl_Budget.Controls.Add(gv_budget);
            tbl_Budget.Location = new Point(4, 24);
            tbl_Budget.Margin = new Padding(4, 3, 4, 3);
            tbl_Budget.Name = "tbl_Budget";
            tbl_Budget.Padding = new Padding(4, 3, 4, 3);
            tbl_Budget.Size = new Size(1013, 545);
            tbl_Budget.TabIndex = 0;
            tbl_Budget.Text = "Budget Total";
            tbl_Budget.UseVisualStyleBackColor = true;
            // 
            // tbl_Totals
            // 
            tbl_Totals.Controls.Add(gv_Totals);
            tbl_Totals.Location = new Point(4, 24);
            tbl_Totals.Margin = new Padding(4, 3, 4, 3);
            tbl_Totals.Name = "tbl_Totals";
            tbl_Totals.Size = new Size(1013, 545);
            tbl_Totals.TabIndex = 6;
            tbl_Totals.Text = "Totals";
            tbl_Totals.UseVisualStyleBackColor = true;
            // 
            // gv_Totals
            // 
            gv_Totals.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gv_Totals.Dock = DockStyle.Fill;
            gv_Totals.Location = new Point(0, 0);
            gv_Totals.Margin = new Padding(4, 3, 4, 3);
            gv_Totals.Name = "gv_Totals";
            gv_Totals.RowTemplate.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gv_Totals.Size = new Size(1013, 545);
            gv_Totals.TabIndex = 0;
            // 
            // tbl_Transactions
            // 
            tbl_Transactions.Controls.Add(dg_Transactions);
            tbl_Transactions.Location = new Point(4, 24);
            tbl_Transactions.Margin = new Padding(4, 3, 4, 3);
            tbl_Transactions.Name = "tbl_Transactions";
            tbl_Transactions.Padding = new Padding(4, 3, 4, 3);
            tbl_Transactions.Size = new Size(1013, 545);
            tbl_Transactions.TabIndex = 1;
            tbl_Transactions.Text = "Transactions";
            tbl_Transactions.UseVisualStyleBackColor = true;
            // 
            // dg_Transactions
            // 
            dg_Transactions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dg_Transactions.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dg_Transactions.Dock = DockStyle.Fill;
            dg_Transactions.Location = new Point(4, 3);
            dg_Transactions.Margin = new Padding(4, 3, 4, 3);
            dg_Transactions.Name = "dg_Transactions";
            dg_Transactions.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dg_Transactions.Size = new Size(1005, 539);
            dg_Transactions.TabIndex = 0;
            // 
            // tbl_reccuringCosts
            // 
            tbl_reccuringCosts.Location = new Point(4, 24);
            tbl_reccuringCosts.Margin = new Padding(4, 3, 4, 3);
            tbl_reccuringCosts.Name = "tbl_reccuringCosts";
            tbl_reccuringCosts.Size = new Size(1013, 545);
            tbl_reccuringCosts.TabIndex = 4;
            tbl_reccuringCosts.Text = "Reccuring Costs";
            tbl_reccuringCosts.UseVisualStyleBackColor = true;
            // 
            // tbl_nonReccuringCosts
            // 
            tbl_nonReccuringCosts.Location = new Point(4, 24);
            tbl_nonReccuringCosts.Margin = new Padding(4, 3, 4, 3);
            tbl_nonReccuringCosts.Name = "tbl_nonReccuringCosts";
            tbl_nonReccuringCosts.Size = new Size(1013, 545);
            tbl_nonReccuringCosts.TabIndex = 5;
            tbl_nonReccuringCosts.Text = "Non Reccuring Costs";
            tbl_nonReccuringCosts.UseVisualStyleBackColor = true;
            // 
            // tbl_NoCategory
            // 
            tbl_NoCategory.Location = new Point(4, 24);
            tbl_NoCategory.Margin = new Padding(4, 3, 4, 3);
            tbl_NoCategory.Name = "tbl_NoCategory";
            tbl_NoCategory.Size = new Size(1013, 545);
            tbl_NoCategory.TabIndex = 3;
            tbl_NoCategory.Text = "No category";
            tbl_NoCategory.UseVisualStyleBackColor = true;
            // 
            // SaveInPosterButton
            // 
            SaveInPosterButton.Location = new Point(126, 61);
            SaveInPosterButton.Margin = new Padding(4, 3, 4, 3);
            SaveInPosterButton.Name = "SaveInPosterButton";
            SaveInPosterButton.Size = new Size(104, 27);
            SaveInPosterButton.TabIndex = 3;
            SaveInPosterButton.Text = "Spara InPoster";
            SaveInPosterButton.UseVisualStyleBackColor = true;
            SaveInPosterButton.Click += SaveInPosterButton_Click;
            // 
            // SkapaTomRad
            // 
            SkapaTomRad.Location = new Point(237, 61);
            SkapaTomRad.Margin = new Padding(4, 3, 4, 3);
            SkapaTomRad.Name = "SkapaTomRad";
            SkapaTomRad.Size = new Size(192, 27);
            SkapaTomRad.TabIndex = 3;
            SkapaTomRad.Text = "Skapa tom rad med Inposter";
            SkapaTomRad.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(466, 43);
            label2.Name = "label2";
            label2.Size = new Size(62, 15);
            label2.TabIndex = 5;
            label2.Text = "Filter på år";
            // 
            // txtYearFilter
            // 
            txtYearFilter.Location = new Point(466, 61);
            txtYearFilter.Name = "txtYearFilter";
            txtYearFilter.Size = new Size(100, 23);
            txtYearFilter.TabIndex = 6;
            // 
            // WebBankBudgeterUi
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1066, 737);
            Controls.Add(txtYearFilter);
            Controls.Add(label2);
            Controls.Add(SkapaTomRad);
            Controls.Add(SaveInPosterButton);
            Controls.Add(BudgetTabs);
            Controls.Add(LogTexts);
            Controls.Add(ReloadButton);
            Controls.Add(label1);
            Margin = new Padding(4, 3, 4, 3);
            Name = "WebBankBudgeterUi";
            Text = "SwedBank budgeter";
            ((System.ComponentModel.ISupportInitialize)gv_budget).EndInit();
            BudgetTabs.ResumeLayout(false);
            tbl_Kvar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gv_Kvar).EndInit();
            tbl_INCOME.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gv_incomes).EndInit();
            tbl_Budget.ResumeLayout(false);
            tbl_Totals.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gv_Totals).EndInit();
            tbl_Transactions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dg_Transactions).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.DataGridView gv_budget;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox LogTexts;
        private System.Windows.Forms.Button ReloadButton;
        private System.Windows.Forms.TabControl BudgetTabs;
        private System.Windows.Forms.TabPage tbl_Budget;
        private System.Windows.Forms.TabPage tbl_Transactions;
        private System.Windows.Forms.DataGridView dg_Transactions;
        private System.Windows.Forms.TabPage tbl_INCOME;
        private System.Windows.Forms.TabPage tbl_NoCategory;
        private System.Windows.Forms.TabPage tbl_reccuringCosts;
        private System.Windows.Forms.TabPage tbl_nonReccuringCosts;
        private System.Windows.Forms.DataGridView gv_incomes;
        private System.Windows.Forms.TabPage tbl_Totals;
        private System.Windows.Forms.DataGridView gv_Totals;
        private System.Windows.Forms.Button SaveInPosterButton;
        private System.Windows.Forms.TabPage tbl_Kvar;
        private System.Windows.Forms.DataGridView gv_Kvar;
        private System.Windows.Forms.Button SkapaTomRad;
        private Label label2;
        private TextBox txtYearFilter;
    }
}