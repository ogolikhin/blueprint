using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using TestCommon;
using Utilities;
using Model.ArtifactModel.Enums;
using System.Linq;

namespace ArtifactStoreTests
{
    [Explicit(IgnoreReasons.UnderDevelopment)]  // Dev hasn't finished the story yet.
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class CopyArtifactTests : TestBase
    {
        private const string SVC_PATH = RestPaths.Svc.ArtifactStore.Artifacts_id_.COPY_TO_id_;

        private IUser _user = null;
        private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region 201 Created tests

        [TestCase(BaseArtifactType.Actor, BaseArtifactType.Glossary)]
        [TestCase(BaseArtifactType.Document, BaseArtifactType.Actor)]
        [TestCase(BaseArtifactType.Glossary, BaseArtifactType.TextualRequirement)]
        [TestCase(BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder)]  // Folders can only be children of other folders.
        [TestCase(BaseArtifactType.TextualRequirement, BaseArtifactType.Document)]
        [TestRail(191047)]
        [Description("Create and save a source & destination artifact.  Copy the source artifact under the destination artifact.  Verify the source artifact is unchanged " +
            "and the new artifact is identical to the source artifact.  New copied artifact should not be published.")]
        public void CopyArtifact_SingleSavedArtifact_ToNewParent_ReturnsNewArtifact(BaseArtifactType sourceArtifactType, BaseArtifactType targetArtifactType)
        {
            // Setup:
            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var sourceArtifact = Helper.CreateAndSaveArtifact(_project, author, sourceArtifactType);
            var targetArtifact = Helper.CreateAndSaveArtifact(_project, author, targetArtifactType);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = Helper.ArtifactStore.CopyArtifact(sourceArtifact, targetArtifact, author),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifact, copyResult, author);
        }
        
