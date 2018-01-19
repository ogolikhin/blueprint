using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ServiceLibrary.Models.Collection
{
    [XmlRoot("ArtifactListSettingsXml")]
    [XmlType("ArtifactListSettingsXml")]
    public class ArtifactListSettingsXml
    {
        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Columns"), XmlArrayItem("Columns")]
        public List<ArtifactListColumn> Columns { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Filters"), XmlArrayItem("Filters")]
        public List<ArtifactListFilter> Filters { get; set; }

        public static ArtifactListSettingsXml ConvertFromJsonModel(ArtifactListSettings artifactListSettings)
        {
            return new ArtifactListSettingsXml
            {
                Columns = artifactListSettings.Columns.ToList(),
                Filters = artifactListSettings.Filters.ToList()
            };
        }
    }
}
