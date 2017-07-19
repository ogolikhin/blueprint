using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    [XmlType("AEN")]
    public class XmlEmailNotificationAction : XmlAction
    {
        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("ES", IsNullable = false), XmlArrayItem("E")]
        public List<string> Emails { get; set; }

        [XmlElement("PID")]
        public int? PropertyTypeId { get; set; }
        public bool ShouldSerializePropertyTypeId() { return PropertyTypeId.HasValue; }

        [XmlElement("M")]
        public string Message { get; set; }
    }
}