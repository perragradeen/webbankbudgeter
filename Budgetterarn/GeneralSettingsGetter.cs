using System;
using System.IO;
using System.Linq;
using RefLesses;
using Serializers;

namespace Budgetterarn
{
    /// <summary>
    /// Returns a setting from file, if the setting wanted is not found, the return result is an empty string or false
    /// </summary>
    public class GeneralSettingsGetter
    {
        public GeneralSettingsGetter(string filePath = "")
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                filePath = GetGeneralSettingsPath();

            }
            AllGeneralSettings = SerializationFunctions
                .DeserializeObject<GeneralSettings>(filePath);
        }

        public GeneralSettings AllGeneralSettings { get; private set; }
        private string GetGeneralSettingsPath()
        {
            var path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"Data\"
            );
            path = Path.Combine(path, @"GeneralSettings.xml");
            return path;
        }

        private Property GetSettingProperty(string name)
        {
            return AllGeneralSettings.FirstOrDefault(s =>
                s.Name == name);
        }

        public string GetStringSetting(string name)
        {
            return GetSettingProperty(name)?.Value;
        }

        public string GetTextFileStringSetting(string name)
        {
            var prop = GetSettingProperty(name);
            var textFileRowNumber = prop.RownumberInTextfile;
            var textFilePath = prop.TextfileName;

            TextReader fileReader = new StreamReader(textFilePath);

            var stringFromFile = "";
            for (var i = 0; i <= textFileRowNumber.SafeGetIntFromString(); i++)
            {
                stringFromFile = fileReader.ReadLine();
            }

            return stringFromFile;
        }
        //private static readonly XmlDocument doc = new XmlDocument();
        //static GeneralSettingsGetter()
        //{
        //    try
        //    {
        //        string path = GetGeneralSettingsPath();
        //        doc.Load(path);
        //    }
        //    catch (Exception loadExp)
        //    {
        //        Console.WriteLine(@"Error in Config(): " + loadExp.Message);
        //        throw new Exception("Config()", loadExp);
        //    }
        //}



        //public static string GetStringSetting(string name)
        //{
        //    try
        //    {
        //        return ((XmlElement) (doc.SelectSingleNode("//property[@Name='" + name + "']")))?.GetAttribute("Value");
        //    }
        //    catch (Exception cExcp)
        //    {
        //        Console.WriteLine(
        //            @"Error in config: Settings doc GeneralSettings.xml probably does not contain property name " + name
        //            + @".\r\nSys err; " + cExcp.Message);

        //        return "";
        //    }
        //}

        //public static string GetTextFileStringSetting(string name)
        //{
        //    try
        //    {
        //        var textFileRowNumber =
        //            ((XmlElement) (doc.SelectSingleNode("//property[@Name='" + name + "']")))?.GetAttribute(
        //                "RownumberInTextfile");
        //        var textFilePath = AppDomain.CurrentDomain.BaseDirectory
        //                           + ((XmlElement) (doc.SelectSingleNode("//property[@Name='" + name + "']")))?
        //                           .GetAttribute("TextfileName");

        //        TextReader fileReader = new StreamReader(textFilePath);

        //        var stringFromFile = "";
        //        for (var i = 0; i <= textFileRowNumber.SafeGetIntFromString(); i++)
        //        {
        //            stringFromFile = fileReader.ReadLine();
        //        }

        //        return stringFromFile;
        //    }
        //    catch (Exception exception)
        //    {
        //        var errMess =
        //            "Error in config: Settings doc GeneralSettings.xml probably does not contain property name " + name
        //            + ".\r\nSys err; " + exception.Message;
        //        MessageBox.Show(errMess);

        //        Console.WriteLine(errMess);

        //        return string.Empty;
        //    }
        //}
    }
}