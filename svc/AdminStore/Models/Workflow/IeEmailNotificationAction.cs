using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using AdminStore.Helpers.Workflow;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Models.Workflow
{
    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
    /// <summary>
    /// Email Notification Action
    /// </summary>
    [XmlType("NotificationAction")]
    public class IeEmailNotificationAction : IeBaseAction
    {
        #region Properties

        [XmlIgnore]
        public override ActionTypes ActionType => ActionTypes.EmailNotification;

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Emails"), XmlArrayItem("Email")]
        public List<string> Emails { get; set; }

        // Contains emails, users/groups
        [XmlElement(IsNullable = false)]
        public string PropertyName { get; set; }

        // Optional, not used for the import, will be used for the update
        [XmlElement]
        public int? PropertyId { get; set; }
        public bool ShouldSerializePropertyId() { return PropertyId.HasValue; }

        [XmlElement(IsNullable = false)]
        public string Message { get; set; }

        #endregion

        #region Generated and modified Equals and GetHashCode methods

        protected bool Equals(IeEmailNotificationAction other)
        {
            return base.Equals(other) && WorkflowHelper.CollectionEquals(Emails, other.Emails) && string.Equals(PropertyName, other.PropertyName) && PropertyId.GetValueOrDefault() == other.PropertyId.GetValueOrDefault() && string.Equals(Message, other.Message);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IeEmailNotificationAction) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ (Emails != null ? Emails.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (PropertyName != null ? PropertyName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ PropertyId.GetHashCode();
                hashCode = (hashCode*397) ^ (Message != null ? Message.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion

    }

}