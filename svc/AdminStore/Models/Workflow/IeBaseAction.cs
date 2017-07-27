using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    /// <summary>
    /// Base class for Actions of specific type
    /// </summary>
    [XmlType("BaseAction")]
    public abstract class IeBaseAction
    {
        // Not used, but we keep it for now
        [XmlElement(IsNullable = false)]
        public string Name { get; set; }
    }

}