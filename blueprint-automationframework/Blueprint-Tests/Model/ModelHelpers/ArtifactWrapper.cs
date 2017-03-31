using System;
using System.Collections.Generic;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Newtonsoft.Json;
using NUnit.Framework;
using Utilities;
using Utilities.Factories;

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
        /// <param name="newParentId">The ID of the new parent where this artifact will be copied to.</param>
        /// <param name="orderIndex">(optional) The order index (relative to other artifacts) where this artifact should be copied to.
        ///     By default the artifact is copied to the end (after the last artifact).</param>
        /// <returns>A list of artifacts that were deleted.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Tuple<CopyNovaArtifactResultSet, List<ArtifactWrapper>> CopyTo(IUser user, int newParentId, double? orderIndex = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            // TODO: Refactor ArtifactStore.CopyArtifact to not be static...
            var copyResult = Model.Impl.ArtifactStore.CopyArtifact(ArtifactStore.Address, Artifact.Id, newParentId, user, orderIndex);
            var response = new Tuple<CopyNovaArtifactResultSet, List<ArtifactWrapper>>(copyResult, new List<ArtifactWrapper>());

            if (copyResult?.Artifact != null)
            {
                var wrappedArtifact = new ArtifactWrapper(copyResult.Artifact, ArtifactStore, SvcShared, user);
                response.Item2.Add(wrappedArtifact);
            }

            // Search for and wrap all children that were copied also.
            if (copyResult?.CopiedArtifactsCount > 1)
            {
                // TODO: Move this into a private function.
                Assert.NotNull(copyResult?.Artifact?.ProjectId, "The copied artifact's ProjectId was null!");
                var children = ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(copyResult.Artifact.ProjectId.Value, Artifact.Id, user);

                foreach (var child in children)
                {
                    var novaArtifact = new NovaArtifactDetails
                    {
                        Id = child.Id,
                        ItemTypeId = child.ItemTypeId,
                        LockedByUser = child.LockedByUser,
                        Name = child.Name,
                        OrderIndex = child.OrderIndex,
                        ParentId = child.ParentId,
                        Permissions = child.Permissions,
                        PredefinedType = child.PredefinedType,
                        Prefix = child.Prefix,
                        ProjectId = child.ProjectId,
                        Version = child.Version
                    };

                    // TODO: Also copy children of children...

                    var wrappedArtifact = new ArtifactWrapper(novaArtifact, ArtifactStore, SvcShared, user);
                    response.Item2.Add(wrappedArtifact);
                }
            }

            return response;
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
        /// <param name="thisArtifact">This should be the artifact that this object is wrapping.</param>
        /// <returns>The updated artifact.</returns>
        public ArtifactWrapper SaveWithNewDescription(IUser user, INovaArtifactBase thisArtifact)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(thisArtifact, nameof(thisArtifact));

            Assert.AreEqual(Artifact.Id, thisArtifact.Id, "The '{0}' parameter isn't this artifact!", nameof(thisArtifact));

            var changes = new NovaArtifactDetails
            {
                Id = thisArtifact.Id,
                ProjectId = thisArtifact.ProjectId,
                Version = thisArtifact.Version,
                Description = "NewDescription_" + RandomGenerator.RandomAlphaNumeric(5)
            };

            var updatedArtifact = ArtifactStore.UpdateArtifact(user, changes);
            var wrappedArtifact = new ArtifactWrapper(updatedArtifact, ArtifactStore, SvcShared, user);

            return wrappedArtifact;
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

        /// <seealso cref="INovaArtifactObservable.NovaArtifactObservers"/>
        public List<INovaArtifactObserver> NovaArtifactObservers
        {
            get { return Artifact.NovaArtifactObservers; }
        }

        /// <seealso cref="INovaArtifactObservable.RegisterObserver(INovaArtifactObserver)"/>
        public void RegisterObserver(INovaArtifactObserver observer)
        {
            Artifact.RegisterObserver(observer);
        }

        /// <seealso cref="INovaArtifactObservable.UnregisterObserver(INovaArtifactObserver)"/>
        public void UnregisterObserver(INovaArtifactObserver observer)
        {
            Artifact.UnregisterObserver(observer);
        }

        /// <seealso cref="INovaArtifactObservable.NotifyArtifactDeleted(IEnumerable{int})"/>
        public void NotifyArtifactDeleted(IEnumerable<int> deletedArtifactIds)
        {
            Artifact.NotifyArtifactDeleted(deletedArtifactIds);
        }

        /// <seealso cref="INovaArtifactObservable.NotifyArtifactPublished(IEnumerable{int})"/>
        public void NotifyArtifactPublished(IEnumerable<int> publishedArtifactIds)
        {
            Artifact.NotifyArtifactPublished(publishedArtifactIds);
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
