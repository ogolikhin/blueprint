using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Utilities;

namespace Model.Impl
{
    public class Process: IProcess
    {

        public const string DefaultPreconditionName = "Precondition";
        public const string DefaultUserTaskName = "User Task 1";
        public const string DefaultSystemTaskName = "System Task 1";

        public int ProjectId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }
        public uint ConnectionsAndStates { get; set; }
        public double OrderIndex { get; set; }
        public int TypeId { get; set; }
        public string TypePreffix { get; set; }
        public ItemTypePredefined BaseItemTypePredefined { get; set; }
        public int VersionId { get; set; }
        public int? LockedByUserId { get; set; }
        public int? ArtifactInfoParentId { get; set; }
        public int Permissions { get; set; }
        public int ArtifactDisplayId { get; set; }
        public byte[] Thumbnail { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<ProcessShape>>))]
        public List<ProcessShape> Shapes { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<ProcessLink>>))]
        public List<ProcessLink> Links { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<ArtifactPathLink>>))]
        public List<ArtifactPathLink> ArtifactPathLinks { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<Dictionary<string, PropertyValueInformation>>))]
        public Dictionary<string, PropertyValueInformation> PropertyValues { get; set; }


        public Process()
        {
            Shapes = new List<ProcessShape>();
            Links = new List<ProcessLink>();
            ArtifactPathLinks = new List<ArtifactPathLink>();
            PropertyValues = new Dictionary<string, PropertyValueInformation>();
        }

        public void SetShapes(List<ProcessShape> shapes)
        {
            if (Shapes == null)
            {
                Shapes = new List<ProcessShape>();
            }
            Shapes = shapes;
        }

        public void SetLinks(List<ProcessLink> links)
        {
            if (Links == null)
            {
                Links = new List<ProcessLink>();
            }
            Links = links;
        }

        public void SetArtifactPathLinks(List<ArtifactPathLink> artifactPathLinks)
        {
            if (ArtifactPathLinks == null)
            {
                ArtifactPathLinks = new List<ArtifactPathLink>();
            }
            ArtifactPathLinks = artifactPathLinks;
        }

        public void SetPropertyValues(Dictionary<string, PropertyValueInformation> propertyValues)
        {
            if (PropertyValues == null)
            {
                PropertyValues = new Dictionary<string, PropertyValueInformation>();
            }
            PropertyValues = propertyValues;
        }

    }

    public class ProcessShape: IProcessShape
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }
        public int ProjectId { get; set; }
        public string TypePreffix { get; set; }
        public ItemTypePredefined BaseItemTypePredefined { get; set; }
        public string Purpose { get; set; }
        public int? UserTaskId { get; set; }
        public List<string> InputParameters { get; } = new List<string>();
        public List<string> OutputParameters { get; } = new List<string>();
        public Uri AssociatedImageUrl { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<Dictionary<string, PropertyValueInformation>>))]
        public Dictionary<string, PropertyValueInformation> PropertyValues { get; set; }

        public void SetPropertyValues(Dictionary<string, PropertyValueInformation> propertyValues)
        {
            if (PropertyValues == null)
            {
                PropertyValues = new Dictionary<string, PropertyValueInformation>();
            }
            PropertyValues = propertyValues;
        }
    }

    public class ProcessLink: IProcessLink
    {
        public int SourceId { get; set; }
        public int DestinationId { get; set; }
        public double Orderindex { get; set; }
        public string Label { get; set; }
    }

    public class ArtifactPathLink : IArtifactPathLink
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Name { get; set; }
        public string TypePreffix { get; set; }
        public ItemTypePredefined BaseItemTypePredefined { get; set; }
        public string Link { get; set; }
    }

    public class PropertyValueInformation : IPropertyValueInformation
    {
        public string PropertyName { get; set; }
        public PropertyTypePredefined TypePredefined { get; set; }
        public int? TypeId { get; set; }
        public bool IsVirtual { get; set; }
        public object Value { get; set; }
    }
}
