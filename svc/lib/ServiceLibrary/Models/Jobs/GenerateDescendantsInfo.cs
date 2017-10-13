using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ServiceLibrary.Models.Jobs
{
    [Serializable]
    public class GenerateDescendantsInfo
    {
        [JsonProperty]
        public ItemTypePredefined Predefined { get; set; }

        [JsonProperty]
        public int ArtifactId { get; set; }

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

        [JsonProperty]
        public string DesiredArtifactTypeName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        [JsonProperty]
        public int[] AncestorArtifactTypeIds { get; set; }
    }
}
