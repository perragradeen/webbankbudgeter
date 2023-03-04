using WebBankBudgeter.Service.Model;

namespace WebBankBudgeter.Service.TransactionTests
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
                  && t.DateAsDate <= endDate// new DateTime(2019, 03, 01)
                ).ToList();

            TransactionList filteredTrans = new TransactionList
            {
                Transactions = trans
            };

            //t =>
            //        t.DateAsDate >= new DateTime(2021 - 01 - 01)
            return filteredTrans;

            //TransactionHandler.SetTransactionList(
            //   filteredTrans
            //);
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