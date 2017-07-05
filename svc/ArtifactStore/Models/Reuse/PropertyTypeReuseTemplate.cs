using ServiceLibrary.Models.Enums;

namespace ArtifactStore.Models.Reuse
{
    public class PropertyTypeReuseTemplate
    {
        public int ItemTypeReuseTemplateId { get; set; }

        public int PropertyTypeId { get; set; }

        public PropertyTypePredefined PropertyTypePredefined { get; set; }

        public PropertyTypeReuseTemplateSettings Settings { get; set; }
    }
}
