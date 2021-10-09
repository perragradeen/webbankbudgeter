using System.Collections.Generic;
using System.Xml.Serialization;

namespace Budgetterarn
{
    [XmlRoot(ElementName = "GeneralSettings")]
    public class GeneralSettings : List<Property>
    {
    }

    public class Property
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Value { get; set; }

        [XmlAttribute]
        public string RownumberInTextfile { get; set; }
        [XmlAttribute]
        public string TextfileName { get; set; }
    }
}