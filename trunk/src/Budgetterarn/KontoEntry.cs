using System;
using System.Globalization;
using Budgetterarn.Operations;

namespace Budgetterarn
{
    //Todo: lägg in en katalog "Entities"....
    class KontoEntry
    {
        public DateTime Date { get; set; }
        public string Info { get; set; }
        public double KostnadEllerInkomst { get; set; }
        public double SaldoOrginal { get; set; }//Ska vara unikt och ingå i nyckel. Detta är det som står i kontoutdraget. Om inte orginalsaldot finns med, från tex. gamla Excelposter, så blir detta ack.saldo.
        public double AckumuleratSaldo { get; set; }//Är det saldo som inte nödvändigtvis stämmer med kontoutdraget utan är en beräkning av kostnaden och föregående post. Saknas detta så sätter man in formel, rättare sagt, man sätter alltid in formel här som räknar kostnaden och föregående ack saldo.
        public string TypAvKostnad { get; set; }//kunde varit en enum, men då hade den inte kunnat läsas in från Xml.
        //Ev. ha en övrig inf, en array av string typ, med referenser kommentarer etc

        //dateFormat = currDate.Date.ToString("yyyy-MM-dd", svecia);// string.IsNullOrEmpty(dateFormat).ToString();
        public string DateString { get { return Date.ToString(); } }//"yyyy-MM-dd"); } }
        //De 3 nedan kan innehålla annat än vad de heter, gamla grejjer kan ligga där de skulle vara eg.
        public string ExtendedInfo { get; set; }//Mer detaljer, typ maträtt etc
        public string Place { get; set; }//Var för någonstans, eller vilken händelse, te.x. Stockholmsresan sommar 2009, hemma eller jobblunch
        public string TimeOfDay { get; set; }//Klockslag, t.ex. 12.10
        public KontoEntryType EntryType { get; set; }

        static KontoEntryType EntryTypeFromString(string entryType)
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


        public string KeyForThis
        {
            get { return StringFuncions.mergeStringArrayToString(new[] { DateString, Info, KostnadEllerInkomst.ToString(), SaldoOrginal.ToString(), ExtendedInfo }); }
            //Ta inte med TypAvKostnad, för det finns inget i Html om det, och då kan man inte jämföra med excel eftersom nycklarna e olika.
        }

        //public int ExcelRowNumber { get; set; }
        public object[] RowToSaveForThis
        {
            //Date.Month.ToString()=="1"
            //new string[1]{
            //                "=B" + currRow + "-D" + currRow //BC när den är färdig, men "new" finns som rad o då blird det B+D
            //                }

            get
            {
                return new object[] { "" + Date.ToString("yyyy"), Date.Month.ToString(),//Datum i år o månad
                        Date, //DateString, 
                        Info, KostnadEllerInkomst.ToString(), SaldoOrginal.ToString(), AckumuleratSaldo.ToString(), TypAvKostnad, ExtendedInfo, Place, TimeOfDay, EntryType.ToString() };
            }
        }

        public string[] RowToSaveToUi//(bool toExcel)
        {
            get { return new[] { DateString, Info, KostnadEllerInkomst.ToString(), SaldoOrginal.ToString(), AckumuleratSaldo.ToString(), TypAvKostnad }; }

        }

        /// <summary>
        /// Bytt plats på typ och kost
        /// </summary>
        public string[] RowToSaveToUiSwitched//(bool toExcel)
        {
            get { return new[] { DateString, Info, TypAvKostnad, KostnadEllerInkomst.ToString(), SaldoOrginal.ToString(), AckumuleratSaldo.ToString() }; }

        }

        public bool ThisIsDoubleDoNotAdd { get; set; }

        //For debugging so far. Shows Info on hower over vars
        public override string ToString() { return Info; }

        public System.Drawing.Color FontFrontColor { get; set; }

        public string ReplaceThisKey { get; set; }


        #region Create new KontoEntry and helpfnc.
        public KontoEntry() { }

        public KontoEntry(string[] inArray)
            : this(inArray, false)//CreateKE
        {
            //KontoEntry(inArray, false);//return CreateKE
        }

        readonly bool mFromXls;
        public KontoEntry(string[] inArray, bool fromXls)//CreateKE
        {
            mFromXls = fromXls;

            //Kolla alla insträngar, om någon är null, kan det bli fel ordning på inläsningen, denna antar att beskrivningen är tom
            var index = 0;
            foreach (var currentValue in inArray)
            {
                if (currentValue == null)
                {
                    //Förskjut alla värden ett steg pga nullen, om det inte är från allkort...
                    //inArray[3] = inArray[2];

                    //inArray[2] = inArray[1];
                    //inArray[1] = "";
                }

                index++;
            }

            #region För datum

            var useThisCulture = new CultureInfo("en-US");

            #endregion
            //he, skulle kunna sätta en ny array av inArray och om det saknas värde så sätt "" eller " "

            //gör ev. om från const tilldyn. sätt nästa rel. den första, så kan man ta bort eller skjuta in en lätt
            #region Columnnummer konstanter
            //Todo: gör om till enum
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

            //ev. spara alla celler efter typ som metadata, typ inArray[6..Length] //Gör backup så länge

            if (typ == string.Empty)
            {
                typ = string.Empty;//"";//onödig nu, men ev. sätta " " här
            }

            #endregion
            //TODO: Gör något med ack.salod

            #region Datumkonvertering etc
            //felhantering, sätt dagens datum om det är fel
            DateTime currDate = DateTime.Parse("1/1/1900 12:00:00 AM", useThisCulture);
            try
            {
                if (string.IsNullOrEmpty(date) || date.Length <= 3) { }
                else
                {
                    currDate = DateTime.Parse(date, useThisCulture);
                }
            }
            catch (Exception dateExc)
            {
                Console.WriteLine("Error in parsing date: " + dateExc.Message);
            }

            var svecia = new CultureInfo("sv-SE");

            var dateFormat = currDate.Date.ToString("yyyy-MM-dd", svecia);

            if (string.IsNullOrEmpty(date))
            {
                return;
            }

            #endregion
            #region Sätt värden till denna klass
            Date = DateTime.Parse(dateFormat);
            Info = info;
            KostnadEllerInkomst = GetValueFromEntry(cost);
            SaldoOrginal = GetValueFromEntry(saldo);
            AckumuleratSaldo = GetValueFromEntry(ackumuleratSaldo);
            TypAvKostnad = typ;

            //Sätt default entry type
            EntryType = KontoEntryType.Regular;

            //Sätt metadata
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

        private string RowThatExists(string[] inArray, int columnNumber)
        {
            return inArray.Length > columnNumber && inArray[columnNumber] != null ? inArray[mFromXls ? columnNumber + 2 : columnNumber] : "";
        }

        public static double GetValueFromEntry(string val)
        {
            //Todo, felkollar
            var cultureToUse = System.Threading.Thread.CurrentThread.CurrentCulture;
            if (string.IsNullOrEmpty(val))
                return 0.0;

            if (val.Contains("."))
                cultureToUse = new CultureInfo("en-US");
            else if (val.Contains(","))
                cultureToUse = new CultureInfo("sv-SE");

            double tempd;
            return double.TryParse(val.Replace(" ", string.Empty), NumberStyles.Number, cultureToUse, out tempd) ? Math.Round(tempd, 2) : 0.0;
        }

        #endregion
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
