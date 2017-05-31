using Newtonsoft.Json;
using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Models
{
    [JsonObject]
    public class QuerySingleResult<T>
    {
        public QueryResultCode ResultCode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public T Item { get; set; }
    }
}
