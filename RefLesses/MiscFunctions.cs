namespace RefLesses
{
    public static class MiscFunctions
    {
        public static int SafeGetIntFromString(this string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                int n = 0;
                if (int.TryParse(text, out n))
                {
                    return n;
                }
            }

            return 0;
        }
    }
}