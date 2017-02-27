using System;
using System.Collections.Generic;

namespace Model.ArtifactModel.Impl
{
    /// <summary>
    /// See:  blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/Nova/Models/NovaCollection.cs
    /// </summary>
    public class Collection : NovaCollectionBase
    {
        public string ReviewName { get; set; }

        public DateTime? StartDate { get; set; }

        // IsCreated is a bolean parameter indicating if Rapid Review is created or not for the collection
        public bool IsCreated { get; set; }

        /// <summary>
        /// Updates Collection's artifacts.
        /// </summary>
        /// <param name="artifactsIdsToAdd">(optional) List of artifact's Id to add to Collection.</param>
        /// <param name="artifactsIdsToRemove">(optional) List of artifact's Id to remove from Collection.</param>
        public void UpdateArtifacts(List<int> artifactsIdsToAdd = null, List<int> artifactsIdsToRemove = null)
        {
            UpdateArtifacts(PropertyTypePredefined.CollectionContent, artifactsIdsToAdd, artifactsIdsToRemove);
        }
    }
}
