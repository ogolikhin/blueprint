using Newtonsoft.Json;

namespace ServiceLibrary.Models.Collection
{
    public class PropertyInfo
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PropertyTypeId { get; set; }

        public string Value { get; set; }
    }
}