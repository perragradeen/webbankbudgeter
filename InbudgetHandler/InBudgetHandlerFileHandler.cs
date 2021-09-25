using Budgeter.Core.Entities;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace InbudgetToTable
{
    public class InBudgetHandlerFileHandler
    {
        private readonly string _inBudgetFilePath;
        private JavaScriptSerializer JsonSerializer = new JavaScriptSerializer();

        private List<InBudget> _inPoster;
        public async Task<List<InBudget>> GetInPoster()
        {
            return _inPoster ?? (_inPoster = await GetIncomesFromDisk());
        }

        public void SetInPoster(List<InBudget> inBudgets)
        {
            _inPoster = inBudgets;
        }

        public async Task<List<InBudget>> SetInPosterFromDisk()
        {
            _inPoster = await GetIncomesFromDisk();
            return _inPoster;
        }

        public InBudgetHandlerFileHandler(string inBudgetFilePath)
        {
            _inBudgetFilePath = inBudgetFilePath;
        }

        public void SparaInPosterPåDisk(List<InBudget> inPoster)
        {
            // Write to file
            var jsonString =
                JsonSerializer.Serialize(inPoster);
            FileWriteAllText(jsonString);
        }

        private void FileWriteAllText(string jsonString)
        {
            File.WriteAllText(_inBudgetFilePath, jsonString);
        }

        private async Task<List<InBudget>> GetIncomesFromDisk()
        {
            // Behövs inte ta ut alla unika månader, för dom skrivs in unika
            // var årOMånader = Ta_ut_alla_unika_månader(inPoster);

            var jsonString = await FileReadAllText();

            return JsonSerializer
                           .Deserialize<List<InBudget>>(jsonString);
        }

        private async Task<string> FileReadAllText()
        {
            using (var reader = File.OpenText(_inBudgetFilePath))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
