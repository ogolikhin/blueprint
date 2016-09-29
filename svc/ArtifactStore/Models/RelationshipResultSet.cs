using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ArtifactStore.Models
{
    public enum TraceDirection
    {
        To = 0,
        From = 1,
        TwoWay = 2
    }
    public enum LinkType
    {
        None = 0,
        ParentChild = 1,
        Manual = 2,
        Subartifact = 4,
        Association = 8, // other
        ActorInheritsFrom = 16, //other
        DocumentReference = 32, //other
        GlossaryReference = 64, //other
        ShapeConnector = 128,  //other
        BaselineReference = 256, //other
        ReviewPackageReference = 512, //other
        Reuse = 1024
    }
    public class LinkInfo
    {
        public int SourceArtifactId { get; set; }
        public int SourceItemId { get; set; }
        public int DestinationArtifactId { get; set; }
        public int DestinationItemId { get; set; }
        public int SourceProjectId { get; set; }
        public int DestinationProjectId { get; set; }
        public LinkType LinkType { get; set; }
        public bool IsSuspect { get; set; }
    }
    [JsonObject]
    public class RelationshipResultSet
    {
        [JsonProperty]
        public List<Relationship> ManualTraces;
        [JsonProperty]
        public List<Relationship> OtherTraces;
        [JsonProperty]
        public bool CanCreate { get; set; }
        [JsonProperty]
        public bool CanDelete { get; set; }
    }
    [JsonObject]
    public class Relationship
    {
        [JsonProperty]
        public int ArtifactId { get; set; }
        [JsonProperty]
        public string ArtifactTypePrefix { get; set; }
        [JsonProperty]
        public string ArtifactName { get; set; }
        [JsonProperty]
        public int ItemId { get; set; }
        [JsonProperty]
        public string ItemTypePrefix { get; set; }
        [JsonProperty]
        public string ItemName { get; set; }
        [JsonProperty]
        public string ItemLabel { get; set; }
        [JsonProperty]
        public int ProjectId { get; set; }
        [JsonProperty]
        public string ProjectName { get; set; }
        [JsonProperty]
        public TraceDirection TraceDirection { get; set; }
        [JsonProperty]
        public LinkType TraceType { get; set; }
        [JsonProperty]
        public bool Suspect { get; set; }
        [JsonProperty]
        public bool HasAccess { get; set; } = true;
        [JsonProperty]
        public int PrimitiveItemTypePredefined { get; set; }
    }

    [JsonObject]
    public class ItemIdItemNameParentId
    {
        [JsonProperty]
        public int ItemId { get; set; }
        [JsonProperty]
        public int ParentId { get; set; }
        [JsonProperty]
        public string ItemName { get; set; }
    }

    [JsonObject]
    public class RelationshipExtendedInfo
    {
        [JsonProperty]
        public int ArtifactId { get; set; }
        [JsonProperty]
        public string Description { get; set; }
        [JsonProperty]
        public IEnumerable<ItemIdItemNameParentId> PathToProject { get; set; }
    }

}