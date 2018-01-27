using System.Collections.Generic;
using System.Linq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.ProjectMeta;

namespace ArtifactStore.ArtifactList.Models
{
    public class ProfileColumnsSettings
    {
        public IEnumerable<ProfileColumn> Items { get; set; }

        public bool PropertyTypeIdMatches(int propertyTypeId)
        {
            if (propertyTypeId < 1 || Items.IsEmpty())
            {
                return false;
            }

            return Items.Any(item => item.PropertyTypeId == propertyTypeId);
        }

        public bool PredefinedMatches(PropertyTypePredefined predefined)
        {
            return !Items.IsEmpty() && Items.Any(item => item.Predefined == predefined);
        }
    }
}
