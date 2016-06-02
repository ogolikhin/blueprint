using System.Collections.Generic;
using System.Xml.Serialization;

namespace ArtifactStore.Repositories.PropertyXml.Models
{
    // Originates from Raptor solution with some modifications
    [XmlRoot(ElementName = "CPS")]
    public class XmlCustomProperties
    {
        [XmlElement(ElementName = "CP")]
        public List<XmlCustomProperty> CustomProperties { get; set; }
    }
}
