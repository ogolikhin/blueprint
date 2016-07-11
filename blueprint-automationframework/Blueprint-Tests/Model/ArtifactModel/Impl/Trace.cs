using System;
using System.Collections.Generic;

namespace Model.ArtifactModel.Impl
{
    public class Relationships
    {
        List<Trace> ManualTraces { get; set; }
        List<Trace> OtherTraces { get; set; }
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
        public int TraceDirection { get; set; }
        public int TraceType { get; set; }
        public bool Suspect { get; set; }
        public bool HasAccess { get; set; }
        public int PrimitiveItemTypePredefined { get; set; }
    }
}
