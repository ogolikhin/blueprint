using System.Collections.Generic;
using System.Linq;
using Common;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Newtonsoft.Json;
using Utilities;

namespace Model.ModelHelpers
{
    public class ArtifactWrapper<T> : ArtifactStateWrapper<T>, IArtifactObservable where T : IHaveAnId
    {
        public IArtifactStore ArtifactStore { get; private set; }
        public ISvcShared SvcShared { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="artifact">The artifact to wrap.</param>
        /// <param name="artifactStore">The ArtifactStore to use for REST calls.</param>
        /// <param name="svcShared">The SvcShared to use for REST calls.</param>
        /// <param name="createdBy">The user who created the artifact.</param>
        public ArtifactWrapper(T artifact, IArtifactStore artifactStore, ISvcShared svcShared, IUser createdBy)
        {
            Artifact = artifact;
            ArtifactStore = artifactStore;
            SvcShared = svcShared;
            CreatedBy = createdBy;
        }

        #region IArtifactObservable methods

        [JsonIgnore]
        public List<IArtifactObserver> ArtifactObservers { get; private set; }

        /// <seealso cref="RegisterObserver(IArtifactObserver)"/>
        public void RegisterObserver(IArtifactObserver observer)
        {
            if (ArtifactObservers == null)
            {
                ArtifactObservers = new List<IArtifactObserver>();
            }

            ArtifactObservers.Add(observer);
        }

        /// <seealso cref="UnregisterObserver(IArtifactObserver)"/>
        public void UnregisterObserver(IArtifactObserver observer)
        {
            ArtifactObservers?.Remove(observer);
        }

        /// <seealso cref="NotifyArtifactDeleted(List{IArtifactBase})"/>
        public void NotifyArtifactDeleted(List<IArtifactBase> deletedArtifactsList)
        {
            ThrowIf.ArgumentNull(deletedArtifactsList, nameof(deletedArtifactsList));

            // Notify the observers about any artifacts that were deleted as a result of this publish.
            foreach (var deletedArtifact in deletedArtifactsList)
            {
                IEnumerable<int> deletedArtifactIds =
                    from result in ((ArtifactBase)deletedArtifact).DeletedArtifactResults
                    select result.ArtifactId;

                Logger.WriteDebug("*** Notifying observers about deletion of artifact IDs: {0}", string.Join(", ", deletedArtifactIds));
                deletedArtifact.ArtifactObservers?.ForEach(o => o.NotifyArtifactDeleted(deletedArtifactIds));
            }
        }

        /// <seealso cref="NotifyArtifactPublished(List{INovaArtifactResponse})"/>
        public void NotifyArtifactPublished(List<INovaArtifactResponse> publishedArtifactsList)
        {
            ThrowIf.ArgumentNull(publishedArtifactsList, nameof(publishedArtifactsList));

            // Notify the observers about any artifacts that were deleted as a result of this publish.
            IEnumerable<int> publishedArtifactIds =
                from result in publishedArtifactsList
                select result.Id;

            Logger.WriteDebug("*** Notifying observers about publish of artifact IDs: {0}", string.Join(", ", publishedArtifactIds));
            ArtifactObservers?.ForEach(o => o.NotifyArtifactPublished(publishedArtifactIds));
        }

        #endregion IArtifactObservable methods

        /// <summary>
        /// Deletes this artifact.  (You must publish after deleting to make the delete permanent).
        /// </summary>
        /// <param name="user">The user to perform the delete.</param>
        /// <returns>A list of artifacts that were deleted.</returns>
        public List<NovaArtifactResponse> Delete(IUser user)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            return ArtifactStore.DeleteArtifact(Artifact.Id, user);
        }

        /// <summary>
        /// Discards all unpublished changes for this artifact.
        /// </summary>
        /// <param name="user">The user to perform the discard.</param>
        /// <returns>An object containing a list of artifacts that were discarded and their projects.</returns>
        public INovaArtifactsAndProjectsResponse Discard(IUser user)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            // TODO: Refactor ArtifactStore.DiscardArtifacts to not be static...
            return Model.Impl.ArtifactStore.DiscardArtifacts(ArtifactStore.Address, new List<int> { Artifact.Id }, user);
        }

        /// <summary>
        /// Locks this artifact.
        /// </summary>
        /// <param name="user">The user to perform the delete.</param>
        /// <returns>List of LockResultInfo for the locked artifacts.</returns>
        public List<LockResultInfo> Lock(IUser user)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            return SvcShared.LockArtifacts(user, new List<int> { Artifact.Id });
        }

        /// <summary>
        /// Publishes this artifact.
        /// </summary>
        /// <param name="user">The user to perform the publish.</param>
        /// <returns>An object containing a list of artifacts that were published and their projects.</returns>
        public NovaArtifactsAndProjectsResponse Publish(IUser user)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            return ArtifactStore.PublishArtifacts(new List<int> { Artifact.Id }, user);
        }

        /// <summary>
        /// Updates the artifact with the properties specified in the updateArtifact.
        /// </summary>
        /// <param name="user">The user to perform the update.</param>
        /// <param name="updateArtifact">The artifact whose non-null properties will be used to update this artifact.</param>
        /// <returns>The updated artifact.</returns>
        public INovaArtifactDetails Update(IUser user, NovaArtifactDetails updateArtifact)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            return ArtifactStore.UpdateArtifact(user, updateArtifact);
        }
    }
}
