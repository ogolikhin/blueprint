using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ServiceLibrary.Models.Workflow
{
    [XmlType("UGS")]
    public class XmlUsersGroups
    {
        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlElement("UG", IsNullable = false)]
        public List<XmlUserGroup> UsersGroups { get; set; }

        // ========================================================

        // To make xml attribute nullable.
        [XmlIgnore]
        public bool? IncludeCurrentUser { get; set; }

        [XmlAttribute("ICU")]
        public bool IncludeCurrentUserSerializable
        {
            get { return IncludeCurrentUser.GetValueOrDefault(); }
            set { IncludeCurrentUser = value; }
        }

        public bool ShouldSerializeIncludeCurrentUserSerializable()
        {
            return IncludeCurrentUser.HasValue;
        }
        // ========================================================
    }
}