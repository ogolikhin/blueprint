using System.Net;

namespace Model.OpenApiModel
{
    public interface IArtifactResult<T>
    { 
        T Artifact { get; set; }
        string Message { get; set; }
        HttpStatusCode ResultCode { get; set; }
    }

    public interface IPublishArtifactResult
    {
        int ArtifactId { get; set; }
        int ProjectId { get; set; }
        string Message { get; set; }
        HttpStatusCode ResultCode { get; set; }
    }

    public interface IDiscardArtifactResult
    {
        int ArtifactId { get; set; }
        int ProjectId { get; set; }
        string Message { get; set; }
        HttpStatusCode ResultCode { get; set; }
    }

    public interface IDeleteArtifactResult
    {
        int ArtifactId { get; set; }
        string Message { get; set; }
        HttpStatusCode ResultCode { get; set; }
    }
}
