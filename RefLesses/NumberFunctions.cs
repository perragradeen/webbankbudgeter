using System;
using System.Globalization;

namespace RefLesses
{
    public static class NumberFunctions
    {
        public static double GetValueFromEntry(this string val)
        {
            //Todo, felkollar
            var cultureToUse = System.Threading.Thread.CurrentThread.CurrentCulture;
            if (string.IsNullOrEmpty(val))
                return 0.0;

            if (val.Contains("."))
                cultureToUse = new CultureInfo("en-US");
            else if (val.Contains(","))
                cultureToUse = new CultureInfo("sv-SE");

            double tempd;
            return double.TryParse(val.Replace(" ", string.Empty), NumberStyles.Number, cultureToUse, out tempd) ? Math.Round(tempd, 2) : 0.0;
        }
    }
}
