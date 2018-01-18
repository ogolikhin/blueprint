using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace ArtifactStore.Models.ArtifactListSettings
{
    [XmlType("Column")]
    public class Column
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }
    }
}