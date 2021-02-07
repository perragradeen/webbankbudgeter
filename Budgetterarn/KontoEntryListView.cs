using Budgetterarn.InternalUtilities;
using System.Windows.Forms;

namespace Budgetterarn
{
    public partial class KontoEntryListView : ListView
    {
        // Members
        private ColumnHeader c_AckumuleratSaldo;
        private ColumnHeader c_Date;
        private ColumnHeader c_Info;
        private ColumnHeader c_KostnadEllerInkomst;
        private ColumnHeader c_SaldoOrginal;
        private ColumnHeader c_TypAvKostnad;

        public KontoEntryListView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            c_Date = new System.Windows.Forms.ColumnHeader();
            c_Info = new System.Windows.Forms.ColumnHeader();
            c_KostnadEllerInkomst = new System.Windows.Forms.ColumnHeader();
            c_SaldoOrginal = new System.Windows.Forms.ColumnHeader();
            c_AckumuleratSaldo = new System.Windows.Forms.ColumnHeader();
            c_TypAvKostnad = new System.Windows.Forms.ColumnHeader();

            // m_newIitemsListOrg
            Columns.AddRange(
                new[] { c_Date, c_Info, c_TypAvKostnad, c_KostnadEllerInkomst, c_SaldoOrginal, c_AckumuleratSaldo });
            Dock = System.Windows.Forms.DockStyle.Fill;
            FullRowSelect = true;
            GridLines = true;

            // this.Location = new System.Drawing.Point(3, 3);
            // this.Name = "m_newIitemsListOrg";
            // this.Size = new System.Drawing.Size(574, 577);
            TabIndex = 0;
            UseCompatibleStateImageBehavior = false;
            View = System.Windows.Forms.View.Details;

            // c_Date
            c_Date.Text = "Date";
            c_Date.Width = 62;

            // c_Info
            c_Info.Text = "Info";
            c_Info.Width = 85;

            // c_KostnadEllerInkomst
            c_KostnadEllerInkomst.Text = "KostnadEllerInkomst";
            c_KostnadEllerInkomst.Width = 79;

            // c_SaldoOrginal
            c_SaldoOrginal.Text = "SaldoOrginal";
            c_SaldoOrginal.Width = 75;

            // c_AckumuleratSaldo
            c_AckumuleratSaldo.Text = "AckumuleratSaldo";
            c_AckumuleratSaldo.Width = 75;

            // c_TypAvKostnad
            c_TypAvKostnad.Text = "TypAvKostnad";
            c_TypAvKostnad.Width = 92;

            // Create an instance of a ListView column sorter and assign it 
            // to the ListView control.
            ColumnClick += listView1_ColumnClick;
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            var lvwColumnSorter = (ListViewColumnSorter)ListViewItemSorter;

            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                {
                    lvwColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            Sort();
        }
    }
}