namespace RefLesses
{
    public static class MiscFunctions
    {
        public static int SafeGetIntFromString(this string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                if (int.TryParse(text, out int n))
                {
                    return n;
                }
            }

            return 0;
        }
    }
}