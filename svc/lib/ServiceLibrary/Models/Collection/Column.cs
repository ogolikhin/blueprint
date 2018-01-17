using Newtonsoft.Json;

namespace ServiceLibrary.Models.Collection
{
    public class Column
    {
        public string PropertyName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PropertyTypeId { get; set; }
    }
}