using BudgetterarnUi.InternalUtilities;

// ReSharper disable LocalizableElement

namespace BudgetterarnUi;

public partial class KontoEntryListView
{
    public KontoEntryListView()
    {
        InitializeComponent();
    }

    private void ListView1_ColumnClick(object sender, ColumnClickEventArgs e)
    {
        var lvwColumnSorter = (ListViewColumnSorter)ListViewItemSorter;

        // Determine if clicked column is already the column that is being sorted.
        if (e.Column == lvwColumnSorter.SortColumn)
        {
            // Reverse the current sort direction for this column.
            lvwColumnSorter.Order = lvwColumnSorter.Order == SortOrder.Ascending
                ? SortOrder.Descending
                : SortOrder.Ascending;
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