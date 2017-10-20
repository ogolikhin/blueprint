using System.Xml.Serialization;

namespace ServiceLibrary.Models.Workflow
{
    [XmlType("UG")]
    public class XmlUserGroup
    {
        [XmlElement("ID")]
        public int Id { get; set; }

        // ========================================================
        // To make xml attribute nullable.
        [XmlIgnore]
        public bool? IsGroup { get; set; }

        [XmlAttribute("IG")]
        public bool IsGroupSerializable
        {
            get { return IsGroup.GetValueOrDefault(); }
            set { IsGroup = value; }
        }

        public bool ShouldSerializeIsGroupSerializable()
        {
            return IsGroup.HasValue;
        }
        // ========================================================
    }
}