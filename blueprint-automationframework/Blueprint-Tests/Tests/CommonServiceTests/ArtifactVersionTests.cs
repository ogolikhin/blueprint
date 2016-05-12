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

namespace CommonServiceTests
{
    [TestFixture]
    [Category(Categories.ArtifactVersion)]
    [Explicit(IgnoreReasons.UnderDevelopment)]
    public class ArtifactVersionTests
    {
        private IAdminStore _adminStore;
        private IBlueprintServer _blueprintServer;
        private IUser _user;
        private IUser _user2;
        private IProject _project;


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
            //if (_artifactVersion. != null)
            //{
            //    // Delete or Discard all the artifacts that were added.
            //    var savedArtifactsList = new List<IOpenApiArtifact>();
            //    foreach (var artifact in _storyteller.Artifacts.ToArray())
            //    {
            //        if (artifact.IsPublished)
            //        {
            //            _storyteller.DeleteProcessArtifact(artifact, deleteChildren: true);
            //        }
            //        else
            //        {
            //            savedArtifactsList.Add(artifact);
            //        }
            //    }
            //    if (savedArtifactsList.Any())
            //    {
            //        Storyteller.DiscardProcessArtifacts(savedArtifactsList, _blueprintServer.Address, _user);
            //    }
            //}

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
        }

        #endregion Setup and Cleanup

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase (BaseArtifactType.Process)]
        [Description("")]
        public void GetLockForArtifactWithNoExistingLocks_VerifyLockObtained(BaseArtifactType baseArtifactType)
        {
            var artifact = ArtifactFactory.CreateArtifact(_project, _user, baseArtifactType);
            artifact.Save();

            // Publish artifact to ensure no lock remains on the newly created artifact
            artifact.Publish();

            var artifactsToLock = new List<IArtifactBase> { artifact };

            // User locks the artifact
            var lockResults = Artifact.LockArtifacts(artifactsToLock, _blueprintServer.Address, _user);
            var lockResultInfo = lockResults.First();

            // Assert that the lock was successfully obtained
            Assert.AreEqual(lockResultInfo.Result, LockResult.Success, "The user was not able to obtain a lock on the artifact when" +
                                                                       "the artifact was not locked by any user.");

            // Assert that user can Publish the artifact to verify that the lock was actually obtained
            Assert.DoesNotThrow(() =>
                artifact.Publish(),
                "The user was unable to Publish the artifact even though the lock appears to have been obtained successfully."
                );
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase(BaseArtifactType.Process)]
        [Description("")]
        public void GetLockForArtifactPublishedByOtherUser_VerifyLockObtained(BaseArtifactType baseArtifactType)
        {
            var artifact = ArtifactFactory.CreateArtifact(_project, _user, baseArtifactType);
            artifact.Save();

            // Publish artifact to ensure no lock remains on the newly created artifact
            artifact.Publish();

            var artifactsToLock = new List<IArtifactBase> { artifact };

            // Second user locks the artifact
            var lockResults = Artifact.LockArtifacts(artifactsToLock, _blueprintServer.Address, _user2);
            var lockResultInfo = lockResults.First();

            // Assert that the lock was successfully obtained
            Assert.AreEqual(lockResultInfo.Result, LockResult.Success, "The second user was not able to obtain a lock on the artifact that" +
                                                                       "was published by the first user.");

            // Assert that the second user can Publish the artifact to verify that the lock was actually obtained
            Assert.DoesNotThrow(() =>
                artifact.Publish(),
                "The second user was unable to Publish the artifact even though the lock appears to have been obtained successfully."
                );
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase(BaseArtifactType.Process)]
        [Description("")]
        public void GetLockForArtifactSavedByOtherUser_VerifyNoLockObtained(BaseArtifactType baseArtifactType)
        {
            var artifact = ArtifactFactory.CreateArtifact(_project, _user, baseArtifactType);
            artifact.Save();
            
            // Publish artifact to be sure it is available to the second user
            artifact.Publish();

            // Re-save the process to ensure the lock is obtained by the first user (lock obtained via the Save
            // call itself, not the lock artifact call.
            artifact.Save();

            var artifactsToLock = new List<IArtifactBase> { artifact };


            Assert.Throws<Http409ConflictException>(
                () =>
                    // Second user tries to lock the artifact
                    Artifact.LockArtifacts(artifactsToLock, _blueprintServer.Address, _user2)
                );


            //var deserializedResponse = Deserialization.DeserializeObject<ProcessValidationResponse>(ex.RestResponse.Content);

            //var expectedValidationResponseContent = I18NHelper.FormatInvariant(
            //        ProcessValidationResponse.ArtifactAlreadyLocked,
            //        process.Id,
            //        process.Name,
            //        _user2.Username);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase(BaseArtifactType.Process)]
        [Description("")]
        public void SaveArtifactWhenLockedByOtherUser_VerifyNotSaved()
        {
            throw new NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase(BaseArtifactType.Process)]
        [Description("")]
        public void PublishArtifactWhenLockedByOtherUser_VerifyNotPublished()
        {
            throw new NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase(BaseArtifactType.Process)]
        [Description("")]
        public void DeleteArtifactWhenLockedByOtherUser_VerifyNotDeleted()
        {
            throw new NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase(BaseArtifactType.Process)]
        [Description("")]
        public void DiscardArtifactWhenLockedByOtherUser_VerifyNotDiscarded()
        {
            throw new NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase(BaseArtifactType.Process)]
        [Description("")]
        public void GetLocksForMultipleArtifacts_VerifyLocksObtained()
        {
            throw new NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase(BaseArtifactType.Process)]
        [Description("")]
        public void GetLocksForMultipleArtifactsWithOneArtifactLockedByOtherUsed_VerifyLockIsNotObtainedForArtifactLockedByOtherUser()
        {
            throw new NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase(BaseArtifactType.Process)]
        [Description("")]
        public void GetLockForArtifactWithAuthorizationAsCookie_VerifyLockObtained()
        {
            throw new NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase(BaseArtifactType.Process)]
        [Description("")]
        public void GetLockForVariousArtifactTypes_VerifyLocksObtained()
        {
            throw new NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase(BaseArtifactType.Process)]
        [Description("")]
        public void GetLockForArtifactWhenLockAlreadyExistsForUser_VerifyLockObtained()
        {
            throw new NotImplementedException();
        }
    }
}
