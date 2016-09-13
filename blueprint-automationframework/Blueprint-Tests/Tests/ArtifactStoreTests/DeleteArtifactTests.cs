﻿using System.Collections.Generic;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class DeleteArtifactTests : TestBase
    {
        private IUser _user = null;
        private IUser _userWithNoAccess = null;
        private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _userWithNoAccess = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens, null);
            _project = ProjectFactory.GetProject(_user);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region 200 OK tests

        [TestCase(BaseArtifactType.Actor)]
        public void DeleteArtifact_PublishedArtifactWithNoChildren_ArtifactIsDeleted(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            // Execute:
            List<INovaArtifactResponse> deletedArtifacts = null;

            Assert.DoesNotThrow(() => deletedArtifacts = Helper.ArtifactStore.DeleteArtifact(artifact, _user),
                "'DELETE {0}' should return 200 OK if a valid artifact ID is sent!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            // Verify:
            Assert.AreEqual(1, deletedArtifacts.Count, "There should only be 1 deleted artifact ID returned!");
            Assert.AreEqual(artifact.Id, deletedArtifacts[0].Id, "The artifact ID doesn't match the one that we deleted!");

            // Try to get the artifact and verify it's deleted.
            Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id),
                "We should get a 404 Not Found when trying to get artifact details of a deleted artifact!");
        }

        #endregion 200 OK tests

        #region 400 Bad Request tests
        // DeleteArtifact_xxxx_400BadRequest()
        #endregion 400 Bad Request tests

        #region 401 Unauthorized tests
        // DeleteArtifact_xxxx_401Unauthorized()
        #endregion 401 Unauthorized tests

        #region 403 Forbidden tests

        [TestRail(165817)]
        [TestCase(BaseArtifactType.Actor)]
        [Description("Create and publish an artifact. Attempt to delete the artifact with a user that does not have permission " +
                     "to delete. Verify that HTTP 403 Forbidden exception is thrown.")]
        public void DeleteArtifact_UserDoesNotHavePermissionToDelete_Http403Forbidden(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            // Execute & Verify:
            Assert.Throws<Http403ForbiddenException>(() => Helper.ArtifactStore.DeleteArtifact(artifact, _userWithNoAccess),
                "We should get a 403 Fordbidden when a user trying to delete an artifact does not have permission to delete!");
        }

        #endregion 403 Forbidden tests

        #region 404 Not Found tests

        [TestRail(165818)]
        [TestCase(BaseArtifactType.Actor)]
        [Description("Create and publish an artifact. Delete the artifact. Attempt to delete the same artifact again. Verify that " +
                     "HTTP 404 Not Found Exception is thrown.")]
        public void DeleteArtifact_DeletedArtifactNotPublished_404NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            Assert.DoesNotThrow(() => artifact.Delete(_user),
                "Failed to delete a published artifact!");

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.DeleteArtifact(artifact, _user),
                "We should get a 404 Not Found when trying to delete an artifact that has already been deleted!");
        }

        [TestRail(165819)]
        [TestCase(BaseArtifactType.Actor)]
        [Description("Create and publish an artifact. Delete the artifact and publish.  Attempt to delete the same artifact again. " +
                     "Verify that HTTP 404 Not Found Exception is thrown.")]
        public void DeleteArtifact_DeletedArtifactPublished_404NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            Assert.DoesNotThrow(() => artifact.Delete(_user),
                "Failed to delete a published artifact!");

            artifact.Publish(_user);

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.DeleteArtifact(artifact, _user),
                "We should get a 404 Not Found when trying to delete an artifact that has already been deleted and published!");
        }

        [TestRail(165820)]
        [TestCase(BaseArtifactType.Actor, 0)]
        [TestCase(BaseArtifactType.Actor, int.MaxValue)]
        [Description("Create and publish an artifact.  Attempt to delete the artifact after changing the artifact Id to a " +
                     "non existent Id. Verify that HTTP 404 Not Found Exception is thrown.")]
        public void DeleteArtifact_NonExistentArtifact_404NotFound(BaseArtifactType artifactType, int nonExistentArtifactId)
        {
            // Setup:
            IArtifact artifact = ArtifactFactory.CreateArtifact(_project, _user, artifactType);

            // Save original artifact Id
            var originalId = artifact.Id;

            // Replace artifact Id with non-existent Id
            artifact.Id = nonExistentArtifactId;

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.DeleteArtifact(artifact, _user),
                "We should get a 404 Not Found when trying to delete an artifact that does not exist!");

            // Restore original artifact Id for proper teardown
            artifact.Id = originalId;

        }

        #endregion 404 Not Found tests

        #region 409 Conflict tests
        // DeleteArtifact_xxxx_409Conflict()
        #endregion 409 Conflict tests
    }
}
