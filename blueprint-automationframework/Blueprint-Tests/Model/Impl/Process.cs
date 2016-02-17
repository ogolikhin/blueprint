using System.Collections.Generic;
using Newtonsoft.Json;
using Utilities;

namespace Model.Impl
{
    public class Process: IProcess
    {
        public const string DefaulPreconditionName = "Precondition";
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
        public int BaseItemTypePredefined { get; set; }
        public int VersionId { get; set; }
        public string Description { get; set; }
        public ProcessType Type { get; set; }
        public string RawData { get; set; }
        public int? LockedByUserId { get; set; }
        public int? ArtifactInfoParentId { get; set; }
        public int Permissions { get; set; }
        public int ArtifactDisplayId { get; set; }
        public byte[] Thumbnail { get; set; }
        [JsonConverter(typeof(Deserialization.ConcreteConverter<ProcessShape[]>))]
        public IProcessShape[] Shapes { get; set; }
        [JsonConverter(typeof(Deserialization.ConcreteConverter<ProcessLink[]>))]
        public IProcessLink[] Links { get; set; }
        [JsonConverter(typeof(Deserialization.ConcreteConverter<ArtifactReference[]>))]
        public IArtifactReference[] ArtifactPathLinks { get; set; }
        [JsonConverter(typeof(Deserialization.ConcreteConverter<ProcessShape[]>))]
        public IDictionary<string, IPropertyValueInformation> PropertyValues { get; }
    }

    public class ProcessShape: IProcessShape
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public ProcessShapeType ShapeType { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public class ProcessLink: IProcessLink
    {
        public int SourceId { get; set; }
        public int DestinationId { get; set; }
        public double Orderindex { get; set; }
        public string Label { get; set; }
    }

    public class ArtifactReference : IArtifactReference
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
        public PropertyTypePredefined PropertyTypePredefined { get; set; }
        public int? TypeId { get; set; }
        public bool IsVirtual { get; set; }
        public object Value { get; set; }
    }
}
