using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{

    [XmlRoot(ElementName = "P")]
    public class XmlTriggerPermissions
    {
        [XmlAttribute(AttributeName = "S")]
        public string Skip { get; set; } // bool

        private List<int> _groupIds;
        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlElement(ElementName = "G")]
        public List<int> GroupIds
        {
            get
            {
                return _groupIds ?? (_groupIds = new List<int>());
            }
            set
            {
                _groupIds = value;
            }
        }
    }
  
}