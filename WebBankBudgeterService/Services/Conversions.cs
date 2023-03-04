using System.Text.RegularExpressions;

namespace WebBankBudgeter.Service.Services
{
    public static class Conversions
    {
        public static double SafeGetDouble(object text)
        {
            if (text == null)
            {
                return 0;
            }

            return DoubleParseAdvanced(text.ToString());
        }

        private static double DoubleParseAdvanced(string strToParse, char decimalSymbol = ',')
        {
            var tmp = Regex.Match(strToParse,
                @"([-]?[0-9]+)([\s])?([0-9]+)?[." + decimalSymbol + "]?([0-9 ]+)?([0-9]+)?").Value;

            if (tmp.Length <= 0 || !strToParse.Contains(tmp))
                return 0;

            var currentDecimalSeparator = Thread.CurrentThread
                .CurrentCulture.NumberFormat.NumberDecimalSeparator;

            tmp = tmp.Replace(".", currentDecimalSeparator)
                .Replace(decimalSymbol.ToString(), currentDecimalSeparator);

            return double.Parse(tmp);
        }
    }
}