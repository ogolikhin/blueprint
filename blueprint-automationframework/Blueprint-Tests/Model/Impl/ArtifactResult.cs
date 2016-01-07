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
}
