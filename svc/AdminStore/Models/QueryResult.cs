using System.Collections.Generic;
using Newtonsoft.Json;

namespace AdminStore.Models
{
    [JsonObject]
    public class QueryResult<TResult>
    {
        public int Total { get; set; }
        public IEnumerable<TResult> Items { get; set; }
    }
}