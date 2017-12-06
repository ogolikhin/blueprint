using System.Collections.Generic;
using Newtonsoft.Json;
using ServiceLibrary.Models.Enums;

namespace AdminStore.Models.DiagramWorkflow
{
    public class DEmailNotificationAction : DBaseAction
    {
        public override ActionTypes ActionType => ActionTypes.EmailNotification;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string> Emails { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PropertyName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PropertyId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }
    }
}