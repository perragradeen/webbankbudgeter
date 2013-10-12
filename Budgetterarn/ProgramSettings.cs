using System;

namespace Budgetterarn {
    class ProgramSettings {
        public static BankType BankType {
            get {
                var fromXls = GeneralSettings.GetStringSetting("BankUrl");
                if (fromXls != null) {
                    var matchedString = fromXls.ToLower();
                    matchedString = matchedString[0].ToString().ToUpper() + matchedString.Substring(1);
                    return (BankType)Enum.Parse(typeof(BankType), matchedString);
                }

                return 0;
            }
        }
    }

    internal enum BankType {
        Handelsbanken,
        Swedbank,
        Mobilhandelsbanken,
    }
}
