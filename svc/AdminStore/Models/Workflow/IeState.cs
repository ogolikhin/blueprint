using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
    [XmlType("State")]
    public class IeState : IIeWorkflowEntityWithId
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
        public bool? IsInitial { get; set; }

        [XmlAttribute("IsInitial")]
        public bool IsInitialSerializable
        {
            get { return IsInitial.GetValueOrDefault(); }
            set { IsInitial = value; }
        }

        public bool ShouldSerializeIsInitialSerializable()
        {
            return IsInitial.HasValue;
        }
        //========================================================

        #region Generated and modified Equals and GetHashCode methods

        protected bool Equals(IeState other)
        {
            return Id.GetValueOrDefault() == other.Id.GetValueOrDefault() && string.Equals(Name, other.Name) && IsInitial.GetValueOrDefault() == other.IsInitial.GetValueOrDefault();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IeState) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode*397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ IsInitial.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }
}