using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ServiceLibrary.Models
{
    [JsonObject]
    public class RoleAssignmentQueryResult<T> : QueryResult<T>
    {
        public string ProjectName { get; set; }
    }
}
