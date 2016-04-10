using Budgeter.Core.Entities;
using PdfToText;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budgetterarn.DAL
{
    public class KontoFromPdfParser
    {
        private string fileFullPath;

        public KontoFromPdfParser(string path)
        {
            fileFullPath = path;
        }

        public List<BankRow> ParseToKontoEntriesFromRedPdf()
        {
            var text = QuickReadPdf.ReadPdf(fileFullPath);
            var parsed = GetKontoFromText(text);
            var rows = ParseKontoStringsToBankRow(parsed);

            return rows;
        }

        private List<BankRow> ParseKontoStringsToBankRow(List<string> parsed)
        {
            //"2015-10-13 ROSE GARDEN SUPREME S. ASKIM -95,00".LastIndexOf(" ")	39	int
            //"2015-10-13 ROSE GARDEN SUPREME S. ASKIM -95,00".Substring(39, 46-39)	" -95,00"	string
            //"2015-10-13 ROSE GARDEN SUPREME S. ASKIM -95,00".Length	46	int

            var rows = new List<BankRow>();

            foreach (var pdftextRow in parsed)
            {
                var entryStrings = new BankRow();

                var dateLength = 10;
                entryStrings.DateValue = pdftextRow
                    .Substring(0, dateLength);

                var firstBeloppCharPos = pdftextRow.LastIndexOf(" ");
                var pdftextRowLength = pdftextRow.Length;
                var beloppStartPos = firstBeloppCharPos;
                var beloppEndPos = pdftextRowLength - beloppStartPos;

                entryStrings.BeloppValue = pdftextRow.Substring(beloppStartPos, beloppEndPos);

                var eventStartPos = dateLength + 1;
                var eventEndPos = firstBeloppCharPos - eventStartPos;

                entryStrings.EventValue = pdftextRow.Substring(eventStartPos, eventEndPos);
                entryStrings.SaldoValue = string.Empty;

                rows.Add(entryStrings);
            }

            return rows;
        }

        private List<string> GetKontoFromText(string text)
        {

            var lines = text.Split(new string[] { "\n" } //Environment.NewLine }
                , StringSplitOptions.RemoveEmptyEntries);

            var riktigaLines = new List<string>();

            // Hitta rätt rader. Skippa resten
            // Leta upp Köpdatum
            // Ta alla nästa rader som börjar med datum.
            // Avsluta vid "forts."
            // Leta upp Köpdatum
            // Avsluta vid "forts." eller icke datum i början
            // Leta upp Köpdatum eller slutet på strängen
            bool latOneWasKöpdatum = false;
            foreach (var pdfTextRad in lines)
            {
                // Leta upp Köpdatum
                //bool currentIskontoRad = IsKontoRad(myString);

                if (latOneWasKöpdatum)
                {
                    // Ta alla nästa rader som börjar med datum.
                    bool currentIskontoRad = IsKontoRad(pdfTextRad);
                    if (currentIskontoRad)
                    {
                        // Ta denna raden
                        riktigaLines.Add(pdfTextRad);

                        // Och nästa
                        continue;
                    }
                }

                latOneWasKöpdatum = IsKöpdatum(pdfTextRad);
            }

            // Loopa igenom alla riktiga rader och lägg dem i bankrow.
            //BankRow row;

            // returnera
            return riktigaLines;
        }

        private bool IsKöpdatum(string myString)
        {
            var starttextToFind = "Köpdatum";

            return
               RefLesses.StringFuncions.ContainsClean(myString, starttextToFind);
        }

        private bool IsKontoRad(string myString)
        {
            if (string.IsNullOrEmpty(myString) || myString.Length < 10)
            {
                return false;
            }

            var datePartOfmyString =
                myString.Substring(0, 10)
                ;

            DateTime ettKöpDatum;
            var isValidDate = DateTime.TryParse(
                datePartOfmyString, //, System.Globalization.DateTimeStyles.AdjustToUniversal,
                out ettKöpDatum);

            return isValidDate;
        }

    }
}
