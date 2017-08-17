using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using AdminStore.Helpers.Workflow;

namespace AdminStore.Models.Workflow
{
    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
    [XmlType("Project")]
    public class IeProject
    {
        // Id or Path can be specified, Id has precedence over Path.
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
        public string Path { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("ArtifactTypes"), XmlArrayItem("ArtifactType")]
        public List<IeArtifactType> ArtifactTypes { get; set; }

        #region Generated and modified Equals and GetHashCode methods

        protected bool Equals(IeProject other)
        {
            return Id.GetValueOrDefault() == other.Id.GetValueOrDefault() && string.Equals(Path, other.Path) && WorkflowHelper.CollectionEquals(ArtifactTypes, other.ArtifactTypes);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IeProject) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode*397) ^ (Path != null ? Path.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ArtifactTypes != null ? ArtifactTypes.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}