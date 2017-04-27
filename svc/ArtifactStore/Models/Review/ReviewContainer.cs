﻿using Newtonsoft.Json;

namespace ArtifactStore.Models.Review
{
    public class ReviewContainer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        public int TotalArtifacts { get; set; }

        public ReviewType ReviewType { get; set; }

        public ReviewSourceType SourceType { get; set; }

        public ReviewSource Source { get; set; }

        public ReviewStatus Status { get; set; }

        public ReviewArtifactsStatus ArtifactsStatus { get; set; }
    }
}