using Model.ArtifactModel;
using System.Collections.Generic;

namespace Model
{
    public interface ICollectionFolder : IArtifactBase
    {
        IAuthorHistory AuthorHistory { get; set; }
        List<ICollectionFolder> CollectionFolders { get; }
        List<IArtifact> Contents { get; }
    }
}
