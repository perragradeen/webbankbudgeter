using System;
using System.Collections;
using System.Globalization;
using Microsoft.Office.Interop.Excel;

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo

namespace Utilities
{
    internal static class ExcelLogRowComparer
    {
        public static string GetStandardExcelColumnName(int columnNumberOneBased)
        {
            var baseValue = Convert.ToInt32('A') - 1;
            var ret = string.Empty;

            if (columnNumberOneBased > 26)
            {
                ret = GetStandardExcelColumnName(columnNumberOneBased / 26);
            }

            return ret + Convert.ToChar(baseValue + (columnNumberOneBased % 26));
        }

        public static void GetExcelRows(Worksheet worksheet, Hashtable storeIn)
        {
            if (storeIn == null)
            {
                return;
            }

            try
            {
                // worksheet.UsedRange.Count ger rader, worksheet.UsedRange.Columns.Count ger kolumner
                // 65536
                for (var i = 1; i <= worksheet.UsedRange.Rows.Count; i++)
                {
                    var numOfRowsToReadAtATime = 5000; // 10 blir 11
                    if (numOfRowsToReadAtATime > worksheet.UsedRange.Rows.Count)
                    {
                        numOfRowsToReadAtATime = worksheet.UsedRange.Rows.Count - 1;
                    }

                    // Todo: ta bara in resterande, räkna inte med de som redan lästs hittils
                    var column = GetStandardExcelColumnName(worksheet.UsedRange.Columns.Count + 1);
                    var range =
                        worksheet
                            .Range["A" + i.ToString(CultureInfo.InvariantCulture),
                                column + (i + numOfRowsToReadAtATime).ToString(CultureInfo.InvariantCulture)]; // "IV" 
                    var myvalues = (Array) range.Cells.Value[Type.Missing]; // Value;

                    string[] strArrayIn = null;
                    string[,] strArrayIn2D = null;

                    if (numOfRowsToReadAtATime > 1)
                    {
                        strArrayIn2D = ConvertToStringArray2Dimensional(myvalues);
                    }
                    else
                    {
                        strArrayIn = ConvertToStringArray(myvalues);
                    }

                    for (var ii = 0; ii < numOfRowsToReadAtATime + 1; ii++)
                    {
                        if (numOfRowsToReadAtATime > 1)
                        {
                            // Hämta ut en inläst rad
                            if (strArrayIn2D != null)
                            {
                                strArrayIn = new string[1 + strArrayIn2D.GetUpperBound(1)];
                                for (var ijj = 0; ijj < strArrayIn2D.GetUpperBound(1) + 1; ijj++)
                                {
                                    strArrayIn[ijj] = strArrayIn2D[ii, ijj];
                                }
                            }
                        }

                        var strArray = string.Empty;
                        var currentColumn = 0;

                        // Onödig ta strArrayIn direkt
                        if (strArrayIn == null)
                        {
                            continue;
                        }

                        var strArrayToSave = new object[strArrayIn.Length];

                        // TODO: skippa konverteringen till string och lagra object direkt istället
                        foreach (var arg in strArrayIn)
                        {
                            strArray += arg;

                            // Onödig ta strArrayIn direkt
                            if (currentColumn < strArrayToSave.Length)
                            {
                                strArrayToSave[currentColumn++] = arg;
                            }
                        }

                        // Logga inte om det finns dubletter
                        if (!storeIn.ContainsKey(strArray))
                        {
                            storeIn.Add(strArray, new ExcelRowEntry(strArrayToSave));
                        }
                    }

                    i += numOfRowsToReadAtATime > 1 ? numOfRowsToReadAtATime : 0;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }

        private static string[] ConvertToStringArray(Array values)
        {
            // create a new string array
            var theArray = new string[values.Length];

            // loop through the 2-D System.Array and populate the 1-D String Array
            for (var i = 1; i <= values.Length; i++)
            {
                if (values.GetValue(1, i) == null)
                {
                    theArray[i - 1] = string.Empty;
                }
                else
                {
                    theArray[i - 1] = values.GetValue(1, i).ToString();
                }
            }

            return theArray;
        }

        private static string[,] ConvertToStringArray2Dimensional(Array values)
        {
            // create a new string array
            var theArray = new string[values.GetUpperBound(0), values.GetUpperBound(1) - 1];

            // string[,] test = new string[11, 2];

            // loop through the 2-D System.Array and populate the 1-D String Array
            for (var i = 1; i <= values.GetUpperBound(0); i++)
            {
                for (var j = 1; j < values.GetUpperBound(1); j++)
                {
                    if (values.GetValue(i, j) == null)
                    {
                        theArray[i - 1, j - 1] = string.Empty;
                    }
                    else
                    {
                        theArray[i - 1, j - 1] = values.GetValue(i, j).ToString();
                    }
                }
            }

            return theArray;
        }
    }
}