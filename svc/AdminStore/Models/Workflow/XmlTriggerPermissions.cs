using System.Collections.Generic;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{

    [XmlRoot(ElementName = "P")]
    public class XmlTriggerPermissions
    {
        [XmlAttribute(AttributeName = "S")]
        public string Skip { get; set; } // bool

        private List<int> _groupIds;
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