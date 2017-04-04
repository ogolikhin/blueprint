using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommon;
using Utilities;
using Utilities.Facades;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ArtifactDetailsTests : TestBase
    {
        /// <summary>
        /// This is the structure returned by the REST call to display error messages.
        /// </summary>
        public class MessageResult
        {
            public int ErrorCode { get; set; }
            public string Message { get; set; }
        }

        private const string GET_ARTIFACT_ID_PATH = RestPaths.Svc.ArtifactStore.ARTIFACTS_id_;
        private const int ALL_PERMISSIONS = 8159;

        private IUser _user = null;
        private List<IProject> _projects = null;
        private IProject _project = null;

        #region Setup and Cleanup

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _projects = ProjectFactory.GetAllProjects(_user, shouldRetrievePropertyTypes: true);
            _project = _projects[0];
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region 200 OK Tests

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForNovaRestMethods))]
        [TestRail(154601)]
        [Description("Create & publish an artifact, GetArtifactDetails.  Verify the artifact details are returned.")]
        public void GetArtifactDetails_PublishedArtifact_ReturnsArtifactDetails(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, artifactType);

            INovaArtifactDetails artifactDetails = null;

            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifactDetails = Helper.ArtifactStore.GetArtifactDetails(viewer, artifact.Id);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", GET_ARTIFACT_ID_PATH);

            // Verify:
            // Compare OpenAPI retrieved artifact with the Nova artifact to verify they both agree.
            var retrievedArtifact = Helper.OpenApi.GetArtifact(_project, artifact.Id, _user);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, retrievedArtifact);
            NovaArtifactDetails.AssertArtifactsEqual(artifact, artifactDetails);

            Assert.AreEqual(RolePermissions.Read, artifactDetails.Permissions, "Viewer should have read permissions!");
        }

        [TestCase(2)]
        [TestCase(11)]
        [TestRail(154706)]
        [Description("Create & publish an artifact, modify & publish it again, GetArtifactDetails.  Verify the artifact details for the latest version are returned.")]
        public void GetArtifactDetails_PublishedArtifactWithMultipleVersions_ReturnsArtifactDetailsForLatestVersion(int numberOfVersions)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifactWithMultipleVersions(_user, _project, ItemTypePredefined.Process, numberOfVersions);
            INovaArtifactDetails artifactDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", GET_ARTIFACT_ID_PATH);

            // Verify:
            // Compare OpenAPI retrieved artifact with the Nova artifact to verify they both agree.
            var retrievedArtifactVersion = Helper.OpenApi.GetArtifact(_project, artifact.Id, _user);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, retrievedArtifactVersion);

            // TODO: add check that Process has SpecificPropery - ClientType (?)

            Assert.NotNull(artifactDetails.Permissions, "Artifact Permissions shouldn't be null!");
            Assert.AreEqual(ALL_PERMISSIONS, (int)artifactDetails.Permissions.Value, "Instance Admin should have all permissions!");
        }

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(154700)]
        [Description("Create & publish an artifact, modify & publish it again, GetArtifactDetails with versionId=1.  Verify the artifact details " +
                     "for the first version are returned.")]
        public void GetArtifactDetailsWithVersionId1_PublishedArtifactWithMultipleVersions_ReturnsArtifactDetailsForFirstVersion(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, artifactType);

            artifact.Lock(_user);
            artifact.SaveWithNewDescription(_user);
            artifact.Publish(_user);

            INovaArtifactDetails artifactDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id, versionId: 1);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", GET_ARTIFACT_ID_PATH);

            // Verify:
            // Compare OpenAPI retrieved artifact with the Nova artifact to verify they both agree.
            var retrievedArtifactVersion1 = Helper.OpenApi.GetArtifact(_project, artifact.Id, _user);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, retrievedArtifactVersion1);

            // TODO: add check that Process has SpecificPropery - ClientType (?)

            Assert.NotNull(artifactDetails.Permissions, "Artifact Permissions shouldn't be null!");
            Assert.AreEqual(ALL_PERMISSIONS, (int)artifactDetails.Permissions.Value, "Instance Admin should have all permissions!");
        }

        [TestCase(ItemTypePredefined.Process)]
        [TestRail(166146)]
        [Description("Create & publish an artifact, modify & publish it again, then delete & publish it.  GetArtifactDetails with versionId=1.  " +
                     "Verify the artifact details for the first version are returned.")]
        public void GetArtifactDetailsWithVersionId1_DeletedArtifactWithMultipleVersions_ReturnsArtifactDetailsForFirstVersion(ItemTypePredefined artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, artifactType);

            artifact.Lock(_user);
            artifact.SaveWithNewDescription(_user);
            artifact.Publish(_user);

            artifact.Lock(_user);
            artifact.Delete(_user);
            artifact.Publish(_user);

            INovaArtifactDetails artifactDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id, versionId: 1);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", GET_ARTIFACT_ID_PATH);

            // Verify:
            // Compare OpenAPI retrieved artifact with the Nova artifact to verify they both agree.
            var retrievedArtifactVersion1 = Helper.OpenApi.GetArtifact(_project, artifact.Id, _user);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, retrievedArtifactVersion1);

            // TODO: add check that Process has SpecificPropery - ClientType (?)

            Assert.NotNull(artifactDetails.Permissions, "Artifact Permissions shouldn't be null!");
            Assert.AreEqual(ALL_PERMISSIONS, (int)artifactDetails.Permissions.Value, "Instance Admin should have all permissions!");
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForNovaRestMethods))]
        [TestRail(182509)]
        [Description("Create two artifacts: main artifact that has inline trace to inline trace artifact. Update the inline trace artifact information - Verify that " +
                     "GetArtifactDetails call returns updated inline trace information.")]
        public void GetArtifactDetails_UpdateInlineTraceArtifact_ReturnsUpdatedInlineTraceLink(ItemTypePredefined artifactType)
        {
            // Setup: Create to artifacts: main artifact and inline trace artifact with the same user on the same project
            var mainArtifact = Helper.CreateAndPublishNovaArtifact(_user, _project, artifactType);
            var inlineTraceArtifact = Helper.CreateAndPublishNovaArtifact(_user, _project, artifactType);

            // Update main artifact to have inline trace to inline trace artifact created
            var artifactDetailsToUpdateMainArtifact = ArtifactStoreHelper.CreateNovaArtifactDetailsForUpdate(mainArtifact);
            artifactDetailsToUpdateMainArtifact.Description = ArtifactStoreHelper.CreateArtifactInlineTraceValue(
                inlineTraceArtifact, Helper.BlueprintServer.Address);
            artifactDetailsToUpdateMainArtifact.Traces = CreateManualTrace(inlineTraceArtifact.Id);

            // Update and publish the main artifact with inline trace to target artifact
            mainArtifact.Lock(_user);
            mainArtifact.Update(_user, artifactDetailsToUpdateMainArtifact);
            mainArtifact.Publish(_user);

            // Update inline trace artifact information
            var artifactDetailsToUpdateInlineTraceArtifact = ArtifactStoreHelper.CreateNovaArtifactDetailsForUpdate(inlineTraceArtifact);
            artifactDetailsToUpdateInlineTraceArtifact.Name = inlineTraceArtifact.Name + "_NameUpdated";

            // Update and publish the inline trace artifact
            inlineTraceArtifact.Lock(_user);
            inlineTraceArtifact.Update(_user, artifactDetailsToUpdateInlineTraceArtifact);
            inlineTraceArtifact.Publish(_user);

            // Execute: Get ArtifactDetails for main artifact
            var mainArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, mainArtifact.Id);

            // Verify: Returned ArtifactDetails contains the updated information for InlineTrace.
            ArtifactStoreHelper.ValidateInlineTraceLinkFromArtifactDetails(mainArtifactDetails, inlineTraceArtifact, validInlineTraceLink: true);

            // If the artifact type is UIMockup, we also expect the HasUIMockup flag in addition to HasManualReuseOrOtherTraces.
            var indicatorFlags = (artifactType == ItemTypePredefined.UIMockup) ? ItemIndicatorFlags.HasManualReuseOrOtherTraces | ItemIndicatorFlags.HasUIMockup :
                    ItemIndicatorFlags.HasManualReuseOrOtherTraces;

            Assert.AreEqual(mainArtifactDetails.IndicatorFlags, indicatorFlags, "IndicatorFlags property should have HasManualReuseOrOtherTraces(4) value!");
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForNovaRestMethods))]
        [TestRail(182552)]
        [Description("Create two artifacts: main and inline trace in different project. Update the inline trace artifact information." +
                     "Verify that GetArtifactDetails call returns updated inline trace information.")]
        public void GetArtifactDetails_UpdateInlineTraceArtifactInDifferentProject_ReturnsUpdatedInlineTraceLink(
            ItemTypePredefined artifactType)
        {
            // Setup: Get projects available from testing environment
            var mainProject = _projects[0];
            var secondProject = _projects[1];

            // Create two artifacts: main artifact under main project and inline trace artifact under the second project
            var mainArtifact = Helper.CreateAndPublishNovaArtifact(_user, mainProject, artifactType);
            var inlineTraceArtifact = Helper.CreateAndPublishNovaArtifact(_user, secondProject, artifactType);

            // Update main artifact to have inline trace to inline trace artifact created
            var artifactDetailsToUpdateMainArtifact = ArtifactStoreHelper.CreateNovaArtifactDetailsForUpdate(mainArtifact);
            artifactDetailsToUpdateMainArtifact.Description = ArtifactStoreHelper.CreateArtifactInlineTraceValue(
                inlineTraceArtifact, Helper.BlueprintServer.Address);
            artifactDetailsToUpdateMainArtifact.Traces = CreateManualTrace(inlineTraceArtifact.Id);

            // Update and publish the main artifact with inline trace to target artifact
            mainArtifact.Lock(_user);
            mainArtifact.Update(_user, artifactDetailsToUpdateMainArtifact);
            mainArtifact.Publish(_user);

            // Update inline trace artifact information
            var artifactDetailsToUpdateInlineTraceArtifact = ArtifactStoreHelper.CreateNovaArtifactDetailsForUpdate(inlineTraceArtifact);
            artifactDetailsToUpdateInlineTraceArtifact.Name = inlineTraceArtifact.Name + "_NameUpdated";

            // Update and publish the inline trace artifact
            inlineTraceArtifact.Lock(_user);
            inlineTraceArtifact.Update(_user, artifactDetailsToUpdateInlineTraceArtifact);
            inlineTraceArtifact.Publish(_user);

            // Execute: Get ArtifactDetails for main artifact
            var mainArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, mainArtifact.Id);

            // Verify: Returned ArtifactDetails contains valid inline traceLink to the inlineTraceArtifact.
            ArtifactStoreHelper.ValidateInlineTraceLinkFromArtifactDetails(mainArtifactDetails, inlineTraceArtifact, validInlineTraceLink: true);

            // If the artifact type is UIMockup, we also expect the HasUIMockup flag in addition to HasManualReuseOrOtherTraces.
            var indicatorFlags = (artifactType == ItemTypePredefined.UIMockup) ? ItemIndicatorFlags.HasManualReuseOrOtherTraces | ItemIndicatorFlags.HasUIMockup :
                ItemIndicatorFlags.HasManualReuseOrOtherTraces;

            Assert.AreEqual(mainArtifactDetails.IndicatorFlags, indicatorFlags, "IndicatorFlags property should have HasManualReuseOrOtherTraces(4) value!");
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForNovaRestMethods))]
        [TestRail(182549)]
        [Description("Create two artifacts: main and inline trace.  Delete the inline trace artifact.  Verify that GetArtifactDetails " +
                     "call returns invalid inline trace link.")]
        public void GetArtifactDetails_DeletedInlinetraceArtifact_ReturnsInvalidInlineTraceLink(
            ItemTypePredefined artifactType)
        {
            // Setup: Create two artifacts: main artifact under main project and inline trace artifact under the second project
            var mainArtifact = Helper.CreateAndPublishNovaArtifact(_user, _project, artifactType);
            var inlineTraceArtifact = Helper.CreateAndPublishNovaArtifact(_user, _project, artifactType);

            // Update main artifact to have inline trace to inline trace artifact created
            var artifactDetailsToUpdateMainArtifact = ArtifactStoreHelper.CreateNovaArtifactDetailsForUpdate(mainArtifact);
            artifactDetailsToUpdateMainArtifact.Description = ArtifactStoreHelper.CreateArtifactInlineTraceValue(
                inlineTraceArtifact, Helper.BlueprintServer.Address);
            artifactDetailsToUpdateMainArtifact.Traces = CreateManualTrace(inlineTraceArtifact.Id);

            // Update and publish the main artifact with inline trace to target artifact
            mainArtifact.Lock(_user);
            mainArtifact.Update(_user, artifactDetailsToUpdateMainArtifact);
            mainArtifact.Publish(_user);

            // Delete and publish the inline trace artifact.
            inlineTraceArtifact.Lock(_user);
            inlineTraceArtifact.Delete(_user);
            inlineTraceArtifact.Publish(_user);

            // Execute: Get ArtifactDetails using userWithPermissionOnMainProject for the main artifact
            var mainArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, mainArtifact.Id);

            // Verify: Returned ArtifactDetails contains invalid inline traceLink to the inlineTraceArtifact.
            ArtifactStoreHelper.ValidateInlineTraceLinkFromArtifactDetails(mainArtifactDetails, inlineTraceArtifact, validInlineTraceLink: false);

            Assert.AreEqual(mainArtifactDetails.IndicatorFlags, null, "IndicatorFlags property should have null value!");
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForNovaRestMethods))]
        [TestRail(182550)]
        [Description("Create two artifacts: main and inline trace in different project. Verify that GetArtifactDetails call returns invalid inline trace link if " +
                     "the user doesn't have the access permission for the inline trace artifact.")]
        public void GetArtifactDetails_UserWithoutPermissionToInlineTraceArtifact_ReturnsInvalidInlineTraceLink(
            ItemTypePredefined artifactType)
        {
            // Setup: Get projects available from testing environment
            var mainProject = _projects[0];
            var secondProject = _projects[1];

            // Create two artifacts: main artifact under main project and inline trace artifact under the second project
            var mainArtifact = Helper.CreateAndPublishNovaArtifact(_user, mainProject, artifactType);
            var inlineTraceArtifact = Helper.CreateAndPublishNovaArtifact(_user, secondProject, artifactType);

            // Update main artifact to have inline trace to inline trace artifact created
            var artifactDetailsToUpdateMainArtifact = ArtifactStoreHelper.CreateNovaArtifactDetailsForUpdate(mainArtifact);
            artifactDetailsToUpdateMainArtifact.Description = ArtifactStoreHelper.CreateArtifactInlineTraceValue(
                inlineTraceArtifact, Helper.BlueprintServer.Address);
            artifactDetailsToUpdateMainArtifact.Traces = CreateManualTrace(inlineTraceArtifact.Id);

            // Update and publish the main artifact with inline trace to target artifact
            mainArtifact.Lock(_user);
            mainArtifact.Update(_user, artifactDetailsToUpdateMainArtifact);
            mainArtifact.Publish(_user);

            // Create user with a permission only on main project
            var userWithPermissionOnMainProjectOnly = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, new List<IProject> { mainProject });

            // Execute: Get ArtifactDetails for the main artifact using the user without permission to inline trace artifact
            var mainArtifactDetailsWithUserWithPermissionOnMainProject = Helper.ArtifactStore.GetArtifactDetails(userWithPermissionOnMainProjectOnly, mainArtifact.Id);

            // Verify: Returned ArtifactDeatils contains invalid inline traceLink to the inlineTraceArtifact
            ArtifactStoreHelper.ValidateInlineTraceLinkFromArtifactDetails(
                mainArtifactDetailsWithUserWithPermissionOnMainProject, inlineTraceArtifact, validInlineTraceLink: false);

            // If the artifact type is UIMockup, we also expect the HasUIMockup flag in addition to HasManualReuseOrOtherTraces.
            var indicatorFlags = (artifactType == ItemTypePredefined.UIMockup) ? ItemIndicatorFlags.HasManualReuseOrOtherTraces | ItemIndicatorFlags.HasUIMockup :
                ItemIndicatorFlags.HasManualReuseOrOtherTraces;

            Assert.AreEqual(mainArtifactDetailsWithUserWithPermissionOnMainProject.IndicatorFlags, indicatorFlags,
                "IndicatorFlags property should have HasManualReuseOrOtherTraces(4) value!");
        }

        #endregion 200 OK Tests

        #region 400 Bad Request

        [TestRail(246535)]
        [TestCase("9999999999", "The request is invalid.")]
        [TestCase("&amp;", "A potentially dangerous Request.Path value was detected from the client (&).")]
        [Description("Create a rest path that tries to get an artifact with an invalid artifact Id. " +
                     "Attempt to get the artifact. Verify that HTTP 400 Bad Request exception is thrown.")]
        public void GetArtifactDetails_InvalidArtifactId_400BadRequest(string artifactId, string expectedErrorMessage)
        {
            // Setup:
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.ARTIFACTS_id_, artifactId);

            var restApi = new RestApiFacade(Helper.ArtifactStore.Address, _user?.Token?.AccessControlToken);

            // Execute & Verify:
            var ex = Assert.Throws<Http400BadRequestException>(() => restApi.SendRequestAndDeserializeObject<NovaArtifactResponse, object>(
               path,
               RestRequestMethod.GET,
               jsonObject: null),
                "We should get a 400 Bad Request when the artifact Id is invalid!");

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, expectedErrorMessage);
        }

        [TestCase("*")]
        [TestCase("&")]
        [TestRail(246558)]
        [Description("GetArtifactDetails using the invalid URL containing a special character. Verify that 400 bad request is returned.")]
        public void GetArtifactDetails_SendInvalidUrl_400BadRequest(string invalidCharacter)
        {
            // Setup:
            int nonExistingArtifactId = int.MaxValue;
            string invalidPath = I18NHelper.FormatInvariant(GET_ARTIFACT_ID_PATH, invalidCharacter + nonExistingArtifactId);

            var restApi = new RestApiFacade(Helper.ArtifactStore.Address, _user?.Token?.AccessControlToken);

            // Execute & Verify:
            var ex = Assert.Throws<Http400BadRequestException>(() => restApi.SendRequestAndDeserializeObject<NovaArtifactResponse, object>(
                invalidPath,
                RestRequestMethod.GET,
                jsonObject: null,
                shouldControlJsonChanges: true
                ),
                "GET {0} call should return a 400 Bad Request exception when trying with invalid URL.", GET_ARTIFACT_ID_PATH);

            // Verify:
            string expectedMessage = I18NHelper.FormatInvariant("A potentially dangerous Request.Path value was detected from the client ({0}).", invalidCharacter);

            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, expectedMessage);
        }

        #endregion 400 Bad Request

        #region 401 Unauthorized Tests

        [TestCase]
        [TestRail(154701)]
        [Description("Create & publish an artifact, GetArtifactDetails but don't send any Session-Token header.  Verify it returns 401 Unauthorized.")]
        public void GetArtifactDetails_PublishedArtifactNoTokenHeader_401Unauthorized()
        {
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Process);

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(user: null, artifactId: artifact.Id);
            }, "'GET {0}' should return 401 Unauthorized when passed a valid artifact ID but no Session-Token in the header!",
                GET_ARTIFACT_ID_PATH);
        }

        [TestCase]
        [TestRail(154702)]
        [Description("Create & publish an artifact, GetArtifactDetails but use an unauthorized token.  Verify it returns 401 Unauthorized.")]
        public void GetArtifactDetails_PublishedArtifactUnauthorizedToken_401Unauthorized()
        {
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Process);
            var unauthorizedUser = Helper.CreateUserAndAddToDatabase();

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(unauthorizedUser, artifact.Id);
            }, "'GET {0}' should return 401 Unauthorized when passed a valid artifact ID but an unauthorized token!",
                GET_ARTIFACT_ID_PATH);
        }

        #endregion 401 Unauthorized Tests

        #region 403 Forbidden Tests

        [TestCase]
        [TestRail(154703)]
        [Description("Create & publish an artifact, GetArtifactDetails with a user that doesn't have access to the artifact.  Verify it returns 403 Forbidden.")]
        public void GetArtifactDetails_PublishedArtifactUserWithoutPermissions_403Forbidden()
        {
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Process);

            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);
            Helper.AssignProjectRolePermissionsToUser(viewer, TestHelper.ProjectRole.None, _project, artifact);

            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(viewer, artifact.Id);
            }, "'GET {0}' should return 403 Forbidden when passed a valid artifact ID but the user doesn't have permission to view the artifact!",
                GET_ARTIFACT_ID_PATH);

            string expectedMessage = I18NHelper.FormatInvariant("You do not have permission to access the artifact (ID: {0})", artifact.Id);
            AssertJsonResponseEquals(expectedMessage, ex.RestResponse.Content,
                "If called by a user without permission to the artifact, we should get an error message of '{0}'!", expectedMessage);
        }

        [TestCase]
        [TestRail(166147)]
        [Description("Create & publish an artifact, modify save & publish it again.  GetArtifactDetails with version=1 with a user that " +
                     "doesn't have access to the artifact.  Verify it returns 403 Forbidden.")]
        public void GetArtifactDetailsWithVersion1_PublishedArtifactWithMultipleVersions_UserWithoutPermissions_403Forbidden()
        {
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Process);

            var unauthorizedUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);
            Helper.AssignProjectRolePermissionsToUser(unauthorizedUser, TestHelper.ProjectRole.None, _project, artifact);

            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(unauthorizedUser, artifact.Id, versionId: 1);
            }, "'GET {0}' should return 403 Forbidden when passed a valid artifact ID but the user doesn't have permission to view the artifact!",
                GET_ARTIFACT_ID_PATH);

            string expectedMessage = I18NHelper.FormatInvariant("You do not have permission to access the artifact (ID: {0})", artifact.Id);

            AssertJsonResponseEquals(expectedMessage, ex.RestResponse.Content,
                "If called by a user without permission to the artifact, we should get an error message of '{0}'!",
                expectedMessage);
        }

        [TestCase]
        [TestRail(185236)]
        [Description("Create & publish parent & child artifacts.  Make sure viewer does not have access to parent.  Viewer request GetArtifactDetails " +
                     "from child artifact.  Verify it returns 403 Forbidden.")]
        public void GetArtifactDetails_PublishedArtifactWithAChild_UserWithoutPermissionsToParent_403Forbidden()
        {
            var parent = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Process);
            var child = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Process, parentId: parent.Id);

            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);
            Helper.AssignProjectRolePermissionsToUser(viewer, TestHelper.ProjectRole.None, _project, parent);

            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(viewer, child.Id, versionId: 1);
            }, "'GET {0}' should return 403 Forbidden when passed a valid child artifact ID but the user doesn't have permission to view parent artifact!",
                GET_ARTIFACT_ID_PATH);

            string expectedMessage = I18NHelper.FormatInvariant("You do not have permission to access the artifact (ID: {0})", child.Id);
            AssertJsonResponseEquals(expectedMessage, ex.RestResponse.Content,
                "If called by a user that does not have permissions to parent artifact, we should get an error message of '{0}'!", expectedMessage);
        }

        #endregion 403 Forbidden Tests

        #region 404 Not Found Tests

        [TestCase]
        [TestRail(154704)]
        [Description("Create & save (but don't publish) an artifact, GetArtifactDetails with a different user.  Verify it returns 404 Not Found.")]
        public void GetArtifactDetails_UnpublishedArtifactOtherUser_404NotFound()
        {
            var artifact = Helper.CreateNovaArtifact(_user, _project, ItemTypePredefined.Process);
            var user2 = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(user2, artifact.Id);
            }, "'GET {0}' should return 404 Not Found when passed an unpublished artifact ID with a different user!",
                GET_ARTIFACT_ID_PATH);
        }

        [TestCase(0)]
        [TestCase(int.MaxValue)]
        [TestRail(154705)]
        [Description("GetArtifactDetails and pass a non-existent Artifact ID (ex. 0 or MaxInt).  Verify it returns 404 Not Found.")]
        public void GetArtifactDetails_NonExistentArtifactId_404NotFound(int artifactId)
        {
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(_user, artifactId);
            }, "'GET {0}' should return 404 Not Found when passed an artifact ID that doesn't exist!",
                GET_ARTIFACT_ID_PATH);
        }

        [TestCase(0)]
        [TestCase(2)]
        [TestRail(166149)]
        [Description("Create & publish an artifact.  GetArtifactDetails and pass a non-existent Version ID (ex. 0 or 2).  Verify it returns 404 Not Found.")]
        public void GetArtifactDetailsWithVersion_NonExistentVersionId_404NotFound(int versionId)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Process);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id, versionId);
            }, "'GET {0}' should return 404 Not Found when passed an artifact ID that doesn't exist!",
                GET_ARTIFACT_ID_PATH);

            // Verify:
            const string expectedMessage = "You have attempted to access an item that does not exist or you do not have permission to view.";
            AssertJsonResponseEquals(expectedMessage, ex.RestResponse.Content,
                "If called by a user without permission to the artifact, we should get an error message of '{0}'!", expectedMessage);
        }

        #endregion 404 Not Found Tests

        #region Private Functions

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
        /// Creates manual trace
        /// </summary>
        /// <param name="artifactId">Artifact Id to which trace need to be created</param>
        /// <returns>List of traces from one trace</returns>
        private static List<NovaTrace> CreateManualTrace(int artifactId)
        {
            var trace = new NovaTrace
            {
                ArtifactId = artifactId,
                ChangeType = ChangeType.Create,
                ItemId = artifactId,
                Direction = TraceDirection.From,
                TraceType = TraceType.Manual
            };
            return new List<NovaTrace>() { trace };
        }

        #endregion Private Functions
    }
}