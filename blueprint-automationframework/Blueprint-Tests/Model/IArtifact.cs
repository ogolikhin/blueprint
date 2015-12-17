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
        string BlueprintUrl { get; set; }
        int ArtifactTypeId { get; set; }
        string ArtifactTypeName { get; set; }
        string BaseArtifactType { get; set; }
        bool AreTracesReadOnly { get; set; }
        bool AreAttachmentsReadOnly { get; set; }
        bool AreDocumentReferencesReadOnly { get; set; }
        List<IProperty> Properties { get; set; }
        List<IComment> Comments { get; set; }
        List<ITrace> Traces { get; set; }
        List<IAttachment> Attachments { get; set; }
    }
}
