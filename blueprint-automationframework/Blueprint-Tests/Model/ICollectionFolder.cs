using System.Collections.Generic;
using Model.OpenApiModel;

namespace Model
{
    public interface ICollectionFolder : IArtifactBase
    {
        IAuthorHistory AuthorHistory { get; set; }
        List<ICollectionFolder> CollectionFolders { get; }
        List<IArtifact> Contents { get; }
    }
}
