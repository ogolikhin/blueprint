using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    [XmlType("Project")]
    public class IeProject
    {
        // Id or Path can be specified, Id has precedence over Path.
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
        public string Path { get; set; }
    }
}