using Common;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Common.Enums;
using Model.Factories;
using Model.Impl;
using Model.JobModel;
using Model.ModelHelpers;
using Model.OpenApiModel.Services;
using Model.SearchServiceModel;
using Model.StorytellerModel;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Utilities;
using Utilities.Facades;
using Utilities.Factories;
using static Model.Impl.ArtifactStore;

namespace Helper
{
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]    // This is a Helper class, so this is expected to be large.
    public class TestHelper : IDisposable, IArtifactObserver, INovaArtifactObserver
    {
        public enum ProjectRole
        {
            None,
            Viewer,
            AuthorFullAccess,
            Author,
            Collaborator
        }

        private Dictionary<GroupLicenseType, LicenseLevel> _groupLicenseTypeToLicenseLevel = new Dictionary<GroupLicenseType, LicenseLevel>
        {
            {GroupLicenseType.Author, LicenseLevel.Author},
            {GroupLicenseType.Collaborate, LicenseLevel.Collaborator},
            {GroupLicenseType.None, LicenseLevel.Viewer}
        };

        public static class GoldenDataProject
        {
            public const string Default = "test";
            public const string CustomData = "Custom Data";
            public const string UIProject = "UI-Project";
            public const string EmptyProjectNonRequiredCustomPropertiesAssigned = "Empty Project With PropertyTypes - Non-Required Properties Assigned";
            public const string EmptyProjectWithSubArtifactRequiredProperties = "Empty Project with SubArtifact Required Properties";
        }

        public static readonly string DraftDescription = "description of item in the Draft state";
        public static readonly string PublishedDescription = "description of item in the Published state";


        private bool _isDisposed = false;

        // Nova services:
        public IAccessControl AccessControl { get; } = AccessControlFactory.GetAccessControlFromTestConfig();
        public IAdminStore AdminStore { get; } = AdminStoreFactory.GetAdminStoreFromTestConfig();
        public IArtifactStore ArtifactStore { get; } = ArtifactStoreFactory.GetArtifactStoreFromTestConfig();
        public IBlueprintServer BlueprintServer { get; } = BlueprintServerFactory.GetBlueprintServerFromTestConfig();
        public IConfigControl ConfigControl { get; } = ConfigControlFactory.GetConfigControlFromTestConfig();
        public IFileStore FileStore { get; } = FileStoreFactory.GetFileStoreFromTestConfig();
        public ImpactAnalysis ImpactAnalysis { get; } = ImpactAnalysisFactory.GetImpactAnalysisFromTestConfig();
        public IOpenApi OpenApi { get; } = OpenApiFactory.GetOpenApiFromTestConfig();
        public ISearchService SearchService { get; } = SearchServiceFactory.GetSearchServiceFromTestConfig();
        public IStoryteller Storyteller { get; } = StorytellerFactory.GetStorytellerFromTestConfig();
        public ISvcComponents SvcComponents { get; } = SvcComponentsFactory.GetSvcSharedFromTestConfig();
        public ISvcShared SvcShared { get; } = SvcSharedFactory.GetSvcSharedFromTestConfig();

        // Lists of objects created by this class to be disposed:
        public List<IArtifactBase> Artifacts { get; } = new List<IArtifactBase>();
        public List<IProject> Projects { get; } = new List<IProject>();
        public List<IUser> Users { get; } = new List<IUser>();
        public List<InstanceUser> InstanceUsers { get; } = new List<InstanceUser>();
        public List<IGroup> Groups { get; } = new List<IGroup>();
        public List<IProjectRole> ProjectRoles { get; } = new List<IProjectRole>();

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Dictionary<IUser, List<int>> NovaArtifacts { get; } = new Dictionary<IUser, List<int>>();

        #region IArtifactObserver methods

        /// <seealso cref="INovaArtifactObserver.WrappedArtifactsToDispose" />
        public List<ArtifactWrapper> WrappedArtifactsToDispose { get; } = new List<ArtifactWrapper>();   // TODO: Dispose these artifacts and track their state...

        // TODO: Change the IArtifactObserver functions below to INovaArtifactObserver functions.

        /// <seealso cref="IArtifactObserver.NotifyArtifactDeleted(IEnumerable{int})" />
        public void NotifyArtifactDeleted(IEnumerable<int> deletedArtifactIds)
        {
            ThrowIf.ArgumentNull(deletedArtifactIds, nameof(deletedArtifactIds));
            var artifactIds = deletedArtifactIds as IList<int> ?? deletedArtifactIds.ToList();

            Logger.WriteTrace("*** {0}.{1}({2}) was called.",
                nameof(TestHelper), nameof(TestHelper.NotifyArtifactDeleted), string.Join(", ", artifactIds));

            ArtifactObserverHelper.NotifyArtifactDeleted(Artifacts, artifactIds);
        }

        /// <seealso cref="IArtifactObserver.NotifyArtifactDiscarded(IEnumerable{int})" />
        public void NotifyArtifactDiscarded(IEnumerable<int> discardedArtifactIds)
        {
            ThrowIf.ArgumentNull(discardedArtifactIds, nameof(discardedArtifactIds));
            var artifactIds = discardedArtifactIds as int[] ?? discardedArtifactIds.ToArray();

            Logger.WriteTrace("*** {0}.{1}({2}) was called.",
                nameof(TestHelper), nameof(TestHelper.NotifyArtifactDiscarded), string.Join(", ", artifactIds));

            ArtifactObserverHelper.NotifyArtifactDiscarded(Artifacts, artifactIds);
        }

        /// <seealso cref="IArtifactObserver.NotifyArtifactPublished(IEnumerable{int})" />
        public void NotifyArtifactPublished(IEnumerable<int> publishedArtifactIds)
        {
            ThrowIf.ArgumentNull(publishedArtifactIds, nameof(publishedArtifactIds));
            var artifactIds = publishedArtifactIds as IList<int> ?? publishedArtifactIds.ToList();

            Logger.WriteTrace("*** {0}.{1}({2}) was called.",
                nameof(TestHelper), nameof(TestHelper.NotifyArtifactPublished), string.Join(", ", artifactIds));

            ArtifactObserverHelper.NotifyArtifactPublished(Artifacts, artifactIds);
        }

        #endregion IArtifactObserver methods

        #region Artifact Management

        /// <summary>
        /// Create an Open API artifact object and populate required attribute values with ArtifactTypeId, ArtifactTypeName, and ProjectId based the target project
        /// </summary>
        /// <param name="address">address for Blueprint application server</param>
        /// <param name="user">user for authentication</param>
        /// <param name="project">The target project</param>
        /// <param name="artifactType">artifactType</param>
        /// <returns>new artifact object for the target project with selected artifactType</returns>
        public IOpenApiArtifact CreateOpenApiArtifact(string address, IUser user, IProject project, BaseArtifactType artifactType)
        {
            var artifact = ArtifactFactory.CreateOpenApiArtifact(address, user, project, artifactType);
            Artifacts.Add(artifact);
            artifact.RegisterObserver(this);
            return artifact;
        }

        /// <summary>
        /// Create an Open API artifact object using the Blueprint application server address from the TestConfiguration file
        /// </summary>
        /// <param name="project">The target project</param>
        /// <param name="user">user for authentication</param>
        /// <param name="artifactType">artifactType</param>
        /// <returns>new artifact object</returns>
        public IOpenApiArtifact CreateOpenApiArtifact(IProject project, IUser user, BaseArtifactType artifactType)
        {
            var artifact = ArtifactFactory.CreateOpenApiArtifact(project, user, artifactType);
            Artifacts.Add(artifact);
            artifact.RegisterObserver(this);
            return artifact;
        }

        /// <summary>
        /// Create and save an artifact object using the Blueprint application server address from the TestConfiguration file.
        /// </summary>
        /// <param name="project">The target project.</param>
        /// <param name="user">User for authentication.</param>
        /// <param name="artifactType">ArtifactType.</param>
        /// <param name="parentArtifact">(optional) The parent artifact.  By default artifact will be created in root of the project.</param>
        /// <param name="name">(optional) The name of the artifact.  By default a random name is used.</param>
        /// <returns>The new artifact object.</returns>
        public IOpenApiArtifact CreateAndSaveOpenApiArtifact(IProject project,
            IUser user,
            BaseArtifactType artifactType,
            IArtifactBase parentArtifact = null,
            string name = null)
        {
            var artifact = ArtifactFactory.CreateOpenApiArtifact(project, user, artifactType, parent: parentArtifact, name: name);
            Artifacts.Add(artifact);
            artifact.RegisterObserver(this);
            artifact.Save();
            return artifact;
        }

        /// <summary>
        /// Creates a new OpenApi artifact, then saves and publishes it the specified number of times.
        /// </summary>
        /// <param name="project">The project where the artifact is to be created.</param>
        /// <param name="user">The user who will create the artifact.</param>
        /// <param name="artifactType">The type of artifact to create.</param>
        /// <param name="numberOfVersions">(optional) The number of times to save and publish the artifact (to create multiple historical versions).</param>
        /// <returns>The OpenApi artifact.</returns>
        public IOpenApiArtifact CreateAndPublishOpenApiArtifact(IProject project,
            IUser user,
            BaseArtifactType artifactType,
            int numberOfVersions = 1)
        {
            var artifact = CreateOpenApiArtifact(project, user, artifactType);

            for (int i = 0; i < numberOfVersions; ++i)
            {
                artifact.Save();
                artifact.Publish();
            }

            return artifact;
        }

        /// <summary>
        /// Creates a list of new published OpenApi artifacts.
        /// </summary>
        /// <param name="project">The project where the artifacts are to be created.</param>
        /// <param name="user">The user who will create the artifacts.</param>
        /// <param name="artifactType">The type of artifacts to create.</param>
        /// <param name="numberOfArtifacts">The number of artifacts to create.</param>
        /// <returns>The list of OpenApi artifacts.</returns>
        public List<IArtifactBase> CreateAndPublishMultipleOpenApiArtifacts(IProject project, IUser user, BaseArtifactType artifactType, int numberOfArtifacts)
        {
            var artifactList = new List<IArtifactBase>();

            for (int i = 0; i < numberOfArtifacts; ++i)
            {
                var artifact = CreateAndPublishOpenApiArtifact(project, user, artifactType);
                artifactList.Add(artifact);
            }

            return artifactList;
        }


        /// <summary>
        /// Create an artifact object and populate required attribute values with ArtifactTypeId, ArtifactTypeName, and ProjectId based the target project
        /// </summary>
        /// <param name="address">address for Blueprint application server</param>
        /// <param name="user">user for authentication</param>
        /// <param name="project">The target project</param>
        /// <param name="artifactType">artifactType</param>
        /// <returns>new artifact object for the target project with selected artifactType</returns>
        public IArtifact CreateArtifact(string address, IUser user, IProject project, BaseArtifactType artifactType)
        {
            var artifact = ArtifactFactory.CreateArtifact(address, user, project, artifactType);
            Artifacts.Add(artifact);
            artifact.RegisterObserver(this);
            return artifact;
        }

        /// <summary>
        /// Create an artifact object using the Blueprint application server address from the TestConfiguration file.
        /// </summary>
        /// <param name="project">The target project.</param>
        /// <param name="user">User for authentication.</param>
        /// <param name="artifactType">ArtifactType.</param>
        /// <param name="parent">(optional) The parent artifact. By default artifact will be created in the root of the project.</param>
        /// <param name="name">(optional) Artifact's name.</param>
        /// <returns>The new artifact object.</returns>
        public IArtifact CreateArtifact(IProject project, IUser user, BaseArtifactType artifactType,
            IArtifactBase parent = null, string name = null)
        {
            var artifact = ArtifactFactory.CreateArtifact(project, user, artifactType, parent: parent, name: name);
            Artifacts.Add(artifact);
            artifact.RegisterObserver(this);
            return artifact;
        }

        /// <summary>
        /// Create and save an artifact object using the Blueprint application server address from the TestConfiguration file.
        /// </summary>
        /// <param name="project">The target project.</param>
        /// <param name="user">User for authentication.</param>
        /// <param name="artifactType">ArtifactType.</param>
        /// <param name="parent">(optional) The parent artifact. By default artifact will be created in the root of the project.</param>
        /// <param name="name">(optional) Artifact's name.</param>
        /// <returns>The new artifact object.</returns>
        public IArtifact CreateAndSaveArtifact(IProject project, IUser user, BaseArtifactType artifactType,
            IArtifactBase parent = null, string name = null)
        {
            var artifact = CreateArtifact(project, user, artifactType, parent, name: name);
            artifact.Save();
            return artifact;
        }

        /// <summary>
        /// Creates a new artifact collection.
        /// </summary>
        /// <param name="project">The project where the collection should be created.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="parentId">(optional) The parent of this collection.
        ///     By default it creates the collection in the project's default collection folder.</param>
        /// <param name="orderIndex">(optional) The order index of this collection.
        ///     By default the order index should be after the last collection/folder.</param>
        /// <param name="name">(optional) The name of the new Collection.</param>
        /// <returns>The new Collection.</returns>
        public ArtifactWrapper CreateUnpublishedCollection(
            IProject project,
            IUser user,
            int? parentId = null,
            double? orderIndex = null,
            string name = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));

            if (parentId == null)
            {
                var collectionFolder = project.GetDefaultCollectionFolder(user);
                parentId = collectionFolder.Id;
            }

            // fake type as far as we don't have Collection in OpenApi
            return CreateNovaArtifact(user, project, ItemTypePredefined.ArtifactCollection, parentId, orderIndex, name);
        }

        /// <summary>
        /// Creates a new artifact Collection Folder.
        /// </summary>
        /// <param name="project">The project where the collection folder should be created.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="parentId">(optional) The parent of this collection folder.
        ///     By default it creates the collection folder in the project's default collection folder.</param>
        /// <param name="orderIndex">(optional) The order index of this collection folder.
        ///     By default the order index should be after the last collection/folder.</param>
        /// <param name="name">(optional) The name of the new Collection Folder.</param>
        /// <returns>The new Collection Folder.</returns>
        public ArtifactWrapper CreateUnpublishedCollectionFolder(
            IProject project,
            IUser user,
            int? parentId = null,
            double? orderIndex = null,
            string name = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));

            if (parentId == null)
            {
                var collectionFolder = project.GetDefaultCollectionFolder(user);
                parentId = collectionFolder.Id;
            }

            return CreateNovaArtifact(user, project, ItemTypePredefined.CollectionFolder, parentId, orderIndex, name);
        }

        /// <summary>
        /// Creates collection artifact or collection folder (unpublished).
        /// </summary>
        /// <param name="project">Project in which collection artifact or collection folder will be created.</param>
        /// <param name="user">User who creates collection artifact or collection folder.</param>
        /// <param name="artifactType">Collection artifact or collection folder.</param>
        /// <param name="orderIndex">(optional) The order index of this Collection or Collection Folder.
        ///     By default the order index should be after the last collection/folder.</param>
        /// <param name="name">(optional) The name of the new Collection or Collection Folder.</param>
        /// <returns>A new Collection or Collection Folder.</returns>
        public ArtifactWrapper CreateCollectionOrCollectionFolder(
            IProject project,
            IUser user,
            BaselineAndCollectionTypePredefined artifactType,
            double? orderIndex = null,
            string name = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));

            var collectionFolder = project.GetDefaultCollectionFolder(user);

            return CreateNovaArtifact(user, project, (ItemTypePredefined)artifactType, collectionFolder.Id, orderIndex, name);
        }

        /// <summary>
        /// Creates and saves a new Nova artifact (wrapped inside an IArtifact object).
        /// </summary>
        /// <param name="project">The project where the Nova artifact should be created.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="itemType">The Nova base ItemType to create.</param>
        /// <param name="parentId">(optional) The parent of this Nova artifact.
        ///     By default the parent should be the project.</param>
        /// <param name="orderIndex">(optional) The order index of this Nova artifact.
        ///     By default the order index should be after the last artifact.</param>
        /// <param name="baseType">(optional) The OpenAPI base artifact type for this artifact.
        ///     By default the ItemTypePredefined is converted into its equivalent BaseArtifactType.</param>
        /// <param name="name">(optional) The artifact name.  By default a random name is created.</param>
        /// <param name="artifactTypeName">(optional) Name of the artifact type to be used to create the artifact</param>
        /// <returns>The Nova artifact wrapped in an IArtifact.</returns>
        public IArtifact CreateWrapAndSaveNovaArtifact(IProject project, IUser user, ItemTypePredefined itemType,
            int? parentId = null, double? orderIndex = null, BaseArtifactType? baseType = null, string name = null, string artifactTypeName = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));

            if (name == null)
            {
                name = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);
            }

            var collection = Model.Impl.ArtifactStore.CreateArtifact(ArtifactStore.Address, user,
                itemType, name, project, artifactTypeName, parentId, orderIndex);

            return WrapNovaArtifact(collection, project, user, baseType, name);
        }

        /// <summary>
        /// Creates and saves a new Nova artifact (wrapped inside an ArtifactWrapper that tracks the state of the artifact.).
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="project">The project where the Nova artifact should be created.</param>
        /// <param name="itemType">The Nova base ItemType to create.</param>
        /// <param name="parentId">(optional) The parent of this Nova artifact.
        ///     By default the parent should be the project.</param>
        /// <param name="orderIndex">(optional) The order index of this Nova artifact.
        ///     By default the order index should be after the last artifact.</param>
        /// <param name="name">(optional) The artifact name.  By default a random name is created.</param>
        /// <param name="artifactTypeName">(optional) Name of the artifact type to be used to create the artifact</param>
        /// <returns>The Nova artifact wrapped in an ArtifactWrapper that tracks the state of the artifact.</returns>
        public ArtifactWrapper CreateNovaArtifact(
            IUser user, IProject project, ItemTypePredefined itemType,
            int? parentId = null, double? orderIndex = null, string name = null, string artifactTypeName = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));

            if (name == null)
            {
                name = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);
            }

            var artifact = Model.Impl.ArtifactStore.CreateArtifact(ArtifactStore.Address, user,
                itemType, name, project, artifactTypeName, parentId, orderIndex);

            return WrapArtifact(artifact, project, user);
        }

        /// <summary>
        /// Creates and saves a new Nova artifact (wrapped inside an ArtifactWrapper that tracks the state of the artifact.).
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="project">The project where the Nova artifact should be created.</param>
        /// <param name="itemType">The Nova base ItemType to create.</param>
        /// <param name="parentId">(optional) The parent of this Nova artifact.
        ///     By default the parent should be the project.</param>
        /// <param name="orderIndex">(optional) The order index of this Nova artifact.
        ///     By default the order index should be after the last artifact.</param>
        /// <param name="name">(optional) The artifact name.  By default a random name is created.</param>
        /// <param name="artifactTypeName">(optional) Name of the artifact type to be used to create the artifact</param>
        /// <returns>The Nova artifact wrapped in an ArtifactWrapper that tracks the state of the artifact.</returns>
        public ArtifactWrapper CreateAndPublishNovaArtifact(
            IUser user, IProject project, ItemTypePredefined itemType,
            int? parentId = null, double? orderIndex = null, string name = null, string artifactTypeName = null)
        {
            var wrappedArtifact = CreateNovaArtifact(user, project, itemType, parentId, orderIndex, name, artifactTypeName);
            wrappedArtifact.Publish(user);

            return wrappedArtifact;
        }

        /// <summary>
        /// Creates a new artifact, then saves and publishes it the specified number of times.
        /// </summary>
        /// <param name="project">The project where the artifact is to be created.</param>
        /// <param name="user">The user who will create the artifact.</param>
        /// <param name="itemType">The Nova base ItemType to create.</param>
        /// <param name="numberOfVersions">The number of times to save and publish the artifact (to create multiple historical versions).</param>
        /// <param name="parentId">(optional) The parent of this Nova artifact.
        ///     By default the parent should be the project.</param>
        /// <param name="orderIndex">(optional) The order index of this Nova artifact.
        ///     By default the order index should be after the last artifact.</param>
        /// <param name="name">(optional) The artifact name.  By default a random name is created.</param>
        /// <param name="artifactTypeName">(optional) Name of the artifact type to be used to create the artifact</param>
        /// <returns>The Nova artifact wrapped in an ArtifactWrapper that tracks the state of the artifact.</returns>
        public ArtifactWrapper CreateAndPublishNovaArtifactWithMultipleVersions(
            IUser user, IProject project, ItemTypePredefined itemType, int numberOfVersions,
            int? parentId = null, double? orderIndex = null, string name = null, string artifactTypeName = null)
        {
            var artifact = CreateAndPublishNovaArtifact(user, project, itemType, parentId, orderIndex, name, artifactTypeName);

            for (int i = 1; i < numberOfVersions; ++i)
            {
                string newDescription = I18NHelper.FormatInvariant("Description for version: {0}", artifact.Version + 1);

                artifact.Lock(user);
                artifact.SaveWithNewDescription(user, newDescription);
                artifact.Publish(user);
            }

            return artifact;
        }

        /// <summary>
        /// Creates and Publishes a Nova Artifact for a Specific Standard Artifact Type.
        /// </summary>
        /// <param name="user">The user creating the artifact.</param>
        /// <param name="project">The project where the artifact is to be created.</param>
        /// <param name="itemType">The Nova base ItemType to create.</param>
        /// <returns>The Standard Nova artifact in a published state.</returns>
        public ArtifactWrapper CreateAndPublishNovaArtifactForStandardArtifactType(IUser user, IProject project, ItemTypePredefined itemType)
        {
            var artifactTypeName = ArtifactStoreHelper.GetStandardPackArtifactTypeName(itemType);

            return CreateAndPublishNovaArtifact(user, project, itemType,
                artifactTypeName: artifactTypeName);
        }

        /// <summary>
        /// Wraps an INovaArtifactDetails in an ArtifactWrapper and adds it the list of artifacts that get disposed.
        /// </summary>
        /// <param name="artifact">The INovaArtifactDetails that was created by ArtifactStore.</param>
        /// <param name="project">The project where the artifact was created.</param>
        /// <param name="createdBy">The user that created this artifact.</param>
        /// <returns>The ArtifactWrapper for the artifact.</returns>
        public ArtifactWrapper WrapArtifact(INovaArtifactDetails artifact, IProject project, IUser createdBy)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            var wrappedArtifact = ArtifactWrapperFactory.CreateArtifactWrapper(artifact, project, createdBy);
            WrappedArtifactsToDispose.Add(wrappedArtifact);

            return wrappedArtifact;
        }

        /// <summary>
        /// Wraps an INovaProcess in a DocumentArtifactWrapper and adds it the list of artifacts that get disposed.
        /// </summary>
        /// <param name="artifact">The INovaArtifactDetails that was created by ArtifactStore.</param>
        /// <param name="project">The project where the artifact was created.</param>
        /// <param name="createdBy">The user that created this artifact.</param>
        /// <returns>The DocumentArtifactWrapper for the artifact.</returns>
        public DocumentArtifactWrapper WrapDocumentArtifact(INovaArtifactDetails artifact, IProject project, IUser createdBy)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            var wrappedArtifact = (DocumentArtifactWrapper) ArtifactWrapperFactory.CreateArtifactWrapper(artifact, project, createdBy);
            WrappedArtifactsToDispose.Add(wrappedArtifact);

            return wrappedArtifact;
        }

        /// <summary>
        /// Creates and publishes a new Nova artifact (wrapped inside an IArtifact object).
        /// </summary>
        /// <param name="project">The project where the Nova artifact should be created.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="itemType">The Nova base ItemType to create.</param>
        /// <param name="parentId">(optional) The parent of this Nova artifact.
        ///     By default the parent should be the project.</param>
        /// <param name="orderIndex">(optional) The order index of this Nova artifact.
        ///     By default the order index should be after the last artifact.</param>
        /// <param name="baseType">(optional) The OpenAPI base artifact type for this artifact.
        ///     By default the ItemTypePredefined is converted into its equivalent BaseArtifactType.</param>
        /// <param name="name">(optional) The artifact name.  By default a random name is created.</param>
        /// <param name="artifactTypeName">(optional) Name of the artifact type to be used to create the artifact</param>
        /// <returns>The Nova artifact wrapped in an IArtifact.</returns>
        public IArtifact CreateWrapAndPublishNovaArtifact(IProject project, IUser user, ItemTypePredefined itemType,
            int? parentId = null, double? orderIndex = null, BaseArtifactType? baseType = null, string name = null, string artifactTypeName = null)
        {
            var artifact = CreateWrapAndSaveNovaArtifact(project, user, itemType, parentId, orderIndex, baseType, name, artifactTypeName);
            ArtifactStore.PublishArtifact(artifact, user);
            return artifact;
        }

        /// <summary>
        /// Creates, Wraps and Publishes a Nova Artifact for a Specific Standard Artifact Type
        /// </summary>
        /// <param name="project">The project where the artifact is to be created.</param>
        /// <param name="user">The user creating the artifact.</param>
        /// <param name="itemType">The Nova base ItemType to create.</param>
        /// <returns>The Nova artifact wrapped in an IArtifact.</returns>
        public ArtifactWrapper CreateWrapAndPublishNovaArtifactForStandardArtifactType(IProject project, IUser user, ItemTypePredefined itemType)
        {
            var artifactTypeName = ArtifactStoreHelper.GetStandardPackArtifactTypeName(itemType);

            return CreateAndPublishNovaArtifact(user, project, itemType, artifactTypeName: artifactTypeName);
        }

        /// <summary>
        /// Creates, Wraps and Publishes a Nova Artifact for a Specific Custom Artifact Type
        /// </summary>
        /// <param name="project">The project where the artifact is to be created.</param>
        /// <param name="user">The user creating the artifact.</param>
        /// <param name="itemType">The Nova base ItemType to create.</param>
        /// <returns>The Nova artifact wrapped in an IArtifact.</returns>
        public ArtifactWrapper CreateWrapAndPublishNovaArtifactForCustomArtifactType(IProject project, IUser user, ItemTypePredefined itemType)
        {
            var artifactTypeName = ArtifactStoreHelper.GetCustomArtifactTypeName(itemType);

            return CreateAndPublishNovaArtifact(user, project, itemType, artifactTypeName: artifactTypeName);
        }

        /// <summary>
        /// Create and save multiple artifacts using the Blueprint application server address from the TestConfiguration file.
        /// </summary>
        /// <param name="project">The target project.</param>
        /// <param name="user">User for authentication.</param>
        /// <param name="artifactType">ArtifactType.</param>
        /// <param name="numberOfArtifacts">The number of artifacts to create.</param>
        /// <param name="parent">(optional) The parent artifact. By default artifact will be created in the root of the project.</param>
        /// <returns>The list of artifacts created.</returns>
        public List<IArtifactBase> CreateAndSaveMultipleArtifacts(IProject project,
            IUser user,
            BaseArtifactType artifactType,
            int numberOfArtifacts,
            IArtifactBase parent = null)
        {
            var artifactList = new List<IArtifactBase>();

            for (int i = 0; i < numberOfArtifacts; ++i)
            {
                var artifact = CreateAndSaveArtifact(project, user, artifactType, parent);
                artifactList.Add(artifact);
            }

            return artifactList;
        }

        /// <summary>
        /// Creates multiple artifacts.
        /// </summary>
        /// <param name="project">The target project.</param>
        /// <param name="user">User for authentication.</param>
        /// <param name="artifactType">The artifact type.</param>
        /// <param name="numberOfArtifacts">The number of artifacts to create.</param>
        /// <param name="parentId">(optional) The ID of the parent artifact.  By default artifact will be created in the root of the project.</param>
        /// <returns>The list of artifacts created.</returns>
        public List<ArtifactWrapper> CreateAndSaveMultipleArtifacts(
            IProject project,
            IUser user,
            ItemTypePredefined artifactType,
            int numberOfArtifacts,
            int? parentId = null)
        {
            var artifactList = new List<ArtifactWrapper>();

            for (int i = 0; i < numberOfArtifacts; ++i)
            {
                var artifact = CreateNovaArtifact(user, project, artifactType, parentId);
                artifactList.Add(artifact);
            }

            return artifactList;
        }

        /// <summary>
        /// Creates a new artifact, then saves and publishes it the specified number of times.
        /// </summary>
        /// <param name="project">The project where the artifact is to be created.</param>
        /// <param name="user">The user who will create the artifact.</param>
        /// <param name="artifactType">The type of artifact to create.</param>
        /// <param name="parent">(optional) The parent artifact. By default artifact will be created in the root of the project.</param>
        /// <param name="name">(optional) Artifact's name.</param>
        /// <returns>The artifact.</returns>
        public IArtifact CreateAndPublishArtifact(IProject project,
            IUser user,
            BaseArtifactType artifactType,
            IArtifactBase parent = null,
            string name = null)
        {
            var artifact = CreateArtifact(project, user, artifactType, parent, name);

            artifact.Save();
            artifact.Publish();

            return artifact;
        }

        /// <summary>
        /// Creates and publishes a new artifact Collection..
        /// </summary>
        /// <param name="project">The project where the collection should be created.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="parentId">(optional) The parent of this collection.
        ///     By default it creates the collection in the project's default collection folder.</param>
        /// <param name="orderIndex">(optional) The order index of this collection.
        ///     By default the order index should be after the last collection/folder.</param>
        /// <param name="name">(optional) The name of the new Collection.</param>
        /// <returns>The new Collection.</returns>
        public ArtifactWrapper CreateAndPublishCollection(
            IProject project,
            IUser user,
            int? parentId = null,
            double? orderIndex = null,
            string name = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));

            if (parentId == null)
            {
                var collectionFolder = project.GetDefaultCollectionFolder(user);
                parentId = collectionFolder.Id;
            }

            return CreateAndPublishNovaArtifact(user, project, ItemTypePredefined.ArtifactCollection, parentId, orderIndex, name);
        }

        /// <summary>
        /// Creates and publishes a new artifact Collection Folder.
        /// </summary>
        /// <param name="project">The project where the collection folder should be created.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="parentId">(optional) The parent of this collection folder.
        ///     By default it creates the collection folder in the project's default collection folder.</param>
        /// <param name="orderIndex">(optional) The order index of this collection folder.
        ///     By default the order index should be after the last collection/folder.</param>
        /// <param name="name">(optional) The name of the new Collection Folder.</param>
        /// <returns>The new Collection Folder.</returns>
        public ArtifactWrapper CreateAndPublishCollectionFolder(
            IProject project,
            IUser user,
            int? parentId = null,
            double? orderIndex = null,
            string name = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));

            if (parentId == null)
            {
                var collectionFolder = project.GetDefaultCollectionFolder(user);
                parentId = collectionFolder.Id;
            }

            return CreateAndPublishNovaArtifact(user, project, ItemTypePredefined.CollectionFolder, parentId, orderIndex, name);
        }

        /// <summary>
        /// Creates a list of new published artifacts.
        /// </summary>
        /// <param name="project">The project where the artifacts are to be created.</param>
        /// <param name="user">The user who will create the artifacts.</param>
        /// <param name="artifactType">The type of artifacts to create.</param>
        /// <param name="numberOfArtifacts">The number of artifacts to create.</param>
        /// <param name="parent">(optional) The parent of these artifacts.  Defaults to project root.</param>
        /// <returns>The list of artifacts.</returns>
        public List<IArtifactBase> CreateAndPublishMultipleArtifacts(IProject project,
            IUser user,
            BaseArtifactType artifactType,
            int numberOfArtifacts,
            IArtifactBase parent = null)
        {
            var artifactList = new List<IArtifactBase>();

            for (int i = 0; i < numberOfArtifacts; ++i)
            {
                var artifact = CreateAndPublishArtifact(project, user, artifactType, parent);
                artifactList.Add(artifact);
            }

            return artifactList;
        }

        /// <summary>
        /// Creates a list of new published artifacts.
        /// </summary>
        /// <param name="project">The project where the artifacts are to be created.</param>
        /// <param name="user">The user who will create the artifacts.</param>
        /// <param name="artifactType">The type of artifacts to create.</param>
        /// <param name="numberOfArtifacts">The number of artifacts to create.</param>
        /// <param name="parentId">(optional) The ID of the parent of these artifacts.  Defaults to project root.</param>
        /// <returns>The list of artifacts.</returns>
        public List<ArtifactWrapper> CreateAndPublishMultipleArtifacts(
            IProject project,
            IUser user,
            ItemTypePredefined artifactType,
            int numberOfArtifacts,
            int? parentId = null)
        {
            var artifactList = CreateAndSaveMultipleArtifacts(project, user, artifactType, numberOfArtifacts, parentId);

            ArtifactStore.PublishArtifacts(artifactList.Select(a => a.Id), user);

            // Update the artifact states manually because we didn't use ArtifactWrapper to Publish them.
            artifactList.ForEach(a => a.UpdateArtifactState(ArtifactWrapper.ArtifactOperation.Publish));
            artifactList.ForEach(a => a.Version = 1);

            return artifactList;
        }

        /// <summary>
        /// Creates a chain of saved parent/child artifacts of the given artifact types.
        /// </summary>
        /// <param name="project">The project where the artifacts are to be created.</param>
        /// <param name="user">The user who will create the artifacts.</param>
        /// <param name="artifactTypeChain">The artifact types of each artifact in the chain starting at the top parent.</param>
        /// <returns>The list of artifacts in the chain starting at the top parent.</returns>
        public List<ArtifactWrapper> CreateSavedArtifactChain(IProject project, IUser user, ItemTypePredefined[] artifactTypeChain)
        {
            ThrowIf.ArgumentNull(artifactTypeChain, nameof(artifactTypeChain));

            var artifactChain = new List<ArtifactWrapper>();
            ArtifactWrapper bottomArtifact = null;

            // Create artifact chain.
            foreach (var artifactType in artifactTypeChain)
            {
                bottomArtifact = CreateNovaArtifact(user, project, artifactType, parentId: bottomArtifact?.Id);
                artifactChain.Add(bottomArtifact);
            }

            return artifactChain;
        }

        /// <summary>
        /// Creates a chain of published parent/child artifacts of the given artifact types.
        /// </summary>
        /// <param name="project">The project where the artifacts are to be created.</param>
        /// <param name="user">The user who will create the artifacts.</param>
        /// <param name="artifactTypeChain">The artifact types of each artifact in the chain starting at the top parent.</param>
        /// <returns>The list of artifacts in the chain starting at the top parent.</returns>
        public List<ArtifactWrapper> CreatePublishedArtifactChain(IProject project, IUser user, ItemTypePredefined[] artifactTypeChain)
        {
            ThrowIf.ArgumentNull(artifactTypeChain, nameof(artifactTypeChain));

            var artifactChain = new List<ArtifactWrapper>();
            ArtifactWrapper bottomArtifact = null;

            // Create artifact chain.
            foreach (var artifactType in artifactTypeChain)
            {
                bottomArtifact = CreateAndPublishNovaArtifact(user, project, artifactType, bottomArtifact?.Id);
                artifactChain.Add(bottomArtifact);
            }

            return artifactChain;
        }

        /// <summary>
        /// Wraps an INovaArtifactDetails in an IArtifact and adds it the list of artifacts that get disposed.
        /// </summary>
        /// <param name="novaArtifact">The INovaArtifactDetails that was created by ArtifactStore.</param>
        /// <param name="project">The project where this artifact exists.</param>
        /// <param name="user">The user that created this artifact.</param>
        /// <param name="baseType">(optional) You can select a different BaseArtifactType here other than what's in the novaArtifact.
        ///     Use this for artifact types that don't exist in the BaseArtifactType enum.</param>
        /// <param name="name">(optional) Artifact's name.</param>
        /// <returns>The IArtifact wrapper for the novaArtifact.</returns>
        public IArtifact WrapNovaArtifact(INovaArtifactDetails novaArtifact,
            IProject project,
            IUser user,
            BaseArtifactType? baseType = null,
            string name = null)
        {
            ThrowIf.ArgumentNull(novaArtifact, nameof(novaArtifact));

            Assert.NotNull(novaArtifact.PredefinedType, "PredefinedType is null in the Nova Artifact!");

            if (baseType == null)
            {
                baseType = ((ItemTypePredefined)novaArtifact.PredefinedType.Value).ToBaseArtifactType();
            }

            var fakeParent = ArtifactFactory.CreateArtifact(project, user, baseType.Value, novaArtifact.ParentId);

            var artifact = ArtifactFactory.CreateArtifact(project,
                user,
                baseType.Value,
                novaArtifact.Id,
                fakeParent,
                name);

            artifact.IsSaved = true;
            Artifacts.Add(artifact);

            return artifact;
        }

        /// <summary>
        /// Updates a Nova artifact.
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="novaArtifactDetails">The artifact details of the Nova artifact being updated</param>
        /// <returns>The new Nova artifact that was created.</returns>
        public INovaArtifactDetails UpdateNovaArtifact(IUser user, INovaArtifactDetails novaArtifactDetails)
        {     
            return UpdateArtifact(ArtifactStore.Address, user, novaArtifactDetails);
        }

        /// <summary>
        /// Creates a new Baseline, Baseline Folder or Review.
        /// </summary>
        /// <param name="user">The user to perform the operation.</param>
        /// <param name="project">The project in which Baseline, Baseline Folder or Review artifact will be created.</param>
        /// <param name="artifactType">The artifact type to create (pass either ArtifactBaseline, BaselineFolder or ArtifactReviewPackage).</param>
        /// <param name="name">(optional) The name of artifact to create.  By default a random name will be used.</param>
        /// <param name="parentId">(optional) The ID of the parent artifact where this Baseline, Baseline Folder or Review should be created.  
        ///     By default it will be created in the default Baselines and Reviews folder for the project.</param>
        /// <returns>The Baseline, Baseline Folder or Review artifact.</returns>
        public ArtifactWrapper CreateBaselineOrBaselineFolderOrReview(
            IUser user,
            IProject project,
            BaselineAndCollectionTypePredefined artifactType,
            string name = null,
            int? parentId = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(project, nameof(project));

            parentId = parentId ?? project.GetDefaultBaselineFolder(user).Id;

            name = name ?? RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);

            return CreateNovaArtifact(user, project, (ItemTypePredefined)artifactType, parentId.Value, name: name);
        }

        /// <summary>
        /// Creates a new Baseline. Optionally adds artifact using AddToBaseline function (not artifact update)
        /// </summary>
        /// <param name="user">The user to perform the operation.</param>
        /// <param name="project">The project in which Baseline will be created.</param>
        /// <param name="name">(optional) The name of Baseline to create.  By default a random name will be used.</param>
        /// <param name="parentId">(optional) The ID of the parent artifact where Baseline should be created.  
        ///     By default it will be created in the default Baselines and Reviews folder for the project.</param>
        /// <param name="artifactToAddId">(optional) Artifact ID to be added to baseline.  By default empty baseline will be created.</param>
        /// <returns>The Baseline artifact.</returns>
        public ArtifactWrapper CreateBaseline(
            IUser user,
            IProject project,
            string name = null,
            int? parentId = null,
            int? artifactToAddId = null)
        {
            var baselineArtifact = CreateBaselineOrBaselineFolderOrReview(user, project, BaselineAndCollectionTypePredefined.ArtifactBaseline, name, parentId);

            if (artifactToAddId != null)
            {
                var result = ArtifactStore.AddArtifactToBaseline(user, artifactToAddId.Value, baselineArtifact.Id);
                Assert.GreaterOrEqual(result.ArtifactCount, 1, "At least one artifact should be added to Baseline.");
            }

            return baselineArtifact;
        }

        /// <summary>
        /// Creates a new Baseline Folder.
        /// </summary>
        /// <param name="user">The user to perform the operation.</param>
        /// <param name="project">The project in which Baseline Folder will be created.</param>
        /// <param name="name">(optional) The name of Baseline Folder to create.  By default a random name will be used.</param>
        /// <param name="parentId">(optional) The ID of the parent Baseline Folder where the Baseline Folder should be created.  
        ///     By default it will be created in the default Baselines and Reviews folder for the project.</param>
        /// <returns>The Baseline Folder artifact.</returns>
        public ArtifactWrapper CreateBaselineFolder(
            IUser user,
            IProject project,
            string name = null,
            int? parentId = null)
        {
            return CreateBaselineOrBaselineFolderOrReview(user, project, BaselineAndCollectionTypePredefined.BaselineFolder, name, parentId);
        }

        /// <summary>
        /// Create the property changeset for the target artifact
        /// </summary>
        /// <param name="artifactDetails">The nova artifact details</param>
        /// <param name="customProperties">The custom properties to add to the changeset</param>
        /// <param name="specificProperties">The specific properties to add to the changeset</param>
        /// <param name="subArtifacts">The subartifacts to add to the changeset</param>
        /// <returns>The artifact details changeset</returns>
        public static INovaArtifactDetails CreateArtifactChangeSet(
            INovaArtifactDetails artifactDetails,
            List<CustomProperty> customProperties,
            List<CustomProperty> specificProperties,
            List<NovaSubArtifact> subArtifacts)
        {
            ThrowIf.ArgumentNull(artifactDetails, nameof(artifactDetails));

            var changesetDetails = ArtifactFactory.CreateArtifact((ItemTypePredefined) artifactDetails.PredefinedType.Value);
            changesetDetails.Id = artifactDetails.Id;
            changesetDetails.ProjectId = artifactDetails.ProjectId;

            if (customProperties != null)
            {
                changesetDetails.CustomPropertyValues = new List<CustomProperty>();
                changesetDetails.CustomPropertyValues.AddRange(customProperties);
            }

            if (specificProperties != null)
            {
                changesetDetails.SpecificPropertyValues = new List<CustomProperty>();
                changesetDetails.SpecificPropertyValues.AddRange(specificProperties);
            }

            if (subArtifacts != null)
            {
                changesetDetails.SubArtifacts = new List<NovaSubArtifact>();
                changesetDetails.SubArtifacts.AddRange(subArtifacts);
            }

            return changesetDetails;
        }

        /// <summary>
        /// Create the property changeset for the target artifact
        /// </summary>
        /// <param name="artifactDetails">The nova artifact details</param>
        /// <param name="customProperty">(optional) The custom property to add to the changeset</param>
        /// <param name="specificProperty">(optional) The specific property to add to the changeset</param>
        /// <param name="subArtifact">(optional) The subartifact to add to the changeset</param>
        /// <returns>The artifact details changeset</returns>
        public static INovaArtifactDetails CreateArtifactChangeSet(INovaArtifactDetails artifactDetails,
            CustomProperty customProperty = null, CustomProperty specificProperty = null, NovaSubArtifact subArtifact = null)
        {
            ThrowIf.ArgumentNull(artifactDetails, nameof(artifactDetails));

            List<CustomProperty> customProperties = null;
            List<CustomProperty> specificProperties = null;
            List<NovaSubArtifact> subartifacts = null;

            if (customProperty != null)
            {
                customProperties = new List<CustomProperty> {customProperty};
            }

            if (specificProperty != null)
            {
                specificProperties = new List<CustomProperty> {specificProperty};
            }

            if (subArtifact != null)
            {
                subartifacts= new List<NovaSubArtifact> {subArtifact};
            }

            return CreateArtifactChangeSet(artifactDetails, customProperties, specificProperties, subartifacts);
        }

        /// <summary>
        /// Create the property changeset for the target subartifact
        /// </summary>
        /// <param name="subArtifact">The nova subartifact details</param>
        /// <param name="customProperties">(optional) The custom properties to add to the changeset</param>
        /// <param name="specificProperties">(optional) The specific properties to add to the changeset</param>
        /// <returns>The subartifact details changeset</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static NovaSubArtifact CreateSubArtifactChangeSet(NovaSubArtifact subArtifact, List<CustomProperty> customProperties = null, List<CustomProperty> specificProperties = null)
        {
            ThrowIf.ArgumentNull(subArtifact, nameof(subArtifact));

            var changesetDetails = new NovaSubArtifact
            {
                Id = subArtifact.Id
            };

            if (customProperties != null)
            {
                changesetDetails.CustomPropertyValues = new List<CustomProperty>();
                changesetDetails.CustomPropertyValues.AddRange(customProperties);
            }

            if (specificProperties != null)
            {
                changesetDetails.SpecificPropertyValues = new List<CustomProperty>();
                changesetDetails.SpecificPropertyValues.AddRange(specificProperties);
            }

            return changesetDetails;
        }

        /// <summary>
        /// Create the property changeset for the target subartifact
        /// </summary>
        /// <param name="subArtifact">The nova subartifact details</param>
        /// <param name="customProperty">(optional) The custom property to add to the changeset</param>
        /// <param name="specificProperty">(optional) The specific property to add to the changeset</param>
        /// <returns>The subartifact details changeset</returns>
        public static NovaSubArtifact CreateSubArtifactChangeSet(NovaSubArtifact subArtifact, CustomProperty customProperty = null, CustomProperty specificProperty = null)
        {
            ThrowIf.ArgumentNull(subArtifact, nameof(subArtifact));

            List<CustomProperty> customProperties = null;
            List<CustomProperty> specificProperties = null;

            if (customProperty != null)
            {
                customProperties = new List<CustomProperty> { customProperty };
            }

            if (specificProperty != null)
            {
                specificProperties = new List<CustomProperty> { specificProperty };
            }

            return CreateSubArtifactChangeSet(subArtifact, customProperties, specificProperties);
        }

        #endregion Artifact Management

        #region Process Artifact Management

        /// <summary>
        /// Creates and saves a new Nova Process artifact (wrapped inside an ProcessArtifactWrapper that tracks the state of the process artifact.).
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="project">The project where the Nova artifact should be created.</param>
        /// <param name="parentId">(optional) The parent ID of this Nova artifact.
        ///     By default the parent should be the project.</param>
        /// <param name="orderIndex">(optional) The order index of this Nova artifact.
        ///     By default the order index should be after the last artifact.</param>
        /// <param name="name">(optional) The artifact name.  By default a random name is created.</param>
        /// <param name="artifactTypeName">(optional) Name of the artifact type to be used to create the artifact</param>
        /// <returns>The Nova artifact wrapped in an ProcessArtifactWrapper that tracks the state of the artifact.</returns>
        public ProcessArtifactWrapper CreateNovaProcessArtifact(
            IUser user, IProject project,
            int? parentId = null, double? orderIndex = null, string name = null, string artifactTypeName = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));

            name = name ?? RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);

            var artifact = Model.Impl.ArtifactStore.CreateArtifact(ArtifactStore.Address, user,
                ItemTypePredefined.Process, name, project, artifactTypeName, parentId, orderIndex);

            var process = Storyteller.GetNovaProcess(user, artifact.Id);

            return WrapProcessArtifact(process, project, user);
        }

        /// <summary>
        /// Creates and publish a new Nova Process artifact (wrapped inside an ProcessArtifactWrapper that tracks the state of the artifact.).
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="project">The project where the Nova Process artifact should be created.</param>
        /// <param name="parentId">(optional) The parent of this Nova Process artifact.
        ///     By default the parent should be the project.</param>
        /// <param name="orderIndex">(optional) The order index of this Nova Process artifact.
        ///     By default the order index should be after the last artifact.</param>
        /// <param name="name">(optional) The artifact name.  By default a random name is created.</param>
        /// <returns>The Nova Process artifact wrapped in an ProcessArtifactWrapper that tracks the state of the artifact.</returns>
        public ProcessArtifactWrapper CreateAndPublishNovaProcessArtifact(
            IUser user, IProject project,
            int? parentId = null, double? orderIndex = null, string name = null)
        {
            var wrappedProcessArtifact = CreateNovaProcessArtifact(user, project, parentId, orderIndex, name);
            wrappedProcessArtifact.Publish(user);

            return wrappedProcessArtifact;
        }

        /// <summary>
        /// Wraps an INovaProcess in an ProcessArtifactWrapper and adds it the list of artifacts that get disposed.
        /// </summary>
        /// <param name="novaProcess">The INovaProcess that was created by ArtifactStore.</param>
        /// <param name="project">The project where the artifact was created.</param>
        /// <param name="createdBy">The user that created this artifact.</param>
        /// <returns>The ProcessArtifactWrapper for the novaProcessArtifact.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]    // No, it MUST be an INovaProcess!
        public ProcessArtifactWrapper WrapProcessArtifact(INovaProcess novaProcess, IProject project, IUser createdBy)
        {
            ThrowIf.ArgumentNull(novaProcess, nameof(novaProcess));

            var wrappedProcessArtifact = (ProcessArtifactWrapper) ArtifactWrapperFactory.CreateArtifactWrapper(novaProcess, project, createdBy);
            WrappedArtifactsToDispose.Add(wrappedProcessArtifact);

            return wrappedProcessArtifact;
        }

        #endregion Process Artifact Management

        #region Nova Artifact Management
        public enum TestArtifactState
        {
            Created = 0,
            Published = 1,
            PublishedWithDraft = 2,
            ScheduledToDelete = 3,
            Deleted = 4
        }

        /// <summary>
        /// Creates artifact in the specified state
        /// </summary>
        /// <param name="user">User to perform operation</param>
        /// <param name="project">Project in which artifact will be created</param>
        /// <param name="state">State of the artifact(Created, Published, PublishedWithDraft, ScheduledToDelete, Deleted)</param>
        /// <param name="itemType">itemType of artifact to be created</param>
        /// <param name="parentId">Parent Id of artifact to be created</param>
        /// <param name="artifactName">(optional) The name to assign to the new artifact.  By default a random name is assigned.</param>
        /// <returns>Artifact in the required state</returns>
        public ArtifactWrapper CreateNovaArtifactInSpecificState(IUser user, IProject project, TestArtifactState state,
            ItemTypePredefined itemType, int parentId, string artifactName = null)
        {
            artifactName = artifactName ?? RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);
            var artifact = CreateNovaArtifact(user, project, itemType, parentId, name: artifactName);

            if (NovaArtifacts.ContainsKey(user))
            {
                if (NovaArtifacts[user] == null)
                {
                    NovaArtifacts[user] = new List<int> { artifact.Id };
                }
                else
                {
                    NovaArtifacts[user].Add(artifact.Id);
                }
            }
            else
            {
                NovaArtifacts.Add(user, new List<int> { artifact.Id });
            }

            switch (state)
            {
                case TestArtifactState.Created:
                    break;
                case TestArtifactState.Published:
                    artifact.SaveWithNewDescription(user, PublishedDescription);
                    artifact.Publish(user);
                    artifact.RefreshArtifactFromServer(user);
                    break;
                case TestArtifactState.PublishedWithDraft:
                    artifact.Publish(user);
                    artifact.Lock(user);
                    artifact.SaveWithNewDescription(user, DraftDescription);
                    artifact.RefreshArtifactFromServer(user);
                    break;
                case TestArtifactState.ScheduledToDelete:
                    artifact.Publish(user);
                    artifact.Delete(user);
                    break;
                case TestArtifactState.Deleted:
                    artifact.Publish(user);
                    artifact.Delete(user);
                    artifact.Publish(user);
                    break;
                default:
                    Assert.Fail("Unexpected value of Artifact state");
                    break;
            }

            return artifact;
        }

        /// <summary>
        /// Creates a new collection with artifacts in the specified state.
        /// </summary>
        /// <param name="user">User to perform operation.</param>
        /// <param name="project">Project in which artifact will be created.</param>
        /// <param name="collectionState">State of the collection(Created, Published).</param>
        /// <param name="artifactsIdsToAdd">List of artifact's id to be added.</param>
        /// <returns>Collection in the required state.</returns>
        public Collection CreateUnpublishedCollectionWithArtifactsInSpecificState(
            IUser user,
            IProject project,
            TestArtifactState collectionState,
            List<int> artifactsIdsToAdd)
            // TODO: Refactor Collection to derive from ArtifactWrapper...
        {
            var collectionArtifact = CreateUnpublishedCollection(project, user);
            var collection = ArtifactStore.GetCollection(user, collectionArtifact.Id);

            collection.UpdateArtifacts(artifactsIdsToAdd: artifactsIdsToAdd);
            collectionArtifact.Update(user, collection);
            collection = ArtifactStore.GetCollection(user, collectionArtifact.Id);

            switch (collectionState)
            {
                case TestArtifactState.Created:
                    return collection;
                case TestArtifactState.Published:
                    ArtifactStore.PublishArtifacts(new List<int> { collection.Id }, user);
                    return ArtifactStore.GetCollection(user, collectionArtifact.Id);
                default:
                    Assert.Fail("Unexpected value of Collection state");
                    return collection;
            }
        }

        #endregion Nova Artifact Management

        #region Project Management

        /// <summary>
        /// Creates a new project object with the values specified, or with random values for any unspecified parameters.
        /// </summary>
        /// <param name="name">The name of the project.</param>
        /// <param name="description">(optional) The description of the project.</param>
        /// <param name="location">(optional) The location of the project.</param>
        /// <param name="id">(optional) Internal database identifier.  Only set this if you read the project from the database.</param>
        /// <returns>The new project object.</returns>
        public IProject CreateProject(string name = null, string description = null, string location = null, int id = 0)
        {
            var project = ProjectFactory.CreateProject(name, description, location, id);
            Projects.Add(project);
            return project;
        }

        /// <summary>
        /// Gets a project by name
        /// </summary>
        /// <param name="projectName">The project to retrieve</param>
        /// <param name="user">The user getting the project.</param>
        /// <returns>The retrieved project.</returns>
        public IProject GetProject(string projectName, IUser user)
        {
            var allProjects = ProjectFactory.GetAllProjects(user);

            Assert.That(allProjects.Exists(p => (p.Name == projectName)),
                "No project was found named '{0}'!", projectName);

            var project = allProjects.First(p => (p.Name == projectName));
            project.GetAllNovaArtifactTypes(ArtifactStore, user);
            project.GetAllOpenApiArtifactTypes(ProjectFactory.Address, user);

            return project;
        }

        #endregion Project Management

        #region User management

        /// <summary>
        /// Creates a new user object with random values and adds it to the Blueprint database.
        /// </summary>
        /// <param name="instanceAdminRole">(optional) The Instance Admin Role to assign to the user.  Pass null if you don't want any role assigned.</param>
        /// <param name="source">(optional) Where the user exists.</param>
        /// <param name="licenseLevel">(optional) The license level of the user (Author, Collaborator, Viewer).</param>
        /// <returns>A new unique user object that was added to the database.</returns>
        public IUser CreateUserAndAddToDatabase(
            InstanceAdminRole? instanceAdminRole = InstanceAdminRole.DefaultInstanceAdministrator,
            UserSource source = UserSource.Database,
            LicenseLevel licenseLevel = LicenseLevel.Author)
        {
            var user = UserFactory.CreateUserAndAddToDatabase(instanceAdminRole, source, licenseLevel);
            Users.Add(user);
            return user;
        }

        /// <summary>
        /// Creates a new user object with random values, but with the username & password specified
        /// and adds it to the Blueprint database.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="instanceAdminRole">(optional) The Instance Admin Role to assign to the user.  Pass null if you don't want any role assigned.</param>
        /// <param name="source">(optional) Where the user exists.</param>
        /// <returns>A new user object.</returns>
        public IUser CreateUserAndAddToDatabase(string username, string password,
            InstanceAdminRole? instanceAdminRole = InstanceAdminRole.DefaultInstanceAdministrator,
            UserSource source = UserSource.Database)
        {
            var user = UserFactory.CreateUserAndAddToDatabase(username, password, instanceAdminRole, source);
            Users.Add(user);
            return user;
        }

        /// <summary>
        /// Creates a new user object with random values, but with the username, password, and displayname specified
        /// and adds it to the Blueprint database.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="displayname">The displayname.</param>
        /// <param name="instanceAdminRole">(optional) The Instance Admin Role to assign to the user.  Pass null if you don't want any role assigned.</param>
        /// <param name="source">(optional) Where the user exists.</param>
        /// <returns>A new user object.</returns>
        public IUser CreateUserAndAddToDatabase(string username, string password, string displayname,
            InstanceAdminRole? instanceAdminRole = InstanceAdminRole.DefaultInstanceAdministrator,
            UserSource source = UserSource.Database)
        {
            var user = UserFactory.CreateUserAndAddToDatabase(username, password, displayname, instanceAdminRole, source);
            Users.Add(user);
            return user;
        }

        /// <summary>
        /// Used to specify which type of session tokens to get for the user.
        /// </summary>
        [Flags]
        public enum AuthenticationTokenTypes
        {
            None = 0,
            AccessControlToken = 1,
            OpenApiToken = 2,
            BothAccessControlAndOpenApiTokens = 3
        }

        /// <summary>
        /// Creates a new user object with random values and adds it to the Blueprint database,
        /// then authenticates to AdminStore and/or OpenApi to get session tokens.
        /// </summary>
        /// <param name="targets">The authentication targets.</param>
        /// <param name="instanceAdminRole">(optional) The Instance Admin Role to assign to the user.  Pass null if you don't want any role assigned.</param>
        /// <param name="source">(optional) Where the user exists.</param>
        /// <returns>A new user that has the requested access tokens.</returns>
        public IUser CreateUserAndAuthenticate(AuthenticationTokenTypes targets,
            InstanceAdminRole? instanceAdminRole = InstanceAdminRole.DefaultInstanceAdministrator,
            UserSource source = UserSource.Database)
        {
            var user = CreateUserAndAddToDatabase(instanceAdminRole, source);

            if ((targets & AuthenticationTokenTypes.AccessControlToken) != 0)
            {
                AdminStore.AddSession(user);
                Assert.NotNull(user.Token?.AccessControlToken, "User '{0}' didn't get an AccessControl token!", user.Username);
            }

            if ((targets & AuthenticationTokenTypes.OpenApiToken) != 0)
            {
                BlueprintServer.LoginUsingBasicAuthorization(user);
                Assert.NotNull(user.Token?.OpenApiToken, "User '{0}' didn't get an OpenAPI token!", user.Username);
            }

            return user;
        }

        /// <summary>
        /// Creates a new user object with random values and adds it to the Blueprint database,
        /// then assigns random fake AdminStore and/or OpenAPI tokens.
        /// </summary>
        /// <param name="targets">The authentication targets.</param>
        /// <param name="instanceAdminRole">(optional) The Instance Admin Role to assign to the user.  Pass null if you don't want any role assigned.</param>
        /// <param name="source">(optional) Where the user exists.</param>
        /// <param name="badToken">(optional) The invalid token to set.  Defaults to a token GUID that shouldn't exist.</param>
        /// <returns>A new user that has the requested access tokens.</returns>
        public IUser CreateUserWithInvalidToken(AuthenticationTokenTypes targets,
            InstanceAdminRole? instanceAdminRole = InstanceAdminRole.DefaultInstanceAdministrator,
            UserSource source = UserSource.Database,
            string badToken = CommonConstants.InvalidToken)
        {
            var user = CreateUserAndAddToDatabase(instanceAdminRole, source);

            if (badToken != null)
            {
                if ((targets & AuthenticationTokenTypes.AccessControlToken) != 0)
                {
                    user.SetToken(badToken);
                }

                if ((targets & AuthenticationTokenTypes.OpenApiToken) != 0)
                {
                    user.SetToken(I18NHelper.FormatInvariant("{0} {1}",
                        BlueprintToken.OPENAPI_START_OF_TOKEN, badToken));
                }
            }

            return user;
        }

        /// <summary>
        /// Creates a user with project role permissions for one or more projects.
        /// </summary>
        /// <param name="role">Author, Viewer or No permission role</param>
        /// <param name="projects">The list of projects that the role is created for</param>
        /// <param name="artifact">(optional) Specific artifact to apply permissions to instead of project-wide</param>
        /// <param name="licenseType">(optional) The type of user license (Author, Collaborator, Viewer).</param>
        /// <returns>Created authenticated user with required premissions</returns>
        public IUser CreateUserWithProjectRolePermissions(
            ProjectRole role, 
            List<IProject> projects, 
            IArtifactBase artifact = null,
            GroupLicenseType licenseType = GroupLicenseType.Author)
        {
            ThrowIf.ArgumentNull(projects, nameof(projects));

            Logger.WriteTrace("{0}.{1} called.", nameof(TestHelper), nameof(CreateUserWithProjectRolePermissions));

            var newUser = CreateUserAndAddToDatabase(instanceAdminRole: null, licenseLevel : _groupLicenseTypeToLicenseLevel[licenseType]);

            foreach (var project in projects)
            {
                AssignProjectRolePermissionsToUser(newUser, role, project, artifact, licenseType);
            }

            AdminStore.AddSession(newUser);//assign premission and after it authenticate, reverse doesn't work - need to investigate!
            BlueprintServer.LoginUsingBasicAuthorization(newUser);

            Logger.WriteInfo("User {0} created.", newUser.Username);

            Logger.WriteTrace("{0}.{1} finished.", nameof(TestHelper), nameof(CreateUserWithProjectRolePermissions));

            return newUser;
        }

        /// <summary>
        /// Creates a user with project role permissions for one or more projects.
        /// </summary>
        /// <param name="role">Author, Viewer or No permission role.</param>
        /// <param name="projects">The list of projects that the role is created for.</param>
        /// <param name="artifact">Specific artifact to apply permissions to instead of project-wide.</param>
        /// <param name="licenseType">(optional) The type of user license (Author, Collaborator, Viewer).</param>
        /// <returns>Created authenticated user with required premissions.</returns>
        public IUser CreateUserWithProjectRolePermissions(
            ProjectRole role,
            List<IProject> projects,
            INovaArtifactDetails artifact,
            GroupLicenseType licenseType = GroupLicenseType.Author)
        {
            ThrowIf.ArgumentNull(projects, nameof(projects));

            Logger.WriteTrace("{0}.{1} called.", nameof(TestHelper), nameof(CreateUserWithProjectRolePermissions));

            var newUser = CreateUserAndAddToDatabase(instanceAdminRole: null, licenseLevel: _groupLicenseTypeToLicenseLevel[licenseType]);

            foreach (var project in projects)
            {
                AssignProjectRolePermissionsToUser(newUser, role, project, artifact, licenseType);
            }

            AdminStore.AddSession(newUser);//assign premission and after it authenticate, reverse doesn't work - need to investigate!
            BlueprintServer.LoginUsingBasicAuthorization(newUser);

            Logger.WriteInfo("User {0} created.", newUser.Username);

            Logger.WriteTrace("{0}.{1} finished.", nameof(TestHelper), nameof(CreateUserWithProjectRolePermissions));

            return newUser;
        }

        /// <summary>
        /// Creates a user with project role permissions for the specified project. Optionally, creates role permissions for a single artifact within
        /// a project.
        /// </summary>
        /// <param name="role">Author, Viewer or No permission role</param>
        /// <param name="project">The project that the role is created for</param>
        /// <param name="artifact">(optional) Specific artifact to apply permissions to instead of project-wide</param>
        /// <param name="licenseType">(optional) The type of user license (Author, Collaborator, Viewer).</param>
        /// <returns>Newly created, authenticated user with required premissions</returns>
        public IUser CreateUserWithProjectRolePermissions( 
            ProjectRole role,
            IProject project, 
            IArtifactBase artifact = null,
            GroupLicenseType licenseType = GroupLicenseType.Author)
        {
            ThrowIf.ArgumentNull(project, nameof(project));

            if (artifact != null)
            {
                Assert.IsTrue(artifact.ProjectId == project.Id, "Artifact should belong to the project");
            }

            return CreateUserWithProjectRolePermissions(role, new List<IProject> { project }, artifact, licenseType);
        }

        /// <summary>
        /// Creates a user with project role permissions for the specified project and creates role permissions for a single artifact
        /// within the project.
        /// </summary>
        /// <param name="role">Author, Viewer or No permission role.</param>
        /// <param name="project">The project that the role is created for.</param>
        /// <param name="artifact">Specific artifact to apply permissions to instead of project-wide.</param>
        /// <param name="licenseType">(optional) The type of user license (Author, Collaborator, Viewer).</param>
        /// <returns>Newly created, authenticated user with required premissions.</returns>
        public IUser CreateUserWithProjectRolePermissions(
            ProjectRole role,
            IProject project,
            INovaArtifactDetails artifact,
            GroupLicenseType licenseType = GroupLicenseType.Author)
        {
            ThrowIf.ArgumentNull(project, nameof(project));

            if (artifact != null)
            {
                Assert.IsTrue(artifact.ProjectId == project.Id, "Artifact should belong to the project");
            }

            return CreateUserWithProjectRolePermissions(role, new List<IProject> { project }, artifact, licenseType);
        }

        /// <summary>
        /// Assigns project role permissions to the specified user and gets updated Session-Token.
        /// Optionally, creates role permissions for a single artifact within a project.
        /// </summary>
        /// <param name="user">User to assign role</param>
        /// <param name="role">Author, Viewer or No permission role</param>
        /// <param name="project">The project that the role is created for</param>
        /// <param name="artifact">(optional) Specific artifact to apply permissions to instead of project-wide
        /// after adding a new permissions role</param>
        /// <param name="licenseType">(optional) The type of user license (Author, Collaborator, Viewer).</param>
        public void AssignProjectRolePermissionsToUser(
            IUser user, 
            ProjectRole role, 
            IProject project, 
            IArtifactBase artifact,
            GroupLicenseType licenseType = GroupLicenseType.Author)
        {

            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            Logger.WriteTrace("{0}.{1} called.", nameof(TestHelper), nameof(AssignProjectRolePermissionsToUser));

            if (artifact != null)
            {
                Assert.IsTrue(artifact.ProjectId == project.Id, "Artifact should belong to the project");
            }

            var rolePermissions = GetRolePermissionsForProjectRole(role);

            AssignProjectRolePermissionsToUser(user, rolePermissions, project, artifact, licenseType);
        }

        /// <summary>
        /// Assigns project role permissions to the specified user and gets updated Session-Token.
        /// Optionally, creates role permissions for a single artifact within a project.
        /// </summary>
        /// <param name="user">User to assign role.</param>
        /// <param name="role">Author, Viewer or No permission role.</param>
        /// <param name="project">The project that the role is created for.</param>
        /// <param name="artifact">(optional) Specific artifact to apply permissions to instead of project-wide
        ///     after adding a new permissions role.</param>
        /// <param name="licenseType">(optional) The type of user license (Author, Collaborator, Viewer).</param>
        public void AssignProjectRolePermissionsToUser(
            IUser user,
            ProjectRole role,
            IProject project,
            INovaArtifactDetails artifact = null,
            GroupLicenseType licenseType = GroupLicenseType.Author)
        {

            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            Logger.WriteTrace("{0}.{1} called.", nameof(TestHelper), nameof(AssignProjectRolePermissionsToUser));

            if (artifact != null)
            {
                Assert.IsTrue(artifact.ProjectId == project.Id, "Artifact should belong to the project");
            }

            var rolePermissions = GetRolePermissionsForProjectRole(role);

            AssignProjectRolePermissionsToUser(user, rolePermissions, project, artifact, licenseType);
        }

        /// <summary>
        /// Gets the specific role permissions for the specified Project Role.
        /// </summary>
        /// <param name="role">The Project Role whose permissions you want to get.</param>
        /// <returns>The permissions for the Project Role.</returns>
        public static RolePermissions GetRolePermissionsForProjectRole(ProjectRole role)
        {
            var rolePermissions = RolePermissions.None;

            if (role == ProjectRole.Viewer)
            {
                rolePermissions = RolePermissions.Read;
            }
            else if (role == ProjectRole.AuthorFullAccess)
            {
                rolePermissions = RolePermissions.Delete |
                        RolePermissions.Edit |
                        RolePermissions.CanReport |
                        RolePermissions.Comment |
                        RolePermissions.DeleteAnyComment |
                        RolePermissions.CreateRapidReview |
                        RolePermissions.ExcelUpdate |
                        RolePermissions.Read |
                        RolePermissions.Reuse |
                        RolePermissions.Share |
                        RolePermissions.Trace;
            }
            else if (role == ProjectRole.Collaborator)
            {
                rolePermissions = RolePermissions.Read |
                        RolePermissions.Comment;
            }
            else if (role == ProjectRole.None)
            {
                rolePermissions = RolePermissions.None;
            }
            else if (role == ProjectRole.Author)
            {
                rolePermissions = RolePermissions.Edit |
                        RolePermissions.CanReport |
                        RolePermissions.Comment |
                        RolePermissions.CreateRapidReview |
                        RolePermissions.ExcelUpdate |
                        RolePermissions.Read |
                        RolePermissions.Reuse |
                        RolePermissions.Share |
                        RolePermissions.Trace;
            }

            return rolePermissions;
        }

        /// <summary>
        /// Assigns project role permissions to the specified user and gets updated Session-Token.
        /// Optionally, creates role permissions for a single artifact within a project.
        /// </summary>
        /// <param name="user">User to assign role.</param>
        /// <param name="rolePermissions">Role permissions.</param>
        /// <param name="project">The project that the role is created for.</param>
        /// <param name="artifact">Specific artifact to apply permissions to instead of project-wide
        ///     after adding a new permissions role.</param>
        /// <param name="licenseType">(optional) The type of user license (Author, Collaborator, Viewer).</param>
        public void AssignProjectRolePermissionsToUser(
            IUser user, 
            RolePermissions rolePermissions, 
            IProject project, 
            IArtifactBase artifact,
            GroupLicenseType licenseType = GroupLicenseType.Author)
        {
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            Logger.WriteTrace("{0}.{1} called.", nameof(TestHelper), nameof(AssignProjectRolePermissionsToUser));

            if (artifact != null)
            {
                Assert.IsTrue(artifact.ProjectId == project.Id, "Artifact should belong to the project");
            }

            IProjectRole projectRole = null;

            projectRole = ProjectRoleFactory.CreateProjectRole(
                        project, rolePermissions,
                        rolePermissions.ToString());

            if (projectRole != null)
            {
                ProjectRoles.Add(projectRole);
            }

            var permissionsGroup = CreateGroupAndAddToDatabase(licenseType);
            permissionsGroup.AddUser(user);
            permissionsGroup.AssignRoleToProjectOrArtifact(project, role: projectRole, artifact: artifact);

            Logger.WriteInfo("User {0} created.", user.Username);
            Logger.WriteTrace("{0}.{1} finished.", nameof(TestHelper), nameof(AssignProjectRolePermissionsToUser));
        }

        /// <summary>
        /// Assigns project role permissions to the specified user and gets updated Session-Token.
        /// Optionally, creates role permissions for a single artifact within a project.
        /// </summary>
        /// <param name="user">User to assign role</param>
        /// <param name="rolePermissions">Role permissions.</param>
        /// <param name="project">The project that the role is created for</param>
        /// <param name="artifact">(optional) Specific artifact to apply permissions to instead of project-wide
        /// after adding a new permissions role</param>
        /// <param name="licenseType">(optional) The type of user license (Author, Collaborator, Viewer).</param>
        public void AssignProjectRolePermissionsToUser(
            IUser user,
            RolePermissions rolePermissions,
            IProject project,
            INovaArtifactDetails artifact = null,
            GroupLicenseType licenseType = GroupLicenseType.Author)
        {
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            Logger.WriteTrace("{0}.{1} called.", nameof(TestHelper), nameof(AssignProjectRolePermissionsToUser));

            if (artifact != null)
            {
                Assert.IsTrue(artifact.ProjectId == project.Id, "Artifact should belong to the project");
            }

            IProjectRole projectRole = null;

            projectRole = ProjectRoleFactory.CreateProjectRole(
                        project, rolePermissions,
                        rolePermissions.ToString());

            if (projectRole != null)
            {
                ProjectRoles.Add(projectRole);
            }

            var permissionsGroup = CreateGroupAndAddToDatabase(licenseType);
            permissionsGroup.AddUser(user);
            permissionsGroup.AssignRoleToProjectOrNovaArtifact(project, role: projectRole, artifact: artifact);

            Logger.WriteInfo("User {0} created.", user.Username);
            Logger.WriteTrace("{0}.{1} finished.", nameof(TestHelper), nameof(AssignProjectRolePermissionsToUser));
        }

        /// <summary>
        /// Assigns project role permissions to the specified user and gets updated Session-Token.
        /// Optionally, creates role permissions for a single artifact within a project.
        /// </summary>
        /// <param name="user">User to assign role</param>
        /// <param name="rolePermissions">Role permissions.</param>
        /// <param name="project">The project that the role is created for</param>
        /// <param name="artifact">(optional) Specific artifact to apply permissions to instead of project-wide
        /// after adding a new permissions role</param>
        public void AssignNovaProjectRolePermissionsToUser(IUser user, RolePermissions rolePermissions, IProject project, INovaArtifactDetails artifact = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));
            if (artifact != null)
            {
                Assert.IsTrue(artifact.ProjectId == project.Id, "Artifact should belong to the project");
            }

            Logger.WriteTrace("{0}.{1} called.", nameof(TestHelper), nameof(AssignProjectRolePermissionsToUser));

            IProjectRole projectRole = null;

            projectRole = ProjectRoleFactory.CreateProjectRole(
                        project, rolePermissions,
                        rolePermissions.ToString());

            if (projectRole != null)
            {
                ProjectRoles.Add(projectRole);
            }

            var licenseType = GroupLicenseType.Author;
            var permissionsGroup = CreateGroupAndAddToDatabase(licenseType);
            permissionsGroup.AddUser(user);
            permissionsGroup.AssignRoleToProjectOrNovaArtifact(project, projectRole, artifact);

            Logger.WriteInfo("User {0} created.", user.Username);

            Logger.WriteTrace("{0}.{1} finished.", nameof(TestHelper), nameof(AssignProjectRolePermissionsToUser));
        }

        /// <summary>
        /// Creates and Adds an Instance User
        /// </summary>
        /// <param name="adminUser">The admin user creating the instance user.</param>
        /// <param name="login">(optional) The login name of the user. Randomly generated if not specified.</param>
        /// <param name="firstName">(optional) The first name of the user. Randomly generated if not specified.</param>
        /// <param name="lastName">(optional) The last name of the user. Randomly generated if not specified.</param>
        /// <param name="email">(optional) The email of the user. Randomly generated if not specified.</param>
        /// <param name="displayname">(optional) The user's display name. Randomly generated if not specified.</param>
        /// <param name="source">(optional) The source of the user. Defaults to Database if not specified.</param>
        /// <param name="licenseLevel">(optinal) The user's license level. Defaults to Viewer if not specified.</param>
        /// <param name="instanceAdminRole">(optional) The user's instance admin role. Defaults to null (empty).</param>
        /// <param name="adminPrivileges">(optional) The user's instance admin privileges.  Defaults to None.</param>
        /// <param name="imageId">(optional) The user's image id. Defaults to null.</param>
        /// <param name="password">(optional) The user's password. Randomly generated if not specified.</param>
        /// <returns>The created instance user.</returns>
        public InstanceUser CreateAndAddInstanceUser(IUser adminUser, 
            string login = null,
            string firstName = null,
            string lastName = null,
            string email = null,
            string displayname = null,
            UserSource? source = null,
            LicenseLevel? licenseLevel = null,
            InstanceAdminRole? instanceAdminRole = null,
            InstanceAdminPrivileges? adminPrivileges = null,
            int? imageId = null,
            string password = null)
        {
            var createdUser = AdminStoreHelper.GenerateRandomInstanceUser(login, firstName, lastName, email, displayname, source, licenseLevel,
                instanceAdminRole, adminPrivileges, imageId, password);

            int createdUserId = 0;

            Assert.DoesNotThrow(() =>
            {
                createdUserId = AdminStore.AddUser(adminUser, createdUser);
            }, "'POST {0}' should return 201 OK for a valid session token!", RestPaths.Svc.AdminStore.Users.USERS);

            Assert.IsNotNull(createdUserId, "The returned user id is null!");

            AdminStoreHelper.UpdateUserIdAndIncrementCurrentVersion(createdUser, createdUserId);

            InstanceUsers.Add(createdUser);

            return createdUser;
        }

        /// <summary>
        /// Creates and Adds a number of Instance Users
        /// </summary>
        /// <param name="adminUser">The admin user creating the instance user.</param>
        /// <param name="numberOfUsers">The number of users to create.</param>
        /// <returns>A list of created instance users.</returns>
        public List<InstanceUser> CreateAndAddInstanceUsers(IUser adminUser, int numberOfUsers)
        {
            List<InstanceUser> createdUsers = new List<InstanceUser>();

            for (int i = 0; i < numberOfUsers; i++)
            {
                createdUsers.Add(CreateAndAddInstanceUser(adminUser));
            }

            return createdUsers;
        }

        /// <summary>
        /// Deletes all Instance Users in instance users list
        /// </summary>
        /// <param name="adminUser">The admin user deleting the instance users.</param>
        public void DeleteInstanceUsers(IUser adminUser)
        {
            var ids = InstanceUsers.Where(u => u.Id != null).Select(u => u.Id.Value).ToList();

            AdminStore.DeleteUsers(adminUser, ids);

            InstanceUsers.RemoveAll(user => user.Id != null && ids.Contains(user.Id.Value));
        }

        /// <summary>
        /// Gets all non-deleted Instance Users in the users table
        /// </summary>
        /// <param name="adminUser">The admin user getting the instance users.</param>
        /// <returns>A list of all current users</returns>
        public List<InstanceUser> GetCurrentUsers(IUser adminUser)
        {
            var queryResult = AdminStore.GetUsers(adminUser, offset: 0, limit: int.MaxValue);

            return queryResult.Items.ToList();
        }

        #endregion User management

        #region Group management
        /// <summary>
        /// Creates a new group object with random values and adds it to the Blueprint database.
        /// </summary>
        /// <param name="licenseType">(optional) The license level to assign to the group. By default there is no license.</param>
        /// <returns>A new unique group object that was added to the database.</returns>
        public IGroup CreateGroupAndAddToDatabase(GroupLicenseType? licenseType = null)
        {
            var group = GroupFactory.CreateGroup(licenseType);
            group.AddGroupToDatabase();
            Groups.Add(group);
            return group;
        }
        #endregion Group management

        #region Job Management

        /// <summary>
        /// Create ALM Change Summary Jobs as a setup for testing Nova GET Jobs API calls
        /// </summary>
        /// <param name="address">address for Blueprint application server</param>
        /// <param name="user">user for authentication</param>
        /// <param name="baselineOrReviewId">The baseline or review artifact ID.</param>
        /// <param name="numberOfJobsToBeCreated">The number of ALM Change Summary Jobs to be created.</param>
        /// <param name="project">The project where ALM targets reside.</param>
        /// <returns> List of ALM Summary Jobs created in decending order by jobId </returns>
        public List<IOpenAPIJob> CreateALMSummaryJobsSetup(string address, IUser user, int baselineOrReviewId, int numberOfJobsToBeCreated, IProject project)
        {
            ThrowIf.ArgumentNull(project, nameof(project));

            var almTarget = AlmTarget.GetAlmTargets(address, user, project).First();
            Assert.IsNotNull(almTarget, "ALM target does not exist on the project {0}!", project.Name);

            var jobsToBeFound = new List<IOpenAPIJob>();

            for (int i = 0; i < numberOfJobsToBeCreated; i++)
            {
                var openAPIJob = OpenApi.AddAlmChangeSummaryJob(user, project, baselineOrReviewId, almTarget);
                jobsToBeFound.Add(openAPIJob);
            }

            jobsToBeFound.Reverse();
            return jobsToBeFound;
        }

        #endregion Job Management

        #region Database Settings Management

        /// <summary>
        /// Gets the value of the specified Application Setting from the database.
        /// </summary>
        /// <param name="key">The Application Setting key name.</param>
        /// <returns>The value for the specified key.</returns>
        /// <exception cref="SqlQueryFailedException">If the SQL query failed.</exception>
        public static string GetApplicationSetting(string key)
        {
            string selectQuery = I18NHelper.FormatInvariant("SELECT Value FROM [dbo].[ApplicationSettings] WHERE [ApplicationSettings].[Key] ='{0}'", key);

            return DatabaseHelper.ExecuteSingleValueSqlQuery<string>(selectQuery, "Value");
        }

        /// <summary>
        /// Updates the specified database Application Setting with a new value.
        /// </summary>
        /// <param name="key">The Application Setting key name.</param>
        /// <param name="value">The new value to set.</param>
        /// <exception cref="SqlQueryFailedException">If the SQL query failed.</exception>
        public static void UpdateApplicationSettings(string key, string value)
        {
            string updateQuery = I18NHelper.FormatInvariant("UPDATE [dbo].[ApplicationSettings] SET Value = '{0}' WHERE [ApplicationSettings].[Key] ='{1}'", value, key);

            using (var database = DatabaseFactory.CreateDatabase())
            {
                database.Open();
                string query = updateQuery;

                Logger.WriteDebug("Running: {0}", query);

                using (var cmd = database.CreateSqlCommand(query))
                using (var sqlDataReader = cmd.ExecuteReader())
                {
                    if (sqlDataReader.RecordsAffected <= 0)
                    {
                        throw new SqlQueryFailedException(I18NHelper.FormatInvariant("No rows were updated when running: {0}", query));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the specific column value of the Instances from the database.
        /// </summary>
        /// <param name="column">The Instances column name.</param>
        /// <returns>The value for the specified column.</returns>
        /// <exception cref="SqlQueryFailedException">If the SQL query failed.</exception>
        public static string GetValueFromInstancesTable(string column)
        {
            string selectQuery = I18NHelper.FormatInvariant("SELECT {0} FROM [dbo].[Instances]", column);

            return DatabaseHelper.ExecuteSingleValueSqlQuery<string>(selectQuery, column);
        }

        /// <summary>
        /// Updates the specific Instances column with a new value.
        /// </summary>
        /// <param name="column">The Instances column name.</param>
        /// <param name="value">The new value to set.</param>
        /// <exception cref="SqlQueryFailedException">If the SQL query failed.</exception>
        public static void UpdateValueFromInstancesTable(string column, string value)
        {
            string updateQuery = I18NHelper.FormatInvariant("UPDATE [dbo].[Instances] SET {0} = {1}", column, value);

            using (var database = DatabaseFactory.CreateDatabase())
            {
                database.Open();
                string query = updateQuery;

                Logger.WriteDebug("Running: {0}", query);

                using (var cmd = database.CreateSqlCommand(query))
                using (var sqlDataReader = cmd.ExecuteReader())
                {
                    if (sqlDataReader.RecordsAffected <= 0)
                    {
                        throw new SqlQueryFailedException(I18NHelper.FormatInvariant("No rows were updated when running: {0}", query));
                    }
                }
            }
        }

        #endregion Database Settings Management

        #region Custom Asserts

        /// <summary>
        /// Asserts that both artifacts are identical.
        /// </summary>
        /// <param name="firstArtifact">The first artifact to compare.</param>
        /// <param name="secondArtifact">The second artifact to compare.</param>
        /// <param name="compareBlueprintUrls">(optional) Pass true if you want to also compare the BlueprintUrl properties of the artifacts.</param>
        /// <param name="compareVersions">(optional) Pass true if you want to also compare the Version properties of the artifacts.</param>
        public static void AssertArtifactsAreEqual(IArtifactBase firstArtifact,
            IArtifactBase secondArtifact,
            bool compareBlueprintUrls = false,
            bool compareVersions = false)
        {
            ThrowIf.ArgumentNull(firstArtifact, nameof(firstArtifact));
            ThrowIf.ArgumentNull(secondArtifact, nameof(secondArtifact));

            Assert.AreEqual(firstArtifact.AreAttachmentsReadOnly, secondArtifact.AreAttachmentsReadOnly,
                "First artifact AreAttachmentsReadOnly: '{0}' doesn't match second artifact AreAttachmentsReadOnly: '{1}'",
                firstArtifact.AreAttachmentsReadOnly, secondArtifact.AreAttachmentsReadOnly);
            Assert.AreEqual(firstArtifact.AreDocumentReferencesReadOnly, secondArtifact.AreDocumentReferencesReadOnly,
                "First artifact AreDocumentReferencesReadOnly: '{0}' doesn't match second artifact AreDocumentReferencesReadOnly: '{1}'",
                firstArtifact.AreDocumentReferencesReadOnly, secondArtifact.AreDocumentReferencesReadOnly);
            Assert.AreEqual(firstArtifact.AreTracesReadOnly, secondArtifact.AreTracesReadOnly,
                "First artifact AreTracesReadOnly: '{0}' doesn't match second artifact AreTracesReadOnly: '{1}'",
                firstArtifact.AreTracesReadOnly, secondArtifact.AreTracesReadOnly);
            Assert.AreEqual(firstArtifact.ArtifactTypeId, secondArtifact.ArtifactTypeId,
                "First artifact ArtifactTypeId: '{0}' doesn't match second artifact ArtifactTypeId: '{1}'",
                firstArtifact.ArtifactTypeId, secondArtifact.ArtifactTypeId);
            Assert.AreEqual(firstArtifact.ArtifactTypeName, secondArtifact.ArtifactTypeName,
                "First artifact ArtifactTypeName: '{0}' doesn't match second artifact ArtifactTypeName: '{1}'",
                firstArtifact.ArtifactTypeName, secondArtifact.ArtifactTypeName);
            Assert.AreEqual(firstArtifact.BaseArtifactType, secondArtifact.BaseArtifactType,
                "First artifact BaseArtifactType: '{0}' doesn't match second artifact BaseArtifactType: '{1}'",
                firstArtifact.BaseArtifactType, secondArtifact.BaseArtifactType);

            if (compareBlueprintUrls)
            {
                Assert.AreEqual(firstArtifact.BlueprintUrl, secondArtifact.BlueprintUrl,
                    "First artifact BlueprintUrl: '{0}' doesn't match second artifact BlueprintUrl: '{1}'",
                    firstArtifact.BlueprintUrl, secondArtifact.BlueprintUrl);
            }

            Assert.AreEqual(firstArtifact.Id, secondArtifact.Id,
                "First artifact ID: '{0}' doesn't match second artifact ID: '{1}'",
                firstArtifact.Id, secondArtifact.Id);
            Assert.AreEqual(firstArtifact.Name, secondArtifact.Name,
                "First artifact Name: '{0}' doesn't match second artifact Name: '{1}'",
                firstArtifact.Name, secondArtifact.Name);
            Assert.AreEqual(firstArtifact.ProjectId, secondArtifact.ProjectId,
                "First artifact ProjectId: '{0}' doesn't match second artifact ProjectId: '{1}'",
                firstArtifact.ProjectId, secondArtifact.ProjectId);

            if (compareVersions)
            {
                Assert.AreEqual(firstArtifact.Version, secondArtifact.Version,
                    "First artifact Version: '{0}' doesn't match second artifact Version: '{1}'",
                    firstArtifact.Version, secondArtifact.Version);
            }
        }

        /// <summary>
        /// Asserts that the REST response body is empty.
        /// </summary>
        /// <param name="response">The REST response.</param>
        public static void AssertResponseBodyIsEmpty(RestResponse response)
        {
            ThrowIf.ArgumentNull(response, nameof(response));

            Assert.IsEmpty(response.Content, "The REST response body should be empty, but it contains: '{0}'", response.Content);
        }

        /// <summary>
        /// Asserts that the two dates are equal.  Before comparing, this function will convert the dates to UTC time.
        /// </summary>
        /// <param name="firstDate">The first date to compare.</param>
        /// <param name="secondDate">The second date to compare.</param>
        /// <param name="message">The assert message to display if the dates are different.</param>
        public static void AssertUtcDatesAreEqual(DateTime firstDate, DateTime secondDate, string message)
        {
            Assert.AreEqual(firstDate.ToUniversalTime(), secondDate.ToUniversalTime(), message);
        }

        /// <summary>
        /// Asserts that the two dates are equal.  Before comparing, this function will convert the dates to UTC time.
        /// </summary>
        /// <param name="firstDate">The first date to compare.</param>
        /// <param name="secondDateStr">The second date (as a string) to compare.</param>
        /// <param name="message">The assert message to display if the dates are different.</param>
        public static void AssertUtcDatesAreEqual(DateTime firstDate, string secondDateStr, string message)
        {
            DateTime secondDate = DateTime.Parse(secondDateStr, CultureInfo.InvariantCulture);
            AssertUtcDatesAreEqual(firstDate, secondDate, message);
        }

        /// <summary>
        /// Asserts that the two dates are equal.  Before comparing, this function will convert the dates to UTC time.
        /// </summary>
        /// <param name="firstDateStr">The first date (as a string) to compare.</param>
        /// <param name="secondDateStr">The second date (as a string) to compare.</param>
        /// <param name="message">The assert message to display if the dates are different.</param>
        public static void AssertUtcDatesAreEqual(string firstDateStr, string secondDateStr, string message)
        {
            DateTime firstDate = DateTime.Parse(firstDateStr, CultureInfo.InvariantCulture);
            DateTime secondDate = DateTime.Parse(secondDateStr, CultureInfo.InvariantCulture);
            AssertUtcDatesAreEqual(firstDate, secondDate, message);
        }

        /// <summary>
        /// Verifies that the content returned in the rest response contains the specified ErrorCode and Message.
        /// </summary>
        /// <param name="restResponse">The RestResponse that was returned.</param>
        /// <param name="expectedErrorCode">The expected error code.</param>
        /// <param name="expectedErrorMessage">The expected error message.</param>
        public static void ValidateServiceError(RestResponse restResponse, int expectedErrorCode, string expectedErrorMessage)
        {
            IServiceErrorMessage serviceError = null;

            Assert.DoesNotThrow(() =>
            {
                serviceError = JsonConvert.DeserializeObject<ServiceErrorMessage>(restResponse.Content);
            }, "Failed to deserialize the content of the REST response into a ServiceErrorMessage object!");

            var expectedError = ServiceErrorMessageFactory.CreateServiceErrorMessage(
                expectedErrorCode,
                expectedErrorMessage);

            serviceError.AssertEquals(expectedError);
        }

        /// <summary>
        /// Verifies that the content returned in the rest response contains the specified Message.
        /// </summary>
        /// <param name="restResponse">The RestResponse that was returned.</param>
        /// <param name="expectedErrorMessage">The expected error message.</param>
        public static void ValidateServiceError(RestResponse restResponse, string expectedErrorMessage)
        {
            string errorMessage = null;

            Assert.DoesNotThrow(() =>
            {
                errorMessage = JsonConvert.DeserializeObject<string>(restResponse.Content);
            }, "Failed to deserialize the content of the REST response into a string!");

            Assert.AreEqual(expectedErrorMessage, errorMessage, "The error message received doesn't match what we expected!");
        }

        /// <summary>
        /// Verifies that the content returned in the rest response contains the specified ProcessValidationError.
        /// </summary>
        /// <param name="restResponse">The RestResponse that was returned.</param>
        /// <param name="expectedErrorCode">The expected error code.</param>
        /// <param name="expectedErrorMessage">The expected error message.</param>
        /// <param name="expectedInvalidShapeIds">The expected list of shape's ids.</param>
        public static void ValidateProcessValidationError(RestResponse restResponse, int expectedErrorCode,
            string expectedErrorMessage, List<int> expectedInvalidShapeIds)
        {
            ThrowIf.ArgumentNull(expectedInvalidShapeIds, nameof(expectedInvalidShapeIds));
            ProcessValidationError processValidationError = null;

            Assert.DoesNotThrow(() =>
            {
                processValidationError = JsonConvert.DeserializeObject<ProcessValidationError>(restResponse.Content);
            }, "Failed to deserialize the content of the REST response into a ProcessValidationError object!");

            var expectedError = ServiceErrorMessageFactory.CreateServiceErrorMessage(
                expectedErrorCode,
                expectedErrorMessage);

            var actualError = ServiceErrorMessageFactory.CreateServiceErrorMessage(
                processValidationError.ErrorCode,
                processValidationError.Message);
            actualError.AssertEquals(expectedError);

            Assert.AreEqual(expectedInvalidShapeIds.Count, processValidationError.ErrorContent.Count,
                "Number of invalid shapes should have expected value.");
            foreach (int id in expectedInvalidShapeIds)
            {
                Assert.True(processValidationError.ErrorContent.Exists(shapeId => shapeId == id),
                    "List of invalid shapes id should contain expected values.");
            }
        }

        /// <summary>
        /// Verifies that the content returned in the rest response does not contain a stack trace.
        /// </summary>
        /// <param name="restResponse">The RestResponse that was returned.</param>
        public static void ValidateNoStackTraceInResponse(RestResponse restResponse)
        {
            ThrowIf.ArgumentNull(restResponse, nameof(restResponse));

            Assert.False(restResponse.Content.Contains("BluePrintSys."), 
                "The REST response received appears to contain a stack trace!\nActual REST Content: {0}",
                restResponse.Content);
        }

        /// <summary>
        /// Verifies that the error message returned in the REST response equals the expected message.
        /// Ex. 'message="Expected error message"'
        /// </summary>
        /// <param name="restResponse">The RestResponse that was returned.</param>
        /// <param name="expectedErrorMessage">The expected error message</param>
        public static void ValidateServiceErrorMessage(RestResponse restResponse, string expectedErrorMessage)
        {
            ValidateNoStackTraceInResponse(restResponse);

            MessageResult errorMessage = null;

            Assert.DoesNotThrow(() =>
            {
                errorMessage = JsonConvert.DeserializeObject<MessageResult>(restResponse.Content);
            }, "Failed to deserialize the content of the REST response into a MessageResult object!");

            Assert.AreEqual(expectedErrorMessage, errorMessage.Message,
                "The error message does not contain the expected error message!\nActual Error Message: {0}",
                errorMessage.Message);
        }

        /// <summary>
        /// Verifies that the body returned in the REST response equals the expected string.
        /// Ex. '"Unauthorized call"'
        /// </summary>
        /// <param name="restResponse">The RestResponse that was returned.</param>
        /// <param name="expectedBody">The expected body contents.</param>
        public static void ValidateBodyContents(RestResponse restResponse, string expectedBody)
        {
            ValidateNoStackTraceInResponse(restResponse);

            string actualBody = null;

            Assert.DoesNotThrow(() =>
            {
                actualBody = JsonConvert.DeserializeObject<string>(restResponse.Content);
            }, "Failed to deserialize the content of the REST response into a MessageResult object!");

            Assert.AreEqual(expectedBody, actualBody,
                "The error message does not contain the expected string!\nActual body: '{0}'",
                actualBody);
        }

        #endregion Custom Asserts

        #region Members inherited from IDisposable

        /// <summary>
        /// Disposes this object and all disposable objects owned by this object.
        /// </summary>
        /// <param name="disposing">Pass true if explicitly called, or false if called from the destructor.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:ReduceCyclomaticComplexity")]    // TODO: Move Nova artifacts to separate file?
        protected virtual void Dispose(bool disposing)
        {
            Logger.WriteTrace("{0}.{1} called.", nameof(TestHelper), nameof(TestHelper.Dispose));

            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                Logger.WriteDebug("Deleting/Discarding all artifacts created by this TestHelper instance...");

                Storyteller?.Dispose();

                ArtifactBase.DisposeArtifacts(Artifacts, this);

                if (NovaArtifacts.Count > 0)
                {
                    foreach (var user in Users)
                    {
                        ArtifactStore.DiscardAllArtifacts(user);
                    }

                    var deleteExpectedStatusCodes = new List<System.Net.HttpStatusCode> { System.Net.HttpStatusCode.OK,
                        System.Net.HttpStatusCode.NotFound, System.Net.HttpStatusCode.Forbidden};
                    var publishExpectedStatusCodes = new List<System.Net.HttpStatusCode> { System.Net.HttpStatusCode.OK,
                        System.Net.HttpStatusCode.Conflict, System.Net.HttpStatusCode.NotFound};

                    foreach (var user in NovaArtifacts.Keys)
                    {
                        {
                            var neverPublishedArtifacIds = new List<int>();

                            foreach (int id in NovaArtifacts[user])
                            {
                                var results = ArtifactStore.DeleteArtifact(id, user, deleteExpectedStatusCodes);

                                if ((results != null) && (results[0].Version == 0))
                                {
                                    neverPublishedArtifacIds.Add(results[0].Id);
                                }
                            }

                            foreach (int id in neverPublishedArtifacIds)
                            {
                                NovaArtifacts[user].Remove(id);
                            }

                            if (NovaArtifacts[user].Count > 0)
                            {
                                ArtifactStore.PublishArtifacts(NovaArtifacts[user], user, expectedStatusCodes: publishExpectedStatusCodes);
                            }
                        }
                    }
                }

                Logger.WriteDebug("Deleting all groups created by this TestHelper instance...");
                foreach (var group in Groups)
                {
                    group.DeleteGroup();
                }

                if (ProjectRoles != null)
                {
                    Logger.WriteDebug("Deleting all project roles created by this TestHelper instance...");

                    foreach (var role in ProjectRoles)
                    {
                        role.DeleteRole();
                    }
                }

                if (Projects != null)
                {
                    Logger.WriteDebug("Deleting all projects created by this TestHelper instance...");

                    foreach (var project in Projects)
                    {
                        project.DeleteProject();
                    }
                }

                if (Users != null)
                {
                    Logger.WriteDebug("Deleting all users created by this TestHelper instance...");

                    foreach (var user in Users)
                    {
                        user.DeleteUser();
                    }
                }

                ArtifactStore?.Dispose();
                FileStore?.Dispose();
                ConfigControl?.Dispose();
                BlueprintServer?.Dispose();
                AdminStore?.Dispose();
                AccessControl?.Dispose();
            }

            _isDisposed = true;

            Logger.WriteTrace("{0}.{1} finished.", nameof(TestHelper), nameof(TestHelper.Dispose));
        }

        /// <summary>
        /// Disposes this object and all disposable objects owned by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion Members inherited from IDisposable
    }
}
