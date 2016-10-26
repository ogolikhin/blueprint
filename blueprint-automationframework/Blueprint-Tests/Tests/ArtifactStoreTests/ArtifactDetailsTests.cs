using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommon;
using Utilities;

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

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(154601)]
        [Description("Create & publish an artifact, GetArtifactDetails.  Verify the artifact details are returned.")]
        public void GetArtifactDetails_PublishedArtifact_ReturnsArtifactDetails(BaseArtifactType artifactType)
        {
            const int VIEWER_PERMISSIONS = 1;

            IArtifact artifact = Helper.CreateAndPublishArtifact(_projects[0], _user, artifactType);
            var retrievedArtifact = OpenApiArtifact.GetArtifact(artifact.Address, _projects[0], artifact.Id, _user);

            NovaArtifactDetails artifactDetails = null;

            IUser viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _projects[0]);

            Assert.DoesNotThrow(() =>
            {
                artifactDetails = Helper.ArtifactStore.GetArtifactDetails(viewer, artifact.Id);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, retrievedArtifact);

            Assert.AreEqual(VIEWER_PERMISSIONS, artifactDetails.Permissions, "Viewer should have read permissions (i.e. 1)!");
        }

        [TestCase(2)]
        [TestCase(11)]
        [TestRail(154706)]
        [Description("Create & publish an artifact, modify & publish it again, GetArtifactDetails.  Verify the artifact details for the latest version are returned.")]
        public void GetArtifactDetails_PublishedArtifactWithMultipleVersions_ReturnsArtifactDetailsForLatestVersion(int numberOfVersions)
        {
            var openApiArtifacts = new List<IOpenApiArtifact>();
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projects[0], _user, BaseArtifactType.Process);

            var retrievedArtifactVersion = OpenApiArtifact.GetArtifact(artifact.Address, _projects[0], artifact.Id, _user);
            openApiArtifacts.Add(retrievedArtifactVersion);

            // Create several artifact versions.
            for (int i = 1; i < numberOfVersions; ++i)
            {
                // These are internal properties used by automation, so OpenAPI doesn't set them for us.
                retrievedArtifactVersion.Address = artifact.Address;
                retrievedArtifactVersion.CreatedBy = artifact.CreatedBy;

                // Modify & publish the artifact.
                retrievedArtifactVersion.Name = I18NHelper.FormatInvariant("{0}-version{1}", retrievedArtifactVersion.Name, i + 1);

                Artifact.SaveArtifact(retrievedArtifactVersion, _user);
                retrievedArtifactVersion.Publish();

                // Get the artifact from OpenAPI.
                retrievedArtifactVersion = OpenApiArtifact.GetArtifact(artifact.Address, _projects[0], artifact.Id, _user);
                openApiArtifacts.Add(retrievedArtifactVersion);
            }

            NovaArtifactDetails artifactDetails = null;

            Assert.DoesNotThrow(() =>
            {
                artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, retrievedArtifactVersion);

            // TODO: add check that Process has SpecificPropery - ClientType (?)

            Assert.AreEqual(8159, artifactDetails.Permissions, "Instance Admin should have all permissions (i.e. 8159)!");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(154700)]
        [Description("Create & publish an artifact, modify & publish it again, GetArtifactDetails with versionId=1.  Verify the artifact details for the first version are returned.")]
        public void GetArtifactDetailsWithVersionId1_PublishedArtifactWithMultipleVersions_ReturnsArtifactDetailsForFirstVersion(BaseArtifactType artifactType)
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projects[0], _user, artifactType);
            var retrievedArtifactVersion1 = OpenApiArtifact.GetArtifact(artifact.Address, _projects[0], artifact.Id, _user);

            // These are internal properties used by automation, so OpenAPI doesn't set them for us.
            retrievedArtifactVersion1.Address = artifact.Address;
            retrievedArtifactVersion1.CreatedBy = artifact.CreatedBy;

            // Modify & publish the artifact.
            var retrievedArtifactVersion2 = retrievedArtifactVersion1.DeepCopy();
            retrievedArtifactVersion2.Name = I18NHelper.FormatInvariant("{0}-version2", retrievedArtifactVersion1.Name);

            Artifact.SaveArtifact(retrievedArtifactVersion2, _user);
            retrievedArtifactVersion2.Publish();

            NovaArtifactDetails artifactDetails = null;

            Assert.DoesNotThrow(() =>
            {
                artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id, versionId: 1);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, retrievedArtifactVersion1);

            // TODO: add check that Process has SpecificPropery - ClientType (?)

            Assert.AreEqual(8159, artifactDetails.Permissions, "Instance Admin should have all permissions (i.e. 8159)!");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(166146)]
        [Description("Create & publish an artifact, modify & publish it again, then delete & publish it.  GetArtifactDetails with versionId=1.  " +
            "Verify the artifact details for the first version are returned.")]
        public void GetArtifactDetailsWithVersionId1_DeletedArtifactWithMultipleVersions_ReturnsArtifactDetailsForFirstVersion(BaseArtifactType artifactType)
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projects[0], _user, artifactType);
            var retrievedArtifactVersion1 = OpenApiArtifact.GetArtifact(artifact.Address, _projects[0], artifact.Id, _user);

            // These are internal properties used by automation, so OpenAPI doesn't set them for us.
            retrievedArtifactVersion1.Address = artifact.Address;
            retrievedArtifactVersion1.CreatedBy = artifact.CreatedBy;

            // Modify & publish the artifact.
            var retrievedArtifactVersion2 = retrievedArtifactVersion1.DeepCopy();
            retrievedArtifactVersion2.Name = I18NHelper.FormatInvariant("{0}-version2", retrievedArtifactVersion1.Name);

            Artifact.SaveArtifact(retrievedArtifactVersion2, _user);
            retrievedArtifactVersion2.Publish();

            artifact.Delete();
            artifact.Publish();

            NovaArtifactDetails artifactDetails = null;

            Assert.DoesNotThrow(() =>
            {
                artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id, versionId: 1);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, retrievedArtifactVersion1);

            // TODO: add check that Process has SpecificPropery - ClientType (?)

            Assert.AreEqual(8159, artifactDetails.Permissions, "Instance Admin should have all permissions (i.e. 8159)!");
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(182509)]
        [Description("Create two artifacts: main artifact that has inline trace to inline trace artifact. Update the inline trace artifact information - Verify that GetArtifactDetails call returns updated inline trace information.")]
        public void GetArtifactDetails_UpdateInlineTraceArtifact_ReturnsUpdatedInlineTraceLink(
            BaseArtifactType baseArtifactType)
        {
            // Setup: Create to artifacts: main artifact and inline trace artifact with the same user on the same project
            var mainArtifact = Helper.CreateAndPublishArtifact(_projects[0], _user, baseArtifactType);
            var inlineTraceArtifact = Helper.CreateAndPublishArtifact(_projects[0], _user, baseArtifactType);
            NovaArtifactDetails inlineTraceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, inlineTraceArtifact.Id);

            // Update main artifact to have inline trace to inline trace artifact created
            NovaArtifactDetails artifactDetailsToUpdateMainArtifact = ArtifactStoreHelper.CreateNovaArtifactDetailsWithArtifact(mainArtifact);
            artifactDetailsToUpdateMainArtifact.Description = ArtifactStoreHelper.CreateArtifactInlineTraceValue(inlineTraceArtifact, inlineTraceArtifactDetails);

            // Update and publish the main artifact with inline trace to target artifact
            mainArtifact.Lock();
            Assert.DoesNotThrow(() => inlineTraceArtifactDetails = Artifact.UpdateArtifact(mainArtifact, _user, artifactDetailsChanges: artifactDetailsToUpdateMainArtifact),
                "UpdateArtifact call failed when using the following artifact ID: {0}!", mainArtifact.Id);
            mainArtifact.Publish();

            // Update inline trace artifact information
            NovaArtifactDetails artifactDetailsToUpdateInlineTraceArtifact = ArtifactStoreHelper.CreateNovaArtifactDetailsWithArtifact(inlineTraceArtifact);
            artifactDetailsToUpdateInlineTraceArtifact.Name = inlineTraceArtifact.Name + "_NameUpdated";

            // Update and publish the inline trace artifact
            inlineTraceArtifact.Lock();
            Assert.DoesNotThrow(() => inlineTraceArtifactDetails = Artifact.UpdateArtifact(inlineTraceArtifact, _user, artifactDetailsChanges: artifactDetailsToUpdateInlineTraceArtifact),
                "UpdateArtifact call failed when using the following artifact ID: {0}!", inlineTraceArtifact.Id);
            inlineTraceArtifact.Publish();

            // Execute: Get ArtifactDetails for main artifact
            var mainArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, mainArtifact.Id);

            // Validation: Verify that returned ArtifactDeatils contains the updated information for InlineTrace
            ArtifactStoreHelper.ValidateInlineTraceLinkFromArtifactDetails(mainArtifactDetails, inlineTraceArtifact, validInlineTraceLink: true);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(182552)]
        [Description("Create two artifacts: main and inline trace on different project. Update the inline trace artifact information - Verify that GetArtifactDetails call returns updated inline trace information.")]
        public void GetArtifactDetails_UpdateInlineTraceArtifactOnDifferentProject_ReturnsUpdatedInlineTraceLink(
    BaseArtifactType baseArtifactType)
        {
            // Setup: Get projects available from testing environment
            var mainProject = _projects[0];
            var secondProject = _projects[1];

            // Create two artifacts: main artifact under main project and inline trace artifact under the second project
            var mainArtifact = Helper.CreateAndPublishArtifact(mainProject, _user, baseArtifactType);
            var inlineTraceArtifact = Helper.CreateAndPublishArtifact(secondProject, _user, baseArtifactType);
            NovaArtifactDetails inlineTraceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, inlineTraceArtifact.Id);

            // Update main artifact to have inline trace to inline trace artifact created
            NovaArtifactDetails artifactDetailsToUpdateMainArtifact = ArtifactStoreHelper.CreateNovaArtifactDetailsWithArtifact(mainArtifact);
            artifactDetailsToUpdateMainArtifact.Description = ArtifactStoreHelper.CreateArtifactInlineTraceValue(inlineTraceArtifact, inlineTraceArtifactDetails);

            // Update and publish the main artifact with inline trace to target artifact
            mainArtifact.Lock();
            Assert.DoesNotThrow(() => inlineTraceArtifactDetails = Artifact.UpdateArtifact(mainArtifact, _user, artifactDetailsChanges: artifactDetailsToUpdateMainArtifact),
                "UpdateArtifact call failed when using the following artifact ID: {0}!", mainArtifact.Id);
            mainArtifact.Publish();

            // Update inline trace artifact information
            NovaArtifactDetails artifactDetailsToUpdateInlineTraceArtifact = ArtifactStoreHelper.CreateNovaArtifactDetailsWithArtifact(inlineTraceArtifact);
            artifactDetailsToUpdateInlineTraceArtifact.Name = inlineTraceArtifact.Name + "_NameUpdated";

            // Update and publish the inline trace artifact
            inlineTraceArtifact.Lock();
            Assert.DoesNotThrow(() => inlineTraceArtifactDetails = Artifact.UpdateArtifact(inlineTraceArtifact, _user, artifactDetailsChanges: artifactDetailsToUpdateInlineTraceArtifact),
                "UpdateArtifact call failed when using the following artifact ID: {0}!", inlineTraceArtifact.Id);
            inlineTraceArtifact.Publish();

            // Execute: Get ArtifactDetails for main artifact
            var mainArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, mainArtifact.Id);

            // Validation: Verify that returned ArtifactDeatils contains valid inline traceLink to the inlineTraceArtifact
            ArtifactStoreHelper.ValidateInlineTraceLinkFromArtifactDetails(mainArtifactDetails, inlineTraceArtifact, validInlineTraceLink: true);
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
            NovaArtifactDetails inlineTraceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, inlineTraceArtifact.Id);

            // Update main artifact to have inline trace to inline trace artifact created
            NovaArtifactDetails artifactDetailsToUpdateMainArtifact = ArtifactStoreHelper.CreateNovaArtifactDetailsWithArtifact(mainArtifact);
            artifactDetailsToUpdateMainArtifact.Description = ArtifactStoreHelper.CreateArtifactInlineTraceValue(inlineTraceArtifact, inlineTraceArtifactDetails);

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

            // Validation: Verify that returned ArtifactDeatils contains invalid inline traceLink to the inlineTraceArtifact
            ArtifactStoreHelper.ValidateInlineTraceLinkFromArtifactDetails(mainArtifactDetails, inlineTraceArtifact, validInlineTraceLink: false);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(182550)]
        [Description("Create two artifacts: main and inline trace on different project. - Verify that GetArtifactDetails call returns invalid inline trace link if the user doesn't have the access permission for the inline trace artifact")]
        public void GetArtifactDetails_GetArtifactDetailsUsingUserWithoutPermissionToInlineTraceArtifact_ReturnsInvalidInlineTraceLink(
            BaseArtifactType baseArtifactType)
        {
            // Setup: Get projects available from testing environment
            var mainProject = _projects[0];
            var secondProject = _projects[1];

            // Create two artifacts: main artifact under main project and inline trace artifact under the second project
            var mainArtifact = Helper.CreateAndPublishArtifact(mainProject, _user, baseArtifactType);
            var inlineTraceArtifact = Helper.CreateAndPublishArtifact(secondProject, _user, baseArtifactType);
            NovaArtifactDetails inlineTraceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, inlineTraceArtifact.Id);

            // Update main artifact to have inline trace to inline trace artifact created
            NovaArtifactDetails artifactDetailsToUpdateMainArtifact = ArtifactStoreHelper.CreateNovaArtifactDetailsWithArtifact(mainArtifact);
            artifactDetailsToUpdateMainArtifact.Description = ArtifactStoreHelper.CreateArtifactInlineTraceValue(inlineTraceArtifact, inlineTraceArtifactDetails);

            // Update and publish the main artifact with inline trace to target artifact
            mainArtifact.Lock();
            Assert.DoesNotThrow(() => inlineTraceArtifactDetails = Artifact.UpdateArtifact(mainArtifact, _user, artifactDetailsChanges: (NovaArtifactDetails)artifactDetailsToUpdateMainArtifact),
                "UpdateArtifact call failed when using the following artifact ID: {0}!", mainArtifact.Id);
            mainArtifact.Publish();

            // Create user with a permission only on main project
            var userWithPermissionOnMainProject = Helper.CreateUserWithProjectRolePermissions(role: TestHelper.ProjectRole.Author, projects: new List<IProject> { mainProject });

            // Execute: Get ArtifactDetails for the main artifact using the user without permission to inline trace artifact
            var mainArtifactDetailsWithUserWithPermissionOnMainProject = Helper.ArtifactStore.GetArtifactDetails(userWithPermissionOnMainProject, mainArtifact.Id);

            // Validation: Verify that returned ArtifactDeatils contains invalid inline traceLink to the inlineTraceArtifact
            ArtifactStoreHelper.ValidateInlineTraceLinkFromArtifactDetails(mainArtifactDetailsWithUserWithPermissionOnMainProject, inlineTraceArtifact, validInlineTraceLink: false);
        }

        #endregion 200 OK Tests

        #region 401 Unauthorized Tests

        [TestCase]
        [TestRail(154701)]
        [Description("Create & publish an artifact, GetArtifactDetails but don't send any Session-Token header.  Verify it returns 401 Unauthorized.")]
        public void GetArtifactDetails_PublishedArtifactNoTokenHeader_401Unauthorized()
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projects[0], _user, BaseArtifactType.Process);

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(user: null, artifactId: artifact.Id);
            }, "'GET {0}' should return 401 Unauthorized when passed a valid artifact ID but no Session-Token in the header!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        [TestCase]
        [TestRail(154702)]
        [Description("Create & publish an artifact, GetArtifactDetails but use an unauthorized token.  Verify it returns 401 Unauthorized.")]
        public void GetArtifactDetails_PublishedArtifactUnauthorizedToken_401Unauthorized()
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projects[0], _user, BaseArtifactType.Process);
            IUser unauthorizedUser = Helper.CreateUserAndAddToDatabase();

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(unauthorizedUser, artifact.Id);
            }, "'GET {0}' should return 401 Unauthorized when passed a valid artifact ID but an unauthorized token!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        #endregion 401 Unauthorized Tests

        #region 403 Forbidden Tests

        [TestCase]
        [TestRail(154703)]
        [Description("Create & publish an artifact, GetArtifactDetails with a user that doesn't have access to the artifact.  Verify it returns 403 Forbidden.")]
        public void GetArtifactDetails_PublishedArtifactUserWithoutPermissions_403Forbidden()
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projects[0], _user, BaseArtifactType.Process);

            IUser viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _projects[0]);
            Helper.AssignProjectRolePermissionsToUser(viewer, TestHelper.ProjectRole.None, _projects[0], artifact);

            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(viewer, artifact.Id);
            }, "'GET {0}' should return 403 Forbidden when passed a valid artifact ID but the user doesn't have permission to view the artifact!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

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
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projects[0], _user, BaseArtifactType.Process);

            IUser unauthorizedUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _projects[0]);
            Helper.AssignProjectRolePermissionsToUser(unauthorizedUser, TestHelper.ProjectRole.None, _projects[0], artifact);

            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(unauthorizedUser, artifact.Id, versionId: 1);
            }, "'GET {0}' should return 403 Forbidden when passed a valid artifact ID but the user doesn't have permission to view the artifact!", RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            string expectedMessage = I18NHelper.FormatInvariant("You do not have permission to access the artifact (ID: {0})", artifact.Id);

            AssertJsonResponseEquals(expectedMessage, ex.RestResponse.Content, "If called by a user without permission to the artifact, we should get an error message of '{0}'!", expectedMessage);
        }

        [TestCase]
        [TestRail(185236)]
        [Description("Create & publish parent & child artifacts.  Make sure viewer does not have access to parent.  Viewer request GetArtifactDetails from child artifact.  " +
                    "Verify it returns 403 Forbidden.")]
        public void GetArtifactDetails_PublishedArtifactWithAChild_UserWithoutPermissionsToParent_403Forbidden()
        {
            IArtifact parent = Helper.CreateAndPublishArtifact(_projects[0], _user, BaseArtifactType.Process);
            IArtifact child = Helper.CreateAndPublishArtifact(_projects[0], _user, BaseArtifactType.Process, parent);

            IUser viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _projects[0]);
            Helper.AssignProjectRolePermissionsToUser(viewer, TestHelper.ProjectRole.None, _projects[0], parent);

            var ex = Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(viewer, child.Id, versionId: 1);
            }, "'GET {0}' should return 403 Forbidden when passed a valid child artifact ID but the user doesn't have permission to view parent artifact!",
                            RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

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
            IArtifact artifact = Helper.CreateAndSaveArtifact(_projects[0], _user, BaseArtifactType.Process);
            IUser user2 = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(user2, artifact.Id);
            }, "'GET {0}' should return 404 Not Found when passed an unpublished artifact ID with a different user!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
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
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        [TestCase(0)]
        [TestCase(2)]
        [TestRail(166149)]
        [Description("Create & publish an artifact.  GetArtifactDetails and pass a non-existent Version ID (ex. 0 or 2).  Verify it returns 404 Not Found.")]
        public void GetArtifactDetailsWithVersion_NonExistentVersionId_404NotFound(int versionId)
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projects[0], _user, BaseArtifactType.Process);

            var ex = Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id, versionId);
            }, "'GET {0}' should return 404 Not Found when passed an artifact ID that doesn't exist!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

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

            JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
            {
                // This will alert us if new properties are added to the return JSON format.
                MissingMemberHandling = MissingMemberHandling.Error
            };

            MessageResult messageResult = JsonConvert.DeserializeObject<MessageResult>(jsonContent, jsonSettings);

            Assert.AreEqual(expectedMessage, messageResult.Message, assertMessage, assertMessageParams);
        }

        #endregion Private Functions
    }
}