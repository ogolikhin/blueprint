using System.Xml.Serialization;

namespace AdminStore.Models.Workflow {

    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
    [XmlType("Trigger")]
    public class IeTrigger
    {

        [XmlElement(IsNullable = false)]
        public string Name { get; set; }

        [XmlElement(typeof(IeEmailNotificationAction), ElementName = "EmailNotificationAction")]
        [XmlElement(typeof(IeGenerateAction), ElementName = "GenerateAction")]
        [XmlElement(typeof(IePropertyChangeAction), ElementName = "PropertyChangeAction")]
        public IeBaseAction Action { get; set; }

        [XmlElement(typeof(IeStateCondition), ElementName = "StateCondition")]
        public IeCondition Condition { get; set; }

        #region Generated and modified Equals and GetHashCode methods

        protected bool Equals(IeTrigger other)
        {
            return string.Equals(Name, other.Name) && Equals(Action, other.Action) && Equals(Condition, other.Condition);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IeTrigger)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Action != null ? Action.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Condition != null ? Condition.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}