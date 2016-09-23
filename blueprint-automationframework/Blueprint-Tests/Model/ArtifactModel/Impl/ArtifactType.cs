using System.Collections.Generic;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;

namespace Model.Impl
{
    public class ArtifactTypeBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Prefix { get; set; }
    }

    public class OpenApiArtifactType : ArtifactTypeBase
    {
        public string Description { get; set; }
        public BaseArtifactType BaseArtifactType { get; set; }
        public List<OpenApiPropertyType> PropertyTypes { get; } = new List<OpenApiPropertyType>();
    }

    public class NovaArtifactType : ArtifactTypeBase
    {
        public BaseArtifactType BaseType { get; set; }

        public int ProjectId { get; set; }
        public int VersionId { get; set; }
        public int? InstanceItemTypeId { get; set; }
        public int? IconImageId { get; set; }
        public bool UsedInThisProject { get; set; }
        public List<int> CustomPropertyTypeIds { get; } = new List<int>();
    }
}
