using System.Collections.Generic;
using Newtonsoft.Json;

namespace ServiceLibrary.Models.Collection
{
    public class Settings
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Filter> Filters { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Column> Columns { get; set; }
    }
}