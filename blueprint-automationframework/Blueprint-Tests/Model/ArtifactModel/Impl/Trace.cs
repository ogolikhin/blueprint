using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Model.ArtifactModel.Impl
{
    public class Relationships
    {
        public List<Trace> ManualTraces { get; } = new List<Trace>();
        public List<Trace> OtherTraces { get; } = new List<Trace>();
    }
    public class Trace
    {
        public int ArtifactId { get; set; }

        public string ArtifactTypePrefix { get; set; }

        public string ArtifactName { get; set; }

        public int ItemId { get; set; }

        public string ItemTypePrefix { get; set; }

        public string ItemName { get; set; }

        public int ProjectId { get; set; }

        public string ProjectName { get; set; }

        [JsonProperty("TraceDirection")]
        public TraceDirection Direction { get; set; }

        public TraceTypes TraceType { get; set; }

        public bool Suspect { get; set; }

        public bool HasAccess { get; set; }

        public int PrimitiveItemTypePredefined { get; set; }
    }

    public enum TraceDirection
    {
        To,
        From,
        Both
    }

    [FlagsAttribute]
    public enum TraceTypes
    {
        None = 0,
        Manual = 2,
        Association = 8,
        ActorInherits = 16,
        DocReference = 32
    }
}
