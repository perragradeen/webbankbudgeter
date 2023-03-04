using WebBankBudgeterService.Model;

namespace WebBankBudgeterServiceTest
{
    public class TransFilterer
    {
        public static TransactionList FilterTransactions(
            TransactionList transactionList,
            DateTime? fromDate = null,
            DateTime? endDate = null)
        {
            fromDate ??= GetFirstDateOfCurrentYear();
            endDate ??= GetLastDateOfCurrentYear();

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

        private static DateTime? GetFirstDateOfCurrentYear()
        {
            return new DateTime(DateTime.Today.Year, 1, 1);
        }

        private static DateTime? GetLastDateOfCurrentYear()
        {
            return new DateTime(DateTime.Today.Year, 12, 31);
        }
    }
}