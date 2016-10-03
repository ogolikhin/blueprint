using System.Collections.Generic;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;

namespace Model.Impl
{
    public abstract class ArtifactTypeBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public abstract string Prefix { get; set; }
    }

    public class OpenApiArtifactType : ArtifactTypeBase
    {
        public string Description { get; set; }
        public BaseArtifactType BaseArtifactType { get; set; }
        public List<OpenApiPropertyType> PropertyTypes { get; } = new List<OpenApiPropertyType>();
        public override string Prefix { get; set; }
    }

    public class NovaArtifactType : ArtifactTypeBase
    {
        public int ProjectId { get; set; }
        public int VersionId { get; set; }
        public override string Prefix { get; set; }
        public int? InstanceItemTypeId { get; set; }
        public ItemTypePredefined PredefinedType { get; set; }
        public int? IconImageId { get; set; }
        public bool UsedInThisProject { get; set; }
        public List<int> CustomPropertyTypeIds { get; } = new List<int>();
    }
}
