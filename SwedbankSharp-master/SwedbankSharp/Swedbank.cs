using SwedbankSharp.JsonSchemas;
using System;
using System.Threading.Tasks;

namespace SwedbankSharp
{
    public class Swedbank
    {
        private readonly AppData _selectedBank;
        private readonly SwedbankRequester _requester;
        private string _currentProfile = null;
        private readonly Action<string> _writeToOutput;

        public Swedbank(AppData selectedBank, SwedbankRequester requester, Action<string> writeToOutput)
        {
            _selectedBank = selectedBank;
            _requester = requester;
            _writeToOutput = writeToOutput;
        }

        private void SendToWriteOutput(string v)
        {
            _writeToOutput?.Invoke(v);
        }

        /// <summary>
        /// Logs out of the API
        /// </summary>
        public async Task TerminateAsync()
        {
            await _requester.PutAsync("identification/logout");
        }

        /// <summary>
        /// Shows account details and transaction for account
        /// </summary>
        /// <param name="accountId">Unique ID for account in current session from Swedbank API</param>
        /// <returns></returns>
        public async Task<TransactionList> GetAccountTransactionListAsync(string accountId)
        {
            VerifyProfileIsSet();

            var url = "engagement/transactions/" + accountId;// + "?numberOfTransactions=100";

            var transactionListListTotal = await _requester.GetAsync<TransactionList>(url);

            var more = TransactionListListHasMoreTransactions(transactionListListTotal);
            while (more)
            {
                TransactionList transactionListList = null;
                try
                {
                    transactionListList = await _requester.GetAsync<TransactionList>(url);
                    transactionListListTotal.Transactions.AddRange(transactionListList?.Transactions);

                    more = TransactionListListHasMoreTransactions(transactionListList);
                    if (more)
                    {
                        url = GetNextBatchOfTransactionsUrl(transactionListList);
                    }
                }
                catch (Exception e)
                {
                    SendToWriteOutput("Error in retrieving transactions from" + Environment.NewLine
                        + $"{e.Message} " + Environment.NewLine
                        + $"{transactionListList?.Account?.Name} " + Environment.NewLine
                        + $"URL: {url}"
                        );
                }

                //                more = false;
            }

            return transactionListListTotal;
        }

        private string GetNextBatchOfTransactionsUrl(TransactionList transactionListList)
        {
            return transactionListList.Links.Next.Uri.Substring(4);
        }

        private bool TransactionListListHasMoreTransactions(TransactionList transactionListList)
        {
            return transactionListList.MoreTransactionsAvailable;

                //.Links != null
                //&& transactionListList.Links.Next != null
                //&& !string.IsNullOrEmpty(transactionListList.Links.Next.Uri);
        }

        //public async Task<JsonSchemas.TransactionList> GetAccountTransactionListAsync(string accountId)
        //{
        //    VerifyProfileIsSet();

        //    JsonSchemas.TransactionList transactionListList = await _requester.GetAsync<JsonSchemas.TransactionList>("engagement/transactions/" + accountId);

        //    return transactionListList;
        //}

        public async Task SetProfileForSessionAsync(string profileId)
        {
            var response = await _requester.PostAsync("profile/" + profileId);
            response.EnsureSuccessStatusCode();

            _currentProfile = profileId; //Potential race condition.
        }

        /// <summary>
        /// List all bank accounts that are available for the current profile.
        /// </summary>
        /// <returns>List of accounts</returns>
        public async Task<JsonSchemas.Overview> GetAccountListAsync()
        {
            VerifyProfileIsSet();

            var output = await _requester.GetAsync<JsonSchemas.Overview>("engagement/overview");

            if (output.TransactionAccounts == null)
                throw new ApplicationException("Unable to list bank accounts");

            return output;
        }

        private void VerifyProfileIsSet()
        {
            if (_currentProfile == null)
                throw new Exception("Profile not selected");
        }

        /// <summary>
        /// Profile information
        /// Access a list of profiles and each temporary ID-number. Every privateperson and corporation have their own profiles.
        /// </summary>
        /// <returns></returns>
        public async Task<JsonSchemas.Profile> GetProfileAsync()
        {
            var output = await _requester.GetAsync<JsonSchemas.Profile>("profile/");

            return output;
        }

        /// <summary>
        /// Gets reminders such as unfulfilled payments
        /// </summary>
        /// <returns>Reminders</returns>
        public async Task<JsonSchemas.Reminders> GetRemindersAsync()
        {
            VerifyProfileIsSet();

            return await _requester.GetAsync<JsonSchemas.Reminders>("message/reminders");
        }

        /// <summary>
        /// Gets BaseInfo (Grouped accounts?)
        /// </summary>
        /// <returns>Reminders</returns>
        public async Task<JsonSchemas.BaseInfo> GetBaseInfoAsync()
        {
            VerifyProfileIsSet();

            return await _requester.GetAsync<JsonSchemas.BaseInfo>("transfer/baseinfo");
        }
    }
}
