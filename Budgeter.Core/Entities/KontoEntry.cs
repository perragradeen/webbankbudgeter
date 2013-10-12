using System;
using System.Drawing;
using RefLesses;

namespace Budgeter.Core.Entities
{
    // Todo: lägg in en katalog "Entities"....
    public class KontoEntry
    {
        public DateTime Date { get; set; }
        public string Info { get; set; }
        public double KostnadEllerInkomst { get; set; }
        public double SaldoOrginal { get; set; }

        // Ska vara unikt och ingå i nyckel. Detta är det som står i kontoutdraget. Om inte orginalsaldot finns med, från tex. gamla Excelposter, så blir detta ack.saldo.
        public double AckumuleratSaldo { get; set; }

        // Är det saldo som inte nödvändigtvis stämmer med kontoutdraget utan är en beräkning av kostnaden och föregående post. Saknas detta så sätter man in formel, rättare sagt, man sätter alltid in formel här som räknar kostnaden och föregående ack saldo.
        public string TypAvKostnad { get; set; } // kunde varit en enum, men då hade den inte kunnat läsas in från Xml.
        // Ev. ha en övrig inf, en array av string typ, med referenser kommentarer etc

        // dateFormat = currDate.Date.ToString("yyyy-MM-dd", svecia);// string.IsNullOrEmpty(dateFormat).ToString();
        public string DateString
        {
            get
            {
                return Date.ToString();
            }
        } // "yyyy-MM-dd"); } }
        // De 3 nedan kan innehålla annat än vad de heter, gamla grejjer kan ligga där de skulle vara eg.
        public string ExtendedInfo { get; set; } // Mer detaljer, typ maträtt etc
        public string Place { get; set; }

        // Var för någonstans, eller vilken händelse, te.x. Stockholmsresan sommar 2009, hemma eller jobblunch
        public string TimeOfDay { get; set; } // Klockslag, t.ex. 12.10
        public KontoEntryType EntryType { get; set; }

        public string KeyForThis
        {
            get
            {
                return
                    StringFuncions.MergeStringArrayToString(
                        new[]
                        {
                           DateString, Info, KostnadEllerInkomst.ToString(), SaldoOrginal.ToString(), ExtendedInfo 
                        });
            }

            // Ta inte med TypAvKostnad, för det finns inget i Html om det, och då kan man inte jämföra med excel eftersom nycklarna e olika.
        }

        // public int ExcelRowNumber { get; set; }
        public object[] RowToSaveForThis
        {
            // Date.Month.ToString()=="1"
            // new string[1]{
            // "=B" + currRow + "-D" + currRow //BC när den är färdig, men "new" finns som rad o då blird det B+D
            // }
            get
            {
                return new object[]
                       {
                           "" + Date.ToString("yyyy"), Date.Month.ToString(), // Datum i år o månad

                           // På amerikanskt Excel så måste man spara i annat format. Eller år, månad, dag för sig i varsin cell. Men det är svårt att här veta vilket typ av office man kommer att öppna i. Men det kommer sparas i Excel via interop på ett visst sätt.
                           // Man kan kanske kolla här vilken kultur som är här och se om den skiljer från installerat office...
                           // Det blir inte problem om:
                           // OS = Us och Excel (office) = Us
                           // OS = Swe och Excel (office) = Swe
                           // Då sparas datum i samma kultur.
                           // Men om man har Os = Swe och Excel = US, så sparas datum i Swe-format och Excel sparar i fel format...
                           Date, // DateString, 
                           Info, KostnadEllerInkomst.ToString(), SaldoOrginal.ToString(), 
                           AckumuleratSaldo.ToString(), TypAvKostnad, ExtendedInfo, Place, TimeOfDay, 
                           EntryType.ToString()
                       };
            }
        }

        // public string[] RowToSaveToUi//(bool toExcel)
        // {
        // get { return new[] { DateString, Info, KostnadEllerInkomst.ToString(), SaldoOrginal.ToString(), AckumuleratSaldo.ToString(), TypAvKostnad }; }

        // }

        /// <summary>
        /// Bytt plats på typ och kost
        /// </summary>
        public string[] RowToSaveToUiSwitched // (bool toExcel)
        {
            get
            {
                return new[]
                       {
                           DateString, Info, TypAvKostnad, KostnadEllerInkomst.ToString(), SaldoOrginal.ToString(), 
                           AckumuleratSaldo.ToString()
                       };
            }
        }

        public bool ThisIsDoubleDoNotAdd { get; set; }

