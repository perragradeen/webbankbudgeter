using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Budgeter.Core.Entities;
using CategoryHandler;
using CategoryHandler.Model;

namespace Budgetterarn
{
    public partial class ListViewWithComboBox
    {
        #region Members

        private const string AutoCatCpation = "Spara kategorival (typ av kostnad) automatiskt?";
        private const int TypAvKostnadKolumnnummer = 2;
        private readonly ComboBox comboBoxCategories = new ComboBox();

        private readonly List<object> previouslySelectedItems = new List<object>();
        private ListViewItem clickedItem;
        private int selectedSubItem;
        private int x;

        #endregion

        // Constructor
        public ListViewWithComboBox()
        {
            #region set comboBoxCategories (comboBox1)

            // Read the categories.
            LoadCategoriesToSelectBox();

            comboBoxCategories.Size = new Size(0, 0);
            comboBoxCategories.Location = new Point(0, 0);
            Controls.AddRange(new Control[] { comboBoxCategories });

            comboBoxCategories.SelectedIndexChanged += CategorySelected;
            comboBoxCategories.LostFocus += CategoryFocusExit;
            comboBoxCategories.KeyPress += CategoryKeyPress;
            comboBoxCategories.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxCategories.Hide();

            #endregion

            Name = "listViewWithComboBox1";

            TabIndex = 0;
            MouseDown += ListViewMouseDown;

            MouseClick += ListViewMouseClick;

            View = View.Details;
            GridLines = true;
            FullRowSelect = true;

            KeyPress += RowKeyPress;
        }

        public void LoadCategoriesToSelectBox()
        {
            comboBoxCategories.Items.Clear();
            foreach (var cat in CategoriesHolder.GetCategoriesList())
            {
                comboBoxCategories.Items.Add(cat.Description);
            }
        }

        public List<KontoEntry> ItemsAsKontoEntries
        {
            get
            {
                var entries = new List<KontoEntry>();

                if (Items.Count <= 0) return entries;
                var items = Items.Cast<ListViewItem>();
                items.ToList().ForEach(
                    viewItem => entries.Add((KontoEntry)viewItem.Tag));

                return entries;
            }
        }

        #region Events (button clicks etc)

        private void RowKeyPress(object sender, KeyPressEventArgs e)
        {
            // TODO: sätt denna i högerklickmeny
            if (e.KeyChar == 'a')
            {
                // Autocat
                SetAutoCategory();
            }

            if (e.KeyChar == 'd')
            {
                // Delete
                DetleteSelectedEntry();
            }

            if (e.KeyChar == 's')
            {
                // Several
                SetSeveralCategoriesAtOnce();
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Delete)
            {
                DetleteSelectedEntry();
            }

            return base.IsInputKey(keyData);
        }

        private void SetAutoCategory()
        {
            // Done:Kolla ordningen på sakerna, i base, sätt kategori närmare info
            // Done: Gör denna som kollar knapp-nyckel i ovan istället, så inte flera klick körs
            // Done: sätt i egen funktin
            var i = comboBoxCategories.SelectedIndex;

            #region Check values

            if (i < 0 && clickedItem == null)
            {
                return;
            }

            #endregion

            // Get selected values
            // Get selected items cat
            var selEntry = (KontoEntry)clickedItem.Tag;
            var selItemsCat = selEntry != null && !string.IsNullOrEmpty(selEntry.TypAvKostnad)
                ? selEntry.TypAvKostnad
                : null;

            var selectedCategoryText = selItemsCat ?? comboBoxCategories.Items[i].ToString(); // Done:Byt namn
            var slectedInfoDescription = clickedItem.SubItems[1].Text;
            var newAutoCategeory = new AutoCategorise { InfoDescription = slectedInfoDescription };

            #region Fråga anv. om den är säker

            // Fråga anv. om den är säker
            var autoCatMessage = "Spara autokategori? Alltså att altid välja:" + Environment.NewLine
                                                                               + selectedCategoryText + Environment.NewLine + "Varje gång info är:"
                                                                               + Environment.NewLine + slectedInfoDescription + Environment.NewLine + "?";
            if (!UserAcceptsFurtherAction(autoCatMessage, AutoCatCpation))
            {
                return;
            }

            #endregion

            // Sätt autokategori (lägg till eller ändra)
            if (!CategoriesHolder.AllCategoriesHandler.SetNewAutoCategorize(
                selectedCategoryText,
                newAutoCategeory,
                UserAcceptsFurtherAction,
                AutoCatCpation))
            {
                return;
            }

            // Spara till fil
            CategoriesHolder.Save();

            // Todo. ev. kopiera savad xml-fil till utvecklingsarea.Eg. ej, föra att anv. ska ha egen datafil...

            // Uppdera listan men nya entries
            UpdateCategoriesWithAutoCatList(Items, newAutoCategeory.InfoDescription);
        }

