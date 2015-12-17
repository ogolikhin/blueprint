using System;
using System.Collections.Generic;
using Utilities.Facades;

namespace Model.Impl
{
    public class Artifact : IArtifact
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProjectId { get; set; }
        public int Version { get; set; }
        public int ParentId { get; set; }
        public string BlueprintUrl { get; set; }
        public int ArtifactTypeId { get; set; }
        public string ArtifactTypeName { get; set; }
        public string BaseArtifactType { get; set; }
        public bool AreTracesReadOnly { get; set; }
        public bool AreAttachmentsReadOnly { get; set; }
        public bool AreDocumentReferencesReadOnly { get; set; }
        public List<IProperty> Properties { get; set; }
        public List<IComment> Comments { get; set; }
        public List<ITrace> Traces { get; set; }
        public List<IAttachment> Attachments { get; set; }
    }
}
