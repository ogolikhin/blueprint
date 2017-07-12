using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    [XmlType("PropertyChangeAction")]
    public class IePropertyChangeAction : IeBaseAction
    {
        #region Properties

        [XmlElement(IsNullable = false)]
        public string PropertyName { get; set; }

        [XmlElement(IsNullable = false)]
        public string PropertyValue { get; set; }

        // Used for User properties and indicates that PropertyValue contains the group name.
        [XmlElement]
        public bool? IsGroup { get; set; }
        public bool ShouldSerializeIsGroup() { return IsGroup.HasValue; }

        #endregion 
    }

}