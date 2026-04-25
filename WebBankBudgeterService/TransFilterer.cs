using WebBankBudgeterService.Model;

namespace WebBankBudgeterService
{
    public class TransFilterer
    {
        public static TransactionList FilterTransactions(
            TransactionList transactionList,
            int selectedYear)
        {
            var startDate = GetStartDate(selectedYear);
            var endDate = GetEndDate(selectedYear);

            var inRange = FilterTransactions(transactionList, startDate, endDate);

            // Strikt kalenderår: undvik att rader från grannår "läcker" in om de råkar
            // ligga inom 1 jan–31 dec men tillhör annat år (R5 i plan.md).
            var sameYear = inRange.Transactions
                .Where(t => t.DateAsDate.Year == selectedYear)
                .ToList();

            return new TransactionList
            {
                Transactions = sameYear,
                Account = inRange.Account
            };
        }

        public static TransactionList FilterTransactions(
            TransactionList transactionList,
            DateTime? fromDate = null,
            DateTime? endDate = null)
        {
            fromDate ??= GetStartDateOfLastYear();
            endDate ??= GetEndDateOfLastYear();

            var trans =
                transactionList.Transactions.Where(t =>
                     t.DateAsDate >= fromDate
                  && t.DateAsDate <= endDate
                ).ToList();

            var filteredTrans = new TransactionList
            {
                Transactions = trans
            };

            return filteredTrans;
        }

        private static DateTime GetStartDate(int selectedYear)
        {
            return new DateTime(selectedYear, 1, 1);
        }

        private static DateTime GetEndDate(int selectedYear)
        {
            return new DateTime(selectedYear, 12, 31);
        }

        private static DateTime? GetStartDateOfLastYear()
        {
            return new DateTime(LastYear(), 1, 1);
        }

        public static int LastYear()
        {
            return DateTime.Today.Year - 1;
        }

        private static DateTime? GetEndDateOfLastYear()
        {
            return new DateTime(LastYear(), 12, 31);
        }
    }
}