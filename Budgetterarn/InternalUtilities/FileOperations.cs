using System;
using System.Windows.Forms;
using Utilities;

namespace Budgetterarn.InternalUtilities
{
    public class FileOperations
    {
        public static string OpenFileOfType(string dlgTitle, FileType fileType, string dirRelativeToBase)
        {
            return OpenFileOfType(dlgTitle, fileType, dirRelativeToBase, "");
        }

        public static string OpenFileOfType(
            string dlgTitle, FileType fileType, string dirRelativeToBase, string absoluteDir)
        {
            try
            {
                var dlg = new OpenFileDialog();
                if (dlgTitle != "")
                {
                    dlg.Title = dlgTitle;
                }

                dlg.Multiselect = false;

                if (dirRelativeToBase != "")
                {
                    dlg.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + dirRelativeToBase; // "Settings";
                }
                else if (absoluteDir != "")
                {
                    dlg.InitialDirectory = absoluteDir;
                }

                dlg.Filter = Utilities.OpenFileFunctions.UsedFileTypesFilterNames[fileType] + "|*."
                             + fileType.ToString();
                if (dlg.ShowDialog() != DialogResult.OK)
                {
                    return "";
                }
                else
                {
                    return dlg.FileName;
                }
            }
            catch (Exception OpenFileOfTypeExp)
            {
                MessageBox.Show("Error in OpenFileOfType(...): " + OpenFileOfTypeExp);
                return null;
            }
        }
    }
}