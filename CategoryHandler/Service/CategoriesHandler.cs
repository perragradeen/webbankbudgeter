using System;
using System.Collections.Generic;
using CategoryHandler.Model;

namespace CategoryHandler.Service
{
    /// <summary>
    /// TypAvKostnad är en kategori (category) Behållare till TypAvKostnad:er, och funktion för automatisk sättnig av TypAvKostnad för en entry t.ex. "hyra..." 
    /// Hette innan "NewItemsHandler"
    /// </summary>
    public class CategoriesHandler
    {
        /// <summary>
        /// List of Categories
        /// </summary>
        private readonly List<Category> _categoryList;

        public CategoriesHandler(Categories allCategories)
        {
            _categoryList = allCategories.CategoryList;
        }

        public bool SetNewAutoCategorize(
            string selectedCategoryText,
            AutoCategorise newAutoCategeory,
            Func<string, string, bool> userAcceptsFurtherAction,
            string autoCatCpation)
        {
            #region Sätt autokategori (lägg till eller ändra)

            // Done: om kategorin redan finns, ändra i den istället för att lägga till
            if (CategoryWithDescriptionExists(selectedCategoryText))
            {
                var selCategory = GetCategoryWithDescription(selectedCategoryText);

                // Done: om InfoDescription redan finns, ändra i den istället för att lägga till

                // Kolla om samma kategori redan har samma infodescription
                var newAcId = newAutoCategeory.InfoDescription;
                if (selCategory.ObjectWithDescriptionExists(newAcId))
                {
                    // Då finns redan samm InfoDescription under samma kategori, så gör ingenting
                    // Meddela inte anv., effekten blir den samma...
                }
                else
                {
                    // Kolla om någon annan kategori redan har beskrivningen
                    if (AutoCategoriseWithDescriptionExists(newAcId))
                    {
                        // Isåfall ta bort den och lägg till en i den nyavalda kategorien. Fråga användaren först.

                        #region Fråga anv. om den är säker

                        // Fråga anv. om den är säker
                        var autoCatMessage = "Autokategorin finns redan som annan kategori:" + Environment.NewLine
                            + GetCategoryForAutoCategoriseWithDescription(newAcId)
                            + Environment.NewLine + Environment.NewLine
                            + "Vill du skriva över med autokategorin:" + Environment.NewLine
                            + selCategory.Description + Environment.NewLine + "Varje gång info är:"
                            + Environment.NewLine + GetAutoCategoriseWithDescription(newAcId)
                            + Environment.NewLine + "Will du skriva over?";
                        if (
                            !userAcceptsFurtherAction(
                                autoCatMessage, autoCatCpation))
                        {
                            return false;
                        }

                        #endregion

                        if (!RemoveAutoCategoriseWithDescriptionIfItExists(newAcId))
                        {
                            var message = "SetNewAutoCategorize Mystical Error! " + newAcId +
                                          " did not exist or other error.";
                            Console.WriteLine(message);
                            //Todo: log funktion displayed to user Or catch keypress errors
                        }
                    }

                    // Lägg till InfoDescription till kategori som redan finns.
                    selCategory.AutoCategoriseList.Add(newAutoCategeory);
                }
            }
            else
            {
                _categoryList.Add(
                    new Category
                    {
                        Description = selectedCategoryText,
                        AutoCategoriseList = new List<AutoCategorise> {newAutoCategeory}
                    });
            }

            #endregion

            return true;
        }

        private Category GetCategoryForAutoCategoriseWithDescription(string infoDescription)
        {
            foreach (var category in _categoryList)
            {
                foreach (var autoCat in category.AutoCategoriseList)
                {
                    if (autoCat.InfoDescription.Equals(infoDescription))
                    {
                        return category;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the category with a description. Null if no one found.
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        private Category GetCategoryWithDescription(string description)
        {
            foreach (var category in _categoryList)
            {
                if (category.Description.Equals(description))
                {
                    return category;
                }
            }

            return null;
        }

        private bool CategoryWithDescriptionExists(string description)
        {
            return GetCategoryWithDescription(description) != null;
        }

        /// <summary>
        /// Returns the AutoCategorise with a InfoDescription. Null if no one found.
        /// </summary>
        /// <param name="infoDescription"></param>
        /// <returns></returns>
        private AutoCategorise GetAutoCategoriseWithDescription(string infoDescription)
        {
            foreach (var category in _categoryList)
            {
                foreach (var autoCat in category.AutoCategoriseList)
                {
                    if (autoCat.InfoDescription.Equals(infoDescription))
                    {
                        return autoCat;
                    }
                }
            }

            return null;
        }

        private bool AutoCategoriseWithDescriptionExists(string infoDescription)
        {
            return GetAutoCategoriseWithDescription(infoDescription) != null;
        }

        private bool RemoveAutoCategoriseWithDescriptionIfItExists(string infoDescription)
        {
            return
                GetCategoryForAutoCategoriseWithDescription(infoDescription)
                    .RemoveAutoCategoriseWithDescriptionIfItExists(infoDescription);
        }

        // internal string AutocategorizeType(string p, string p_2) {
        // throw new NotImplementedException();
        // }
    }
}