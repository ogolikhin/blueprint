using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    [XmlType("S")]
    public class XmlTransitionCanvasSettings
    {
        [XmlElement("PRT")]
        public XmlPortPair XmlPortPair { get; set; }
    }
}