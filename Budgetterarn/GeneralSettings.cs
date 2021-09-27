using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using RefLesses;

namespace Budgetterarn
{
    /// <summary>
    /// Returns a setting from file, if the setting wanted is not found, the return result is an empty string or false
    /// </summary>
    public static class GeneralSettings
    {
        private static readonly XmlDocument doc = new XmlDocument();

        static GeneralSettings()
        {
            try
            {
                var path = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    @"Data\"
                );
                path = Path.Combine(path, @"GeneralSettings.xml");
                doc.Load(path);
            }
            catch (Exception loadExp)
            {
                Console.WriteLine(@"Error in Config(): " + loadExp.Message);
                throw new Exception("Config()", loadExp);
            }
        }

        public static string GetStringSetting(string name)
        {
            try
            {
                return ((XmlElement) (doc.SelectSingleNode("//property[@name='" + name + "']")))?.GetAttribute("value");
            }
            catch (Exception cExcp)
            {
                Console.WriteLine(
                    @"Error in config: Settings doc GeneralSettings.xml probably does not contain property name " + name
                    + @".\r\nSys err; " + cExcp.Message);

                return "";
            }
        }

        public static string GetTextFileStringSetting(string name)
        {
            try
            {
                var textFileRowNumber =
                    ((XmlElement) (doc.SelectSingleNode("//property[@name='" + name + "']")))?.GetAttribute(
                        "rownumberInTextfile");
                var textFilePath = AppDomain.CurrentDomain.BaseDirectory
                                   + ((XmlElement) (doc.SelectSingleNode("//property[@name='" + name + "']")))?
                                   .GetAttribute("textfileName");

                TextReader fileReader = new StreamReader(textFilePath);

                var stringFromFile = "";
                for (var i = 0; i <= textFileRowNumber.SafeGetIntFromString(); i++)
                {
                    stringFromFile = fileReader.ReadLine();
                }

                return stringFromFile;
            }
            catch (Exception exception)
            {
                var errMess =
                    "Error in config: Settings doc GeneralSettings.xml probably does not contain property name " + name
                    + ".\r\nSys err; " + exception.Message;
                MessageBox.Show(errMess);

                Console.WriteLine(errMess);

                return string.Empty;
            }
        }
    }
}