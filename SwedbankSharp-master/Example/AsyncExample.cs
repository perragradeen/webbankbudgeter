using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SwedbankSharp;

namespace Example
{
    public class AsyncExample
    {
        public void WriteToOutput(string message)
        {
            Console.WriteLine(message);
        }

        public async Task RunAsync()
        {
            WriteToOutput("Personal ID number: ");
            long idnumber = 7906072439;
            //long idnumber2 = 7703217583; 
            var idNumbers = new List<long> {
                idnumber
                //, idnumber2
            };

            var getter = new GetAllEntriesForAccounts(WriteToOutput);
            var transactionLists = await
                getter.GetTransactionListsFromPersonsIds(idNumbers);
            var allTransactions = GetAllEntriesForAccounts.GetAllTransactions(transactionLists);

            WriteToOutput($"Bank-Accounts = {transactionLists.Count}");
            WriteToOutput($"Bank-Transactions = {allTransactions.Count}");

            WriteToOutput("\nPress any key to exit...");
            Console.ReadKey();
        }

        static int ReadKey()
        {
            while (true)
            {
                ConsoleKeyInfo choice = Console.ReadKey();
                Console.WriteLine();
                if (char.IsDigit(choice.KeyChar))
                {
                    int answer = Convert.ToInt32(choice.KeyChar);
                    return answer - 48;
                }
                Console.WriteLine("\nSorry, you need to input a number.");
            }
        }
    }
}
