using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Model.Impl;
using Newtonsoft.Json.Converters;
using System.Xml.Serialization;
using Utilities;

namespace Model.ArtifactModel.Impl
{
    public class NovaTrace : ITrace
    {
        #region Inherited from ITrace

        public int ProjectId { get; set; }

        public int ArtifactId { get; set; }

        [JsonProperty("TraceDirection")]
        //[JsonConverter(typeof(StringEnumConverter))]
        public TraceDirection Direction { get; set; }

        //[JsonConverter(typeof(StringEnumConverter))]
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

        public ArtifactUpdateChangeType ChangeType { get; set; }

        #endregion Additional Properties

        public NovaTrace()
        { }
        public NovaTrace (IArtifact targetArtifact, TraceDirection direction = TraceDirection.From, bool isSuspect = false,
            ArtifactUpdateChangeType changeType = ArtifactUpdateChangeType.Add)
        {
            ThrowIf.ArgumentNull(targetArtifact, nameof(targetArtifact));
            ArtifactId = targetArtifact.Id;
            ChangeType = changeType;
            Direction = direction;
            IsSuspect = isSuspect;
            ItemId = targetArtifact.Id;
            ProjectId = targetArtifact.ProjectId;
            TraceType = TraceTypes.Manual;
        }
    }

    public class Relationships
    {
        public List<NovaTrace> ManualTraces { get; } = new List<NovaTrace>();
        public List<NovaTrace> OtherTraces { get; } = new List<NovaTrace>();
        public bool CanEdit { get; set; }
        public int RevisionId { get; set; }
    }

    public class TracePathItem
    {
        public int? ItemId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends ParentId, even if it's null.
        public int? ParentId { get; set; }
        public string ItemName { get; set; }
    }

    public class TraceDetails
    {
        public int ArtifactId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends Description, even if it's null.
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
