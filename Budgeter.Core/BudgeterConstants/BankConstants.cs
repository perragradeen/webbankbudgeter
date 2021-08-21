using System.Collections.Generic;

namespace Budgeter.Core.BudgeterConstants
{
    public class BankConstants
    {
        public const string SheetName = "Kontoutdrag_officiella"; // "Kontoutdrag f.o.m. 0709 bot.up.";

        public static readonly List<string> SwedbankSaldonames =
            new List<string>
            {
                "PrivatPG (Privatkonto) 8105-9,964 260 134-9",
                "Gemensamt (Privatkonto) 8105-9,964 260 138-0",
                "Vivis"
            };
    }
}
