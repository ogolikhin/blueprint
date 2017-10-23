using System.Xml.Serialization;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Models.Workflow
{
    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
    /// <summary>
    /// Generate Action
    /// </summary>
    [XmlType("GenerateAction")]
    public class IeGenerateAction : IeBaseAction
    {
        [XmlIgnore]
        public override ActionTypes ActionType => ActionTypes.Generate;

        [XmlElement(IsNullable = false)]

        public GenerateActionTypes GenerateActionType { get; set; }

        // Used only for GenerateActionType = Children
        [XmlElement]
        public int? ChildCount { get; set; }
        public bool ShouldSerializeChildCount() { return ChildCount.HasValue; }

        // Used only for GenerateActionType = Children
        [XmlElement(IsNullable = false)]
        public string ArtifactType { get; set; }

        // Used only for GenerateActionType = Children
        // Optional, not used for the import, will be used for the update
        [XmlElement]
        public int? ArtifactTypeId { get; set; }
        public bool ShouldSerializeArtifactTypeId() { return ArtifactTypeId.HasValue; }

        #region Generated and modified Equals and GetHashCode methods

        protected bool Equals(IeGenerateAction other)
        {
            return base.Equals(other) && GenerateActionType == other.GenerateActionType && ChildCount.GetValueOrDefault() == other.ChildCount.GetValueOrDefault() && string.Equals(ArtifactType, other.ArtifactType) && ArtifactTypeId.GetValueOrDefault() == other.ArtifactTypeId.GetValueOrDefault();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IeGenerateAction)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)GenerateActionType;
                hashCode = (hashCode * 397) ^ ChildCount.GetHashCode();
                hashCode = (hashCode * 397) ^ (ArtifactType != null ? ArtifactType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ArtifactTypeId.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }
}