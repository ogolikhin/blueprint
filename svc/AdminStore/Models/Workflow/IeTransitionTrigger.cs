using System;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    /// <summary>
    /// Transition Trigger
    /// </summary>
    [Serializable()]
    [XmlType("TransitionTrigger")]
    public class IeTransitionTrigger : IeTrigger
    {
        public IeTransitionTrigger() : base(TriggerTypes.Transition)
        {
            
        }
    }
}