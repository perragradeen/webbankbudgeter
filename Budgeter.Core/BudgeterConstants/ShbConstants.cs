using System.Collections.Generic;

namespace Budgeter.Core.BudgeterConstants
{
    public class ShbConstants
    {
        public const string SheetName = "Kontoutdrag_officiella"; // "Kontoutdrag f.o.m. 0709 bot.up.";

        public const string LönekontoName = "LÖNEKONTO";

        // Ex.
        // Saldo på kontot:	69 563,47	
        // Kortköp - ej fakturerat:	-2 256,30
        // Kortköp - fakturerat:	-17 312,64
        public const string AllkortName = "Allkort";
        public const string AllkortEjFaktureratName = "ejFaktureratEtc";
        public const string AllkortFaktureratName = "ejFaktureratEtc_fakturerat";

        public static readonly List<string> SwedbankSaldonames =
            new List<string>
            {
                "PrivatPG (Privatkonto) 8105-9,964 260 134-9",
                "Gemensamt (Privatkonto) 8105-9,964 260 138-0",
                "Vivis"
            };
        public const string ShbAllkortKreditKontoIdentifierare = "ALLKORT - korttransaktioner";
    }
}