        // For debugging so far. Shows Info on hower over vars
        public Color FontFrontColor { get; set; }

        public string ReplaceThisKey { get; set; }

        #region Create new KontoEntry and helpfnc.

        private readonly bool mFromXls;

        public KontoEntry()
        {
        }

        public KontoEntry(string[] inArray, bool fromXls = false) // CreateKE
        {
            mFromXls = fromXls;

            // Kolla alla insträngar, om någon är null, kan det bli fel ordning på inläsningen, denna antar att beskrivningen är tom
            var index = 0;
            foreach (var currentValue in inArray)
            {
                if (currentValue == null)
                {
                    // Förskjut alla värden ett steg pga nullen, om det inte är från allkort...
                    // inArray[3] = inArray[2];

                    // inArray[2] = inArray[1];
                    // inArray[1] = "";
                }

                index++;
            }

            // he, skulle kunna sätta en ny array av inArray och om det saknas värde så sätt "" eller " "

            // gör ev. om från const tilldyn. sätt nästa rel. den första, så kan man ta bort eller skjuta in en lätt
            #region Columnnummer konstanter

            // Todo: gör om till enum
            const int saldoColumnNumber = 3;
            const int ackSaldoColumnNumber = 4;
            const int typColumnNumber = 5;

            const int columnNumberExtendedInfo = 6;
            const int columnNumberPlace = 7;
            const int columnNumberTimeOfDay = 8;
            const int columnNumberEntryType = 9;

            #endregion

            #region Hämta info från inArray

            var date = RowThatExists(inArray, 0);
            var info = RowThatExists(inArray, 1);
            var cost = RowThatExists(inArray, 2);
            var saldo = RowThatExists(inArray, saldoColumnNumber);
            var ackumuleratSaldo = RowThatExists(inArray, ackSaldoColumnNumber);
            var typ = RowThatExists(inArray, typColumnNumber);

            // ev. spara alla celler efter typ som metadata, typ inArray[6..Length] //Gör backup så länge
            if (typ == string.Empty)
            {
                typ = string.Empty; // "";//onödig nu, men ev. sätta " " här
            }

            #endregion

            // TODO: Gör något med ack.salod
            #region Sätt värden till denna klass

            Date = DateFunctions.ParseDateWithCultureEtc(date);
            Info = info;
            KostnadEllerInkomst = cost.GetValueFromEntry();
            SaldoOrginal = saldo.GetValueFromEntry();
            AckumuleratSaldo = ackumuleratSaldo.GetValueFromEntry();
            TypAvKostnad = typ;

            // Sätt default entry type
            EntryType = KontoEntryType.Regular;

            // Sätt metadata
            if (!fromXls)
            {
                return;
            }

            ExtendedInfo = RowThatExists(inArray, columnNumberExtendedInfo);
            Place = RowThatExists(inArray, columnNumberPlace);
            TimeOfDay = RowThatExists(inArray, columnNumberTimeOfDay);
            EntryType = EntryTypeFromString(RowThatExists(inArray, columnNumberEntryType));

            #endregion
        }

        public KontoEntry(BankRow fromBank)
        {
            Date = fromBank.Date;
            Info = fromBank.EventValue;
            KostnadEllerInkomst = fromBank.BeloppValue.GetValueFromEntry();
            SaldoOrginal = fromBank.SaldoValue.GetValueFromEntry();
        }

        public string RowThatExists(string[] inArray, int columnNumber)
        {
            return inArray.Length > columnNumber && inArray[columnNumber] != null
                       ? inArray[mFromXls ? columnNumber + 2 : columnNumber]
                       : "";
        }

        #endregion

        private static KontoEntryType EntryTypeFromString(string entryType)
        {
            if (string.IsNullOrEmpty(entryType))
            {
                return KontoEntryType.Regular;
            }

            KontoEntryType kontoEntryType;

            try
            {
                kontoEntryType = (KontoEntryType)Enum.Parse(typeof(KontoEntryType), entryType, true);
            }
            catch (Exception enumEx)
            {
                Console.WriteLine("Error in EntryTypeFromString. String: " + entryType + "\r\n" + enumEx.Message);
                return KontoEntryType.Regular;
            }

            return kontoEntryType;
        }

        public override string ToString()
        {
            return Info;
        }
    }

    public enum KontoEntryType
    {
        Regular = 0, 
        Ignore, 
        SplitChild, 
        Split, 
        AllkortsFakturaDragning
    }
}