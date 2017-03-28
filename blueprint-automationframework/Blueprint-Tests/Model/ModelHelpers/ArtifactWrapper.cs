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
        public ArtifactWrapper(T artifact, IArtifactStore artifactStore, ISvcShared svcShared)
        {
            Artifact = artifact;
            ArtifactStore = artifactStore;
            SvcShared = svcShared;
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
        /// Locks this artifact.
        /// </summary>
        /// <param name="user">The user to perform the delete.</param>
        /// <returns>List of LockResultInfo for the locked artifacts.</returns>
        public List<LockResultInfo> Lock(IUser user)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            return SvcShared.LockArtifacts(user, new List<int> { Artifact.Id });
        }
    }
}
