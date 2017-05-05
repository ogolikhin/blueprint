using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace AdminStore.Models
{
    [JsonObject]
    public class QueryDataResult<TResult>
    {
        public int Total { get; set; }
        public IEnumerable<TResult> Items { get; set; }
    }
}