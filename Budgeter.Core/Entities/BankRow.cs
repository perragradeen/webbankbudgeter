using RefLesses;
using System;
using System.Globalization;

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

        public DateTime Date { get {
            return DateFunctions.ParseDateWithCultureEtc(DateValue);
        } }
    }
}
