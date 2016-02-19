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

        [JsonConverter(typeof(Deserialization.ConcreteConverter<ProcessShape[]>))]
        public List<IProcessShape> Shapes { get; private set; }

        [JsonConverter(typeof(Deserialization.ConcreteConverter<ProcessLink[]>))]
        public List<IProcessLink> Links { get; private set; }

        [JsonConverter(typeof(Deserialization.ConcreteConverter<ArtifactPathLink[]>))]
        public List<IArtifactPathLink> ArtifactPathLinks { get; private set; }

        [JsonConverter(typeof(Deserialization.ConcreteConverter<ProcessShape[]>))]
        public IDictionary<string, IPropertyValueInformation> PropertyValues { get; private set; }
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
        public IDictionary<string, IPropertyValueInformation> PropertyValues { get; } = new Dictionary<string, IPropertyValueInformation>();
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
        public PropertyTypePredefined PropertyTypePredefined { get; set; }
        public int? TypeId { get; set; }
        public bool IsVirtual { get; set; }
        public object Value { get; set; }
    }
}
