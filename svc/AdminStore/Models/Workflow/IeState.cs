using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    [XmlType("State")]
    public class IeState
    {
        // Optional, not used for the import, can be used for the update
        //[XmlElement]
        //public int? Id { get; set; }
        //public bool ShouldSerializeId() { return Id.HasValue; }

        [XmlElement(IsNullable = false)]
        public string Name { get; set; }

        [XmlElement(IsNullable = false)]
        public string Description { get; set; }

        //========================================================
        // To make xml attribute nullable.
        [XmlIgnore]
        public bool? IsInitial { get; set; }

        [XmlAttribute("IsInitial")]
        public bool IsInitialSerializable
        {
            get { return IsInitial.GetValueOrDefault(); }
            set { IsInitial = value; }
        }

        public bool ShouldSerializeIsInitialSerializable()
        {
            return IsInitial.HasValue;
        }
        //========================================================
    }
}