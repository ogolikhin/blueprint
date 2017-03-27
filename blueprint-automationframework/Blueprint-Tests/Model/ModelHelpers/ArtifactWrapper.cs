using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Newtonsoft.Json;
using Utilities;

namespace Model.ModelHelpers
{
    public class ArtifactWrapper<T> : ArtifactStateWrapper<T>, IArtifactObservable where T : IArtifactId
    {
        public IArtifactStore ArtifactStore { get; private set; }
        public ISvcShared SvcShared { get; private set; }

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

        public List<NovaArtifactResponse> Delete(IUser user = null)
        {
            user = user ?? LockOwner ?? CreatedBy;

            return ArtifactStore.DeleteArtifact(Artifact.Id, user);
        }

        public List<LockResultInfo> Lock(IUser user = null)
        {
            user = user ?? CreatedBy;

            return SvcShared.LockArtifacts(user, new List<int> { Artifact.Id });
        }
    }
}
