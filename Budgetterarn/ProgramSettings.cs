using Budgeter.Core.BudgeterConstants;

namespace Budgetterarn
{
    public class ProgramSettings
    {
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