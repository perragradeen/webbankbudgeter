using System.Collections.Generic;

namespace RefLesses
{
    public static class MiscFunctions
    {
        public static void AddToOrChangeValueInDictionaryForKey(
            this Dictionary<string, string> saldon, string saldoName, double saldoValue)
        {
            if (saldon.ContainsKey(saldoName))
            {
                saldon[saldoName] = saldoValue.ToString();
            }
            else
            {
                saldon.Add(saldoName, saldoValue.ToString());
            }
        }

        public static string SafeGetStringFromDictionary(this Dictionary<string, string> saldon, string key)
        {
            if (saldon.ContainsKey(key))
            {
                return saldon[key] ?? string.Empty;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}