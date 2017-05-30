using System.Collections.Generic;
using Model.ModelHelpers;

namespace Model.ArtifactModel
{
    public interface IArtifactObserver
    {
        /// <summary>
        /// Notifies this observer about artifacts that were deleted and published.
        /// </summary>
        /// <param name="deletedArtifactIds">The list of artifact IDs that were deleted.</param>
        void NotifyArtifactDeleted(IEnumerable<int> deletedArtifactIds);

        /// <summary>
        /// Notifies this observer about artifacts that were discarded.  If the artifact was never published, it is removed from the artifact list.
        /// </summary>
        /// <param name="discardedArtifactIds">The list of artifact IDs that were discarded.</param>
        void NotifyArtifactDiscarded(IEnumerable<int> discardedArtifactIds);

        /// <summary>
        /// Notifies this observer about artifacts that were published.
        /// </summary>
        /// <param name="publishedArtifactIds">The list of artifact IDs that were published.</param>
        void NotifyArtifactPublished(IEnumerable<int> publishedArtifactIds);
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
        void NotifyArtifactDeleted(List<IArtifactBase> deletedArtifactsList);

        /// <summary>
        /// Notifies all registered observers about artifacts that were published.
        /// </summary>
        /// <param name="publishedArtifactsList">The list of artifacts that were published.</param>
        void NotifyArtifactPublished(List<INovaArtifactDetails> publishedArtifactsList);
    }

    public interface INovaArtifactObserver
    {
        /// <summary>
        /// A list of wrapped artifacts to delete in this object's Dispose() method.
        /// </summary>
        List<ArtifactWrapper> WrappedArtifactsToDispose { get; }

        /// <summary>
        /// Notifies this observer about artifacts that were deleted and published.
        /// </summary>
        /// <param name="deletedArtifactIds">The list of artifact IDs that were deleted.</param>
        void NotifyArtifactDeleted(IEnumerable<int> deletedArtifactIds);

        /// <summary>
        /// Notifies this observer about artifacts that were published.
        /// </summary>
        /// <param name="publishedArtifactIds">The list of artifact IDs that were published.</param>
        void NotifyArtifactPublished(IEnumerable<int> publishedArtifactIds);
    }

    public interface INovaArtifactObservable
    {
        /// <summary>
        /// A list of observers to listen for state changes in this artifact.
        /// </summary>
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
        /// <param name="deletedArtifactIds">The list of IDs of artifacts that were deleted.</param>
        void NotifyArtifactDeleted(IEnumerable<int> deletedArtifactIds);

        /// <summary>
        /// Notifies all registered observers about artifacts that were published.
        /// </summary>
        /// <param name="publishedArtifactIds">The list of IDs of artifacts that were published.</param>
        void NotifyArtifactPublished(IEnumerable<int> publishedArtifactIds);
    }
}
