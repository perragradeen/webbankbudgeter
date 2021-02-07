using Budgeter.Core.BudgeterConstants;
using Budgetterarn.DAL;
using LoadTransactionsFromFile;
using System;
using System.Windows.Forms;

namespace Budgetterarn.WebCrawlers
{
    public class DocChecker : ShbConstants
    {
        public KontoEntriesHolder _kontoEntriesHolder;
        public string HtmlBodyInnerText { get; set; }

        private HtmlElement HtmlBody
        {
            get
            {
                var doc = _kontoEntriesHolder.Doc;
                if (doc == null || doc.Body == null)
                {
                    return null;
                }

                return doc.Body;
            }
        }

        public DocChecker()
        {
            HtmlBodyInnerText = HtmlBody.InnerText;
        }

        public DocChecker(KontoEntriesHolder kontoEntriesHolder)
        {
            _kontoEntriesHolder = kontoEntriesHolder;
            HtmlBodyInnerText = HtmlBody.InnerText;
        }

        //public DocChecker (HtmlDocument doc,
        //    SortedList kontoEntries,
        //    SortedList newKontoEntries,
        //    ref bool somethingChanged,
        //    SaldoHolder saldoHolder)
        //    : this()
        //{
        //    kontoEntriesHolder = new KontoEntriesHolder
        //    {
        //        Doc = doc,
        //        KontoEntries = kontoEntries,
        //        NewKontoEntries = newKontoEntries,
        //        SaldoHolder = saldoHolder,
        //        SomethingChanged = somethingChanged
        //    };
        //}

        public void CheckDocForEntries()
        {
            try
            {
                // Hämta saldon för Svenska Handelsbanken
                GetShbSaldos();

                GetKontoEntriesLöne();
            }
            catch (Exception)
            {
                // Swallow!!! = LAZY
            }

            try
            {
                // Hämta saldon för Svenska Handelsbanken
                GetShbSaldos();

                GetKontoEntriesAllkort();
            }
            catch (Exception)
            {
                // Swallow!!! = LAZY
            }

            try
            {
                // Hämta saldon för Svenska Handelsbanken
                GetShbSaldos();

                GetKontoEntriesAllkortKrediter();
            }
            catch (Exception)
            {
                // Swallow!!! = LAZY
            }
        }

        private void GetKontoEntriesAllkort()
        {
            var body = _kontoEntriesHolder.Doc.Window.Frames[0].Document.Body;

            // Verifierat historiska
            //var allkortKontoEntriesElement = body.Children[10].Children[10].Children[0]; // <2016-05-22

            var allkortKontoEntriesElement = body.Children[14].Children[10].Children[0];

            // Kolla saldon igen
            GetShbSaldos(body.InnerText);

            LoadKonton.CheckHtmlTr(allkortKontoEntriesElement,
                _kontoEntriesHolder.KontoEntries,
                _kontoEntriesHolder.NewKontoEntries,
                false);

            //foreach (HtmlElement currentElement in kontoEntriesHolder.Doc.Body.Children)
            //{
            //    foreach (HtmlElement subElement in currentElement.All)
            //    {
            //        // Leta upp den andra tabellen
            //        if (subElement.TagName.ToLower() != "table")
            //        {
            //            continue;
            //        }

            //        var foundDebug = StringFuncions.GetTextBetweenStartAndEndText(subElement.InnerHtml,
            //            "Res", "tradatum");
            //        if (foundDebug == "kon")
            //        {

            //        }
            //    }
            //}
        }

        private void GetKontoEntriesLöne()
        {
            var body = _kontoEntriesHolder.Doc.Window.Frames[0].Document.Body;

            //var kontoEntriesElement = body.Children[12].Children[1].Children[0];

            // Verifierat historiska
            //var kontoEntriesElement = body.Children[11].Children[8].Children[0];

            //Funkade 2016-04-09 < 19:42
            var kontoEntriesElement = body.Children[14].Children[8].Children[0];
            GetShbSaldos(body.Children[11].InnerText);


            //var kontoEntriesElement = body.Children[10].Children[8].Children[0];
            //var allkortKontoEntriesElement = body.Children[17].Children[14].Children[0];
            //(body.Children)).Items[11])).Children)).Items[8]

            // Kolla saldon igen
            //GetShbSaldos(body.InnerText);

            LoadKonton.CheckHtmlTr(kontoEntriesElement,
                _kontoEntriesHolder.KontoEntries,
                _kontoEntriesHolder.NewKontoEntries,
                false);
        }

