using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.StorytellerModel.Enums;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using TestCommon;

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
            var processArtifact = Helper.Storyteller.CreateAndSaveProcessArtifact(_project, _secondaryUser);
            var process = Helper.Storyteller.GetProcess(_secondaryUser, processArtifact.Id);

            var expectedProcessStatus = ProcessStatusState.NeverPublishedAndUpdated;

            StorytellerTestHelper.VerifyProcessStatus(process, expectedProcessStatus);

            // Publish the saved process
            Helper.Storyteller.PublishProcess(_secondaryUser, process);

            // Get the process Artifact after publish
            process = Helper.Storyteller.GetProcess(_secondaryUser, process.Id);

            expectedProcessStatus = ProcessStatusState.PublishedAndNotLocked;

            StorytellerTestHelper.VerifyProcessStatus(process, expectedProcessStatus);

            // Find precondition task
            var preconditionTask = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);
            
            // Find outgoing process link for precondition task
            var preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);
            Assert.IsNotNull(preconditionOutgoingLink, "Outgoing link for the default precondition was not found.");
            
            // Add user/system Task immediately after the precondition
            process.AddUserAndSystemTask(preconditionOutgoingLink);

            // Update the process to lock by the second user
            Helper.Storyteller.UpdateProcess(_secondaryUser, process);
            process = Helper.Storyteller.GetProcess(_secondaryUser, process.Id);

            expectedProcessStatus = ProcessStatusState.PublishedAndLockedByMe;

            StorytellerTestHelper.VerifyProcessStatus(process, expectedProcessStatus);

            // Get the process with the first user
            process = Helper.Storyteller.GetProcess(_primaryUser, process.Id);

            expectedProcessStatus = ProcessStatusState.PublishedAndLockedByAnotherUser;

            StorytellerTestHelper.VerifyProcessStatus(process, expectedProcessStatus);
        }

        [TestRail(107384)]
        [TestCase]
        [Description("Lock an existing process by deleting with the second user. Verify that the" +
            "status of the process model obtained by the first user.")]
        public void LockArtifactByDeletingWithSecondUser_VerifyTheReturnedProcessStatusWithFirstUser()
        {
            // Create and save the process artifact with the second user 
            var processArtifact = Helper.Storyteller.CreateAndSaveProcessArtifact(_project, _secondaryUser);
            var process = Helper.Storyteller.GetProcess(_secondaryUser, processArtifact.Id);

            var expectedProcessStatus = ProcessStatusState.NeverPublishedAndUpdated;

            StorytellerTestHelper.VerifyProcessStatus(process, expectedProcessStatus);

            // Publish the saved process
            Helper.Storyteller.PublishProcess(_secondaryUser, process);

            // Get the process Artifact after publish
            process = Helper.Storyteller.GetProcess(_secondaryUser, process.Id);

            expectedProcessStatus = ProcessStatusState.PublishedAndNotLocked;

            StorytellerTestHelper.VerifyProcessStatus(process, expectedProcessStatus);

            // Delete the process to lock by the second user
            Helper.Storyteller.DeleteProcessArtifact(processArtifact);

            // Get the process with the first user
            process = Helper.Storyteller.GetProcess(_primaryUser, process.Id);

            expectedProcessStatus = ProcessStatusState.PublishedAndLockedByAnotherUser;

            StorytellerTestHelper.VerifyProcessStatus(process, expectedProcessStatus);
        }

        #endregion Tests

    }
}
