using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using AdminStore.Helpers.Workflow;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Models.Workflow
{
    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
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

        // To specify an empty choice property value use PropertyValue property with the empty string.
        // An empty list is treated as not specified.
        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("ValidValues"), XmlArrayItem("ValidValue")]
        public List<IeValidValue> ValidValues { get; set; }

        // To specify an empty user property value use PropertyValue property with the empty string.
        // An empty list is treated as not specified.
        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("UsersGroups"), XmlArrayItem("UserGroup")]
        public List<IeUserGroup> UsersGroups { get; set; }

        [XmlElement]
        public bool? IncludeCurrentUser { get; set; }
        public bool ShouldSerializeIncludeCurrentUser() { return IncludeCurrentUser.HasValue; }

        #endregion 

        #region Generated and modified Equals and GetHashCode methods

        protected bool Equals(IePropertyChangeAction other)
        {
            return base.Equals(other) && string.Equals(PropertyName, other.PropertyName) && PropertyId.GetValueOrDefault() == other.PropertyId.GetValueOrDefault() && string.Equals(PropertyValue, other.PropertyValue) && WorkflowHelper.CollectionEquals(ValidValues, other.ValidValues) && WorkflowHelper.CollectionEquals(UsersGroups, other.UsersGroups) && IncludeCurrentUser.GetValueOrDefault() == other.IncludeCurrentUser.GetValueOrDefault();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IePropertyChangeAction) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ (PropertyName != null ? PropertyName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ PropertyId.GetHashCode();
                hashCode = (hashCode*397) ^ (PropertyValue != null ? PropertyValue.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ValidValues != null ? ValidValues.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (UsersGroups != null ? UsersGroups.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ IncludeCurrentUser.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }

}