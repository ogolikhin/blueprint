﻿using Newtonsoft.Json;
using Utilities;

namespace Model.Impl
{
    public class ArtifactResult : IArtifactResult<IArtifact>
    {
        [JsonConverter(typeof(Deserialization.ConcreteConverter<Artifact>))]
        public IArtifact Artifact { get; set; }
        public string Message { get; set; }
        public string ResultCode { get; set; }
    }

    public class OpenApiArtifactResult : IArtifactResult<IOpenApiArtifact>
    {
        [JsonConverter(typeof(Deserialization.ConcreteConverter<OpenApiArtifact>))]
        public IOpenApiArtifact Artifact { get; set; }
        public string Message { get; set; }
        public string ResultCode { get; set; }
    }
}
