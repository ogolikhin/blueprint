using System.Net;
using Newtonsoft.Json;
using Utilities;

namespace Model.ArtifactModel.Impl
{
    public class ArtifactResult
    {
        [JsonConverter(typeof(Deserialization.ConcreteConverter<Artifact>))]
        public IArtifact Artifact { get; set; }
        public string Message { get; set; }
        public HttpStatusCode ResultCode { get; set; }
    }

    public class OpenApiArtifactResult
    {
        [JsonConverter(typeof(Deserialization.ConcreteConverter<OpenApiArtifact>))]
        public IOpenApiArtifact Artifact { get; set; }
        public string Message { get; set; }
        public HttpStatusCode ResultCode { get; set; }
    }

    public class PublishArtifactResult
    {
        public int ArtifactId { get; set; }
        public string Message { get; set; }
        public HttpStatusCode ResultCode { get; set; }
        public int ProjectId { get; set; }
    }

    public class DiscardArtifactResult
    {
        public int ArtifactId { get; set; }
        public string Message { get; set; }
        public HttpStatusCode ResultCode { get; set; }
        public int ProjectId { get; set; }
    }

    public class DeleteArtifactResult
    {
        public int ArtifactId { get; set; }
        public string Message { get; set; }
        public HttpStatusCode ResultCode { get; set; }
    }
}
