using Model.ArtifactModel.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Model.ArtifactModel.Impl
{
    // Found in:  blueprint/svc/lib/ServiceLibrary/Models/SubArtifact.cs
    public class SubArtifact
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public int ArtifactId { get; set; }
        public int ItemTypeId { get; set; }
        public string DisplayName { get; set; }
        public ItemTypePredefined PredefinedType { get; set; }
        public string Prefix { get; set; }
        public bool HasChildren { get; set; } = false;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<SubArtifact> Children { get; set; }
    }
}
