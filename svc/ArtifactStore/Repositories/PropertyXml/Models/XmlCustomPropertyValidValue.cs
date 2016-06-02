﻿using System.Xml.Serialization;

namespace ArtifactStore.Repositories.PropertyXml.Models
{
    // Originates from Raptor solution with some modifications
    [XmlType]
    public class XmlCustomPropertyValidValue
    {
        [XmlAttribute(AttributeName = "Id")]
        public string LookupListItemId { get; set; }

        [XmlAttribute(AttributeName = "S")]
        public string Selected { get; set; }

        [XmlAttribute(AttributeName = "V")]
        public string Value { get; set; }

        [XmlAttribute(AttributeName = "O")]
        public string OrderIndex { get; set; }

        [XmlAttribute(AttributeName = "SId")]
        public string StandardLookupListItemId { get; set; }
    }
}
