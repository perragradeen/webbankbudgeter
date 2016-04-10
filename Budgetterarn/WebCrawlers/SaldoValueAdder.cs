using RefLesses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Budgetterarn.WebCrawlers
{
    public class SaldoValueAdder
    {
        private string htmlBodyText;
        private Model.SaldoHolder saldoHolder;

        public SaldoValueAdder(string htmlBodyText, Model.SaldoHolder saldoHolder)
        {
            // TODO: Complete member initialization
            this.htmlBodyText = htmlBodyText;
            this.saldoHolder = saldoHolder;
        }

        internal void AddSaldo(string saldoName, string startText, string endText)
        {
            var saldoAllkort =
                StringFuncions.GetTextBetweenStartAndEndText(htmlBodyText,
                    startText, endText);

            saldoHolder.AddToOrChangeValueInDictionaryForKey(saldoName,
                saldoAllkort);
        }
    }
}
