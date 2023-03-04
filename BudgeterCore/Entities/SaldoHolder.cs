using RefLesses;

namespace BudgeterCore.Entities
{
    public class SaldoHolder
    {
        public SaldoHolder()
        {
            Saldon = new List<Saldo>();
        }

        public List<Saldo> Saldon { get; }

        private void SetSaldoValue(
            string saldoName,
            double saldoValue)
        {
            Saldon
                .Where(w => w.SaldoName == saldoName)
                .ToList()
                .ForEach(s => s.SaldoValue = saldoValue);
        }

        private bool HasSaldoName(string saldoName)
        {
            return Saldon.Any(s => s.SaldoName.Equals(saldoName));
        }

        public void AddToOrChangeValueInDictionaryForKey(
            string saldoName,
            string saldoValueText)
        {
            if (string.IsNullOrEmpty(saldoValueText))
            {
                return;
            }

            var saldoValue = saldoValueText.GetDoubleValueFromStringEntry();

            AddToOrChangeValueInDictionaryForKey(saldoName, saldoValue);
        }

        public void AddToOrChangeValueInDictionaryForKey(
            string saldoName,
            double saldoValue)
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