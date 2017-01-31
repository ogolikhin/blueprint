using Model.ArtifactModel;
using System.Collections.Generic;

namespace Model
{
    public interface ICollection : IArtifactBase
    {
        IAuthorHistory AuthorHistory { get; set; }
        List<IArtifact> Contents { get; }
    }
}
