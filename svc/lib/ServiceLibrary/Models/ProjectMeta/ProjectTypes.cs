using System.Collections.Generic;
using Newtonsoft.Json;

namespace ServiceLibrary.Models.ProjectMeta
{
    [JsonObject]
    public class ProjectTypes
    {
        private List<ItemType> _artifactTypes;
        [JsonProperty]
        public List<ItemType> ArtifactTypes => _artifactTypes ?? (_artifactTypes = new List<ItemType>());

        private List<ItemType> _subArtifactTypes;
        [JsonProperty]
        public List<ItemType> SubArtifactTypes => _subArtifactTypes ?? (_subArtifactTypes = new List<ItemType>());

        private List<PropertyType> _propertyTypes;
        [JsonProperty]
        public List<PropertyType> PropertyTypes => _propertyTypes ?? (_propertyTypes = new List<PropertyType>());
    }
}