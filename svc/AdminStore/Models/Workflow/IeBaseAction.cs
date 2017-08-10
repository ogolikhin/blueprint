using System.Xml.Serialization;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Models.Workflow
{
    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
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