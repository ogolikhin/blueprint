using System.Collections.Generic;
using System.Linq;
using ServiceLibrary.Models.ProjectMeta;

namespace ArtifactStore.Collections.Models
{
    public class PropertyTypeInfo
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public PropertyTypePredefined Predefined { get; set; }

        public PropertyPrimitiveType PrimitiveType { get; set; }

        public bool IsCustom => Predefined == PropertyTypePredefined.CustomGroup;

        public bool PredefinedMatches(IEnumerable<PropertyTypePredefined> predefineds)
        {
            return predefineds != null && predefineds.Contains(Predefined);
        }
    }
}
