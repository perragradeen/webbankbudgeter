﻿using System.Globalization;

namespace WebBankBudgeterService.Model
{
    public class Transaction
    {
        public string Id { get; set; }
        private string Date { get; set; }
        public string Description { get; set; }
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
                    Group = "No group"
                };

                return categories.ToString();
            }
        }

        public string CategoryNameNoGroup
        {
            get
            {
                var categories = Categorizations?.Categories?.FirstOrDefault();
                if (!(categories == null ||
                      categories.Name == null))
                {
                    return categories.Name;
                }

                categories = new Categories
                {
                    Name = "No Category",
                    Group = "No group"
                };

                return categories.Name;
            }
        }

        private DateTime _dateAsDate;

        public DateTime DateAsDate
        {
            get
            {
                if (!(_dateAsDate == DateTime.MinValue))
                {
                    return _dateAsDate;
                }

                if (!DateTime.TryParse(Date, out var date))
                {
                    return DateTime.MinValue;
                }

                _dateAsDate = date;
                return _dateAsDate;
            }
            set { _dateAsDate = value; }
        }

        public string DateAsYearMothText
        {
            get
            {
                var yearMonthNameName = GetYearMonthName(DateAsDate);

                return yearMonthNameName;
            }
        }

        public static string GetYearMonthName(DateTime dateTime)
        {
            return dateTime.Year + " " + GetMonthAsFullString(dateTime);
        }

        public static DateTime GetDateFromYearMonthName(string yearAndMonthAsText)
        {
            if (string.IsNullOrWhiteSpace(yearAndMonthAsText))
            {
                return DateTime.Today;
            }

            return DateTime.ParseExact(yearAndMonthAsText, "yyyy MMMM", CultureInfo.InvariantCulture);
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

                if (double.TryParse(Amount.ToString()
                        .Replace(" ", string.Empty)
                    , out var value))
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
            set { _amountAsDouble = value; }
        }

        public ExpenseControlIncludedAlternatives ExpenseControlIncludedAsEnum
        {
            get
            {
                var e = Enum.TryParse(
                    ExpenseControlIncluded,
                    out ExpenseControlIncludedAlternatives v);

                if (e)
                {
                    return v;
                }

                return ExpenseControlIncludedAlternatives.INCLUDED;
            }
        }

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
}