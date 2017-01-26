using Model.ArtifactModel.Impl.PredefinedProperties;
using System;
using System.Collections.Generic;

namespace Model.ArtifactModel.Impl
{
    /// <summary>
    /// See:  blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/Nova/Models/NovaCollection.cs
    /// </summary>
    public class Collection : NovaArtifactDetails
    {
        public string ReviewName { get; set; }

        public DateTime? StartDate { get; set; }

        // IsCreated is a bolean parameter indicating if Rapid Review is created or not for the collection
        public bool IsCreated { get; set; }

        public List<CollectionItem> Artifacts { get; set; }

        /// <summary>
        /// Updates Collection's artifacts.
        /// </summary>
        /// <param name="artifactsIdsToAdd">List of artifact's Id to add to Collection.</param>
        /// <param name="artifactsIdsToRemove">List of artifact's Id to remove from Collection.</param>
        public void UpdateArtifacts(List<int> artifactsIdsToAdd = null, List<int> artifactsIdsToRemove = null)
        {

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
            collectionContentProperty.Name = "CollectionContent";
            collectionContentProperty.PropertyTypeId = -1;
            collectionContentProperty.PropertyType = PropertyTypePredefined.Collection;
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
