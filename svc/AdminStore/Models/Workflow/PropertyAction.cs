using System;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    /// <summary>
    /// Sample Property Change/Update Notification Action
    /// Note: The properties aren't defined by design/requirements
    /// </summary>
    [Serializable()]
    [XmlType("PropertyAction")]
    public class PropertyAction : BaseAction
    {
        #region Constructors
        public PropertyAction() { }
        #endregion

        #region Properties
        [XmlElement("Project")]
        public int ProjectId { get; set; }

        [XmlElement("Artifact")]
        public int ArtifactId { get; set; }

        #endregion 
    }

}