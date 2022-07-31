using System;
using System.Collections;
using System.IO;
using Budgeter.Core.Entities;
using Utilities;

namespace LoadTransactionsFromFile
{
    public static class LoadEntriesFromFileHandler
    {
        public static Hashtable LoadEntriesFromFile(
            KontoutdragExcelFileInfo kontoutdragExcelFileInfo)
        {
            // Backa inte upp filen innan laddning,
            // eftersom filen inte ändras vid laddning...

            //try
            //{
                var filePath = kontoutdragExcelFileInfo.ExcelFileSavePath;
                if (string.IsNullOrEmpty(filePath))
                {
                    return null;
                }

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException(filePath);
                }

                var kontoUtdragXls = OpenFileFunctions.GetHashTableFromExcelSheet(
                        filePath,
                        kontoutdragExcelFileInfo.SheetName);

                return (Hashtable)kontoUtdragXls[kontoutdragExcelFileInfo.SheetName];

            //}
            //catch (Exception fileOpneExcp)
            //{
            //    // TOOD: skriv ut till ui via action...
            //    Console.WriteLine("User cancled or other error: "
            //                      + fileOpneExcp.Message);

            //    return null;
            //}
        }
    }
}