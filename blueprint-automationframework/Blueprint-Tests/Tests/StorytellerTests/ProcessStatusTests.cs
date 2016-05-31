using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class ProcessStatusTests : TestBase
    {
        private IUser _primaryUser;
        private IUser _secondaryUser;
        private IProject _project;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();
            _primaryUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _secondaryUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_primaryUser);
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            Helper?.Dispose();
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
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _secondaryUser);

            var expectedProcessStatus = new ProcessStatus(
                isLocked: true, isLockedByMe: true, isDeleted: false,
                isReadOnly: false, isUnpublished: true,
                hasEverBeenPublished: false);

            VerifyStatus(process, expectedProcessStatus);

            // Publish the saved process
            Helper.Storyteller.PublishProcess(_secondaryUser, process);

            // Get the process Artifact after publish
            process = Helper.Storyteller.GetProcess(_secondaryUser, process.Id);

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
            process = Helper.Storyteller.UpdateProcess(_secondaryUser, process);

            expectedProcessStatus = new ProcessStatus(
                isLocked: true, isLockedByMe: true, isDeleted: false,
                isReadOnly: false, isUnpublished: true,
                hasEverBeenPublished: true);

            VerifyStatus(process, expectedProcessStatus);

            // Get the process with the first user
            process = Helper.Storyteller.GetProcess(_primaryUser, process.Id);

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
            var processArtifact = Helper.Storyteller.CreateAndSaveProcessArtifact(_project, BaseArtifactType.Process, _secondaryUser);
            var process = Helper.Storyteller.GetProcess(_secondaryUser, processArtifact.Id);

            var expectedProcessStatus = new ProcessStatus(
                isLocked: true, isLockedByMe: true, isDeleted: false,
                isReadOnly: false, isUnpublished: true,
                hasEverBeenPublished: false);

            VerifyStatus(process, expectedProcessStatus);

            // Publish the saved process
            Helper.Storyteller.PublishProcess(_secondaryUser, process);

            // Get the process Artifact after publish
            process = Helper.Storyteller.GetProcess(_secondaryUser, process.Id);

            expectedProcessStatus = new ProcessStatus(
                isLocked: false, isLockedByMe: false, isDeleted: false,
                isReadOnly: false, isUnpublished: false,
                hasEverBeenPublished: true);

            VerifyStatus(process, expectedProcessStatus);

            // Delete the process to lock by the second user
            Helper.Storyteller.DeleteProcessArtifact(processArtifact);

            // Get the process with the first user
            process = Helper.Storyteller.GetProcess(_primaryUser, process.Id);

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
                "IsLocked from the process model is {0} but {1} is expected.",
                retrivedProcessStatus.IsLocked, expectedStatus.IsLocked);

            Assert.That(retrivedProcessStatus.IsLockedByMe.Equals(expectedStatus.IsLockedByMe),
                "IsLockedByMe from the process model is {0} but {1} is expected.",
                retrivedProcessStatus.IsLockedByMe, expectedStatus.IsLockedByMe);

            Assert.That(retrivedProcessStatus.IsDeleted.Equals(expectedStatus.IsDeleted),
                "IsDeleted from the process model is {0} but {1} is expected.",
                retrivedProcessStatus.IsDeleted, expectedStatus.IsDeleted);

            Assert.That(retrivedProcessStatus.IsReadOnly.Equals(expectedStatus.IsReadOnly),
                "IsReadOnly from the process model is {0} but {1} is expected.",
                retrivedProcessStatus.IsReadOnly, expectedStatus.IsReadOnly);

            Assert.That(retrivedProcessStatus.IsUnpublished.Equals(expectedStatus.IsUnpublished),
                "IsUnpublished from the process model is {0} but {1} is expected.",
                retrivedProcessStatus.IsUnpublished, expectedStatus.IsUnpublished);

            Assert.That(retrivedProcessStatus.HasEverBeenPublished.Equals(expectedStatus.HasEverBeenPublished),
                "HasEverBeenPublished from the process model is {0} but {1} is expected.",
                retrivedProcessStatus.HasEverBeenPublished, expectedStatus.HasEverBeenPublished);
        }
    }
}
