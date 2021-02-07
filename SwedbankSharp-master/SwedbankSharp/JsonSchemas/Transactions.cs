using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SwedbankSharp.JsonSchemas
{
    public class TransactionList
    {
        public List<Transaction> Transactions { get; set; }
        public Account Account { get; set; }
        public int NumberOfTransactions { get; set; }
        public List<ReservedTransaction> ReservedTransactions { get; set; }
        public int NumberOfReservedTransactions { get; set; }
        public bool MoreTransactionsAvailable { get; set; }
        public Links2 Links { get; set; }
    }

    public class Transaction
    {
        public string Id { get; set; }
        public string Date { get; set; }
        public string Description { get; set; }
        public string Currency { get; set; }
        public object Amount { get; set; }

        public string ExpenseControlIncluded { get; set; }
        public Categorizations Categorizations { get; set; }
        public string CategoryName
        {
            get
            {
                var categories = Categorizations?.Categories?.FirstOrDefault();
                if (!(categories == null ||
                    categories.Name == null))
                {
                    return categories.ToString();
                }

                categories = new Categories
                {
                    Name = "No Category",
                    Group = "No group",
                    Id = IdNoCategory
                };

                return categories.ToString();
            }
        }

        private DateTime _dateAsDate;
        public DateTime DateAsDate
        {
            get
            {
                if (!(_dateAsDate == null ||
                    _dateAsDate == DateTime.MinValue))
                {
                    return _dateAsDate;
                }

                var date = DateTime.MinValue;
                if (DateTime.TryParse(Date, out date))
                {
                    _dateAsDate = date;
                    return _dateAsDate;
                }

                return DateTime.MinValue;
            }
            set
            {
                _dateAsDate = value;
            }
        }

        public string DateAsYearMothText
        {
            get
            {

                var yearMonthNameName = DateAsDate.Year + " " + GetMonthAsFullString(DateAsDate);

                return yearMonthNameName;
            }
        }

        private double _amountAsDouble;
        public const string IdNoCategory = "000000000000000000000000000000000000000000000000";

        public double AmountAsDouble
        {
            get
            {
                if (_amountAsDouble != 0)
                {
                    return _amountAsDouble;
                }

                if (Amount == null)
                {
                    return 0;
                }

                var value = 0.0;
                if (double.TryParse(Amount.ToString()
                    .Replace(" ", string.Empty)
                    //.Replace(",", ".")
                    , out value))
                {
                    _amountAsDouble = value;
                    return _amountAsDouble;
                }

                if (double.TryParse(Amount.ToString()
                        .Replace(" ", string.Empty)
                    .Replace(",", ".")
                    , out value))
                {
                    _amountAsDouble = value;
                    return _amountAsDouble;
                }

                return 0;
            }
            set
            {
                _amountAsDouble = value;
            }
        }

        public ExpenseControlIncludedAlternatives ExpenseControlIncludedAsEnum { get {
                var e = Enum.TryParse(
                    ExpenseControlIncluded,
                    out ExpenseControlIncludedAlternatives v);

                if (e)
                {
                    return v;
                }

                return ExpenseControlIncludedAlternatives.INCLUDED;
            } }

        public override string ToString()
        {
            var date = string.IsNullOrWhiteSpace(Date) ? DateAsDate.ToShortDateString() : Date;
            var amount = string.IsNullOrWhiteSpace(Amount?.ToString()) ? AmountAsDouble.ToString() : Amount;
            return $"{date}: {Description} = {amount}. {CategoryName}";
        }

        public static string GetMonthAsFullString(DateTime key)
        {
            return new DateTime(key.Year, key.Month, 1)
                        .ToString("MMMM", CultureInfo.InvariantCulture);
        }
    }

    public class Categorizations
    {
        public List<Categories> Categories { get; set; }
    }

    public class Categories
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }
        public string Amount { get; set; }
        public override string ToString()
        {
            return $"{Group} {Name}";
        }
    }

    public class Account
    {
        public string AvailableAmount { get; set; }
        public string CreditGranted { get; set; }
        public QuickbalanceSubscription QuickbalanceSubscription { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public string Currency { get; set; }
        public string AccountNumber { get; set; }
        public string ClearingNumber { get; set; }
        public object Balance { get; set; }
        public string FullyFormattedNumber { get; set; }
    }

    public class ReservedTransaction
    {
        public string Date { get; set; }
        public string Description { get; set; }
        public string Currency { get; set; }
        public string Amount { get; set; }
    }

    public enum ExpenseControlIncludedAlternatives
    {
        INCLUDED = 0,
        OUTDATED
    }
}
