using System;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    /// <summary>
    /// Property Change/Update Notification Action
    /// </summary>
    [Serializable()]
    [XmlType("PropertyChangeAction")]
    public class IePropertyChangeAction : IeBaseAction
    {
        #region Properties

        public string PropertyName { get; set; }

        public string PropertyValue { get; set; }

        public ActionDataTypes PropertyValueType { get; set; }

        [XmlElement("Group")]
        public string GroupName { get; set; }

        [XmlElement("User")]
        public string UserName { get; set; }

        #endregion 
    }

}