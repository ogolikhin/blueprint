using System.Xml.Serialization;

namespace ServiceLibrary.Models.Workflow
{
    [XmlType("APC")]
    public class XmlPropertyChangeAction : XmlAction
    {
        [XmlElement("PID")]
        public int? PropertyTypeId { get; set; }
        public bool ShouldSerializePropertyTypeId() { return PropertyTypeId.HasValue; }

        [XmlElement("PV", IsNullable = false)]
        public string PropertyValue { get; set; }

        // Used for User properties and indicates that PropertyValue contains the group name.
        [XmlElement("IG")]
        public bool? IsGroup { get; set; }
        public bool ShouldSerializeIsGroup() { return IsGroup.HasValue; }
    }
}