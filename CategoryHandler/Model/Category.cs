using System.Collections.Generic;
using System.Xml.Serialization;

namespace CategoryHandler.Model
{
    // [XmlElement("Category")]
    public class Category
    {
        [XmlAttribute("description")]
        public string Description { get; set; } // Kategorins (typen av kostnads) beskrivning. Ex. "hyra..."

        /// <summary>
        /// Kategorins grupp, övergripande grupp. Ie.
        /// "ID_ACCOMMODATION",
        /// "ID_HOUSEHOLD",
        /// "ID_OTHER",
        /// "ID_TRANSPORT"
        /// "ID_INCOME"
        /// </summary>
        [XmlAttribute("group")]
        public string Group { get; set; }

        // Todo: Gör listors inläsning till följande, så slipper man ha den extra taggen <AutoCategoriseList> med i xmlen, men iofs så sätter användaren autocats i programmet, och behöver aldrig fundera på listan...Om den inte vill se vilka auto som finns, men nu får man stå ut med att ändra i den "fula" filen.
        //[XmlElement("AutoCategorise")]
        //public List<AutoCategorise> AutoCategorise { get; set; }

        public List<AutoCategorise> AutoCategoriseList { get; set; } = new List<AutoCategorise>();

        /// <summary>
        /// Returns the AutoCategorise with a InfoDescription. Null if no one found.
        /// </summary>
        /// <param name="infoDescription"></param>
        /// <returns></returns>
        private AutoCategorise GetObjectWithDescription(string infoDescription)
        {
            foreach (var currObject in AutoCategoriseList)
            {
                if (currObject.InfoDescription.Equals(infoDescription))
                {
                    return currObject;
                }
            }

            return null;
        }

        public bool ObjectWithDescriptionExists(string description)
        {
            return GetObjectWithDescription(description) != null;
        }

        public bool RemoveAutoCategoriseWithDescriptionIfItExists(string infoDescription)
        {
            if (ObjectWithDescriptionExists(infoDescription))
            {
                var index = 0;
                foreach (var currObject in AutoCategoriseList)
                {
                    if (currObject.InfoDescription.Equals(infoDescription))
                    {
                        AutoCategoriseList.RemoveAt(index);
                        return true;
                    }

                    index++;
                }
            }

            return false;
        }

        // For easier debugging
        public override string ToString()
        {
            return Description;
        }
    }
}