using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ServiceLibrary.Models
{
    [JsonObject]
    public class QueryResult<T>
    {
        public static QueryResult<T> Empty => new QueryResult<T> { Items = Enumerable.Empty<T>(), Total = 0 };

        public int Total { get; set; }

        public IEnumerable<T> Items { get; set; }
    }
}
