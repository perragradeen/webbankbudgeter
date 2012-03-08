using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using Budgetterarn.InternalUtilities;

namespace Budgetterarn
{
    public partial class KontoEntryListView : ListView
    {
        public KontoEntryListView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.c_Date = new System.Windows.Forms.ColumnHeader();
            this.c_Info = new System.Windows.Forms.ColumnHeader();
            this.c_KostnadEllerInkomst = new System.Windows.Forms.ColumnHeader();
            this.c_SaldoOrginal = new System.Windows.Forms.ColumnHeader();
            this.c_AckumuleratSaldo = new System.Windows.Forms.ColumnHeader();
            this.c_TypAvKostnad = new System.Windows.Forms.ColumnHeader();
            // 
            // m_newIitemsListOrg
            // 
            this.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.c_Date,
            this.c_Info,
            this.c_TypAvKostnad,
            this.c_KostnadEllerInkomst,
            this.c_SaldoOrginal,
            this.c_AckumuleratSaldo});
            //
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FullRowSelect = true;
            this.GridLines = true;
            //this.Location = new System.Drawing.Point(3, 3);
            //this.Name = "m_newIitemsListOrg";
            //this.Size = new System.Drawing.Size(574, 577);
            this.TabIndex = 0;
            this.UseCompatibleStateImageBehavior = false;
            this.View = System.Windows.Forms.View.Details;
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
            this.c_TypAvKostnad.Width = 92;

            // Create an instance of a ListView column sorter and assign it 
            // to the ListView control.
            ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView1_ColumnClick);

        }

        //Members
        private ColumnHeader c_Date;
        private ColumnHeader c_Info;
        private ColumnHeader c_KostnadEllerInkomst;
        private ColumnHeader c_SaldoOrginal;
        private ColumnHeader c_AckumuleratSaldo;
        private ColumnHeader c_TypAvKostnad;

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e) {
            var lvwColumnSorter = (ListViewColumnSorter) ListViewItemSorter;
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvwColumnSorter.SortColumn) {
                // Reverse the current sort direction for this column.
                if (lvwColumnSorter.Order == SortOrder.Ascending) {
                    lvwColumnSorter.Order = SortOrder.Descending;
                } else {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
            } else {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            Sort();

        }
    }

}
