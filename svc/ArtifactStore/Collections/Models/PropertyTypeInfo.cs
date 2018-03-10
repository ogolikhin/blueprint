using System;
using System.Collections.Generic;
using System.Linq;
using ArtifactStore.ArtifactList.Models;
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

        public bool ExistsIn(ProfileColumns profileColumns)
        {
            return Predefined == PropertyTypePredefined.CustomGroup
                ? profileColumns.Items.Any(info => info.PropertyTypeId == Id)
                : profileColumns.Items.Any(info => info.Predefined == Predefined);
        }

        public bool NameMatches(string search)
        {
            return string.IsNullOrEmpty(search) ||
                   Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
