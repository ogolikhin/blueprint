using System;
using System.Collections.Generic;

namespace Model
{
    public interface IArtifact
    {
        int Id { get; set; }
        string Name { get; set; }
        int ProjectId { get; set; }
        int Version { get; set; }
        int ParentId { get; set; }
        Uri BlueprintUrl { get; set; }
        int ArtifactTypeId { get; set; }
        string ArtifactTypeName { get; set; }
        string BaseArtifactType { get; set; }
        bool AreTracesReadOnly { get; set; }
        bool AreAttachmentsReadOnly { get; set; }
        bool AreDocumentReferencesReadOnly { get; set; }
        List<IProperty> Properties { get; }
        List<IComment> Comments { get; }
        List<ITrace> Traces { get; }
        List<IAttachment> Attachments { get; }
        void SetProperties(List<IProperty> aproperty);
    }
}
