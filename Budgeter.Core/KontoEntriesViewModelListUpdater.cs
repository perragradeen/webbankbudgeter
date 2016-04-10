using Budgeter.Core.Entities;
using System.Collections;
using System.Collections.Generic;

namespace Budgeter.Core
{
    public class KontoEntriesViewModelListUpdater
    {
        public KontoEntriesViewModelListUpdater()
        {
            NewIitemsListOrg = new List<KontoEntry>();
            ToAddToListview = new List<KontoEntry>();
            ToAddToOrgListview = new List<KontoEntry>();
        }

        public SortedList kontoEntries { get; set; }

        /// <summary>
        /// Nya
        /// Dessa nya som kommer från datat (från bankens webhtml etc.)
        /// Dessa är de som analyseras.
        /// </summary>
        public SortedList NewKontoEntriesIn { get; set; }
        
        public List<KontoEntry> ToAddToOrgListview { get; set; }

        /// <summary>
        /// Slutresultat
        /// Dessa är slutresultatet.
        /// Dessa ska läggas in i UI-listor och sprars för att jämföras med nästa datahämtning.
        /// </summary>
        public List<KontoEntry> ToAddToListview { get; set; }

        /// <summary>
        /// Delresultat
        /// Dessa är de som kommer sättas i CheckAndAddNewItemsForLists.
        /// Dessa kommer från starten av CheckAndAddNewItemsForLists från de som redan ligger i UI
        /// </summary>
        public List<KontoEntry> NewIitemsListEdited { get; set; }
        public List<KontoEntry> NewIitemsListOrg { get; set; }
    }
}