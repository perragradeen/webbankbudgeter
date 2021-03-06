using Budgeter.Core.BudgeterConstants;
using Budgeter.Core.Entities;
using LoadTransactionsFromFile;
using LoadTransactionsFromFile.DAL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly Categories _allCategories;
        private readonly TableGetter _tableGetter;

        public TransactionList TransactionList => _transactionCalcsHandler.TransactionList;

        public TransactionHandler(
            Action<string> writeToOutput,
            TableGetter tableGetter,
            string categoriesFilePath)
        {
            _writeToOutput = writeToOutput;
            _tableGetter = tableGetter;

            _allCategories = SerializationFunctions.DeserializeObject(
                categoriesFilePath,
                typeof(Categories))
            as Categories;
        }

        public async Task<bool> GetTransactionsAsync()
        {
            TransactionList transactionListdata;
            try
            {
                transactionListdata =
                    await GetData();
                if (transactionListdata == null)
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
                new TransactionCalcs(transactionListdata);

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

        //private async Task<TransactionList> GetData()
        private async Task<TransactionList> GetData()
        {
            //return TestData.GetTestDatasFromFiles();

            //----------------------

            //long idnumber = 7906072439;
            ////long idnumber2 = 7703217583;
            //var idNumbers = new List<long>
            //{
            //    idnumber
            //    //, idnumber2
            //};

            //var getter = new GetAllEntriesForAccounts(_writeToOutput);
            var transactionLists =
                await GetTransactionsTransFormedFromFile();

            //getter.GetTransactionListsFromPersonsIds(idNumbers);
            if (!transactionLists.Any())
            {
                return null;
            }

            var allTransactions = GetAllTransactions(transactionLists);

            var transactionListTotal = transactionLists.FirstOrDefault();
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
            //var statusText = toolStripStatusLabel1.Text = @"Nothing loaded.";
            //var changedExcelFileSavePath = Filerefernces._excelFileSavePath;

            // Todo:Viktig: g�r en funktion f�r denna eller refa med en filnamns och s�kv�gsklass....
            var testfilePath = @"C:\Temp";
            var kontoutdragInfoForLoad = new KontoutdragInfoForLoad
            {
                FilePath = testfilePath,
                ExcelFileSavePath = testfilePath,
                ExcelFileSaveFileName = @"pelles budget.xls",
                SheetName = ShbConstants.SheetName,
            };
            kontoutdragInfoForLoad.FilePath = System.IO.Path.Combine(
                kontoutdragInfoForLoad.FilePath,
                kontoutdragInfoForLoad.ExcelFileSaveFileName
                );

            // Ladda fr�n fil
            var entriesLoadedFromDataStore =
                LoadEntriesFromFileHandler.LoadEntriesFromFile(kontoutdragInfoForLoad);

            var kontoEntriesHolder = new KontoEntriesHolder();
            _ = LoadKontonDal.TransFormEntriesFromExcelFileToTable(
                kontoutdragInfoForLoad,
                kontoEntriesHolder.KontoEntries,
                kontoEntriesHolder.SaldoHolder,
                entriesLoadedFromDataStore);

            return kontoEntriesHolder.KontoEntries;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task<List<TransactionList>> GetTransactionsTransFormedFromFile()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var transactionsFromFile = GetTransactionsFromFile();

            var transactions = GetTransFormedTransactionsFromFileToList(transactionsFromFile); // Add reoccuring and income categories

            //var returnList = FillAllCategories(transactions);
            return transactions;
        }

        private List<TransactionList> GetTransFormedTransactionsFromFileToList(SortedList transactionsFromFile)
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
            return _allCategories.CategoryList.FirstOrDefault(c =>
                c.Description == categoryName)
                ?.Group;
        }

        private static List<Transaction> GetAllTransactions(List<TransactionList> transactionLists)
        {
            var allTransactions = new List<Transaction>();
            foreach (var list in transactionLists)
            {
                allTransactions.AddRange(list.Transactions);
            }

            return allTransactions;
        }

        public object GetTotalBalanceForTransactions(List<TransactionList> transactionLists)
        {
            return transactionLists.Sum(l =>
                 GetAmountNotZero(l));
        }

        private double GetAmountNotZero(TransactionList l)
        {
            var balance = Conversions.SafeGetDouble(l.Account?.Balance);
            var availableAmount = Conversions.SafeGetDouble(l.Account?.AvailableAmount);

            return balance > 0 ?
                    balance :
                    availableAmount;
        }
    }
}