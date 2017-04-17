﻿using System.Collections.Generic;
using System.Linq;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.ModelHelpers;
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

        #region Setup and Teardown

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

        #endregion Setup and Teardown

        [TestCase(ItemTypePredefined.UseCase, ItemTypePredefined.UseCase, ItemTypePredefined.UseCase)]
        [TestCase(ItemTypePredefined.PrimitiveFolder, // PrimitiveFolders can only have projects as parents.
            ItemTypePredefined.Actor,
            ItemTypePredefined.BusinessProcess,
            ItemTypePredefined.Document,
            ItemTypePredefined.DomainDiagram,
            ItemTypePredefined.GenericDiagram,
            ItemTypePredefined.Glossary,
            ItemTypePredefined.Process,
            ItemTypePredefined.Storyboard,
            ItemTypePredefined.TextualRequirement,
            ItemTypePredefined.UIMockup,
            ItemTypePredefined.UseCase,
            ItemTypePredefined.UseCaseDiagram)]
        [TestRail(164525)]
        [Description("Create a chain of published parent/child artifacts and other top level artifacts.  GetExpandedArtifactTree with the ID of the artifact at the bottom of the chain." +
                     "Verify a list of top level artifacts is returned and only one has children.")]
        public void GetExpandedArtifactTree_LastPublishedArtifactInChain_ReturnsExpectedArtifactHierarchy(params ItemTypePredefined[] artifactTypeChain)
        {
            ThrowIf.ArgumentNull(artifactTypeChain, nameof(artifactTypeChain));

            // Setup:
            var artifactChain = Helper.CreatePublishedArtifactChain(_project, _user, artifactTypeChain);

            // Create some other top-level artifacts not part of the chain.
            var otherTopLevelArtifacts = new List<ArtifactWrapper>();
            otherTopLevelArtifacts.Add(Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Actor));
            otherTopLevelArtifacts.Add(Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Process));

            var viewerUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            // Execute:
            List<INovaArtifact> artifacts = null;

            Assert.DoesNotThrow(() => artifacts = Helper.ArtifactStore.GetExpandedArtifactTree(viewerUser, _project, artifactChain.Last().Id),
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
            var artifactChain = Helper.CreateSavedArtifactChain(_project, _user, artifactTypeChain);

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
            var artifactChain = Helper.CreateSavedArtifactChain(_project, _user, artifactTypeChain);

            // Create some other top-level artifacts not part of the chain.
            var otherTopLevelArtifacts = new List<IArtifact>();
            otherTopLevelArtifacts.Add(Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor));
            otherTopLevelArtifacts.Add(Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process));

            // Execute:
            List<INovaArtifact> artifacts = null;

            Assert.DoesNotThrow(() => artifacts = Helper.ArtifactStore.GetExpandedArtifactTree(_user, _project, artifactChain[artifactIndex].Id),
                "'GET {0}' should return 200 OK when passed valid parameters!", REST_PATH);

            // Verify:
            VerifyArtifactTree(artifactChain, artifacts, artifactIndex);
            VerifyOtherTopLevelArtifactsExist(otherTopLevelArtifacts, artifacts);
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
            var artifactChain = Helper.CreateSavedArtifactChain(_project, _user, artifactTypeChain);

            // Create some other top-level artifacts not part of the chain.
            var otherTopLevelArtifacts = new List<IArtifact>();
            otherTopLevelArtifacts.Add(Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor));
            otherTopLevelArtifacts.Add(Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process));

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
            VerifyOtherTopLevelArtifactsExist(otherTopLevelArtifacts, artifacts);
        }

        [TestCase(BaseArtifactType.Actor, true)]
        [TestCase(BaseArtifactType.Actor, false)]
        [TestRail(182430)]
        [Description("Create & publish some artifacts.  GetExpandedArtifactTree with the ID of the project instead of an artifact.  " +
            "Verify all the artifacts you created were returned.")]
        public void GetExpandedArtifactTree_ArtifactIdIsAProjectId_ReturnsProjectTree(BaseArtifactType artifactType, bool includeChildren)
        {
            // Setup:
            var publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts: 3);

            List<INovaArtifact> returnedArtifacts = null;

            // Execute:
            Assert.DoesNotThrow(() => returnedArtifacts = Helper.ArtifactStore.GetExpandedArtifactTree(_user, _project, _project.Id, includeChildren),
                "'GET {0}' should return 200 OK when a Project ID is passed in place of an Artifact ID!", REST_PATH);

            // Verify:
            // Make sure we can find all the artifacts we created in the list returned by the GetExpandedArtifactTree call.
            foreach (var artifact in publishedArtifacts)
            {
                var matchingNovaArtifact = returnedArtifacts.Find(a => a.Id == artifact.Id);
                Assert.NotNull(matchingNovaArtifact, "Couldn't find artifact ID {0} in the list of returned artifacts!", artifact.Id);

                matchingNovaArtifact.AssertEquals(artifact, shouldCompareVersions: false);
            }
        }

        [TestCase("")]
        [TestCase(CommonConstants.InvalidToken)]
        [TestRail(164532)]
        [Description("Create an artifact.  GetExpandedArtifactTree with the ID of the artifact but pass an invalid token.  Verify 401 Unauthorized is returned with the correct error message.")]
        public void GetExpandedArtifactTree_InvalidToken_401Unauthorized(string token)
        {
            // Setup:
            var artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Actor);

            var userWithBadOrMissingToken = UserFactory.CreateUserAndAddToDatabase();
            userWithBadOrMissingToken.Token.SetToken(_user.Token.AccessControlToken);
            userWithBadOrMissingToken.Token.AccessControlToken = token;

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.ArtifactStore.GetExpandedArtifactTree(userWithBadOrMissingToken, _project, artifact.Id),
                "'GET {0}' should return 401 Unauthorized when a bad or empty Session-Token is passed!", REST_PATH);

            // Verify:
            var messageResult = JsonConvert.DeserializeObject<MessageResult>(ex.RestResponse.Content);
            const string expectedMessage = "Token is invalid.";

            Assert.AreEqual(expectedMessage, messageResult.Message,
                "If a bad or empty token is passed, we should get an error message of '{0}'!", expectedMessage);
        }

        [TestCase]
        [TestRail(164533)]
        [Description("Create an artifact.  GetExpandedArtifactTree with the ID of the artifact without a Session-Token header.  Verify 401 Unauthorized is returned with the correct error message.")]
        public void GetExpandedArtifactTree_MissingTokenHeader_401Unauthorized()
        {
            // Setup:
            var artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Actor);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.ArtifactStore.GetExpandedArtifactTree(user: null, project: _project, artifactId: artifact.Id),
                "'GET {0}' should return 401 Unauthorized when no Session-Token header is passed!", REST_PATH);

            // Verify:
            var messageResult = JsonConvert.DeserializeObject<MessageResult>(ex.RestResponse.Content);
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
            var artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Actor);
            var nonExistingProject = ProjectFactory.CreateProject();
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
        [TestRail(164538)]
        [Description("GetExpandedArtifactTree with the ID of the artifact instead of a project.  Verify 404 Not Found is returned with the correct error message.")]
        public void GetExpandedArtifactTree_ProjectIdIsAnArtifactId_404NotFound()
        {
            // Setup:
            var artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Actor);
            var notAProject = ProjectFactory.CreateProject();
            notAProject.Id = artifact.Id;

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetExpandedArtifactTree(_user, notAProject, artifact.Id),
                "'GET {0}' should return 404 Not Found when an Artifact ID is passed in place of a Project ID!", REST_PATH);

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, ErrorCodes.ResourceNotFound, I18NHelper.FormatInvariant(
                "The project (Id:{0}) can no longer be accessed. It may have been deleted, or is no longer accessible by you.", notAProject.Id));
        }

        [TestCase]
        [TestRail(164557)]
        [Description("GetExpandedArtifactTree with the ID of the sub-artifact instead of an artifact.  Verify 404 Not Found is returned with the correct error message.")]
        public void GetExpandedArtifactTree_ArtifactIdIsASubArtifact_404NotFound()
        {
            // Setup:
            var artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process);
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
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
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
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
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
            var otherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);
            var artifactTypeChain = new ItemTypePredefined[] { ItemTypePredefined.Actor, ItemTypePredefined.Glossary, ItemTypePredefined.Process };
            var artifactChain = Helper.CreatePublishedArtifactChain(_project, _user, artifactTypeChain);

            var artifact = artifactChain.Last();
            artifact.Lock(_user);
            artifact.Delete(_user);

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
            var otherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);
            var artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Actor);

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
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);
            var userWithoutPermission = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.None, _project);

            // Execute:
            var ex = Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.GetExpandedArtifactTree(userWithoutPermission, _project, artifact.Id),
                "'GET {0}' should return 403 Forbidden when called by a user without permission to the project!", REST_PATH);

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("User does not have permissions for Project (Id:{0}).", _project.Id);
            AssertJsonResponseEquals(expectedMessage, ex.RestResponse.Content,
                "If called by a user without permission to the project, we should get an error message of '{0}'!", expectedMessage);
        }

        [TestCase(0, ItemTypePredefined.Actor, ItemTypePredefined.BusinessProcess, ItemTypePredefined.Document, ItemTypePredefined.DomainDiagram, ItemTypePredefined.GenericDiagram)]
        [TestCase(2, ItemTypePredefined.Actor, ItemTypePredefined.BusinessProcess, ItemTypePredefined.Document, ItemTypePredefined.DomainDiagram, ItemTypePredefined.GenericDiagram)]
        [TestCase(4, ItemTypePredefined.Actor, ItemTypePredefined.BusinessProcess, ItemTypePredefined.Document, ItemTypePredefined.DomainDiagram, ItemTypePredefined.GenericDiagram)]
        [TestRail(164559)]
        [Description("GetExpandedArtifactTree with a user that doesn't have access to the artifact.  Verify 403 Forbidden is returned with the correct error message.")]
        public void GetExpandedArtifactTree_UserWithoutPermissionToPublishedArtifact_403Forbidden(int artifactIndex, params ItemTypePredefined[] artifactTypeChain)
        {
            ThrowIf.ArgumentNull(artifactTypeChain, nameof(artifactTypeChain));

            // Setup:
            var artifactChain = Helper.CreatePublishedArtifactChain(_project, _user, artifactTypeChain);

            // Create some other top-level artifacts not part of the chain.
            var otherTopLevelArtifacts = new List<ArtifactWrapper>();
            otherTopLevelArtifacts.Add(Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Actor));
            otherTopLevelArtifacts.Add(Helper.CreateNovaArtifact(_user, _project, ItemTypePredefined.Process));

            // Create a user without permission to the artifact.
            var userWithoutPermission = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);
            Helper.AssignProjectRolePermissionsToUser(userWithoutPermission, TestHelper.ProjectRole.None, _project, artifactChain[artifactIndex]);

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

            var jsonSettings = new JsonSerializerSettings()
            {
                // This will alert us if new properties are added to the return JSON format.
                MissingMemberHandling = MissingMemberHandling.Error
            };

            var messageResult = JsonConvert.DeserializeObject<MessageResult>(jsonContent, jsonSettings);

            Assert.AreEqual(expectedMessage, messageResult.Message, assertMessage, assertMessageParams);
        }

        /// <summary>
        /// Verify that all other top level artifacts exist in the list of Nova artifacts with the same properties.
        /// </summary>
        /// <param name="otherTopLevelArtifacts">The list of other top level artifacts that aren't part of the artifact chain.</param>
        /// <param name="novaArtifacts">The list of NovaArtifacts returned by the GetExpandedArtifactTree call.</param>
        private static void VerifyOtherTopLevelArtifactsExist(List<IArtifact> otherTopLevelArtifacts, List<INovaArtifact> novaArtifacts)
        {
            foreach (var artifact in otherTopLevelArtifacts)
            {
                var novaArtifact = novaArtifacts.Find(a => a.Id == artifact.Id);

                Assert.NotNull(novaArtifact, "Couldn't find Artifact ID {0} in the list of Nova Artifacts!", artifact.Id);
                novaArtifact.AssertEquals(artifact, shouldCompareVersions: false);
            }
        }

        /// <summary>
        /// Verify that all other top level artifacts exist in the list of Nova artifacts with the same properties.
        /// </summary>
        /// <param name="otherTopLevelArtifacts">The list of other top level artifacts that aren't part of the artifact chain.</param>
        /// <param name="novaArtifacts">The list of NovaArtifacts returned by the GetExpandedArtifactTree call.</param>
        private static void VerifyOtherTopLevelArtifactsExist(List<ArtifactWrapper> otherTopLevelArtifacts, List<INovaArtifact> novaArtifacts)
        {
            foreach (var artifact in otherTopLevelArtifacts)
            {
                var novaArtifact = novaArtifacts.Find(a => a.Id == artifact.Id);

                Assert.NotNull(novaArtifact, "Couldn't find Artifact ID {0} in the list of Nova Artifacts!", artifact.Id);
                NovaArtifactBase.AssertAreEqual(artifact, novaArtifact, shouldCompareVersions: false);
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
            var topArtifact = VerifyAllTopLevelArtifacts(artifactChain.Select(a => a.Id), novaArtifacts);
            VerifyChildArtifactsInChain(artifactChain, topArtifact, artifactIndex);
        }

        /// <summary>
        /// Verifies that all top level Nova artifacts have the correct values for Children & ParentId, and verifies the children of the one
        /// artifact chain that should exist.
        /// </summary>
        /// <param name="artifactChain">The list of parent/child artifacts we created.</param>
        /// <param name="novaArtifacts">The list of NovaArtifacts returned by the GetExpandedArtifactTree call.</param>
        /// <param name="artifactIndex">(optional) The index of the artifact chain containing the artifact whose tree we requested in the test.
        /// By default the entire chain is assumed to be returned from GetExpandedArtifactTree.</param>
        private void VerifyArtifactTree(List<ArtifactWrapper> artifactChain, List<INovaArtifact> novaArtifacts, int artifactIndex = int.MaxValue)
        {
            var topArtifact = VerifyAllTopLevelArtifacts(artifactChain.Select(a => a.Id), novaArtifacts);
            VerifyChildArtifactsInChain(artifactChain, topArtifact, artifactIndex);
        }

        /// <summary>
        /// Verifies that the ParentId of all top level Nova artifacts is the ProjectId, and that none of the Nova artifacts has Children except
        /// the top level artifact in the chain we created.
        /// </summary>
        /// <param name="artifactChain">The list of parent/child artifact IDs we created.</param>
        /// <param name="novaArtifacts">The list of NovaArtifacts returned by the GetExpandedArtifactTree call.</param>
        /// <returns>The NovaArtifact corresponding to the top level artifact in the chain we created.</returns>
        private INovaArtifact VerifyAllTopLevelArtifacts(IEnumerable<int> artifactChain, List<INovaArtifact> novaArtifacts)
        {
            int topArtifactId = artifactChain.First();
            INovaArtifact topNovaArtifact = null;

            // Make sure only the top artifact has children and that all top-level artifacts have parentId == projectId.
            foreach (var artifact in novaArtifacts)
            {
                if (artifact.Id == topArtifactId)
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
            var currentChild = topNovaArtifact;

            foreach (var artifact in artifactChain)
            {
                Assert.NotNull(currentChild, "No NovaArtifact was returned that matches with artifact '{0}' that we created!", artifact.Id);

                currentChild.AssertEquals(artifact, shouldCompareVersions: false);
                currentChild = currentChild.Children?[0];

                if (--artifactIndex < 0)
                {
                    break;
                }
            }
            Assert.IsNull(currentChild, "The Children property of the last artifact in the chain should be null!");
        }

        /// <summary>
        /// Verifies that children of the Nova artifact equal it's corresponding artifact in the chain we created and that the bottom Nova artifact
        /// doesn't have any children in the Children property.
        /// </summary>
        /// <param name="artifactChain">The list of parent/child artifacts we created.</param>
        /// <param name="topNovaArtifact">The top level Nova artifact returned by the GetExpandedArtifactTree call that matches the top level artifact we created.</param>
        /// <param name="artifactIndex">(optional) The index of the artifact chain containing the artifact whose tree we requested in the test.
        /// By default the entire chain is assumed to be returned from GetExpandedArtifactTree.</param>
        private static void VerifyChildArtifactsInChain(List<ArtifactWrapper> artifactChain, INovaArtifact topNovaArtifact, int artifactIndex = int.MaxValue)
        {
            // Verify all the children in the chain.
            var currentChild = topNovaArtifact;

            foreach (var artifact in artifactChain)
            {
                Assert.NotNull(currentChild, "No NovaArtifact was returned that matches with artifact '{0}' that we created!", artifact.Id);

                NovaArtifactBase.AssertAreEqual(artifact, currentChild, shouldCompareVersions: false);
                currentChild = currentChild.Children?[0];

                if (--artifactIndex < 0)
                {
                    break;
                }
            }
            Assert.IsNull(currentChild, "The Children property of the last artifact in the chain should be null!");
        }

        #endregion Private functions
    }
}
