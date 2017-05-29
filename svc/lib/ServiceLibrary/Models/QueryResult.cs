using System.Collections.Generic;
using Newtonsoft.Json;

namespace ServiceLibrary.Models
{
    [JsonObject]
    public class QueryResult<T>
    {
        public int Total { get; set; }

        public IEnumerable<T> Items { get; set; }
    }
}
