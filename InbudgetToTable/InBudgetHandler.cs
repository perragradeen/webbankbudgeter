using Budgeter.Core.Entities;
using InbudgetToTable.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebBankBudgeter.Service.Model;
using WebBankBudgeter.Service.Services;

namespace InbudgetToTable
{
    public class InBudgetHandler
    {
        public const string SummaText = "Summa";
        private readonly string _inBudgetFilePath;

        private List<InBudget> _inPoster;
        public async Task<List<InBudget>> GetInPoster()
        {
            if (_inPoster == null)
            {
                _inPoster = await GetIncomes();
            }

            return _inPoster;
        }

        public InBudgetHandler(string inBudgetFilePath)
        {
            _inBudgetFilePath = inBudgetFilePath;
        }
        public static List<InBudget> InPosterTillTable(DataGridView bindToUiElement)
        {
            var inPoster = new List<InBudget>();
            foreach (DataGridViewRow rad in bindToUiElement.Rows)
            {
                //-		rad	{DataGridViewRow { Index=0 }}	object {System.Windows.Forms.DataGridViewRow}

                var celler = rad.Cells.Cast<DataGridViewCell>();
                var sistaRadensIndexNummer = bindToUiElement.Rows.GetLastRow(DataGridViewElementStates.Displayed);
                //if (rad.Index == sistaRadensIndexNummer)
                //{
                //    break;
                //}

                var kolumnerEfter1 = celler.Where(värde => värde.ColumnIndex != 0);

                var radRubrik = celler.Where(värde => värde.ColumnIndex == 0)
                    .FirstOrDefault()?.Value?.ToString();
                if (
                    radRubrik == SummaText
                    || string.IsNullOrWhiteSpace(radRubrik))
                {
                    continue;
                }

                var kolumnRubrikGet = bindToUiElement.Columns.Cast<DataGridViewTextBoxColumn>();
                foreach (var kolumn in kolumnerEfter1)
                {
                    // Hämta den kolumnrubrik som kolumnens index har
                    var årMånad = Transaction.GetDateFromYearMonthName(
                        kolumnRubrikGet.FirstOrDefault(kolumnLetare => kolumnLetare.Index == kolumn.ColumnIndex).HeaderText);
                    var värde = Conversions.SafeGetDouble(kolumn.Value);

                    inPoster.Add(new InBudget
                    {
                        BudgetValue = värde,
                        CategoryDescription = radRubrik,
                        YearAndMonth = årMånad
                    });
                }
            }

            return inPoster;
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

        public async Task<List<InBudget>> GetIncomes()
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

        public async Task<List<Rad>> HämtaRaderFörUiBindningAsync()
        {
            var rader = new List<Rad>();

            var inPoster = await GetInPoster();

            // Loopa data
            // Grupper på categori (per rad)
            foreach (var inPostGrupp in inPoster
                .GroupBy(inPos => inPos.CategoryDescription))
            {
                // Skriv ut cat i kolumn 1
                var rad = new Rad { RadNamnY = inPostGrupp.Key };


                // Skriv ut värdet i kr kolumner från vänster till höger
                var inPosterna = inPostGrupp
                    .ToList();
                //.OrderBy(tid => tid.YearAndMonth)
                //.Select(inPosten => inPosten.BudgetValue)
                //.ToDictionary<string, double>(d=>d, v=> v.BudgetValue);
                //rad.Kolumner.AddRange(inPosterna);
                foreach (var inPost in inPosterna)
                {
                    var årOMånadKey = Transaction.GetYearMonthName(inPost.YearAndMonth);
                    if (!rad.Kolumner.ContainsKey(årOMånadKey))
                    {
                        rad.Kolumner.Add(årOMånadKey, 0);
                    }

                    rad.Kolumner[årOMånadKey] = inPost.BudgetValue;
                }

                rader.Add(rad);
            }

            // Summering på kolumner
            var kolumnSummor = new Dictionary<string, double>();
            foreach (var rad in rader)
            {
                foreach (var kolumn in rad.Kolumner)
                {
                    if (!kolumnSummor.ContainsKey(kolumn.Key))
                    {
                        kolumnSummor.Add(kolumn.Key, 0);
                    }

                    kolumnSummor[kolumn.Key] += kolumn.Value;
                }
            }

            var summaRad = new Rad
            {
                RadNamnY = SummaText,
                Kolumner = kolumnSummor
            };

            rader.Add(summaRad);
            return rader;
        }

        public async Task<List<string>> HämtaRubrikePåInPosterAsync()
        {
            var inPoster = await GetInPoster();

            var sorteradeÅrOMånader =
                inPoster.OrderBy(tid => tid.YearAndMonth);
            return HämtaRubrikePåInPoster(sorteradeÅrOMånader);
        }

        private List<string> HämtaRubrikePåInPoster(IOrderedEnumerable<InBudget> sorteradeÅrOMånader)
        {
            var inPosterKolumnRubriker = new List<string>();
            foreach (var inPost in sorteradeÅrOMånader)
            {
                var årOMånad = Transaction.GetYearMonthName(inPost.YearAndMonth);

                if (!inPosterKolumnRubriker.Contains(årOMånad))
                {
                    inPosterKolumnRubriker.Add(årOMånad);
                }
            }

            return inPosterKolumnRubriker;
        }
    }
}
