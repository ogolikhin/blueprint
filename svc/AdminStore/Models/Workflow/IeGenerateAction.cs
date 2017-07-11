using System;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    /// <summary>
    /// Generate Action 
    /// </summary>
    [Serializable()]
    [XmlType("GenerateAction")]
    public class IeGenerateAction :IeBaseAction
    {
        [XmlElement("Childs")]
        public int ChildCount { get; set; }

        [XmlElement("ArtifactType")]
        public string ArtifactTypeName { get; set; }
    }
}