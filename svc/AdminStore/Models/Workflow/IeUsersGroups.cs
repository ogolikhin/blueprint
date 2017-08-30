using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using AdminStore.Helpers.Workflow;

namespace AdminStore.Models.Workflow
{
    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
    [XmlType("UsersGroups")]
    public class IeUsersGroups
    {
        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlElement("UserGroup", IsNullable = false)]
        public List<IeUserGroup> UsersGroups { get; set; }

        //========================================================

        // To make xml attribute nullable.
        [XmlIgnore]
        public bool? IncludeCurrentUser { get; set; }

        [XmlAttribute("IncludeCurrentUser")]
        public bool IncludeCurrentUserSerializable
        {
            get { return IncludeCurrentUser.GetValueOrDefault(); }
            set { IncludeCurrentUser = value; }
        }

        public bool ShouldSerializeIncludeCurrentUserSerializable()
        {
            return IncludeCurrentUser.HasValue;
        }
        //========================================================

        #region Generated and modified Equals and GetHashCode methods

        protected bool Equals(IeUsersGroups other)
        {
            return WorkflowHelper.CollectionEquals(UsersGroups, other.UsersGroups) && IncludeCurrentUser == other.IncludeCurrentUser;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IeUsersGroups) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((UsersGroups != null ? UsersGroups.GetHashCode() : 0)*397) ^ IncludeCurrentUser.GetHashCode();
            }
        }

        #endregion

    }
}