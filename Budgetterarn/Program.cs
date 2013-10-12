using System;
using System.Windows.Forms;
using Budgetterarn;

namespace Budgeter.Winforms
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // ListviewWithComboBoxTest.Test.MainTestes(null);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BudgeterForm());
        }
    }
}