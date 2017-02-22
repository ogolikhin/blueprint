using Model.ArtifactModel.Impl.PredefinedProperties;
using System;
using System.Collections.Generic;
using Utilities;


namespace Model.ArtifactModel.Impl
{
    public class NovaCollectionBase : NovaArtifactDetails
    {
        public List<CollectionItem> Artifacts { get; set; }

        /// <summary>
        /// Updates Collection's artifacts.
        /// </summary>
        /// <param name="artifactsIdsToAdd">List of artifact's Id to add to Collection.</param>
        /// <param name="artifactsIdsToRemove">List of artifact's Id to remove from Collection.</param>
        protected void UpdateArtifacts(PropertyTypePredefined collectionBaseType,
            List<int> artifactsIdsToAdd = null, List<int> artifactsIdsToRemove = null)
        {

            ThrowIf.ArgumentNull(collectionBaseType, nameof(collectionBaseType));

            var collectionContentValue = new CollectionContentValue();

            if (artifactsIdsToAdd?.Count > 0)
            {
                collectionContentValue.AddedArtifacts.AddRange(artifactsIdsToAdd);
            }
            if (artifactsIdsToRemove?.Count > 0)
            {
                collectionContentValue.RemovedArtifacts.AddRange(artifactsIdsToRemove);
            }

            var collectionContentProperty = new CustomProperty();
            collectionContentProperty.Name = nameof(collectionBaseType);
            collectionContentProperty.PropertyTypeId = -1;
            collectionContentProperty.PropertyType = collectionBaseType;
            collectionContentProperty.CustomPropertyValue = collectionContentValue;

            if (SpecificPropertyValues.Count == 0)
            {
                SpecificPropertyValues.Add(collectionContentProperty);
            }
            /* Collection object returned from the server has empty SpecificPropertyValues list
            we use SpecificPropertyValues only to update list of artifacts
            */
            else
            {
                SpecificPropertyValues[0] = collectionContentProperty;
            }
        }
    }
}
