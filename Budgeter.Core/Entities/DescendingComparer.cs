using System;
using System.Collections;

namespace Budgeter.Core.Entities
{
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
                {
                    return System.Convert.ToInt32(x).CompareTo(System.Convert.ToInt32(y)) * -1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("No real exception in DescendingComparer.Compare(obj x, obj y): " + ex.Message);
                return x.ToString().CompareTo(y.ToString());
            }
        }
    }
}