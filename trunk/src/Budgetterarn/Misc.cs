using System;
using System.Windows.Forms;
using System.Xml;

namespace Budgetterarn {
    /// <summary>
    /// Returns a setting from file, if the setting wanted is not found, the return result is an empty string or false
    /// </summary>
    public class GeneralSettings {
        static readonly XmlDocument doc = new XmlDocument();

        static GeneralSettings() {
            try {
                doc.Load(AppDomain.CurrentDomain.BaseDirectory + @"Data\" + @"GeneralSettings.xml");
            } catch (Exception loadExp) {
                Console.WriteLine("Error in Config(): " + loadExp.Message);
                throw new Exception("Config()", loadExp);
            }
        }

        public static bool GetSetting(string name) {
            try {
                return ((XmlElement)((doc.SelectSingleNode("//property[@name='" + name + "']")))).GetAttribute("value") == "true";

            } catch (Exception cExcp) {
                Console.WriteLine("Error in config: Settings doc GeneralSettings.xml probably does not contain property name " + name + ".\r\nSys err; " + cExcp.Message);

                return false;
            }
        }
        public static string GetStringSetting(string name) {
            try {
                return ((XmlElement)((doc.SelectSingleNode("//property[@name='" + name + "']")))).GetAttribute("value");

            } catch (Exception cExcp) {
                Console.WriteLine("Error in config: Settings doc GeneralSettings.xml probably does not contain property name " + name + ".\r\nSys err; " + cExcp.Message);

                return "";
            }
        }
        public static string GetTextfileStringSetting(string name) {
            try {
                var textfileRowNumber =
                    ((XmlElement) ((doc.SelectSingleNode("//property[@name='" + name + "']")))).GetAttribute(
                        "rownumberInTextfile");
                var textfilePath = AppDomain.CurrentDomain.BaseDirectory + 
                    ((XmlElement) ((doc.SelectSingleNode("//property[@name='" + name + "']")))).GetAttribute(
                        "textfileName");

                System.IO.TextReader fileReader = new System.IO.StreamReader(textfilePath);

                var stringFromFile = "";
                for (var i = 0; i <= int.Parse(textfileRowNumber); i++) {
                    stringFromFile = fileReader.ReadLine();
                }

                return stringFromFile;


            } catch (Exception cExcp) {
                var errMess =
                    "Error in config: Settings doc GeneralSettings.xml probably does not contain property name " + name +
                    ".\r\nSys err; " + cExcp.Message;
                MessageBox.Show(errMess);

                Console.WriteLine(errMess);

                return "";
            }
        }
    }
}
