using System;
using System.Windows.Forms;
using Utilities;

namespace Budgetterarn.InternalUtilities
{
    public static class FileOperations
    {
        public static string OpenFileOfType(
            Action<string> writeToOutput,
            string dlgTitle = @"Open file",
            FileType fileType = FileType.Xls,
            string dirRelativeToBase = "",
            string absoluteDir = "")
        {
            try
            {
                var dialog = new OpenFileDialog();
                if (dlgTitle != "")
                {
                    dialog.Title = dlgTitle;
                }

                dialog.Multiselect = false;

                if (dirRelativeToBase != "")
                {
                    dialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
                                              + dirRelativeToBase;
                }
                else if (absoluteDir != "")
                {
                    dialog.InitialDirectory = absoluteDir;
                }

                dialog.Filter =
                    OpenFileFunctions.UsedFileTypesFilterNames[fileType]
                    + @"|*."
                    + fileType;
                return dialog.ShowDialog() != DialogResult.OK
                    ? string.Empty
                    : dialog.FileName;
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