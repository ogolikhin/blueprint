using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
    [XmlType("UserGroup")]
    public class IeUserGroup
    {
        // Optional, not used for the import, will be used for the update
        //========================================================
        // To make xml attribute nullable.
        [XmlIgnore]
        public int? Id { get; set; }

        [XmlAttribute("Id")]
        public int IdSerializable
        {
            get { return Id.GetValueOrDefault(); }
            set { Id = value; }
        }

        public bool ShouldSerializeIdSerializable()
        {
            return Id.HasValue;
        }
        //========================================================

        [XmlElement(IsNullable = false)]
        public string Name { get; set; }

        //========================================================
        // To make xml attribute nullable.
        [XmlIgnore]
        public bool? IsGroup { get; set; }

        [XmlAttribute("IsGroup")]
        public bool IsGroupSerializable
        {
            get { return IsGroup.GetValueOrDefault(); }
            set { IsGroup = value; }
        }

        public bool ShouldSerializeIsGroupSerializable()
        {
            return IsGroup.HasValue;
        }
        //========================================================

        //========================================================
        // GroupProjectId or GroupProjectPath can be specified, GroupProjectId has precedence over GroupProjectPath.
        [XmlElement(IsNullable = false)]
        public string GroupProjectPath { get; set; }

        [XmlElement("GroupProjectId")]
        public int? GroupProjectId { get; set; }
        public bool ShouldSerializeGroupProjectId() { return GroupProjectId.HasValue; }
        //========================================================
    }
}