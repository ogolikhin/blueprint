﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ServiceLibrary.Models.Workflow
{
    [XmlType("APC")]
    public class XmlPropertyChangeAction : XmlAction
    {
        [XmlIgnore]
        public override ActionTypes ActionType => ActionTypes.PropertyChange;

        [XmlElement("PID")]
        public int PropertyTypeId { get; set; }

        [XmlElement("PV", IsNullable = false)]
        public string PropertyValue { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("VVS"), XmlArrayItem("VV")]
        public List<string> ValidValues { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("UGS"), XmlArrayItem("UG")]
        public List<XmlUserGroup> UsersGroups { get; set; }

        [XmlElement("CUID")]
        public int? CurrentUserId { get; set; }
        public bool ShouldSerializeCurrentUserId() { return CurrentUserId.HasValue; }
    }
}