using System.Drawing;
using System.Windows.Forms;

namespace BudgetterarnUi
{
    public partial class ListViewWithComboBox : KontoEntryListView
    {
        private readonly ComboBox comboBoxCategories = new ComboBox();
        private ListViewItem clickedItem;

        private void InitializeComponent()
        {
            comboBoxCategories.Size = new Size(0, 0);
            comboBoxCategories.Location = new Point(0, 0);
            Controls.AddRange(new Control[] { comboBoxCategories });

            comboBoxCategories.SelectedIndexChanged += CategorySelected;
            comboBoxCategories.LostFocus += CategoryFocusExit;
            comboBoxCategories.KeyPress += CategoryKeyPress;
            comboBoxCategories.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxCategories.Hide();

            Name = "listViewWithComboBox1";

            TabIndex = 0;
            MouseDown += ListViewMouseDown;

            MouseClick += ListViewMouseClick;
        }

    }
}