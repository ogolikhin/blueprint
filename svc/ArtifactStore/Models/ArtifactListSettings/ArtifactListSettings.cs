using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace ArtifactStore.Models.ArtifactListSettings
{
    [XmlRoot("ArtifactListSettings")]
    [XmlType("ArtifactListSettings")]
    public class ArtifactListSettings
    {
        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Columns"), XmlArrayItem("Column")]
        public List<Column> Columns { get; set; }
    }
}