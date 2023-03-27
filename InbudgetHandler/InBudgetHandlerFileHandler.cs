using BudgeterCore.Entities;
using System.Linq;
using System.Text.Json;

namespace InbudgetHandler
{
    public class InBudgetHandlerFileHandler
    {
        private readonly string _inBudgetFilePath;

        private List<InBudget> _inPoster;

        public InBudgetHandlerFileHandler(string inBudgetFilePath)
        {
            _inBudgetFilePath = inBudgetFilePath;
        }

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

        public void SparaInPosterPåDisk(List<InBudget> inPoster)
        {
            // Merga med ej synliga inposter
            foreach (var postFromUi in inPoster)
            {
                var postHidden = _inPoster.FirstOrDefault(p =>
                    p.YearAndMonth == postFromUi.YearAndMonth
                    && p.CategoryDescription == postFromUi.CategoryDescription);
                if (postHidden != null)
                {
                    postHidden.BudgetValue = postFromUi.BudgetValue;
                }
                else
                {
                    _inPoster.Add(postFromUi);
                }
            }

            // Write to file
            var jsonString =
                JsonSerializer.Serialize(_inPoster);

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