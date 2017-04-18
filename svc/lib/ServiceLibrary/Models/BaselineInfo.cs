using Newtonsoft.Json;
using System;

namespace ServiceLibrary.Models
{
    public class BaselineInfo
    {
        public int ItemId { get; set; }

        public bool IsSealed { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? UtcTimestamp { get; set; }
    }
}
