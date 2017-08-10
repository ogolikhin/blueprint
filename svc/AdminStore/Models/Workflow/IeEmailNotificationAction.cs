using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
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

    }

}