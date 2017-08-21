﻿using Newtonsoft.Json;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Workflow;

namespace ArtifactStore.Models.Workflow
{
    public class WorkflowTransition : IWorkflowEvent
    {
        public int WorkflowId { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public WorkflowState FromState { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public WorkflowState ToState { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public WorkflowEventTriggers Triggers { get; } = new WorkflowEventTriggers();
    }

    public class SqlWorkflowTransition
    {
        public int WorkflowId { get; set; }
        public int WorkflowEventId { get; set; }
        public string WorkflowEventName { get; set; }
        public int FromStateId { get; set; }
        public string FromStateName { get; set; }
        public int ToStateId { get; set; }
        public string ToStateName { get; set; }
        
        public string Triggers { get; set; }
    }
}