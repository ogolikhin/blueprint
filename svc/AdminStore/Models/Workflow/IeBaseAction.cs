using System.Xml.Serialization;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Models.Workflow
{
    /// <summary>
    /// Base class for Actions of specific type
    /// </summary>
    [XmlType("BaseAction")]
    public abstract class IeBaseAction
    {
        [XmlIgnore]
        public abstract ActionTypes ActionType { get; }

        // Not used, but we keep it for now
        [XmlElement(IsNullable = false)]
        public string Name { get; set; }
    }

}