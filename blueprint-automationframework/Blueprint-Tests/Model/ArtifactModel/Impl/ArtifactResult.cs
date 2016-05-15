using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;
using Utilities;
using System.Diagnostics.CodeAnalysis;

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

    public class NovaDiscardArtifactResult
    {
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public List<DiscardArtifactResult> DiscardResults { get; set; }
    }

    public class DeleteArtifactResult
    {
        public int ArtifactId { get; set; }
        public string Message { get; set; }
        public HttpStatusCode ResultCode { get; set; }
    }
}
