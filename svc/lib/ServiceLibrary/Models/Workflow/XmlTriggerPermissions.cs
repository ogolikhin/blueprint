using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ServiceLibrary.Models.Workflow
{

    [XmlType("P")]
    public class XmlTriggerPermissions
    {
        // ========================================================
        // To make xml attribute nullable.
        [XmlIgnore]
        public int? Skip { get; set; } // Should be 0 or 1

        [XmlAttribute("S")]
        public int SkipSerializable
        {
            get { return Skip.GetValueOrDefault(); }
            set { Skip = value; }
        }

        public bool ShouldSerializeSkipSerializable()
        {
            return Skip.HasValue;
        }
        // ========================================================

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