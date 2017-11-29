using System.Collections.Generic;
using Newtonsoft.Json;

namespace AdminStore.Models.DiagramWorkflow
{
    public class DUsersGroups
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DUserGroup> UsersGroups { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IncludeCurrentUser { get; set; }

    }
}