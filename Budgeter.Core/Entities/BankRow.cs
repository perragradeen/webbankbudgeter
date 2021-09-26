using RefLesses;
using System;
using System.Linq;

namespace Budgeter.Core.Entities
{
    /// <summary>
    /// Från raden i banken. Från html. Kort
    /// </summary>
    public class BankRow
    {
        public string DateValue { get; set; }

        /// <summary>
        /// Info om vad som köpts eller hänt på kontoraden
        /// </summary>
        public string EventValue { get; set; }

        public string BeloppValue { get; set; }

        public string SaldoValue { get; set; }

        public DateTime Date => DateFunctions.ParseDateWithCultureEtc(DateValue);

        public bool IsValidBankRow =>
            StringIsNumber(BeloppValue)
            && (DateFunctions.IsValidDate(DateValue)
                || Date == DateTime.MinValue);

        private static bool StringIsNumber(string beloppValue) =>
            beloppValue
                .Replace(" ", string.Empty)
                .Replace("+", string.Empty)
                .Replace("-", string.Empty)
                .Replace(",", string.Empty)
                .Replace(".", string.Empty)
                .All(char.IsDigit);
    }
}