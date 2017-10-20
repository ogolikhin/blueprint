using System.Collections.Generic;
using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Models.Reuse
{
    public class ItemTypeReuseTemplate
    {
        public int ItemTypeReuseTemplateId { get; set; }

        public int ItemTypeId { get; set; }

        public bool? AllowReadOnlyOverride { get; set; }

        public ItemTypeReuseTemplateSetting ReadOnlySettings { get; set; }

        public ItemTypeReuseTemplateSetting SensitivitySettings { get; set; }

        public IDictionary<int, PropertyTypeReuseTemplate> PropertyTypeReuseTemplates { get; } = new Dictionary<int, PropertyTypeReuseTemplate>();

    }
}
