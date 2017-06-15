using Newtonsoft.Json;

namespace ArtifactStore.Models.Review
{
    public class AddParticipantsResult
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int NewParticipantCount { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int AlreadyIncludedCount { get; set; }

        
    }
}
