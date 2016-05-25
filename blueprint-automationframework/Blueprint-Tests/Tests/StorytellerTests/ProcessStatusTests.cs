using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class ProcessStatusTests
    {
        private IAdminStore _adminStore;
        private IBlueprintServer _blueprintServer;
        private IStoryteller _storyteller;
        private IUser _primaryUser;
        private IUser _secondaryUser;
        private IProject _project;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _blueprintServer = BlueprintServerFactory.GetBlueprintServerFromTestConfig();
            _storyteller = StorytellerFactory.GetStorytellerFromTestConfig();
            _primaryUser = UserFactory.CreateUserAndAddToDatabase();
            _secondaryUser = UserFactory.CreateUserAndAddToDatabase();
            _project = ProjectFactory.GetProject(_primaryUser, shouldRetrievePropertyTypes: true);

            // Get a valid Access Control token for the user (for the new Storyteller REST calls).
            _adminStore.AddSession(_primaryUser);
            Assert.IsFalse(string.IsNullOrWhiteSpace(_primaryUser.Token.AccessControlToken),
                "The primary user didn't get an Access Control token!");

            // Get a valid OpenApi token for the user (for the OpenApi artifact REST calls).
            _blueprintServer.LoginUsingBasicAuthorization(_primaryUser, string.Empty);
            Assert.IsFalse(string.IsNullOrWhiteSpace(_primaryUser.Token.OpenApiToken),
                "The primary user didn't get an OpenApi token!");

            // Get a valid Access Control token for the user (for the new Storyteller REST calls).
            _adminStore.AddSession(_secondaryUser);
            Assert.IsFalse(string.IsNullOrWhiteSpace(_secondaryUser.Token.AccessControlToken),
                "The secondary user didn't get an Access Control token!");

            // Get a valid OpenApi token for the user (for the OpenApi artifact REST calls).
            _blueprintServer.LoginUsingBasicAuthorization(_secondaryUser, string.Empty);
            Assert.IsFalse(string.IsNullOrWhiteSpace(_secondaryUser.Token.OpenApiToken),
                "The secondary user didn't get an OpenApi token!");
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            if (_adminStore != null)
            {
                // Delete all the sessions that were created.
                foreach (var session in _adminStore.Sessions.ToArray())
                {
                    _adminStore.DeleteSession(session);
                }
            }

            if (_primaryUser != null)
            {
                _primaryUser.DeleteUser();
                _primaryUser = null;
            }

            if (_secondaryUser != null)
            {
                _secondaryUser.DeleteUser();
                _secondaryUser = null;
            }
        }

        [TearDown]
        public void Teardown()
        {
            if (_storyteller.Artifacts != null)
            {
                // Delete or Discard all the artifacts that were added.
                var savedArtifactsListPrimaryUser = new List<IArtifactBase>();
                var savedArtifactsListSecondaryUser = new List<IArtifactBase>();
                foreach (var artifact in _storyteller.Artifacts.ToArray())
                {
                    if (artifact.IsPublished)
                    {
                        _storyteller.DeleteProcessArtifact(artifact, deleteChildren: true);
                    }

                    if (!artifact.IsPublished && artifact.CreatedBy.Equals(_primaryUser))
                    {
                        savedArtifactsListPrimaryUser.Add(artifact);
                    }

                    if (!artifact.IsPublished && artifact.CreatedBy.Equals(_secondaryUser))
                    {
                        savedArtifactsListSecondaryUser.Add(artifact);
                    }
                }

                if (savedArtifactsListPrimaryUser.Any())
                {
                    Artifact.DiscardArtifacts(savedArtifactsListPrimaryUser, _blueprintServer.Address, _primaryUser);
                }

                if (savedArtifactsListSecondaryUser.Any())
                {
                    Artifact.DiscardArtifacts(savedArtifactsListSecondaryUser, _blueprintServer.Address, _secondaryUser);
                }

                // Clear all possible List Items
                savedArtifactsListPrimaryUser.Clear();
                savedArtifactsListSecondaryUser.Clear();
                _storyteller.Artifacts.Clear();
            }
        }

        #endregion Setup and Cleanup

        #region Tests

        [TestRail(107383)]
        [TestCase]
        [Description("Lock an existing process by updating with the second user. Verify that the" +
             "status of the process model obtained by the first user.")]
        public void LockArtifactByUpdatingWithSecondUser_VerifyTheReturnedProcessStatusWithFirstUser()
        {
            // Create and save the process artifact with the second user 
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _secondaryUser);

            // Process Status flags: IsLocked, IsLockedByMe, IsDeleted, IsReadOnly, IsUnpublished, HasEverBeenPublished

            // Expected Status: true, true, false, false, true, fals
            var expectedProcessStatus = new ProcessStatus(
                isLocked: true, isLockedByMe: true, isDeleted: false,
                isReadOnly: false, isUnpublished: true,
                hasEverBeenPublished: false);

            VerifyStatus(process, expectedProcessStatus);

            // Publish the saved process
            _storyteller.PublishProcess(_secondaryUser, process);

            // Get the process Artifact after publish
            process = _storyteller.GetProcess(_secondaryUser, process.Id);

            // Process Status flags: IsLocked, IsLockedByMe, IsDeleted, IsReadOnly, IsUnpublished, HasEverBeenPublished

            // Expected Status: false, false, false, false, false, true 
            expectedProcessStatus = new ProcessStatus(
                isLocked: false, isLockedByMe: false, isDeleted: false,
                isReadOnly: false, isUnpublished: false,
                hasEverBeenPublished: true);

            VerifyStatus(process, expectedProcessStatus);

            // Find precondition task
            var preconditionTask = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);
            
            // Find outgoing process link for precondition task
            var preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);
            Assert.IsNotNull(preconditionOutgoingLink, "Outgoing link for the default precondition was not found.");
            
            // Add user/system Task immediately after the precondition
            process.AddUserAndSystemTask(preconditionOutgoingLink);

            // Update the process to lock by the second user
            process = _storyteller.UpdateProcess(_secondaryUser, process);

            // Process Status flags: IsLocked, IsLockedByMe, IsDeleted, IsReadOnly, IsUnpublished, HasEverBeenPublished

            // Expected Status: true, true, false, false, true, true
            expectedProcessStatus = new ProcessStatus(
                isLocked: true, isLockedByMe: true, isDeleted: false,
                isReadOnly: false, isUnpublished: true,
                hasEverBeenPublished: true);

            VerifyStatus(process, expectedProcessStatus);

            // Get the process with the first user
            process = _storyteller.GetProcess(_primaryUser, process.Id);

            // Process Status flags: IsLocked, IsLockedByMe, IsDeleted, IsReadOnly, IsUnpublished, HasEverBeenPublished

            // Expected Status: true, false, false, true, false, true
            expectedProcessStatus = new ProcessStatus(
                isLocked: true, isLockedByMe: false, isDeleted: false,
                isReadOnly: true, isUnpublished: false,
                hasEverBeenPublished: true);

            VerifyStatus(process, expectedProcessStatus);
        }

        [TestRail(107384)]
        [TestCase]
        [Description("Lock an existing process by deleting with the second user. Verify that the" +
            "status of the process model obtained by the first user.")]
        public void LockArtifactByDeletingWithSecondUser_VerifyTheReturnedProcessStatusWithFirstUser()
        {
            // Create and save the process artifact with the second user 
            var processArtifact = _storyteller.CreateAndSaveProcessArtifact(_project, BaseArtifactType.Process, _secondaryUser);
            var process = _storyteller.GetProcess(_secondaryUser, processArtifact.Id);

            // Process Status flags: IsLocked, IsLockedByMe, IsDeleted, IsReadOnly, IsUnpublished, HasEverBeenPublished

            // Expected Status: true, true, false, false, true, false 
            var expectedProcessStatus = new ProcessStatus(
                isLocked: true, isLockedByMe: true, isDeleted: false,
                isReadOnly: false, isUnpublished: true,
                hasEverBeenPublished: false);

            VerifyStatus(process, expectedProcessStatus);

            // Publish the saved process
            _storyteller.PublishProcess(_secondaryUser, process);

            // Get the process Artifact after publish
            process = _storyteller.GetProcess(_secondaryUser, process.Id);

            // Process Status flags: IsLocked, IsLockedByMe, IsDeleted, IsReadOnly, IsUnpublished, HasEverBeenPublished

            // Expected Status: false, false, false, false, false, true
            expectedProcessStatus = new ProcessStatus(
                isLocked: false, isLockedByMe: false, isDeleted: false,
                isReadOnly: false, isUnpublished: false,
                hasEverBeenPublished: true);

            VerifyStatus(process, expectedProcessStatus);

            // Delete the process to lock by the second user
            _storyteller.DeleteProcessArtifact(processArtifact);

            // Get the process with the first user
            process = _storyteller.GetProcess(_primaryUser, process.Id);

            // Process Status flags: IsLocked, IsLockedByMe, IsDeleted, IsReadOnly, IsUnpublished, HasEverBeenPublished

            // Expected Status: true, false, false, true, false, true
            expectedProcessStatus = new ProcessStatus(
                isLocked: true, isLockedByMe: false, isDeleted: false,
                isReadOnly: true, isUnpublished: false,
                hasEverBeenPublished: true);

            VerifyStatus(process, expectedProcessStatus);
        }

        #endregion Tests

        /// <summary>
        /// Verify process status by checking the status boolean parameters from the process model
        /// </summary>
        /// <param name="retrievedProcess">The process model retrieved from the server side</param>
        /// <param name="expectedStatus">The list of boolean parameters that represents expected status of the returned process</param>
        public static void VerifyStatus(IProcess retrievedProcess, ProcessStatus expectedStatus)
        {
            ThrowIf.ArgumentNull(retrievedProcess, nameof(retrievedProcess));

            ThrowIf.ArgumentNull(expectedStatus, nameof(expectedStatus));

            var retrivedProcessStatus = retrievedProcess.Status;

            Assert.That(retrivedProcessStatus.IsLocked.Equals(expectedStatus.IsLocked),
                "IsLocked from the process model is {0} and {1} is expected.",
                retrivedProcessStatus.IsLocked, !retrivedProcessStatus.IsLocked);

            Assert.That(retrivedProcessStatus.IsLockedByMe.Equals(expectedStatus.IsLockedByMe),
                "IsLockedByMe from the process model is {0} and {1} is expected.",
                retrivedProcessStatus.IsLockedByMe, !retrivedProcessStatus.IsLockedByMe);

            Assert.That(retrivedProcessStatus.IsDeleted.Equals(expectedStatus.IsDeleted),
                "IsDeleted from the process model is {0} and {1} is expected.",
                retrivedProcessStatus.IsDeleted, !retrivedProcessStatus.IsDeleted);

            Assert.That(retrivedProcessStatus.IsReadOnly.Equals(expectedStatus.IsReadOnly),
                "IsReadOnly from the process model is {0} and {1} is expected.",
                retrivedProcessStatus.IsReadOnly, !retrivedProcessStatus.IsReadOnly);

            Assert.That(retrivedProcessStatus.IsUnpublished.Equals(expectedStatus.IsUnpublished),
                "IsUnpublished from the process model is {0} and {1} is expected.",
                retrivedProcessStatus.IsUnpublished, !retrivedProcessStatus.IsUnpublished);

            Assert.That(retrivedProcessStatus.HasEverBeenPublished.Equals(expectedStatus.HasEverBeenPublished),
                "HasEverBeenPublished from the process model is {0} and {1} is expected.",
                retrivedProcessStatus.HasEverBeenPublished, !retrivedProcessStatus.HasEverBeenPublished);
        }
    }
}
