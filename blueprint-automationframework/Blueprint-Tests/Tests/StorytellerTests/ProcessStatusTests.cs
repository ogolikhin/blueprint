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
    [Explicit(IgnoreReasons.UnderDevelopment)]
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class ProcessStatusTests
    {
        // TODO This will need to be updated with the value that cannot does not exist in the system 
        //Non-existence artifact Id sample
        private const int NONEXISTENT_ARTIFACT_ID = 99999999;
        //Invalid process artifact Id sample
        private const int INVALID_ID = -33;

        private IAdminStore _adminStore;
        private IBlueprintServer _blueprintServer;
        private IStoryteller _storyteller;
        private IUser _primaryUser;
        private IUser _secondaryUser;
        private IProject _project;
        private IList<int> _invalidList;

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

            _invalidList = new List<int>() { NONEXISTENT_ARTIFACT_ID, INVALID_ID }.AsReadOnly();

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
                    if (!_invalidList.Contains(artifact.Id) && artifact.IsPublished)
                    {
                        _storyteller.DeleteProcessArtifact(artifact, deleteChildren: true);
                    }

                    if (!_invalidList.Contains(artifact.Id) && !artifact.IsPublished && artifact.CreatedBy.Equals(_primaryUser))
                    {
                        savedArtifactsListPrimaryUser.Add(artifact);
                    }

                    if (!_invalidList.Contains(artifact.Id) && !artifact.IsPublished && artifact.CreatedBy.Equals(_secondaryUser))
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

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase]
        [Description("Lock an existing process by updating with the second user. Verify that the" +
             "status of the process model obtained by the first user.")]
        public void LockArtifactByUpdatingWithSecondUser_VerifyTheReturnedProcessStatusWithFirstUser()
        {
            // Create and save the process artifact with the second user 
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _secondaryUser);

            // Process Status flags: IsLocked, IsLockedByMe, IsDeleted, IsReadOnly, IsUnpublished, HasEverBeenPublished

            // Expected Status: true, true, false, false, true, fals
            var expectedProcessStatuses = new Queue<bool>(new List<bool> { true, true, false, false, true, false });
            VerifyStatus(process, expectedProcessStatuses);

            // Publish the saved process
            _storyteller.PublishProcess(_secondaryUser, process);

            // Get the process Artifact after publish
            process = _storyteller.GetProcess(_secondaryUser, process.Id);

            // Process Status flags: IsLocked, IsLockedByMe, IsDeleted, IsReadOnly, IsUnpublished, HasEverBeenPublished

            // Expected Status: false, false, false, false, false, true 
            expectedProcessStatuses = new Queue<bool>(new List<bool> { false, false, false, false, false, true });
            VerifyStatus(process, expectedProcessStatuses);

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
            expectedProcessStatuses = new Queue<bool>(new List<bool> { true, true, false, false, true, true });
            VerifyStatus(process, expectedProcessStatuses);

            // Get the process with the first user
            process = _storyteller.GetProcess(_primaryUser, process.Id);
            
            // Process Status flags: IsLocked, IsLockedByMe, IsDeleted, IsReadOnly, IsUnpublished, HasEverBeenPublished
            
            // Expected Status: true, false, false, true, false, true 
            expectedProcessStatuses = new Queue<bool>(new List<bool> { true, false, false, true, false, true });
            VerifyStatus(process, expectedProcessStatuses);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
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
            var expectedProcessStatuses = new Queue<bool>(new List<bool> { true, true, false, false, true, false });
            VerifyStatus(process, expectedProcessStatuses);

            // Publish the saved process
            _storyteller.PublishProcess(_secondaryUser, process);

            // Get the process Artifact after publish
            process = _storyteller.GetProcess(_secondaryUser, process.Id);

            // Process Status flags: IsLocked, IsLockedByMe, IsDeleted, IsReadOnly, IsUnpublished, HasEverBeenPublished

            // Expected Status: false, false, false, false, false, true
            expectedProcessStatuses = new Queue<bool>(new List<bool> { false, false, false, false, false, true });
            VerifyStatus(process, expectedProcessStatuses);

            // Delete the process to lock by the second user
            _storyteller.DeleteProcessArtifact(processArtifact);

            // Get the process with the first user
            process = _storyteller.GetProcess(_primaryUser, process.Id);

            // Process Status flags: IsLocked, IsLockedByMe, IsDeleted, IsReadOnly, IsUnpublished, HasEverBeenPublished

            // Expected Status: true, false, false, true, false, true
            expectedProcessStatuses = new Queue<bool>(new List<bool> { true, false, false, true, false, true });
            VerifyStatus(process, expectedProcessStatuses);
        }

        #endregion Tests

        /// <summary>
        /// Verify process status by checking the status boolean parameters from the process model
        /// </summary>
        /// <param name="retrievedProcess">The process model retrieved from the server side</param>
        /// <param name="expectedStatuses">The list of boolean parameters that represents expected status of the returned process</param>
        public static void VerifyStatus(IProcess retrievedProcess, Queue<bool> expectedStatuses)
        {
            ThrowIf.ArgumentNull(retrievedProcess, nameof(retrievedProcess));

            ThrowIf.ArgumentNull(expectedStatuses, nameof(expectedStatuses));

            var retrivedProcessStatus = retrievedProcess.Status;

            Assert.That(retrivedProcessStatus.IsLocked.Equals(expectedStatuses.Dequeue()),
                "IsLocked from the process model is {0} and {1} is expected.",
                retrivedProcessStatus.IsLocked, !retrivedProcessStatus.IsLocked);

            Assert.That(retrivedProcessStatus.IsLockedByMe.Equals(expectedStatuses.Dequeue()),
                "IsLockedByMe from the process model is {0} and {1} is expected.",
                retrivedProcessStatus.IsLockedByMe, !retrivedProcessStatus.IsLockedByMe);

            Assert.That(retrivedProcessStatus.IsDeleted.Equals(expectedStatuses.Dequeue()),
                "IsDeleted from the process model is {0} and {1} is expected.",
                retrivedProcessStatus.IsDeleted, !retrivedProcessStatus.IsDeleted);

            Assert.That(retrivedProcessStatus.IsReadOnly.Equals(expectedStatuses.Dequeue()),
                "IsReadOnly from the process model is {0} and {1} is expected.",
                retrivedProcessStatus.IsReadOnly, !retrivedProcessStatus.IsReadOnly);

            Assert.That(retrivedProcessStatus.IsUnpublished.Equals(expectedStatuses.Dequeue()),
                "IsUnpublished from the process model is {0} and {1} is expected.",
                retrivedProcessStatus.IsUnpublished, !retrivedProcessStatus.IsUnpublished);

            Assert.That(retrivedProcessStatus.HasEverBeenPublished.Equals(expectedStatuses.Dequeue()),
                "HasEverBeenPublished from the process model is {0} and {1} is expected.",
                retrivedProcessStatus.HasEverBeenPublished, !retrivedProcessStatus.HasEverBeenPublished);
        }
    }
}
