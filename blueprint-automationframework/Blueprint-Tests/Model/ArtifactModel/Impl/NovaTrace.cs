using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Model.ArtifactModel.Impl
{
    public class NovaTrace : ITrace
    {
        #region Inherited from ITrace

        public int ProjectId { get; set; }

        public int ArtifactId { get; set; }

        [JsonProperty("TraceDirection")]
        public TraceDirection Direction { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TraceTypes TraceType { get; set; }

        [JsonProperty("Suspect")]
        public bool IsSuspect { get; set; }

        #endregion Inherited from ITrace

        #region Additional Properties

        public string ArtifactTypePrefix { get; set; }

        public string ArtifactName { get; set; }

        public int ItemId { get; set; }

        public string ItemTypePrefix { get; set; }

        public string ItemName { get; set; }

        public string ProjectName { get; set; }

        public bool HasAccess { get; set; }

        public int PrimitiveItemTypePredefined { get; set; }

        public int ChangeType { get; set; }

        #endregion Additional Properties
    }

    public class Relationships
    {
        public List<NovaTrace> ManualTraces { get; } = new List<NovaTrace>();
        public List<NovaTrace> OtherTraces { get; } = new List<NovaTrace>();
    }

    public class TracePathItem
    {
        public int ItemId { get; set; }
        public int ParentId { get; set; }
        public string ItemName { get; set; }
    }

    public class TraceDetails
    {
        public int ArtifactId { get; set; }
        public string Description { get; set; }
        public List<TracePathItem> PathToProject { get; } = new List<TracePathItem>();
    }

    public enum TraceDirection
    {
        To,
        From,
        TwoWay
    }

    [Flags]
    public enum TraceTypes
    {
        None = 0,
        Manual = 2,
        Association = 8,
        ActorInherits = 16,
        DocReference = 32
    }
}