        internal static void UpdateCategoriesWithAutoCatList(ListViewItemCollection items)
        {
            UpdateCategoriesWithAutoCatList(items, string.Empty);
        }

        /// <summary>
        /// Går igenom en hel lista o sätt autokat.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="infoToCheck">empty string means all, null means none</param>
        private static void UpdateCategoriesWithAutoCatList(IEnumerable items, string infoToCheck)
        {
            #region Uppdera listan men nya entries

            // Todo: egen funktionför detta
            // Todo: uppdera listan men nya entries. ladda oxo om filen ev. . Om det kommer fler, fast då används ju minnet som är uppdaterat ändå.
            foreach (ListViewItem listViewItem in items)
            {
                var newKe = (KontoEntry)listViewItem.Tag;
                if (newKe == null)
                {
                    continue;
                }

                // Slå upp autokategori
                var lookedUpCat = CategoriesHolder.AutocategorizeType(newKe.Info);
                if (lookedUpCat == null)
                {
                    continue;
                }

                // Om det är info man nyss har ändrat, eller om infon är en tom sträng (skulle kunna ha null istället)
                if (!newKe.Info.Equals(infoToCheck) && !infoToCheck.Equals(string.Empty))
                {
                    continue;
                }

                // Skippa att fråga om o sätta exakt samma kategori.
                if (newKe.TypAvKostnad != null && newKe.TypAvKostnad.Equals(lookedUpCat))
                {
                    continue;
                }

                // Ska man skriva över vald autocat? det är nog upp till användaren...
                if (!string.IsNullOrEmpty(newKe.TypAvKostnad))
                {
                    var autoCatMessage =
                        "Rad har redan en typ av kostnad. Ska den nu valda skrivas över? Alltså att byta från:"
                        + Environment.NewLine + newKe.TypAvKostnad + Environment.NewLine + "Till:" + Environment.NewLine
                        + lookedUpCat + Environment.NewLine + "?" + Environment.NewLine + "För info:"
                        + Environment.NewLine + newKe.Info + Environment.NewLine + Environment.NewLine
                        + "För rad (ihopskriven):" + newKe.KeyForThis;

                    if (!UserAcceptsFurtherAction(autoCatMessage, AutoCatCpation))
                    {
                        continue;
                    }
                }

                // Har man kommit förbi alla contines, så ska kategorin bytas.
                newKe.TypAvKostnad = lookedUpCat;

                // Sätt ny kategori i listan också, så anv. ser att det ändrats.
                listViewItem.SubItems[TypAvKostnadKolumnnummer].Text = newKe.TypAvKostnad;
            }

            #endregion
        }

        private static bool UserAcceptsFurtherAction(string message, string caption)
        {
            // Done:Popup mbox and ask user Are u sure?...etc
            var saveAutocatOrNot = MessageBox.Show(message, caption, MessageBoxButtons.YesNoCancel);

            return saveAutocatOrNot.Equals(DialogResult.Yes);
        }

        private void SetSeveralCategoriesAtOnce()
        {
            // Save items selected at the moment of s press...
            previouslySelectedItems.Clear();
            foreach (var item in SelectedItems)
            {
                previouslySelectedItems.Add(item);
            }

            // Popup box:
            PopupComboboxOfCaytegories(TypAvKostnadKolumnnummer);
        }

