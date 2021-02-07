using System.Collections.Generic;

namespace CategoryHandler.Model
{
    // <Categories xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
    // <CategoryList>
    // <Category description="hyra (inkl. 1k amortering)">
    // <AutoCategoriseList>
    // <AutoCategorise>HSB GÖTEBORG</AutoCategorise>
    // <AutoCategorise>HSB kom eoingGÖTEBORG</AutoCategorise>
    // </AutoCategoriseList>
    // </Category>
    // <Category description="si och akassa"></Category>

    // [XmlRoot("Categories")]
    /// <summary>
    /// TypAvKostnad är en kategori (category) Behållare till TypAvKostnad:er, och funktion för automatisk sättnig av TypAvKostnad för en entry t.ex. "hyra..." 
    /// Hette innan "NewItemsHandler"
    /// </summary>
    public class Categories
    {
        /// <summary>
        /// Set from Xml file
        /// </summary>
        public List<Category> CategoryList { get; set; } = new List<Category>();
    }
}