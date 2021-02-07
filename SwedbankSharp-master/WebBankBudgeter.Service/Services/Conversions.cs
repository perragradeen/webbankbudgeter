using System.Text.RegularExpressions;
using System.Threading;

namespace WebBankBudgeter.Service.Services
{
    public class Conversions
    {
        public static double SafeGetDouble(object text)
        {
            if (text == null)
            {
                return 0;
            }

            return DoubleParseAdvanced(text.ToString());

            //return double.TryParse(text.ToString()
            //        .Replace(" ", string.Empty)
            //        .Replace(",", ",")
            //    , NumberStyles.Any, CultureInfo.InvariantCulture
            //    , out double value) 
            //    ? value : 0;
        }

        public static double DoubleParseAdvanced(string strToParse, char decimalSymbol = ',')
        {
            string tmp = Regex.Match(strToParse, @"([-]?[0-9]+)([\s])?([0-9]+)?[." + decimalSymbol + "]?([0-9 ]+)?([0-9]+)?").Value;

            if (tmp.Length > 0 && strToParse.Contains(tmp))
            {
                var currDecSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;

                tmp = tmp.Replace(".", currDecSeparator).Replace(decimalSymbol.ToString(), currDecSeparator);

                return double.Parse(tmp);
            }

            return 0;
        }

    }
}