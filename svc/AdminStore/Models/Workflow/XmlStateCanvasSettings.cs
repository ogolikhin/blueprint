using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    [XmlType("S")]
    public class XmlStateCanvasSettings
    {
        [XmlElement("LN", IsNullable = false)]
        public string Location { get; set; }
    }
}