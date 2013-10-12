using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Budgeter.Core.Entities
{
    public class KontoutdragInfo
    {
        public KontoutdragInfo()
        {
            KontoEntries = new SortedList(new DescendingComparer());
            saldon = new Dictionary<string, string>();
        }

        /// <summary>
        /// Även vid sparning (saveToTable)
        /// </summary>
        public SortedList KontoEntries { get; set; }
        public SortedList NewKontoEntries { get; set; }

        /// <summary>
        /// Key = description, Value= amount
        /// </summary>
        Dictionary<string, string> saldon = new Dictionary<string, string>();
    }

    public class KontoutdragInfoForSave
    {
        public string excelFileSavePath { get; set; }
        public string excelFileSavePathWithoutFileName { get; set; }
        public string excelFileSaveFileName { get; set; }

        public string sheetName { get; set; }
    }

    public class KontoutdragInfoForLoad : KontoutdragInfoForSave
    {
        public string filePath { get; set; }

        public bool clearContentBeforeReadingNewFile { get; set; }
        public bool somethingChanged { get; set; }
    }

    public class KontoutdragInfoForLoadUiThreaded : KontoutdragInfoForLoad
    {
        public Thread mainThread { get; set; }
        public Thread workerThread { get; set; }
    }

    public class LoadOrSaveResult
    {
        public int skippedOrSaved { get; set; }
        public bool somethingLoadedOrSaved { get; set; }
    }

    //Tagit från nätet: http://www.codeproject.com/KB/cs/Descending_Sorted_List.aspx?fid=1353560&df=90&mpp=25&noise=3&sort=Position&view=Quick&select=2570977#xx2570977xx
    public class DescendingComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            try
            {
                if (x.GetType() == typeof(string))
                {
                    return x.ToString().CompareTo(y.ToString()) * -1;
                }
                else
                    return System.Convert.ToInt32(x).CompareTo
                        (System.Convert.ToInt32(y)) * -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("No real exception in DescendingComparer.Compare(obj x, obj y): " + ex.Message);
                return x.ToString().CompareTo(y.ToString());
            }
        }
    }

}
