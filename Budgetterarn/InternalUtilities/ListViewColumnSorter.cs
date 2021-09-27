using System.Collections;
using System.Windows.Forms;

namespace Budgetterarn.InternalUtilities
{
    /// <summary>
    /// This class is an implementation of the 'IComparer' interface.
    /// </summary>
    public class ListViewColumnSorter : IComparer
    {
        /// <summary>
        /// Case insensitive comparer object
        /// </summary>
        private readonly CaseInsensitiveComparer ObjectCompare;

        /// <summary>
        /// Class constructor.  Initializes various elements
        /// </summary>
        public ListViewColumnSorter()
        {
            // Initialize the column to '0'
            SortColumn = 0;

            // Initialize the sort order to 'none'
            Order = SortOrder.None;

            // Initialize the CaseInsensitiveComparer object
            ObjectCompare = new CaseInsensitiveComparer();
        }

        /// <summary>
        /// The number of the column to which to apply the sorting operation (Defaults to '0').
        /// </summary>
        public int SortColumn { get; set; }

        /// <summary>
        /// The order of sorting to apply (for example, 'Ascending' or 'Descending').
        /// </summary>
        public SortOrder Order { get; set; }

        /// <summary>
        /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
        /// </summary>
        /// <param name="x">First object to be compared</param>
        /// <param name="y">Second object to be compared</param>
        /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
        public int Compare(object x, object y)
        {
            // Cast the objects to be compared to ListViewItem objects
            var listviewX = (ListViewItem) x;
            var listviewY = (ListViewItem) y;

            // Compare the two items
            var compareResult = ObjectCompare.Compare(
                listviewX?.SubItems[SortColumn].Text, listviewY?.SubItems[SortColumn].Text);

            switch (Order)
            {
                // Calculate correct return value based on object comparison
                case SortOrder.Ascending:
                    // Ascending sort is selected, return normal result of compare operation
                    return compareResult;
                case SortOrder.Descending:
                    // Descending sort is selected, return negative result of compare operation
                    return -compareResult;
                default:
                    // Return '0' to indicate they are equal
                    return 0;
            }
        }
    }
}