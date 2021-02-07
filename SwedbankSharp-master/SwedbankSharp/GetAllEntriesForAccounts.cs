using SwedbankSharp.JsonSchemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwedbankSharp
{
    public class GetAllEntriesForAccounts
    {
        private readonly Action<string> _writeToOutput;

        public GetAllEntriesForAccounts(Action<string> writeToOutput = null)
        {
            _writeToOutput = writeToOutput;
        }

        public async Task<List<TransactionList>> GetTransactionListsFromPersonsIds(List<long> socialSecurityIdNumbers)
        {
            var transactionLists = new List<TransactionList>();

            foreach (var idNumber in socialSecurityIdNumbers)
            {
                var handler = new GetAllEntriesForAccount(idNumber, _writeToOutput);

                var transactions = await handler.GetAllTransactions();
                if (transactions == null)
                {
                    continue;
                }

                transactionLists.AddRange(transactions);
            }

            return transactionLists;
        }

        public static List<Transaction> GetAllTransactions(List<TransactionList> transactionLists)
        {
            var allTransactions = new List<Transaction>();
            foreach (var list in transactionLists)
            {
                allTransactions.AddRange(list.Transactions);
            }

            return allTransactions;
        }
    }

    public class GetAllEntriesForAccount
    {
        private readonly long _idNumber;
        private Swedbank _loggedInClient;
        private readonly Action<string> _writeToOutput;
        private List<BankAccount> _bankAccounts;

        public GetAllEntriesForAccount(long idNumber)
        {
            _idNumber = idNumber;
        }

        public GetAllEntriesForAccount(long idNumber, Action<string> writeToOutput) : this(idNumber)
        {
            _writeToOutput = writeToOutput;
        }

        internal async Task<List<TransactionList>> GetAllTransactions()
        {
            _loggedInClient = await LoginToBank();
            if (_loggedInClient == null)
            {
                return null;
            }

            _bankAccounts = await GetAllAccounts();

            var ts = await GetTransactionListsFromBankAccounts();

            LogOutCurrentClient();

            return ts;
        }

        private async Task<List<BankAccount>> GetAllAccounts()
        {
            var accounts = await _loggedInClient.GetAccountListAsync();

            var bankAccounts = new List<BankAccount>();
            bankAccounts.AddRange(accounts.TransactionAccounts);
            bankAccounts.AddRange(accounts.SavingAccounts);
            //Skippa Kreditkort så länge...bankAccounts.AddRange(accounts.CardAccounts);

            return bankAccounts;
        }

        private async Task<List<TransactionList>> GetTransactionListsFromBankAccounts()
        {
            int i = 1;
            var transactionLists = new List<TransactionList>();
            foreach (var ta in _bankAccounts)
            {
                // Skippa mastercard
                if (ta.Id == "197ceb90d8e99111be96304c136510d84865d8db")
                    continue;

                SendToWriteOutput(i + ". " + ta.Name);
                
                var selectedAccount = _bankAccounts[i - 1];
                SendToWriteOutput("\nRetrieving account details...");

                try
                {
                    var transactionList = await _loggedInClient.GetAccountTransactionListAsync(selectedAccount.Id);
                    transactionLists.Add(transactionList);

                    SendToWriteOutput("Account Name: " + transactionList?.Account?.Name);
                    SendToWriteOutput("Account Balance: " + transactionList?.Account?.Balance + transactionList?.Account?.Currency);
                    SendToWriteOutput(
                        "Account trans: " + string.Join(" | ", transactionList?.Transactions?.Select(a => a.Description))
                        );
                }
                catch (Exception e)
                {

                    SendToWriteOutput($"Error {e.Message}");
                }

                i++;
            }

            return transactionLists;
        }

        private void SendToWriteOutput(string v)
        {
            _writeToOutput?.Invoke(v);
        }

        private async void LogOutCurrentClient()
        {
            await _loggedInClient.TerminateAsync();
        }


        private async Task<Swedbank> LoginToBank()
        {
            var client = new SwedbankLogin(BankType.Swedbank, _writeToOutput);

            await client.InitializeMobileBankIdLoginAsync(_idNumber);

            var loggedIn = false;
            Swedbank loggedInClient = null;
            const int abortLoginAfterSeconds = 20; //30;
            var totalWaitTimeInSeconds = 5;
            const int eachWaitTimeInSeconds = 1;
            var abort = false;
            while (!loggedIn && !Aborting(totalWaitTimeInSeconds, abortLoginAfterSeconds))
            {
                await Task.Delay(TimeSpan.FromSeconds(eachWaitTimeInSeconds));
                totalWaitTimeInSeconds += eachWaitTimeInSeconds;
                abort = Aborting(totalWaitTimeInSeconds, abortLoginAfterSeconds);

                SendToWriteOutput("Waiting for bankid..." + _idNumber);

                var status = await client.VerifyLoginAsync();
                loggedIn = status.LoggedIn;

                loggedInClient = status.Swedbank;
            }

            if (abort)
            {
                return null;
            }

            SendToWriteOutput("\nRetrieving list of profiles...");
            var profile = await loggedInClient.GetProfileAsync();

            var privateProfileId = profile.Banks.First().PrivateProfile.Id; //Assume we have a private profile.
            await loggedInClient.SetProfileForSessionAsync(privateProfileId);

            return loggedInClient;
        }

        private static bool Aborting(int totalWaitTimeInSeconds, int abortLoginAfterSeconds)
        {
            return totalWaitTimeInSeconds >= abortLoginAfterSeconds;
        }
    }

}
