using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Model.ArtifactModel.Enums;
using Model.Factories;
using Utilities;
using Utilities.Factories;

namespace Model.ModelHelpers
{
    public class ArtifactWrapper : INovaArtifactDetails, INovaArtifactObservable
    {
        public ArtifactState ArtifactState { get; } = new ArtifactState();

        public static IArtifactStore ArtifactStore { get; } = ArtifactStoreFactory.GetArtifactStoreFromTestConfig();
        public static ISvcShared SvcShared { get; } = SvcSharedFactory.GetSvcSharedFromTestConfig();

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
        /// <param name="project">The project where the artifact was created.</param>
        /// <param name="createdBy">The user who created the artifact.</param>
        /// <exception cref="AssertionException">If the Project ID of the artifact is different than the ID of the IProject.</exception>
        public ArtifactWrapper(
            INovaArtifactDetails artifact,
            IProject project,
            IUser createdBy)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(project, nameof(project));

            Artifact = artifact;

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
                var wrappedArtifact = ArtifactWrapperFactory.CreateArtifactWrapper(copyResult.Artifact, targetProject, user);
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
                    var novaArtifact = ArtifactFactory.CreateArtifact((ItemTypePredefined) child.PredefinedType.Value);

                    novaArtifact.Id = child.Id;
                    novaArtifact.ItemTypeId = child.ItemTypeId;
                    novaArtifact.LockedByUser = child.LockedByUser;
                    novaArtifact.Name = child.Name;
                    novaArtifact.OrderIndex = child.OrderIndex;
                    novaArtifact.ParentId = child.ParentId;
                    novaArtifact.Permissions = child.Permissions;
                    novaArtifact.PredefinedType = child.PredefinedType;
                    novaArtifact.Prefix = child.Prefix;
                    novaArtifact.ProjectId = child.ProjectId;
                    novaArtifact.Version = child.Version;

                    // TODO: Also copy children of children...

                    var wrappedArtifact = ArtifactWrapperFactory.CreateArtifactWrapper(novaArtifact, targetProject, user);
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

            UpdateArtifactState(ArtifactOperation.Delete);
            
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

            var response = ArtifactStore.DiscardArtifact(user, Artifact.Id);

            Artifact.LockedByUser = null;
            Artifact.LockedDateTime = null;

            UpdateArtifactState(ArtifactOperation.Discard);

            return response;
        }

