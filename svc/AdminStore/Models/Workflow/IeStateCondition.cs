using System.Xml.Serialization;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Models.Workflow
{
    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
    [XmlType("StateCondition")]
    public class IeStateCondition : IeCondition
    {
        [XmlIgnore]
        public override ConditionTypes ConditionType => ConditionTypes.State;

        [XmlElement(IsNullable = false)]
        public string State { get; set; }

        // Optional, not used for the import, will be used for the update
        [XmlElement]
        public int? StateId { get; set; }
        public bool ShouldSerializeStateId() { return StateId.HasValue; }

        #region Generated and modified Equals and GetHashCode methods

        protected bool Equals(IeStateCondition other)
        {
            return base.Equals(other) && string.Equals(State, other.State) && StateId.GetValueOrDefault() == other.StateId.GetValueOrDefault();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IeStateCondition) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ (State != null ? State.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ StateId.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }
}