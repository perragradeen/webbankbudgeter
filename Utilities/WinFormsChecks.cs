using System.Windows.Forms;

namespace Utilities
{
    public static class WinFormsChecks
    {
        /// <summary>
        /// Saves if user wants to
        /// </summary>
        /// <param name="somethingChanged">bool indicating if something has changed</param>
        /// <returns>DialogResult from users choice</returns>
        public static DialogResult SaveCheck(bool somethingChanged)
        {
            var saveOrNotResult = DialogResult.None;
            if (!somethingChanged)
                return saveOrNotResult;

            return UserWantsToSave();
        }

        private static DialogResult UserWantsToSave()
        {
            return MessageBox.Show(
                    @"Läget ej sparat! Spara nu?",
                    @"Spara?",
                    MessageBoxButtons.YesNoCancel);
        }
    }
}