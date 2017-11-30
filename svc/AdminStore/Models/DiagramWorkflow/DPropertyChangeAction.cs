using System.Collections.Generic;
using Newtonsoft.Json;
using ServiceLibrary.Models.Enums;

namespace AdminStore.Models.DiagramWorkflow
{
    public class DPropertyChangeAction : DBaseAction
    {
        public override ActionTypes ActionType => ActionTypes.PropertyChange;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PropertyName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PropertyId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PropertyValue { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DValidValue> ValidValues { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DUsersGroups UsersGroups { get; set; }
    }
}