namespace BudgetterarnUi.WinFormsUtilities
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
            {
                return saveOrNotResult;
            }

            return UserWantsToSave();
        }

        private static DialogResult UserWantsToSave()
        {
            return MessageBox.Show(
                @"Läget ej sparat! Spara nu?",
                @"Spara?",
                MessageBoxButtons.YesNoCancel);
        }

        public static int WriteLineToMessageBox(
            string[] message) // TODO: gör egen klass o snygga till
        {
            var caption = message[0];
            var mess = message[1];
            return (int)MessageBox.Show(
               mess, // @"Läget ej sparat! Spara nu?",
               caption, //  @"Spara?",
               MessageBoxButtons.YesNo);

            //WriteToOutput(Environment.NewLine);
            //WriteToOutput(message);
            //LogTexts.ScrollToCaret();
        }
    }
}