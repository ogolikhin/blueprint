using System;
using System.Collections.Generic;
using System.Linq;
using ServiceLibrary.Models.Enums;
using PropertyTypeInfo = System.Tuple<int, ArtifactStore.Models.PropertyTypePredefined>;

namespace ArtifactStore.Models.Reuse
{
    public class ReuseSensitivityCollector
    {
        public ReuseSensitivityCollector()
        {
            ArtifactModifications = new Dictionary<int, ArtifactModification>();
        }

        public Dictionary<int, ArtifactModification> ArtifactModifications { get; private set; }

        public class ArtifactModification
        {
            public ItemTypeReuseTemplateSetting ArtifactAspects { get; set; }

            private readonly Lazy<HashSet<PropertyTypeInfo>> _modifiedPropertiesHolder = new Lazy<HashSet<PropertyTypeInfo>>(() => new HashSet<PropertyTypeInfo>());

            public void RegisterArtifactPropertyModification(int propertyTypeId, PropertyTypePredefined predefined)
            {
                _modifiedPropertiesHolder.Value.Add(new PropertyTypeInfo(propertyTypeId, predefined));
            }

            public IEnumerable<PropertyTypeInfo> ModifiedPropertyTypes
            {
                get
                {
                    return _modifiedPropertiesHolder.IsValueCreated
                        ? _modifiedPropertiesHolder.Value
                        : Enumerable.Empty<PropertyTypeInfo>();
                }
            }
        }

        //Reuse Sensitivity

        public void RegisterArtifactPropertyModification(int artifactItemId, int propertyTypeId, PropertyTypePredefined predefined)
        {
            var modifications = GetOrCreateArtifactModifications(artifactItemId);

            modifications.RegisterArtifactPropertyModification(propertyTypeId, predefined);
        }

        public void RegisterArtifactModification(int artifactItemId, ItemTypeReuseTemplateSetting setting)
        {
            var modifications = GetOrCreateArtifactModifications(artifactItemId);

            modifications.ArtifactAspects |= setting;
        }

        private ArtifactModification GetOrCreateArtifactModifications(int artifactItemId)
        {
            ArtifactModification modifications;

            if (ArtifactModifications.TryGetValue(artifactItemId, out modifications))
            {
                return modifications;
            }

            modifications = new ArtifactModification();
            ArtifactModifications.Add(artifactItemId, modifications);

            return modifications;
        }
    }
}