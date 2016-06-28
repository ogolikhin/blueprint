using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArtifactStore.Models
{
    public enum TraceDirection
    {
        To,
        From,
        ToWay
    }
    public enum TraceType
    {
        ParentChild,
        Manual,
        Reuse,
        Other
    }
    public class RelationshipResultSet
    {
        List<Relationship> Relationships;
    }
    public class Relationship
    {
        public int ArtifactId;
        public string ArtifactName;
        public int? SubartifactId;
        public string SubartifactName;
        public int ProjectId;
        public string ProjectName;
        public TraceDirection TraceDirection;
        public TraceType TraceType;
        public bool Suspect;
    }
}