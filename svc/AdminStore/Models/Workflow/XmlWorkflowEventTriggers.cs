using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    [XmlRoot(ElementName = "TS")]
    public class XmlWorkflowEventTriggers
    {
        private List<XmlWorkflowEventTrigger> _triggers;
        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlElement(ElementName = "T")]
        public List<XmlWorkflowEventTrigger> Triggers
        {
            get
            {
                return _triggers ?? (_triggers = new List<XmlWorkflowEventTrigger>());
            }
            set
            {
                _triggers = value;
            }
        }
    }
}