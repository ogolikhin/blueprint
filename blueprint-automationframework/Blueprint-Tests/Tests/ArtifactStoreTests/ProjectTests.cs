using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ProjectTests : TestBase
    {
        private const int NON_EXISTING_ID = int.MaxValue;
        private const string PATH_PROJECT_CHILDREN = RestPaths.Svc.ArtifactStore.Projects_id_.CHILDREN;
        private const string PATH_ARTIFACT_CHILDREN = RestPaths.Svc.ArtifactStore.Projects_id_.Artifacts_id_.CHILDREN;

        private IUser _adminUser = null;
        private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_adminUser);
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _adminUser);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region GetProjectChildrenByProjectId tests

        [TestCase(BaseArtifactType.Actor)]
        [TestCase(BaseArtifactType.Document)]
        [TestRail(125497)]
        [Description("Executes Get project children call after creating an artifact in the project and Verify that the returned result" +
            "from the successful response contains the artifact.")]
        public void GetProjectChildrenByProjectId_CreateArtifact_VerifyResultContainsTheArtifact(BaseArtifactType artifactType)
        {
            // Setup:
            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            var publishedArtifact = Helper.CreateAndPublishArtifact(_project, _adminUser, artifactType);

            // Execute:
            List<NovaArtifact> returnedNovaArtifactList = null;
            Assert.DoesNotThrow(() => returnedNovaArtifactList = Helper.ArtifactStore.GetProjectChildrenByProjectId(_project.Id, viewer),
                "The 'GET {0}' endpoint should return 200 OK if valid parameters are passed!", PATH_PROJECT_CHILDREN);

            // Verify: validate the list of artifacts returned by the REST call.
            ArtifactStoreHelper.ValidateNovaArtifacts(_project, novaArtifacts: returnedNovaArtifactList);

            var returnedNovaArtifact = FindArtifactFromNovaArtifacts(returnedNovaArtifactList, publishedArtifact);

            ArtifactStoreHelper.AssertArtifactsEqual(publishedArtifact, returnedNovaArtifact, skipIdAndVersion: true);
        }

        [TestCase(BaseArtifactType.Process)]
        [TestCase(BaseArtifactType.PrimitiveFolder)]
        [TestRail(267023)]
        [Description("Executes Get project children call after creating an artifact in the project to which user has no read permissions. " +
            "Verify that the returned result does not include artifact to which user does not have permissions.")]
        public void GetProjectChildrenByProjectId_CreateArtifact_UserHasNoPermissions_VerifyResultsHaveNoArtifact(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, artifactType);

            var userWithNoPermissionsToArtifact = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithNoPermissionsToArtifact, TestHelper.ProjectRole.None, _project, artifact);

            // Execute:
            List<NovaArtifact> returnedNovaArtifactList = null;
            Assert.DoesNotThrow(() => returnedNovaArtifactList = Helper.ArtifactStore.GetProjectChildrenByProjectId(_project.Id, userWithNoPermissionsToArtifact),
                "The 'GET {0}' endpoint should return 200 OK if valid parameters are passed!", PATH_PROJECT_CHILDREN);

            // Verify:
            ArtifactStoreHelper.ValidateNovaArtifacts(_project, novaArtifacts: returnedNovaArtifactList);

            Assert.IsNull(returnedNovaArtifactList.Find(a => a.Id.Equals(artifact.Id)), "Artifact should not be in the list of returned artifacts!");
        }

        [TestCase]
        [TestRail(125501)]
        [Description("Executes Get project children call with an invalid token.  Verifies 401 Unauthorized is returned.")]
        public void GetProjectChildrenByProjectId_InvalidToken_401Unauthorized()
        {
            // Setup:
            var unauthorizedUser = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetProjectChildrenByProjectId(_project.Id, unauthorizedUser);
            }, "The 'GET {0}' endpoint should return 401 Unauthorized if an unauthorized token is passed!", PATH_PROJECT_CHILDREN);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, "Token is invalid.");
        }
        
        [TestCase]
        [TestRail(125502)]
        [Description("Executes Get project children call with no Session-Token header.  Verifies 401 Unauthorized is returned.")]
        public void GetProjectChildrenByProjectId_MissingTokenHeader_401Unauthorized()
        {
            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetProjectChildrenByProjectId(_project.Id, user: null);
            }, "The 'GET {0}' endpoint should return 401 Unauthorized if no Session-Token header was passed!", PATH_PROJECT_CHILDREN);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, "Token is missing or malformed.");
        }

        [TestCase]
        [TestRail(190006)]
        [Description("Executes Get project children call with a user with no permissions.  Verifies 403 Forbidden is returned.")]
        public void GetProjectChildrenByProjectId_UserWithNoPermissionsToProject_403Forbidden()
        {
            // Setup:
            var userWithoutPermission = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.None, _project);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetProjectChildrenByProjectId(_project.Id, userWithoutPermission);
            }, "The 'GET {0}' endpoint should return 403 Forbidden if called by a user with no permissions!", PATH_PROJECT_CHILDREN);

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("User does not have permissions for Project (Id:{0}).", _project.Id);
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.UnauthorizedAccess, expectedMessage);
        }

        [TestCase]
        [TestRail(125500)]
        [Description("Executes Get project children call with a non-existent project ID.  Verifies 404 Not Found is returned.")]
        public void GetProjectChildrenByProjectId_NonExistentProjectId_404NotFound()
        {
            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetProjectChildrenByProjectId(NON_EXISTING_ID, _adminUser);
            }, "The 'GET {0}' endpoint should return 404 Not Found if a non-existing project ID is passed!", PATH_PROJECT_CHILDREN);

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant(
                "The project (Id:{0}) can no longer be accessed. It may have been deleted, or is no longer accessible by you.", NON_EXISTING_ID);
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.ResourceNotFound, expectedMessage);
        }

        [TestCase]
        [TestRail(134083)]
        [Description("Executes Get project children for for project with orphan artifact. Verifies orphan artifact returned among other artifacts")]
        public void GetArtifactChildrenByProjectId_CreateOrphanArtifact_VerifyOrphanArtifactReturned()
        {
            // Setup:
            const string DELETE_ARTIFACT_ID_PATH = RestPaths.Svc.ArtifactStore.ARTIFACTS_id_;
            const string DISCARD_PATH = RestPaths.Svc.ArtifactStore.Artifacts.DISCARD;
            const string PUBLISH_PATH = RestPaths.Svc.ArtifactStore.Artifacts.PUBLISH;

            List<IArtifact> grandChildArtifacts = new List<IArtifact>();
            var parentArtifactList = CreateAndPublishParentAndTwoChildArtifactsAndGrandChildOfSecondParentArtifact_GetParents(_project, _adminUser, grandChildArtifacts);

            // Move grandchild artifact to the first parent.
            grandChildArtifacts[0].Lock();
            Helper.ArtifactStore.MoveArtifact(grandChildArtifacts[0], parentArtifactList[0], _adminUser);

            Assert.DoesNotThrow(() => Helper.ArtifactStore.DeleteArtifact(parentArtifactList[1], _adminUser),
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!", DELETE_ARTIFACT_ID_PATH);

            var artifacts = new List<IArtifactBase>();
            artifacts.AddRange(grandChildArtifacts);

            Assert.DoesNotThrow(() => Helper.ArtifactStore.PublishArtifact(parentArtifactList[1], _adminUser),
                "'POST {0}' should return 200 OK if a valid artifact ID is sent!", PUBLISH_PATH);

            Assert.DoesNotThrow(() => Helper.ArtifactStore.DiscardArtifacts(artifacts: artifacts, user: _adminUser),
                "'POST {0}' should return 200 OK if an artifact was deleted!", DISCARD_PATH);

            // Execute:
            List<NovaArtifact> returnedNovaArtifactList = null;
            Assert.DoesNotThrow(() => returnedNovaArtifactList = Helper.ArtifactStore.GetProjectChildrenByProjectId(_project.Id, _adminUser),
                "The 'GET {0}' should return 200 OK if valid parameters are passed!", PATH_PROJECT_CHILDREN);

            // Verify:
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_adminUser, grandChildArtifacts[0].Id);
            Assert.AreEqual(_project.Id, artifactDetails.ParentId, "Artifact parent Id should be the id of the project! (orphan artifact)");

            ArtifactStoreHelper.ValidateNovaArtifacts(_project, returnedNovaArtifactList);

            // Assert that grandchild artifact is returned from GetProjectChilden call
            Assert.IsNotNull(FindArtifactFromNovaArtifacts(returnedNovaArtifactList, grandChildArtifacts[0]));
        }

        #endregion GetProjectChildrenByProjectId tests

        #region GetArtifactChildrenByProjectAndArtifactId tests

        [TestCase]
        [TestRail(125511)]
        [Description("Executes Get published artifact children call for published artifact and returns 200 OK if successful")]
        public void GetArtifactChildrenByProjectAndArtifactId_PublishedParentArtifact_OK()
        {
            // Setup:
            List<IArtifact> childArtifacts = new List<IArtifact>(); 
            var parentArtifact = CreateAndPublishParentAndTwoChildArtifacts_GetParentArtifact(_project, _adminUser, childArtifacts);
            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            // Execute:
            List<NovaArtifact> returnedNovaArtifactList = null;
            Assert.DoesNotThrow(() =>
            {
                returnedNovaArtifactList = Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifact.Id, viewer);
            }, "The 'GET {0}' endpoint should return 200 OK if valid parameters are passed!", PATH_ARTIFACT_CHILDREN);

            // Verify: validate the list of artifacts returned by the REST call.
            ArtifactStoreHelper.ValidateNovaArtifacts(_project,
                novaArtifacts: returnedNovaArtifactList,
                parentArtifact: parentArtifact,
                expectedNumberOfArtifacts: 2);

            // Assert that two child artifacts under the parent artifact are returned from Get ArtifactChilden call
            var firstChildNovaArtifact = FindArtifactFromNovaArtifacts(returnedNovaArtifactList, childArtifacts[0]);
            var secondChildNovaArtifact = FindArtifactFromNovaArtifacts(returnedNovaArtifactList, childArtifacts[1]);

            ArtifactStoreHelper.AssertArtifactsEqual(childArtifacts[0], firstChildNovaArtifact, skipIdAndVersion: true);
            ArtifactStoreHelper.AssertArtifactsEqual(childArtifacts[1], secondChildNovaArtifact, skipIdAndVersion: true);
        }

        [TestCase]
        [TestRail(134074)]
        [Description("Executes Get draft artifact children call for published artifact and returns 200 OK if successful")]
        public void GetArtifactChildrenByProjectAndArtifactId_PublishedArtifactWithDraft_OK()
        {
            // Setup:
            List<IArtifact> childArtifacts = new List<IArtifact>();
            var parentArtifact = CreateAndPublishParentAndTwoChildArtifacts_GetParentArtifact(_project, _adminUser, childArtifacts);

            // Save parent to create a draft.
            parentArtifact.Save();

            // Execute:
            List<NovaArtifact> returnedNovaArtifactList = null;
            Assert.DoesNotThrow(() =>
            {
                returnedNovaArtifactList = Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifact.Id, _adminUser);
            }, "The 'GET {0}' endpoint should return 200 OK if valid parameters are passed!", PATH_ARTIFACT_CHILDREN);

            // Verify: validate the list of artifacts returned by the REST call.
            ArtifactStoreHelper.ValidateNovaArtifacts(_project,
                novaArtifacts: returnedNovaArtifactList,
                parentArtifact: parentArtifact,
                expectedNumberOfArtifacts: 2);

            // Assert that two child artifacts under the parent artifact are returned from Get ArtifactChilden call
            var firstChildNovaArtifact = FindArtifactFromNovaArtifacts(returnedNovaArtifactList, childArtifacts[0]);
            var secondChildNovaArtifact = FindArtifactFromNovaArtifacts(returnedNovaArtifactList, childArtifacts[1]);

            ArtifactStoreHelper.AssertArtifactsEqual(childArtifacts[0], firstChildNovaArtifact, skipIdAndVersion: true);
            ArtifactStoreHelper.AssertArtifactsEqual(childArtifacts[1], secondChildNovaArtifact, skipIdAndVersion: true);
        }

        [TestCase]
        [TestRail(134077)]
        [Description("Executes Get publish artifact of second level children call for published artifact and returns 200 OK if successful")]
        public void GetArtifactChildrenByProjectAndArtifactId_SecondLevelPublishedArtifactChild_OK()
        {
            // Setup:
            List<IArtifact> grandChildArtifacts = new List<IArtifact>();
            var parentArtifactList = CreateAndPublishParentAndTwoChildArtifactsAndGrandChildOfSecondParentArtifact_GetParents(_project, _adminUser, grandChildArtifacts);

            // Execute:
            List<NovaArtifact> returnedNovaArtifactList = null;
            Assert.DoesNotThrow(() =>
            {
                returnedNovaArtifactList = Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifactList[1].Id, _adminUser);
            }, "The 'GET {0}' endpoint should return 200 OK if valid parameters are passed!", PATH_ARTIFACT_CHILDREN);

            // Verify: validate the list of artifacts returned by the REST call.
            ArtifactStoreHelper.ValidateNovaArtifacts(_project,
                novaArtifacts: returnedNovaArtifactList,
                parentArtifact: parentArtifactList[1],
                expectedNumberOfArtifacts: 1);

            // Assert that grandchild artifact under the second parent artifact is returned from Get ArtifactChilden call
            var grandChildNovaArtifact = FindArtifactFromNovaArtifacts(returnedNovaArtifactList, grandChildArtifacts[0]);

            ArtifactStoreHelper.AssertArtifactsEqual(grandChildArtifacts[0], grandChildNovaArtifact, skipIdAndVersion: true);
        }

        [TestCase]
        [TestRail(134080)]
        [Description("Executes Get draft artifact of second level children call for published artifact and returns 200 OK if successful")]
        public void GetArtifactChildrenByProjectAndArtifactId_SecondLevelPublishedArtifactChildWithDraft_OK()
        {
            // Setup:
            List<IArtifact> grandChildArtifacts = new List<IArtifact>();
            var parentArtifactList = CreateAndPublishParentAndTwoChildArtifactsAndGrandChildOfSecondParentArtifact_GetParents(_project, _adminUser, grandChildArtifacts);

            // Save second parent to create a draft.
            parentArtifactList[1].Save();

            // Execute:
            List<NovaArtifact> returnedNovaArtifactList = null;
            Assert.DoesNotThrow(() =>
            {
                returnedNovaArtifactList = Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifactList[1].Id, _adminUser);
            }, "The 'GET {0}' endpoint should return 200 OK if valid parameters are passed!", PATH_ARTIFACT_CHILDREN);

            // Verify: validate the list of artifacts returned by the REST call.
            ArtifactStoreHelper.ValidateNovaArtifacts(_project,
                novaArtifacts: returnedNovaArtifactList,
                parentArtifact: parentArtifactList[1],
                expectedNumberOfArtifacts: 1);

            // Assert that grandchild artifact under the second parent artifact is returned from Get ArtifactChilden call
            var grandChildNovaArtifact = FindArtifactFromNovaArtifacts(returnedNovaArtifactList, grandChildArtifacts[0]);
            ArtifactStoreHelper.AssertArtifactsEqual(grandChildArtifacts[0], grandChildNovaArtifact, skipIdAndVersion: true);
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(267024)]
        [Description("Executes Get artifact children call after creating an artifact in the project to which user has no read permissions. " +
            "Verify that the returned result does not include artifact to which user does not have permissions.")]
        public void GetArtifactChildrenByProjectAndArtifactId_CreateArtifact_UserHasNoPermissions_VerifyResultsHaveNoArtifact(BaseArtifactType artifactType)
        {
            // Setup:
            var folder = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.PrimitiveFolder);
            var visibleArtifact = Helper.CreateAndPublishArtifact(_project, _adminUser, artifactType, folder);
            var invisibleArtifact = Helper.CreateAndPublishArtifact(_project, _adminUser, artifactType, folder);

            var userWithNoPermissionsToArtifact = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithNoPermissionsToArtifact, TestHelper.ProjectRole.None, _project, invisibleArtifact);

            // Execute:
            List<NovaArtifact> returnedNovaArtifactList = null;
            Assert.DoesNotThrow(() => returnedNovaArtifactList = Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(
                _project.Id, folder.Id, userWithNoPermissionsToArtifact),
                "The 'GET {0}' endpoint should return 200 OK if valid parameters are passed!", PATH_PROJECT_CHILDREN);

            // Verify:
            var artifact = FindArtifactFromNovaArtifacts(returnedNovaArtifactList, visibleArtifact);
            ArtifactStoreHelper.AssertArtifactsEqual(visibleArtifact, artifact, skipIdAndVersion: true);
            Assert.AreEqual(artifact.ParentId, folder.Id, "Artifact parent id is not correct!");

            Assert.IsNull(returnedNovaArtifactList.Find(a => a.Id.Equals(invisibleArtifact.Id)), "Artifact should not be in the list of returned artifacts!");
        }

        [TestCase]
        [TestRail(134072)]
        [Description("Executes Get published artifact children call and returns 401 Unauthorized if successful")]
        public void GetArtifactChildrenByProjectAndArtifactId_InvalidToken_401Unauthorized()
        {
            // Setup:
            List<IArtifact> childArtifacts = new List<IArtifact>();
            var parentArtifact = CreateAndPublishParentAndTwoChildArtifacts_GetParentArtifact(_project, _adminUser, childArtifacts);
            var unauthorizedUser = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifact.Id, unauthorizedUser);
            }, "The 'GET {0}' endpoint should return 401 Unauthorized if an unauthorized token is passed!", PATH_ARTIFACT_CHILDREN);

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, "Token is invalid.");
        }

        [TestCase]
        [TestRail(134073)]
        [Description("Executes Get published artifact children call but don't send a Session-Token header.  Verify it returns '401 Unauthorized'.")]
        public void GetArtifactChildrenByProjectAndArtifactId_MissingTokenHeader_401Unauthorized()
        {
            // Setup:
            List<IArtifact> childArtifacts = new List<IArtifact>();
            var parentArtifact = CreateAndPublishParentAndTwoChildArtifacts_GetParentArtifact(_project, _adminUser, childArtifacts);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifact.Id, user: null);
            }, "The 'GET {0}' endpoint should return 401 Unauthorized if no Session-Token header was passed!", PATH_ARTIFACT_CHILDREN);

            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, "Token is missing or malformed.");
        }

        [TestCase]
        [TestRail(190009)]
        [Description("Executes Get project children call with a user with no permissions to the project.  Verifies 403 Forbidden is returned.")]
        public void GetArtifactChildrenByProjectAndArtifactId_UserWithNoPermissionsToProject_403Forbidden()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Actor);
            var userWithoutPermission = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.None, _project);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, artifact.Id, userWithoutPermission);
            }, "The 'GET {0}' endpoint should return 403 Forbidden if called by a user with no permissions!", PATH_PROJECT_CHILDREN);

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("User does not have permissions for Artifact (Id:{0}).", artifact.Id);
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.UnauthorizedAccess, expectedMessage);
        }

        [TestCase]
        [TestRail(190007)]
        [Description("Executes Get project children call with a user with no permissions to the artifact.  Verifies 403 Forbidden is returned.")]
        public void GetArtifactChildrenByProjectAndArtifactId_UserWithNoPermissionsToArtifact_403Forbidden()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Actor);
            var userWithoutPermission = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithoutPermission, TestHelper.ProjectRole.None, _project, artifact);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, artifact.Id, userWithoutPermission);
            }, "The 'GET {0}' endpoint should return 403 Forbidden if called by a user with no permissions!", PATH_PROJECT_CHILDREN);

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("User does not have permissions for Artifact (Id:{0}).", artifact.Id);
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.UnauthorizedAccess, expectedMessage);
        }

        [TestCase]
        [TestRail(190008)]
        [Description("Executes Get artifact children call for Project that does not exists and returns 404 Not Found if successful")]
        public void GetArtifactChildrenByProjectAndArtifactId_NonExistentProjectId_404NotFound()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Actor);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(NON_EXISTING_ID, artifact.Id, _adminUser);
            }, "The 'GET {0}' endpoint should return 404 Not Found if a non-existing project ID is passed!", PATH_ARTIFACT_CHILDREN);

            // Verify:
            var expectedMessage = I18NHelper.FormatInvariant("Artifact (Id:{0}) in Project (Id:{1}) is not found.", artifact.Id, NON_EXISTING_ID);
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.ResourceNotFound, expectedMessage);
        }

        [TestCase]
        [TestRail(134071)]
        [Description("Executes Get artifact children call for artifact that does not exists and returns 404 Not Found if successful")]
        public void GetArtifactChildrenByProjectAndArtifactId_NonExistentArtifactId_404NotFound()
        {
            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, NON_EXISTING_ID, _adminUser);
            }, "The 'GET {0}' endpoint should return 404 Not Found if a non-existing artifact ID is passed!", PATH_ARTIFACT_CHILDREN);

            // Verify:
            var expectedMessage = I18NHelper.FormatInvariant("Artifact (Id:{0}) in Project (Id:{1}) is not found.", NON_EXISTING_ID, _project.Id);
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.ResourceNotFound, expectedMessage);
        }

        #endregion GetArtifactChildrenByProjectAndArtifactId tests

        #region Private functions

        /// <summary>
        /// Create and publish a parent artifact with 2 child artifacts, then return the parent.
        /// </summary>
        /// <param name="project">The project where the artifacts should be created.</param>
        /// <param name="user">The user to create the artifacts.</param>
        /// <param name="childArtifacts">child artifacts that are created from this call.</param>
        /// <returns>The parent artifact.</returns>
        private IArtifact CreateAndPublishParentAndTwoChildArtifacts_GetParentArtifact(IProject project, IUser user, List<IArtifact> childArtifacts)
        {
            // Create parent artifact with ArtifactType and populate all required values without properties.
            var parentArtifact = Helper.CreateAndPublishArtifact(project, user, BaseArtifactType.Document);

            // Create first child artifact with ArtifactType and populate all required values without properties.
            childArtifacts.Add(Helper.CreateAndPublishArtifact(project, user, BaseArtifactType.Document, parentArtifact));

            // Create second child artifact with ArtifactType and populate all required values without properties.
            childArtifacts.Add(Helper.CreateAndPublishArtifact(project, user, BaseArtifactType.Document, parentArtifact));

            return parentArtifact;
        }

        /// <summary>
        /// Create and publish a parent artifact (top level) with 2 child artifacts, then add a grandchild artifact to one of the child artifacts and publish it,
        /// then return the two parent artifacts.
        /// </summary>
        /// <param name="project">The project where the artifacts should be created.</param>
        /// <param name="user">The user to create the artifacts.</param>
        /// <param name="grandChildArtifacts">The grandchild artifact created from this call.</param>
        /// <returns>The two parent artifacts.</returns>
        private List<IArtifact> CreateAndPublishParentAndTwoChildArtifactsAndGrandChildOfSecondParentArtifact_GetParents(IProject project, IUser user, List<IArtifact> grandChildArtifacts)
        {
            var parentArtifactList = new List<IArtifact>();

            // Create grand parent artifact with ArtifactType and populate all required values without properties.
            var grandParentArtifact = Helper.CreateAndPublishArtifact(project, user, BaseArtifactType.Document);

            // Create first parent artifact with ArtifactType and populate all required values without properties.
            var parentArtifact = Helper.CreateAndPublishArtifact(project, user, BaseArtifactType.Document, grandParentArtifact);
            parentArtifactList.Add(parentArtifact);

            // Create second parent artifact with ArtifactType and populate all required values without properties.
            parentArtifact = Helper.CreateAndPublishArtifact(project, user, BaseArtifactType.Document, grandParentArtifact);
            parentArtifactList.Add(parentArtifact);

            // Create child artifact of second parent with ArtifactType and populate all required values without properties.
            grandChildArtifacts.Add(Helper.CreateAndPublishArtifact(project, user, BaseArtifactType.Document, parentArtifact));

            return parentArtifactList;
        }

        /// <summary>
        /// Find artifact from nova aritfact list 
        /// </summary>
        /// <param name="novaArtifacts">List of novaArtifacts</param>
        /// <param name="artifactToFind">the artifact to find from the novaArtifacts</param>
        /// <returns>the nova artifact found from novaArtifacts</returns>
        private static NovaArtifact FindArtifactFromNovaArtifacts(
            List<NovaArtifact> novaArtifacts,
            IArtifactBase artifactToFind)
        {
            ThrowIf.ArgumentNull(novaArtifacts, nameof(novaArtifacts));
            ThrowIf.ArgumentNull(artifactToFind, nameof(artifactToFind));

            var returnedNovaArtifact = novaArtifacts.Find(a => a.Id.Equals(artifactToFind.Id));

            string path = I18NHelper.FormatInvariant(PATH_PROJECT_CHILDREN, artifactToFind.ProjectId);
            Assert.IsNotNull(returnedNovaArtifact, "The returned result from GET {0} doesn't contain the published artifact whose Id is {1}.", path, artifactToFind.Id);

            return returnedNovaArtifact;
        }

        #endregion Private functions
    }
}
