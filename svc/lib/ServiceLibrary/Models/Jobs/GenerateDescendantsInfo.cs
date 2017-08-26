using System;
using Newtonsoft.Json;

namespace ServiceLibrary.Models.Jobs
{
    [Serializable]
    public class GenerateDescendantsInfo
    {
        [JsonProperty]
        public ItemTypePredefined Predefined { get; set; }

        [JsonProperty]
        public int ArtifacId { get; set; }

        [JsonProperty]
        public int UserId { get; set; }

        [JsonProperty]
        public int ProjectId { get; set; }

        [JsonProperty]
        public int RevisionId { get; set; }

        [JsonProperty]
        public int DesiredArtifactTypeId { get; set; }

        [JsonProperty]
        public int ChildCount { get; set; }
    }
}
