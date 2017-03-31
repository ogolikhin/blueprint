using System;
using System.Collections.Generic;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Newtonsoft.Json;
using Utilities;

namespace Model.ModelHelpers
{
    public class ArtifactWrapper : INovaArtifactDetails, INovaArtifactObservable
    {
        public ArtifactState ArtifactState { get; } = new ArtifactState();
        public IArtifactStore ArtifactStore { get; private set; }
        public ISvcShared SvcShared { get; private set; }

        /// <summary>
        /// The artifact that is being wrapped.
        /// </summary>
        [JsonIgnore]
        public INovaArtifactDetails Artifact { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="artifact">The artifact to wrap.</param>
        /// <param name="artifactStore">The ArtifactStore to use for REST calls.</param>
        /// <param name="svcShared">The SvcShared to use for REST calls.</param>
        /// <param name="createdBy">The user who created the artifact.</param>
        public ArtifactWrapper(INovaArtifactDetails artifact, IArtifactStore artifactStore, ISvcShared svcShared, IUser createdBy)
        {
            Artifact = artifact;
            ArtifactStore = artifactStore;
            SvcShared = svcShared;
            ArtifactState.CreatedBy = createdBy;
        }

        /// <summary>
        /// Deletes this artifact.  (You must publish after deleting to make the delete permanent).
        /// </summary>
        /// <param name="user">The user to perform the delete.</param>
        /// <returns>A list of artifacts that were deleted.</returns>
        public List<NovaArtifactResponse> Delete(IUser user)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            var response = ArtifactStore.DeleteArtifact(Artifact.Id, user);

            ArtifactState.IsMarkedForDeletion = true;

            return response;
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
            var response = Model.Impl.ArtifactStore.DiscardArtifacts(ArtifactStore.Address, new List<int> { Artifact.Id }, user);

            ArtifactState.IsDraft = true;
            ArtifactState.IsMarkedForDeletion = false;
            ArtifactState.LockOwner = null;

            return response;
        }

        /// <summary>
        /// Locks this artifact.
        /// </summary>
        /// <param name="user">The user to perform the delete.</param>
        /// <returns>List of LockResultInfo for the locked artifacts.</returns>
        public List<LockResultInfo> Lock(IUser user)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            var response = SvcShared.LockArtifacts(user, new List<int> { Artifact.Id });

            ArtifactState.LockOwner = user;

            return response;
        }

        /// <summary>
        /// Publishes this artifact.
        /// </summary>
        /// <param name="user">The user to perform the publish.</param>
        /// <returns>An object containing a list of artifacts that were published and their projects.</returns>
        public NovaArtifactsAndProjectsResponse Publish(IUser user)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            var response = ArtifactStore.PublishArtifacts(new List<int> { Artifact.Id }, user);

            // If it was marked for deletion, publishing it will make it permanently deleted.
            if (ArtifactState.IsMarkedForDeletion)
            {
                ArtifactState.IsDeleted = true;
            }

            ArtifactState.IsPublished = true;
            ArtifactState.LockOwner = null;

            return response;
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

            var response = ArtifactStore.UpdateArtifact(user, updateArtifact);

            ArtifactState.IsDraft = true;

            return response;
        }

        #region INovaArtifactObservable members

        public List<INovaArtifactObserver> NovaArtifactObservers
        {
            get { return Artifact.NovaArtifactObservers; }
        }

        public void RegisterObserver(INovaArtifactObserver observer)
        {
            Artifact.RegisterObserver(observer);
        }

        public void UnregisterObserver(INovaArtifactObserver observer)
        {
            Artifact.UnregisterObserver(observer);
        }

        public void NotifyArtifactDeleted(List<INovaArtifactBase> deletedArtifactsList)
        {
            Artifact.NotifyArtifactDeleted(deletedArtifactsList);
        }

        public void NotifyArtifactPublished(List<INovaArtifactResponse> publishedArtifactsList)
        {
            Artifact.NotifyArtifactPublished(publishedArtifactsList);
        }

        #endregion INovaArtifactObservable members

        #region INovaArtifactDetails members

        public int Id
        {
            get { return Artifact.Id; }
            set { Artifact.Id = value; }
        }

        public int? ItemTypeId
        {
            get { return Artifact.ItemTypeId; }
            set { Artifact.ItemTypeId = value; }
        }

        public string Name
        {
            get { return Artifact.Name; }
            set { Artifact.Name = value; }
        }

        public int? ParentId
        {
            get { return Artifact.ParentId; }
            set { Artifact.ParentId = value; }
        }

        public int? ProjectId
        {
            get { return Artifact.ProjectId; }
            set { Artifact.ProjectId = value; }
        }

        public int? Version
        {
            get { return Artifact.Version; }
            set { Artifact.Version = value; }
        }

        public List<AttachmentValue> AttachmentValues
        {
            get { return Artifact.AttachmentValues; }
        }

        public Identification CreatedBy
        {
            get { return Artifact.CreatedBy; }
            set { Artifact.CreatedBy = value; }
        }

        public DateTime? CreatedOn
        {
            get { return Artifact.CreatedOn; }
            set { Artifact.CreatedOn = value; }
        }

        public string Description
        {
            get { return Artifact.Description; }
            set { Artifact.Description = value; }
        }

        public string ItemTypeName
        {
            get { return Artifact.ItemTypeName; }
            set { Artifact.ItemTypeName = value; }
        }

        public int? ItemTypeIconId
        {
            get { return Artifact.ItemTypeIconId; }
            set { Artifact.ItemTypeIconId = value; }
        }

        public int ItemTypeVersionId
        {
            get { return Artifact.ItemTypeVersionId; }
            set { Artifact.ItemTypeVersionId = value; }
        }

        public bool? LastSaveInvalid
        {
            get { return Artifact.LastSaveInvalid; }
            set { Artifact.LastSaveInvalid = value; }
        }

        public RolePermissions? Permissions
        {
            get { return Artifact.Permissions; }
            set { Artifact.Permissions = value; }
        }

        public double? OrderIndex
        {
            get { return Artifact.OrderIndex; }
            set { Artifact.OrderIndex = value; }
        }

        public Identification LastEditedBy
        {
            get { return Artifact.LastEditedBy; }
            set { Artifact.LastEditedBy = value; }
        }

        public DateTime? LastEditedOn
        {
            get { return Artifact.LastEditedOn; }
            set { Artifact.LastEditedOn = value; }
        }

        public Identification LockedByUser
        {
            get { return Artifact.LockedByUser; }
            set { Artifact.LockedByUser = value; }
        }

        public DateTime? LockedDateTime
        {
            get { return Artifact.LockedDateTime; }
            set { Artifact.LockedDateTime = value; }
        }

        public string Prefix
        {
            get { return Artifact.Prefix; }
            set { Artifact.Prefix = value; }
        }

        public List<CustomProperty> CustomPropertyValues
        {
            get { return Artifact.CustomPropertyValues; }
        }

        public List<CustomProperty> SpecificPropertyValues
        {
            get { return Artifact.SpecificPropertyValues; }
        }

        public int? PredefinedType
        {
            get { return Artifact.PredefinedType; }
            set { Artifact.PredefinedType = value; }
        }

        public List<NovaTrace> Traces
        {
            get { return Artifact.Traces; }
            set { Artifact.Traces = value; }
        }

        public List<NovaSubArtifact> SubArtifacts
        {
            get { return Artifact.SubArtifacts; }
            set { Artifact.SubArtifacts = value; }
        }

        #endregion INovaArtifactDetails members
    }
}
