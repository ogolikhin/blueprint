using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;

namespace AdminStore.Models
{
    [JsonObject]
    public class InstanceItem
    {
        [JsonProperty]
        public int Id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ParentFolderId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty]
        public InstanceItemTypeEnum Type { get; set; }

        
        public bool HasChildren {
            get {
                return this.Type == InstanceItemTypeEnum.Folder;
            }
        }

        [JsonIgnore]
        public bool? IsAccesible { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum InstanceItemTypeEnum
    {
        Folder = 0,
        Project = 1
    }
}