        private void CategoryKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13 || e.KeyChar == 27)
            {
                comboBoxCategories.Hide();
            }
        }

        private void CategorySelected(object sender, EventArgs e)
        {
            var i = comboBoxCategories.SelectedIndex;
            if (i < 0 || !UserEventFire)
            {
                return;
            }

            var str = comboBoxCategories.Items[i].ToString();

            if (previouslySelectedItems.Count > 0)
            {
                // Set all items selected at the moment of s press...
                foreach (ListViewItem item in previouslySelectedItems)
                {
                    item.SubItems[selectedSubItem].Text = str;
                    ((KontoEntry)item.Tag).TypAvKostnad = str;
                }

                // items set, clear so next selection don't overwrite
                previouslySelectedItems.Clear();
            }
            else
            {
                if (clickedItem == null) return;
                clickedItem.SubItems[selectedSubItem].Text = str;
                ((KontoEntry)clickedItem.Tag).TypAvKostnad = str;
            }
        }

        private void CategoryFocusExit(object sender, EventArgs e)
        {
            comboBoxCategories.Hide();
        }

        public void ListViewDoubleClick(object sender, EventArgs e)
        {
            PopupComboboxOfCaytegories();
        }

        private void ListViewMouseClick(object sender, EventArgs e)
        {
            PopupComboboxOfCaytegories();
        }

        private void ListViewMouseDown(object sender, MouseEventArgs e)
        {
            clickedItem = GetItemAt(e.X, e.Y);
            x = e.X;

#if DEBUG
            BudgeterForm.StatusLabelText = x.ToString();
#endif
        }

        #endregion

        private bool UserEventFire { get; set; }

        private void DetleteSelectedEntry()
        {
            if (SelectedIndices.Count < 1 || SelectedItems.Count < 1)
            {
                return;
            }

            Items.RemoveAt(SelectedIndices[0]);

            // Todo: Ta bort den ur minnet, newKontoEntries
        }

        private void PopupComboboxOfCaytegories(int? selectedSubColumnItem = null)
        {
            // Check whether the subitem was clicked

            #region Check posistion clicked

            var start = x;
            var position = 0;
            var end = Columns[0].Width;

            // Ex. 128 - 206
            for (var i = 0; i < Columns.Count; i++)
            {
                if (start > position && start < end)
                {
                    selectedSubItem = i;

#if DEBUG
                    var columnsel = Columns[selectedSubItem].Text;

                    BudgeterForm.StatusLabelText = x + ". Column start: " + start + ". Column end: " + end
                                                   + ". Column pos: " + position + ". Column selected: " + columnsel
                                                   + ". Column date width: " + Columns[0].Width + ". Column typ width: "
                                                   + Columns[2].Width + ". Column kost width: " + Columns[3].Width;
#endif

                    break;
                }

                position = end;
                end += ((i + 1) < Columns.Count) ? Columns[i + 1].Width : 0;
            }

            // If Sent in selection is made
            if (selectedSubColumnItem != null)
            {
                selectedSubItem = (int)selectedSubColumnItem;
            }

            #endregion

            #region Kolla om rätt kolumn är klickad

            // Om fel kolumn är vald, retrun
            var column = Columns[selectedSubItem].Text;
            if (column != "TypAvKostnad")
            {
                return;
            }

            #endregion

            #region Set box properties

            // Sätt boxbredden till en multipel av den längsta.
            var widestText = 0;
            foreach (var box in comboBoxCategories.Items)
            {
                var enl = box.ToString().Length; // Lengt of entry text
                widestText = widestText > enl ? widestText : enl;
            }

            if (clickedItem != null)
            {
                comboBoxCategories.Size = new Size(widestText * 4, clickedItem.Bounds.Bottom - clickedItem.Bounds.Top);

                // Gamla bredden: end - position

                // Set rest of the box properties
                comboBoxCategories.Location = new Point(position, clickedItem.Bounds.Y);
            }
            else
            {
                comboBoxCategories.Size = new Size(widestText * 4, 90 - 76); // Gamla bredden: end - position

                // Set rest of the box properties
                comboBoxCategories.Location = new Point(position, 76);
            }

            comboBoxCategories.Show();

            // Hämta cellens text, och sätt den som vald i boxen
            UserEventFire = false; // Disble event reaction
            if (clickedItem != null)
            {
                comboBoxCategories.Text = clickedItem.SubItems[selectedSubItem].Text; // Here event fires
            }

            UserEventFire = true;

            comboBoxCategories.SelectAll();
            comboBoxCategories.Focus();

            #endregion

            #region Sätt automatiskt texten i vald cell till vald text i kategoriboxen.

            // Kolla om det finns något i boxen (kategorilistan, comboboxen)
            if (comboBoxCategories == null || comboBoxCategories.SelectedItem == null)
            {
                return;
            }

            // Hämta text från boxen
            var selectedItemInCatText = string.IsNullOrEmpty(comboBoxCategories.SelectedItem.ToString())
                ? " "
                : comboBoxCategories.SelectedItem.ToString();

            // Sätt texten i cellen. Om flera inte är valda
            if (selectedSubColumnItem != null || clickedItem == null) return;
            clickedItem.SubItems[selectedSubItem].Text = selectedItemInCatText;
            ((KontoEntry)clickedItem.Tag).TypAvKostnad = selectedItemInCatText;

            #endregion
        }
    }
}