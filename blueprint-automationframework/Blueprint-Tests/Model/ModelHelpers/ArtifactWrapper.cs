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

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="artifact">The artifact to wrap.</param>
        /// <param name="artifactStore">The ArtifactStore to use for REST calls.</param>
        /// <param name="svcShared">The SvcShared to use for REST calls.</param>
        /// <param name="project">The project where the artifact was created.</param>
        /// <param name="createdBy">The user who created the artifact.</param>
        /// <exception cref="AssertionException">If the Project ID of the artifact is different than the ID of the IProject.</exception>
        public ArtifactWrapper(
            INovaArtifactDetails artifact,
            IArtifactStore artifactStore,
            ISvcShared svcShared,
            IProject project,
            IUser createdBy)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(project, nameof(project));

            Artifact = artifact;
            ArtifactStore = artifactStore;
            SvcShared = svcShared;

            ArtifactState.Project = project;
            ArtifactState.CreatedBy = createdBy;
            ArtifactState.LockOwner = createdBy;

            Assert.AreEqual(artifact.ProjectId, project.Id, "The artifact doesn't belong to the project specified!");
        }

        #endregion Constructors

        /// <summary>
        /// Copies this artifact (and any children) to a new location.
        /// </summary>
        /// <param name="user">The user to perform the copy.</param>
        /// <param name="targetProject">The project where this artifact will be copied to.</param>
        /// <param name="targetParentId">The ID of the parent where this artifact will be copied to.</param>
        /// <param name="orderIndex">(optional) The order index (relative to other artifacts) where this artifact should be copied to.
        ///     By default the artifact is copied to the end (after the last artifact).</param>
        /// <returns>The copy results and a list of artifacts that were copied.  The first item in the list is the main artifact that you copied.
        ///     If the artifact had any children, the copied children will also be in the list.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Tuple<CopyNovaArtifactResultSet, List<ArtifactWrapper>> CopyTo(
            IUser user,
            IProject targetProject,
            int targetParentId,
            double? orderIndex = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            // TODO: Refactor ArtifactStore.CopyArtifact to not be static...
            var copyResult = Model.Impl.ArtifactStore.CopyArtifact(ArtifactStore.Address, Artifact.Id, targetParentId, user, orderIndex);
            var response = new Tuple<CopyNovaArtifactResultSet, List<ArtifactWrapper>>(copyResult, new List<ArtifactWrapper>());

            if (copyResult?.Artifact != null)
            {
                var wrappedArtifact = new ArtifactWrapper(copyResult.Artifact, ArtifactStore, SvcShared, targetProject, user);
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

                    var wrappedArtifact = new ArtifactWrapper(novaArtifact, ArtifactStore, SvcShared, targetProject, user);
                    response.Item2.Add(wrappedArtifact);
                }
            }

            return response;
        }

        /// <summary>
        /// Deletes this artifact.  No lock is required, but it must not be locked by another user.
        /// (If this artifact is published, you must publish after deleting to make the delete permanent).
        /// </summary>
        /// <param name="user">The user to perform the delete.</param>
        /// <returns>A list of artifacts that were deleted.</returns>
        public List<INovaArtifactResponse> Delete(IUser user)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            var response = ArtifactStore.DeleteArtifact(Artifact.Id, user);

            ArtifactState.IsDraft = false;

            // If the artifact was published, you need to publish after delete to permanently delete it.
            if (ArtifactState.IsPublished)
            {
                ArtifactState.IsMarkedForDeletion = true;
            }
            else
            {
                ArtifactState.LockOwner = null;
                ArtifactState.IsDeleted = true;
            }

            return response;
        }

        /// <summary>
        /// Discards all unpublished changes for this artifact.  If the artifact was never published, the discard effectively deletes the artifact.
        /// </summary>
        /// <param name="user">The user to perform the discard.</param>
        /// <returns>An object containing a list of artifacts that were discarded and their projects.</returns>
        public INovaArtifactsAndProjectsResponse Discard(IUser user)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            // TODO: Refactor ArtifactStore.DiscardArtifacts to not be static...
            var response = Model.Impl.ArtifactStore.DiscardArtifacts(ArtifactStore.Address, new List<int> { Artifact.Id }, user);

            ArtifactState.LockOwner = null;
            ArtifactState.IsDraft = false;
            ArtifactState.IsMarkedForDeletion = false;

            // Create & Discard = Deleted.  Published & Discard = not Deleted.
            ArtifactState.IsDeleted = !ArtifactState.IsPublished;

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
        /// Moves this artifact to be under a new parent.  You must lock the artifact before moving.
        /// </summary>
        /// <param name="user">The user to perform the move.</param>
        /// <param name="newParent">The new parent of this artifact.</param>
        /// <param name="orderIndex"> (optional)The order index(relative to other artifacts) where this artifact should be moved to.
        ///     By default the artifact is moved to the end (after the last artifact).</param>
        /// <returns>The artifact that was moved (this artifact).</returns>
        public INovaArtifactDetails MoveArtifact(
            IUser user,
            int newParent,
            double? orderIndex = null)
        {
            var movedArtifact = ArtifactStore.MoveArtifact(user, Id, newParent, orderIndex);

            Artifact = movedArtifact;
            ArtifactState.IsDraft = true;

            return this;
        }

        /// <summary>
        /// Publishes this artifact.  You must lock the artifact before publishing.
        /// </summary>
        /// <param name="user">The user to perform the publish.</param>
        /// <returns>An object containing a list of artifacts that were published and their projects.</returns>
        public NovaArtifactsAndProjectsResponse Publish(IUser user)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            var response = ArtifactStore.PublishArtifacts(new List<int> { Artifact.Id }, user);

            Artifact.Version = response.Artifacts[0].Version;

            // If it was marked for deletion, publishing it will make it permanently deleted.
            if (ArtifactState.IsMarkedForDeletion)
            {
                ArtifactState.IsDeleted = true;
            }

            ArtifactState.LockOwner = null;
            ArtifactState.IsDraft = false;
            ArtifactState.IsPublished = true;
            ArtifactState.IsMarkedForDeletion = false;

            return response;
        }

        /// <summary>
        /// Gets the artifact from ArtifactStore and replaces the current artifact with the properties returned from the server.
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        public void RefreshArtifactFromServer(IUser user)
        {
            Artifact = ArtifactStore.GetArtifactDetails(user, Id);
        }

        /// <summary>
        /// Updates this artifact with a new random Description.  You must lock the artifact before saving.
        /// </summary>
        /// <param name="user">The user to perform the update.</param>
        /// <param name="description">(optional) The new description to save.  By default a random description is generated.</param>
        /// <returns>The updated artifact.</returns>
        public ArtifactWrapper SaveWithNewDescription(IUser user, string description = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            var changes = new NovaArtifactDetails
            {
                Id = Artifact.Id,
                ProjectId = Artifact.ProjectId,
                Description = description ?? "NewDescription_" + RandomGenerator.RandomAlphaNumeric(5)
            };

            var updatedArtifact = ArtifactStore.UpdateArtifact(user, changes);
            CSharpUtilities.ReplaceAllNonNullProperties(changes, Artifact);
            CSharpUtilities.ReplaceAllNonNullProperties(updatedArtifact, Artifact);

            return this;
        }

        /// <summary>
        /// Updates this artifact with the properties specified in the updateArtifact.  You must lock the artifact before updating.
        /// </summary>
        /// <param name="user">The user to perform the update.</param>
        /// <param name="updateArtifact">The artifact whose non-null properties will be used to update this artifact.</param>
        /// <returns>The updated artifact.</returns>
        public INovaArtifactDetails Update(IUser user, INovaArtifactDetails updateArtifact)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            var updatedArtifact = ArtifactStore.UpdateArtifact(user, updateArtifact);
//            var propertiesToNotReplace = new List<string> { "AttachmentValues", "CustomPropertyValues", "SpecificPropertyValues" };
//            CSharpUtilities.ReplaceAllNonNullProperties(updateArtifact, Artifact, propertiesToNotReplace);
//            CSharpUtilities.ReplaceAllNonNullProperties(updatedArtifact, Artifact, propertiesToNotReplace);

            ArtifactState.IsDraft = true;

            return updatedArtifact;
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

        public int? ItemTypeVersionId
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
