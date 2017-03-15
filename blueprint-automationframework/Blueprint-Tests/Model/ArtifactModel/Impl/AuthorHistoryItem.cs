using Newtonsoft.Json;
using System;

namespace Model.ArtifactModel.Impl
{
    // developer's implementation blueprint/svc/lib/ServiceLibrary/Models/AuthorHistory.cs
    public class AuthorHistoryItem
    {
        public int ItemId { get; set; }

        [JsonProperty("createdBy")]
        public int CreatedByUserId { get; set; }

        public DateTime CreatedOn { get; set; }

        [JsonProperty("lastEditedBy")]
        public int LastEditedByUserId { get; set; }

        public DateTime LastEditedOn { get; set; }
    }
}
