using System;
using System.Windows.Forms;
using Utilities;

namespace Budgetterarn.InternalUtilities
{
    public static class FileOperations
    {
        public static string OpenFileOfType(
            string dlgTitle,
            FileType fileType,
            string dirRelativeToBase,
            Action<string> writeToOutput,
            string absoluteDir = "")
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
                    dlg.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
                        + dirRelativeToBase; // "Settings";
                }
                else if (absoluteDir != "")
                {
                    dlg.InitialDirectory = absoluteDir;
                }

                dlg.Filter =
                    OpenFileFunctions.UsedFileTypesFilterNames[fileType]
                    + @"|*."
                    + fileType;
                return dlg.ShowDialog() != DialogResult.OK
                    ? string.Empty : dlg.FileName;
            }
            catch (Exception openFileOfTypeExp)
            {
                writeToOutput(@"Error in OpenFileOfType(...): "
                    + openFileOfTypeExp);
                return null;
            }
        }
    }
}