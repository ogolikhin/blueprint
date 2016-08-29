using System;
using System.Collections.Generic;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ArtifactDetailsTests : TestBase
    {
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

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(154601)]
        [Description("Create & publish an artifact, GetArtifactDetails.  Verify the artifact details are returned.")]
        public void GetArtifactDetails_PublishedArtifact_ReturnsArtifactDetails(BaseArtifactType artifactType)
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            var retrievedArtifact = OpenApiArtifact.GetArtifact(artifact.Address, _project, artifact.Id, _user);

            NovaArtifactDetails artifactDetails = null;

            Assert.DoesNotThrow(() =>
            {
                artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            artifactDetails.AssertEquals(retrievedArtifact);

            Assert.IsEmpty(artifactDetails.CustomProperties,
                "We found Custom Properties in an artifact that shouldn't have any Custom Properties!");

            Assert.IsEmpty(artifactDetails.SpecificPropertyValues,
                "SpecificPropertyValues isn't implemented yet so it should be empty!");

            Assert.AreEqual(8159, artifactDetails.Permissions, "Instance Admin should have all permissions (i.e. 8159)!");
        }

        [TestCase(2)]
        [TestCase(11)]
        [TestRail(154706)]
        [Description("Create & publish an artifact, modify & publish it again, GetArtifactDetails.  Verify the artifact details for the latest version are returned.")]
        public void GetArtifactDetails_PublishedArtifactWithMultipleVersions_ReturnsArtifactDetailsForLatestVersion(int numberOfVersions)
        {
            var openApiArtifacts = new List<IOpenApiArtifact>();
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            var retrievedArtifactVersion = OpenApiArtifact.GetArtifact(artifact.Address, _project, artifact.Id, _user);
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
                retrievedArtifactVersion = OpenApiArtifact.GetArtifact(artifact.Address, _project, artifact.Id, _user);
                openApiArtifacts.Add(retrievedArtifactVersion);
            }

            NovaArtifactDetails artifactDetails = null;

            Assert.DoesNotThrow(() =>
            {
                artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
            }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            artifactDetails.AssertEquals(retrievedArtifactVersion);

            Assert.IsEmpty(artifactDetails.CustomProperties,
                "We found Custom Properties in an artifact that shouldn't have any Custom Properties!");

            Assert.IsEmpty(artifactDetails.SpecificPropertyValues,
                "SpecificPropertyValues isn't implemented yet so it should be empty!");

            Assert.AreEqual(8159, artifactDetails.Permissions, "Instance Admin should have all permissions (i.e. 8159)!");
        }

        [TestCase]  // TODO: This isn't in TestRail because it's only a temporary testcase.  When versionId is implemented, delete this test.
        [Description("This is a temporary testcase to let us know if/when the versionId parameter is implemented.")]
        public void GetArtifactDetailsWithVersionId1_PublishedArtifactWithMultipleVersions_501NotImplemented()
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            Assert.Throws<Http501NotImplementedException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id, versionId: 1);
            }, "If this is getting a 200 OK, that means the versionId feature is implemented now and this test should be deleted and test 154700 should be enabled.");
        }

        [TestCase(BaseArtifactType.Process)]
        [Explicit(IgnoreReasons.UnderDevelopment)]  // XXX: Currently fails with a 501 Not Implemented error.
        [TestRail(154700)]
        [Description("Create & publish an artifact, modify & publish it again, GetArtifactDetails with versionId=1.  Verify the artifact details for the first version are returned.")]
        public void GetArtifactDetailsWithVersionId1_PublishedArtifactWithMultipleVersions_ReturnsArtifactDetailsForFirstVersion(BaseArtifactType artifactType)
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            var retrievedArtifactVersion1 = OpenApiArtifact.GetArtifact(artifact.Address, _project, artifact.Id, _user);

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

            artifactDetails.AssertEquals(retrievedArtifactVersion1);

            Assert.IsEmpty(artifactDetails.CustomProperties,
                "We found Custom Properties in an artifact that shouldn't have any Custom Properties!");

            Assert.IsEmpty(artifactDetails.SpecificPropertyValues,
                "SpecificPropertyValues isn't implemented yet so it should be empty!");

            Assert.AreEqual(8159, artifactDetails.Permissions, "Instance Admin should have all permissions (i.e. 8159)!");
        }

        [TestCase]
        [TestRail(154701)]
        [Description("Create & publish an artifact, GetArtifactDetails but don't send any Session-Token header.  Verify it returns 401 Unauthorized.")]
        public void GetArtifactDetails_PublishedArtifactNoTokenHeader_401Unauthorized()
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

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
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);
            IUser unauthorizedUser = Helper.CreateUserAndAddToDatabase();

            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(unauthorizedUser, artifact.Id);
            }, "'GET {0}' should return 401 Unauthorized when passed a valid artifact ID but an unauthorized token!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        [TestCase]
        [TestRail(154703)]
        [Description("Create & publish an artifact, GetArtifactDetails with a user that doesn't have access to the artifact.  Verify it returns 403 Forbidden.")]
        public void GetArtifactDetails_PublishedArtifactUserWithoutPermissions_403Forbidden()
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);
            IUser unauthorizedUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken,
                InstanceAdminRole.BlueprintAnalytics);

            Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetArtifactDetails(unauthorizedUser, artifact.Id);
            }, "'GET {0}' should return 403 Forbidden when passed a valid artifact ID but the user doesn't have permission to view the artifact!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);
        }

        [TestCase]
        [TestRail(154704)]
        [Description("Create & save (but don't publish) an artifact, GetArtifactDetails with a different user.  Verify it returns 404 Not Found.")]
        public void GetArtifactDetails_UnpublishedArtifactOtherUser_404NotFound()
        {
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Process);
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
    }
}