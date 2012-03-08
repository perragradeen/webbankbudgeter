using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using Utilities;

namespace Budgetterarn
{
    //<Categories xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
    //  <CategoryList>
    //    <Category description="hyra (inkl. 1k amortering)">
    //      <AutoCategoriseList>
    //        <AutoCategorise>HSB GÖTEBORG</AutoCategorise>
    //        <AutoCategorise>HSB kom eoingGÖTEBORG</AutoCategorise>
    //      </AutoCategoriseList>
    //    </Category>
    //    <Category description="si och akassa"></Category>

    
    //[XmlRoot("Categories")]
    /// <summary>
    /// TypAvKostnad är en kategori (category) Behållare till TypAvKostnad:er, och funktion för automatisk sättnig av TypAvKostnad för en entry t.ex. "hyra..." 
    /// Hette innan "NewItemsHandler"
    /// </summary>
    public class Categories
    {
        private List<Category> categoryList = new List<Category>();//Alla kategorier
        public List<Category> CategoryList
        {
            get { return categoryList; }
            set { categoryList = value; }
        }

        /// <summary>
        /// Returns the category with a description. Null if no one found.
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public Category GetCategoryWithDescription(string description) {
            foreach (var category in categoryList) {
                if (category.Description.Equals(description)) {
                    return category;
                }
            }

            return null;
        }

        public bool CategoryWithDescriptionExists(string description) {
            return GetCategoryWithDescription(description) != null;
        }

        /// <summary>
        /// Returns the AutoCategorise with a InfoDescription. Null if no one found.
        /// </summary>
        /// <param name="infoDescription"></param>
        /// <returns></returns>
        public AutoCategorise GetAutoCategoriseWithDescription(string infoDescription) {
            foreach (var category in categoryList) {
                foreach (var autoCat in category.AutoCategoriseList) {
                    if (autoCat.InfoDescription.Equals(infoDescription)) {
                        return autoCat;
                    }

                }
            }

            return null;
        }
        public Category GetCategoryForAutoCategoriseWithDescription(string infoDescription) {
            foreach (var category in categoryList) {
                foreach (var autoCat in category.AutoCategoriseList) {
                    if (autoCat.InfoDescription.Equals(infoDescription)) {
                        return category;
                    }

                }
            }

            return null;
        }

        public bool AutoCategoriseWithDescriptionExists(string infoDescription) {
            return GetAutoCategoriseWithDescription(infoDescription) != null;
        }
        public bool RemoveAutoCategoriseWithDescriptionIfItExists(string infoDescription) {
            return GetCategoryForAutoCategoriseWithDescription(infoDescription)
                .RemoveAutoCategoriseWithDescriptionIfItExists(
                infoDescription);
        }


        /// <summary>
        /// Find category with description
        /// </summary>
        /// <param name="entryInfoDescription">En beskrivning på entryn. Ex. "HSB GÖTEBORG"</param>
        /// <returns></returns>
        public string AutocategorizeType(string entryInfoDescription)
        {
            //Kolla alla kategorier (typer av kostnader) Ex. "hyra..."
            foreach (var currentCategory in categoryList)
            {
                //Kolla alla autokategorier för den nuvarande kategorin (typen av kostnad) Ex. "HSB GÖTEBORG"
                foreach (AutoCategorise currentAutoCategory in currentCategory.AutoCategoriseList)
                {
                    //Om den nuvarande autokategorins infobeskrivning är samma som den inskickade entryns infobeskrivning. ignorera CaSe (gemener/VERSALER)
                    //Ex. inskickat argument = "HSB GÖTEBORG", autokategorins infobeskrivning = "HSB GÖTEBORG"
                    if (entryInfoDescription != null && currentAutoCategory.InfoDescription.ToLower() == entryInfoDescription.ToLower())
                    {
                        //Returnera den nuvarande kategorins (föräldern till autokategorins) kategoribeskrivning (typ av kostnadsbeskr.)
                        //Ex. "hyra..."
                        return currentCategory.Description;
                    }

                }
            }

            return null;
        }

