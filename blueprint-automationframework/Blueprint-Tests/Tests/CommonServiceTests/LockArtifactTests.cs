using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TestCommon;
using Utilities;

namespace CommonServiceTests
{
    [TestFixture]
    [Category(Categories.ArtifactVersion)]
    public class LockArtifactTests : TestBase
    {
        private IUser _user;
        private IUser _user2;
        private IProject _project;

        #region Setup and Cleanup

        [SetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _user2 = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
        }

        [TearDown]
        public void ClassTearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        [TestRail(107358)]
        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForNovaRestMethods))]
        [Description("Attempt to get a lock on an artifact that has been published by the same user. Verify that the " +
                     "lock was obtained by the user.")]
        public void Lock_UnlockedArtifactPublishedBySameUser_VerifyLockObtained(ItemTypePredefined baseArtifactType)
        {
            // Setup:
            // Publish artifact to ensure no lock remains on the newly created artifact.
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, baseArtifactType);

            List<LockResultInfo> lockResultInfo = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                lockResultInfo = Helper.SvcShared.LockArtifact(_user, artifact.Id);
            }, "Failed to lock an unlocked artifact published by the same user!");

            // Verify:
            // Assert that the lock was successfully obtained
            Assert.AreEqual(lockResultInfo.First().Result, LockResult.Success, 
                "The user was not able to obtain a lock on the {0} artifact when the artifact was not locked by any user.", baseArtifactType);

            // Assert that user can Publish the artifact to verify that the lock was actually obtained
            Assert.DoesNotThrow(() => Helper.ArtifactStore.PublishArtifact(artifact.Id, _user),
                "The user was unable to Publish the artifact even though the lock appears to have been obtained successfully.");
        }

        [TestRail(107359)]
        [TestCase(ItemTypePredefined.Process)]
        [Description("Attempt to get a lock on an artifact that has been published by another user. Verify that the " +
                     "lock was obtained by the user.")]
        public void Lock_UnlockedArtifactPublishedByOtherUser_VerifyLockObtained(ItemTypePredefined baseArtifactType)
        {
            // Setup:
            // Publish artifact to ensure no lock remains on the newly created artifact.
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, baseArtifactType);

            List<LockResultInfo> lockResultInfo = null;

            // Execute:
            // Second user locks the artifact
            Assert.DoesNotThrow(() =>
            {
                lockResultInfo = Helper.SvcShared.LockArtifact(_user2, artifact.Id);
            }, "Failed to lock an unlocked artifact published by another user!");

            // Verify:
            // Assert that the lock was successfully obtained
            Assert.AreEqual(lockResultInfo.First().Result, LockResult.Success,
                "The second user was not able to obtain a lock on the artifact that was published by the first user.");

            // Assert that the second user can Publish the artifact to verify that the lock was actually obtained
            Assert.DoesNotThrow(() => Helper.ArtifactStore.PublishArtifact(artifact.Id, _user2),
                "The second user was unable to Publish the artifact even though the lock appears to have been obtained successfully.");
        }

        [TestRail(107361)]
        [TestCase(ItemTypePredefined.Process)]
        [Description("Attempt to save a previously published artifact that has been locked by another user.  Verify that " +
                     "the user cannot save the artifact.")]
        public void Lock_SaveArtifactWhenLockedByOtherUser_VerifyNotSaved(ItemTypePredefined baseArtifactType)
        {
            // Setup:
            // Publish artifact to ensure no lock remains on the newly created artifact
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, baseArtifactType);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                Helper.SvcShared.LockArtifact(_user, artifact.Id);
            }, "Failed to lock an unlocked artifact published by the same user!");

            // Verify:
            // Assert that the second user cannot save the artifact
            var ex = Assert.Throws<Http409ConflictException>(() =>
                // Second user tries to save the artifact
                Helper.ArtifactStore.UpdateArtifact(_user2, artifact.Artifact),
                "The second user attempted to save the artifact locked by another user and either an unexpected exception was thrown or " +
                "the second user's attempted save was successful.");

            Assert.IsNotNull(ex.RestResponse.Content, "No response content found in the exception message.");

            const string expectedExceptionMessage = "Artifact locked by another user.";

            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.LockedByOtherUser, expectedExceptionMessage);
        }

        [TestRail(107362)]
        [TestCase(ItemTypePredefined.Process)]
        [Description("Attempt to publish a previously published artifact that has been locked by another user.  Verify that " +
                     "the user cannot publish the artifact.")]
        public void Lock_PublishArtifactWhenLockedByOtherUser_VerifyNotPublished(ItemTypePredefined baseArtifactType)
        {
            // Setup:
            // Publish artifact to ensure no lock remains on the newly created artifact
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, baseArtifactType);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                Helper.SvcShared.LockArtifact(_user, artifact.Id);
            }, "Failed to lock an unlocked artifact published by the same user!");

            // Verify:
            // Assert that the second user cannot publish the artifact
            var ex = Assert.Throws<Http409ConflictException>(() =>
                // Second user tries to publish the artifact
                Helper.ArtifactStore.PublishArtifact(artifact.Id, _user2),
                "The second user attempted to publish the artifact locked by another user and either an unexpected exception was thrown or " +
                "the second user's attempted publish was successful.");

            Assert.IsNotNull(ex.RestResponse.Content, "No response content found in the exception message.");

            string expectedExceptionMessage = I18NHelper.FormatInvariant(
                "Artifact with ID {0} has nothing to publish. The artifact will now be refreshed.",
                artifact.Id);

            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotPublish, expectedExceptionMessage);
        }

        [TestRail(107363)]
        [TestCase(ItemTypePredefined.Process)]
        [Description("Verify that the artifact deletion doesn't work when the artifact is locked by the other user.")]
        public void Lock_DeleteArtifactWhenLockedByOtherUser_VerifyNotDeleted(ItemTypePredefined baseArtifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, baseArtifactType);

            // Execute:
            // Update the process to lock it with the user1
            Assert.DoesNotThrow(() =>
            {
                Helper.SvcShared.LockArtifact(_user, artifact.Id);
            }, "Failed to lock an unlocked artifact published by the same user!");

            // Verify:
            // Assert that the second user cannot delete the locked artifact
            var ex = Assert.Throws<Http409ConflictException>(() =>
                // Second user tries to save the artifact
                Helper.ArtifactStore.DeleteArtifact(artifact.Id, _user2),
                "The second user attempted to delete the artifact locked by another user and either an unexpected " +
                "exception was thrown or the second user's attempted delete was successful.");

            Assert.IsNotNull(ex.RestResponse.Content, "No response content found in the exception message.");

            string expectedExceptionMessage = I18NHelper.FormatInvariant("Artifact \"{0}: {1}\" is already locked by other user",
                artifact.Prefix, artifact.Id);

            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.LockedByOtherUser, expectedExceptionMessage);

        }

        [TestRail(107364)]
        [TestCase(ItemTypePredefined.Process)]
        [Description("Verify that the artifact discard doesn't work when the artifact is locked by the other user.")]
        public void Lock_DiscardArtifactWhenLockedByOtherUser_VerifyNotDiscarded(ItemTypePredefined baseArtifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, baseArtifactType);

            // Execute:
            // Update the process to lock it with the user1
            Assert.DoesNotThrow(() =>
            {
                Helper.SvcShared.LockArtifact(_user, artifact.Id);
            }, "Failed to lock an unlocked artifact published by the same user!");

            // Verify:
            // Assert that the second user cannot discard the locked artifact
            var ex = Assert.Throws<Http409ConflictException>(() =>
                // Second user tries to save the artifact
                Helper.ArtifactStore.DiscardArtifact(_user2, artifact.Id),
                "The second user attempted to discard the artifact locked by another user and either an unexpected " +
                "exception was thrown or the second user's attempted discard was successful.");

            Assert.IsNotNull(ex.RestResponse.Content, "No response content found in the exception message.");

            string expectedExceptionMessage = I18NHelper.FormatInvariant("Artifact with ID {0} has nothing to discard.",
                artifact.Id);

            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotDiscard, expectedExceptionMessage);
        }

        [TestRail(107365)]
        [TestCase(ItemTypePredefined.Process, 3)]
        [Description("Verify the lock status perperty values of Locked artifacts.")]
        public void Lock_MultipleArtifacts_VerifyLocksObtained(ItemTypePredefined baseArtifactType, int numberOfArtifacts)
        {
            // Setup:
            var artifactList = Helper.CreateAndPublishMultipleArtifacts(_project, _user, baseArtifactType, numberOfArtifacts);

            List<LockResultInfo> lockResultInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                // Obtain locks for artifact(s) with user.
                lockResultInfoList = Helper.SvcShared.LockArtifacts(_user, artifactList.Select(a => a.Id).ToList());
            }, "Locking multiple artifacts failed!");

            // Verify:
            Assert.That(lockResultInfoList.Count == artifactList.Count,
                "LockArtifacts should return {0} LockResultInfo objects, but it returned {1}.",
                lockResultInfoList.Count, artifactList.Count);

            // Verify that lock obtained by checking the status section of artifact(s)?
            foreach (var artifact in artifactList)
            {
                var lockResultInfo = lockResultInfoList[artifactList.IndexOf(artifact)];

                Assert.That(lockResultInfo.Result.Equals(LockResult.Success),
                    "Lock Result for Artifact ID {0} should be \"Success\" but the result from the get lock call is \"{1}\",",
                    artifact.Id, lockResultInfo.Result);

                Assert.That(lockResultInfo.Info.LockOwnerLogin == null,
                    "Artifact ID {0} should be locked by 'null' (meaning current user '{1}') but the result from the get lock call informed that " +
                    "it's locked by '{2}'.",
                    artifact.Id, artifact.Artifact.CreatedBy.DisplayName, lockResultInfo.Info.LockOwnerLogin);
            }
        }

        [TestRail(107366)]
        [TestCase(ItemTypePredefined.Process, 3)]
        [Description("When locking multiple artifacts, if one artifact is locked by other user, locks will be obtained for all other unlocked artifacts.")]
        public void Lock_MultipleUnlockedArtifactsWithOneArtifactLockedByOtherUser_AllLocksObtainedExceptForArtifactLockedByOtherUser(ItemTypePredefined baseArtifactType, int numberOfArtifacts)
        {
            // Setup:
            var artifactList = Helper.CreateAndPublishMultipleArtifacts(_project, _user, baseArtifactType, numberOfArtifacts);

            // Lock one artifact with user2.
            var firstArtifact = artifactList[0];
            var lockResult = Helper.SvcShared.LockArtifact(_user2, firstArtifact.Id);
            Assert.That(lockResult.First().Result == LockResult.Success, "User2 failed to get lock for the first artifact!");

            List<LockResultInfo> lockResultInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                // Obtain locks for artifact(s) with the user1
                lockResultInfoList = Helper.SvcShared.LockArtifacts(_user, artifactList.Select(a => a.Id).ToList());
            }, "LockArtifacts() should return 200 OK when passed multiple valid artifact IDs.");

            // Verify:
            // Verify that lock obtained for all artifacts except the first one.
            foreach (var artifact in artifactList)
            {
                var lockResultInfo = lockResultInfoList.Find(x => x.Info.ArtifactId == artifact.Id);

                Assert.NotNull(lockResultInfo, "No LockResultInfo was returned for artifact ID {0} after trying to lock it!", artifact.Id);

                var expectedLockResult = (artifact.Id == firstArtifact.Id) ? LockResult.AlreadyLocked : LockResult.Success;
                var expectedLockOwner = (artifact.Id == firstArtifact.Id) ? _user2.Username : null;

                Assert.AreEqual(lockResultInfo.Result, expectedLockResult,
                    "The lock result for artifact ID {0} should be '{1}' but the result from the get lock call is '{2}'.",
                    artifact.Id, expectedLockResult, lockResultInfo.Result);

                Assert.AreEqual(lockResultInfo.Info.LockOwnerLogin, expectedLockOwner,
                    "Artifact ID {0} should be locked by the user '{1}' but the result from the get lock call shows the lock owner is '{2}'.",
                    artifact.Id, expectedLockOwner ?? "null", lockResultInfo.Info.LockOwnerLogin);
            }
        }

        [TestRail(107378)]
        [TestCase(ItemTypePredefined.Process)]
        [Description("User attempts to get a lock when they already have a lock on the artifact.  Verify that the lock is obtained.")]
        public void Lock_ArtifactIsAlreadyLockedBySameUser_VerifyLockObtained(ItemTypePredefined baseArtifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, _project, baseArtifactType);

            // Lock the artifact again with the same user.
            Helper.SvcShared.LockArtifact(_user, artifact.Id);

            List<LockResultInfo> lockResultInfo = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                // Attempt to lock artifact that already has been locked due to the save
                lockResultInfo = Helper.SvcShared.LockArtifact(_user, artifact.Id);
            }, "Failed to lock an already locked artifact!");

            // Verify:
            // Assert that the lock was successfully obtained
            Assert.NotNull(lockResultInfo, "LockArtifact() returned null!");
            Assert.AreEqual(LockResult.Success,  lockResultInfo[0].Result,
                "The user was not able to obtain a lock on the artifact when the artifact was already locked by the user.");

            // Assert that user can Publish the artifact to verify that the lock was actually obtained
            Assert.DoesNotThrow(() => Helper.ArtifactStore.PublishArtifact(artifact.Id, _user),
                "The user was unable to Publish the artifact even though the lock appears to have been obtained successfully.");
        }
    }
}
