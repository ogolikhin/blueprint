﻿using Common;
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
        private const int NON_EXISTING_PROJECT_ID = int.MaxValue;
        private const string PATH_PROJECT_CHILDREN = RestPaths.Svc.ArtifactStore.Projects_id_.CHILDREN;
        private const string REST_ARTIFACT_CHILDREN = RestPaths.Svc.ArtifactStore.Projects_id_.Artifacts_id_.CHILDREN;

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

        [TestCase]
        [TestRail(125497)]
        [Description("Executes Get project children call and returns 200 OK if successful")]
        public void GetProjectChildrenByProjectId_OK()
        {
            // Setup:
            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            // Execute & Verify:
            List<NovaArtifact> returnedNovaArtifactList = null;
            Assert.DoesNotThrow(() =>
            {
                returnedNovaArtifactList = Helper.ArtifactStore.GetProjectChildrenByProjectId(_project.Id, viewer);
            }, "The 'GET {0}' endpoint should return 200 OK if valid parameters are passed!", PATH_PROJECT_CHILDREN);

            // Verify: validate the list of artifacts returned by the REST call.
            ArtifactStoreHelper.ValidateNovaArtifacts(_project, novaArtifactList: returnedNovaArtifactList);
        }

        [TestCase]
        [TestRail(125500)]
        [Description("Executes Get project children call with a non-existent project ID.  Verifies 404 Not Found is returned.")]
        public void GetProjectChildrenByProjectId_NonExistentProjectId_404NotFound()
        {
            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetProjectChildrenByProjectId(NON_EXISTING_PROJECT_ID, _adminUser);
            }, "The 'GET {0}' endpoint should return 404 Not Found if a non-existing project ID is passed!", PATH_PROJECT_CHILDREN);
        }

        [TestCase]
        [TestRail(190006)]
        [Description("Executes Get project children call with a user with no permissions.  Verifies 403 Forbidden is returned.")]
        public void GetProjectChildrenByProjectId_UserWithNoPermissionsToProject_403Forbidden()
        {
            // Setup:
            var userWithoutPermission = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.None, _project);

            // Execute & Verify:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetProjectChildrenByProjectId(_project.Id, userWithoutPermission);
            }, "The 'GET {0}' endpoint should return 403 Forbidden if called by a user with no permissions!", PATH_PROJECT_CHILDREN);

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("User does not have permissions for Project (Id:{0}).", _project.Id);
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.UnauthorizedAccess, expectedMessage);
        }

        [TestCase]
        [TestRail(125501)]
        [Description("Executes Get project children call with an invalid token.  Verifies 401 Unauthorized is returned.")]
        public void GetProjectChildrenByProjectId_InvalidToken_401Unauthorized()
        {
            // Setup:
            var unauthorizedUser = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetProjectChildrenByProjectId(_project.Id, unauthorizedUser);
            }, "The 'GET {0}' endpoint should return 401 Unauthorized if an unauthorized token is passed!", PATH_PROJECT_CHILDREN);
        }
        
        [TestCase]
        [TestRail(125502)]
        [Description("Executes Get project children call with no Session-Token header.  Verifies 401 Unauthorized is returned.")]
        public void GetProjectChildrenByProjectId_MissingTokenHeader_401Unauthorized()
        {
            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetProjectChildrenByProjectId(_project.Id, user: null);
            }, "The 'GET {0}' endpoint should return 401 Unauthorized if no Session-Token header was passed!", PATH_PROJECT_CHILDREN);
        }

        #endregion GetProjectChildrenByProjectId tests

        #region GetArtifactChildrenByProjectAndArtifactId tests

        [TestCase]
        [TestRail(125511)]
        [Description("Executes Get published artifact children call for published artifact and returns 200 OK if successful")]
        public void GetArtifactChildrenByProjectAndArtifactId_PublishedParentArtifact_OK()
        {
            // Setup:
            var parentArtifact = CreateAndPublishParentAndTwoChildArtifacts_GetParentArtifact(_project, _adminUser);
            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            // Execute & Verify:
            List<NovaArtifact> returnedNovaArtifactList = null;
            Assert.DoesNotThrow(() =>
            {
                returnedNovaArtifactList = Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifact.Id, viewer);
            }, "The 'GET {0}' endpoint should return 200 OK if valid parameters are passed!", REST_ARTIFACT_CHILDREN);

            // Verify: validatie the list of artifacts returned by the REST call.
            ArtifactStoreHelper.ValidateNovaArtifacts(_project,
                novaArtifactList: returnedNovaArtifactList,
                parentArtifact: parentArtifact,
                expectedNumberOfArtifacts: 2);
        }

        [TestCase]
        [TestRail(190008)]
        [Description("Executes Get artifact children call for Project that does not exists and returns 404 Not Found if successful")]
        public void GetArtifactChildrenByProjectAndArtifactId_NonExistentProjectId_404NotFound()
        {
            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, NON_EXISTING_PROJECT_ID, _adminUser);
            }, "The 'GET {0}' endpoint should return 404 Not Found if a non-existing project ID is passed!", REST_ARTIFACT_CHILDREN);
        }

        [TestCase]
        [TestRail(134071)]
        [Description("Executes Get artifact children call for artifact that does not exists and returns 404 Not Found if successful")]
        public void GetArtifactChildrenByProjectAndArtifactId_NonExistentArtifactId_404NotFound()
        {
            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, NON_EXISTING_PROJECT_ID, _adminUser);
            }, "The 'GET {0}' endpoint should return 404 Not Found if a non-existing project ID is passed!", REST_ARTIFACT_CHILDREN);
        }

        [TestCase]
        [TestRail(190009)]
        [Description("Executes Get project children call with a user with no permissions to the project.  Verifies 403 Forbidden is returned.")]
        public void GetArtifactChildrenByProjectAndArtifactId_UserWithNoPermissionsToProject_403Forbidden()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.Actor);
            var userWithoutPermission = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.None, _project);

            // Execute & Verify:
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

            // Execute & Verify:
            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, artifact.Id, userWithoutPermission);
            }, "The 'GET {0}' endpoint should return 403 Forbidden if called by a user with no permissions!", PATH_PROJECT_CHILDREN);

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("User does not have permissions for Artifact (Id:{0}).", artifact.Id);
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.UnauthorizedAccess, expectedMessage);
        }

        [TestCase]
        [TestRail(134072)]
        [Description("Executes Get published artifact children call and returns 401 Unauthorized if successful")]
        public void GetArtifactChildrenByProjectAndArtifactId_InvalidToken_401Unauthorized()
        {
            // Setup:
            var parentArtifact = CreateAndPublishParentAndTwoChildArtifacts_GetParentArtifact(_project, _adminUser);
            var unauthorizedUser = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifact.Id, unauthorizedUser);
            }, "The 'GET {0}' endpoint should return 401 Unauthorized if an unauthorized token is passed!", REST_ARTIFACT_CHILDREN);
        }

        [TestCase]
        [TestRail(134073)]
        [Description("Executes Get published artifact children call but don't send a Session-Token header.  Verify it returns '401 Unauthorized'.")]
        public void GetArtifactChildrenByProjectAndArtifactId_MissingTokenHeader_401Unauthorized()
        {
            // Setup:
            var parentArtifact = CreateAndPublishParentAndTwoChildArtifacts_GetParentArtifact(_project, _adminUser);

            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifact.Id, user: null);
            }, "The 'GET {0}' endpoint should return 401 Unauthorized if no Session-Token header was passed!", REST_ARTIFACT_CHILDREN);
        }

        [TestCase]
        [TestRail(134074)]
        [Description("Executes Get draft artifact children call for published artifact and returns 200 OK if successful")]
        public void GetArtifactChildrenByProjectAndArtifactId_PublishedArtifactWithDraft_OK()
        {
            // Setup:
            var parentArtifact = CreateAndPublishParentAndTwoChildArtifacts_GetParentArtifact(_project, _adminUser);

            // Save parent to create a draft.
            parentArtifact.Save();

            // Execute & Verify:
            List<NovaArtifact> returnedNovaArtifactList = null;
            Assert.DoesNotThrow(() =>
            {
                returnedNovaArtifactList = Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifact.Id, _adminUser);
            }, "The 'GET {0}' endpoint should return 200 OK if valid parameters are passed!", REST_ARTIFACT_CHILDREN);

            // Verify: validate the list of artifacts returned by the REST call.
            ArtifactStoreHelper.ValidateNovaArtifacts(_project,
                novaArtifactList: returnedNovaArtifactList,
                parentArtifact: parentArtifact,
                expectedNumberOfArtifacts: 2);
        }

        [TestCase]
        [TestRail(134077)]
        [Description("Executes Get publish artifact of second level children call for published artifact and returns 200 OK if successful")]
        public void GetArtifactChildrenByProjectAndArtifactId_SecondLevelPublishedArtifactChild_OK()
        {
            // Setup:
            var parentArtifactList = CreateAndPublishParentAndTwoChildArtifactsAndGrandChildOfSecondParentArtifact_GetParents(_project, _adminUser);

            // Execute & Verify:
            List<NovaArtifact> returnedNovaArtifactList = null;
            Assert.DoesNotThrow(() =>
            {
                returnedNovaArtifactList = Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifactList[1].Id, _adminUser);
            }, "The 'GET {0}' endpoint should return 200 OK if valid parameters are passed!", REST_ARTIFACT_CHILDREN);

            // Verify: validate the list of artifacts returned by the REST call.
            ArtifactStoreHelper.ValidateNovaArtifacts(_project,
                novaArtifactList: returnedNovaArtifactList,
                parentArtifact: parentArtifactList[1],
                expectedNumberOfArtifacts: 1);
        }

        [TestCase]
        [TestRail(134080)]
        [Description("Executes Get draft artifact of second level children call for published artifact and returns 200 OK if successful")]
        public void GetArtifactChildrenByProjectAndArtifactId_SecondLevelPublishedArtifactChildWithDraft_OK()
        {
            // Setup:
            var parentArtifactList = CreateAndPublishParentAndTwoChildArtifactsAndGrandChildOfSecondParentArtifact_GetParents(_project, _adminUser);

            // Save second parent to create a draft.
            parentArtifactList[1].Save();

            // Execute & Verify:
            List<NovaArtifact> returnedNovaArtifactList = null;
            Assert.DoesNotThrow(() =>
            {
                returnedNovaArtifactList = Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifactList[1].Id, _adminUser);
            }, "The 'GET {0}' endpoint should return 200 OK if valid parameters are passed!", REST_ARTIFACT_CHILDREN);

            // Verify: validate the list of artifacts returned by the REST call.
            ArtifactStoreHelper.ValidateNovaArtifacts(_project,
                novaArtifactList: returnedNovaArtifactList,
                parentArtifact: parentArtifactList[1],
                expectedNumberOfArtifacts: 1);
        }

        [TestCase]
        [TestRail(134083)]
        [Description("Executes Get publish artifact of second level children call for published artifact, creates orphan artifact and returns 200 OK if successful")]
        public void GetArtifactChildrenByProjectAndArtifactId_MovedArtifact_OK()
        {
            // Setup:
            var parentArtifactList = CreateAndPublishParentAndTwoChildArtifactsAndGrandChildOfSecondParentArtifact_GetParents(_project, _adminUser);

            // Move second parent below the first parent.
            parentArtifactList[1].Lock();
            Helper.ArtifactStore.MoveArtifact(parentArtifactList[1], parentArtifactList[0], _adminUser);
            parentArtifactList[1].Publish();

            // Execute & Verify:
            List<NovaArtifact> returnedNovaArtifactList = null;
            Assert.DoesNotThrow(() =>
            {
                returnedNovaArtifactList = Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifactList[1].Id, _adminUser);
            }, "The 'GET {0}' endpoint should return 200 OK if valid parameters are passed!", REST_ARTIFACT_CHILDREN);

            // Verify: validate the list of artifacts returned by the REST call.
            ArtifactStoreHelper.ValidateNovaArtifacts(
                _project,
                novaArtifactList: returnedNovaArtifactList,
                parentArtifact: parentArtifactList[1],
                expectedNumberOfArtifacts: 1);
        }

        #endregion GetArtifactChildrenByProjectAndArtifactId tests

        #region Private functions

        /// <summary>
        /// Create and publish a parent artifact with 2 child artifacts, then return the parent.
        /// </summary>
        /// <param name="project">The project where the artifacts should be created.</param>
        /// <param name="user">The user to create the artifacts.</param>
        /// <returns>The parent artifact.</returns>
        private IArtifact CreateAndPublishParentAndTwoChildArtifacts_GetParentArtifact(IProject project, IUser user)
        {
            // Create parent artifact with ArtifactType and populate all required values without properties.
            var parentArtifact = Helper.CreateAndPublishArtifact(project, user, BaseArtifactType.Document);

            // Create first child artifact with ArtifactType and populate all required values without properties.
            Helper.CreateAndPublishArtifact(project, user, BaseArtifactType.Document, parentArtifact);

            // Create second child artifact with ArtifactType and populate all required values without properties.
            Helper.CreateAndPublishArtifact(project, user, BaseArtifactType.Document, parentArtifact);

            return parentArtifact;
        }

        /// <summary>
        /// Create and publish a parent artifact (top level) with 2 child artifacts, then add a grandchild artifact to one of the child artifacts and publish it,
        /// then return the two parent artifacts.
        /// </summary>
        /// <param name="project">The project where the artifacts should be created.</param>
        /// <param name="user">The user to create the artifacts.</param>
        /// <returns>The two parent artifacts.</returns>
        private List<IArtifact> CreateAndPublishParentAndTwoChildArtifactsAndGrandChildOfSecondParentArtifact_GetParents(IProject project, IUser user)
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
            Helper.CreateAndPublishArtifact(project, user, BaseArtifactType.Document, parentArtifact);

            return parentArtifactList;
        }

        #endregion Private functions
    }
}
