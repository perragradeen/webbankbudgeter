using System.Diagnostics;

namespace Utilities
{
    /// <summary>
    /// Helper class for opening Excel files
    /// </summary>
    public static class ExcelOpener
    {
        /// <summary>
        /// Öppnar en Excel-fil med standardapplikationen
        /// </summary>
        /// <param name="filePath">Sökväg till Excel-filen</param>
        public static void LoadExcelFileInExcel(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Excel-filen hittades inte: {filePath}");
                }

                // Använd Process.Start för att öppna filen med standardapplikationen
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                };

                Process.Start(processStartInfo);
            }
            catch (Exception ex)
            {
                throw new Exception($"Kunde inte öppna Excel-filen: {ex.Message}", ex);
            }
        }
    }
}

