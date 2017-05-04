using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using Model.StorytellerModel.Enums;
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

        [Explicit(IgnoreReasons.UnderDevelopmentDev)] // Need more update information from commit 7f940997 https://github.com/BlueprintSys/blueprint-current/commit/7f940997f33d1cc6706712e7c385fe39004fc8ae
        [TestRail(107383)]
        [TestCase(ProcessStatusState.NeverPublishedAndUpdated)]
        [TestCase(ProcessStatusState.PublishedAndNotLocked)]
        [TestCase(ProcessStatusState.PublishedAndLockedByAnotherUser)]
        [Description("Lock an existing process by updating with the second user. Verify that the" +
             "status of the process model obtained by the first user.")]
        public void ProcessStatus_LockArtifactByUpdatingWithSecondUser_VerifyProcessStatusWithFirstUser(
            ProcessStatusState processStatusState)
        {
            // Setup:
            // Create and save the process artifact with the second user 
            var novaProcess = Helper.Storyteller.CreateAndSaveNovaProcessArtifact(_project, _secondaryUser);

            // Execute and Verify: 
            SetAndValidateProcessStatus(Helper, novaProcess, processStatusState, _primaryUser, _secondaryUser);
        }

        [Explicit(IgnoreReasons.UnderDevelopmentDev)] // Need more update information from commit 7f940997 https://github.com/BlueprintSys/blueprint-current/commit/7f940997f33d1cc6706712e7c385fe39004fc8ae
        [TestRail(107384)]
        [TestCase]
        [Description("Lock an existing process by deleting with the second user. Verify that the" +
            "status of the process model obtained by the first user.")]
        public void ProcessStatus_LockArtifactByDeletingWithSecondUser_VerifyProcessStatusWithFirstUser()
        {
            // Setup:
            // Create and save the process artifact with the second user 
            var novaProcess = Helper.Storyteller.CreateAndPublishNovaProcessArtifact(_project, _secondaryUser);

            // Execute:
            // Delete the published process with the second user
            Helper.Storyteller.DeleteNovaProcessArtifact(_secondaryUser, novaProcess);

            // Verify:
            // Get the process with the first user
            novaProcess = Helper.Storyteller.GetNovaProcess(_primaryUser, novaProcess.Id);
            var stProcess = Helper.Storyteller.GetProcess(_primaryUser, novaProcess.Id);
            StorytellerTestHelper.VerifyProcessStatus(ProcessStatusState.PublishedAndLockedByAnotherUser, stProcess);
        }

        #endregion Tests

        #region private functions

        /// TODO: Need more update information from commit 7f940997 https://github.com/BlueprintSys/blueprint-current/commit/7f940997f33d1cc6706712e7c385fe39004fc8ae
        /// <summary>
        /// Set and validate Process Status at the event of process update
        /// </summary>
        /// <param name="helper">the test helper</param>
        /// <param name="novaProcess">the nova process to update</param>
        /// <param name="processStatusState">process status state, (currently only returned by Storyteller 1 shared component API call)</param>
        /// <param name="user1">the primary user</param>
        /// <param name="user2">the secondary user</param>
        /// <returns>updated nova process</returns>
        private static INovaProcess SetAndValidateProcessStatus(
            TestHelper helper,
            INovaProcess novaProcess,
            ProcessStatusState processStatusState,
            IUser user1,
            IUser user2)
        {
            ThrowIf.ArgumentNull(helper, nameof(helper));
            ThrowIf.ArgumentNull(novaProcess, nameof(novaProcess));
            ThrowIf.ArgumentNull(user1, nameof(user1));
            ThrowIf.ArgumentNull(user2, nameof(user2));

            var stProcessArtifact = helper.Storyteller.GetProcess(user2, novaProcess.Id);

            if (processStatusState != ProcessStatusState.NeverPublishedAndUpdated)
            {
                StorytellerTestHelper.VerifyProcessStatus(ProcessStatusState.NeverPublishedAndUpdated, stProcessArtifact);

                if (processStatusState != ProcessStatusState.PublishedAndNotLocked)
                {
                    // Publish the saved process
                    helper.Storyteller.PublishNovaProcess(user2, novaProcess);

                    // Get the process Artifact after publish
                    novaProcess = helper.Storyteller.GetNovaProcess(user2, novaProcess.Id);

                    stProcessArtifact = helper.Storyteller.GetProcess(user2, novaProcess.Id);

                    StorytellerTestHelper.VerifyProcessStatus(ProcessStatusState.PublishedAndNotLocked, stProcessArtifact);

                    if (processStatusState != ProcessStatusState.PublishedAndLockedByMe)
                    {
                        // Find precondition task
                        var preconditionTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

                        // Find outgoing process link for precondition task
                        var preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);
                        Assert.IsNotNull(preconditionOutgoingLink, "Outgoing link for the default precondition was not found.");

                        // Add user/system Task immediately after the precondition
                        novaProcess.Process.AddUserAndSystemTask(preconditionOutgoingLink);

                        // Update the process to lock by the second user
                        novaProcess = helper.Storyteller.UpdateNovaProcess(user2, novaProcess);

                        stProcessArtifact = helper.Storyteller.GetProcess(user2, novaProcess.Id);

                        StorytellerTestHelper.VerifyProcessStatus(ProcessStatusState.PublishedAndLockedByMe, stProcessArtifact);

                        if (processStatusState != ProcessStatusState.PublishedAndLockedByAnotherUser)
                        {
                            // Get the process with the first user
                            novaProcess = helper.Storyteller.GetNovaProcess(user1, novaProcess.Id);

                            stProcessArtifact = helper.Storyteller.GetProcess(user1, novaProcess.Id);

                            StorytellerTestHelper.VerifyProcessStatus(ProcessStatusState.PublishedAndLockedByAnotherUser, stProcessArtifact);
                        }
                    }
                }
            }

            return novaProcess;
        }

        #endregion private functions

    }
}
