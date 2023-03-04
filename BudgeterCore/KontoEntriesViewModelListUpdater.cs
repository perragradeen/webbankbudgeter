using Budgeter.Core.Entities;
using System.Collections;

namespace Budgeter.Core
{
    public class KontoEntriesViewModelListUpdater
    {
        public KontoEntriesViewModelListUpdater()
        {
            //NewItemsListOrg = new List<KontoEntry>();
            ToAddToListview = new List<KontoEntry>();
        }

        public SortedList KontoEntries { get; set; }

        /// <summary>
        /// Nya
        /// Dessa nya som kommer från datat (från bankens webhtml etc.)
        /// Dessa är de som analyseras.
        /// </summary>
        public SortedList NewKontoEntriesIn { get; set; }

        /// <summary>
        /// Slutresultat
        /// Dessa är slutresultatet.
        /// Dessa ska läggas in i UI-listor och sprars för att jämföras med nästa datahämtning.
        /// </summary>
        public List<KontoEntry> ToAddToListview { get; }

        /// <summary>
        /// Delresultat
        /// Dessa är de som kommer sättas i CheckAndAddNewItemsForLists.
        /// Dessa kommer från starten av CheckAndAddNewItemsForLists från de som redan ligger i UI
        /// </summary>
        public List<KontoEntry> NewItemsListEdited { get; set; }

        //public List<KontoEntry> NewItemsListOrg { get; }
    }
}