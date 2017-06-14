using Newtonsoft.Json;

namespace ArtifactStore.Models.Review
{
    public class AddParticipantsResult
    {
        public int Total { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Successful { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Failed { get; set; }

        
    }
}
