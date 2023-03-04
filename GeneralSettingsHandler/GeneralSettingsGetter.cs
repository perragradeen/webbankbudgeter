using RefLesses;
using Serializers;

namespace GeneralSettingsHandler
{
    /// <summary>
    /// Returns a setting from file, if the setting wanted is not found, the return result is an empty string or false
    /// </summary>
    public class GeneralSettingsGetter
    {
        public GeneralSettingsGetter(string filePath)
        {
            AllGeneralSettings = SerializationFunctions
                .DeserializeObject<GeneralSettings>(filePath);
        }

        public GeneralSettings AllGeneralSettings { get; private set; }
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
    }
}