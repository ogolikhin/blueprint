using System.Collections.Generic;
using Newtonsoft.Json;

namespace AdminStore.Models.DiagramWorkflow
{
    public class DWorkflow
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public int NumberOfStates { get; set; }

        public int NumberOfActions { get; set; }

        public bool HasProcessArtifactType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DState> States { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DTransitionEvent> TransitionEvents { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DPropertyChangeEvent> PropertyChangeEvents { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DNewArtifactEvent> NewArtifactEvents { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DProject> Projects { get; set; }
    }
}