using System;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    /// <summary>
    /// Property Change Trigger
    /// </summary>
    [Serializable()]
    [XmlType("PropertyChangeTrigger")]
    public class IePropertyChangeTrigger : IeTrigger
    {
        public IePropertyChangeTrigger() : base(TriggerTypes.PropertyChange)
        {

        }
    }
}