        internal bool SetNewAutoCategorize(string selectedCategoryText, AutoCategorise newAutoCategeory) {
            #region Sätt autokategori (lägg till eller ändra)
            var cats = this;

            //Done: om kategorin redan finns, ändra i den istället för att lägga till
            if (cats.CategoryWithDescriptionExists(selectedCategoryText)) {
                var selCategory = cats.GetCategoryWithDescription(selectedCategoryText);

                //Done: om InfoDescription redan finns, ändra i den istället för att lägga till

                //Kolla om samma kategori redan har samma infodescription
                var newAcId = newAutoCategeory.InfoDescription;
                if (selCategory.ObjectWithDescriptionExists(newAcId)) {
                    //Då finns redan samm InfoDescription under samma kategori, så gör ingenting
                    //Meddela inte anv., effekten blir den samma...
                } else {
                    //Kolla om någon annan kategori redan har beskrivningen
                    if (cats.AutoCategoriseWithDescriptionExists(newAcId)) {
                        //Isåfall ta bort den och lägg till en i den nyavalda kategorien. Fråga användaren först.
                        #region Fråga anv. om den är säker
                        //Fråga anv. om den är säker
                        var autoCatMessage = "Autokategorin finns redan som annan kategori:" + Environment.NewLine
                                             + cats.GetCategoryForAutoCategoriseWithDescription(newAcId) + Environment.NewLine
                                             + Environment.NewLine
                                             + "Vill du skriva över med autokategorin:" + Environment.NewLine
                                             + selCategory.Description + Environment.NewLine
                                             + "Varje gång info är:" + Environment.NewLine
                                             + cats.GetAutoCategoriseWithDescription(newAcId) + Environment.NewLine
                                             + "Will du skriva over?";
                        if (!ListViewWithComboBox.UserAcceptsFurtherAction(autoCatMessage, ListViewWithComboBox.AutoCatCpation)) {
                            return false;
                        }
                        #endregion

                        if (!cats.RemoveAutoCategoriseWithDescriptionIfItExists(newAcId))
                            MessageBox.Show("Mystical Error! " + newAcId + " did not exist or other error.");
                    }

                    //Lägg till InfoDescription till kategori som redan finns.
                    selCategory.AutoCategoriseList.Add(newAutoCategeory);
                }
            } else {
                cats.CategoryList.Add(
                    new Category
                    {
                        Description = selectedCategoryText,
                        AutoCategoriseList = new List<AutoCategorise> { newAutoCategeory }
                    });
            }
            #endregion

            return true;
        }

        //internal string AutocategorizeType(string p, string p_2) {
        //    throw new NotImplementedException();
        //}
    }

    //[XmlElement("Category")]
    public class Category
    {
        [XmlAttribute("description")]
        public string Description { get; set; }//Kategorins (typen av kostnads) beskrivning. Ex. "hyra..."

        //Todo: Gör listors inläsning till följande, så slipper man ha den extra taggen <AutoCategoriseList> med i xmlen, men iofs så sätter användaren autocats i programmet, och behöver aldrig fundera på listan...Om den inte vill se vilka auto som finns, men nu får man stå ut med att ändra i den "dula" filen.
        [XmlElementAttribute("AutoCategorise")]
        public List<AutoCategorise> AutoCategorise { get; set; }

        private List<AutoCategorise> autoCategoriseList = new List<AutoCategorise>();
        public List<AutoCategorise> AutoCategoriseList
        {
            get { return autoCategoriseList; }
            set { autoCategoriseList = value; }
        }

        /// <summary>
        /// Returns the AutoCategorise with a InfoDescription. Null if no one found.
        /// </summary>
        /// <param name="infoDescription"></param>
        /// <returns></returns>
        public AutoCategorise GetObjectWithDescription(string infoDescription) {
            foreach (var currObject in autoCategoriseList) {
                if (currObject.InfoDescription.Equals(infoDescription)) {
                    return currObject;
                }
            }

            return null;
        }
        public bool ObjectWithDescriptionExists(string description) {
            return GetObjectWithDescription(description) != null;
        }
        public bool RemoveAutoCategoriseWithDescriptionIfItExists(string infoDescription) {
            if (ObjectWithDescriptionExists(infoDescription)) {
                var index = 0;
                foreach (var currObject in autoCategoriseList) {
                    if (currObject.InfoDescription.Equals(infoDescription)) {
                        autoCategoriseList.RemoveAt(index);
                        return true;                        
                    }

                    index++;
                }
            }
            return false;
        }


        //For easier debugging
        public override string ToString() {
            return Description;
        }
    }

    //[XmlElement("AutoCategorise")]
    public class AutoCategorise
    {
        //En beskrivning på entryn. Ex. "HSB GÖTEBORG"
        [XmlText]
        public string InfoDescription { get; set; }

        ////For easier debugging
        public override string ToString() {
            return InfoDescription;
        }
    }

    public class CategoriesHolder
    {
        public static Categories AllCategories = new Categories();
        public static string SaveFilePath { get; set; }

        /// <summary>
        /// Lägg innehållet i en xml-fil till minnet i den statiska vaiabeln AllCategories
        /// </summary>
        /// <param name="filename">xml-fil som ska läsas in</param>
        public static void DeserializeObject(string filename)
        {
            //Spara sökvägen till nyligen inläst fil
            SaveFilePath = filename;

            try {
                AllCategories = SerializationFunctions.DeserializeObject(SaveFilePath, typeof(Categories)) as Categories;
            } catch (Exception e) {
                MessageBox.Show("Error in: CategoriesHolder, DeserializeObject! filname:" + filename + ". SysErr: " + e.Message);
            }
        }

        public static void Save() {
            SerializationFunctions.SerializeObject(SaveFilePath, typeof(Categories), AllCategories);
        }
    }

}
