using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Budgeter.Core
{
    public class ShbConstants
    {
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
                "Privatkonto 8417-8,4 751 687-7", 
                "Servicekonto 8417-8,4 778 356-8", 
                "Servicekonto 8417-8,914 636 458-4", 
                "e-sparkonto 8417-8,983 306 619-5"
            };
        public const string ShbAllkortKreditKontoIdentifierare = "ALLKORT - korttransaktioner";
    }
}
