using CategoryHandler.Model;
using CategoryHandler.Service;
using XmlSerializer;

namespace CategoryHandler
{
    public static class CategoriesHolder
    {
        private static Categories AllCategories = new();
        public static CategoriesHandler AllCategoriesHandler;
        private static string SaveFilePath { get; set; }

        /// <summary>
        /// Lägg innehållet i en xml-fil till minnet i den statiska vaiabeln AllCategories
        /// </summary>
        /// <param name="filename">xml-fil som ska läsas in</param>
        public static void LoadAllCategoriesAndCreateHandler(string filename)
        {
            // Spara sökvägen till nyligen inläst fil
            SaveFilePath = filename;

            try
            {
                AllCategories = SerializationFunctions
                    .DeserializeObject<Categories>(SaveFilePath);

                AllCategoriesHandler = new CategoriesHandler(AllCategories);
            }
            catch (Exception e)
            {
                throw new Exception(
                    "Error in: CategoriesHolder, DeserializeObject! filname:"
                    + filename + ". SysErr: " + e.Message);
            }
        }

        public static void Save()
        {
            SerializationFunctions.SerializeObject(
                SaveFilePath,
                typeof(Categories),
                AllCategories);
        }

        /// <summary>
        /// Find category with description
        /// </summary>
        /// <param name="entryInfoDescription">En beskrivning på entryn. Ex. "HSB GÖTEBORG"</param>
        /// <returns></returns>
        public static string AutocategorizeType(string entryInfoDescription)
        {
            // Kolla alla kategorier (typer av kostnader) Ex. "hyra..."
            foreach (var currentCategory in GetCategoriesList())
            {
                // Kolla alla autokategorier för den nuvarande kategorin (typen av kostnad) Ex. "HSB GÖTEBORG"
                foreach (var currentAutoCategory in currentCategory.AutoCategoriseList)
                {
                    // Om den nuvarande autokategorins infobeskrivning är samma som den inskickade entryns infobeskrivning. ignorera CaSe (gemener/VERSALER)
                    // Ex. inskickat argument = "HSB GÖTEBORG", autokategorins infobeskrivning = "HSB GÖTEBORG"
                    if (entryInfoDescription != null
                        && currentAutoCategory.InfoDescription.ToLower().Trim()
                            == entryInfoDescription.ToLower().Trim()
                       )
                    {
                        // Returnera den nuvarande kategorins (föräldern till autokategorins) kategoribeskrivning (typ av kostnadsbeskr.)
                        // Ex. "hyra..."
                        return currentCategory.Description;
                    }
                }
            }

            return null;
        }

        public static IEnumerable<Category> GetCategoriesList()
        {
            return AllCategories.CategoryList;
        }
    }
}