        private void GetKontoEntriesAllkortKrediter()
        {
            // kontoEntriesHolder.Doc.B
            var body = _kontoEntriesHolder.Doc.Window.Frames[0].Document.Body;

            //var baseElement = body.FirstChild
            //    .NextSibling.NextSibling.NextSibling.NextSibling.NextSibling.NextSibling;

            // Verifierat historiska
            //body.Children[6].Children[14].Children[0];
            //body.Children[9.Children[14].Children[0];

            var allkortKontoEntriesElement =
                body.Children[9].Children[14].Children[0];
            //body.Children[14].Children[4].Children[0];
            //baseElement.Children[17].FirstChild;

            LoadKonton.CheckHtmlTr(allkortKontoEntriesElement,
                _kontoEntriesHolder.KontoEntries,
                _kontoEntriesHolder.NewKontoEntries,
                true);
        }

        public void GetShbSaldos(string htmlBodyInnerText = null)
        {
            var saldoValueAdder = new SaldoValueAdder(
                htmlBodyInnerText ?? HtmlBodyInnerText
                , _kontoEntriesHolder.SaldoHolder);

            saldoValueAdder.AddSaldo(ShbConstants.AllkortName,
                "Saldo på kontot:", "Kontoform:");

            saldoValueAdder.AddSaldo(ShbConstants.AllkortEjFaktureratName,
                "Kortköp - ej fakturerat:", "Clearingnummer:");

            saldoValueAdder.AddSaldo(ShbConstants.AllkortFaktureratName,
                "Kortköp - fakturerat:", "Kontovillkor och IBAN");

            saldoValueAdder.AddSaldo(ShbConstants.LönekontoName,
                "Saldo:", "Clearingnummer:");
        }
    }
}



//Leta upp: "För period fr o m:t o m:"
//const string toFind = "Reskontradatum Transaktionsdatum Text Belopp Saldo";//"För period fr o m:t o m:"; // : "De senaste transaktionerna";
//if (doc == null || doc.Body == null) { }
//else
//{
//    foreach (HtmlElement currentElement in doc.Body.Children)
//    {
//        #region Gå igenom alla element för denna ram
//        if (currentElement.OuterText == null)
//            continue;

//        var allkortKredit = (currentElement.OuterText != null &&
//           StringFuncions.ContainsClean(currentElement.OuterText,
//            //"Konto: 629 011 192  Period:"
//            ShbAllkortKreditKontoIdentifierare
//            ));

//        //Om man är i lönekontot, den har lite annan struktur
//        var löneKonto = (currentElement.OuterText != null &&
//            currentElement.OuterText.Trim().Contains("Konto: 629 010 552  Period:"));
//        //629 010 552"
//        //Disponibelt belopp - med kredit:
//        //Konto: 629 011 192  Period:

//        var nuKreditKonton = (currentElement.OuterText != null
//            &&
//            (
//                currentElement.OuterText.Trim().Contains("Urval:Ej fakturerat") ||
//                currentElement.OuterText.Trim().Contains("Urval:Kortköp - ej fakturerat")
//            ));//Urval:Ej fakturerat

//        var noTables = 0;

//        if (nuKreditKonton)
//        {
//            foreach (HtmlElement subElement in currentElement.All)
//            {
//                #region Leta upp den andra tabellen

//                if (subElement.TagName.ToLower() != "table")
//                {
//                    continue;
//                }

//                noTables++;
//                if (noTables == 4) //Hoppa över 1:a tabellen
//                {
//                    LoadKonton.CheckHtmlTr(subElement, kontoEntries, newKontoEntries, true);
//                }

//                #endregion
//            }
//        }

//        if (((currentElement.OuterText == null ||
//        (!currentElement.OuterText.Trim().StartsWith(toFind) &&
//        !currentElement.OuterText.Trim().Contains(toFind))) && !allkortKredit))
//        {
//            continue;
//        }

//        //Leta upp den andra tabellen
//        noTables = 0;
//        foreach (HtmlElement subElement in currentElement.All)
//        {
//            #region Leta upp den andra tabellen

//            if (subElement.TagName.ToLower() != "table")
//            {
//                continue;
//            }

//            #region Old
//            //if (allkortKredit) {
//            //    noTables++;
//            //    if (noTables > 1) //Hoppa över 1:a tabellen
//            //    {
//            //        CheckHtmlTr(subElement, kontoEntries, newKontoEntries);
//            //    }
//            //} else {

//            #endregion
//            noTables++; //va fel här innan...
//            #region Old
//            if (noTables == 13 && !löneKonto && allkortKredit)
//            {
//                LoadKonton.CheckHtmlTr(subElement, kontoEntries, newKontoEntries);
//            }
//            else
//            #endregion
//                if (noTables == 12)
//                {
//                    LoadKonton.CheckHtmlTr(subElement, kontoEntries, newKontoEntries);
//                }
//            //}

//            #endregion
//        }

//        #endregion
//    }
//}