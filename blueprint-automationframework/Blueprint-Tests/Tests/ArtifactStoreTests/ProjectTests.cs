using Helper;
using Model;
using NUnit.Framework;
using CustomAttributes;
using System.Collections.Generic;
using TestCommon;
using Model.ArtifactModel;
using Model.Factories;
using Utilities;

namespace ArtifactStoreTests 
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ProjectTests : TestBase
    {
        private const int NON_EXISTING_PROJECT_ID = int.MaxValue;
        private const string REST_PATH_ARTIFACT = RestPaths.Svc.ArtifactStore.Projects_id_.ARTIFACTS_id_;
        private const string REST_PATH_CHILDREN = RestPaths.Svc.ArtifactStore.Projects_id_.Artifacts_id_.CHILDREN;

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

        #region GetProjectChildrenByProjectId

        [TestCase]
        [TestRail(125497)]
        [Description("Executes Get project children call and returns 200 OK if successful")]
        public void GetProjectChildrenByProjectId_OK()
        {
            // Setup:
            IUser viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            // Execute & Verify:
            Assert.DoesNotThrow(() =>
            {
                Helper.ArtifactStore.GetProjectChildrenByProjectId(_project.Id, viewer);
            }, "The 'GET {0}' endpoint should return 200 OK if valid parameters are passed!", REST_PATH_ARTIFACT);
        }

        [TestCase]
        [TestRail(125500)]
        [Description("Executes Get project children call and returns 404 Not Found if successful")]
        public void GetProjectChildrenByProjectId_NotFound()
        {
            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetProjectChildrenByProjectId(NON_EXISTING_PROJECT_ID, _user);
            }, "The 'GET {0}' endpoint should return 404 Not Found if a non-existing project ID is passed!", REST_PATH_ARTIFACT);
        }

        [TestCase]
        [TestRail(125501)]
        [Description("Executes Get project children call and returns 401 Unauthorized if successful")]
        public void GetProjectChildrenByProjectId_Unauthorized()
        {
            // Setup:
            IUser unauthorizedUser = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetProjectChildrenByProjectId(_project.Id, unauthorizedUser);
            }, "The 'GET {0}' endpoint should return 401 Unauthorized if an unauthorized token is passed!", REST_PATH_ARTIFACT);
        }
        
        [TestCase]
        [TestRail(125502)]
        [Description("Executes Get project children call and returns 'Bad Request' if successful")]
        public void GetProjectChildrenByProjectId_BadRequest()
        {
            // Execute & Verify:
            Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.ArtifactStore.GetProjectChildrenByProjectId(_project.Id, user: null);
            }, "The 'GET {0}' endpoint should return 400 Bad Request if no Session-Token header was passed!", REST_PATH_ARTIFACT);
        }

        #endregion GetProjectChildrenByProjectId

        #region GetArtifactChildrenByProjectAndArtifactId Published

        [TestCase]
        [TestRail(125511)]
        [Description("Executes Get published artifact children call for published artifact and returns 200 OK if successful")]
        public void GetPublishedArtifactChildrenByProjectAndArtifactId_OK()
        {
            // Setup:
            var parentArtifact = CreateAndPublishParentAndTwoChildArtifacts_GetParentArtifact(_project, _user);
            IUser viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            // Execute & Verify:
            Assert.DoesNotThrow(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifact.Id, viewer);
            }, "The 'GET {0}' endpoint should return 200 OK if valid parameters are passed!", REST_PATH_CHILDREN);
        }

        [TestCase]
        [TestRail(134071)]
        [Description("Executes Get artifact children call for artifact that does not exists and returns 404 Not Found if successful")]
        public void GetPublishedArtifactChildrenByProjectAndArtifactId_NotFound()
        {
            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, NON_EXISTING_PROJECT_ID, _user);
            }, "The 'GET {0}' endpoint should return 404 Not Found if a non-existing project ID is passed!", REST_PATH_CHILDREN);
        }

        [TestCase]
        [TestRail(134072)]
        [Description("Executes Get published artifact children call and returns 401 Unauthorized if successful")]
        public void GetPublishedArtifactChildrenByProjectAndArtifactId_Unauthorized()
        {
            // Setup:
            var parentArtifact = CreateAndPublishParentAndTwoChildArtifacts_GetParentArtifact(_project, _user);
            IUser unauthorizedUser = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifact.Id, unauthorizedUser);
            }, "The 'GET {0}' endpoint should return 401 Unauthorized if an unauthorized token is passed!", REST_PATH_CHILDREN);
        }

        [TestCase]
        [TestRail(134073)]
        [Description("Executes Get published artifact children call and returns 'Bad Request' if successful")]
        public void GetPublishedArtifactChildrenByProjectAndArtifactId_BadRequest()
        {
            // Setup:
            var parentArtifact = CreateAndPublishParentAndTwoChildArtifacts_GetParentArtifact(_project, _user);

            // Execute & Verify:
            Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifact.Id, user: null);
            }, "The 'GET {0}' endpoint should return 400 Bad Request if no Session-Token header was passed!", REST_PATH_CHILDREN);
        }

        #endregion GetArtifactChildrenByProjectAndArtifactId Published

        #region GetArtifactChildrenByProjectAndArtifactId Published with Draft

        [TestCase]
        [TestRail(134074)]
        [Description("Executes Get draft artifact children call for published artifact and returns 200 OK if successful")]
        public void GetPublishedWithDraftArtifactChildrenByProjectAndArtifactId_OK()
        {
            // Setup:
            var parentArtifact = CreateAndPublishParentAndTwoChildArtifacts_GetParentArtifact(_project, _user);

            // Save parent to create a draft.
            parentArtifact.Save();

            // Execute & Verify:
            Assert.DoesNotThrow(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifact.Id, _user);
            }, "The 'GET {0}' endpoint should return 200 OK if valid parameters are passed!", REST_PATH_CHILDREN);
        }

        [TestCase]
        [TestRail(134075)]
        [Description("Executes Get draft artifact children call and returns 401 Unauthorized if successful")]
        public void GetPublishedWithDraftArtifactChildrenByProjectAndArtifactId_Unauthorized()
        {
            // Setup:
            var parentArtifact = CreateAndPublishParentAndTwoChildArtifacts_GetParentArtifact(_project, _user);

            // Save parent to create a draft.
            parentArtifact.Save();

            IUser unauthorizedUser = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifact.Id, unauthorizedUser);
            }, "The 'GET {0}' endpoint should return 401 Unauthorized if an unauthorized token is passed!", REST_PATH_CHILDREN);
        }

        [TestCase]
        [TestRail(134076)]
        [Description("Executes Get draft artifact children call and returns 'Bad Request' if successful")]
        public void GetPublishedWithDraftArtifactChildrenByProjectAndArtifactId_BadRequest()
        {
            // Setup:
            var parentArtifact = CreateAndPublishParentAndTwoChildArtifacts_GetParentArtifact(_project, _user);

            // Save parent to create a draft.
            parentArtifact.Save();

            // Execute & Verify:
            Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifact.Id, user: null);
            }, "The 'GET {0}' endpoint should return 400 Bad Request if no Session-Token header was passed!", REST_PATH_CHILDREN);
        }

        #endregion GetArtifactChildrenByProjectAndArtifactId Published with Draft

        #region GetArtifactChildrenByProjectAndArtifactId Published (2nd level)

        [TestCase]
        [TestRail(134077)]
        [Description("Executes Get publish artifact of second level children call for published artifact and returns 200 OK if successful")]
        public void GetSecondLevelPublishedArtifactChildrenByProjectAndArtifactId_OK()
        {
            // Setup:
            var parentArtifactList = CreateAndPublishParentAndTwoChildArtifactsAndGrandChildOfSecondParentArtifact_GetParents(_project, _user);

            // Execute & Verify:
            Assert.DoesNotThrow(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifactList[1].Id, _user);
            }, "The 'GET {0}' endpoint should return 200 OK if valid parameters are passed!", REST_PATH_CHILDREN);
        }

        [TestCase]
        [TestRail(134078)]
        [Description("Executes Get publish artifact of second level children call and returns 401 Unauthorized if successful")]
        public void GetSecondLevelPublishedArtifactChildrenByProjectAndArtifactId_Unauthorized()
        {
            // Setup:
            var parentArtifactList = CreateAndPublishParentAndTwoChildArtifactsAndGrandChildOfSecondParentArtifact_GetParents(_project, _user);
            IUser unauthorizedUser = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifactList[1].Id, unauthorizedUser);
            }, "The 'GET {0}' endpoint should return 401 Unauthorized if an unauthorized token is passed!", REST_PATH_CHILDREN);
        }

        [TestCase]
        [TestRail(134079)]
        [Description("Executes Get publish artifact of second level children call and returns 'Bad Request' if successful")]
        public void GetSecondLevelPublishedArtifactChildrenByProjectAndArtifactId_BadRequest()
        {
            // Setup:
            var parentArtifactList = CreateAndPublishParentAndTwoChildArtifactsAndGrandChildOfSecondParentArtifact_GetParents(_project, _user);

            // Execute & Verify:
            Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifactList[1].Id, user: null);
            }, "The 'GET {0}' endpoint should return 400 Bad Request if no Session-Token header was passed!", REST_PATH_CHILDREN);
        }

        #endregion GetArtifactChildrenByProjectAndArtifactId Published (2nd level)

        #region GetArtifactChildrenByProjectAndArtifactId Published with Draft (2nd level)

        [TestCase]
        [TestRail(134080)]
        [Description("Executes Get draft artifact of second level children call for published artifact and returns 200 OK if successful")]
        public void GetSecondLevelPublishedWithDraftArtifactChildrenByProjectAndArtifactId_OK()
        {
            // Setup:
            var parentArtifactList = CreateAndPublishParentAndTwoChildArtifactsAndGrandChildOfSecondParentArtifact_GetParents(_project, _user);

            // Save second parent to create a draft.
            parentArtifactList[1].Save();

            // Execute & Verify:
            Assert.DoesNotThrow(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifactList[1].Id, _user);
            }, "The 'GET {0}' endpoint should return 200 OK if valid parameters are passed!", REST_PATH_CHILDREN);
        }

        [TestCase]
        [TestRail(134081)]
        [Description("Executes Get draft artifact of second level children call and returns 401 Unauthorized if successful")]
        public void GetSecondLevelPublishedWithDraftArtifactChildrenByProjectAndArtifactId_Unauthorized()
        {
            // Setup:
            var parentArtifactList = CreateAndPublishParentAndTwoChildArtifactsAndGrandChildOfSecondParentArtifact_GetParents(_project, _user);

            // Save second parent to create a draft.
            parentArtifactList[1].Save();

            IUser unauthorizedUser = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifactList[1].Id, unauthorizedUser);
            }, "The 'GET {0}' endpoint should return 401 Unauthorized if an unauthorized token is passed!", REST_PATH_CHILDREN);
        }

        [TestCase]
        [TestRail(134082)]
        [Description("Executes Get draft artifact of second level children call and returns 'Bad Request' if successful")]
        public void GetSecondLevelPublishedWithDraftArtifactChildrenByProjectAndArtifactId_BadRequest()
        {
            // Setup:
            var parentArtifactList = CreateAndPublishParentAndTwoChildArtifactsAndGrandChildOfSecondParentArtifact_GetParents(_project, _user);

            // Save second parent to create a draft.
            parentArtifactList[1].Save();

            // Execute & Verify:
            Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifactList[1].Id, user: null);
            }, "The 'GET {0}' endpoint should return 400 Bad Request if no Session-Token header was passed!", REST_PATH_CHILDREN);
        }

        #endregion GetArtifactChildrenByProjectAndArtifactId Published with Draft (2nd level)

        #region GetArtifactChildrenByProjectAndArtifactId (moved)

        [TestCase]
        [TestRail(134083)]
        [Description("Executes Get publish artifact of second level children call for published artifact, creates orphan artifact and returns 200 OK if successful")]
        public void GetChildrenOfMovedArtifactId_OK()
        {
            // Setup:
            var parentArtifactList = CreateAndPublishParentAndTwoChildArtifactsAndGrandChildOfSecondParentArtifact_GetParents(_project, _user);

            // Move second parent below the first parent.
            parentArtifactList[1].Lock();
            Helper.ArtifactStore.MoveArtifact(parentArtifactList[1], parentArtifactList[0], _user);
            parentArtifactList[1].Publish();

            // Execute & Verify:
            Assert.DoesNotThrow(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifactList[1].Id, _user);
            }, "The 'GET {0}' endpoint should return 200 OK if valid parameters are passed!", REST_PATH_CHILDREN);
        }

        [TestCase]
        [TestRail(134084)]
        [Description("Executes Get publish artifact of second level children call and returns 401 Unauthorized if successful")]
        public void GetChildrenOfMovedArtifactId_Unauthorized()
        {
            // Setup:
            var parentArtifactList = CreateAndPublishParentAndTwoChildArtifactsAndGrandChildOfSecondParentArtifact_GetParents(_project, _user);

            // Move second parent below the first parent.
            parentArtifactList[1].Lock();
            Helper.ArtifactStore.MoveArtifact(parentArtifactList[1], parentArtifactList[0], _user);
            parentArtifactList[1].Publish();

            IUser unauthorizedUser = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            // Execute & Verify:
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifactList[1].Id, unauthorizedUser);
            }, "The 'GET {0}' endpoint should return 401 Unauthorized if an unauthorized token is passed!", REST_PATH_CHILDREN);
        }

        [TestCase]
        [TestRail(134085)]
        [Description("Executes Get publish artifact of second level children call and returns 'Bad Request' if successful")]
        public void GetChildrenOfMovedArtifactId_BadRequest()
        {
            // Setup:
            var parentArtifactList = CreateAndPublishParentAndTwoChildArtifactsAndGrandChildOfSecondParentArtifact_GetParents(_project, _user);

            // Move second parent below the first parent.
            parentArtifactList[1].Lock();
            Helper.ArtifactStore.MoveArtifact(parentArtifactList[1], parentArtifactList[0], _user);
            parentArtifactList[1].Publish();

            // Execute & Verify:
            Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.ArtifactStore.GetArtifactChildrenByProjectAndArtifactId(_project.Id, parentArtifactList[1].Id, user: null);
            }, "The 'GET {0}' endpoint should return 400 Bad Request if no Session-Token header was passed!", REST_PATH_CHILDREN);
        }

        #endregion GetArtifactChildrenByProjectAndArtifactId (moved)

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
