
using Model.OpenApiModel;

namespace Model
{
    public interface IBaseline : IArtifactBase
    {
        IAuthorHistory AuthorHistory { get; set; }
    }
}
