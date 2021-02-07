using Budgeter.Core.Entities;
using InbudgetToTable;
using InbudgetToTable.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebBankBudgeter.UiBinders
{
    internal class InBudgetUiHandler
    {
        private readonly InBudgetHandler _inBudgetHandler;
        private readonly DataGridView _gv_incomes;

        public InBudgetUiHandler(
            InBudgetHandler inBudgetHandler,
            DataGridView gv_incomes)
        {
            _inBudgetHandler = inBudgetHandler;
            _gv_incomes = gv_incomes;
        }

        public async Task<List<Rad>> HämtaRaderFörUiBindningAsync()
        {
            return await _inBudgetHandler.HämtaRaderFörUiBindningAsync();
        }

        public async Task<List<string>> HämtaRubrikePåInPosterAsync()
        {
            return await _inBudgetHandler.HämtaRubrikePåInPosterAsync(); // ev göra: cacha denna o HämtaInPosterFrånUi
        }

        public List<InBudget> HämtaInPosterFrånUi()//todo: ta bort
        {
            return InBudgetHandler.InPosterTillTable(_gv_incomes);
        }

        public void SparaInPosterPåDisk(List<InBudget> inPoster)
        {
            _inBudgetHandler.SparaInPosterPåDisk(inPoster);
        }

        public async Task BindInPosterToUiAsync()
        {
            // Sortera år+månad kronologiskt
            var rader = await _inBudgetHandler.HämtaRaderFörUiBindningAsync();
            var inPosterKolumnRubriker = await _inBudgetHandler.HämtaRubrikePåInPosterAsync();

            // UI
            BindInPosterRaderTillUi(rader, inPosterKolumnRubriker);
        }

        public async Task<List<InBudget>> GetInPoster()
        {
            return await _inBudgetHandler.GetInPoster();
        }

        private void BindInPosterRaderTillUi(List<Rad> rader, List<string> inPosterKolumnRubriker)
        {
            var bindToUiElement = _gv_incomes;
            BindInPosterRaderTillUi(rader, inPosterKolumnRubriker, bindToUiElement);
        }

        public void BindInPosterRaderTillUi(List<Rad> rader, List<string> inPosterKolumnRubriker, DataGridView bindToUiElement)
        {
            // UI
            // Skriv ut år+månad på rad 1 med headers från vänster till höger
            foreach (var rubrik in inPosterKolumnRubriker)
            {
                bindToUiElement.Columns.Add(rubrik, rubrik);
            }

            // TODO: ev. fyll i luckor med blankt o matcha på månad+år etc...
            foreach (var rad in rader)
            {
                var radNummer = bindToUiElement.Rows.Add();
                var kolumnNummer = 0;

                bindToUiElement.Rows[radNummer].Cells[kolumnNummer++].Value = rad.RadNamnY;

                foreach (var kolumnVärde in rad.Kolumner)
                {
                    bindToUiElement.Rows[radNummer].Cells[kolumnNummer++].Value = kolumnVärde.Value.ToString();
                }
            }
        }

        //private List<InBudget> Ta_ut_alla_unika_månader(List<InBudget> inPoster)
        //{
        //    var år = inPoster.GroupBy(post => post.YearAndMonth.Year).ToList();
        //    var årOMånad = år.ToList().GroupBy(post => post.ToList().);

        //    return årOMånad.ToList();
        //}
    }
}