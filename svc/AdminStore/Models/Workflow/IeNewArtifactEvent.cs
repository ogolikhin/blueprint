using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
    [XmlType("NewArtifact")]
    public class IeNewArtifactEvent : IeEvent
    {
        [XmlIgnore]
        public override EventTypes EventType => EventTypes.NewArtifact;
    }
}