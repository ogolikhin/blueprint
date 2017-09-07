using System.Collections.Generic;
using Newtonsoft.Json;
using ServiceLibrary.Models.ProjectMeta;

namespace ArtifactStore.Models
{
    public class ArtifactDetailsOptions
    {
        public bool? Types { get; set; }
        public bool? Properties { get; set; }
        public bool? Comments { get; set; }
        public bool? Attachmnents { get; set; }
    }

    [JsonObject]
    public class ArtifactDetailsResultSet
    {
        private List<PropertyType> _propertyTypes;
        [JsonProperty]
        public List<PropertyType> PropertyTypes => _propertyTypes ?? (_propertyTypes = new List<PropertyType>());

        private List<PropertyType> _propertyValues;
        [JsonProperty]
        public List<PropertyType> PropertyValues => _propertyValues ?? (_propertyValues = new List<PropertyType>());

    }




}