using Newtonsoft.Json;
using Utilities;

namespace Model.Impl
{
    public class ArtifactResult : IArtifactResult
    {
        [JsonConverter(typeof(Deserialization.ConcreteConverter<Artifact>))]
        public IArtifact Artifact { get; set; }
        public string Message { get; set; }
        public string ResultCode { get; set; }
    }

    public class OpenApiArtifactResult : IOpenApiArtifactResult
    {
        [JsonConverter(typeof(Deserialization.ConcreteConverter<OpenApiArtifact>))]
        public IOpenApiArtifact Artifact { get; set; }
        public string Message { get; set; }
        public string ResultCode { get; set; }
    }
}
