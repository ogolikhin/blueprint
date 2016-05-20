using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using CustomAttributes;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using Utilities;
using Newtonsoft.Json;

namespace CommonServiceTests
{
    [TestFixture]
    [Category(Categories.ArtifactVersion)]
    public class LockArtifactTests
    {
        private IAdminStore _adminStore;
        private IBlueprintServer _blueprintServer;
        private IUser _user;
        private IUser _user2;
        private IProject _project;

        private readonly List<IArtifact> _artifacts = new List<IArtifact>();

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _blueprintServer = BlueprintServerFactory.GetBlueprintServerFromTestConfig();
            _user = UserFactory.CreateUserAndAddToDatabase();
            _user2 = UserFactory.CreateUserAndAddToDatabase();
            _project = ProjectFactory.GetProject(_user);

            // Get a valid Access Control token for the user (for the new Storyteller REST calls).
            _adminStore.AddSession(_user);

            Assert.IsFalse(
                string.IsNullOrWhiteSpace(_user.Token.AccessControlToken),
                "The user didn't get an Access Control token!");

            // Get a valid OpenApi token for the user (for the OpenApi artifact REST calls).
            _blueprintServer.LoginUsingBasicAuthorization(_user, string.Empty);

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.OpenApiToken), "The user didn't get an OpenApi token!");

            // Get a valid Access Control token for the second user (for the new Storyteller REST calls).
            _adminStore.AddSession(_user2);

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user2.Token.AccessControlToken), "The second user didn't get an Access Control token!");

            // Get a valid OpenApi token for the second user (for the OpenApi artifact REST calls).
            _blueprintServer.LoginUsingBasicAuthorization(_user2, string.Empty);

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user2.Token.OpenApiToken), "The second user didn't get an OpenApi token!");
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            var savedArtifactsList = new List<IArtifactBase>();

            // Delete or Discard all the _artifacts that were added.
            foreach (var artifact in _artifacts.ToArray())
            {
                if (artifact.IsPublished)
                {
                    artifact.Delete(artifact.CreatedBy);
                }
                else
                {
                    savedArtifactsList.Add(artifact);
                }
            }
            if (savedArtifactsList.Any())
            {
                Artifact.DiscardArtifacts(savedArtifactsList, _blueprintServer.Address, _user);
            }

            if (_adminStore != null)
            {
                // Delete all the sessions that were created.
                foreach (var session in _adminStore.Sessions.ToArray())
                {
                    _adminStore.DeleteSession(session);
                }
            }

            if (_user != null)
            {
                _user.DeleteUser();
                _user = null;
            }

            if (_user2 != null)
            {
                _user2.DeleteUser();
                _user2 = null;
            }
        }

        #endregion Setup and Cleanup

        [TestRail(107358)]
        [TestCase (BaseArtifactType.Process)]
        [Description("Attempt to get a lock on an artifact that has been published by the same user. Verify that the" +
                     "lock was obtained by the user.")]
        public void GetLockForArtifactWithNoExistingLocks_VerifyLockObtained(BaseArtifactType baseArtifactType)
        {
            var artifact = CreateArtifact(_project, _user, baseArtifactType);
            artifact.Save();

            // Publish artifact to ensure no lock remains on the newly created artifact
            artifact.Publish();

            var lockResultInfo = artifact.Lock(_user);

            // Assert that the lock was successfully obtained
            Assert.AreEqual(lockResultInfo.Result, LockResult.Success, "The user was not able to obtain a lock on the artifact when" +
                                                                       "the artifact was not locked by any user.");

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
            var artifact = CreateArtifact(_project, _user, baseArtifactType);
            artifact.Save();

            // Publish artifact to ensure no lock remains on the newly created artifact
            artifact.Publish();

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
            var artifact = CreateArtifact(_project, _user, baseArtifactType);
            artifact.Save();

            // Publish artifact to ensure no lock remains on the newly created artifact
            artifact.Publish();

            // first user locks the artifact
            artifact.Lock(_user);

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
            var artifact = CreateArtifact(_project, _user, baseArtifactType);
            artifact.Save();

            // Publish artifact to ensure no lock remains on the newly created artifact
            artifact.Publish();

            var artifactsToLock = new List<IArtifactBase> { artifact };

            // first user locks the artifact
            Artifact.LockArtifacts(artifactsToLock, _blueprintServer.Address, _user);

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
            var artifact = ArtifactFactory.CreateArtifact(_project, _user, baseArtifactType);
            artifact.Save();
            artifact.Publish();

            // Update the process to lock it with the user1
            artifact.Lock(_user);

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

            // Remove locks by publishing the artifact for clean up process
            _artifacts.Add(artifact);

        }

        [TestRail(107364)]
        [TestCase(BaseArtifactType.Process)]
        [Description("Verify that the artifact discard doesn't work when the artifact is locked by the other user.")]
        public void DiscardArtifactWhenLockedByOtherUser_VerifyNotDiscarded(BaseArtifactType baseArtifactType)
        {
            // Create an artifact and publish with the user1
            var artifact = ArtifactFactory.CreateArtifact(_project, _user, baseArtifactType);
            artifact.Save();
            artifact.Publish();

            // Update the process to lock it with the user1
            artifact.Lock(_user);

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

            // Remove locks by publishing the artifact for clean up process
            _artifacts.Add(artifact);
        }

        [TestRail(107365)]
        [TestCase(BaseArtifactType.Process, 3)]
        [Description("Verify the lock status perperty values of Locked artifacts")]
        public void GetLocksForMultipleArtifacts_VerifyLocksObtained(BaseArtifactType baseArtifactType, int iteration)
        {
            // Create artifact(s) and publish with the user1
            for (int i = 0; i < iteration; i++)
            {
                var artifact = ArtifactFactory.CreateArtifact(_project, _user, baseArtifactType);
                artifact.Save();
                artifact.Publish();
                _artifacts.Add(artifact);
            }

            List<IArtifactBase> artifactList = _artifacts.ConvertAll(a => (IArtifactBase)a);

            // Obtain locks for artifact(s) with the user1
            Artifact.LockArtifacts(artifactList, artifactList.First().Address, _user);

            // Verify that lock obtained by checking the status section of artifact(s)?
            foreach (var artifact in _artifacts)
            {
                var lockResultInfo = artifact.Lock(_user2);
                Assert.That(lockResultInfo.Result.Equals(LockResult.AlreadyLocked),
                    "The artifact {0} should be in \"AlreadyLocked\" but the result from the get lock call is {1},",
                    artifact.Id, lockResultInfo.Result.ToString());
                Assert.That(lockResultInfo.Info.LockOwnerLogin.Equals(artifact.CreatedBy.Username),
                    "The artifact {0} should be locked by the user {1} but the result from the getl lock call shows " +
                    "the lock owner is {2}.", artifact.Id, artifact.CreatedBy.Username, lockResultInfo.Info.LockOwnerLogin);
            }
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase(BaseArtifactType.Process)]
        [Description("")]
        public void GetLocksForMultipleArtifactsWithOneArtifactLockedByOtherUsed_VerifyLockIsNotObtainedForArtifactLockedByOtherUser()
        {
            throw new NotImplementedException();
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase(BaseArtifactType.Process)]
        [Description("")]
        public void GetLockForArtifactWithAuthorizationAsCookie_VerifyLockObtained()
        {
            throw new NotImplementedException();
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase(BaseArtifactType.Process)]
        [Description("")]
        public void GetLockForVariousArtifactTypes_VerifyLocksObtained()
        {
            throw new NotImplementedException();
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase(BaseArtifactType.Process)]
        [Description("")]
        public void GetLockForArtifactWhenLockAlreadyExistsForUser_VerifyLockObtained()
        {
            throw new NotImplementedException();
        }

        #region Private Methods

        /// <summary>
        /// Create Artifact and Add to Artifact List
        /// </summary>
        /// <param name="project">The project where the artifact will be created</param>
        /// <param name="user">The user creating the artifact</param>
        /// <param name="baseArtifactType">The base type of the artifact being created</param>
        /// <returns>The created artifact</returns>
        private IArtifact CreateArtifact(IProject project, IUser user, BaseArtifactType baseArtifactType )
        {
            var artifact = ArtifactFactory.CreateArtifact(project, user, baseArtifactType);

            // Add artifact to artifacts list for later discard/deletion
            _artifacts.Add(artifact);

            return artifact;
        }

        #endregion Private Methods
    }
}
