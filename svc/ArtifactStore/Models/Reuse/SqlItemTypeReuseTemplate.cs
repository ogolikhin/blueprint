using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;

namespace ArtifactStore.Models.Reuse
{
    public class SqlItemTypeReuseTemplate
    {
        public int? TemplateId { get; set; }

        public int TypeId { get; set; }

        public ItemTypePredefined TypePredefined { get; set; }

        public bool? TypeAllowReadOnlyOverride { get; set; }

        public ItemTypeReuseTemplateSetting? TypeReadOnlySettings { get; set; }

        public ItemTypeReuseTemplateSetting? TypeSensitivitySettings { get; set; }

        public int PropertyTypeId { get; set; }

        public PropertyTypePredefined PropertyTypePredefined { get; set; }

        public PropertyPrimitiveType PropertyTypePrimitive { get; set; }

        public PropertyTypeReuseTemplateSettings? PropertySettings { get; set; }
    }
}