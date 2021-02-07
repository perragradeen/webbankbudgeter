using System.Xml.Serialization;

namespace CategoryHandler.Model
{
    // [XmlElement("AutoCategorise")]
    public class AutoCategorise
    {
        // En beskrivning på entryn. Ex. "HSB GÖTEBORG"
        [XmlText]
        public string InfoDescription { get; set; }

        ////For easier debugging
        public override string ToString()
        {
            return InfoDescription;
        }
    }
}