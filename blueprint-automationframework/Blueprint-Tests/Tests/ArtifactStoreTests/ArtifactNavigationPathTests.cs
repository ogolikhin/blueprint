using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TestCommon;
using Utilities;

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
        [Description("Create & publish an artifact.  Verify the basic artifact information returned from parent has correct values.")]
        public void ArtifactNavigation_PublishedArtifact_ReturnsParentArtifactInfo_200OK(BaseArtifactType artifactType, int numberOfVersions)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, numberOfVersions: numberOfVersions);

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, artifact.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyParentInformation(basicArtifactInfoList[0], artifact.ParentId);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestCase(BaseArtifactType.PrimitiveFolder)]
        [TestRail(183597)]
        [Description("Create & save an artifact.  Verify the basic artifact information returned from parent has correct values.")]
        public void ArtifactNavigation_SavedArtifact_ReturnsParentArtifactInfo_200OK(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);

            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, artifact.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyParentInformation(basicArtifactInfoList[0], artifact.ParentId);
        }

        [TestCase(BaseArtifactType.Actor, 2)]
        [TestRail(183598)]
        [Description("Create & publish an artifact and its child.  Verify the basic artifact information returned from parent has correct values.")]
        public void ArtifactNavigation_PublishedArtifactWithAChild_ReturnsParentArtifactInfo_200OK(BaseArtifactType artifactType, int numberOfVersions)
        {
            // Setup:
            var parentArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, numberOfVersions: numberOfVersions);
            var childArtifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType, parentArtifact, numberOfVersions: numberOfVersions);
             
            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, childArtifact.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);

            // Verify:
            VerifyParentInformation(basicArtifactInfoList.Last(), childArtifact.ParentId);
        }

        [TestCase]
        [TestRail(183599)]
        [Description("Verify the basic artifact information returned from project is empty.")]
        public void ArtifactNavigation_Project_ReturnsArtifactInfo_200OK()
        {
            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(_user, _project.Id),
                                "'GET {0}' should return 200 OK when passed a valid artifact ID!", SVC_PATH);
            
            // Verify:
            Assert.IsEmpty(basicArtifactInfoList, "Project should not have a parent information!");
        }

        //TODO Test for artifact in a folder
        //TODO Test for sub-artifact
        //TODO Test for collection/baseline/review          
        //TODO Test for artifact in a long chain of 10 or more folders
        //TODO Test for artifact in a long chain of 10 or more child artifacts
        //TODO Test for artifact in a long chain of mixwd folders and child artifacts. Use TestCase(TestCaseSources.AllArtifactTypesForOpenApiRestMethods)]
        //TODO Test for project in a folder
        //TODO Test for sub-artifact at end of a chain of artifacts.

        #endregion 200 OK tests

        #region Negative tests
        //TODO 400 - The session token is missing or malformed
        [TestCase]
        [TestRail(183631)]
        [Description("Get an artifact navigation path without a Session-Token header. Execute GetArtifactNagivationPath - Must return 400 Bad Request.")]
        public void ArtifactNavigationPath_MissingTokenHeader_400BadRequest()
        {
            // Setup: Create a user without session
            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute and Validation: Execute GetNavigationPath without a Session-Token header
            Assert.Throws<Http400BadRequestException>(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(user: null, itemId: _project.Id), "Calling GET {0} without a Session-Token header should return 400 BadRequest!", RestPaths.Svc.ArtifactStore.Artifacts_id_.NAVIGATION_PATH);
        }

        //TODO 401 - The session token is invalid.    
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

        //TODO 403 - The user does not have permissions to view the artifact.
        [TestCase]
        [TestRail(183633)]
        [Description("Get an artifact navigation path using the user with invalid session. Execute GetArtifactNagivationPath - Must return 403 Forbidden.")]
        public void ArtifactNavigationPath_WithoutPermissionToViewArtifact_403Forbidden()
        {
            // Setup: Create user with no permission on the project
            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;
            var userWithNoPermissionOnAnyProject = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.None, projects: new List<IProject>() { _project });

            // Execute: Execute GetNavigationPath using the user without view permission to the artifact
            var ex = Assert.Throws<Http403ForbiddenException>(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(user: userWithNoPermissionOnAnyProject, itemId: _project.Id), "Calling GET {0} using the user without view permission should return 403 Forbidden!", RestPaths.Svc.ArtifactStore.Artifacts_id_.NAVIGATION_PATH);

            // Validation: Exception should contain proper errorCode in the response content.
            var serviceErrorMessage = Deserialization.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);
            Assert.That(serviceErrorMessage.ErrorCode.Equals(ErrorCodes.UnauthorizedAccess), "{0} using the user without view permission to the artifact should return {1} errorCode but {2} is returned", RestPaths.Svc.ArtifactStore.Artifacts_id_.NAVIGATION_PATH, ErrorCodes.UnauthorizedAccess, serviceErrorMessage.ErrorCode);
        }

        //TODO 404 - An artifact for the specified id is not found, does not exist or is deleted
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
            Assert.That(serviceErrorMessage.ErrorCode.Equals(ErrorCodes.ResourceNotFound), "{0} with non-exsting artifact ID should return {1} errorCode but {2} is returned", RestPaths.Svc.ArtifactStore.Artifacts_id_.NAVIGATION_PATH, ErrorCodes.ResourceNotFound, serviceErrorMessage.ErrorCode);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestRail(184485)]
        [Description("Get an artifact navigation path with non-existing artifact ID. Execute GetArtifactNagivationPath - Must return 404 Not Found.")]
        public void ArtifactNavigationPath_WithInvalidArtifactId_404NotFound(int invalidArtifactId)
        {
            // Setup:
            List<INovaVersionControlArtifactInfo> basicArtifactInfoList = null;

            // Execute and Validation: Execute GetNavigationPath with invalidArtifacId artifact ID
            Assert.Throws<Http404NotFoundException>(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(user: _user, itemId: invalidArtifactId), "Calling GET {0} with non-existing artifact ID should return 404 Not Found!", RestPaths.Svc.ArtifactStore.Artifacts_id_.NAVIGATION_PATH);
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
            var ex = Assert.Throws<Http404NotFoundException>(() => basicArtifactInfoList = Helper.ArtifactStore.GetNavigationPath(user: _user, itemId: deletedArtifact.Id), "Calling GET {0} with non-existing artifact ID should return 404 Not Found!", RestPaths.Svc.ArtifactStore.Artifacts_id_.NAVIGATION_PATH);

            // Validation: Exception should contain proper errorCode in the response content.
            var serviceErrorMessage = Deserialization.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);
            Assert.That(serviceErrorMessage.ErrorCode.Equals(ErrorCodes.ResourceNotFound), "{0} with deleted artifact ID should return {1} errorCode but {2} is returned", RestPaths.Svc.ArtifactStore.Artifacts_id_.NAVIGATION_PATH, ErrorCodes.ResourceNotFound, serviceErrorMessage.ErrorCode);
        }
        #endregion Negative tests

        #region private calls

        /// <summary>
        /// Verifies that an artifact ancestor in a path returns proper values
        /// </summary>
        /// <param name="basicArtifactInfo">Basic information about ancestor artifact/project.</param>
        /// <param name="id">Id of artifact or sub-artifact.</param>
        private void VerifyParentInformation(INovaVersionControlArtifactInfo basicArtifactInfo, int id)
        {
            INovaVersionControlArtifactInfo parentArtifactInfo = null;

            parentArtifactInfo = Helper.ArtifactStore.GetVersionControlInfo(_user, id);

            Assert.AreEqual(parentArtifactInfo.ItemTypeId, basicArtifactInfo.ItemTypeId, "the item type is not item type of a parent!");
            Assert.AreEqual(parentArtifactInfo.Name, basicArtifactInfo.Name, "The name is not the name of the parent!");
            Assert.AreEqual(parentArtifactInfo.ProjectId, basicArtifactInfo.ProjectId, "The project id is not the project id of the parent!");
        }

        #endregion private calls
    }
}
