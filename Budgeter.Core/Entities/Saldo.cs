using RefLesses;
using System.Collections.Generic;
using System.Linq;

namespace Budgetterarn.Model
{
    public class Saldo
    {
        public string SaldoName { get; set; }
        public double SaldoValue { get; set; }
    }

    public class SaldoHolder
    {
        public SaldoHolder()
        {
            Saldon = new List<Saldo>();
        }

        public List<Saldo> Saldon { get; private set; }

        public Saldo GetSaldoForName(string saldoName)
        {
            return Saldon.Find(s => s.SaldoName.Equals(saldoName));
        }

        private void SetSaldoValue(string saldoName, double saldoValue)
        {
            Saldon.Select(s => s.SaldoValue = saldoValue);
        }

        public bool HasSaldoName(string saldoName)
        {
            return Saldon.Any(s => s.SaldoName.Equals(saldoName));
        }

        public void AddToOrChangeValueInDictionaryForKey(
            string saldoName, string saldoValueText)
        {
            if (string.IsNullOrEmpty(saldoValueText))
            {
                return;
            }

            double saldoValue = saldoValueText.GetDoubleValueFromStringEntry();

            AddToOrChangeValueInDictionaryForKey(saldoName, saldoValue);
        }

        public void AddToOrChangeValueInDictionaryForKey(
            string saldoName, double saldoValue)
        {
            if (HasSaldoName(saldoName))
            {
                SetSaldoValue(saldoName, saldoValue);
            }
            else
            {
                Saldon.Add(new Saldo
                {
                    SaldoName = saldoName,
                    SaldoValue = saldoValue
                });
            }
        }
    }
}
