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

            return FilterTransactions(transactionList, startDate, endDate);
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