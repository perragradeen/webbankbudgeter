using System;

namespace Budgetterarn
{
    public class ProgramSettings
    {
        public ProgramSettings()
        {
            AutoLoadEtc = AutoLoadEtcFromXml();
        }

        public bool AutoLoadEtc { get; private set; }

        private bool AutoLoadEtcFromXml()
        {
            var s = GeneralSettings.GetStringSetting("AutonavigateEtc");

            bool b;
            return bool.TryParse(s, out b) && b;
        }

        public static BankType BankType
        {
            get
            {
                var fromXls = GeneralSettings.GetStringSetting("BankUrl");
                if (fromXls != null)
                {
                    var matchedString = fromXls.ToLower();
                    matchedString = matchedString[0].ToString().ToUpper() + matchedString.Substring(1);
                    return (BankType)Enum.Parse(typeof(BankType), matchedString);
                }

                return 0;
            }
        }
    }

    public enum BankType
    {
        Handelsbanken, 
        Swedbank, 
        Mobilhandelsbanken, 
    }
}