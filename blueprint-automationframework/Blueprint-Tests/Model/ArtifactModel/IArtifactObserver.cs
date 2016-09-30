using System.Collections.Generic;

namespace Model.ArtifactModel
{
    public interface IArtifactObserver
    {
        /// <summary>
        /// Notifies this observer about artifacts that were deleted and published.
        /// </summary>
        /// <param name="deletedArtifactIds">The list of artifact IDs that were deleted.</param>
        void NotifyArtifactDeletion(IEnumerable<int> deletedArtifactIds);

        /// <summary>
        /// Notifies this observer about artifacts that were published.
        /// </summary>
        /// <param name="publishedArtifactIds">The list of artifact IDs that were published.</param>
        void NotifyArtifactPublish(IEnumerable<int> publishedArtifactIds);
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
        /// Notifies all registered observers about artifacts that were deleted and published.
        /// </summary>
        /// <param name="deletedArtifactsList">The list of artifacts that were deleted.</param>
        void NotifyArtifactDeletion(List<IArtifactBase> deletedArtifactsList);

        /// <summary>
        /// Notifies all registered observers about artifacts that were published.
        /// </summary>
        /// <param name="publishedArtifactsList">The list of artifacts that were published.</param>
        void NotifyArtifactPublish(List<INovaArtifactResponse> publishedArtifactsList);
    }

    public interface INovaArtifactObserver
    {
        /// <summary>
        /// Notifies this observer about artifacts that were deleted and published.
        /// </summary>
        /// <param name="deletedArtifactIds">The list of artifact IDs that were deleted.</param>
        void NotifyArtifactDeletion(IEnumerable<int> deletedArtifactIds);

        /// <summary>
        /// Notifies this observer about artifacts that were published.
        /// </summary>
        /// <param name="publishedArtifactIds">The list of artifact IDs that were published.</param>
        void NotifyArtifactPublish(IEnumerable<int> publishedArtifactIds);
    }

    public interface INovaArtifactObservable
    {
        List<INovaArtifactObserver> NovaArtifactObservers { get; }

        /// <summary>
        /// Registers an observer to be notified about changes to this artifact.
        /// </summary>
        /// <param name="observer">The observer.</param>
        void RegisterObserver(INovaArtifactObserver observer);

        /// <summary>
        /// Unregisters the observer.
        /// </summary>
        /// <param name="observer">The observer.</param>
        void UnregisterObserver(INovaArtifactObserver observer);

        /// <summary>
        /// Notifies all registered observers about artifacts that were deleted and published.
        /// </summary>
        /// <param name="deletedArtifactsList">The list of artifacts that were deleted.</param>
        void NotifyArtifactDeletion(List<INovaArtifactBase> deletedArtifactsList);

        /// <summary>
        /// Notifies all registered observers about artifacts that were published.
        /// </summary>
        /// <param name="publishedArtifactsList">The list of artifacts that were published.</param>
        void NotifyArtifactPublish(List<INovaArtifactResponse> publishedArtifactsList);
    }
}
