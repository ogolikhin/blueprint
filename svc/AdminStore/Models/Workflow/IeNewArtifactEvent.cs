using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    [XmlType("NewArtifact")]
    public class IeNewArtifactEvent : IeEvent
    {
        [XmlIgnore]
        public override EventType EventType => EventType.NewArtifact;
    }
}