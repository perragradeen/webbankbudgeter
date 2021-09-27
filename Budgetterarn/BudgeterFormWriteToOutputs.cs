using System;
using System.Windows.Forms;

namespace Budgetterarn
{
    public partial class BudgeterForm
    {
        #region Write to Output functions

        private static void WriteExceptionToOutput(Exception e, string message = "")
        {
            MessageBox.Show(message + " " + e.Message);
        }

        private static void WriteToOutput(string message)
        {
            MessageBox.Show(message);
        }

        private static void WriteToUiStatusLog(string statusInfo)
        {
            toolStripStatusLabel1.Text = statusInfo;
        }

        private static void AddToUiStatusLog(string statusInfo)
        {
            toolStripStatusLabel1.Text += statusInfo;
        }

        /// <summary>
        /// Settings (mostly debug)
        /// </summary>
        public static string StatusLabelText
        {
            set => WriteToUiStatusLog(value);
        }

        /// <summary>
        /// Titeltexten för fönstret
        /// </summary>
        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        #endregion
    }
}
