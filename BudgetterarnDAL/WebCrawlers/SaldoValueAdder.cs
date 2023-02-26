using Budgeter.Core.Entities;
using RefLesses;

namespace Budgetterarn.WebCrawlers
{
    public class SaldoValueAdder
    {
        private readonly string htmlBodyText;
        private readonly SaldoHolder saldoHolder;

        public SaldoValueAdder(string htmlBodyText, SaldoHolder saldoHolder)
        {
            this.htmlBodyText = htmlBodyText;
            this.saldoHolder = saldoHolder;
        }

        internal void AddSaldo(string saldoName, string startText, string endText)
        {
            var saldoAllkort =
                StringFunctions.GetTextBetweenStartAndEndText(htmlBodyText,
                    startText, endText);

            saldoHolder.AddToOrChangeValueInDictionaryForKey(saldoName,
                saldoAllkort);
        }
    }
}