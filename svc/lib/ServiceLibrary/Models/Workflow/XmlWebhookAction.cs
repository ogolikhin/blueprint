using ServiceLibrary.Models.Enums;
using System.Xml.Serialization;

namespace ServiceLibrary.Models.Workflow
{
    [XmlType("AWH")]
    public class XmlWebhookAction : XmlAction
    {
        [XmlIgnore]
        public override ActionTypes ActionType => ActionTypes.Webhook;

        [XmlElement("Id")]
        public int WebhookId { get; set; }
    }
}
