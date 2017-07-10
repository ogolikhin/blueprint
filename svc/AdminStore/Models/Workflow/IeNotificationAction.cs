using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    /// <summary>
    /// Sample Email Notification Action
    /// Note: The properties aren't defined by design/requirements
    /// </summary>
    [Serializable()]
    [XmlType("NotificationAction")]
    public class IeNotificationAction : IeBaseAction
    {
        #region Constructors
        //public IeNotificationAction() { }
        #endregion

        #region Properties

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Groups"), XmlArrayItem("Group")]
        public List<string> GroupNames { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Users"), XmlArrayItem("User")]
        public List<string> UserNames { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Emails"), XmlArrayItem("Email")]
        public List<string> Emails { get; set; }

        [XmlElement("PropertyTarget")]
        public string PropertyTargetName;

        public string Message { get; set; }

        #endregion

    }

}