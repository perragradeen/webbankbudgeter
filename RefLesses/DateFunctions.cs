using System;
using System.Globalization;

namespace RefLesses
{
    public static class DateFunctions
    {
        public static DateTime ParseDateWithCultureEtc(string dateValue)
        {
            if (string.IsNullOrEmpty(dateValue))
            {
                return DateTime.MinValue;
            }

            #region Datumkonvertering etc

            var useThisCulture = new CultureInfo("en-US");

            // felhantering, sätt dagens datum om det är fel
            var currDate = DateTime.MinValue;
            try
            {
                if (string.IsNullOrEmpty(dateValue) || dateValue.Length <= 3)
                {
                }
                else
                {
                    currDate = DateTime.Parse(dateValue, useThisCulture);
                }
            }
            catch (Exception DateValueExc)
            {
                Console.WriteLine("Error in parsing DateValue: " + DateValueExc.Message);
            }

            var svecia = new CultureInfo("sv-SE");

            var DateValueFormated = currDate.Date.ToString("yyyy-MM-dd", svecia);

            #endregion

            return DateTime.Parse(DateValueFormated);
        }

        public static bool IsValidDate(string dateValue)
        {
            var useThisCulture = new CultureInfo("en-US");
            if (!IsValidDate(dateValue, useThisCulture))
            {
                useThisCulture = new CultureInfo("sv-SE");
                return IsValidDate(dateValue, useThisCulture);
            }

            return true;
        }

        private static bool IsValidDate(string dateValue, CultureInfo useThisCulture)
        {
            return DateTime.TryParse(
                dateValue,
                useThisCulture,
                DateTimeStyles.AdjustToUniversal,
                out _);
        }
    }
}