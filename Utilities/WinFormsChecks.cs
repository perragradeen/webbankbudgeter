using System.Windows.Forms;

namespace Utilities
{
    public static class WinFormsChecks
    {
        public delegate void SaveFunction();

        /// <summary>
        /// Saves if user wants to
        /// </summary>
        /// <param name="somethingChanged">bool indicating if something has changed</param>
        /// <param name="saveFunc">The function that will perform the actual saving.</param>
        /// <returns>True if something was saved</returns>
        public static DialogResult SaveCheck(
            bool somethingChanged,
            SaveFunction saveFunc)
        {
            var saveOr = DialogResult.None;
            if (!somethingChanged)
                return saveOr;

            saveOr = MessageBox
                .Show("Läget ej sparat! Spara nu?", "Spara?",
                    MessageBoxButtons.YesNoCancel);

            // Cancel
            if (saveOr == DialogResult.Yes)
            {
                saveFunc();
            }

            return saveOr;
        }
    }
}