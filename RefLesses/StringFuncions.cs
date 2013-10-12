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
    }
}