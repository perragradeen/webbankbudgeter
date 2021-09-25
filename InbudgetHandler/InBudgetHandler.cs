using Budgeter.Core.Entities;
using InbudgetToTable.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBankBudgeter.Service.Model;

namespace InbudgetToTable
{
    public class InBudgetHandler
    {
        public const string SummaText = "Summa";

        private readonly InBudgetHandlerFileHandler _inBudgetHandlerFileHandler;

        public async Task<List<InBudget>> GetInPoster()
        {
            return await _inBudgetHandlerFileHandler.GetInPoster();
        }

        public InBudgetHandler(string inBudgetFilePath)
        {
            _inBudgetHandlerFileHandler =
                new InBudgetHandlerFileHandler(inBudgetFilePath);
        }

        public void SparaInPosterPåDisk(List<InBudget> inPoster)
        {
            _inBudgetHandlerFileHandler.SparaInPosterPåDisk(inPoster);
        }

        internal async Task<List<InBudget>> SetInPosterFromDisk()
        {
            return await _inBudgetHandlerFileHandler.SetInPosterFromDisk();
        }

        public void SetInPoster(List<InBudget> inBudgets)
        {
            _inBudgetHandlerFileHandler.SetInPoster(inBudgets);
        }

        /// <summary>
        /// Ex
        /// 2021-06
        /// 2016-05
        /// 2016-04
        /// </summary>
        /// <param name="inPoster"></param>
        /// <param name="nu"></param>
        /// <returns></returns>
        public DateTime HämtaSenasteDatum(List<InBudget> inPoster, DateTime nu)
        {
            var datum = inPoster
                .Select(i => i.YearAndMonth)
                .OrderByDescending(d => d)
                .ToList();

            foreach (var nuvarandeDatum in datum)
            {
                //	YearAndMonth == nu?			
                //		returnera	alla de inbudgetar?	eller
                if (MånadOchÅrÄrSamma(nu, nuvarandeDatum))
                {
                    return nu;
                }

                //	hämta senaste finns			
                return nuvarandeDatum;
            }

            throw new ArgumentException("Finns inga poster");
        }

        private bool MånadOchÅrÄrSamma(DateTime ena, DateTime andra)
        {
            return ena.Year == andra.Year
                && ena.Month == andra.Month;
        }

        public async Task<List<Rad>> HämtaRaderFörUiBindningAsync()
        {
            var rader = new List<Rad>();

            var inPoster = await GetInPoster();

            // Sortera på datum stigande
            inPoster = inPoster
                .OrderBy(i=>i.YearAndMonth)
                .ToList();

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

        public async Task<List<string>> HämtaRubrikerPåInPosterAsync()
        {
            var inPoster = await GetInPoster();

            var sorteradeÅrOMånader =
                inPoster.OrderBy(tid => tid.YearAndMonth);
            return HämtaRubrikerPåInPoster(sorteradeÅrOMånader);
        }

        private List<string> HämtaRubrikerPåInPoster(IOrderedEnumerable<InBudget> sorteradeÅrOMånader)
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
