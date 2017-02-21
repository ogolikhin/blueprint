using System.Collections.Generic;
using Model.ArtifactModel;

namespace Model.Archive.ArtifactModel
{
    public interface ICollections : IArtifactBase
    {
        IAuthorHistory AuthorHistory { get; set; }
        List<ICollectionFolder> CollectionFolders { get; }
        List<ICollection> Collections { get; }
    }
}
