using Newtonsoft.Json;

namespace ArtifactStore.Models
{
    public class UserGroup
    {
        [JsonProperty]
        public int Id { get; set; }
        [JsonProperty]
        public bool IsGroup { get; set; }
}
}