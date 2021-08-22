using System.Windows.Forms;
using Budgetterarn.InternalUtilities;

// ReSharper disable LocalizableElement

namespace Budgetterarn
{
    public partial class KontoEntryListView
    {
        // Members
        private ColumnHeader _cAckumuleratSaldo;
        private ColumnHeader _cDate;
        private ColumnHeader _cInfo;
        private ColumnHeader _cKostnadEllerInkomst;
        private ColumnHeader _cSaldoOrginal;
        private ColumnHeader _cTypAvKostnad;

        public KontoEntryListView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            _cDate = new ColumnHeader();
            _cInfo = new ColumnHeader();
            _cKostnadEllerInkomst = new ColumnHeader();
            _cSaldoOrginal = new ColumnHeader();
            _cAckumuleratSaldo = new ColumnHeader();
            _cTypAvKostnad = new ColumnHeader();

            // m_newIitemsListOrg
            Columns.AddRange(
                new[] { _cDate, _cInfo, _cTypAvKostnad, _cKostnadEllerInkomst, _cSaldoOrginal, _cAckumuleratSaldo });
            Dock = DockStyle.Fill;
            FullRowSelect = true;
            GridLines = true;

            TabIndex = 0;
            UseCompatibleStateImageBehavior = false;
            View = View.Details;

            // c_Date
            _cDate.Text = "Date";
            _cDate.Width = 62;

            // c_Info
            _cInfo.Text = "Info";
            _cInfo.Width = 85;

            // c_KostnadEllerInkomst
            _cKostnadEllerInkomst.Text = "KostnadEllerInkomst";
            _cKostnadEllerInkomst.Width = 79;

            // c_SaldoOrginal
            _cSaldoOrginal.Text = "SaldoOrginal";
            _cSaldoOrginal.Width = 75;

            // c_AckumuleratSaldo
            _cAckumuleratSaldo.Text = "AckumuleratSaldo";
            _cAckumuleratSaldo.Width = 75;

            // c_TypAvKostnad
            _cTypAvKostnad.Text = "TypAvKostnad";
            _cTypAvKostnad.Width = 92;

            // Create an instance of a ListView column sorter and assign it 
            // to the ListView control.
            ColumnClick += ListView1_ColumnClick;
        }

        private void ListView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            var lvwColumnSorter = (ListViewColumnSorter)ListViewItemSorter;

            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                lvwColumnSorter.Order = lvwColumnSorter.Order == SortOrder.Ascending
                    ? SortOrder.Descending : SortOrder.Ascending;
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