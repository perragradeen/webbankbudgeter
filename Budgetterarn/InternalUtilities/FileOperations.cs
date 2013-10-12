using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;

namespace Budgetterarn.Operations
{
    public class FileOperations
    {
        public static string OpenFileOfType(string dlgTitle, Utilities.FileType fileType, string dirRelativeToBase)
        {
            return OpenFileOfType(dlgTitle, fileType, dirRelativeToBase, "");
        }
        public static string OpenFileOfType(string dlgTitle, Utilities.FileType fileType, string dirRelativeToBase, string absoluteDir)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                if (dlgTitle != "")
                {
                    dlg.Title = dlgTitle;
                }

                dlg.Multiselect = false;

                if (dirRelativeToBase != "")
                {
                    dlg.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + dirRelativeToBase;// "Settings";
                }
                else if (absoluteDir != "")
                {
                    dlg.InitialDirectory = absoluteDir;
                }

                dlg.Filter = (Utilities.OpenFileFunctions.UsedFileTypesFilterNames[fileType]).ToString() + "|*." + fileType.ToString();
                if (dlg.ShowDialog() != DialogResult.OK)
                    return "";
                else
                    return dlg.FileName;

            }
            catch (Exception OpenFileOfTypeExp)
            {
                MessageBox.Show("Error in OpenFileOfType(...): " + OpenFileOfTypeExp);
                return null;
            }
        }

    }
}
