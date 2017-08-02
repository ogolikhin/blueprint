using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Models.Workflow
{
    [XmlType("PropertyChangeAction")]
    public class IePropertyChangeAction : IeBaseAction
    {
        #region Properties

        [XmlIgnore]
        public override ActionTypes ActionType => ActionTypes.PropertyChange;

        [XmlElement(IsNullable = false)]
        public string PropertyName { get; set; }

        // Optional, not used for the import, will be used for the update
        [XmlElement]
        public int? PropertyId { get; set; }
        public bool ShouldSerializePropertyId() { return PropertyId.HasValue; }

        [XmlElement(IsNullable = false)]
        public string PropertyValue { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("ValidValues"), XmlArrayItem("ValidValue")]
        public List<IeValidValue> ValidValues { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("UsersGroups"), XmlArrayItem("UserGroup")]
        public List<IeUserGroup> UsersGroups { get; set; }

        // Used for User properties and indicates that PropertyValue contains the group name.
        [XmlElement]
        public bool? IncludeCurrentUser { get; set; }
        public bool ShouldSerializeIncludeCurrentUser() { return IncludeCurrentUser.HasValue; }

        #endregion 
    }

}