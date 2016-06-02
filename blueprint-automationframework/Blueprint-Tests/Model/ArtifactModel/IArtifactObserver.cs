using System.Collections.Generic;

namespace Model.ArtifactModel
{
    public interface IArtifactObserver
    {
        /// <summary>
        /// Notifies this observer about artifacts that were deleted & published.
        /// </summary>
        /// <param name="deletedArtifactIds">The list of artifact IDs that were deleted.</param>
        void NotifyArtifactDeletion(IEnumerable<int> deletedArtifactIds);
    }

    public interface IArtifactObservable
    {
        List<IArtifactObserver> ArtifactObservers { get; }

        /// <summary>
        /// Registers an observer to be notified about changes to this artifact.
        /// </summary>
        /// <param name="observer">The observer.</param>
        void RegisterObserver(IArtifactObserver observer);

        /// <summary>
        /// Unregisters the observer.
        /// </summary>
        /// <param name="observer">The observer.</param>
        void UnregisterObserver(IArtifactObserver observer);

        /// <summary>
        /// Notifies all registered observers about artifacts that were deleted & published.
        /// </summary>
        /// <param name="deletedArtifactsList">The list of artifacts that were deleted.</param>
        void NotifyArtifactDeletion(List<IArtifactBase> deletedArtifactsList);
    }
}
