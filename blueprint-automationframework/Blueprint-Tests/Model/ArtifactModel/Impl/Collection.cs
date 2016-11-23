using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.ArtifactModel.Impl.PredefinedProperties;

namespace Model.ArtifactModel.Impl
{
    public class Collection : NovaArtifactDetails
    {
        public string ReviewName { get; set; }
        public bool IsCreated { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public List<CollectionItem> Artifacts { get; set; }

        public void UpdateArtifacts (List<int> artifactsIdsToAdd = null, List<int> artifactsIdsToRemove = null)
        {
            CollectionContentValue collectionContentValue = new CollectionContentValue();

            if (artifactsIdsToAdd?.Count > 0)
            {
                collectionContentValue.AddedArtifacts.AddRange(artifactsIdsToAdd);
            }
            if (artifactsIdsToRemove?.Count > 0)
            {
                collectionContentValue.AddedArtifacts.AddRange(artifactsIdsToRemove);
            }

            CustomProperty collectionContentProperty = new CustomProperty();
            collectionContentProperty.Name = "CollectionContent";
            collectionContentProperty.PropertyTypeId = -1;
            collectionContentProperty.PropertyType = PropertyTypePredefined.Collection;
            collectionContentProperty.CustomPropertyValue = collectionContentValue;
            SpecificPropertyValues.Add(collectionContentProperty);
        }
    }
}
