using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.StorytellerModel.Enums;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using TestCommon;
using Utilities.Factories;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class BranchLabelTests : TestBase
    {
        private IUser _adminUser;
        private IProject _project;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_adminUser);
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        [TestCase(1, 0.0)]
        [TestCase(5, 0.0)]
        [TestCase(30, 0.0)]
        [TestCase(1, 1.0)]
        [TestCase(5, 1.0)]
        [TestCase(30, 1.0)]
        [Description("Add a randomized user decision branch label for a specific branch and verify that the label is returned " +
                     "after saving the process.")]
        public void AddUserDecisionBranchWithPlainTextLabelOfVaryingLength_VerifyReturnedBranchLabel(
            int lengthOfLabelSent,
            double orderIndexOfUserDecisionBranch)
        {
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcessWithOneUserDecision(
                _project,
                _adminUser);

            // Get precondition shape in process
            var precondition = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Get user decision shape in process
            var userDecision = novaProcess.Process.GetNextShape(precondition);

            // Get link for specified branch by order index
            var branchLink = novaProcess.Process.GetOutgoingLinkForShape(userDecision, orderIndexOfUserDecisionBranch);

            // Set a randomized link label for the specified branch
            branchLink.Label = RandomGenerator.RandomAlphaNumericUpperAndLowerCase((uint)lengthOfLabelSent);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _adminUser);
        }

        [TestCase(5, 0.0, ProcessType.BusinessProcess)]
        [TestCase(5, 1.0, ProcessType.UserToSystemProcess)]
        [Description("Add a randomized user decision label for different process types and verify the returned label" +
                     "is returned after saving the process.")]
        public void AddUserDecisionBranchLabelForDifferentProcessTypes_VerifyReturnedBranchLabelIsPresent(
            int lengthOfLabelSent,
            double orderIndexOfUserDecisionBranch,
            ProcessType processType)
        {
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcessWithOneUserDecision(
                _project,
                _adminUser);

            // Get precondition shape in process
            var precondition = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Get user decision shape in process
            var userDecision = novaProcess.Process.GetNextShape(precondition);

            // Get link for specified branch by order index
            var branchLink = novaProcess.Process.GetOutgoingLinkForShape(userDecision, orderIndexOfUserDecisionBranch);

            // Set a randomized link label for the specified branch
            branchLink.Label = RandomGenerator.RandomAlphaNumericUpperAndLowerCase((uint)lengthOfLabelSent);

            // Set the process type
            novaProcess.Process.ProcessType = processType;

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _adminUser);
        }

        [TestCase(5, 0.0)]
        [TestCase(5, 1.0)]
        [Description("Modify a user decision branch label and verify that the returned label is changed " +
                     "after saving the process.")]
        public void ModifyUserDecisionBranchLabel_VerifyReturnedBranchLabelIsModified(
            int lengthOfLabelSent,
            double orderIndexOfUserDecisionBranch)
        {
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcessWithOneUserDecision(
               _project,
               _adminUser);

            // Get precondition shape in process
            var precondition = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Get user decision shape in process
            var returnedUserDecision = novaProcess.Process.GetNextShape(precondition);

            // Get link for specified branch by order index
            var branchLink = novaProcess.Process.GetOutgoingLinkForShape(returnedUserDecision, orderIndexOfUserDecisionBranch);

            // Set a randomized link label for the specified branch
            branchLink.Label = RandomGenerator.RandomAlphaNumericUpperAndLowerCase((uint)lengthOfLabelSent);

            // Update and Verify the returned process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _adminUser);
        }

        [TestCase(0.0)]
        [TestCase(1.0)]
        [Description("Delete a user decision branch label and verify that the returned label is null " +
                     "after saving the process.")]
        public void DeleteUserDecisionBranchLabel_VerifyReturnedBranchHasLabelRemoved(
            double orderIndexOfUserDecisionBranch)
        {
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcessWithOneUserDecision(
                _project,
                _adminUser);

            // Get precondition shape in process
            var precondition = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Get user decision shape in process
            var returnedUserDecision = novaProcess.Process.GetNextShape(precondition);

            // Get link for specified branch by order index
            var branchLink = novaProcess.Process.GetOutgoingLinkForShape(returnedUserDecision, orderIndexOfUserDecisionBranch);

            // Clear the link label for the specified branch
            branchLink.Label = null;

            // Update and Verify the returned process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _adminUser);
        }

        [TestCase(1, 0.0)]
        [TestCase(5, 0.0)]
        [TestCase(30, 0.0)]
        [TestCase(1, 1.0)]
        [TestCase(5, 1.0)]
        [TestCase(30, 1.0)]
        [Description("Add a randomized system decision branch label for a specific branch and verify that the label is returned " +
             "after saving the process.")]
        public void AddSystemDecisionBranchWithPlainTextLabelOfVaryingLength_VerifyReturnedBranchLabel(
            int lengthOfLabelSent,
            double orderIndexOfSystemDecisionBranch)
        {
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcessWithOneSystemDecision(
                _project,
                _adminUser);

            // Get default user task shape
            var defaultUserTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Get system decision shape in process
            var systemDecision = novaProcess.Process.GetNextShape(defaultUserTask);

            // Get link for specified branch by order index
            var branchLink = novaProcess.Process.GetOutgoingLinkForShape(systemDecision, orderIndexOfSystemDecisionBranch);

            // Set a randomized link label for the specified branch
            branchLink.Label = RandomGenerator.RandomAlphaNumericUpperAndLowerCase((uint)lengthOfLabelSent);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess, _adminUser);
        }

        [TestCase(5, 0.0, ProcessType.BusinessProcess)]
        [TestCase(5, 1.0, ProcessType.UserToSystemProcess)]
        [Description("Add a randomized system decision label for different process types and verify the returned label" +
                     "is returned after saving the process.")]
        public void AddSystemDecisionBranchLabelForDifferentProcessTypes_VerifyReturnedBranchLabelIsPresent(
            int lengthOfLabelSent,
            double orderIndexOfSystemDecisionBranch,
            ProcessType processType)
        {
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcessWithOneSystemDecision(
                _project,
                _adminUser);

            // Get default user task shape
            var defaultUserTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Get system decision shape in process
            var systemDecision = novaProcess.Process.GetNextShape(defaultUserTask);

            // Get link for specified branch by order index
            var branchLink = novaProcess.Process.GetOutgoingLinkForShape(systemDecision, orderIndexOfSystemDecisionBranch);

            // Set a randomized link label for the specified branch
            branchLink.Label = RandomGenerator.RandomAlphaNumericUpperAndLowerCase((uint)lengthOfLabelSent);

            // Set the process type
            novaProcess.Process.ProcessType = processType;

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _adminUser);
        }

        [TestCase(5, 0.0)]
        [TestCase(5, 1.0)]
        [Description("Modify a system decision branch label and verify that the returned label is changed " +
                     "after saving the process.")]
        public void ModifySystemDecisionBranchLabel_VerifyReturnedBranchLabelIsModified(
            int lengthOfLabelSent,
            double orderIndexOfSystemDecisionBranch)
        {
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcessWithOneSystemDecision(
                _project,
                _adminUser);

            // Get default user task shape
            var defaultUserTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Get system decision shape in process
            var systemDecision = novaProcess.Process.GetNextShape(defaultUserTask);

            // Get link for specified branch by order index
            var branchLink = novaProcess.Process.GetOutgoingLinkForShape(systemDecision, orderIndexOfSystemDecisionBranch);

            // Set a randomized link label for the specified branch
            branchLink.Label = RandomGenerator.RandomAlphaNumericUpperAndLowerCase((uint)lengthOfLabelSent);

            // Update and Verify the returned process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _adminUser);
        }

        [TestCase(0.0)]
        [TestCase(1.0)]
        [Description("Delete a system decision branch label and verify that the returned label is null " +
                     "after saving the process.")]
        public void DeleteSystemDecisionBranchLabel_VerifyReturnedBranchHasLabelRemoved(
            double orderIndexOfSystemDecisionBranch)
        {
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcessWithOneSystemDecision(
                _project,
                _adminUser);

            // Get default user task shape
            var defaultUserTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Get system decision shape in process
            var systemDecision = novaProcess.Process.GetNextShape(defaultUserTask);

            // Get link for specified branch by order index
            var branchLink = novaProcess.Process.GetOutgoingLinkForShape(systemDecision, orderIndexOfSystemDecisionBranch);

            // Clear the link label for the specified branch
            branchLink.Label = null;

            // Update and Verify the returned process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess.NovaProcess, _adminUser);
        }
    }
}
