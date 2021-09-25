using Budgeter.Core.Entities;
using InbudgetToTable;
using InbudgetToTable.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebBankBudgeter.Service.Model;
using WebBankBudgeter.Service.Services;

namespace WebBankBudgeter.UiBinders
{
    internal class InBudgetUiHandler
    {
        private readonly InBudgetHandler _inBudgetHandler;
        private readonly DataGridView _gv_incomes;
        private readonly Action<string> _writeLineToOutputAndScrollDown;

        public InBudgetUiHandler(
            InBudgetHandler inBudgetHandler,
            DataGridView gv_incomes,
            Action<string> writeLineToOutputAndScrollDown)
        {
            _inBudgetHandler = inBudgetHandler;
            _gv_incomes = gv_incomes;
            _writeLineToOutputAndScrollDown = writeLineToOutputAndScrollDown;
        }

        public async Task<List<Rad>> HämtaRaderFörUiBindningAsync()
        {
            return await _inBudgetHandler.HämtaRaderFörUiBindningAsync();
        }

        public async Task<List<string>> HämtaRubrikePåInPosterAsync()
        {
            return await _inBudgetHandler.HämtaRubrikerPåInPosterAsync(); // ev göra: cacha denna o HämtaInPosterFrånUi
        }

        public void SparaInPosterPåDisk()
        {
            _inBudgetHandler.SparaInPosterPåDisk(
                HämtaInPosterFrånUITabell(_gv_incomes));
        }

        public void SparaInPosterPåDisk(List<InBudget> inPosterAttMerga)
        {
            var inPosterFrånUI = HämtaInPosterFrånUITabell(_gv_incomes);
            inPosterAttMerga.AddRange(inPosterFrånUI);
            _inBudgetHandler.SparaInPosterPåDisk(inPosterAttMerga);
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

                try
                {
                    foreach (var kolumnVärde in rad.Kolumner)
                    {
                        bindToUiElement
                            .Rows[radNummer]
                            .Cells[kolumnNummer++]
                            .Value = SkrivVärdeSomText(kolumnVärde);
                    }
                }
                catch (Exception e)
                {
                    _writeLineToOutputAndScrollDown(e.Message);
                }
            }
        }

        private static string SkrivVärdeSomText(KeyValuePair<string, double> kolumnVärde)
        {
            return kolumnVärde.Value.ToString(CultureInfo.InvariantCulture);
        }

        private static List<InBudget> HämtaInPosterFrånUITabell(DataGridView bindToUiElement)
        {
            var inPoster = new List<InBudget>();
            foreach (DataGridViewRow rad in bindToUiElement.Rows)
            {
                //-		rad	{DataGridViewRow { Index=0 }}	object {System.Windows.Forms.DataGridViewRow}

                var celler = rad.Cells.Cast<DataGridViewCell>().ToList();

                var kolumnerEfter1 = celler.Where(värde => värde.ColumnIndex != 0);

                var radRubrik = HämtaRubrik(celler);
                if (radRubrik == InBudgetHandler.SummaText
                    || string.IsNullOrWhiteSpace(radRubrik))
                {
                    continue;
                }

                var kolumnRubrikHämtare = KolumnRubrikHämtare(bindToUiElement);
                foreach (var kolumn in kolumnerEfter1)
                {
                    // Hämta den kolumnrubrik som kolumnens index har
                    var rubrikText = GetHeaderText(kolumnRubrikHämtare, kolumn);
                    var årMånad = Transaction.GetDateFromYearMonthName(rubrikText);

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

        private static List<DataGridViewTextBoxColumn> KolumnRubrikHämtare(DataGridView bindToUiElement)
        {
            return bindToUiElement.Columns.Cast<DataGridViewTextBoxColumn>().ToList();
        }

        private static string GetHeaderText(List<DataGridViewTextBoxColumn> kolumnRubrikGet, DataGridViewCell kolumn)
        {
            return kolumnRubrikGet
                                    .FirstOrDefault(kolumnLetare => kolumnLetare.Index == kolumn.ColumnIndex)
                                        ?.HeaderText;
        }

        private static string HämtaRubrik(List<DataGridViewCell> celler)
        {
            return celler
                .FirstOrDefault(värde => värde.ColumnIndex == 0)?
                .Value?
                .ToString();
        }
    }
}