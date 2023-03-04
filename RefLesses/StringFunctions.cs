namespace RefLesses
{
    public static class StringFunctions
    {
        public static string MergeStringArrayToString(IEnumerable<string> inArray, bool spaceBetweenThem = false)
        {
            var space = spaceBetweenThem ? " " : string.Empty;

            return inArray.Aggregate(string.Empty, (current, item) =>
                current + item + space);
        }

        public static string GetTextBetweenStartAndEndText(string inText, string startText, string endText)
        {
            //Kolla saldo
            if (inText == null
                || !ContainsClean(inText, startText)
                || !ContainsClean(inText, endText))
            {
                return string.Empty;
            }

            var elemText = inText.Trim();
            var startIndex = elemText
                                 .ToLower()
                                 .IndexOf(startText.ToLower(), StringComparison.Ordinal)
                             + startText.Length;
            var endIndex = elemText
                .ToLower()
                .IndexOf(endText.ToLower(), StringComparison.Ordinal);

            var saldo = elemText
                .Substring(startIndex, endIndex - startIndex)
                .Trim()
                .Replace(" ", string.Empty);

            //Saldo:44 476,09 Information och villkor om kontot
            return saldo;
        }

        private static bool ContainsClean(string compareThis, string withThis)
        {
            if (compareThis == null || withThis == null)
            {
                return false;
            }

            return compareThis.Trim().ToLower()
                .Contains(
                    withThis.Trim().ToLower());
        }
    }
}