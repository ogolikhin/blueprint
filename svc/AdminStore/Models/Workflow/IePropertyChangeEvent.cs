using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
    /// <summary>
    /// Property Change Trigger
    /// </summary>
    [XmlType("PropertyChange")]
    public class IePropertyChangeEvent : IeEvent
    {
        [XmlIgnore]
        public override EventTypes EventType => EventTypes.PropertyChange;

        [XmlElement(IsNullable = false)]
        public string PropertyName { get; set; }

        // Optional, not used for the import, will be used for the update
        [XmlElement]
        public int? PropertyId { get; set; }
        public bool ShouldSerializePropertyId() { return PropertyId.HasValue; }

        #region Generated and modified Equals and GetHashCode methods

        protected bool Equals(IePropertyChangeEvent other)
        {
            return base.Equals(other) && string.Equals(PropertyName, other.PropertyName) && PropertyId.GetValueOrDefault() == other.PropertyId.GetValueOrDefault();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IePropertyChangeEvent)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (PropertyName != null ? PropertyName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ PropertyId.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }
}