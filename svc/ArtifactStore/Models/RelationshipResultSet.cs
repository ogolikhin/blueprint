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
    public enum TraceType
    {
        ParentChild,
        Manual,
        Reuse,
        Other
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
        public int ArtifactId;
        public string ArtifactName;
        public int itemId;
        public string SubartifactName;
        public int ProjectId;
        public string ProjectName;
        public TraceDirection TraceDirection;
        public TraceType TraceType;
        public bool Suspect;
    }
}