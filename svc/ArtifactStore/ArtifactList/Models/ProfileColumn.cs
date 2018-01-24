using System;
using System.Collections.Generic;
using System.Linq;
using ArtifactStore.Collections.Models;
using ServiceLibrary.Models.ProjectMeta;

namespace ArtifactStore.ArtifactList.Models
{
    public class ProfileColumn
    {
        public string PropertyName { get; set; }

        public int PropertyTypeId { get; set; }

        public int Predefined { get; set; }

        public bool NameMatches(string search)
        {
            return string.IsNullOrEmpty(search) ||
                   PropertyName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public bool ExistsIn(IEnumerable<PropertyTypeInfo> propertyTypeInfos)
        {
            return Predefined == (int)PropertyTypePredefined.CustomGroup ?
                propertyTypeInfos.Any(info => info.Id == PropertyTypeId) :
                propertyTypeInfos.Any(info => (int)info.Predefined == Predefined);
        }
    }
}
