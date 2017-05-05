using System.Collections.Generic;
using Newtonsoft.Json;

namespace AdminStore.Models
{
    [JsonObject]
    public class QueryResult<T>
    {
        public int Total { get; set; }
        public IEnumerable<T> Items { get; set; }
    }
}