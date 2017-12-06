using Newtonsoft.Json;
using ServiceLibrary.Models.Enums;

namespace AdminStore.Models.DiagramWorkflow
{
    public abstract class DBaseAction
    {
        public abstract ActionTypes ActionType { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
    }
}