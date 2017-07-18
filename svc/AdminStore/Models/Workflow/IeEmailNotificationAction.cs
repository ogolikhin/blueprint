using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    /// <summary>
    /// Email Notification Action
    /// </summary>
    [XmlType("NotificationAction")]
    public class IeEmailNotificationAction : IeBaseAction
    {
        #region Properties

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Emails"), XmlArrayItem("Email")]
        public List<string> Emails { get; set; }

        // Contains emails, users/groups
        [XmlElement(IsNullable = false)]
        public string PropertyName { get; set; }

        [XmlElement(IsNullable = false)]
        public string Message { get; set; }

        #endregion

    }

}