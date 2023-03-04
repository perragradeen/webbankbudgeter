using System.Collections;

namespace BudgeterCore.Entities
{
    public class DescendingComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            try
            {
                if (x is string)
                {
                    return string.Compare(
                        x.ToString(),
                        y?.ToString(),
                        StringComparison.Ordinal) * -1;
                }

                return Convert.ToInt32(x)
                    .CompareTo(Convert.ToInt32(y)) * -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("No real exception in DescendingComparer.Compare(obj x, obj y): " + ex.Message);
                return string.Compare(
                    x?.ToString(),
                    y?.ToString(),
                    StringComparison.Ordinal);
            }
        }
    }
}