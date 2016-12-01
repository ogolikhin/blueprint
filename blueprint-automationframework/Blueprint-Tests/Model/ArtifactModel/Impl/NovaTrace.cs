using System;
using System.Collections.Generic;
using Model.ArtifactModel.Enums;
using Newtonsoft.Json;
using Utilities;

namespace Model.ArtifactModel.Impl
{
    // Taken from:  blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/Nova/Models/NovaTrace.cs
    public class NovaTrace : ITrace, INovaTrace
    {
        #region Serialized JSON properties

        public int ArtifactId { get; set; }
        public string ArtifactTypePrefix { get; set; }
        public string ArtifactName { get; set; }
        public int ItemId { get; set; }
        public string ItemTypePrefix { get; set; }
        public string ItemName { get; set; }
        public string ItemLabel { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }

        [JsonProperty("TraceDirection")]
        public TraceDirection Direction { get; set; }
        public TraceType TraceType { get; set; }

        [JsonProperty("Suspect")]
        public bool IsSuspect { get; set; }
        public bool HasAccess { get; set; }
        public int PrimitiveItemTypePredefined { get; set; }
        public ChangeType? ChangeType { get; set; }

        #endregion Serialized JSON properties

        public NovaTrace()
        { }

        public NovaTrace (IArtifact targetArtifact, TraceDirection direction = TraceDirection.From, bool isSuspect = false,
            ChangeType changeType = Enums.ChangeType.Create)
        {
            ThrowIf.ArgumentNull(targetArtifact, nameof(targetArtifact));
            ArtifactId = targetArtifact.Id;
            ChangeType = changeType;
            Direction = direction;
            IsSuspect = isSuspect;
            ItemId = targetArtifact.Id;
            ProjectId = targetArtifact.ProjectId;
            TraceType = TraceType.Manual;
        }
    }

    // Found in:  blueprint/svc/ArtifactStore/Models/RelationshipResultSet.cs
    public class Relationships
    {
        public List<Relationship> ManualTraces { get; } = new List<Relationship>();
        public List<Relationship> OtherTraces { get; } = new List<Relationship>();
        public bool CanEdit { get; set; }
        public int RevisionId { get; set; }
    }

    // Found in:  blueprint/svc/ArtifactStore/Models/RelationshipResultSet.cs
    public class Relationship : ITrace, INovaTrace
    {
        public int ArtifactId { get; set; }
        public string ArtifactTypePrefix { get; set; }
        public string ArtifactName { get; set; }
        public int ItemId { get; set; }
        public string ItemTypePrefix { get; set; }
        public string ItemName { get; set; }
        public string ItemLabel { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }

        [JsonProperty("TraceDirection")]
        public TraceDirection Direction { get; set; }
        public LinkType TraceType { get; set; }

        [JsonProperty("Suspect")]
        public bool IsSuspect { get; set; }
        public bool HasAccess { get; set; } = true;
        public bool ReadOnly { get; set; }
        public int PrimitiveItemTypePredefined { get; set; }
    }

    // Found in:  blueprint/svc/ArtifactStore/Models/RelationshipResultSet.cs  as ItemIdItemNameParentId
    public class TracePathItem
    {
        public int ItemId { get; set; }
        public int VersionProjectId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]   // Dev always sends ParentId, even if it's null.
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public int ItemTypeId { get; set; }
    }

    // Found in:  blueprint/svc/ArtifactStore/Models/RelationshipResultSet.cs  as RelationshipExtendedInfo
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

    // Found in:  blueprint-current/Source/BluePrintSys.RC.Service.Business/Models/Api/TraceType.cs
    /// <summary>
    /// Type of trace.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1714:FlagsEnumsShouldHavePluralNames")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    [Flags]
    public enum TraceType
    {
        /// <summary>
        /// Parent
        /// </summary>
        Parent = 0,
        /// <summary>
        /// Child
        /// </summary>
        Child = 1,
        /// <summary>
        /// Manual
        /// </summary>
        Manual = 2,
        /// <summary>
        /// All other traces including inherits from etc
        /// </summary>
        Other = 4,
        /// <summary>
        /// Reuse
        /// </summary>
        Reuse = 8,

        // -----------------------------------

        // NOTE: The following values are not a part of TraceType in: blueprint-current/Source/BluePrintSys.RC.Service.Business/Models/Api/TraceType.cs

        ActorInherits = 16, // Came from LinkType in:  blueprint-current/Source/BluePrintSys.RC.Data.AccessAPI/Model/LinkType.cs
        DocReference = 32   // Came from LinkType in:  blueprint-current/Source/BluePrintSys.RC.Data.AccessAPI/Model/LinkType.cs
    }

    // Found in:  blueprint-current/Source/BluePrintSys.RC.Data.AccessAPI/Model/LinkType.cs
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1714:FlagsEnumsShouldHavePluralNames")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    [Flags]
    public enum LinkType
    {
        ParentChild = 0x1,
        Manual = 0x2,

        // for now we don't store SubArtifact in the DB. just return it from [TraceabilityMatrix].[GetRawData]
        SubArtifact = 0x4,

        // below there are all OTHER link types
        Association = 0x8,
        [JsonProperty("ActorInheritsFrom")]
        ActorInherits = 0x10,
        DocumentReference = 0x20,

        GlossaryReference = 0x40,
        ShapeConnector = 0x80,

        BaselineReference = 0x100,
        ReviewPackageReference = 0x200,

        Reuse = 0x400
    }
}
