namespace RefLesses
{
    public static class MiscFunctions
    {
        public static int SafeGetIntFromString(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            return int.TryParse(text, out var n) ? n : 0;
        }
    }
}