namespace RefLesses
{
   public class StringFuncions
    {
        public static string mergeStringArrayToString(string[] inArray)
        {
            return mergeStringArrayToString(inArray, false);
        }
        public static string mergeStringArrayToString(string[] inArray, bool spaceBetweenThem)
        {
            string returnString = "";

            foreach (string item in inArray)
            {
                returnString += item + (spaceBetweenThem ? " " : "");
            }

            return returnString;
        }
    }
}
