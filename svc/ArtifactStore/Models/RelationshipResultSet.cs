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
        public LinkType LinkType { get; set; }
        public bool IsSuspect { get; set; }
    }
    public class RelationshipResultSet
    {
        public List<Relationship> ManualTraces;
        public List<Relationship> OtherTraces;
    }
    public class Relationship
    {
        public int ArtifactId { get; set; }
        public string ArtifactTypePrefix { get; set; }
        public string ArtifactName { get; set; }
        public int ItemId { get; set; }
        public string ItemTypePrefix { get; set; }
        public string ItemName { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public TraceDirection TraceDirection { get; set; }
        public LinkType TraceType { get; set; }
        public bool Suspect { get; set; }
    }
}