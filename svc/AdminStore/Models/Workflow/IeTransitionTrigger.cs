using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    [XmlType("TransitionTrigger")]
    class IeTransitionTrigger : IeTrigger
    {
        [XmlIgnore]
        public override TriggerType TriggerType => TriggerType.Transition;

        [XmlElement(typeof(IeGenerateAction), ElementName = "GenerateAction")]
        [XmlElement(typeof(IePropertyChangeAction), ElementName = "PropertyChangeAction")]
        public IeBaseAction Action { get; set; }
    }
}