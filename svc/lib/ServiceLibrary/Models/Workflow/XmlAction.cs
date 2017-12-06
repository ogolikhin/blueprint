using System.Xml.Serialization;
using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Models.Workflow
{
    [XmlType("A")]
    public abstract class XmlAction
    {
        [XmlIgnore]
        public abstract ActionTypes ActionType { get; }

        [XmlElement("N", IsNullable = false)]
        public string Name { get; set; }
    }
}