﻿using System;
using System.Globalization;

namespace RefLesses
{
    public static class NumberFunctions
    {
        public static double GetDoubleValueFromStringEntry(this string val)
        {
            // Todo, felkollar
            var cultureToUse = System.Threading.Thread.CurrentThread.CurrentCulture;
            if (string.IsNullOrEmpty(val))
            {
                return 0.0;
            }

            if (val.Contains("."))
            {
                cultureToUse = new CultureInfo("en-US");
            }
            else if (val.Contains(","))
            {
                cultureToUse = new CultureInfo("sv-SE");
            }

            var cleanVal = val.Trim()
                .Replace(" ", string.Empty)
                .Replace(":", string.Empty)
                .Replace(";", string.Empty)
                .Replace(":", string.Empty);

            double tempd;
            return double.TryParse(cleanVal, NumberStyles.Number, cultureToUse, out tempd)
                       ? Math.Round(tempd, 2)
                       : 0.0;
        }
    }
}