using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Utilities;

namespace Model.Impl
{
    public class ArtifactType : IArtifactType
    {
        #region Properties
        [JsonProperty("Id")]
        public int Id { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("Prefix")]
        public string Prefix { get; set; }

        [JsonProperty("BaseArtifactType")]
        public BaseArtifactType BaseArtifactType { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<PropertyType>>))]
        [JsonProperty("PropertyTypes")]
        public List<PropertyType> PropertyTypes { get; set; }

        #endregion Properties

        #region Constructors

        public ArtifactType()
        {
            PropertyTypes = new List<PropertyType>();
        }

        #endregion Constructors
    }
}
