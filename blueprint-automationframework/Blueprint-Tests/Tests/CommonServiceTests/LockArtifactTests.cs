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

        // TODO: check discard exception: Artifact 13631 has nothing to discard on tear-down
        [TestRail(107358)]
        [TestCase(BaseArtifactType.Actor)]
        [TestCase(BaseArtifactType.BusinessProcess)]
        [TestCase(BaseArtifactType.Document)]
        [TestCase(BaseArtifactType.DomainDiagram)]
        [TestCase(BaseArtifactType.GenericDiagram)]
        [TestCase(BaseArtifactType.Glossary)]
        [TestCase(BaseArtifactType.Process)]
        [TestCase(BaseArtifactType.Storyboard)]
        [TestCase(BaseArtifactType.TextualRequirement)]
        [TestCase(BaseArtifactType.UIMockup)]
        [TestCase(BaseArtifactType.UseCase)]
        [TestCase(BaseArtifactType.UseCaseDiagram)]
        [Description("Attempt to get a lock on an artifact that has been published by the same user. Verify that the" +
                     "lock was obtained by the user.")]
        public void GetLockForArtifactWithNoExistingLocks_VerifyLockObtained(BaseArtifactType baseArtifactType)
        {
            var artifact = Helper.CreateArtifact(_project, _user, baseArtifactType);
            artifact.Save();

            // Publish artifact to ensure no lock remains on the newly created artifact
            artifact.Publish();

            var lockResultInfo = artifact.Lock();

            // Assert that the lock was successfully obtained
            Assert.AreEqual(lockResultInfo.Result, 
                LockResult.Success, 
                I18NHelper.FormatInvariant("The user was not able to obtain a lock on the {0} artifact when " +
                "the artifact was not locked by any user.", baseArtifactType.ToString())
                                                                       );

            // Assert that user can Publish the artifact to verify that the lock was actually obtained
            Assert.DoesNotThrow(() =>
                artifact.Publish(),
                "The user was unable to Publish the artifact even though the lock appears to have been obtained successfully."
                );
        }

        [TestRail(107359)]
        [TestCase(BaseArtifactType.Process)]
        [Description("Attempt to get a lock on an artifact that has been published by another user. Verify that the" +
                     "lock was obtained by the user.")]
        public void GetLockForArtifactPublishedByOtherUser_VerifyLockObtained(BaseArtifactType baseArtifactType)
        {
            var artifact = Helper.CreateArtifact(_project, _user, baseArtifactType);
            artifact.Save();

            // Publish artifact to ensure no lock remains on the newly created artifact
            artifact.Publish();

            // Second user locks the artifact
            var lockResultInfo = artifact.Lock(_user2);

            // Assert that the lock was successfully obtained
            Assert.AreEqual(lockResultInfo.Result, LockResult.Success, "The second user was not able to obtain a lock on the artifact that" +
                                                                       "was published by the first user.");

            // Assert that the second user can Publish the artifact to verify that the lock was actually obtained
            Assert.DoesNotThrow(() =>
                artifact.Publish(_user2),
                "The second user was unable to Publish the artifact even though the lock appears to have been obtained successfully."
                );
        }

        [TestRail(107361)]
        [TestCase(BaseArtifactType.Process)]
        [Description("Attempt to save a previously published artifact that has been locked by another user.  Verify that" +
                     "the user cannot save the artifact.")]
        public void SaveArtifactWhenLockedByOtherUser_VerifyNotSaved(BaseArtifactType baseArtifactType)
        {
            var artifact = Helper.CreateArtifact(_project, _user, baseArtifactType);
            artifact.Save();

            // Publish artifact to ensure no lock remains on the newly created artifact
            artifact.Publish();

            artifact.Lock();

            // Assert that the second user cannot save the artifact
            var ex = Assert.Throws<Http409ConflictException>(() =>
                // Second user tries to save the artifact
                artifact.Save(_user2),
                "The second user attempted to save the artifact locked by another user and either an unexpected exception was thrown or" +
                "the second user's attempted save was successful."
                );

            Assert.IsNotNull(ex.RestResponse.Content, "No response content found in the exception message.");

            var failedSaveArtifactResults = JsonConvert.DeserializeObject<List<FailedArtifactResult>>(ex.RestResponse.Content);
            var failedSaveArtifactResult = failedSaveArtifactResults.First(a => a.ArtifactId == artifact.Id);

            string errorMessage =
                I18NHelper.FormatInvariant(
                    "The expected message content is: \"{0}\" but \"{1}\" was returned", 
                    ArtifactValidationMessage.ArtifactAlreadyLocked, 
                    failedSaveArtifactResult.Message);

            Assert.AreEqual(ArtifactValidationMessage.ArtifactAlreadyLocked, 
                failedSaveArtifactResult.Message, 
                errorMessage);
        }

        [TestRail(107362)]
        [TestCase(BaseArtifactType.Process)]
        [Description("Attempt to publish a previously published artifact that has been locked by another user.  Verify that" +
                     "the user cannot publish the artifact.")]
        public void PublishArtifactWhenLockedByOtherUser_VerifyNotPublished(BaseArtifactType baseArtifactType)
        {
            var artifact = Helper.CreateArtifact(_project, _user, baseArtifactType);
            artifact.Save();

            // Publish artifact to ensure no lock remains on the newly created artifact
            artifact.Publish();

            artifact.Lock();

            // Assert that the second user cannot publish the artifact
            var ex = Assert.Throws<Http409ConflictException>(() =>
                // Second user tries to publish the artifact
                artifact.Publish(_user2),
                "The second user attempted to publish the artifact locked by another user and either an unexpected exception was thrown or" +
                "the second user's attempted publish was successful."
                );

            Assert.IsNotNull(ex.RestResponse.Content, "No response content found in the exception message.");

            var failedPublishArtifactResults = JsonConvert.DeserializeObject<List<FailedArtifactResult>>(ex.RestResponse.Content);
            var failedPublishArtifactResult = failedPublishArtifactResults.First(a => a.ArtifactId == artifact.Id);

            string expectedMessage = I18NHelper.FormatInvariant(
                ArtifactValidationMessage.ArtifactAlreadyPublished,
                failedPublishArtifactResult.ArtifactId);

            string errorMessage =
                I18NHelper.FormatInvariant(
                    "The expected message content is: \"{0}\" but \"{1}\" was returned",
                    expectedMessage,
                    failedPublishArtifactResult.Message);

            Assert.AreEqual(expectedMessage,
                failedPublishArtifactResult.Message,
                errorMessage);
        }

        [TestRail(107363)]
        [TestCase(BaseArtifactType.Process)]
        [Description("Verify that the artifact deletion doesn't work when the artifact is locked by the other user.")]
        public void DeleteArtifactWhenLockedByOtherUser_VerifyNotDeleted(BaseArtifactType baseArtifactType)
        {
            // Create an artifact and publish with the user1
            var artifact = Helper.CreateArtifact(_project, _user, baseArtifactType);
            artifact.Save();
            artifact.Publish();

            // Update the process to lock it with the user1
            artifact.Lock();

            // Assert that the second user cannot delete the locked artifact
            var ex = Assert.Throws<Http409ConflictException>(() =>
                // Second user tries to save the artifact
                artifact.Delete(_user2),
                "The second user attempted to delete the artifact locked by another user and either an unexpected " +
                "exception was thrown or the second user's attempted delete was successful."
                );

            Assert.IsNotNull(ex.RestResponse.Content, "No response content found in the exception message.");

            var failedDeleteArtifactResults = JsonConvert.DeserializeObject<List<FailedArtifactResult>>(ex.RestResponse.Content);

            var failedDeleteArtifactResult = failedDeleteArtifactResults.First(a => a.ArtifactId == artifact.Id);

            string errorMessage =
                I18NHelper.FormatInvariant(
                    "The expected message content is: \"{0}\" but \"{1}\" was returned",
                    ArtifactValidationMessage.ArtifactAlreadyLocked,
                    failedDeleteArtifactResult.Message);

            Assert.AreEqual(ArtifactValidationMessage.ArtifactAlreadyLocked,
                failedDeleteArtifactResult.Message,
                errorMessage);
        }

        [TestRail(107364)]
        [TestCase(BaseArtifactType.Process)]
        [Description("Verify that the artifact discard doesn't work when the artifact is locked by the other user.")]
        public void DiscardArtifactWhenLockedByOtherUser_VerifyNotDiscarded(BaseArtifactType baseArtifactType)
        {
            // Create an artifact and publish with the user1
            var artifact = Helper.CreateArtifact(_project, _user, baseArtifactType);
            artifact.Save();
            artifact.Publish();

            // Update the process to lock it with the user1
            artifact.Lock();

            // Assert that the second user cannot discard the locked artifact
            var ex = Assert.Throws<Http409ConflictException>(() =>
                // Second user tries to save the artifact
                artifact.Discard(_user2),
                "The second user attempted to discard the artifact locked by another user and either an unexpected " +
                "exception was thrown or the second user's attempted discard was successful."
                );

            Assert.IsNotNull(ex.RestResponse.Content, "No response content found in the exception message.");

            var failedDiscardArtifactResults = JsonConvert.DeserializeObject<List<FailedArtifactResult>>(ex.RestResponse.Content);

            var failedDiscardArtifactResult = failedDiscardArtifactResults.First(a => a.ArtifactId == artifact.Id);

            string expectedMessage = I18NHelper.FormatInvariant(
                ArtifactValidationMessage.ArtifactNothingToDiscard,
                failedDiscardArtifactResult.ArtifactId);

            string errorMessage =
                I18NHelper.FormatInvariant(
                    "The expected message content is: \"{0}\" but \"{1}\" was returned",
                    ArtifactValidationMessage.ArtifactAlreadyLocked,
                    failedDiscardArtifactResult.Message);

            Assert.AreEqual(expectedMessage,
                failedDiscardArtifactResult.Message,
                errorMessage);
        }

        [TestRail(107365)]
        [TestCase(BaseArtifactType.Process, 3)]
        [Description("Verify the lock status perperty values of Locked artifacts")]
        public void GetLocksForMultipleArtifacts_VerifyLocksObtained(BaseArtifactType baseArtifactType, int iteration)
        {
            // Create artifact(s) and publish with the user1
            for (int i = 0; i < iteration; i++)
            {
                var artifact = Helper.CreateArtifact(_project, _user, baseArtifactType);
                artifact.Save();
                artifact.Publish();
            }

            List<IArtifactBase> artifactList = Helper.Artifacts;

            // Obtain locks for artifact(s) with the user1
            var lockResultInfoList = Artifact.LockArtifacts(artifactList, artifactList.First().Address, _user);

            // Verify that lock obtained by checking the status section of artifact(s)?
            foreach (var artifact in artifactList)
            {
                var lockResultInfo = lockResultInfoList[artifactList.IndexOf(artifact)];

                Assert.That(lockResultInfo.Result.Equals(LockResult.Success),
                    "The artifact {0} should be in \"Success\" but the result from the get lock call is \"{1}\",",
                    artifact.Id, lockResultInfo.Result.ToString());

                Assert.That(lockResultInfo.Info.LockOwnerLogin == null,
                    "The artifact {0} should be locked by the user {1} but the result from the get lock call informed that " +
                    "it's locked by {2}.",
                    artifact.Id, artifact.CreatedBy.Username, lockResultInfo.Info.LockOwnerLogin);
            }
        }

        [TestRail(107366)]
        [TestCase(BaseArtifactType.Process, 3)]
        [Description("Verify that the locked artifacts with one user cannot be locked by other user")]
        public void GetLocksForMultipleArtifactsWithOneArtifactLockedByOtherUser_VerifyLockIsNotObtainedForArtifactLockedByOtherUser(BaseArtifactType baseArtifactType, int iteration)
        {
            // Create artifact(s) and publish with the user1
            for (int i = 0; i < iteration; i++)
            {
                var artifact = Helper.CreateArtifact(_project, _user, baseArtifactType);
                artifact.Save();
                artifact.Publish();
            }

            List<IArtifactBase> artifactList = Helper.Artifacts;

            // Obtain locks for artifact(s) with the user1
            Artifact.LockArtifacts(artifactList, artifactList.First().Address, _user);

            // Verify that lock obtained by checking the status section of artifact(s)?
            foreach (var artifact in artifactList)
            {
                var lockResultInfo = ((IArtifact)artifact).Lock(_user2);

                Assert.That(lockResultInfo.Result.Equals(LockResult.AlreadyLocked),
                    "The artifact {0} should be in \"AlreadyLocked\" but the result from the get lock call is \"{1}\",",
                    artifact.Id, lockResultInfo.Result.ToString());

                Assert.That(lockResultInfo.Info.LockOwnerLogin.Equals(artifact.CreatedBy.Username),
                    "The artifact {0} should be locked by the user {1} but the result from the get lock call shows " +
                    "the lock owner is {2}.", artifact.Id, artifact.CreatedBy.Username, lockResultInfo.Info.LockOwnerLogin);
            }
        }

        [TestRail(107378)]
        [TestCase(BaseArtifactType.Process)]
        [Description("User attempts to get a lock when they already have a lock on the artifact.  Verify that the lock is obtained.")]
        public void GetLockForArtifactWhenLockAlreadyExistsForUser_VerifyLockObtained(BaseArtifactType baseArtifactType)
        {
            var artifact = Helper.CreateArtifact(_project, _user, baseArtifactType);

            // Adds the artifact
            artifact.Save();
            artifact.Publish();

            // Saves the artifact
            artifact.Save();

            // Attempt to lock artifact that already has been locked due to the save
            var lockResultInfo = artifact.Lock();

            // Assert that the lock was successfully obtained
            Assert.AreEqual(lockResultInfo.Result, LockResult.Success, "The user was not able to obtain a lock on the artifact when" +
                                                                       "the artifact was already locked by the user.");

            // Assert that user can Publish the artifact to verify that the lock was actually obtained
            Assert.DoesNotThrow(() =>
                artifact.Publish(),
                "The user was unable to Publish the artifact even though the lock appears to have been obtained successfully."
                );
        }
    }
}
