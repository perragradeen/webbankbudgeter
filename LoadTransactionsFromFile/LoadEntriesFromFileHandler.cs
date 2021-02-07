using Budgeter.Core.Entities;
using System;
using System.Collections;
using System.IO;
using Utilities;

namespace LoadTransactionsFromFile
{
    public class LoadEntriesFromFileHandler
    {
        public static Hashtable LoadEntriesFromFile(
            KontoutdragInfoForLoad kontoutdragInfoForLoad)
        {
            // Backa inte upp filen innan laddning, eftersom filen inte ändras vid laddning...
            // BackupOrginialFile("Before.Load");

            // Öppna fil först, och ladda, sen ev. spara ändringar, som inte ändrats av laddningen, av filöpnningen
            var kontoUtdragXls = new Hashtable();

            // Todo: Gör om till arraylist, eller lista av dictionary items, för att kunna välja ordning
            #region Öppna fil och hämta rader

            try
            {
                var filePath = kontoutdragInfoForLoad.FilePath;

                if (string.IsNullOrEmpty(filePath))
                {
                    return null;
                }

                if (!System.IO.File.Exists(filePath))
                {
                    throw new FileNotFoundException(filePath);
                }

                OpenFileFunctions.OpenExcelSheet(filePath, kontoutdragInfoForLoad.SheetName, kontoUtdragXls, 0);
            }
            catch (Exception fileOpneExcp)
            {
                Console.WriteLine("User cancled or other error: " + fileOpneExcp.Message);
                kontoutdragInfoForLoad.FilePath = fileOpneExcp.Message;

                if (kontoUtdragXls.Count < 1)
                {
                    // throw fileOpneExcp;
                    return null;
                }
            }

            #endregion

            return (Hashtable)kontoUtdragXls[kontoutdragInfoForLoad.SheetName];
        }
    }
}
