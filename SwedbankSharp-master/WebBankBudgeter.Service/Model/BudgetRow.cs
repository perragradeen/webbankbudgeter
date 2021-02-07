using System.Collections.Generic;

namespace WebBankBudgeter.Service.Model
{
    public class BudgetRow
    {
        public BudgetRow()
        {
            AmountsForMonth = new Dictionary<string, double>();
        }

        /// <summary>
        /// Ex. "el"
        /// </summary>
        public string CategoryText { get; set; }

        /// <summary>
        /// Ex. "2016 January", -2215,22
        /// </summary>
        public Dictionary<string, double> AmountsForMonth { get; set; }

        //Kan inte räkna ut det här då det blir 0 + 0 + 1 + 1 => snitt 0,5 men man vill ha 1. Så skippas 2 första kolumnerna
        //public double AmountAverageText
        //{
        //    get
        //    {
        //        var averageValue = 0.0;
        //        averageValue = AmountsForMonth.Values.ToList().Average(d => d);

        //        return Math.Round(averageValue, 0); // .ToString("N");
        //    }
        //}



        //public double April { get; set; }
        //public double May { get; set; }
        //public double June { get; set; }

        //public List<string> AmountTexts
        //{
        //    get
        //    {
        //        var texts = new List<string>();

        //        texts.AddRange(new List<string>
        //        {
        //            April.ToString("N2"),
        //            May.ToString("N2"),
        //            June.ToString("N2"),
        //        });

        //        return texts;
        //    }
        //}
    }
}