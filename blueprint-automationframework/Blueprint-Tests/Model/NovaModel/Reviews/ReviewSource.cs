using Newtonsoft.Json;

namespace Model.NovaModel.Reviews
{
    // Found in:  blueprint/svc/ArtifactStore/Models/Review/ReviewSource.cs
    public class ReviewSource
    {
        public int Id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string Prefix { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string Name { get; set; }
    }
}
