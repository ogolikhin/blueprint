using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.OpenApiModel.Services;
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

        private IUser _user = null;
        private List<IProject> _projects = null;

        #region Setup and Cleanup

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _projects = ProjectFactory.GetAllProjects(_user, shouldRetrievePropertyTypes: true);
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
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _projects[0], artifactType);
            var retrievedArtifact = OpenApi.GetArtifact(Helper.OpenApi.Address, _projects[0], artifact.Artifact.Id, _user);

            INovaArtifactDetails artifactDetails = null;

            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _projects[0]);

            Assert.DoesNotThrow(() =>
            {
                artifactDetails = Helper.ArtifactStore.GetArtifactDetails(viewer, artifact.Artifact.Id);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", GET_ARTIFACT_ID_PATH);

            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, retrievedArtifact);

            Assert.AreEqual(RolePermissions.Read, artifactDetails.Permissions, "Viewer should have read permissions (i.e. 1)!");
        }

        [TestCase(2)]
        [TestCase(11)]
        [TestRail(154706)]
        [Description("Create & publish an artifact, modify & publish it again, GetArtifactDetails.  Verify the artifact details for the latest version are returned.")]
        public void GetArtifactDetails_PublishedArtifactWithMultipleVersions_ReturnsArtifactDetailsForLatestVersion(int numberOfVersions)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_projects[0], _user, BaseArtifactType.Process, numberOfVersions: numberOfVersions);
            INovaArtifactDetails artifactDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", GET_ARTIFACT_ID_PATH);

            // Verify:
            var retrievedArtifactVersion = OpenApi.GetArtifact(artifact.Address, _projects[0], artifact.Id, _user);
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, retrievedArtifactVersion);

            // TODO: add check that Process has SpecificPropery - ClientType (?)

            Assert.NotNull(artifactDetails.Permissions, "Artifact Permissions shouldn't be null!");
            Assert.AreEqual(8159, (int)artifactDetails.Permissions.Value, "Instance Admin should have all permissions (i.e. 8159)!");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(154700)]
        [Description("Create & publish an artifact, modify & publish it again, GetArtifactDetails with versionId=1.  Verify the artifact details for the first version are returned.")]
        public void GetArtifactDetailsWithVersionId1_PublishedArtifactWithMultipleVersions_ReturnsArtifactDetailsForFirstVersion(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_projects[0], _user, artifactType);
            var retrievedArtifactVersion1 = OpenApi.GetArtifact(artifact.Address, _projects[0], artifact.Id, _user);

            artifact.Save();
            artifact.Publish();

            INovaArtifactDetails artifactDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id, versionId: 1);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", GET_ARTIFACT_ID_PATH);

            // Execute:
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, retrievedArtifactVersion1);

            // TODO: add check that Process has SpecificPropery - ClientType (?)

            Assert.NotNull(artifactDetails.Permissions, "Artifact Permissions shouldn't be null!");
            Assert.AreEqual(8159, (int)artifactDetails.Permissions.Value, "Instance Admin should have all permissions (i.e. 8159)!");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(166146)]
        [Description("Create & publish an artifact, modify & publish it again, then delete & publish it.  GetArtifactDetails with versionId=1.  " +
            "Verify the artifact details for the first version are returned.")]
        public void GetArtifactDetailsWithVersionId1_DeletedArtifactWithMultipleVersions_ReturnsArtifactDetailsForFirstVersion(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_projects[0], _user, artifactType);
            var retrievedArtifactVersion1 = OpenApi.GetArtifact(artifact.Address, _projects[0], artifact.Id, _user);

            artifact.Save();
            artifact.Publish();
            artifact.Delete();
            artifact.Publish();

            INovaArtifactDetails artifactDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id, versionId: 1);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", GET_ARTIFACT_ID_PATH);

            // Verify:
            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, retrievedArtifactVersion1);

            // TODO: add check that Process has SpecificPropery - ClientType (?)

            Assert.NotNull(artifactDetails.Permissions, "Artifact Permissions shouldn't be null!");
            Assert.AreEqual(8159, (int)artifactDetails.Permissions.Value, "Instance Admin should have all permissions (i.e. 8159)!");
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(182509)]
        [Description("Create two artifacts: main artifact that has inline trace to inline trace artifact. Update the inline trace artifact information - Verify that " +
            "GetArtifactDetails call returns updated inline trace information.")]
        public void GetArtifactDetails_UpdateInlineTraceArtifact_ReturnsUpdatedInlineTraceLink(BaseArtifactType baseArtifactType)
        {
            // Setup: Create to artifacts: main artifact and inline trace artifact with the same user on the same project
            var mainArtifact = Helper.CreateAndPublishArtifact(_projects[0], _user, baseArtifactType);
            var inlineTraceArtifact = Helper.CreateAndPublishArtifact(_projects[0], _user, baseArtifactType);
            NovaArtifactDetails inlineTraceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, inlineTraceArtifact.Id);

            // Update main artifact to have inline trace to inline trace artifact created
            var artifactDetailsToUpdateMainArtifact = ArtifactStoreHelper.CreateNovaArtifactDetailsWithArtifact(mainArtifact);
            artifactDetailsToUpdateMainArtifact.Description = ArtifactStoreHelper.CreateArtifactInlineTraceValue(inlineTraceArtifact, inlineTraceArtifactDetails);
            artifactDetailsToUpdateMainArtifact.Traces = CreateManualTrace(inlineTraceArtifact.Id);

            // Update and publish the main artifact with inline trace to target artifact
            mainArtifact.Lock();
            Assert.DoesNotThrow(() => inlineTraceArtifactDetails = Artifact.UpdateArtifact(mainArtifact, _user, artifactDetailsChanges: artifactDetailsToUpdateMainArtifact),
                "UpdateArtifact call failed when using the following artifact ID: {0}!", mainArtifact.Id);
            mainArtifact.Publish();

            // Update inline trace artifact information
            var artifactDetailsToUpdateInlineTraceArtifact = ArtifactStoreHelper.CreateNovaArtifactDetailsWithArtifact(inlineTraceArtifact);
            artifactDetailsToUpdateInlineTraceArtifact.Name = inlineTraceArtifact.Name + "_NameUpdated";

            // Update and publish the inline trace artifact
            inlineTraceArtifact.Lock();

            Assert.DoesNotThrow(() =>
            {
                Artifact.UpdateArtifact(inlineTraceArtifact, _user, artifactDetailsToUpdateInlineTraceArtifact);
            }, "UpdateArtifact call failed when using the following artifact ID: {0}!", inlineTraceArtifact.Id);

            inlineTraceArtifact.Publish();

            // Execute: Get ArtifactDetails for main artifact
            var mainArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, mainArtifact.Id);

            // Verify: Returned ArtifactDeatils contains the updated information for InlineTrace
            ArtifactStoreHelper.ValidateInlineTraceLinkFromArtifactDetails(mainArtifactDetails, inlineTraceArtifact, validInlineTraceLink: true);

            var indicatorFlags = baseArtifactType == BaseArtifactType.UIMockup ? ItemIndicatorFlags.HasManualReuseOrOtherTraces | ItemIndicatorFlags.HasUIMockup :
                    ItemIndicatorFlags.HasManualReuseOrOtherTraces;

            Assert.AreEqual(mainArtifactDetails.IndicatorFlags, indicatorFlags, "IndicatorFlags property should have HasManualReuseOrOtherTraces(4) value!");
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(182552)]
        [Description("Create two artifacts: main and inline trace in different project. Update the inline trace artifact information." +
            "Verify that GetArtifactDetails call returns updated inline trace information.")]
        public void GetArtifactDetails_UpdateInlineTraceArtifactInDifferentProject_ReturnsUpdatedInlineTraceLink(
            BaseArtifactType baseArtifactType)
        {
            // Setup: Get projects available from testing environment
            var mainProject = _projects[0];
            var secondProject = _projects[1];

            // Create two artifacts: main artifact under main project and inline trace artifact under the second project
            var mainArtifact = Helper.CreateAndPublishArtifact(mainProject, _user, baseArtifactType);
            var inlineTraceArtifact = Helper.CreateAndPublishArtifact(secondProject, _user, baseArtifactType);
            var inlineTraceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, inlineTraceArtifact.Id);

            // Update main artifact to have inline trace to inline trace artifact created
            var artifactDetailsToUpdateMainArtifact = ArtifactStoreHelper.CreateNovaArtifactDetailsWithArtifact(mainArtifact);
            artifactDetailsToUpdateMainArtifact.Description = ArtifactStoreHelper.CreateArtifactInlineTraceValue(inlineTraceArtifact, inlineTraceArtifactDetails);
            artifactDetailsToUpdateMainArtifact.Traces = CreateManualTrace(inlineTraceArtifact.Id);

            // Update and publish the main artifact with inline trace to target artifact
            mainArtifact.Lock();

            Assert.DoesNotThrow(() =>
            {
                inlineTraceArtifactDetails = Artifact.UpdateArtifact(mainArtifact, _user,
                    artifactDetailsChanges: artifactDetailsToUpdateMainArtifact);
            }, "UpdateArtifact call failed when using the following artifact ID: {0}!", mainArtifact.Id);

            mainArtifact.Publish();

            // Update inline trace artifact information
            var artifactDetailsToUpdateInlineTraceArtifact = ArtifactStoreHelper.CreateNovaArtifactDetailsWithArtifact(inlineTraceArtifact);
            artifactDetailsToUpdateInlineTraceArtifact.Name = inlineTraceArtifact.Name + "_NameUpdated";

            // Update and publish the inline trace artifact
            inlineTraceArtifact.Lock();

            Assert.DoesNotThrow(() =>
            {
                inlineTraceArtifactDetails = Artifact.UpdateArtifact(inlineTraceArtifact, _user,
                    artifactDetailsChanges: artifactDetailsToUpdateInlineTraceArtifact);
            }, "UpdateArtifact call failed when using the following artifact ID: {0}!", inlineTraceArtifact.Id);

            inlineTraceArtifact.Publish();

            // Execute: Get ArtifactDetails for main artifact
            var mainArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, mainArtifact.Id);

            //Verify: Returned ArtifactDeatils contains valid inline traceLink to the inlineTraceArtifact
            ArtifactStoreHelper.ValidateInlineTraceLinkFromArtifactDetails(mainArtifactDetails, inlineTraceArtifact, validInlineTraceLink: true);

            var indicatorFlags = baseArtifactType == BaseArtifactType.UIMockup ? ItemIndicatorFlags.HasManualReuseOrOtherTraces | ItemIndicatorFlags.HasUIMockup :
                ItemIndicatorFlags.HasManualReuseOrOtherTraces;

            Assert.AreEqual(mainArtifactDetails.IndicatorFlags, indicatorFlags, "IndicatorFlags property should have HasManualReuseOrOtherTraces(4) value!");
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(182549)]
        [Description("Create two artifacts: main and inline trace. Delete the inline trace artifact. - Verify that GetArtifactDetails call returns invalid inline trace link")]
        public void GetArtifactDetails_DeletedInlinetraceArtifact_ReturnsInvalidInlineTraceLink(
            BaseArtifactType baseArtifactType)
        {
            // Setup: Create two artifacts: main artifact under main project and inline trace artifact under the second project
            var mainArtifact = Helper.CreateAndPublishArtifact(_projects[0], _user, baseArtifactType);
            var inlineTraceArtifact = Helper.CreateAndPublishArtifact(_projects[0], _user, baseArtifactType);
            var inlineTraceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, inlineTraceArtifact.Id);

            // Update main artifact to have inline trace to inline trace artifact created
            var artifactDetailsToUpdateMainArtifact = ArtifactStoreHelper.CreateNovaArtifactDetailsWithArtifact(mainArtifact);
            artifactDetailsToUpdateMainArtifact.Description = ArtifactStoreHelper.CreateArtifactInlineTraceValue(inlineTraceArtifact, inlineTraceArtifactDetails);
            artifactDetailsToUpdateMainArtifact.Traces = CreateManualTrace(inlineTraceArtifact.Id);

            // Update and publish the main artifact with inline trace to target artifact
            mainArtifact.Lock();
            Assert.DoesNotThrow(() => inlineTraceArtifactDetails = Artifact.UpdateArtifact(mainArtifact, _user, artifactDetailsChanges: artifactDetailsToUpdateMainArtifact),
                "UpdateArtifact call failed when using the following artifact ID: {0}!", mainArtifact.Id);
            mainArtifact.Publish();

            // Delete and publish the inline trace artifact
            inlineTraceArtifact.Delete();
            inlineTraceArtifact.Publish();

            // Execute: Get ArtifactDetails using userWithPermissionOnMainProject for the main artifact
            var mainArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, mainArtifact.Id);

            // Verify: Returned ArtifactDeatils contains invalid inline traceLink to the inlineTraceArtifact
            ArtifactStoreHelper.ValidateInlineTraceLinkFromArtifactDetails(mainArtifactDetails, inlineTraceArtifact, validInlineTraceLink: false);

            Assert.AreEqual(mainArtifactDetails.IndicatorFlags, null, "IndicatorFlags property should have null value!");
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(182550)]
        [Description("Create two artifacts: main and inline trace in different project. Verify that GetArtifactDetails call returns invalid inline trace link if " +
            "the user doesn't have the access permission for the inline trace artifact")]
        public void GetArtifactDetails_UserWithoutPermissionToInlineTraceArtifact_ReturnsInvalidInlineTraceLink(
            BaseArtifactType baseArtifactType)
        {
            // Setup: Get projects available from testing environment
            var mainProject = _projects[0];
            var secondProject = _projects[1];

            // Create two artifacts: main artifact under main project and inline trace artifact under the second project
            var mainArtifact = Helper.CreateAndPublishArtifact(mainProject, _user, baseArtifactType);
            var inlineTraceArtifact = Helper.CreateAndPublishArtifact(secondProject, _user, baseArtifactType);
            var inlineTraceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, inlineTraceArtifact.Id);

            // Update main artifact to have inline trace to inline trace artifact created
            var artifactDetailsToUpdateMainArtifact = ArtifactStoreHelper.CreateNovaArtifactDetailsWithArtifact(mainArtifact);
            artifactDetailsToUpdateMainArtifact.Description = ArtifactStoreHelper.CreateArtifactInlineTraceValue(inlineTraceArtifact, inlineTraceArtifactDetails);
            artifactDetailsToUpdateMainArtifact.Traces = CreateManualTrace(inlineTraceArtifact.Id);

            // Update and publish the main artifact with inline trace to target artifact
            mainArtifact.Lock();
            Assert.DoesNotThrow(() => inlineTraceArtifactDetails = Artifact.UpdateArtifact(mainArtifact, _user, artifactDetailsChanges: artifactDetailsToUpdateMainArtifact),
                "UpdateArtifact call failed when using the following artifact ID: {0}!", mainArtifact.Id);
            mainArtifact.Publish();

            // Create user with a permission only on main project
            var userWithPermissionOnMainProjectOnly = Helper.CreateUserWithProjectRolePermissions(role: TestHelper.ProjectRole.Author, projects: new List<IProject> { mainProject });

            // Execute: Get ArtifactDetails for the main artifact using the user without permission to inline trace artifact
            var mainArtifactDetailsWithUserWithPermissionOnMainProject = Helper.ArtifactStore.GetArtifactDetails(userWithPermissionOnMainProjectOnly, mainArtifact.Id);

            // Verify: Returned ArtifactDeatils contains invalid inline traceLink to the inlineTraceArtifact
            ArtifactStoreHelper.ValidateInlineTraceLinkFromArtifactDetails(
                mainArtifactDetailsWithUserWithPermissionOnMainProject, inlineTraceArtifact, validInlineTraceLink: false);

            var indicatorFlags = baseArtifactType == BaseArtifactType.UIMockup ? ItemIndicatorFlags.HasManualReuseOrOtherTraces | ItemIndicatorFlags.HasUIMockup :
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
            var artifact = Helper.CreateAndPublishArtifact(_projects[0], _user, BaseArtifactType.Process);

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
            var artifact = Helper.CreateAndPublishArtifact(_projects[0], _user, BaseArtifactType.Process);
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
            var artifact = Helper.CreateAndPublishArtifact(_projects[0], _user, BaseArtifactType.Process);

            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _projects[0]);
            Helper.AssignProjectRolePermissionsToUser(viewer, TestHelper.ProjectRole.None, _projects[0], artifact);

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
        [Description("Create & publish an artifact, modify save & publish it again.  GetArtifactDetails with version=1 with a user that doesn't have access to the artifact.  " +
            "Verify it returns 403 Forbidden.")]
        public void GetArtifactDetailsWithVersion1_PublishedArtifactWithMultipleVersions_UserWithoutPermissions_403Forbidden()
        {
            var artifact = Helper.CreateAndPublishArtifact(_projects[0], _user, BaseArtifactType.Process);

            var unauthorizedUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _projects[0]);
            Helper.AssignProjectRolePermissionsToUser(unauthorizedUser, TestHelper.ProjectRole.None, _projects[0], artifact);

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
        [Description("Create & publish parent & child artifacts.  Make sure viewer does not have access to parent.  Viewer request GetArtifactDetails from child artifact.  " +
                    "Verify it returns 403 Forbidden.")]
        public void GetArtifactDetails_PublishedArtifactWithAChild_UserWithoutPermissionsToParent_403Forbidden()
        {
            var parent = Helper.CreateAndPublishArtifact(_projects[0], _user, BaseArtifactType.Process);
            var child = Helper.CreateAndPublishArtifact(_projects[0], _user, BaseArtifactType.Process, parent);

            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _projects[0]);
            Helper.AssignProjectRolePermissionsToUser(viewer, TestHelper.ProjectRole.None, _projects[0], parent);

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
            var artifact = Helper.CreateAndSaveArtifact(_projects[0], _user, BaseArtifactType.Process);
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
            var artifact = Helper.CreateAndPublishArtifact(_projects[0], _user, BaseArtifactType.Process);

            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id, versionId);
            }, "'GET {0}' should return 404 Not Found when passed an artifact ID that doesn't exist!",
                GET_ARTIFACT_ID_PATH);

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