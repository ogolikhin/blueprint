using System;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    [Serializable()]
    [XmlType("TransitionTrigger")]
    public class IeTransitionTrigger : IeTrigger
    {
        public IeTransitionTrigger() : base(TriggerTypes.Transition)
        {
            
        }
    }
}