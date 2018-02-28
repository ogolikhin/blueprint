using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace ServiceLibrary.Models.Workflow
{
    public class WebhookArtifactInfo
    {
        public string Id { get; set; }

        public string EventType { get; set; }

        public string PublisherId { get; set; }

        public WebhookArtifactInfoScope Scope { get; set; }

        public WebhookResource Resource { get; set; }
    }

    public class WebhookArtifactInfoScope
    {
        public string Type { get; set; }
        public int WorkflowId { get; set; }
    }

    public class WebhookResource
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ProjectId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ParentId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ArtifactTypeId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ArtifactTypeName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BaseArtifactType { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<WebhookPropertyInfo> ArtifactPropertyInfo { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public WebhookStateInfo State { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public WebhookStateChangeInfo ChangedState { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Revision { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Version { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BlueprintUrl { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Link { get; set; }
    }

    public class WebhookStateChangeInfo
    {
        public WebhookStateInfo OldValue { get; set; }
        public WebhookStateInfo NewValue { get; set; }
    }

    public class WebhookStateInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int WorkflowId { get; set; }
    }

    public class WebhookPropertyInfo
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PropertyTypeId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string TextOrChoiceValue { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BasePropertyType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? NumberValue { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? DateValue { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For json serialization, the property sometimes needs to be null")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<WebhookUserPropertyValue> UsersAndGroups { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For json serialization, the property sometimes needs to be null")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Choices { get; set; }

    }

    public class WebhookUserPropertyValue
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Department { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int Id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ProjectId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }
    }
}
