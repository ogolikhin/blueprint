using System.Collections.Generic;
using Newtonsoft.Json;

namespace Model.Impl
{
    [JsonObject]
    public class QueryResult<T>
    {
        public int Total { get; set; }
        public List<T> Items { get; set; }
    }
}
