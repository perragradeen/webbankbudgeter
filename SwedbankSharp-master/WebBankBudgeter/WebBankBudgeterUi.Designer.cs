namespace WebBankBudgeter
{
    partial class WebBankBudgeterUi
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.gv_budget = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.LogTexts = new System.Windows.Forms.RichTextBox();
            this.ReloadButton = new System.Windows.Forms.Button();
            this.BudgetTabs = new System.Windows.Forms.TabControl();
            this.tbl_Kvar = new System.Windows.Forms.TabPage();
            this.gv_Kvar = new System.Windows.Forms.DataGridView();
            this.tbl_INCOME = new System.Windows.Forms.TabPage();
            this.gv_incomes = new System.Windows.Forms.DataGridView();
            this.tbl_Budget = new System.Windows.Forms.TabPage();
            this.tbl_Totals = new System.Windows.Forms.TabPage();
            this.gv_Totals = new System.Windows.Forms.DataGridView();
            this.tbl_Transactions = new System.Windows.Forms.TabPage();
            this.dg_Transactions = new System.Windows.Forms.DataGridView();
            this.tbl_reccuringCosts = new System.Windows.Forms.TabPage();
            this.tbl_nonReccuringCosts = new System.Windows.Forms.TabPage();
            this.tbl_NoCategory = new System.Windows.Forms.TabPage();
            this.SaveInPosterButton = new System.Windows.Forms.Button();
            this.SkapaTomRad = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.gv_budget)).BeginInit();
            this.BudgetTabs.SuspendLayout();
            this.tbl_Kvar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gv_Kvar)).BeginInit();
            this.tbl_INCOME.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gv_incomes)).BeginInit();
            this.tbl_Budget.SuspendLayout();
            this.tbl_Totals.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gv_Totals)).BeginInit();
            this.tbl_Transactions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dg_Transactions)).BeginInit();
            this.SuspendLayout();
            // 
            // gv_budget
            // 
            this.gv_budget.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.gv_budget.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gv_budget.DefaultCellStyle = dataGridViewCellStyle2;
            this.gv_budget.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gv_budget.Location = new System.Drawing.Point(3, 3);
            this.gv_budget.Name = "gv_budget";
            this.gv_budget.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.gv_budget.Size = new System.Drawing.Size(861, 465);
            this.gv_budget.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Info: ";
            // 
            // LogTexts
            // 
            this.LogTexts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LogTexts.Location = new System.Drawing.Point(27, 603);
            this.LogTexts.Name = "LogTexts";
            this.LogTexts.Size = new System.Drawing.Size(871, 24);
            this.LogTexts.TabIndex = 2;
            this.LogTexts.Text = "";
            // 
            // ReloadButton
            // 
            this.ReloadButton.Location = new System.Drawing.Point(27, 53);
            this.ReloadButton.Name = "ReloadButton";
            this.ReloadButton.Size = new System.Drawing.Size(75, 23);
            this.ReloadButton.TabIndex = 3;
            this.ReloadButton.Text = "ReLoad";
            this.ReloadButton.UseVisualStyleBackColor = true;
            // 
            // BudgetTabs
            // 
            this.BudgetTabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BudgetTabs.Controls.Add(this.tbl_Kvar);
            this.BudgetTabs.Controls.Add(this.tbl_INCOME);
            this.BudgetTabs.Controls.Add(this.tbl_Budget);
            this.BudgetTabs.Controls.Add(this.tbl_Totals);
            this.BudgetTabs.Controls.Add(this.tbl_Transactions);
            this.BudgetTabs.Controls.Add(this.tbl_reccuringCosts);
            this.BudgetTabs.Controls.Add(this.tbl_nonReccuringCosts);
            this.BudgetTabs.Controls.Add(this.tbl_NoCategory);
            this.BudgetTabs.Location = new System.Drawing.Point(27, 100);
            this.BudgetTabs.Name = "BudgetTabs";
            this.BudgetTabs.SelectedIndex = 0;
            this.BudgetTabs.Size = new System.Drawing.Size(875, 497);
            this.BudgetTabs.TabIndex = 4;
            // 
            // tbl_Kvar
            // 
            this.tbl_Kvar.Controls.Add(this.gv_Kvar);
            this.tbl_Kvar.Location = new System.Drawing.Point(4, 22);
            this.tbl_Kvar.Name = "tbl_Kvar";
            this.tbl_Kvar.Padding = new System.Windows.Forms.Padding(3);
            this.tbl_Kvar.Size = new System.Drawing.Size(867, 471);
            this.tbl_Kvar.TabIndex = 7;
            this.tbl_Kvar.Text = "Kvar";
            this.tbl_Kvar.UseVisualStyleBackColor = true;
            // 
            // gv_Kvar
            // 
            this.gv_Kvar.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gv_Kvar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gv_Kvar.Location = new System.Drawing.Point(3, 3);
            this.gv_Kvar.Name = "gv_Kvar";
            this.gv_Kvar.Size = new System.Drawing.Size(861, 465);
            this.gv_Kvar.TabIndex = 0;
            // 
            // tbl_INCOME
            // 
            this.tbl_INCOME.Controls.Add(this.gv_incomes);
            this.tbl_INCOME.Location = new System.Drawing.Point(4, 22);
            this.tbl_INCOME.Name = "tbl_INCOME";
            this.tbl_INCOME.Size = new System.Drawing.Size(867, 471);
            this.tbl_INCOME.TabIndex = 2;
            this.tbl_INCOME.Text = "Incomes";
            this.tbl_INCOME.UseVisualStyleBackColor = true;
            // 
            // gv_incomes
            // 
            this.gv_incomes.AllowDrop = true;
            this.gv_incomes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gv_incomes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gv_incomes.Location = new System.Drawing.Point(0, 0);
            this.gv_incomes.Name = "gv_incomes";
            this.gv_incomes.Size = new System.Drawing.Size(867, 471);
            this.gv_incomes.TabIndex = 0;
            // 
            // tbl_Budget
            // 
            this.tbl_Budget.Controls.Add(this.gv_budget);
            this.tbl_Budget.Location = new System.Drawing.Point(4, 22);
            this.tbl_Budget.Name = "tbl_Budget";
            this.tbl_Budget.Padding = new System.Windows.Forms.Padding(3);
            this.tbl_Budget.Size = new System.Drawing.Size(867, 471);
            this.tbl_Budget.TabIndex = 0;
            this.tbl_Budget.Text = "Budget Total";
            this.tbl_Budget.UseVisualStyleBackColor = true;
            // 
            // tbl_Totals
            // 
            this.tbl_Totals.Controls.Add(this.gv_Totals);
            this.tbl_Totals.Location = new System.Drawing.Point(4, 22);
            this.tbl_Totals.Name = "tbl_Totals";
            this.tbl_Totals.Size = new System.Drawing.Size(867, 471);
            this.tbl_Totals.TabIndex = 6;
            this.tbl_Totals.Text = "Totals";
            this.tbl_Totals.UseVisualStyleBackColor = true;
            // 
            // gv_Totals
            // 
            this.gv_Totals.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gv_Totals.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gv_Totals.Location = new System.Drawing.Point(0, 0);
            this.gv_Totals.Name = "gv_Totals";
            this.gv_Totals.RowTemplate.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.gv_Totals.Size = new System.Drawing.Size(867, 471);
            this.gv_Totals.TabIndex = 0;
            // 
            // tbl_Transactions
            // 
            this.tbl_Transactions.Controls.Add(this.dg_Transactions);
            this.tbl_Transactions.Location = new System.Drawing.Point(4, 22);
            this.tbl_Transactions.Name = "tbl_Transactions";
            this.tbl_Transactions.Padding = new System.Windows.Forms.Padding(3);
            this.tbl_Transactions.Size = new System.Drawing.Size(867, 471);
            this.tbl_Transactions.TabIndex = 1;
            this.tbl_Transactions.Text = "Transactions";
            this.tbl_Transactions.UseVisualStyleBackColor = true;
            // 
            // dg_Transactions
            // 
            this.dg_Transactions.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dg_Transactions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dg_Transactions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dg_Transactions.Location = new System.Drawing.Point(3, 3);
            this.dg_Transactions.Name = "dg_Transactions";
            this.dg_Transactions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dg_Transactions.Size = new System.Drawing.Size(861, 465);
            this.dg_Transactions.TabIndex = 0;
            // 
            // tbl_reccuringCosts
            // 
            this.tbl_reccuringCosts.Location = new System.Drawing.Point(4, 22);
            this.tbl_reccuringCosts.Name = "tbl_reccuringCosts";
            this.tbl_reccuringCosts.Size = new System.Drawing.Size(867, 471);
            this.tbl_reccuringCosts.TabIndex = 4;
            this.tbl_reccuringCosts.Text = "Reccuring Costs";
            this.tbl_reccuringCosts.UseVisualStyleBackColor = true;
            // 
            // tbl_nonReccuringCosts
            // 
            this.tbl_nonReccuringCosts.Location = new System.Drawing.Point(4, 22);
            this.tbl_nonReccuringCosts.Name = "tbl_nonReccuringCosts";
            this.tbl_nonReccuringCosts.Size = new System.Drawing.Size(867, 471);
            this.tbl_nonReccuringCosts.TabIndex = 5;
            this.tbl_nonReccuringCosts.Text = "Non Reccuring Costs";
            this.tbl_nonReccuringCosts.UseVisualStyleBackColor = true;
            // 
            // tbl_NoCategory
            // 
            this.tbl_NoCategory.Location = new System.Drawing.Point(4, 22);
            this.tbl_NoCategory.Name = "tbl_NoCategory";
            this.tbl_NoCategory.Size = new System.Drawing.Size(867, 471);
            this.tbl_NoCategory.TabIndex = 3;
            this.tbl_NoCategory.Text = "No category";
            this.tbl_NoCategory.UseVisualStyleBackColor = true;
            // 
            // SaveInPosterButton
            // 
            this.SaveInPosterButton.Location = new System.Drawing.Point(108, 53);
            this.SaveInPosterButton.Name = "SaveInPosterButton";
            this.SaveInPosterButton.Size = new System.Drawing.Size(89, 23);
            this.SaveInPosterButton.TabIndex = 3;
            this.SaveInPosterButton.Text = "Spara InPoster";
            this.SaveInPosterButton.UseVisualStyleBackColor = true;
            this.SaveInPosterButton.Click += new System.EventHandler(this.SaveInPosterButton_Click);
            // 
            // SkapaTomRad
            // 
            this.SkapaTomRad.Location = new System.Drawing.Point(203, 53);
            this.SkapaTomRad.Name = "SkapaTomRad";
            this.SkapaTomRad.Size = new System.Drawing.Size(165, 23);
            this.SkapaTomRad.TabIndex = 3;
            this.SkapaTomRad.Text = "Skapa tom rad med Inposter";
            this.SkapaTomRad.UseVisualStyleBackColor = true;
            // 
            // WebBankBudgeterUi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(914, 639);
            this.Controls.Add(this.SkapaTomRad);
            this.Controls.Add(this.SaveInPosterButton);
            this.Controls.Add(this.BudgetTabs);
            this.Controls.Add(this.LogTexts);
            this.Controls.Add(this.ReloadButton);
            this.Controls.Add(this.label1);
            this.Name = "WebBankBudgeterUi";
            this.Text = "SwedBank budgeter";
            ((System.ComponentModel.ISupportInitialize)(this.gv_budget)).EndInit();
            this.BudgetTabs.ResumeLayout(false);
            this.tbl_Kvar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gv_Kvar)).EndInit();
            this.tbl_INCOME.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gv_incomes)).EndInit();
            this.tbl_Budget.ResumeLayout(false);
            this.tbl_Totals.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gv_Totals)).EndInit();
            this.tbl_Transactions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dg_Transactions)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}

