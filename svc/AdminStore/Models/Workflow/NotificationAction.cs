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
    public class NotificationAction : BaseAction
    {
        #region Constructors
        public NotificationAction() { }
        #endregion

        #region Properties
        [XmlElement("UserGroup")]
        public int UserGroupId { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Users"), XmlArrayItem("User")]
        public List<int> Users { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Addresses"), XmlArrayItem("Email")]
        public List<string> Addresses { get; set; }

        #endregion

    }

}