        [TestCase(BaseArtifactType.Actor, BaseArtifactType.Glossary)]
        [TestCase(BaseArtifactType.Document, BaseArtifactType.Actor)]
        [TestCase(BaseArtifactType.Glossary, BaseArtifactType.TextualRequirement)]
        [TestCase(BaseArtifactType.PrimitiveFolder, BaseArtifactType.PrimitiveFolder)]  // Folders can only be children of other folders.
        [TestCase(BaseArtifactType.TextualRequirement, BaseArtifactType.Document)]
        [TestRail(191048)]
        [Description("Create and publish a source & parent artifact (source should have 2 published versions).  Copy the source artifact into the project root.  " +
            "Verify the source artifact is unchanged and the new artifact is identical to the source artifact.  New copied artifact should not be published.")]
        public void CopyArtifact_SinglePublishedChildArtifact_ToProjectRoot_ReturnsNewArtifact(BaseArtifactType sourceArtifactType, BaseArtifactType parentArtifactType)
        {
            // Setup:
            var parentArtifact = Helper.CreateAndPublishArtifact(_project, _user, parentArtifactType);
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, sourceArtifactType, parentArtifact, numberOfVersions: 2);

            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, sourceArtifact.Id, _project.Id, author),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifact, copyResult, author);
        }
        
        [TestCase(BaseArtifactType.Actor, false)]
        [TestCase(BaseArtifactType.TextualRequirement, true)]
        [TestRail(191049)]
        [Description("Create & publish an artifact then create & save a folder.  Add an attachment to the artifact.  Copy the artifact into the folder.  " +
            "Verify the source artifact is unchanged and the new artifact is identical to the source artifact.  New copied artifact should not be published.")]
        public void CopyArtifact_SinglePublishedArtifactWithAttachment_ToNewSavedFolder_ReturnsNewArtifactWithAttachment(
            BaseArtifactType artifactType, bool shouldPublishAttachment)
        {
            // Setup:
            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, author, artifactType);
            var targetArtifact = Helper.CreateAndSaveArtifact(_project, author, BaseArtifactType.PrimitiveFolder);

            // Create & add attachment to the source artifact:
            var attachmentFile = FileStoreTestHelper.CreateNovaFileWithRandomByteArray();
            ArtifactStoreHelper.AddArtifactAttachmentAndSave(author, sourceArtifact, attachmentFile, Helper.ArtifactStore);

            if (shouldPublishAttachment)
            {
                sourceArtifact.Publish();
            }

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = Helper.ArtifactStore.CopyArtifact(sourceArtifact, targetArtifact, author),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifact, copyResult, author);

            // Verify the attachment was copied.
            var copiedArtifactAttachments = Helper.ArtifactStore.GetAttachments(sourceArtifact, author, addDrafts: true);
            Assert.AreEqual(1, copiedArtifactAttachments.AttachedFiles.Count, "Copied artifact should have 1 attachments at this point.");
            Assert.AreEqual(attachmentFile.FileName, copiedArtifactAttachments.AttachedFiles[0].FileName, "Filename of copied artifact attachment must have expected value.");
            Assert.AreEqual(0, copiedArtifactAttachments.DocumentReferences.Count, "Copied artifact shouldn't have any Document References.");

            var sourceArtifactAttachments = ArtifactStore.GetAttachments(Helper.ArtifactStore.Address, copyResult.Artifact.Id, author, addDrafts: true);
            Assert.AreEqual(1, sourceArtifactAttachments.AttachedFiles.Count, "Source artifact should have 1 attachments at this point.");
            Assert.AreEqual(attachmentFile.FileName, sourceArtifactAttachments.AttachedFiles[0].FileName, "Filename of source artifact attachment must have expected value.");
            Assert.AreEqual(0, sourceArtifactAttachments.DocumentReferences.Count, "Source artifact shouldn't have any Document References.");

            // Nova copy does a shallow copy of attachments, so sourceArtifactAttachments should equal copiedArtifactAttachments.
            AttachedFile.AssertEquals(sourceArtifactAttachments.AttachedFiles[0], copiedArtifactAttachments.AttachedFiles[0]);

            // TODO: Get the file contents and compare.
        }

        [TestCase(BaseArtifactType.Actor, TraceDirection.From, false, false)]
        [TestCase(BaseArtifactType.Glossary, TraceDirection.To, true, false)]
        [TestCase(BaseArtifactType.TextualRequirement, TraceDirection.TwoWay, true, true)]
        [TestRail(191050)]
        [Description("Create & save an artifact then create & publish a folder.  Add a manual trace between the artifact & folder.  Copy the artifact into the folder.  " +
            "Verify the source artifact is unchanged and the new artifact (and trace) is identical to the source artifact.  New copied artifact should not be published.")]
        public void CopyArtifact_SinglePublishedArtifactWithManualTrace_ToNewFolder_ReturnsNewArtifactWithManualTrace(
            BaseArtifactType artifactType, TraceDirection direction, bool isSuspect, bool shouldPublishTrace)
        {
            // Setup:
            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var sourceArtifact = Helper.CreateAndSaveArtifact(_project, author, artifactType);
            var targetArtifact = Helper.CreateAndPublishArtifact(_project, author, BaseArtifactType.PrimitiveFolder);

            // Create & add manual trace to the source artifact:
            ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(author, sourceArtifact, targetArtifact, ArtifactUpdateChangeType.Add,
                Helper.ArtifactStore, direction, isSuspect);

            if (shouldPublishTrace)
            {
                sourceArtifact.Publish();
            }

            // Execute:
            CopyNovaArtifactResultSet copyResult = null;

            Assert.DoesNotThrow(() => copyResult = Helper.ArtifactStore.CopyArtifact(sourceArtifact, targetArtifact, author),
                "'POST {0}' should return 201 Created when valid parameters are passed.", SVC_PATH);

            // Verify:
            AssertCopiedArtifactPropertiesAreIdenticalToOriginal(sourceArtifact, copyResult, author);

            // Get traces & compare.
            Relationships sourceRelationships = ArtifactStore.GetRelationships(Helper.ArtifactStore.Address, author, copyResult.Artifact.Id, addDrafts: true);
            Relationships targetRelationships = Helper.ArtifactStore.GetRelationships(author, targetArtifact, addDrafts: true);

            Assert.AreEqual(1, sourceRelationships.ManualTraces.Count, "Copied artifact should have 1 manual trace.");
            Assert.AreEqual(2, targetRelationships.ManualTraces.Count, "Target artifact should have 2 manual traces.");

            ArtifactStoreHelper.ValidateTrace(sourceRelationships.ManualTraces[0], targetArtifact);
            ArtifactStoreHelper.ValidateTrace(targetRelationships.ManualTraces[0], sourceArtifact);
        }
        /*
        [Category(Categories.CustomData)]
        [TestCase(BaseArtifactType.TextualRequirement)]
        [TestRail(191051)]
        [Description("Create and publish a folder.  Copy a reused artifact into the folder.  Verify the source artifact is unchanged and the new artifact " +
            "is identical to the source artifact (except no Reuse relationship).  New copied artifact should not be published.")]
        public void CopyArtifact_SinglePublishedReusedArtifact_ToNewFolder_ReturnsNewArtifactNotReused(BaseArtifactType artifactType)
        {
            Assert.Fail("Test not implemented yet.");
        }

        [Category(Categories.CustomData)]
        [TestCase(BaseArtifactType.Actor)]
        [TestRail(191052)]
        [Description("Create and publish an artifact (that has custom properties) and a folder.  Copy the artifact into the folder.  Verify the source artifact is unchanged " +
            "and the new artifact is identical to the source artifact (including custom properties).  New copied artifact should not be published.")]
        public void CopyArtifact_SinglePublishedArtifactWithCustomProperties_ToNewFolder_ReturnsNewArtifactWithCustomProperties(BaseArtifactType artifactType)
        {
            Assert.Fail("Test not implemented yet.");
        }
        */
        #endregion 201 Created tests

        // TODO ---------------- POSITIVE TESTS
        // TODO - Copy artifact to be a child of itself
        // TODO - Copy artifact (possibly with descendants) to one of its child
        // TODO - Copy orphan artifact

        #region 400 Bad Request tests

        [TestCase(-1.1)]
        [TestCase(0)]
        [TestRail(191207)]
        [Description("Create & save an artifact.  Copy the artifact and specify an OrderIndex <= 0.  Verify 400 Bad Request is returned.")]
        public void CopyArtifact_SavedArtifact_NotPositiveOrderIndex_400BadRequest(double orderIndex)
        {
            // Setup:
            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            IArtifact sourceArtifact = Helper.CreateAndSaveArtifact(_project, author, BaseArtifactType.Process);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
                ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, sourceArtifact.Id, _project.Id, author),
                "'POST {0}?orderIndex={1}' should return 400 Bad Request for non-positive OrderIndex values", SVC_PATH, orderIndex);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters,
                "Parameter orderIndex cannot be equal to or less than 0.");
        }

        [TestCase(ItemTypePredefined.ArtifactCollection, -0.0001)]
        [TestCase(ItemTypePredefined.CollectionFolder, 0)]
        [TestRail(191208)]
        [Description("Create & save a Collection or Collection Folder artifact.  Copy the Collection or Collection Folder and specify an OrderIndex <= 0.  " +
            "Verify 400 Bad Request is returned.")]
        public void CopyArtifact_SavedCollectionOrCollectionFolder_NotPositiveOrderIndex__400BadRequest(
            ItemTypePredefined artifactType, double orderIndex)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var collectionFolder = _project.GetDefaultCollectionFolder(Helper.ArtifactStore.Address, author);
            var fakeBaseType = BaseArtifactType.PrimitiveFolder;
            IArtifact sourceArtifact = Helper.CreateWrapAndSaveNovaArtifact(_project, author, artifactType, collectionFolder.Id, baseType: fakeBaseType);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
                ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, sourceArtifact.Id, _project.Id, author),
                "'POST {0}?orderIndex={1}' should return 400 Bad Request for non-positive OrderIndex values", SVC_PATH, orderIndex);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters,
                "Parameter orderIndex cannot be equal to or less than 0.");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(191225)]
        [Description("Create & publish an artifact.  Copy an artifact with call that does not have token in a header.  Verify response returns code 401 Unauthorized.")]
        public void CopyArtifact_PublishedArtifact_CopyWithNoTokenInAHeader_401Unauthorized(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() =>
            {
                ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, sourceArtifact.Id, _project.Id, user : null);
            }, "'POST {0}' should return 400 Bad Request when called with no token in a header!", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters, "Token is missing or malformed.");
        }

        #endregion 400 Bad Request tests

        #region 401 Unauthorized tests

        [TestCase(BaseArtifactType.Process)]
        [TestRail(191209)]
        [Description("Create & publish two artifacts.  Copy one artifact to be a child of the other with invalid token in a request.  Verify response returns code 401 Unauthorized.")]
        public void CopyArtifact_PublishedArtifact_CopyToParentArtifactWithInvalidToken_401Unauthorized(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            IArtifact newParentArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            IUser userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, sourceArtifact.Id, newParentArtifact.Id, userWithBadToken);
            }, "'POST {0}' should return 401 Unauthorized when called with an invalid token!", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.UnauthorizedAccess, "Unauthorized call");
        }

        #endregion 401 Unauthorized tests

        #region 403 Forbidden tests

        [TestCase(BaseArtifactType.Process)]
        [TestCase(BaseArtifactType.PrimitiveFolder)]
        [TestRail(192079)]
        [Description("Create & publish two artifacts.  Each one in different project.  Copy an artifact to be a child of the other artifact in different project.  " +
            "Verify returned code 403 Forbidden.")]
        public void CopyArtifact_PublishedArtifacts_CopyToBeAChildOfAnotherArtifactInDifferentProject_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            var projects = ProjectFactory.GetProjects(_user, numberOfProjects: 2);

            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projects);

            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(projects.First(), author, artifactType);
            IArtifact newParentArtifact = Helper.CreateAndPublishArtifact(projects.Last(), author, artifactType);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, sourceArtifact.Id, newParentArtifact.Id, author),
                "'POST {0}' should return 403 Forbidden when user tries to copy an artifact to a different project", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy artifact to a different project.");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(192080)]
        [Description("Create & save folder and artifact.  Copy a folder to be a child of an artifact.  Verify returned code 403 Forbidden.")]
        public void CopyArtifact_SavedArtifacts_CopyFolderToBeAChildOfArtifact_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, author, artifactType);
            IArtifact folder = Helper.CreateAndSaveArtifact(_project, author, BaseArtifactType.PrimitiveFolder);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, folder.Id, artifact.Id, author),
                "'POST {0}' should return 403 Forbidden when user tries to copy a folder to be a child of a regular artifact", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy a folder artifact to non folder/project parent.");
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(192081)]
        [Description("Create collection or collection folder. Copy regular artifact to be a child of the collection or collection folder. Verify returned code 403 Forbidden.")]
        public void CopyArtifact_PublishedArtifact_CopyToCollectionOrCollectionFolder_403Forbidden(ItemTypePredefined artifactType)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            INovaArtifact collectionFolder = _project.GetDefaultCollectionFolder(Helper.ArtifactStore.Address, author);

            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, artifact.Id, collectionFolder.Id, author),
               "'POST {0}' should return 403 Forbidden when user tries to copy a regular artifact to a {1} artifact type", SVC_PATH, artifactType);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy an artifact to non project section.");
        }

        [Ignore(IgnoreReasons.UnderDevelopment)]
        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(192082)]
        [Description("Create & save a collection or collection folder with a regular artifact. Copy collection or collection folder to be a child of the regular artifact.  " + 
            "Verify returned code 403 Forbidden.")]
        public void CopyArtifact_CollectionOrCollectionFolder_CopyToRegularArtifact_403Forbidden(ItemTypePredefined artifactType)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            INovaArtifact collectionFolder = _project.GetDefaultCollectionFolder(Helper.ArtifactStore.Address, author);

            IArtifact childArtifact = Helper.CreateWrapAndSaveNovaArtifact(_project, author, artifactType, collectionFolder.Id, baseType: BaseArtifactType.PrimitiveFolder);

            IArtifact parentArtifact = Helper.CreateAndSaveArtifact(_project, author, BaseArtifactType.Process);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => ArtifactStore.CopyArtifact(Helper.BlueprintServer.Address, childArtifact.Id, parentArtifact.Id, author),
                   "'POST {0}' should return 403 Forbidden when user tries to copy a collection or collection folder to be a child of a regular artifact", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy a collection artifact to non collection section.");
        }

        [Ignore(IgnoreReasons.UnderDevelopment)]
        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(192083)]
        [Description("Create a collection or collection folder. Copy a collection or collection folder to be a child of a collection artifact. Verify returned code 403 Forbidden.")]
        public void CopyArtifact_CollectionOrCollectionFolder_CopyToCollectionArtifact_403Forbidden(ItemTypePredefined artifactType)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            INovaArtifact collectionFolder = _project.GetDefaultCollectionFolder(Helper.ArtifactStore.Address, author);

            IArtifact collection = Helper.CreateWrapAndPublishNovaArtifact(_project, author, artifactType, collectionFolder.Id, baseType: BaseArtifactType.PrimitiveFolder);

            IArtifact collectionArtifact = Helper.CreateAndPublishCollection(_project, author);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => ArtifactStore.CopyArtifact(Helper.BlueprintServer.Address, collection.Id, collectionArtifact.Id, author),
                   "'POST {0}' should return 403 Forbidden when user tries to copy collection or collection folder to collection artifact", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy a collection artifact to non folder parent.");
        }

        [Ignore(IgnoreReasons.UnderDevelopment)]
        [TestCase]
        [TestRail(192084)]
        [Description("Find default Collections folder. Copy it to the same project. Verify returned code 403 Forbidden.")]
        public void CopyArtifact_CopyDefaultCollectionsFolder_403Forbidden(ItemTypePredefined artifactType)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            INovaArtifact collectionFolder = _project.GetDefaultCollectionFolder(Helper.ArtifactStore.Address, author);

            IArtifact collection = Helper.CreateWrapAndPublishNovaArtifact(_project, author, artifactType, collectionFolder.Id, baseType: BaseArtifactType.PrimitiveFolder);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => ArtifactStore.CopyArtifact(Helper.BlueprintServer.Address, collection.Id, _project.Id, author),
                   "'POST {0}' should return 403 Forbidden when user tries to copy default Collections folder into the project root", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy default Collections folder.");
        }

        // TODO: Copy default Collections folder to itself

        [TestCase(BaseArtifactType.Actor)]
        [TestCase(BaseArtifactType.PrimitiveFolder)]
        [TestRail(192085)]
        [Description("Create & publish two artifacts with sub-artifacts.  Copy an artifact to be a child of another artifact sub-artifact.  Verify returned code 403 Forbidden.")]
        public void CopyArtifact_SavedArtifacts_CopyToBeAChildOfSubArtifact_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, author, artifactType);
            IArtifact newParentArtifact = Helper.CreateAndPublishArtifact(_project, author, BaseArtifactType.Process);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(author, newParentArtifact.Id);

            Assert.IsNotNull(subArtifacts, "This artifact does not have sub-artifacts!");
            Assert.IsEmpty(subArtifacts, "This artifact does not have sub-artifacts!");

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, sourceArtifact.Id, subArtifacts.First().Id, author),
                "'POST {0}' should return 403 Forbidden when user tries to copy an artifact to be a child of a sub-artifact", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy artifact to a sub-artifact.");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(192086)]
        [Description("Create & publish two artifacts with sub-artifacts.  Copy a sub-artifact to be a child of another artifact.  Verify returned code 403 Forbidden.")]
        public void CopyArtifact_PublishedArtifacts_CopySubArtifactToBeAChildOfAnotherArtifact_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, author, artifactType);
            IArtifact newParentArtifact = Helper.CreateAndPublishArtifact(_project, author, artifactType);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(author, sourceArtifact.Id);

            Assert.IsNotNull(subArtifacts, "This artifact does not have sub-artifacts!");
            Assert.IsEmpty(subArtifacts, "This artifact does not have sub-artifacts!");

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, subArtifacts.First().Id, newParentArtifact.Id, author),
                "'POST {0}' should return 403 Forbidden when user tries to copy a sub-artifact to be a child of another artifact", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy sub-artifact to an artifact.");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(192087)]
        [Description("Create & publish two artifacts with sub-artifacts.  Copy a sub-artifact to be a child of another artifact sub-artifact.  Verify returned code 403 Forbidden.")]
        public void CopyArtifact_SavedArtifacts_CopySubArtifactToBeAChildOfAnotherArtifactSubArtifact_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            IArtifact sourceArtifact = Helper.CreateAndSaveArtifact(_project, author, artifactType);
            IArtifact newParentArtifact = Helper.CreateAndSaveArtifact(_project, author, artifactType);

            var sourceSubArtifacts = Helper.ArtifactStore.GetSubartifacts(author, sourceArtifact.Id);
            var newParentSubArtifacts = Helper.ArtifactStore.GetSubartifacts(author, newParentArtifact.Id);

            Assert.IsNotNull(sourceSubArtifacts, "This artifact does not have sub-artifacts!");
            Assert.IsEmpty(sourceSubArtifacts, "This artifact does not have sub-artifacts!");

            Assert.IsNotNull(newParentSubArtifacts, "This artifact does not have sub-artifacts!");
            Assert.IsEmpty(newParentSubArtifacts, "This artifact does not have sub-artifacts!");

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, sourceSubArtifacts.First().Id, newParentSubArtifacts.First().Id, author),
                "'POST {0}' should return 403 Forbidden when user tries to copy a sub-artifact to be a child of another artifact sub-artifact", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy sub-artifact to a sub-artifact.");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(192088)]
        [Description("Create & publish an artifact with sub-artifacts.  Copy a sub-artifact to be a child of another sub-artifact in the same artifact.  Verify returned code 403 Forbidden.")]
        public void CopyArtifact_PublishedArtifact_CopySubArtifactToBeAChildOfAnotherSubArtifact_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, author, artifactType);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(author, artifact.Id);

            Assert.IsNotNull(subArtifacts, "This artifact does not have sub-artifacts!");
            Assert.IsEmpty(subArtifacts, "This artifact does not have sub-artifacts!");

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, subArtifacts.First().Id, subArtifacts.Last().Id, author),
                "'POST {0}' should return 403 Forbidden when user tries to copy a sub-artifact to be a child of another sub-artifact within the same artifact", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy sub-artifact to a sub-artifact.");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(192089)]
        [Description("Create & save an artifact with sub-artifacts.  Copy a sub-artifact to be a child of default Collections folder.  Verify returned code 403 Forbidden.")]
        public void CopyArtifact_SavedArtifact_CopySubArtifactToBeAChildOfDefaultCollectionsFolder_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            INovaArtifact collectionFolder = _project.GetDefaultCollectionFolder(Helper.ArtifactStore.Address, author);

            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, author, artifactType);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(author, artifact.Id);

            Assert.IsNotNull(subArtifacts, "This artifact does not have sub-artifacts!");
            Assert.IsEmpty(subArtifacts, "This artifact does not have sub-artifacts!");

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, subArtifacts.First().Id, collectionFolder.Id, author),
                "'POST {0}' should return 403 Forbidden when user tries to copy a sub-artifact to be a child of default Collections folder", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy sub-artifact to Collections folder.");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(192090)]
        [Description("Create & publish an artifact with sub-artifacts.  Copy a sub-artifact to be a child of the project.  Verify returned code 403 Forbidden.")]
        public void CopyArtifact_PublishedArtifacts_CopySubArtifactToBeAChildOfProject_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, author, artifactType);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(author, artifact.Id);

            Assert.IsNotNull(subArtifacts, "This artifact does not have sub-artifacts!");
            Assert.IsEmpty(subArtifacts, "This artifact does not have sub-artifacts!");

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, subArtifacts.First().Id, _project.Id, author),
                "'POST {0}' should return 403 Forbidden when user tries to copy a sub-artifact to be a child of a project", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy sub-artifact to a project.");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(192091)]
        [Description("Create & publish an artifact with sub-artifacts.  Copy a project to be a child of the sub-artifact.  Verify returned code 403 Forbidden.")]
        public void CopyArtifact_SavedArtifact_CopyProjectToBeAChildOfSubArtifact_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, author, artifactType);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(author, artifact.Id);

            Assert.IsNotNull(subArtifacts, "This artifact does not have sub-artifacts!");
            Assert.IsEmpty(subArtifacts, "This artifact does not have sub-artifacts!");

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, _project.Id, subArtifacts.First().Id, author),
                "'POST {0}' should return 403 Forbidden when user tries to copy a project to be a child of a sub-artifact", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy a project to a sub-artifact.");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestCase(BaseArtifactType.PrimitiveFolder)]
        [TestRail(192092)]
        [Description("Create & publish an artifact.  Copy a project to be a child of the artifact.  Verify returned code 403 Forbidden.")]
        public void CopyArtifact_PublishedArtifact_CopyProjectToBeAChildOfArtifact_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, author, artifactType);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, _project.Id, artifact.Id, author),
                "'POST {0}' should return 403 Forbidden when user tries to copy a project to be a child of an artifact", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy project to an artifact.");
        }

        [TestCase]
        [TestRail(192093)]
        [Description("Copy project to be a child of default Collections folder. Verify returned code 403 Forbidden.")]
        public void CopyArtifact_CopyProjectToBeAChildOfDefaultCollectionsFolder_403Forbidden()
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            INovaArtifact collectionFolder = _project.GetDefaultCollectionFolder(Helper.ArtifactStore.Address, author);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => ArtifactStore.CopyArtifact(Helper.BlueprintServer.Address, _project.Id, collectionFolder.Id, author),
                   "'POST {0}' should return 403 Forbidden when user tries to copy a project to be a child of default Collections folder", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy a project to collection section.");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(192094)]
        [Description("Copy a project to be a child of another project.  Verify returned code 403 Forbidden.")]
        public void CopyArtifact_CopyProjectToBeAChildOfAnotherProject_403Forbidden()
        {
            // Setup:
            var projects = ProjectFactory.GetProjects(_user, numberOfProjects: 2);

            IUser author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projects);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => ArtifactStore.CopyArtifact(Helper.ArtifactStore.Address, projects.First().Id, projects.Last().Id, author),
                "'POST {0}' should return 403 Forbidden when user tries to copy a project to be a child of another project", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.Forbidden, "Cannot copy project to a different project.");
        }

        #endregion 403 Forbidden tests

        #region Private functions

        /// <summary>
        /// Asserts that the properties of the copied artifact are the same as the original artifact (except Id and Version)
        /// and that the the expected number of files were copied.
        /// </summary>
        /// <param name="originalArtifact">The original artifact that was copied.</param>
        /// <param name="copyResult">The result returned from the Nova copy call.</param>
        /// <param name="user">The user to use for getting artifact details.</param>
        /// <param name="expectedNumberOfArtifactsCopied">(optional) The number of artifacts that were expected to be copied.</param>
        /// <exception cref="AssertionException">If any expectations failed.</exception>
        private void AssertCopiedArtifactPropertiesAreIdenticalToOriginal(IArtifact originalArtifact,
            CopyNovaArtifactResultSet copyResult,
            IUser user,
            int expectedNumberOfArtifactsCopied = 1)
        {
            Assert.NotNull(copyResult, "The result returned from CopyArtifact() shouldn't be null!");
            Assert.NotNull(copyResult.Artifact, "The Artifact property returned by CopyArtifact() shouldn't be null!");
            Assert.AreEqual(-1, copyResult.Artifact.Version, "Version of a copied artifact should always be -1 (i.e. not published)!");
            Assert.AreEqual(expectedNumberOfArtifactsCopied, copyResult.CopiedArtifactsCount,
                "There should be exactly {0} artifact copied, but the result reports {1} artifacts were copied.",
                expectedNumberOfArtifactsCopied, copyResult.CopiedArtifactsCount);
            Assert.AreNotEqual(originalArtifact.Id, copyResult.Artifact.Id,
                "The ID of the copied artifact should not be the same as the original artifact!");

            ArtifactStoreHelper.AssertArtifactsEqual(originalArtifact, copyResult.Artifact, skipIdAndVersion: true);

            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(user, copyResult.Artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, copyResult.Artifact);
        }

        #endregion Private functions
    }
}
