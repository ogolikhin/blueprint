using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ArtifactStore.ArtifactList.Models.Xml
{
    [XmlRoot("Settings")]
    [XmlType("Settings")]
    public class XmlProfileSettings
    {
        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Columns"), XmlArrayItem("Column")]
        public List<XmlProfileColumn> Columns { get; set; }
    }
}
