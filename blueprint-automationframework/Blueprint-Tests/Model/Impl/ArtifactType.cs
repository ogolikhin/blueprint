using Newtonsoft.Json;
using Model.OpenApiModel;

namespace Model.Impl
{
    public class ArtifactType : IArtifactType
    {
        #region Properties
        [JsonProperty("Id")]
        public int Id { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("Prefix")]
        public string Prefix { get; set; }

        [JsonProperty("BaseArtifactType")]
        public BaseArtifactType BaseArtifactType { get; set; }

        #endregion Properties
        public ArtifactType()
        { }
    }
}
