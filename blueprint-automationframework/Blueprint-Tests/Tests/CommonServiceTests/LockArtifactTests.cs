using System.Collections.Generic;
using System.Linq;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using Utilities;
using Newtonsoft.Json;
using TestCommon;

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
        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [Description("Attempt to get a lock on an artifact that has been published by the same user. Verify that the " +
                     "lock was obtained by the user.")]
        public void Lock_UnlockedArtifactPublishedBySameUser_VerifyLockObtained(BaseArtifactType baseArtifactType)
        {
            // Setup:
            // Publish artifact to ensure no lock remains on the newly created artifact.
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, baseArtifactType);

            LockResultInfo lockResultInfo = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                lockResultInfo = artifact.Lock();
            }, "Failed to lock an unlocked artifact published by the same user!");

            // Verify:
            // Assert that the lock was successfully obtained
            Assert.AreEqual(lockResultInfo.Result, LockResult.Success, 
                "The user was not able to obtain a lock on the {0} artifact when the artifact was not locked by any user.", baseArtifactType);

            // Assert that user can Publish the artifact to verify that the lock was actually obtained
            Assert.DoesNotThrow(() => artifact.Publish(),
                "The user was unable to Publish the artifact even though the lock appears to have been obtained successfully.");
        }

        [TestRail(107359)]
        [TestCase(BaseArtifactType.Process)]
        [Description("Attempt to get a lock on an artifact that has been published by another user. Verify that the " +
                     "lock was obtained by the user.")]
        public void Lock_UnlockedArtifactPublishedByOtherUser_VerifyLockObtained(BaseArtifactType baseArtifactType)
        {
            // Setup:
            // Publish artifact to ensure no lock remains on the newly created artifact.
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, baseArtifactType);

            LockResultInfo lockResultInfo = null;

            // Execute:
            // Second user locks the artifact
            Assert.DoesNotThrow(() =>
            {
                lockResultInfo = artifact.Lock(_user2);
            }, "Failed to lock an unlocked artifact published by another user!");

            // Verify:
            // Assert that the lock was successfully obtained
            Assert.AreEqual(lockResultInfo.Result, LockResult.Success,
                "The second user was not able to obtain a lock on the artifact that was published by the first user.");

            // Assert that the second user can Publish the artifact to verify that the lock was actually obtained
            Assert.DoesNotThrow(() => artifact.Publish(_user2),
                "The second user was unable to Publish the artifact even though the lock appears to have been obtained successfully.");
        }

        [TestRail(107361)]
        [TestCase(BaseArtifactType.Process)]
        [Description("Attempt to save a previously published artifact that has been locked by another user.  Verify that " +
                     "the user cannot save the artifact.")]
        public void Lock_SaveArtifactWhenLockedByOtherUser_VerifyNotSaved(BaseArtifactType baseArtifactType)
        {
            // Setup:
            // Publish artifact to ensure no lock remains on the newly created artifact
            var artifact = Helper.CreateAndPublishOpenApiArtifact(_project, _user, baseArtifactType);

            // Execute:
            Artifact.Lock(artifact, artifact.Address, _user);

            // Verify:
            // Assert that the second user cannot save the artifact
            var ex = Assert.Throws<Http409ConflictException>(() =>
                // Second user tries to save the artifact
                artifact.Save(_user2),
                "The second user attempted to save the artifact locked by another user and either an unexpected exception was thrown or " +
                "the second user's attempted save was successful.");

            Assert.IsNotNull(ex.RestResponse.Content, "No response content found in the exception message.");

            var failedSaveArtifactResults = JsonConvert.DeserializeObject<List<FailedArtifactResult>>(ex.RestResponse.Content);
            var failedSaveArtifactResult = failedSaveArtifactResults.First(a => a.ArtifactId == artifact.Id);

            Assert.AreEqual(ArtifactValidationMessage.ArtifactAlreadyLocked, failedSaveArtifactResult.Message,
                "The expected message content is: \"{0}\" but \"{1}\" was returned",
                ArtifactValidationMessage.ArtifactAlreadyLocked,
                failedSaveArtifactResult.Message);
        }

        [TestRail(107362)]
        [TestCase(BaseArtifactType.Process)]
        [Description("Attempt to publish a previously published artifact that has been locked by another user.  Verify that " +
                     "the user cannot publish the artifact.")]
        public void Lock_PublishArtifactWhenLockedByOtherUser_VerifyNotPublished(BaseArtifactType baseArtifactType)
        {
            // Setup:
            // Publish artifact to ensure no lock remains on the newly created artifact
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, baseArtifactType);

            // Execute:
            artifact.Lock(_user);

            // Verify:
            // Assert that the second user cannot publish the artifact
            var ex = Assert.Throws<Http409ConflictException>(() =>
                // Second user tries to publish the artifact
                artifact.Publish(_user2),
                "The second user attempted to publish the artifact locked by another user and either an unexpected exception was thrown or " +
                "the second user's attempted publish was successful.");

            Assert.IsNotNull(ex.RestResponse.Content, "No response content found in the exception message.");

            var failedPublishArtifactResults = JsonConvert.DeserializeObject<List<FailedArtifactResult>>(ex.RestResponse.Content);
            var failedPublishArtifactResult = failedPublishArtifactResults.First(a => a.ArtifactId == artifact.Id);

            string expectedMessage = I18NHelper.FormatInvariant(
                ArtifactValidationMessage.ArtifactAlreadyPublished,
                failedPublishArtifactResult.ArtifactId);

            Assert.AreEqual(expectedMessage, failedPublishArtifactResult.Message,
                "The expected message content is: \"{0}\" but \"{1}\" was returned",
                expectedMessage,
                failedPublishArtifactResult.Message);
        }

        [TestRail(107363)]
        [TestCase(BaseArtifactType.Process)]
        [Description("Verify that the artifact deletion doesn't work when the artifact is locked by the other user.")]
        public void Lock_DeleteArtifactWhenLockedByOtherUser_VerifyNotDeleted(BaseArtifactType baseArtifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, baseArtifactType);

            // Execute:
            // Update the process to lock it with the user1
            artifact.Lock(_user);

            // Verify:
            // Assert that the second user cannot delete the locked artifact
            var ex = Assert.Throws<Http409ConflictException>(() =>
                // Second user tries to save the artifact
                artifact.Delete(_user2),
                "The second user attempted to delete the artifact locked by another user and either an unexpected " +
                "exception was thrown or the second user's attempted delete was successful.");

            Assert.IsNotNull(ex.RestResponse.Content, "No response content found in the exception message.");

            var failedDeleteArtifactResults = JsonConvert.DeserializeObject<List<FailedArtifactResult>>(ex.RestResponse.Content);
            var failedDeleteArtifactResult = failedDeleteArtifactResults.First(a => a.ArtifactId == artifact.Id);

            Assert.AreEqual(ArtifactValidationMessage.ArtifactAlreadyLocked, failedDeleteArtifactResult.Message,
                "The expected message content is: \"{0}\" but \"{1}\" was returned",
                ArtifactValidationMessage.ArtifactAlreadyLocked,
                failedDeleteArtifactResult.Message);
        }

        [TestRail(107364)]
        [TestCase(BaseArtifactType.Process)]
        [Description("Verify that the artifact discard doesn't work when the artifact is locked by the other user.")]
        public void Lock_DiscardArtifactWhenLockedByOtherUser_VerifyNotDiscarded(BaseArtifactType baseArtifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, baseArtifactType);

            // Execute:
            // Update the process to lock it with the user1
            artifact.Lock(_user);

            // Verify:
            // Assert that the second user cannot discard the locked artifact
            var ex = Assert.Throws<Http409ConflictException>(() =>
                // Second user tries to save the artifact
                artifact.Discard(_user2),
                "The second user attempted to discard the artifact locked by another user and either an unexpected " +
                "exception was thrown or the second user's attempted discard was successful.");

            Assert.IsNotNull(ex.RestResponse.Content, "No response content found in the exception message.");

            var failedDiscardArtifactResults = JsonConvert.DeserializeObject<List<FailedArtifactResult>>(ex.RestResponse.Content);
            var failedDiscardArtifactResult = failedDiscardArtifactResults.First(a => a.ArtifactId == artifact.Id);

            string expectedMessage = I18NHelper.FormatInvariant(
                ArtifactValidationMessage.ArtifactNothingToDiscard,
                failedDiscardArtifactResult.ArtifactId);

            Assert.AreEqual(expectedMessage, failedDiscardArtifactResult.Message,
                "The expected message content is: \"{0}\" but \"{1}\" was returned",
                expectedMessage,
                failedDiscardArtifactResult.Message);
        }

        [TestRail(107365)]
        [TestCase(BaseArtifactType.Process, 3)]
        [Description("Verify the lock status perperty values of Locked artifacts.")]
        public void Lock_MultipleArtifacts_VerifyLocksObtained(BaseArtifactType baseArtifactType, int numberOfArtifacts)
        {
            // Setup:
            var artifactList = Helper.CreateAndPublishMultipleArtifacts(_project, _user, baseArtifactType, numberOfArtifacts);

            List<LockResultInfo> lockResultInfoList = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                // Obtain locks for artifact(s) with user.
                lockResultInfoList = Artifact.LockArtifacts(artifactList, artifactList.First().Address, _user);
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
                    artifact.Id, artifact.CreatedBy.Username, lockResultInfo.Info.LockOwnerLogin);
            }
        }

        [TestRail(107366)]
        [TestCase(BaseArtifactType.Process, 3)]
        [Description("When locking multiple artifacts, if one artifact is locked by other user, locks will be obtained for all other unlocked artifacts.")]
        public void Lock_MultipleUnlockedArtifactsWithOneArtifactLockedByOtherUser_AllLocksObtainedExceptForArtifactLockedByOtherUser(BaseArtifactType baseArtifactType, int numberOfArtifacts)
        {
            // Setup:
            var artifactList = Helper.CreateAndPublishMultipleArtifacts(_project, _user, baseArtifactType, numberOfArtifacts);

            // Lock one artifact with user2.
            IArtifact firstArtifact = artifactList[0] as IArtifact;
            var lockResult = firstArtifact.Lock(_user2);
            Assert.That(lockResult.Result == LockResult.Success, "User2 failed to get lock for the first artifact!");

            List<LockResultInfo> lockResultInfoList = null;
            List<LockResult> expectedLockResults = new List<LockResult> { LockResult.Success, LockResult.AlreadyLocked };

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                // Obtain locks for artifact(s) with the user1
                lockResultInfoList = Artifact.LockArtifacts(artifactList, artifactList.First().Address, _user, expectedLockResults);
            }, "LockArtifacts() should return 200 OK when passed multiple valid artifact IDs.");
            
            // Verify:
            // Verify that lock obtained for all artifacts except the first one.
            foreach (var artifact in artifactList)
            {
                LockResultInfo lockResultInfo = lockResultInfoList.Find(x => x.Info.ArtifactId == artifact.Id);

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
        [TestCase(BaseArtifactType.Process)]
        [Description("User attempts to get a lock when they already have a lock on the artifact.  Verify that the lock is obtained.")]
        public void Lock_ArtifactIsAlreadyLockedBySameUser_VerifyLockObtained(BaseArtifactType baseArtifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishOpenApiArtifact(_project, _user, baseArtifactType);

            // Saves the artifact (which gets a lock).
            artifact.Save();

            LockResultInfo lockResultInfo = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                // Attempt to lock artifact that already has been locked due to the save
                lockResultInfo = Artifact.Lock(artifact, artifact.Address, _user);
            }, "Failed to lock an already locked artifact!");

            // Verify:
            // Assert that the lock was successfully obtained
            Assert.AreEqual(lockResultInfo.Result, LockResult.Success,
                "The user was not able to obtain a lock on the artifact when the artifact was already locked by the user.");

            // Assert that user can Publish the artifact to verify that the lock was actually obtained
            Assert.DoesNotThrow(() => artifact.Publish(),
                "The user was unable to Publish the artifact even though the lock appears to have been obtained successfully.");
        }
    }
}
