using System.Collections.Generic;
using System.Xml.Serialization;

namespace ServiceLibrary.Repositories.ProjectMeta.PropertyXml.Models
{
    // Originates from Raptor solution with some modifications
    [XmlRoot(ElementName = "CPS")]
    public class XmlCustomProperties
    {
        private List<XmlCustomProperty> _customProperties;
        [XmlElement(ElementName = "CP")]
        public List<XmlCustomProperty> CustomProperties => _customProperties ?? (_customProperties = new List<XmlCustomProperty>());
    }
}
