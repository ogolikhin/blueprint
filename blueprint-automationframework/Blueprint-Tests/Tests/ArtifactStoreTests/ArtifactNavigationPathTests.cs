using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Model.ArtifactModel.Enums;
using TestCommon;
using Utilities;
using Common;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ArtifactNavigationPathTests : TestBase
    {
        private const string SVC_PATH = RestPaths.Svc.ArtifactStore.Artifacts_id_.NAVIGATION_PATH;
        private IUser _user = null;
        private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region 200 OK tests

        [TestCase(BaseArtifactType.Actor, 2)]
        [TestCase(BaseArtifactType.PrimitiveFolder, 2)]
        [TestRail(183596)]
        [Description("Create & publish an artifact.  Verify get artifact navigation path call returns project information.")]
        public void ArtifactNavigation_PublishedArtifact_ReturnsProjectInfo(BaseArtifactType artifactType, int numberOfVersions)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, numberOfVersions: numberOfVersions);

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, artifact.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyAncestorsInformation(basicArtifactInfoList, artifact.ParentId);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestCase(BaseArtifactType.PrimitiveFolder)]
        [TestRail(183597)]
        [Description("Create & save an artifact.  Verify get artifact navigation path call returns project information.")]
        public void ArtifactNavigation_SavedArtifact_ReturnsProjectInfo(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, artifact.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyAncestorsInformation(basicArtifactInfoList, artifact.ParentId);
        }

        [TestCase(BaseArtifactType.Actor, 2)]
        [TestRail(183598)]
        [Description("Create & publish an artifact and its child.  Verify get artifact navigation path call returns project parent and project information.")]
        public void ArtifactNavigation_PublishedArtifactWithAChild_ReturnsParentArtifactAndProjectInfo(BaseArtifactType artifactType, int numberOfVersions)
        {
            // Setup:
            var parentArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, numberOfVersions: numberOfVersions);
            var childArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, parentArtifact, numberOfVersions: numberOfVersions);
             
            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, childArtifact.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyAncestorsInformation(basicArtifactInfoList, childArtifact.ParentId);
        }

        [TestCase]
        [TestRail(183599)]
        [Description("Verify get artifact navigation path call for project returns an empty list.")]
        public void ArtifactNavigation_Project_ReturnsEmptyList()
        {
            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, _project.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);
            
            // Verify:
            Assert.IsEmpty(basicArtifactInfoList, "Project should not have a parent information!");
        }


        [TestCase(BaseArtifactType.Actor, 2)]
        [TestRail(183607)]
        [Description("Create & publish an artifact within a folder. Verify get artifact navigation path call returns folder and project information")]
        public void ArtifactNavigation_PublishedArtifactInAFolder_ReturnsFolderAndProjectInfo(BaseArtifactType artifactType, int numberOfVersions)
        {
            // Setup:
            var folder = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.PrimitiveFolder);
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, folder, numberOfVersions: numberOfVersions);

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, artifact.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyAncestorsInformation(basicArtifactInfoList, artifact.ParentId);
        }

        [TestCase(BaseArtifactType.UseCase, 2)]
        [TestCase(BaseArtifactType.Process, 2)]
        [TestRail(183608)]
        [Description("Create & publish an artifact with subartifacts. Verify get artifact navigation path call for sub-artifact returns artifact and project information")]
        public void ArtifactNavigation_SubArtifactIdOfPublishedArtifact_ReturnsArtifactAndProjectInfo(BaseArtifactType artifactType, int numberOfVersions)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, numberOfVersions: numberOfVersions);

            List<INovaSubArtifact> subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, artifact.Id);
            Assert.IsTrue(subArtifacts.Count > 0, "There is no sub-artifact in this artifact");

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, subArtifacts.First().Id),
                                "'GET {0}' should return 200 OK when passed a valid sub-artifact ID!", SVC_PATH);

            // Verify:
            VerifyAncestorsInformation(basicArtifactInfoList, subArtifacts.First().ParentId);
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(183630)]
        [Description("Create & publish a collection artifact. Verify get artifact navigation path call for collection returns Collections folder and project information.")]
        public void ArtifactNavigation_Collection_ReturnsCollectionFolderAndProjectInfo(ItemTypePredefined artifactType)
        {
            // Setup:
            INovaArtifact collectionFolder = _project.GetDefaultCollectionFolder(Helper.ArtifactStore.Address, _user);

            var collection = ArtifactStore.CreateArtifact(Helper.ArtifactStore.Address,
                _user, artifactType, "Collection test", _project, collectionFolder.Id);

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            IArtifact fakeArtifact = null;


            fakeArtifact = ArtifactFactory.CreateArtifact(
                    _project, _user, BaseArtifactType.Actor, collection.Id);   // Don't use Helper because this isn't a real artifact, it's just wrapping the bad artifact ID.

            try
            {                
                // Execute:
                Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, collection.Id),
                                    "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

                // Verify:
                VerifyAncestorsInformation(basicArtifactInfoList, collection.ParentId);
            }
            finally
            {
                fakeArtifact.Discard(_user);
            }
        }

        [Ignore(IgnoreReasons.UnderDevelopment)] //Artifacts for Baseline and Review need to be added to Custom Data project
        [Category(Categories.CustomData)]
        [TestCase(96384)]
        [TestRail(185119)]
        [Description("Verify get artifact navigation path call for Baseline returns Baseline & Review folder and project information")]
        public void ArtifactNavigation_Baseline_ReturnsBaselineFolderAndProjectInfo(int id)
        {
            // Setup:
            var basicArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, id);

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            Assert.IsNotNull(basicArtifactInfo, "Cannot navigate to artifact which id is null!");

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, basicArtifactInfo.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyAncestorsInformation(basicArtifactInfoList, basicArtifactInfo.ParentId);
        }


        [TestCase(BaseArtifactType.Actor, 10, BaseArtifactType.PrimitiveFolder)]
        [TestRail(183641)]
        [Description("Create & publish an artifact within a chain of 10 folders. Verify get artifact navigation path call for artifact in a chain of folders returns information about all ancestor folders and project.")]
        public void ArtifactNavigation_PublishedArtifactInAChainOfFolders_ReturnsListOfFoldersAndProjectInfo(BaseArtifactType artifactType, int numberOfArtifacts, BaseArtifactType folderType)
        {
            // Setup:
            List<BaseArtifactType> artifactTypes = CreateListOfArtifactTypes(numberOfArtifacts, folderType);

            var folders = Helper.CreatePublishedArtifactChain(_project, _user, artifactTypes.ToArray());

            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, folders.Last());

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, artifact.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyAncestorsInformation(basicArtifactInfoList, artifact.ParentId);
        }

        [TestCase(BaseArtifactType.PrimitiveFolder, // PrimitiveFolders can only have projects as parents.
            BaseArtifactType.Actor,
            BaseArtifactType.BusinessProcess,
            BaseArtifactType.Document,
            BaseArtifactType.DomainDiagram,
            BaseArtifactType.GenericDiagram,
            BaseArtifactType.Glossary,
            BaseArtifactType.Process,
            BaseArtifactType.Storyboard,
            BaseArtifactType.TextualRequirement,
            BaseArtifactType.UIMockup,
            BaseArtifactType.UseCase,
            BaseArtifactType.UseCaseDiagram)]
        [TestRail(184481)]
        [Description("Create a chain of published parent/child artifacts and other top level artifacts. Verify a list of top level artifact information is returned and values of properties are correct.")]
        public void ArtifactNavigation_PublishedChainWithAllArtifactTypes_ReturnListOfArtifactInfo(params BaseArtifactType[] artifactTypeChain)
        {
            ThrowIf.ArgumentNull(artifactTypeChain, nameof(artifactTypeChain));

            // Setup:
            var artifactChain = Helper.CreatePublishedArtifactChain(_project, _user, artifactTypeChain);

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, artifactChain.Last().Id),
                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyAncestorsInformation(basicArtifactInfoList, artifactChain.Last().ParentId);
        }

        [TestCase(BaseArtifactType.Glossary, 5, BaseArtifactType.Process)]
        [TestRail(184482)]
        [Description("Create & publish an artifact with chains of 5 child artifacts and 5 folders. Move chain of artifacts to one folder before the last. Verify get artifact navigation path call for artifacts and folders in a chain returns information about all ancestor artifacts and project.")]
        public void ArtifactNavigation_PublishedArtifactInAChainOfArtifactsAndFolders_ReturnsListOfArtifactAndFolderInfo(BaseArtifactType artifactType, int numberOfArtifacts, BaseArtifactType artifactTypeInChain)
        {
            List<BaseArtifactType> folderTypes = CreateListOfArtifactTypes(numberOfArtifacts, BaseArtifactType.PrimitiveFolder);
            List<BaseArtifactType> artifactTypes = CreateListOfArtifactTypes(numberOfArtifacts, artifactTypeInChain);

            var artifacts = Helper.CreatePublishedArtifactChain(_project, _user, artifactTypes.ToArray());
            var folders = Helper.CreatePublishedArtifactChain(_project, _user, folderTypes.ToArray());

            artifacts.First().Lock(_user);
            Helper.ArtifactStore.MoveArtifact(artifacts.First(), folders[folders.Count - 2], _user);

            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, artifacts.Last());

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, artifact.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyAncestorsInformation(basicArtifactInfoList, artifact.ParentId);
        }

        [TestCase(BaseArtifactType.Glossary, 5, BaseArtifactType.Process)]
        [TestRail(184483)]
        [Description("Create & save an artifact with chains of 5 child artifacts and 5 folders. Move chain of artifacts to one folder before the last. Verify get artifact navigation path call for artifacts and folders in a chain returns information about all ancestor artifacts and project.")]
        public void ArtifactNavigation_SavedArtifactInAChainOfArtifactsAndFolders_ReturnsListOfArtifactAndFolderInfo(BaseArtifactType artifactType, int numberOfArtifacts, BaseArtifactType artifactTypeInChain)
        {
            // Setup:
            List<BaseArtifactType> folderTypes = CreateListOfArtifactTypes(numberOfArtifacts, BaseArtifactType.PrimitiveFolder);
            List<BaseArtifactType> artifactTypes = CreateListOfArtifactTypes(numberOfArtifacts, artifactTypeInChain);

            var artifacts = Helper.CreateSavedArtifactChain(_project, _user, artifactTypes.ToArray());
            var folders = Helper.CreateSavedArtifactChain(_project, _user, folderTypes.ToArray());

            artifacts.First().Lock(_user);
            Helper.ArtifactStore.MoveArtifact(artifacts.First(), folders[folders.Count - 2], _user);

            var artifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType, artifacts.Last());

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, artifact.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyAncestorsInformation(basicArtifactInfoList, artifact.ParentId);
        }

        [TestCase(BaseArtifactType.UseCase, 5, BaseArtifactType.Process)]
        [TestRail(184484)]
        [Description("Create & publish an artifact with sub-artifacts and chains of 5 child artifacts and 5 folders. Move chain of artifacts to one folder before the last. Verify get artifact navigation path call for artifacts and folders in a chain returns information about all ancestor artifacts and project.")]
        public void ArtifactNavigation_SubArtifactinPublishedArtifactAndAChainOfArtifactsAndFolders_ReturnsListOfArtifactAndFolderInfo(BaseArtifactType artifactType, int numberOfArtifacts, BaseArtifactType artifactTypeInChain)
        {
            // Setup:
            List<BaseArtifactType> folderTypes = CreateListOfArtifactTypes(numberOfArtifacts, BaseArtifactType.PrimitiveFolder);
            List<BaseArtifactType> artifactTypes = CreateListOfArtifactTypes(numberOfArtifacts, artifactTypeInChain);

            var artifacts = Helper.CreatePublishedArtifactChain(_project, _user, artifactTypes.ToArray());
            var folders = Helper.CreatePublishedArtifactChain(_project, _user, folderTypes.ToArray());

            artifacts.First().Lock(_user);
            Helper.ArtifactStore.MoveArtifact(artifacts.First(), folders[folders.Count - 2], _user);

            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, artifacts.Last());

            List<INovaSubArtifact> subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, artifact.Id);
            Assert.IsTrue(subArtifacts.Count > 0, "There is no sub-artifact in this artifact");

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, subArtifacts.Last().Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyAncestorsInformation(basicArtifactInfoList, subArtifacts.Last().ParentId);
        }

        [TestCase(BaseArtifactType.Actor, 2)]
        [TestCase(BaseArtifactType.PrimitiveFolder, 2)]
        [TestRail(185143)]
        [Description("Create, publish & delete artifact.  Verify get artifact navigation path call returns project information for another user.")]
        public void ArtifactNavigation_PublishedArtifactDeletedAndAccessedByAnotherUser_ReturnsProjectInfo(BaseArtifactType artifactType, int numberOfVersions)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, numberOfVersions: numberOfVersions);

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            artifact.Delete(_user);

            IUser anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(anotherUser, artifact.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyAncestorsInformation(basicArtifactInfoList, artifact.ParentId);
        }

        //TODO Test for project in a folder

        #endregion 200 OK tests

        #region 400 Bad Request Tests

        [TestCase]
        [TestRail(183631)]
        [Description("Get an artifact navigation path without a Session-Token header. Execute GetArtifactNagivationPath - Must return 400 Bad Request.")]
        public void ArtifactNavigationPath_MissingTokenHeader_400BadRequest()
        {
            // Setup: Create a user without session
            // Execute and Validation: Execute GetNavigationPath without a Session-Token header
            Assert.Throws<Http400BadRequestException>(() => Helper.ArtifactStore.GetNavigationPath(user: null, itemId: _project.Id), "Calling GET {0} without a Session-Token header should return 400 BadRequest!", RestPaths.Svc.ArtifactStore.Artifacts_id_.NAVIGATION_PATH);
        }

        #endregion 400 Bad Request Tests

        #region 401 Unauthorized Tests

        [TestCase]
        [TestRail(183632)]
        [Description("Get an artifact navigation path using the user with invalid session. Execute GetArtifactNagivationPath - Must return 401 Unautorized.")]
        public void ArtifactNavigationPath_InvalidSession_401Unauthorized()
        {
            // Setup: Create a user with invalid session
            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;
            IUser userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute and Validation: Execute GetNavigationPath using the user with invalid session
            Assert.Throws<Http401UnauthorizedException>(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(user: userWithBadToken, itemId: _project.Id), "Calling GET {0} using the user with invalid session should return 401 Unauthorized!", RestPaths.Svc.ArtifactStore.Artifacts_id_.NAVIGATION_PATH);
        }

        #endregion 401 Unauthorized Tests

        #region 403 Forbidden Tests

        [TestCase]
        [TestRail(183633)]
        [Description("Get an artifact navigation path using the user without permission to the project. Execute GetArtifactNagivationPath - Must return 403 Forbidden.")]
        public void ArtifactNavigationPath_WithoutPermissionToViewArtifact_403Forbidden()
        {
            // Setup: Create user with no permission on the project
            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;
            var userWithNoPermissionOnAnyProject = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.None, project: _project);

            // Execute: Execute GetNavigationPath using the user without view permission to the artifact
            var ex = Assert.Throws<Http403ForbiddenException>(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(user: userWithNoPermissionOnAnyProject, itemId: _project.Id), "Calling GET {0} using the user without view permission should return 403 Forbidden!", RestPaths.Svc.ArtifactStore.Artifacts_id_.NAVIGATION_PATH);

            // Validation: Exception should contain proper errorCode in the response content.
            var serviceErrorMessage = Deserialization.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);
            Assert.That(serviceErrorMessage.ErrorCode.Equals(ErrorCodes.UnauthorizedAccess), "{0} using the user without view permission to the artifact should return {1} errorCode but {2} is returned", RestPaths.Svc.ArtifactStore.Artifacts_id_.NAVIGATION_PATH, ErrorCodes.UnauthorizedAccess, serviceErrorMessage.ErrorCode);
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(185178)]
        [Description("Create & publish an artifact. User without permissions to artifact calls GetArtifactNagivationPath.  Verify returned code 403 Forbidden.")]
        public void ArtifactNavigationPath_PublishedArtifact_UserWithoutPermissionsToArtifact_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            // Create a user that has access to the project but not the artifact.
            IUser userWithoutPermissions = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithoutPermissions, TestHelper.ProjectRole.None, _project, artifact);

            // Execute & Verify:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.GetNavigationPath(user: userWithoutPermissions, itemId: artifact.Id),
                "'GET {0}' should return 403 Forbidden when user without permissions tries to get artifact path!", SVC_PATH);

            string expectedExceptionMessage = I18NHelper.FormatInvariant("User does not have permissions for Artifact (Id:{0}).", artifact.Id);
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "Expected '{0}' error when user without permissions tries to get artifact path.", expectedExceptionMessage);
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(185184)]
        [Description("Create & publish parent and child artifact. User without permissions to parent artifact calls GetArtifactNagivationPath for child artifact.  Verify returned code 403 Forbidden.")]
        public void ArtifactNavigationPath_PublishedParentAndChildArtifacts_UserWithoutPermissionsToParentArtifact_403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            var parentArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            var childArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, parentArtifact);

            // Create a user that has access to the project but not the artifact.
            IUser userWithoutPermissions = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithoutPermissions, TestHelper.ProjectRole.None, _project, parentArtifact);

            // Execute & Verify:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.GetNavigationPath(user: userWithoutPermissions, itemId: childArtifact.Id),
                "'GET {0}' should return 403 Forbidden when user without permissions tries to get artifact path for artifact which parent artifact has no permissions!", SVC_PATH);

            string expectedExceptionMessage = I18NHelper.FormatInvariant("User does not have permissions for Artifact (Id:{0}).", childArtifact.Id);
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "Expected '{0}' error when user without permissions tries to get artifact path for artifact which parent artifact has no permissions.", expectedExceptionMessage);
        }
        #endregion 403 Forbidden Tests

        #region 404 Not Found Tests

        [TestCase(int.MaxValue)]
        [TestRail(183634)]
        [Description("Get an artifact navigation path with non-existing artifact ID. Execute GetArtifactNagivationPath - Must return 404 Not Found.")]
        public void ArtifactNavigationPath_WithNonExistingArtifactId_404NotFound(int nonExistingArtifactId)
        {
            // Setup:
            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute: Execute GetNavigationPath with non-existing artifact ID
            var ex = Assert.Throws<Http404NotFoundException>(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(user: _user, itemId: nonExistingArtifactId), "Calling GET {0} with non-existing artifact ID should return 404 Not Found!", RestPaths.Svc.ArtifactStore.Artifacts_id_.NAVIGATION_PATH);

            // Validation: Exception should contain proper errorCode in the response content.
            var serviceErrorMessage = Deserialization.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);
            Assert.That(serviceErrorMessage.ErrorCode.Equals(ErrorCodes.ResourceNotFound), "{0} with non-existing artifact ID should return {1} errorCode but {2} is returned", RestPaths.Svc.ArtifactStore.Artifacts_id_.NAVIGATION_PATH, ErrorCodes.ResourceNotFound, serviceErrorMessage.ErrorCode);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestRail(184485)]
        [Description("Get an artifact navigation path with invalid artifact ID. Execute GetArtifactNagivationPath - Must return 404 Not Found.")]
        public void ArtifactNavigationPath_WithInvalidArtifactId_404NotFound(int invalidArtifactId)
        {
            // Setup:
            // Execute and Validation: Execute GetNavigationPath with invalid artifact ID
            Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetNavigationPath(user: _user, itemId: invalidArtifactId), "Calling GET {0} with invalid artifact ID should return 404 Not Found!", RestPaths.Svc.ArtifactStore.Artifacts_id_.NAVIGATION_PATH);
        }

        [TestCase]
        [TestRail(185142)]
        [Description("Get an artifact navigation path with saved-only artifact by other user. Execute GetArtifactNagivationPath - Must return 404 Not Found.")]
        public void ArtifactNavigationPath_WithSavedOnlyArtifactByOtherUser_404NotFound()
        {
            // Setup: Create and save artifact with the second user
            var secondUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            var savedArtifactBySecondUser = Helper.CreateAndSaveArtifact(_project, secondUser, BaseArtifactType.Actor);

            // Execute and Validation: Execute GetNavigationPath with saved-only artifact by other user
            Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetNavigationPath(user: _user, itemId: savedArtifactBySecondUser.Id), "Calling GET {0} with saved-only artifact by other user should return 404 Not Found!", RestPaths.Svc.ArtifactStore.Artifacts_id_.NAVIGATION_PATH);
        }

        [TestCase]
        [TestRail(184486)]
        [Description("Get an artifact navigation path with deleted artifact ID. Execute GetArtifactNagivationPath - Must return 404 Not Found.")]
        public void ArtifactNavigationPath_WithDeletedArtifactId_404NotFound()
        {
            // Setup: Created and publish artifact then delete the artifact
            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;
            var deletedArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            deletedArtifact.Delete();
            deletedArtifact.Publish();

            // Execute: Execute GetNavigationPath with the deleted artifact ID
            var ex = Assert.Throws<Http404NotFoundException>(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(user: _user, itemId: deletedArtifact.Id), "Calling GET {0} with deleted artifact ID should return 404 Not Found!", RestPaths.Svc.ArtifactStore.Artifacts_id_.NAVIGATION_PATH);

            // Validation: Exception should contain proper errorCode in the response content.
            var serviceErrorMessage = Deserialization.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);
            Assert.That(serviceErrorMessage.ErrorCode.Equals(ErrorCodes.ResourceNotFound), "{0} with deleted artifact ID should return {1} errorCode but {2} is returned", RestPaths.Svc.ArtifactStore.Artifacts_id_.NAVIGATION_PATH, ErrorCodes.ResourceNotFound, serviceErrorMessage.ErrorCode);
        }

        #endregion 404 Not Found Tests

        #region private calls

        /// <summary>
        /// Verifies that an artifact ancestors in a path return proper values
        /// </summary>
        /// <param name="basicArtifactInfo">List of artifact basic information about ancestors artifact.</param>
        /// <param name="id">Id of artifact or sub-artifact.</param>
        private void VerifyAncestorsInformation(List<INovaVersionControlArtifactInfo> basicArtifactInfo, int? id)
        {
            INovaVersionControlArtifactInfo parentArtifactInfo = null;

            basicArtifactInfo.Reverse();

            foreach (var artifactinfo in basicArtifactInfo)
            {
                Assert.IsNotNull(id, "Cannot verify values of artifact with id value null!");

                parentArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, (int)id);

                Assert.AreEqual(parentArtifactInfo.ItemTypeId, artifactinfo.ItemTypeId, "the item type is not item type of a parent!");
                Assert.AreEqual(parentArtifactInfo.Name, artifactinfo.Name, "The name is not the name of the parent!");
                Assert.AreEqual(parentArtifactInfo.ProjectId, artifactinfo.ProjectId, "The project id is not the project id of the parent!");

                if (id != _project.Id)
                    id = (int)parentArtifactInfo.ParentId;
            }
        }

        /// <summary>
        /// Creates a list of artifact types.
        /// </summary>
        /// <param name="numberOfArtifacts">The number of artifact types to add to the list.</param>
        /// <param name="artifactType">The artifact type.</param>
        /// <returns>A list of artifact types.</returns>
        private static List<BaseArtifactType> CreateListOfArtifactTypes(int numberOfArtifacts, BaseArtifactType artifactType)
        {
            List<BaseArtifactType> artifactTypes = new List<BaseArtifactType>();

            for (int i = 0; i < numberOfArtifacts; i++)
            {
                artifactTypes.Add(artifactType);
            }

            return artifactTypes;
        }
        #endregion private calls
    }
}
