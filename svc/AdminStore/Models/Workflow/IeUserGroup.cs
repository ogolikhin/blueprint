using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
    [XmlType("UserGroup")]
    public class IeUserGroup
    {
        // Optional, not used for the import, will be used for the update
        //========================================================
        // To make xml attribute nullable.
        [XmlIgnore]
        public int? Id { get; set; }

        [XmlAttribute("Id")]
        public int IdSerializable
        {
            get { return Id.GetValueOrDefault(); }
            set { Id = value; }
        }

        public bool ShouldSerializeIdSerializable()
        {
            return Id.HasValue;
        }
        //========================================================

        [XmlElement(IsNullable = false)]
        public string Name { get; set; }

        //========================================================
        // To make xml attribute nullable.
        [XmlIgnore]
        public bool? IsGroup { get; set; }

        [XmlAttribute("IsGroup")]
        public bool IsGroupSerializable
        {
            get { return IsGroup.GetValueOrDefault(); }
            set { IsGroup = value; }
        }

        public bool ShouldSerializeIsGroupSerializable()
        {
            return IsGroup.HasValue;
        }
        //========================================================

        //========================================================
        // GroupProjectId or GroupProjectPath can be specified, GroupProjectId has precedence over GroupProjectPath.
        [XmlElement(IsNullable = false)]
        public string GroupProjectPath { get; set; }

        [XmlElement("GroupProjectId")]
        public int? GroupProjectId { get; set; }
        public bool ShouldSerializeGroupProjectId() { return GroupProjectId.HasValue; }
        //========================================================

        #region Generated and modified Equals and GetHashCode methods

        protected bool Equals(IeUserGroup other)
        {
            return Id.GetValueOrDefault() == other.Id.GetValueOrDefault() && string.Equals(Name, other.Name) && IsGroup.GetValueOrDefault() == other.IsGroup.GetValueOrDefault() && string.Equals(GroupProjectPath, other.GroupProjectPath) && GroupProjectId.GetValueOrDefault() == other.GroupProjectId.GetValueOrDefault();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IeUserGroup) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode*397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ IsGroup.GetHashCode();
                hashCode = (hashCode*397) ^ (GroupProjectPath != null ? GroupProjectPath.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ GroupProjectId.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }
}