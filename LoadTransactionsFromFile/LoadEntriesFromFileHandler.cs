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
            // Backa inte upp filen innan laddning, eftersom filen inte ändras vid laddning...
            // BackupOrginialFile("Before.Load");

            // Öppna fil först, och ladda, sen ev. spara ändringar, som
            // inte ändrats av laddningen, av filöpnningen
            var kontoUtdragXls = new Hashtable();

            // Todo: Gör om till arraylist, eller lista av dictionary items,
            // för att kunna välja ordning

            #region Öppna fil och hämta rader

            try
            {
                var filePath = kontoutdragExcelFileInfo.ExcelFileSavePath;

                if (string.IsNullOrEmpty(filePath))
                {
                    return null;
                }

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException(filePath);
                }

                OpenFileFunctions.OpenExcelSheet(
                    filePath,
                    kontoutdragExcelFileInfo.SheetName,
                    kontoUtdragXls,
                    0);
            }
            catch (Exception fileOpneExcp)
            {
                Console.WriteLine("User cancled or other error: "
                                  + fileOpneExcp.Message);

                if (kontoUtdragXls.Count < 1)
                {
                    // throw fileOpneExcp;
                    return null;
                }
            }

            #endregion

            return (Hashtable) kontoUtdragXls[kontoutdragExcelFileInfo.SheetName];
        }
    }
}