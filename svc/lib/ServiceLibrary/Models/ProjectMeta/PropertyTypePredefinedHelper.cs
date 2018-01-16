using System.Collections.Generic;

namespace ServiceLibrary.Models.ProjectMeta
{
    public static class PropertyTypePredefinedHelper
    {
        public static Dictionary<PropertyTypePredefined, int> PropertyTypePredefineds()
        {
            var fakeDictionary = new Dictionary<PropertyTypePredefined, int>
            {
                { PropertyTypePredefined.ArtifactType, -1 },
                { PropertyTypePredefined.ID, -2 }
            };

            return fakeDictionary;
        }
    }
}