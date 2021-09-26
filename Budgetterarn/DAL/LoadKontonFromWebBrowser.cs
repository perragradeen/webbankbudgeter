using System;
using System.Collections.Generic;
using Budgeter.Core.BudgeterConstants;
using Budgeter.Core.Entities;
using Budgetterarn.WebCrawlers;
using LoadTransactionsFromFile;

namespace Budgetterarn.DAL
{
    public class LoadKontonFromWebBrowser
    {
        private readonly KontoEntriesHolder kontoEntriesHolder;

        public LoadKontonFromWebBrowser(KontoEntriesHolder kontoEntriesHolder)
        {
            this.kontoEntriesHolder = kontoEntriesHolder;
        }

        /// <summary>
        /// Kolla browser efter entries.
        /// </summary>
        /// <param name="kontoEntriesHolder"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool GetAllVisibleEntriesFromWebBrowser(string text)
        {
            var noNewKontoEntriesBeforeLoading =
                kontoEntriesHolder.NewKontoEntries.Count;

            if (TextAndBankChoiceIsInvalid(text)) return false;

            // TODO: läs saldon Get saldo
            //GetSwedbankSaldo(webBrowser1.Document.Body, kontoEntriesHolder.SaldoHolder);

            SparaKontoRaderTillNylistan(text);

            return ReturneraOmNågotÄndrats(noNewKontoEntriesBeforeLoading);
        }

        private bool ReturneraOmNågotÄndrats(int noNewKontoEntriesBeforeLoading)
        {
            // Returnera aom något ändrats. Är de nya inte samma som innan laddning,
            // så är det sant att något ändrats.
            return kontoEntriesHolder.NewKontoEntries.Count !=
                            noNewKontoEntriesBeforeLoading;
        }

        private static bool TextAndBankChoiceIsInvalid(string text)
        {
            return text == null
                || ProgramSettings.BankType != BankType.Swedbank;
        }

        private void SparaKontoRaderTillNylistan(string text)
        {
            var entryAdder = new EntryAdder(kontoEntriesHolder);
            entryAdder.SetKontoEntriesToNewList(
                GetKontoEntriesFromHtml(text));
        }

        private static List<KontoEntry> GetKontoEntriesFromHtml(string text)
        {
            var entries = GetTextLinesWithKontoEntries(text);
            var rows = GetListOfTextRowsFromTextLines(entries);
            var parsedBankRows = GetParsedBankRows(rows);
            var kontoEntriesFromHtml = GetValidKontoEntriwsFromParsedBankRows(
                parsedBankRows);
            return kontoEntriesFromHtml;
        }

        private static List<KontoEntry> GetValidKontoEntriwsFromParsedBankRows(
            List<BankRow> parsedBankRows)
        {
            List<KontoEntry> kontoEntriesFromHtml = new List<KontoEntry>();
            foreach (var entryStrings in parsedBankRows)
            {
                if (!entryStrings.IsValidBankRow) continue;

                kontoEntriesFromHtml.Add(new KontoEntry(entryStrings));
            }
            return kontoEntriesFromHtml;
        }

        private static List<BankRow> GetParsedBankRows(List<List<string>> rows)
        {
            var returnList = new List<BankRow>();
            foreach (var row in rows)
            {
                returnList.Add(GetSwedBankTableRowv2(row));
            }

            return returnList;
        }

        private static List<List<string>> GetListOfTextRowsFromTextLines(
            string[] entries)
        {
            var rows = new List<List<string>>();
            var currentEntriesColumns = new List<string>();
            var currentColumnCount = 0;
            foreach (var textPart in entries)
            {
                currentColumnCount++;
                currentEntriesColumns.Add(textPart);

                if (currentColumnCount <= 4) continue;
                currentColumnCount = 0;
                rows.Add(new List<string>(currentEntriesColumns));
                currentEntriesColumns = new List<string>();
            }

            return rows;
        }

        private static string[] GetTextLinesWithKontoEntries(string text)
        {
            var entryBlob = GetStartOfKontoEntries(text);
            var rowBreakString = GetRowBreakString(text);

            return entryBlob.Split(
                new string[] { rowBreakString },
                StringSplitOptions.RemoveEmptyEntries);
        }

        private static string GetRowBreakString(string text)
        {
            if (text.IndexOf(Environment.NewLine) >= 0)
            {
                return Environment.NewLine;
            }

            var standardRowBreak = "\n";
            var altStandardRowBreak = "\r\n";
            return (text.IndexOf(standardRowBreak) == -1
                ? altStandardRowBreak
                : standardRowBreak);
        }

        public static string GetStartOfKontoEntries(string text)
        {
            var rowBreakString = GetRowBreakString(text);
            var findKey = "Belopp" + rowBreakString + "Saldo" + rowBreakString;

            var index = text.IndexOf(findKey);
            int length = findKey.Length;

            if (index == -1)
            {
                throw new Exception("Fel. Hittar inte: " + findKey);
            }

            return text.Substring(index + length);
        }

        /// <summary>
        /// TRADERA
        /// 2021-08-06
        /// 2021-08-06
        /// -160,00
        /// 194 122,84
        /// </summary>
        /// <param name="htmlElement"></param>
        /// <returns></returns>
        private static BankRow GetSwedBankTableRowv2(
            IReadOnlyList<string> htmlElement)
        {
            const int eventColNum = 1;
            const int dateColNum = 2;
            const int beloppColNum = 4;
            const int saldoColNum = 5;

            var entry = new BankRow
            {
                DateValue = htmlElement[dateColNum - 1] ?? string.Empty,
                EventValue = htmlElement[eventColNum - 1] ?? string.Empty,
                BeloppValue = htmlElement[beloppColNum - 1] ?? string.Empty,
                SaldoValue = htmlElement[saldoColNum - 1] ?? string.Empty
            };

            return entry;
        }
    }
}