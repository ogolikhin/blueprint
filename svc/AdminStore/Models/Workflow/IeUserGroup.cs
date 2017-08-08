using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    [XmlType("UserGroup")]
    public class IeUserGroup
    {
        // Optional, not used for the import, will be used for the update
        //========================================================
        // To make xml attribute nullable.
        [XmlIgnore]
        public int? Id { get; set; }

        [XmlAttribute("Id")]
        public int IdSerializable
        {
            get { return Id.GetValueOrDefault(); }
            set { Id = value; }
        }

        public bool ShouldSerializeIdSerializable()
        {
            return Id.HasValue;
        }
        //========================================================

        [XmlElement(IsNullable = false)]
        public string Name { get; set; }

        //========================================================
        // To make xml attribute nullable.
        [XmlIgnore]
        public bool? IsGroup { get; set; }

        [XmlAttribute("IsGroup")]
        public bool IsGroupSerializable
        {
            get { return IsGroup.GetValueOrDefault(); }
            set { IsGroup = value; }
        }

        public bool ShouldSerializeIsGroupSerializable()
        {
            return IsGroup.HasValue;
        }
        //========================================================
    }
}