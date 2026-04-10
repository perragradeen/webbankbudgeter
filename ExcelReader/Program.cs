using System;
using System.Collections;
using Utilities;

var excelPath = @"C:\Files\Dropbox\budget\Program\webbankbudgeter\pelles budget.xls";

try
{
    Console.WriteLine($"Läser Excel-fil: {excelPath}");
    Console.WriteLine("Läser ALLA flikar...\n");
    
    var allData = OpenFileFunctions.GetHashTableFromExcelSheet(
        excelPath, 
        "", 
        onlyLoadSelectedSheetName: false);
    
    Console.WriteLine($"Antal flikar hittade: {allData.Count}\n");
    
    foreach (DictionaryEntry sheetEntry in allData)
    {
        var sheetName = sheetEntry.Key.ToString();
        var sheetData = sheetEntry.Value as Hashtable;
        
        Console.WriteLine($"========================================");
        Console.WriteLine($"FLIK: '{sheetName}'");
        Console.WriteLine($"Antal rader: {sheetData?.Count ?? 0}");
        Console.WriteLine($"========================================");
        
        if (sheetData != null && sheetData.Count > 0)
        {
            int rowCount = 0;
            foreach (DictionaryEntry rowEntry in sheetData)
            {
                if (rowCount >= 20) // Visa max 20 rader per flik
                {
                    Console.WriteLine("...(fler rader finns)");
                    break;
                }
                
                var excelRow = rowEntry.Value as ExcelRowEntry;
                if (excelRow != null && excelRow.Args.Length > 0)
                {
                    var firstCol = excelRow.Args[0]?.ToString();
                    if (!string.IsNullOrWhiteSpace(firstCol))
                    {
                        Console.Write($"  Rad {rowCount + 1}: ");
                        for (int i = 0; i < Math.Min(excelRow.Args.Length, 10); i++)
                        {
                            var val = excelRow.Args[i]?.ToString();
                            if (!string.IsNullOrEmpty(val))
                            {
                                Console.Write($"[{i}]='{val.Substring(0, Math.Min(30, val.Length))}' ");
                            }
                        }
                        Console.WriteLine();
                        rowCount++;
                    }
                }
            }
        }
        Console.WriteLine();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Fel vid läsning: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}
