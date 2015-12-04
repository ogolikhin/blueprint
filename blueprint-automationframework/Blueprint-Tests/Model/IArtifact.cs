using System.Collections.Generic;

namespace Model
{
    public interface IArtifact
    {
        IAuthorHistory AuthorHistory { get; set; }
        List<IArtifact> ChildArtifacts { get; }
        List<IDocument> DocumentReferences { get; }
        List<IFile> FileAttachments { get; }
        List<IRelationship> Relationships { get; }
    }
}
