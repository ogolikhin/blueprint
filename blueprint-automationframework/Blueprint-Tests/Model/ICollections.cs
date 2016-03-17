using System.Collections.Generic;
using Model.OpenApiModel;

namespace Model
{
    public interface ICollections : IArtifactBase
    {
        IAuthorHistory AuthorHistory { get; set; }
        List<ICollectionFolder> CollectionFolders { get; }
        List<ICollection> Collections { get; }
    }
}
