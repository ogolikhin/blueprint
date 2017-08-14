using System.Xml.Serialization;

namespace AdminStore.Models.Workflow {

    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
    [XmlType("Trigger")]
    public class IeTrigger
    {

        [XmlElement(IsNullable = false)]
        public string Name { get; set; }

        [XmlElement(typeof(IeEmailNotificationAction), ElementName = "EmailNotificationAction")]
        [XmlElement(typeof(IeGenerateAction), ElementName = "GenerateAction")]
        [XmlElement(typeof(IePropertyChangeAction), ElementName = "PropertyChangeAction")]
        public IeBaseAction Action { get; set; }

        [XmlElement(typeof(IeStateCondition), ElementName = "StateCondition")]
        public IeCondition Condition { get; set; }
    }
}