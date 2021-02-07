namespace RefLesses
{
    public class StringFuncions
    {
        public static string MergeStringArrayToString(string[] inArray)
        {
            return MergeStringArrayToString(inArray, false);
        }

        public static string MergeStringArrayToString(string[] inArray, bool spaceBetweenThem)
        {
            var returnString = string.Empty;

            foreach (var item in inArray)
            {
                returnString += item + (spaceBetweenThem ? " " : string.Empty);
            }

            return returnString;
        }

        public static string GetTextBetweenStartAndEndText(string inText, string startText, string endText)
        {
            //var startText = "Ej fakturerat:";
            //var endText = "  Clearingnummer:";

            //Kolla saldo
            if (inText != null &&
                // C:\Files\Dropbox\budget\Program\webbankbudgeter\Budgetterarn\DAL\LoadKonton.cs
                ContainsClean(inText, startText)
                && ContainsClean(inText, endText)
                )
            {
                var elemText = inText.Trim();
                var startIndex = elemText.ToLower().IndexOf(startText.ToLower()) + startText.Length;
                var endIndex = elemText.ToLower().IndexOf(endText.ToLower());

                var saldo =
                    elemText.Substring(startIndex, endIndex - startIndex).Trim()
                    //.Replace(extraText, string.Empty)
                    .Replace(" ", string.Empty);

                //Saldo:44 476,09 Information och villkor om kontot
                //saldoAllkortKreditEjFakturerat = saldo;
                return saldo;
            }

            return string.Empty;
        }

        public static double SafeGetDouble(string inText)
        {
            double result;
            if (double.TryParse(inText, out result))
            {
                return result;
            }

            return 0.0;
        }
        public static bool ContainsClean(string compareThis, string withThis)
        {
            if (compareThis == null || withThis == null)
            {
                return false;
            }

            return compareThis.Trim().ToLower()
                        .Contains(
                    withThis.Trim().ToLower());
        }

        public static string RemoveSekFromMoneyString(string beloppVal)
        {
            return beloppVal.Replace("SEK", string.Empty).Trim().Replace(" ", string.Empty);
        }
    }
}