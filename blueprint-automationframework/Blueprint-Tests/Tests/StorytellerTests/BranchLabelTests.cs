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
            var process = StorytellerTestHelper.CreateAndGetDefaultProcessWithOneUserDecision(
                Helper.Storyteller,
                _project,
                _adminUser);

            // Get precondition shape in process
            var precondition = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Get user decision shape in process
            var userDecision = process.GetNextShape(precondition);

            // Get link for specified branch by order index
            var branchLink = process.GetOutgoingLinkForShape(userDecision, orderIndexOfUserDecisionBranch);

            // Set a randomized link label for the specified branch
            branchLink.Label = RandomGenerator.RandomAlphaNumericUpperAndLowerCase((uint)lengthOfLabelSent);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(process, Helper.Storyteller, _adminUser);
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
            var process = StorytellerTestHelper.CreateAndGetDefaultProcessWithOneUserDecision(
                Helper.Storyteller,
                _project,
                _adminUser);

            // Get precondition shape in process
            var precondition = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Get user decision shape in process
            var userDecision = process.GetNextShape(precondition);

            // Get link for specified branch by order index
            var branchLink = process.GetOutgoingLinkForShape(userDecision, orderIndexOfUserDecisionBranch);

            // Set a randomized link label for the specified branch
            branchLink.Label = RandomGenerator.RandomAlphaNumericUpperAndLowerCase((uint)lengthOfLabelSent);

            // Set the process type
            process.ProcessType = processType;

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(process, Helper.Storyteller, _adminUser);
        }

        [TestCase(5, 0.0)]
        [TestCase(5, 1.0)]
        [Description("Modify a user decision branch label and verify that the returned label is changed " +
                     "after saving the process.")]
        public void ModifyUserDecisionBranchLabel_VerifyReturnedBranchLabelIsModified(
            int lengthOfLabelSent,
            double orderIndexOfUserDecisionBranch)
        {
            var process = StorytellerTestHelper.CreateAndGetDefaultProcessWithOneUserDecision(
               Helper.Storyteller,
               _project,
               _adminUser);

            // Save the process
            Helper.Storyteller.UpdateProcess(_adminUser, process);

            // Get the process
            var returnedProcess = Helper.Storyteller.GetNovaProcess(_adminUser, process.Id).Process;

            // Get precondition shape in process
            var precondition = returnedProcess.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Get user decision shape in process
            var returnedUserDecision = returnedProcess.GetNextShape(precondition);

            // Get link for specified branch by order index
            var branchLink = returnedProcess.GetOutgoingLinkForShape(returnedUserDecision, orderIndexOfUserDecisionBranch);

            // Set a randomized link label for the specified branch
            branchLink.Label = RandomGenerator.RandomAlphaNumericUpperAndLowerCase((uint)lengthOfLabelSent);

            // Update and Verify the returned process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, Helper.Storyteller, _adminUser);
        }

        [TestCase(0.0)]
        [TestCase(1.0)]
        [Description("Delete a user decision branch label and verify that the returned label is null " +
                     "after saving the process.")]
        public void DeleteUserDecisionBranchLabel_VerifyReturnedBranchHasLabelRemoved(
            double orderIndexOfUserDecisionBranch)
        {
            var process = StorytellerTestHelper.CreateAndGetDefaultProcessWithOneUserDecision(
                Helper.Storyteller,
                _project,
                _adminUser);

            // Save the process
            Helper.Storyteller.UpdateProcess(_adminUser, process);

            // Get the process
            var returnedProcess = Helper.Storyteller.GetNovaProcess(_adminUser, process.Id).Process;

            // Get precondition shape in process
            var precondition = returnedProcess.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Get user decision shape in process
            var returnedUserDecision = returnedProcess.GetNextShape(precondition);

            // Get link for specified branch by order index
            var branchLink = returnedProcess.GetOutgoingLinkForShape(returnedUserDecision, orderIndexOfUserDecisionBranch);

            // Clear the link label for the specified branch
            branchLink.Label = null;

            // Update and Verify the returned process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, Helper.Storyteller, _adminUser);
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
            var process = StorytellerTestHelper.CreateAndGetDefaultProcessWithOneSystemDecision(
                Helper.Storyteller,
                _project,
                _adminUser);

            // Get default user task shape
            var defaultUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Get system decision shape in process
            var systemDecision = process.GetNextShape(defaultUserTask);

            // Get link for specified branch by order index
            var branchLink = process.GetOutgoingLinkForShape(systemDecision, orderIndexOfSystemDecisionBranch);

            // Set a randomized link label for the specified branch
            branchLink.Label = RandomGenerator.RandomAlphaNumericUpperAndLowerCase((uint)lengthOfLabelSent);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(process, Helper.Storyteller, _adminUser);
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
            var process = StorytellerTestHelper.CreateAndGetDefaultProcessWithOneSystemDecision(
                Helper.Storyteller,
                _project,
                _adminUser);

            // Get default user task shape
            var defaultUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Get system decision shape in process
            var systemDecision = process.GetNextShape(defaultUserTask);

            // Get link for specified branch by order index
            var branchLink = process.GetOutgoingLinkForShape(systemDecision, orderIndexOfSystemDecisionBranch);

            // Set a randomized link label for the specified branch
            branchLink.Label = RandomGenerator.RandomAlphaNumericUpperAndLowerCase((uint)lengthOfLabelSent);

            // Set the process type
            process.ProcessType = processType;

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(process, Helper.Storyteller, _adminUser);
        }

        [TestCase(5, 0.0)]
        [TestCase(5, 1.0)]
        [Description("Modify a system decision branch label and verify that the returned label is changed " +
                     "after saving the process.")]
        public void ModifySystemDecisionBranchLabel_VerifyReturnedBranchLabelIsModified(
            int lengthOfLabelSent,
            double orderIndexOfSystemDecisionBranch)
        {
            var process = StorytellerTestHelper.CreateAndGetDefaultProcessWithOneSystemDecision(
                Helper.Storyteller,
                _project,
                _adminUser);

            // Save the process
            Helper.Storyteller.UpdateProcess(_adminUser, process);

            // Get the process
            var returnedProcess = Helper.Storyteller.GetNovaProcess(_adminUser, process.Id).Process;

            // Get default user task shape
            var defaultUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Get system decision shape in process
            var systemDecision = process.GetNextShape(defaultUserTask);

            // Get link for specified branch by order index
            var branchLink = process.GetOutgoingLinkForShape(systemDecision, orderIndexOfSystemDecisionBranch);

            // Set a randomized link label for the specified branch
            branchLink.Label = RandomGenerator.RandomAlphaNumericUpperAndLowerCase((uint)lengthOfLabelSent);

            // Update and Verify the returned process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, Helper.Storyteller, _adminUser);
        }

        [TestCase(0.0)]
        [TestCase(1.0)]
        [Description("Delete a system decision branch label and verify that the returned label is null " +
                     "after saving the process.")]
        public void DeleteSystemDecisionBranchLabel_VerifyReturnedBranchHasLabelRemoved(
            double orderIndexOfSystemDecisionBranch)
        {
            var process = StorytellerTestHelper.CreateAndGetDefaultProcessWithOneSystemDecision(
                Helper.Storyteller,
                _project,
                _adminUser);

            // Save the process
            Helper.Storyteller.UpdateProcess(_adminUser, process);

            // Get the process
            var returnedProcess = Helper.Storyteller.GetNovaProcess(_adminUser, process.Id).Process;

            // Get default user task shape
            var defaultUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Get system decision shape in process
            var systemDecision = process.GetNextShape(defaultUserTask);

            // Get link for specified branch by order index
            var branchLink = process.GetOutgoingLinkForShape(systemDecision, orderIndexOfSystemDecisionBranch);

            // Clear the link label for the specified branch
            branchLink.Label = null;

            // Update and Verify the returned process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, Helper.Storyteller, _adminUser);
        }
    }
}
