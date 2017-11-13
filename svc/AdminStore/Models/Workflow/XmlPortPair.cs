using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    [XmlType("PRT")]
    public class XmlPortPair
    {
        [XmlElement("FR", IsNullable = false)]
        public int FromPort { get; set; }

        [XmlElement("TO", IsNullable = false)]
        public int ToPort { get; set; }
    }
}