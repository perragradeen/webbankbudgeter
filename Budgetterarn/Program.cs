using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Budgeter.Winforms
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //ListviewWithComboBoxTest.Test.MainTestes(null);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BudgeterForm());
        }
    }
}
