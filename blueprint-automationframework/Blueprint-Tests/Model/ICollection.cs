using System.Collections.Generic;
using Model.OpenApiModel;

namespace Model
{
    public interface ICollection : IArtifactBase
    {
        IAuthorHistory AuthorHistory { get; set; }
        List<IArtifact> Contents { get; }
    }
}
