using System.Collections.Generic;
using Model.ArtifactModel.Enums;
using Newtonsoft.Json;

namespace Model.ArtifactModel.Impl
{
    // Found in:  blueprint/svc/lib/ServiceLibrary/Models/SubArtifact.cs
    public class SubArtifact
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public int ItemTypeId { get; set; }
        public string DisplayName { get; set; }
        public ItemTypePredefined PredefinedType { get; set; }
        public string Prefix { get; set; }
        public bool HasChildren { get; set; } = false;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")] // Setter is needed for deserialization.
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<SubArtifact> Children { get; set; }
    }
}
