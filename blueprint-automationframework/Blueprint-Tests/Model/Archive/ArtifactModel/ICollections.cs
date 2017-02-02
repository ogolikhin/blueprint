using Model.ArtifactModel;
using System.Collections.Generic;

namespace Model
{
    public interface ICollections : IArtifactBase
    {
        IAuthorHistory AuthorHistory { get; set; }
        List<ICollectionFolder> CollectionFolders { get; }
        List<ICollection> Collections { get; }
    }
}