        /// <summary>
        /// Discards all unpublished changes for multiple artifacts.  If an artifact was never published, the discard effectively deletes the artifact.
        /// </summary>
        /// <param name="user">The user to perform the discard.</param>
        /// <param name="artifacts">The artifacts to discard.</param>
        /// <returns>An object containing a list of artifacts that were discarded and their projects.</returns>
        public static INovaArtifactsAndProjectsResponse DiscardArtifacts(IUser user, List<ArtifactWrapper> artifacts)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifacts, nameof(artifacts));

            var response = ArtifactStore.DiscardArtifacts(user, artifacts.Select(a => a.Id));

            artifacts.ForEach(a => a.LockedDateTime = null);
            artifacts.ForEach(a => a.UpdateArtifactState(ArtifactOperation.Discard));

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

            Artifact.LockedDateTime = response[0].Info.UtcLockedDateTime;

            UpdateArtifactState(ArtifactOperation.Lock, user);

            return response;
        }

        /// <summary>
        /// Locks multiple artifacts.
        /// </summary>
        /// <param name="user">The user to perform the delete.</param>
        /// <param name="artifacts">The artifacts to lock.</param>
        /// <returns>List of LockResultInfo for the locked artifacts.</returns>
        public static List<LockResultInfo> LockArtifacts(IUser user, List<ArtifactWrapper> artifacts)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifacts, nameof(artifacts));

            var response = SvcShared.LockArtifacts(user, artifacts.Select(a => a.Id));

            foreach (var artifact in artifacts)
            {
                var lockedArtifact = response.Find(a => (a.Result == LockResult.Success) && (a.Info.ArtifactId.Value == artifact.Id));

                if (lockedArtifact != null)
                {
                    artifact.LockedDateTime = lockedArtifact.Info.UtcLockedDateTime;
                    artifact.UpdateArtifactState(ArtifactOperation.Lock, user);
                }
            }

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

            UpdateArtifactState(ArtifactOperation.Move);

            return this;
        }

        /// <summary>
        /// Publishes this artifact.  You must lock the artifact before publishing.
        /// NOTE: This method only updates the Version of the wrapped artifact with the new version returned by the Publish call.  All other
        /// properties are the same as they were before this function was called.  If you need this object to have all of the properties the
        /// same as they are on the server, call RefreshArtifactFromServer().
        /// </summary>
        /// <param name="user">The user to perform the publish.</param>
        /// <returns>An object containing a list of artifacts that were published and their projects.</returns>
        public INovaArtifactsAndProjectsResponse Publish(IUser user)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            var response = ArtifactStore.PublishArtifact(Artifact.Id, user);

            Artifact.Version = response.Artifacts[0].Version;

            UpdateArtifactState(ArtifactOperation.Publish);

            return response;
        }

        /// <summary>
        /// Publishes multiple artifacts.  You must lock the artifacts before publishing.
        /// NOTE: This method only updates the Version of the wrapped artifacts with the new versions returned by the Publish call.  All other
        /// properties are the same as they were before this function was called.  If you need the artifacts to have all of the properties the
        /// same as they are on the server, call RefreshArtifactFromServer().
        /// </summary>
        /// <param name="user">The user to perform the publish.</param>
        /// <param name="artifacts">The artifacts to publish.</param>
        /// <returns>An object containing a list of artifacts that were published and their projects.</returns>
        public static INovaArtifactsAndProjectsResponse PublishArtifacts(IUser user, List<ArtifactWrapper> artifacts)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifacts, nameof(artifacts));

            var response = ArtifactStore.PublishArtifacts(artifacts.Select(a => a.Id), user);

            foreach (var artifact in artifacts)
            {
                var publishedArtifact = response.Artifacts.Find(a => a.Id == artifact.Id);

                if (publishedArtifact != null)
                {
                    artifact.Version = response.Artifacts[0].Version;
                    artifact.UpdateArtifactState(ArtifactOperation.Publish);
                }
            }

            return response;
        }

        /// <summary>
        /// Gets the artifact from ArtifactStore and replaces the current artifact with the properties returned from the server.
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        public virtual void RefreshArtifactFromServer(IUser user)
        {
            Artifact = ArtifactStore.GetArtifactDetails(user, Id);
        }

        /// <summary>
        /// Updates this artifact with a new random Description.  You must lock the artifact before saving.
        /// NOTE: This method only updates the Description of the wrapped artifact with the new random description.  All other properties
        /// are the same as they were before this function was called.  If you need this object to have all of the properties the same as
        /// they are on the server, call RefreshArtifactFromServer().
        /// </summary>
        /// <param name="user">The user to perform the update.</param>
        /// <param name="description">(optional) The new description to save.  By default a random description is generated.</param>
        /// <returns>The result of the update artifact call.</returns>
        public virtual INovaArtifactDetails SaveWithNewDescription(IUser user, string description = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            var changes = ArtifactFactory.CreateArtifact((ItemTypePredefined) Artifact.PredefinedType.Value);
            changes.Id = Artifact.Id;
            changes.ProjectId = Artifact.ProjectId;
            changes.Description = description ?? "NewDescription_" + RandomGenerator.RandomAlphaNumeric(5);

            var updatedArtifact = Update(user, changes);

            Artifact.Description = changes.Description;

            return updatedArtifact;
        }

        /// <summary>
        /// Updates this artifact with the properties specified in the updateArtifact.  You must lock the artifact before updating.
        /// NOTE: This method does not update the wrapped artifact with the properties you updated.  If you need this object to have all
        /// of the properties the same as they are on the server, call RefreshArtifactFromServer().
        /// </summary>
        /// <param name="user">The user to perform the update.</param>
        /// <param name="updateArtifact">The artifact whose non-null properties will be used to update this artifact.</param>
        /// <returns>The result of the update artifact call.</returns>
        public virtual INovaArtifactDetails Update(IUser user, INovaArtifactDetails updateArtifact)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(updateArtifact, nameof(updateArtifact));

            // Hack for TFS bug 3739:  If you send a non-null/empty Name of a UseCase SubArtifact to the Update REST call, it returns 500 Internal Server Error
            // Set Name=null for all SubArtifacts to prevent a 500 error.
            var savedSubArtifactNames = RemoveSubArtifactNamesForBug3739(updateArtifact);   // TODO: Remove this when Bug 3739 is fixed.

            try
            {
                var updatedArtifact = ArtifactStore.UpdateArtifact(user, updateArtifact);

                UpdateArtifactState(ArtifactOperation.Update);

                return updatedArtifact;
            }
            finally
            {
                // Hack for TFS bug 3739: Restore sub-artifact names after the update call.
                RestoreSubArtifactNamesForBug3739(updateArtifact, savedSubArtifactNames);   // TODO: Remove this when Bug 3739 is fixed.
            }
        }

        /// <summary>
        /// Updates the internal artifact state based on the operation performed.
        /// </summary>
        /// <param name="operation">The operation that was performed on the artifact.</param>
        /// <param name="lockOwner">(optional) The lock owner to set.  NOTE: This is only needed if operation = Lock.</param>
        public void UpdateArtifactState(ArtifactOperation operation, IUser lockOwner = null)
        {
            switch (operation)
            {
                case ArtifactOperation.Delete:
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

                    ArtifactState.IsDraft = false;
                    break;

                case ArtifactOperation.Discard:
                    ArtifactState.LockOwner = null;
                    ArtifactState.IsDraft = false;
                    ArtifactState.IsMarkedForDeletion = false;
                    
                    // Create & Discard = Deleted.  Published & Discard = not Deleted.
                    ArtifactState.IsDeleted = !ArtifactState.IsPublished;
                    break;

                case ArtifactOperation.Lock:
                    ThrowIf.ArgumentNull(lockOwner, nameof(lockOwner));
                    ArtifactState.LockOwner = lockOwner;
                    break;

                case ArtifactOperation.Publish:
                    // If it was marked for deletion, publishing it will make it permanently deleted.
                    if (ArtifactState.IsMarkedForDeletion)
                    {
                        ArtifactState.IsDeleted = true;
                    }

                    ArtifactState.LockOwner = null;
                    ArtifactState.IsDraft = false;
                    ArtifactState.IsPublished = true;
                    ArtifactState.IsMarkedForDeletion = false;
                    break;

                case ArtifactOperation.Move:
                case ArtifactOperation.Update:
                    ArtifactState.IsDraft = true;
                    break;

                default:
                    throw new ArgumentException("An invalid ArtifactOperation was passed!", nameof(operation));
            }
        }

        public enum ArtifactOperation
        {
            Copy,
            Delete,
            Discard,
            Lock,
            Move,
            Publish,
            Update
        }

        #region Private methods

        /// <summary>
        /// This is a hack to avoid a 500 error because of TFS bug: 3739.
        /// If you send a non-null/empty Name of a UseCase SubArtifact to the Update REST call, it returns 500 Internal Server Error.
        /// </summary>
        /// <param name="updateArtifact">The artifact to be updated.</param>
        /// <returns>A dictionary of sub-artifact IDs and Names for any names that were removed.</returns>
        protected static Dictionary<int, string> RemoveSubArtifactNamesForBug3739(INovaArtifactDetails updateArtifact)
        {
            ThrowIf.ArgumentNull(updateArtifact, nameof(updateArtifact));

            // Hack for TFS bug 3739:  If you send a non-null/empty Name of a UseCase SubArtifact to the Update REST call, it returns 500 Internal Server Error
            // Set Name=null for all SubArtifacts to prevent a 500 error.
            var savedSubArtifactNames = new Dictionary<int, string>();

            if ((updateArtifact.PredefinedType.HasValue) && (updateArtifact.PredefinedType.Value == (int)ItemTypePredefined.UseCase))
            {
                updateArtifact.SubArtifacts?.ForEach(delegate (NovaSubArtifact subArtifact)
                {
                    savedSubArtifactNames.Add(subArtifact.Id.Value, subArtifact.Name);
                    subArtifact.Name = null;
                });
            }

            return savedSubArtifactNames;
        }

        /// <summary>
        /// This is a hack to avoid a 500 error because of TFS bug: 3739.
        /// If you send a non-null/empty Name of a UseCase SubArtifact to the Update REST call, it returns 500 Internal Server Error.
        /// </summary>
        /// <param name="updateArtifact">The artifact that was updated and which will have its sub-artifact Names restored.</param>
        /// <param name="savedSubArtifactNames">A dictionary of sub-artifact IDs and Names for any names that were removed.</param>
        protected static void RestoreSubArtifactNamesForBug3739(INovaArtifactDetails updateArtifact, Dictionary<int, string> savedSubArtifactNames)
        {
            ThrowIf.ArgumentNull(updateArtifact, nameof(updateArtifact));
            ThrowIf.ArgumentNull(savedSubArtifactNames, nameof(savedSubArtifactNames));

            // Hack for TFS bug 3739: Restore sub-artifact names after the update call.
            if (savedSubArtifactNames.Any())
            {
                foreach (var idAndName in savedSubArtifactNames)
                {
                    var subArtifact = updateArtifact.SubArtifacts?.Find(a => a.Id == idAndName.Key);

                    subArtifact.Name = idAndName.Value;
                }
            }
        }

        #endregion Private methods

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
            set { Artifact.CustomPropertyValues = value; }
        }

        public List<CustomProperty> SpecificPropertyValues
        {
            get { return Artifact.SpecificPropertyValues; }
            set { Artifact.CustomPropertyValues = value; }
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
