﻿using System.Collections.Generic;
using Model.ArtifactModel;

namespace Model
{
    public interface ICollectionFolder : IArtifactBase
    {
        IAuthorHistory AuthorHistory { get; set; }
        List<ICollectionFolder> CollectionFolders { get; }
        List<IArtifact> Contents { get; }
    }
}
