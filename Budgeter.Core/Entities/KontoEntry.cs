using RefLesses;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace Budgeter.Core.Entities
{
    public class KontoEntry
    {
        public DateTime Date { get; set; }
        private int Year { get; }
        private int Month { get; }
        private int Day { get; }

        public string Info { get; set; }
        public double KostnadEllerInkomst { get; set; }
        public double SaldoOrginal { get; set; }

        // Ska vara unikt och ingå i nyckel. Detta är det som står i kontoutdraget. Om inte orginalsaldot finns med, från tex. gamla Excelposter, så blir detta ack.saldo.
        public double AckumuleratSaldo { get; set; }

        // Är det saldo som inte nödvändigtvis stämmer med kontoutdraget utan är en beräkning av kostnaden och föregående post. Saknas detta så sätter man in formel, rättare sagt, man sätter alltid in formel här som räknar kostnaden och föregående ack saldo.
        public string TypAvKostnad { get; set; } // kunde varit en enum, men då hade den inte kunnat läsas in från Xml.
        // Ev. ha en övrig inf, en array av string typ, med referenser kommentarer etc

        public bool ForUi { get; set; }

        private string DateString
        {
            get
            {
                var datesTemp = Date.ToString(CultureInfo.InvariantCulture);
                if (ForUi && datesTemp.Contains("00"))
                {
                    return Date.ToString("yyyy-MM-dd");
                }

                return Date.ToString(CultureInfo.InvariantCulture);
            }
        }

        private string DateStringFrom3Ints => Date.ToString("yyyy-MM-dd");


        // De 3 nedan kan innehålla annat än vad de heter, gamla grejjer kan ligga där de skulle vara eg.
        private string ExtendedInfo { get; } // Mer detaljer, typ maträtt etc

        private string Place { get; }

        // Var för någonstans, eller vilken händelse, te.x. Stockholmsresan sommar 2009, hemma eller jobblunch
        private string TimeOfDay { get; } // Klockslag, t.ex. 12.10
        public KontoEntryType EntryType { get; }

        public string KeyForThis
        {
            get
            {
                return
                    StringFunctions.MergeStringArrayToString(
                        new[]
                        {
                            DateStringFrom3Ints,
                            Info,
                            KostnadEllerInkomst.ToString(CultureInfo.InvariantCulture),
                            SaldoOrginal.ToString(CultureInfo.InvariantCulture),
                            ExtendedInfo
                        });
            }

            // Ta inte med TypAvKostnad, för det finns inget i Html om det, och då kan man inte jämföra med excel eftersom nycklarna e olika.
        }

        public bool KeyEqauls(KontoEntry entryNew)
        {
            return KeyForThis.Equals(entryNew.KeyForThis);
        }

        public object[] RowToSaveForThis
        {
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
                    Date.Day, // DateString, 
                    Info, KostnadEllerInkomst.ToString(CultureInfo.InvariantCulture),
                    SaldoOrginal.ToString(CultureInfo.InvariantCulture),
                    AckumuleratSaldo.ToString(CultureInfo.InvariantCulture), 
                    TypAvKostnad, 
                    ExtendedInfo, 
                    Place, 
                    TimeOfDay,
                    EntryType.ToString()
                };
            }
        }

        /// <summary>
        /// Bytt plats på typ och kost. För Ui
        /// </summary>
        public string[] RowToSaveToUiSwitched
        {
            get
            {
                return new[]
                {
                    DateString,
                    Info, 
                    TypAvKostnad, 
                    KostnadEllerInkomst.ToString(CultureInfo.InvariantCulture), 
                    SaldoOrginal.ToString(CultureInfo.InvariantCulture),
                    AckumuleratSaldo.ToString(CultureInfo.InvariantCulture)
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

        public KontoEntry(object[] inArray, bool fromXls = false)
        {
            if (InarrayEmpty(inArray))
            {
                return;
            }

            mFromXls = fromXls;
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

            // ev: Gör något med ack.salod

            #region Sätt värden till denna klass

            Date = DateFunctions.ParseDateWithCultureEtc(date);
            if (mFromXls)
            {
                Year = RowThatExistsNoSpecial(inArray, 0).SafeGetIntFromString();
                Month = RowThatExistsNoSpecial(inArray, 1).SafeGetIntFromString();
                Day = RowThatExistsNoSpecial(inArray, 2).SafeGetIntFromString();

                if (Day == 0)
                {
                    Day = Date.Day;
                }

                try
                {
                    Date = new DateTime(Year, Month, Day);
                }
                catch (Exception e)
                {

                    throw new Exception("Datumfel. data: " + Year + " " + Month + " " + Day, e);
                }
            }

            Info = info;
            KostnadEllerInkomst = cost.GetDoubleValueFromStringEntry();
            SaldoOrginal = saldo.GetDoubleValueFromStringEntry();
            AckumuleratSaldo = ackumuleratSaldo.GetDoubleValueFromStringEntry();
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

        private static bool InarrayEmpty(IEnumerable<object> inArray)
        {
            return inArray.All(item => string.IsNullOrEmpty(
                (string) item));
        }

        public KontoEntry(BankRow fromBank)
        {
            Date = fromBank.Date;
            Info = fromBank.EventValue;
            KostnadEllerInkomst = fromBank.BeloppValue.GetDoubleValueFromStringEntry();
            SaldoOrginal = fromBank.SaldoValue.GetDoubleValueFromStringEntry();
        }

        public string RowThatExists(object[] inArray, int columnNumber)
        {
            return (
                inArray.Length > columnNumber && inArray[columnNumber] != null
                    ? inArray[mFromXls ? columnNumber + 2 : columnNumber]
                    : string.Empty
            ) as string;
        }

        private static string RowThatExistsNoSpecial(IReadOnlyList<object> inArray, int columnNumber)
        {
            return inArray.Count > columnNumber && inArray[columnNumber] != null
                ? inArray[columnNumber].ToString()
                : string.Empty;
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
                kontoEntryType = (KontoEntryType)Enum.Parse(
                    typeof(KontoEntryType),
                    entryType,
                    true);
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
}