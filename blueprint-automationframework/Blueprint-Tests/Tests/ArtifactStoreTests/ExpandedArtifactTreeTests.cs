using System.Collections.Generic;
using System.Linq;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using Model.StorytellerModel.Impl;
using Newtonsoft.Json;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ExpandedArtifactTreeTests : TestBase
    {
        /// <summary>
        /// This is the structure returned by the REST call to display error messages.
        /// </summary>
        public class MessageResult
        {
            public int ErrorCode { get; set; }
            public string Message { get; set; }
        }

        private const string REST_PATH = RestPaths.Svc.ArtifactStore.Projects_id_.ARTIFACTS_id_;

        private IUser _user = null;
        private IProject _project = null;

        private IProjectRole _viewerRole = null;
        private IProjectRole _noneRole = null;

        #region Setup and Teardown

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);

            _viewerRole = ProjectRoleFactory.CreateProjectRole(_project, RolePermissions.Read);
            _noneRole = ProjectRoleFactory.CreateProjectRole(_project, RolePermissions.None);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
            _noneRole.DeleteRole();
        }

        #endregion Setup and Teardown

        [TestCase(BaseArtifactType.UseCase, BaseArtifactType.UseCase, BaseArtifactType.UseCase)]
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
        [TestRail(164525)]
        [Description("Create a chain of published parent/child artifacts and other top level artifacts.  GetExpandedArtifactTree with the ID of the artifact at the bottom of the chain." +
                     "Verify a list of top level artifacts is returned and only one has children.")]
        public void GetExpandedArtifactTree_LastPublishedArtifactInChain_ReturnsExpectedArtifactHierarchy(params BaseArtifactType[] artifactTypeChain)
        {
            ThrowIf.ArgumentNull(artifactTypeChain, nameof(artifactTypeChain));

            // Setup:
            var artifactChain = Helper.CreatePublishedArtifactChain(_project, _user, artifactTypeChain);

            // Create some other top-level artifacts not part of the chain.
            var otherTopLevelArtifacts = new List<IArtifact>();
            otherTopLevelArtifacts.Add(Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor));
            otherTopLevelArtifacts.Add(Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process));

            // Execute:
            List<INovaArtifact> artifacts = null;

            Assert.DoesNotThrow(() => artifacts = Helper.ArtifactStore.GetExpandedArtifactTree(_user, _project, artifactChain.Last().Id),
                "'GET {0}' should return 200 OK when passed valid parameters!", REST_PATH);

            // Verify:
            VerifyArtifactTree(artifactChain, artifacts);
            VerifyOtherTopLevelArtifactsExist(otherTopLevelArtifacts, artifacts);
        }

        [TestCase(BaseArtifactType.Actor, BaseArtifactType.Actor, BaseArtifactType.Actor)]
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
        [TestRail(164526)]
        [Description("Create a chain of saved parent/child artifacts and other top level artifacts.  GetExpandedArtifactTree with the ID of the artifact at the bottom of the chain." +
            "Verify a list of top level artifacts is returned and only one has children.")]
        public void GetExpandedArtifactTree_LastSavedArtifactInChain_ReturnsExpectedArtifactHierarchy(params BaseArtifactType[] artifactTypeChain)
        {
            ThrowIf.ArgumentNull(artifactTypeChain, nameof(artifactTypeChain));

            // Setup:
            var artifactChain = Helper.CreatePublishedArtifactChain(_project, _user, artifactTypeChain);

            // Create some other top-level artifacts not part of the chain.
            Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process);

            // Execute:
            List<INovaArtifact> artifacts = null;

            Assert.DoesNotThrow(() => artifacts = Helper.ArtifactStore.GetExpandedArtifactTree(_user, _project, artifactChain.Last().Id),
                "'GET {0}' should return 200 OK when passed valid parameters!", REST_PATH);

            // Verify:
            VerifyArtifactTree(artifactChain, artifacts);
        }

        [TestCase(1, BaseArtifactType.Actor, BaseArtifactType.Actor, BaseArtifactType.Actor)]
        [TestCase(5, BaseArtifactType.PrimitiveFolder, // PrimitiveFolders can only have projects as parents.
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
        [TestRail(164527)]
        [Description("Create a chain of saved parent/child artifacts and other top level artifacts.  GetExpandedArtifactTree with the ID of an artifact in the middle of the chain." +
            "Verify a list of top level artifacts is returned and only one has children.")]
        public void GetExpandedArtifactTree_MiddleSavedArtifactInChain_ReturnsExpectedArtifactHierarchy(int artifactIndex, params BaseArtifactType[] artifactTypeChain)
        {
            ThrowIf.ArgumentNull(artifactTypeChain, nameof(artifactTypeChain));

            // Setup:
            var artifactChain = Helper.CreatePublishedArtifactChain(_project, _user, artifactTypeChain);

            // Create some other top-level artifacts not part of the chain.
            Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process);

            // Execute:
            List<INovaArtifact> artifacts = null;

            Assert.DoesNotThrow(() => artifacts = Helper.ArtifactStore.GetExpandedArtifactTree(_user, _project, artifactChain[artifactIndex].Id),
                "'GET {0}' should return 200 OK when passed valid parameters!", REST_PATH);

            // Verify:
            VerifyArtifactTree(artifactChain, artifacts, artifactIndex);
        }

        [TestCase(false, 2, BaseArtifactType.Actor, BaseArtifactType.BusinessProcess, BaseArtifactType.Document, BaseArtifactType.DomainDiagram, BaseArtifactType.GenericDiagram)]
        [TestCase(true, 2, BaseArtifactType.Actor, BaseArtifactType.BusinessProcess, BaseArtifactType.Document, BaseArtifactType.DomainDiagram, BaseArtifactType.GenericDiagram)]
        [TestRail(164530)]
        [Description("Create a chain of saved parent/child artifacts and other top level artifacts." +
            "GetExpandedArtifactTree with the ID of an artifact in the middle of the chain with includeChildren=[true|false]." +
            "Verify a list of top level artifacts is returned and only one has children.  If includeChildren=true, the children of the artifact whose ID you passed should also be returned.")]
        public void GetExpandedArtifactTreeWithIncludeChildren_MiddleSavedArtifactInChain_ReturnsExpectedArtifactHierarchyAndChildren(
            bool includeChildren,
            int artifactIndex,
            params BaseArtifactType[] artifactTypeChain)
        {
            ThrowIf.ArgumentNull(artifactTypeChain, nameof(artifactTypeChain));

            // Setup:
            var artifactChain = Helper.CreatePublishedArtifactChain(_project, _user, artifactTypeChain);

            // Create some other top-level artifacts not part of the chain.
            Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process);

            // Execute:
            List<INovaArtifact> artifacts = null;

            Assert.DoesNotThrow(() => artifacts = Helper.ArtifactStore.GetExpandedArtifactTree(_user, _project, artifactChain[artifactIndex].Id, includeChildren),
                "'GET {0}' should return 200 OK when passed valid parameters!", REST_PATH);

            // Verify:
            // If we are including children, increment artifactIndex so we verify that the children were also returned.
            if (includeChildren)
            {
                ++artifactIndex;
            }

            VerifyArtifactTree(artifactChain, artifacts, artifactIndex);
        }

        [TestCase("")]
        [TestCase("00000000-0000-0000-0000-000000000000")]
        [TestRail(164532)]
        [Description("Create an artifact.  GetExpandedArtifactTree with the ID of the artifact but pass an invalid token.  Verify 401 Unauthorized is returned with the correct error message.")]
        public void GetExpandedArtifactTree_InvalidToken_401Unauthorized(string token)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Actor);

            IUser userWithBadOrMissingToken = UserFactory.CreateUserAndAddToDatabase();
            userWithBadOrMissingToken.Token.SetToken(_user.Token.AccessControlToken);
            userWithBadOrMissingToken.Token.AccessControlToken = token;

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.ArtifactStore.GetExpandedArtifactTree(userWithBadOrMissingToken, _project, artifact.Id),
                "'GET {0}' should return 401 Unauthorized when a bad or empty Session-Token is passed!", REST_PATH);

            // Verify:
            MessageResult messageResult = JsonConvert.DeserializeObject<MessageResult>(ex.RestResponse.Content);
            const string expectedMessage = "Token is invalid.";

            Assert.AreEqual(expectedMessage, messageResult.Message,
                "If a bad or empty token is passed, we should get an error message of '{0}'!", expectedMessage);
        }

        [TestCase]
        [TestRail(164533)]
        [Description("Create an artifact.  GetExpandedArtifactTree with the ID of the artifact without a Session-Token header.  Verify 400 Bad Request is returned with the correct error message.")]
        public void GetExpandedArtifactTree_MissingTokenHeader_400BadRequest()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Actor);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.ArtifactStore.GetExpandedArtifactTree(user: null, project: _project, artifactId: artifact.Id),
                "'GET {0}' should return 400 Bad Request when no Session-Token header is passed!", REST_PATH);

            // Verify:
            MessageResult messageResult = JsonConvert.DeserializeObject<MessageResult>(ex.RestResponse.Content);
            const string expectedMessage = "Token is missing or malformed.";

            Assert.AreEqual(expectedMessage, messageResult.Message,
                "If no Session-Token header is passed, we should get an error message of '{0}'!", expectedMessage);
        }

        [TestCase(0)]
        [TestCase(int.MaxValue)]
        [TestRail(164534)]
        [Description("Create an artifact.  GetExpandedArtifactTree with the ID of the artifact but pass a project ID that doesn't exist.  Verify 404 Not Found is returned with the correct error message.")]
        public void GetExpandedArtifactTree_NonExistingProjectId_404NotFound(int projectId)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Actor);
            IProject nonExistingProject = ProjectFactory.CreateProject();
            nonExistingProject.Id = projectId;

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetExpandedArtifactTree(_user, nonExistingProject, artifact.Id),
                "'GET {0}' should return 404 Not Found when a non-existing Project ID is passed!", REST_PATH);

            // Verify:
            // We only send a custom error message if projectId > 0.
            if (projectId > 0)
            {
                string expectedMessage = I18NHelper.FormatInvariant("Artifact (Id:{0}) in Project (Id:{1}) is not found.", artifact.Id, projectId);

                AssertJsonResponseEquals(expectedMessage, ex.RestResponse.Content,
                    "If a non-existing Project ID is passed, we should get an error message of '{0}'!", expectedMessage);
            }
        }

        [TestCase(int.MaxValue)]
        [TestRail(164535)]
        [Description("GetExpandedArtifactTree with the ID of the artifact that doesn't exist.  Verify 404 Not Found is returned with the correct error message.")]
        public void GetExpandedArtifactTree_NonExistingArtifactId_404NotFound(int artifactId)
        {
            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetExpandedArtifactTree(_user, _project, artifactId),
                "'GET {0}' should return 404 Not Found when a non-existing Project ID is passed!", REST_PATH);

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("Artifact (Id:{0}) in Project (Id:{1}) is not found.", artifactId, _project.Id);
            AssertJsonResponseEquals(expectedMessage, ex.RestResponse.Content,
                "If a non-existing Artifact ID is passed, we should get an error message of '{0}'!", expectedMessage);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestRail(164536)]
        [Description("GetExpandedArtifactTree with an invalid artifact ID (<= 0).  Verify 400 Bad Request is returned with the correct error message.")]
        public void GetExpandedArtifactTree_InvalidArtifactId_400BadRequest(int artifactId)
        {
            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.ArtifactStore.GetExpandedArtifactTree(_user, _project, artifactId),
                "'GET {0}' should return 400 Bad Request when an invalid Artifact ID is passed!", REST_PATH);

            // Verify:
            const string expectedMessage = "Parameter expandedToArtifactId must be greater than 0.";
            AssertJsonResponseEquals(expectedMessage, ex.RestResponse.Content,
                "If an invalid Artifact ID is passed, we should get an error message of '{0}'!", expectedMessage);
        }

        [TestCase]
        [TestRail(164537)]
        [Description("GetExpandedArtifactTree with the ID of the project instead of an artifact.  Verify 404 Not Found is returned with the correct error message.")]
        public void GetExpandedArtifactTree_ArtifactIdIsAProjectId_404NotFound()
        {
            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetExpandedArtifactTree(_user, _project, _project.Id),
                "'GET {0}' should return 404 Not Found when a Project ID is passed in place of an Artifact ID!", REST_PATH);

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("Artifact (Id:{0}) in Project (Id:{1}) is not found.", _project.Id, _project.Id);
            AssertJsonResponseEquals(expectedMessage, ex.RestResponse.Content,
                "If a Project ID is passed in place of an Artifact ID, we should get an error message of '{0}'!", expectedMessage);
        }

        [TestCase]
        [TestRail(164538)]
        [Description("GetExpandedArtifactTree with the ID of the artifact instead of a project.  Verify 404 Not Found is returned with the correct error message.")]
        public void GetExpandedArtifactTree_ProjectIdIsAnArtifactId_404NotFound()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Actor);
            IProject notAProject = ProjectFactory.CreateProject();
            notAProject.Id = artifact.Id;

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetExpandedArtifactTree(_user, notAProject, artifact.Id),
                "'GET {0}' should return 404 Not Found when an Artifact ID is passed in place of a Project ID!", REST_PATH);

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("Artifact (Id:{0}) in Project (Id:{1}) is not found.", artifact.Id, artifact.Id);
            AssertJsonResponseEquals(expectedMessage, ex.RestResponse.Content,
                "If an Artifact ID is passed in place of a Project ID, we should get an error message of '{0}'!", expectedMessage);
        }

        [TestCase]
        [TestRail(164557)]
        [Description("GetExpandedArtifactTree with the ID of the sub-artifact instead of an artifact.  Verify 404 Not Found is returned with the correct error message.")]
        public void GetExpandedArtifactTree_ArtifactIdIsASubArtifact_404NotFound()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process);
            var process = Helper.Storyteller.GetProcess(_user, artifact.Id);
            var userTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetExpandedArtifactTree(_user, _project, userTask.Id),
                "'GET {0}' should return 404 Not Found when a sub-artifact ID is passed in place of an Artifact ID!", REST_PATH);

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("Artifact (Id:{0}) in Project (Id:{1}) is not found.", userTask.Id, _project.Id);
            AssertJsonResponseEquals(expectedMessage, ex.RestResponse.Content,
                "If a sub-artifact ID is passed in place of an Artifact ID, we should get an error message of '{0}'!", expectedMessage);
        }

        [TestCase]
        [TestRail(164560)]
        [Description("Create & publish an artifact, then delete & publish it.  GetExpandedArtifactTree with the ID of the deleted artifact.  Verify 404 Not Found is returned with the correct error message.")]
        public void GetExpandedArtifactTree_IdOfDeletedAndPublishedArtifact_404NotFound()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Delete();
            artifact.Publish();

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetExpandedArtifactTree(_user, _project, artifact.Id),
                "'GET {0}' should return 404 Not Found when the ID of a deleted Artifact is passed!", REST_PATH);

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("Artifact (Id:{0}) in Project (Id:{1}) is not found.", artifact.Id, _project.Id);
            AssertJsonResponseEquals(expectedMessage, ex.RestResponse.Content,
                "If the ID of a deleted Artifact is passed, we should get an error message of '{0}'!", expectedMessage);
        }

        [TestCase]
        [TestRail(164599)]
        [Description("Create & publish an artifact, then delete (but don't publish) it.  GetExpandedArtifactTree with the ID of the deleted artifact." +
            "Verify 404 Not Found is returned with the correct error message.")]
        public void GetExpandedArtifactTree_IdOfDeletedWithoutPublishArtifactSameUser_404NotFound()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            artifact.Delete();

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetExpandedArtifactTree(_user, _project, artifact.Id),
                "'GET {0}' should return 404 Not Found when the ID of a deleted Artifact is passed!", REST_PATH);

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("Artifact (Id:{0}) in Project (Id:{1}) is not found.", artifact.Id, _project.Id);
            AssertJsonResponseEquals(expectedMessage, ex.RestResponse.Content,
                "If the ID of a deleted Artifact is passed, we should get an error message of '{0}'!", expectedMessage);
        }

        [TestCase]
        [TestRail(164600)]
        [Description("Create & publish an artifact, then delete (but don't publish) it.  GetExpandedArtifactTree with the ID of the deleted artifact as a different user." +
            "Verify a list of top level artifacts is returned and only one has children.")]
        public void GetExpandedArtifactTree_IdOfDeletedWithoutPublishArtifactOtherUser_ReturnsExpectedArtifactHierarchy()
        {
            // Setup:
            IUser otherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);
            BaseArtifactType[] artifactTypeChain = new BaseArtifactType[] { BaseArtifactType.Actor, BaseArtifactType.Glossary, BaseArtifactType.Process };
            var artifactChain = Helper.CreatePublishedArtifactChain(_project, _user, artifactTypeChain);

            IArtifact artifact = artifactChain.Last();
            artifact.Delete();

            // Execute:
            List<INovaArtifact> artifacts = null;

            Assert.DoesNotThrow(() => artifacts = Helper.ArtifactStore.GetExpandedArtifactTree(otherUser, _project, artifact.Id),
                "'GET {0}' should return 200 OK when passed valid parameters!", REST_PATH);

            // Verify:
            VerifyArtifactTree(artifactChain, artifacts);
        }

        [TestCase]
        [TestRail(164601)]
        [Description("Create & save an artifact.  GetExpandedArtifactTree with the ID of the unpublished artifact (as a different user)." +
            "Verify 404 Not Found is returned with the correct error message.")]
        public void GetExpandedArtifactTree_IdOfUnpublishArtifactOtherUser_404NotFound()
        {
            // Setup:
            IUser otherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Actor);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetExpandedArtifactTree(otherUser, _project, artifact.Id),
                "'GET {0}' should return 404 Not Found when the ID of an unpublished Artifact (saved by another user) is passed!", REST_PATH);

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("Artifact (Id:{0}) in Project (Id:{1}) is not found.", artifact.Id, _project.Id);
            AssertJsonResponseEquals(expectedMessage, ex.RestResponse.Content,
                "If the ID of a deleted Artifact is passed, we should get an error message of '{0}'!", expectedMessage);
        }

        [TestCase]
        [TestRail(164558)]
        [Description("GetExpandedArtifactTree with a user that doesn't have access to the project.  Verify 403 Forbidden is returned with the correct error message.")]
        public void GetExpandedArtifactTree_UserWithoutPermissionToProject_403Forbidden()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);
            IUser userWithoutPermission = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken,
                InstanceAdminRole.BlueprintAnalytics);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.GetExpandedArtifactTree(userWithoutPermission, _project, artifact.Id),
                "'GET {0}' should return 403 Forbidden when called by a user without permission to the project!", REST_PATH);

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("User does not have permissions for Project (Id:{0}).", _project.Id);
            AssertJsonResponseEquals(expectedMessage, ex.RestResponse.Content,
                "If called by a user without permission to the project, we should get an error message of '{0}'!", expectedMessage);
        }

        [TestCase(0, BaseArtifactType.Actor, BaseArtifactType.BusinessProcess, BaseArtifactType.Document, BaseArtifactType.DomainDiagram, BaseArtifactType.GenericDiagram)]
        [TestCase(2, BaseArtifactType.Actor, BaseArtifactType.BusinessProcess, BaseArtifactType.Document, BaseArtifactType.DomainDiagram, BaseArtifactType.GenericDiagram)]
        [TestCase(4, BaseArtifactType.Actor, BaseArtifactType.BusinessProcess, BaseArtifactType.Document, BaseArtifactType.DomainDiagram, BaseArtifactType.GenericDiagram)]
        [TestRail(164559)]
        [Description("GetExpandedArtifactTree with a user that doesn't have access to the artifact.  Verify 403 Forbidden is returned with the correct error message.")]
        public void GetExpandedArtifactTree_UserWithoutPermissionToPublishedArtifact_403Forbidden(int artifactIndex, params BaseArtifactType[] artifactTypeChain)
        {
            ThrowIf.ArgumentNull(artifactTypeChain, nameof(artifactTypeChain));

            // Setup:
            var artifactChain = Helper.CreatePublishedArtifactChain(_project, _user, artifactTypeChain);

            // Create some other top-level artifacts not part of the chain.
            var otherTopLevelArtifacts = new List<IArtifact>();
            otherTopLevelArtifacts.Add(Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor));
            otherTopLevelArtifacts.Add(Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process));

            // Create a user without permission to the artifact.
            IUser userWithoutPermission = Helper.CreateUserAndAddToDatabase(instanceAdminRole: null);
            IGroup viewersGroup = Helper.CreateGroupAndAddToDatabase();

            viewersGroup.AddUser(userWithoutPermission);
            viewersGroup.AssignRoleToProjectOrArtifact(_project, role: _viewerRole);
            // XXX: Next line fails with:  The INSERT statement conflicted with the FOREIGN KEY constraint "FK_RoleRoleAssignments". The conflict occurred in database "Blueprint", table "dbo.Roles", column 'RoleId'.
            viewersGroup.AssignRoleToProjectOrArtifact(_project, _noneRole, artifactChain[artifactIndex]);
            Helper.AdminStore.AddSession(userWithoutPermission);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.GetExpandedArtifactTree(userWithoutPermission, _project, artifactChain.Last().Id),
                "'GET {0}' should return 403 Forbidden when called by a user without permission to the artifact!", REST_PATH);

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("User does not have permissions for Artifact (Id:{0}).", artifactChain.Last().Id);
            AssertJsonResponseEquals(expectedMessage, ex.RestResponse.Content,
                "If called by a user without permission to the artifact, we should get an error message of '{0}'!", expectedMessage);
        }

        #region Private functions

        /// <summary>
        /// Asserts that the returned JSON content has the specified error message.
        /// </summary>
        /// <param name="expectedMessage">The error message expected in the JSON content.</param>
        /// <param name="jsonContent">The JSON content.</param>
        /// <param name="assertMessage">The message to display if the expected message isn't found in the JSON content.</param>
        /// <param name="assertMessageParams">(optional) Parameters to use if assertMessage is a format string.</param>
        private static void AssertJsonResponseEquals(string expectedMessage, string jsonContent, string assertMessage, params object[] assertMessageParams)
        {
            ThrowIf.ArgumentNull(assertMessage, nameof(assertMessage));

            JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
            {
                // This will alert us if new properties are added to the return JSON format.
                MissingMemberHandling = MissingMemberHandling.Error
            };

            MessageResult messageResult = JsonConvert.DeserializeObject<MessageResult>(jsonContent, jsonSettings);

            Assert.AreEqual(expectedMessage, messageResult.Message, assertMessage, assertMessageParams);
        }

        /// <summary>
        /// Verify that all other top level artifacts exist in the list of Nova artifacts with the same properties.
        /// </summary>
        /// <param name="otherTopLevelArtifacts">The list of other top level artifacts that aren't part of the artifact chain.</param>
        /// <param name="novaArtifacts">The list of NovaArtifacts returned by the GetExpandedArtifactTree call.</param>
        private static void VerifyOtherTopLevelArtifactsExist(List<IArtifact> otherTopLevelArtifacts, List<INovaArtifact> novaArtifacts)
        {
            foreach (IArtifact artifact in otherTopLevelArtifacts)
            {
                INovaArtifact novaArtifact = novaArtifacts.Find(a => a.Id == artifact.Id);

                Assert.NotNull(novaArtifact, "Couldn't find Artifact ID {0} in the list of Nova Artifacts!", artifact.Id);
                novaArtifact.AssertEquals(artifact, shouldCompareVersions: false);
            }
        }

        /// <summary>
        /// Verifies that all top level Nova artifacts have the correct values for Children & ParentId, and verifies the children of the one
        /// artifact chain that should exist.
        /// </summary>
        /// <param name="artifactChain">The list of parent/child artifacts we created.</param>
        /// <param name="novaArtifacts">The list of NovaArtifacts returned by the GetExpandedArtifactTree call.</param>
        /// <param name="artifactIndex">(optional) The index of the artifact chain containing the artifact whose tree we requested in the test.
        /// By default the entire chain is assumed to be returned from GetExpandedArtifactTree.</param>
        private void VerifyArtifactTree(List<IArtifact> artifactChain, List<INovaArtifact> novaArtifacts, int artifactIndex = int.MaxValue)
        {
            INovaArtifact topArtifact = VerifyAllTopLevelArtifacts(artifactChain, novaArtifacts);
            VerifyChildArtifactsInChain(artifactChain, topArtifact, artifactIndex);
        }

        /// <summary>
        /// Verifies that the ParentId of all top level Nova artifacts is the ProjectId, and that none of the Nova artifacts has Children except
        /// the top level artifact in the chain we created.
        /// </summary>
        /// <param name="artifactChain">The list of parent/child artifacts we created.</param>
        /// <param name="novaArtifacts">The list of NovaArtifacts returned by the GetExpandedArtifactTree call.</param>
        /// <returns>The NovaArtifact corresponding to the top level artifact in the chain we created.</returns>
        private INovaArtifact VerifyAllTopLevelArtifacts(List<IArtifact> artifactChain, List<INovaArtifact> novaArtifacts)
        {
            IArtifact topArtifact = artifactChain[0];
            INovaArtifact topNovaArtifact = null;

            // Make sure only the top artifact has children and that all top-level artifacts have parentId == projectId.
            foreach (INovaArtifact artifact in novaArtifacts)
            {
                if (artifact.Id == topArtifact.Id)
                {
                    topNovaArtifact = artifact;
                    Assert.NotNull(artifact.Children,
                        "The Children property of artifact '{0}' should not be null because it's the top level in the artifact chain we created!",
                        artifact.Id);
                }
                else
                {
                    Assert.IsNull(artifact.Children,
                        "The Children property of artifact '{0}' should be null because it's not in the artifact chain we created!",
                        artifact.Id);
                }

                Assert.AreEqual(_project.Id, artifact.ParentId, "The parent of all top level artifacts should be the project ID!");
            }

            return topNovaArtifact;
        }

        /// <summary>
        /// Verifies that children of the Nova artifact equal it's corresponding artifact in the chain we created and that the bottom Nova artifact
        /// doesn't have any children in the Children property.
        /// </summary>
        /// <param name="artifactChain">The list of parent/child artifacts we created.</param>
        /// <param name="topNovaArtifact">The top level Nova artifact returned by the GetExpandedArtifactTree call that matches the top level artifact we created.</param>
        /// <param name="artifactIndex">(optional) The index of the artifact chain containing the artifact whose tree we requested in the test.
        /// By default the entire chain is assumed to be returned from GetExpandedArtifactTree.</param>
        private static void VerifyChildArtifactsInChain(List<IArtifact> artifactChain, INovaArtifact topNovaArtifact, int artifactIndex = int.MaxValue)
        {
            // Verify all the children in the chain.
            INovaArtifact currentChild = topNovaArtifact;

            foreach (IArtifact artifact in artifactChain)
            {
                Assert.NotNull(currentChild, "No NovaArtifact was returned that matches with artifact '{0}' that we created!", artifact.Id);

                currentChild.AssertEquals(artifact, shouldCompareVersions: false);
                currentChild = currentChild.Children?[0];

                if (--artifactIndex < 0)
                {
                    break;
                }
            }

            Assert.IsNull(currentChild, "The Children property of the last artifactin the chain should be null!");
        }

        #endregion Private functions
    }
}
