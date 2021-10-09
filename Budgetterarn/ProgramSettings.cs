using System;
using Budgeter.Core.BudgeterConstants;

namespace Budgetterarn
{
    public class ProgramSettings
    {
        //public ProgramSettings()
        //{
        //    AutoLoadEtc = AutoLoadEtcFromXml();
        //}

        //public bool AutoLoadEtc { get; }

        //private static bool AutoLoadEtcFromXml()
        //{
        //    var s = GeneralSettingsGetter.GetStringSetting("AutonavigateEtc");

        //    return bool.TryParse(s, out var b) && b;
        //}

        public static BankType BankType
        {
            get
            {
                //var fromXls = GeneralSettingsGetter.GetStringSetting("BankUrl");
                //if (fromXls == null) return 0;

                //var matchedString = fromXls.ToLower();
                //matchedString = matchedString[0].ToString().ToUpper() + matchedString.Substring(1);
                //return (BankType) Enum.Parse(typeof(BankType), matchedString);
                return (BankType.Swedbank);
            }
        }
    }
}