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
    }
}