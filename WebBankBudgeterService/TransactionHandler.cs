using Budgeter.Core.BudgeterConstants;
using Budgeter.Core.Entities;
using LoadTransactionsFromFile;
using LoadTransactionsFromFile.DAL;
using Serializers;
using System.Collections;
using System.Diagnostics;
using WebBankBudgeter.Service.Model;
using WebBankBudgeter.Service.Model.ViewModel;
using WebBankBudgeter.Service.Services;
using Categories = CategoryHandler.Model.Categories;

namespace WebBankBudgeter.Service
{
    public class TransactionHandler
    {
        private readonly Action<string> _writeToOutput;

        private TransactionCalcs _transactionCalcsHandler;
        private readonly string _filePath;
        private readonly Categories _allCategories;
        private readonly TableGetter _tableGetter;

        public TransactionList TransactionList =>
            _transactionCalcsHandler.TransactionList;

        public void SetTransactionList(TransactionList list)
        {
            _transactionCalcsHandler =
                new TransactionCalcs(list);
        }

        public Categories AllCategories => _allCategories;

        public TransactionHandler(
            Action<string> writeToOutput,
            TableGetter tableGetter,
            string categoriesFilePath,
            string filePath)
        {
            _writeToOutput = writeToOutput;
            _tableGetter = tableGetter;

            _allCategories = SerializationFunctions
                .DeserializeObject<Categories>(categoriesFilePath);
            _filePath = filePath;
        }

        public async Task<bool> GetTransactionsAsync()
        {
            TransactionList transactionDatalist;
            try
            {
                transactionDatalist = await GetData();
                if (transactionDatalist == null)
                {
                    _writeToOutput(Environment.NewLine + "No one logged in.");
                    return false;
                }
            }
            catch (Exception e)
            {
                WriteLineToOutput(e.Message);
                return false;
            }

            _transactionCalcsHandler =
                new TransactionCalcs(transactionDatalist);

            return true;
        }

        public TextToTableOutPuter GetTextTableFromTransactions()
        {
            return _tableGetter.GetTextTableFromTransactions(
                TransactionList.Transactions);
        }

        public void RemoveDuplicates()
        {
            _transactionCalcsHandler.RemoveDuplicates();
        }

        public void SortTransactions()
        {
            _transactionCalcsHandler.SortTrans();
        }

        private void WriteLineToOutput(string message)
        {
            _writeToOutput(message + Environment.NewLine);
        }

        private async Task<TransactionList> GetData()
        {
            var transactionLists =
                await GetTransactionsTransFormedFromFile();

            //getter.GetTransactionListsFromPersonsIds(idNumbers);
            if (!transactionLists.Any())
            {
                return null;
            }

            var allTransactions = GetAllTransactions(transactionLists);

            var transactionListTotal = transactionLists.FirstOrDefault();
            Debug.Assert(transactionListTotal != null, nameof(transactionListTotal) + " != null");

            transactionListTotal.Transactions = allTransactions;
            if (transactionListTotal.Account == null)
            {
                transactionListTotal.Account = new Account();
            }

            transactionListTotal.Account.Balance = GetTotalBalanceForTransactions(transactionLists);

            return transactionListTotal;
        }

        private SortedList GetTransactionsFromFile()
        {
            var kontoutdragExcelFileInfo = new KontoutdragExcelFileInfo
            {
                SheetName = BankConstants.SheetName,
                ExcelFileSavePath = _filePath
            };

            // Ladda från fil
            var entriesLoadedFromDataStore =
                LoadEntriesFromFileHandler.LoadEntriesFromFile(
                    kontoutdragExcelFileInfo);

            var kontoEntriesHolder = new KontoEntriesHolderForLoad();

            _ = EntriesFromExcelTransFormer.TransformFromExcelFileToList(
                kontoEntriesHolder,
                entriesLoadedFromDataStore);

            return kontoEntriesHolder.KontoEntries;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task<List<TransactionList>> GetTransactionsTransFormedFromFile()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var transactionsFromFile = GetTransactionsFromFile();

            var transactions =
                GetTransFormedTransactionsFromFileToList(transactionsFromFile); // Add reoccuring and income categories

            return transactions;
        }

        private List<TransactionList> GetTransFormedTransactionsFromFileToList(IEnumerable transactionsFromFile)
        {
            var listOfSeveralAccounts = new List<TransactionList>();
            var transactions = new List<Transaction>();

            var i = 0;
            foreach (DictionaryEntry item in transactionsFromFile)
            {
                var kontoEnry = item.Value as KontoEntry;
                var transactionTransformer =
                    new TransactionTransformer(kontoEnry, LookUpCategoryGroup);
                var transaction = transactionTransformer.GetTransaction();
                transaction.Id = (++i).ToString();

                transactions.Add(transaction);
            }

            listOfSeveralAccounts.Add(new TransactionList
            {
                Transactions = transactions,
            });

            return listOfSeveralAccounts;
        }

        private string LookUpCategoryGroup(string categoryName)
        {
            return AllCategories.CategoryList.FirstOrDefault(c =>
                    c.Description == categoryName)
                ?.Group;
        }

        private static List<Transaction> GetAllTransactions(IEnumerable<TransactionList> transactionLists)
        {
            var allTransactions = new List<Transaction>();
            foreach (var list in transactionLists)
            {
                allTransactions.AddRange(list.Transactions);
            }

            return allTransactions;
        }

        private static object GetTotalBalanceForTransactions(IEnumerable<TransactionList> transactionLists)
        {
            return transactionLists.Sum(GetAmountNotZero);
        }

        private static double GetAmountNotZero(TransactionList list)
        {
            var balance = Conversions.SafeGetDouble(list.Account?.Balance);
            var availableAmount = Conversions.SafeGetDouble(list.Account?.AvailableAmount);

            return balance > 0 ? balance : availableAmount;
        }
    }
}