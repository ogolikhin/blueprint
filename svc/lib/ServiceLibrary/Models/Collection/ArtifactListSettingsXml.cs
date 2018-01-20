using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ServiceLibrary.Models.Collection
{
    [XmlRoot("Settings")]
    [XmlType("Settings")]
    public class XmlProfileSettings
    {
        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Columns"), XmlArrayItem("Column")]
        public List<ProfileColumn> Columns { get; set; }
    }
}
