using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.Impl;
using Model.StorytellerModel.Enums;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using System;
using TestCommon;
using Utilities;
using Utilities.Factories;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class UpdateProcessTests : TestBase
    {
        private IUser _user;
        private IProject _project;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region Tests

        [TestCase]
        [Description("Update the name of process and verify that the returned process has the" +
                     "modified name.")]
        [Explicit(IgnoreReasons.UnderDevelopment)]//now /svc/components/storyteller/processes/{Id} doesn't allow to update process name
        public void ModifyReturnedProcessName_VerifyReturnedProcess()
        {
            // Create and get the default process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Modify default process Name
            novaProcess.Process.Name = RandomGenerator.RandomValueWithPrefix("novaProcess", 4);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess, _user);
        }

        [TestCase]
        [Description("Update the process type of the process and verify that the returned process" +
                     "has the updated process type. This verifies that the process type toggle in the" +
                     "process diagram is working.")]
        public void ModifyProcessType_VerifyReturnedProcess()
        {
            // Create and get the default process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            var processType = novaProcess.Process.ProcessType;

            Assert.That(processType == ProcessType.BusinessProcess);

            // Modify default process Type
            novaProcess.Process.ProcessType = ProcessType.UserToSystemProcess;

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess, _user);
        }

        [TestCase]
        [Description("Add a new user task after the precondition and verify that the returned process" +
                     "has the new user task in the correct position and has the correct properties.")]
        public void AddUserTaskAfterPrecondition_VerifyReturnedProcess()
        {
            // Create and get the default process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find precondition task
            var preconditionTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition task
            var preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);
                
            Assert.IsNotNull(preconditionOutgoingLink, "Process link was not found.");

            // Add user/system Task immediately after the precondition
            novaProcess.Process.AddUserAndSystemTask(preconditionOutgoingLink);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess, _user);
        }

        [TestCase]
        [Description("Add a new user task before the end shape and verify that the returned process" +
                     "has the new user task in the correct position and has the correct properties.")]
        public void AddUserTaskBeforeEnd_VerifyReturnedProcess()
        {
            // Create and get the default process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find the end shape
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Find the incoming link for the end shape
            var endIncomingLink = novaProcess.Process.GetIncomingLinkForShape(endShape);

            Assert.IsNotNull(endIncomingLink, "Process link was not found.");

            // Add a user/system task immediately before the end shape
            novaProcess.Process.AddUserAndSystemTask(endIncomingLink);

            // Updatea and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess, _user);
        }

        [TestCase]
        [Description("Add a user task after an existing user task and verify that the returned process" +
                     "has the new user task in the correct position and has the correct properties.")]
        public void AddUserTaskAfterUserTask_VerifyReturnedProcess()
        {
            // Create and get the default process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find the end shape
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Find the incoming link for the end shape
            var endIncomingLink = novaProcess.Process.GetIncomingLinkForShape(endShape);

            Assert.IsNotNull(endIncomingLink, "Process link was not found.");

            // Add a user/system task immediately before the end shape
            novaProcess.Process.AddUserAndSystemTask(endIncomingLink);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess, _user);
        }

        [TestCase]
        [Description("Add a new user decision with branch after precondition and verify that the returned " +
                     "process has the new user decision and user task in the correct position and has the " +
                     "correct properties.")]
        public void AddUserDecisionWithBranchAfterPrecondition_VerifyReturnedProcess()
        {
            // Create and get the default process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find precondition task
            var preconditionTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            // Get the branch end point
            var branchEndPoint = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Add Decision point with branch to end
            novaProcess.Process.AddUserDecisionPointWithBranchAfterShape(preconditionTask, preconditionOutgoingLink.Orderindex + 1, branchEndPoint.Id);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess, _user);
        }

        [TestCase]
        [Description("Add a new user decision with branch before the end shape and verify that the returned " +
                     "process has the new user decision and branch with 2 new user tasks in the correct position " +
                     "and has the correct properties.")]
        public void AddUserDecisionWithBranchBeforeEnd_VerifyReturnedProcess()
        {
            // Create and get the default process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find the end shape
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Find incoming process link for end shape
            var endIncomingLink = novaProcess.Process.GetIncomingLinkForShape(endShape);

            // Add Decision point with branch and 2 user tasks
            novaProcess.Process.AddUserDecisionPointWithBranchBeforeShape(endShape, endIncomingLink.Orderindex + 1);

            // Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess, _user);
        }

        [TestCase]
        [Description("Add new user decision point between 2 user tasks and verify that the returned process" +
                     "has the new user decision and branch in the correct position and has the correct properties.")]
        public void AddUserDecisionBetweenTwoUserTasks_VerifyReturnedProcess()
        {
            // Create and get the default process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find the end shape
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Find the incoming link for the end shape
            var endIncomingLink = novaProcess.Process.GetIncomingLinkForShape(endShape);

            // Add a user/system task immediately before the end shape
            var newUserTask = novaProcess.Process.AddUserAndSystemTask(endIncomingLink);

            // Add a user decision point between 2 user/system tasks
            novaProcess.Process.AddUserDecisionPointWithBranchBeforeShape(newUserTask, endIncomingLink.Orderindex + 1, endShape.Id);

            // Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess, _user);
        }

        [TestCase]
        [Description("Add new user decision point before a merge point and verify that returned process" +
                     "has the new user decision point, branch and 2 new user tasks and has the correct " +
                     "properties.")]
        public void AddUserDecisionWithinMainBranchBeforeMergePoint_VerifyReturnedProcess()
        {
            // Create and get the default process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find precondition task
            var preconditionTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find the outgoing link for the precondition
            var preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            // Add Decision point with branch (make order index to to give room for internal branch
            novaProcess.Process.AddUserDecisionPointWithBranchAfterShape(preconditionTask, preconditionOutgoingLink.Orderindex + 1);

            // Find the updated outgoing link for the precondition
            preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            // Add user and system task before new user decision point
            novaProcess.Process.AddUserAndSystemTask(preconditionOutgoingLink);

            // Find the updated outgoing link for the precondition
            preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            // Add a user decision point after the precondition and before the new user/system task
            novaProcess.Process.AddUserDecisionPointWithBranchAfterShape(preconditionTask, preconditionOutgoingLink.Orderindex + 1);

            // Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess, _user);
        }

        [TestCase]
        [Description("Add a second branch to a user decision point and verify that the returned process" +
                     "has the new branch along with a new user task in the the correct position and with" +
                     "the correct properties.")]
        public void AddSecondBranchToUserDecision_VerifyReturnedProcess()
        {
            // Create and get the default process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find precondition task
            var preconditionTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            // Find the endpoint for the new branch
            var branchEndPoint = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Add decision point with branch to end
            var userDecision = novaProcess.Process.AddUserDecisionPointWithBranchAfterShape(preconditionTask, preconditionOutgoingLink.Orderindex + 1);

            // Add branch to decision point with task
            novaProcess.Process.AddBranchWithUserAndSystemTaskToUserDecisionPoint(userDecision, preconditionOutgoingLink.Orderindex + 2, branchEndPoint.Id); 

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess, _user);
        }

        [TestCase]
        [Description("Add a new user decision point to a branch and verify that the returned process" +
                     "has the new decision point, branch and 2 user tasks and has the correct properties.")]
        public void AddUserDecisionToBranch_VerifyReturnedProcess()
        {
            // Create and get the default process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find precondition task
            var preconditionTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            // Determine the branch endpoint
            var branchEndPoint = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Add decision point with branch after precondition

            var decisionPoint = novaProcess.Process.AddUserDecisionPointWithBranchAfterShape(preconditionTask, preconditionOutgoingLink.Orderindex + 1, branchEndPoint.Id);

            // Add branch to decision point with task
            var newUserTask = novaProcess.Process.AddBranchWithUserAndSystemTaskToUserDecisionPoint(decisionPoint, preconditionOutgoingLink.Orderindex + 2, branchEndPoint.Id);

            var newSystemTask = novaProcess.Process.GetNextShape(newUserTask);

            // Add decision point with branch to end
            novaProcess.Process.AddUserDecisionPointWithBranchAfterShape(newSystemTask, preconditionOutgoingLink.Orderindex + 1, branchEndPoint.Id);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess, _user);
        }

        [TestCase]
        [Description("Add a user task after a merge point and verify that the returned process" +
                     "has the new user task and the correct properties.")]
        public void AddUserTaskAfterMergePoint_VerifyReturnedProcess()
        {
            // Create and get the default process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find the end shape
            var endShape = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Find the incoming link for the end shape
            var endIncomingLink = novaProcess.Process.GetIncomingLinkForShape(endShape);

            Assert.IsNotNull(endIncomingLink, "Process link was not found.");

            var newUserTask = novaProcess.Process.AddUserAndSystemTask(endIncomingLink);

            // Find the precondition task
            var precondition = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find the outgoing link for the precondition shape
            var preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(precondition);

            Assert.IsNotNull(preconditionOutgoingLink, "Process link was not found.");

            // Add user decision with branch and merge point after precondition
            novaProcess.Process.AddUserDecisionPointWithBranchAfterShape(precondition, preconditionOutgoingLink.Orderindex + 1, newUserTask.Id);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess, _user);
        }

        [TestCase]
        [Description("Add a user decision point after a merge point and verify that the returned process" +
                     "has the new decision point with branch and 2 new user tasks and has correct properties.")]
        public void AddUserDecisionPointAfterMergePoint_VerifyReturnedProcess()
        {
            // Create and get the default process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find precondition task
            var preconditionTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            // Determine the branch endpoint
            var branchEndPoint = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Add Decision point with branch to end
            var userDecisionPoint = novaProcess.Process.AddUserDecisionPointWithBranchAfterShape(preconditionTask, preconditionOutgoingLink.Orderindex + 1, branchEndPoint.Id);

            // Add decision point before decision point; will have same branch order index as previous added branch
            novaProcess.Process.AddUserDecisionPointWithBranchBeforeShape(userDecisionPoint, preconditionOutgoingLink.Orderindex + 1, userDecisionPoint.Id);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess, _user);
        }

        [TestCase]
        [Description("Add an include to a user task and verify that the returned process contains the added include.")]
        public void AddIncludeToUserTask_VerifyReturnedProcess()
        {
            // Create and get the default process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Create and save process artifact to be used as include; enable recursive delete flag
            var includedProcessArtifact = Helper.Storyteller.CreateAndSaveNovaProcessArtifact(_project, _user);
            // Add include to default user task
            var defaultUserTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            defaultUserTask.AddAssociatedArtifact(includedProcessArtifact);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess, _user);
        }

        [TestCase]
        [Description("Add an include to system task and verify that the returned process contains the added include.")]
        public void AddIncludeToSystemTask_VerifyReturnedProcess()
        {
            // Create and get the default process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Create and publish process artifact to be used as include; enable recursive delete flag
            var includedProcessArtifact = Helper.Storyteller.CreateAndPublishProcessArtifact(_project, _user);

            // Add include to default user task
            var defaultSystemTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultSystemTaskName);
            defaultSystemTask.AddAssociatedArtifact(Helper.ArtifactStore.GetArtifactDetails(_user, includedProcessArtifact.Id));

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess, _user);
        }

        [TestCase]
        [Description("Delete an include from a user task and verify that the returned process does not contain the include.")]
        public void DeleteIncludeFromUserTask_VerifyReturnedProcess()
        {
            // Create and get the default process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Create and publish process artifact to be used as include; enable recursive delete flag
            var includedProcessArtifact = Helper.Storyteller.CreateAndPublishProcessArtifact(_project, _user);

            // Add include to default user task
            var defaultUserTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            defaultUserTask.AddAssociatedArtifact(Helper.ArtifactStore.GetArtifactDetails(_user, includedProcessArtifact.Id));

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess, _user);

            // Get the process using GetProcess
            var novaProcessReturnedFromGet = Helper.Storyteller.GetNovaProcess(_user, novaProcess.Id);

            // Remove the include from the default user task
            defaultUserTask = novaProcessReturnedFromGet.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);
            defaultUserTask.AssociatedArtifact = null;

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcessReturnedFromGet, _user);
        }

        [TestCase((uint)4096, "4KB_File.jpg", "application/json;charset=utf-8")]
        [Description("Upload an Image file to Default Precondition and verify returned process model")]
        public void UploadImageToDefaultPrecondition_VerifyImage(uint fileSize, string fakeFileName, string fileType)
        {
            // Create and get a default nova process artifact
            var addedNovaProcess = Helper.Storyteller.CreateAndSaveNovaProcessArtifact(project: _project, user: _user);

            // Setup: create a file with a random byte array.
            var file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Uploading the file
            var deserialzedUploadResult = Helper.Storyteller.UploadFile(_user, file, DateTime.Now.AddDays(1));

            // Update the default precondition properties in the retrieved process model with Guid and UriToFile
            var defaultPreconditionShape = addedNovaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            defaultPreconditionShape.PropertyValues[PropertyTypeName.AssociatedImageUrl.ToString().LowerCaseFirstCharacter()].Value = deserialzedUploadResult.UriToFile;
            defaultPreconditionShape.PropertyValues[PropertyTypeName.ImageId.ToString().LowerCaseFirstCharacter()].Value = deserialzedUploadResult.Guid;

            // Save the process with the updated properties
            addedNovaProcess = Helper.Storyteller.UpdateNovaProcess(_user, addedNovaProcess);

            // Publish the process
            Helper.Storyteller.PublishNovaProcess(_user, addedNovaProcess);

            // Assert that the Default Precondition SystemTask contains value
            defaultPreconditionShape = addedNovaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            var updatedAssociatedImageUrl = defaultPreconditionShape.PropertyValues[PropertyTypeName.AssociatedImageUrl.ToString().LowerCaseFirstCharacter()].Value.ToString();
            var updatedImageId = defaultPreconditionShape.PropertyValues[PropertyTypeName.ImageId.ToString().LowerCaseFirstCharacter()].Value.ToString();

            Assert.That(updatedAssociatedImageUrl.Contains("/svc/components/RapidReview/diagram/image/"), "The updated associatedImageUri of The precondition contains {0}", updatedAssociatedImageUrl);

            Assert.IsNotNull(updatedImageId, "The updated ImageId of The precondition contains nothing");

            // Assert that there is a row of data available on image table
            VerifyImageRowsFromDb(updatedImageId);
        }

        [TestCase]
        [Description("Add a new system decision point to the default process. The new system decision point added after the default UT.")]
        public void AddSystemDecisionWithBranchAfterDefaultUserTask_VerifyReturnedProcess()
        {
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find the default UserTask
            var defaultUserTask = process.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the target SystemTask
            var targetSystemTask = process.Process.GetProcessShapeByShapeName(Process.DefaultSystemTaskName);

            // Find the branch end point for the system decision point
            var branchEndPoint = process.Process.GetProcessShapeByShapeName(Process.EndName);
            
            // Find the outgoing process link from the default UserTask
            var defaultUserTaskOutgoingProcessLink = process.Process.GetOutgoingLinkForShape(defaultUserTask);

            // Add System Decision point with branch to end
            process.Process.AddSystemDecisionPointWithBranchBeforeSystemTask(targetSystemTask, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint.Id);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(process, _user);
        }

        [TestCase]
        [Description("Add two new system decision points to the default process. The two system decision points added one after the other after the default UT.")]
        public void AddTwoSystemDecisionsWithBranchAfterDefaultUserTaskVerifyReturnedProcess()
        {
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find the default UserTask
            var defaultUserTask = process.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the default SystemTask
            var defaultSystemTask = process.Process.GetProcessShapeByShapeName(Process.DefaultSystemTaskName);

            // Find the branch end point for system decision points
            var branchEndPoint = process.Process.GetProcessShapeByShapeName(Process.EndName);
            
            // Find the outgoing process link from the default UserTask
            var defaultUserTaskOutgoingProcessLink = process.Process.GetOutgoingLinkForShape(defaultUserTask);

            // Find next system task
            var systemTask = process.Process.GetNextShape(defaultUserTask);

            // Add System Decision point with branch to end
            process.Process.AddSystemDecisionPointWithBranchBeforeSystemTask(systemTask, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint.Id);

            // Get the updated default outgoing process link from the default UserTask
            defaultUserTaskOutgoingProcessLink = process.Process.GetIncomingLinkForShape(defaultSystemTask);

            // Find next system task
            systemTask = process.Process.GetNextShape(defaultUserTask);

            // Add System Decision point with branch to end
            process.Process.AddSystemDecisionPointWithBranchBeforeSystemTask(systemTask, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint.Id);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(process, _user);
        }

        [TestCase]
        [Description("Add a new system decision point to the default process. The system decision point gets added with two additonal branches after the default UT.")]
        public void AddSystemDecisionWithTwoBranchesAfterDefaultUserTaskVerifyReturnedProcess()
        {
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find the default UserTask
            var defaultUserTask = process.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the target SystemTask
            var targetSystemTask = process.Process.GetProcessShapeByShapeName(Process.DefaultSystemTaskName);

            // Find the branch end point for system decision points
            var branchEndPoint = process.Process.GetProcessShapeByShapeName(Process.EndName);

            // Find the outgoing process link from the default UserTask
            var defaultUserTaskOutgoingProcessLink = process.Process.GetOutgoingLinkForShape(defaultUserTask);

            // Add System Decision point with a branch merging to branchEndPoint
            var systemDecisionPoint = process.Process.AddSystemDecisionPointWithBranchBeforeSystemTask(targetSystemTask, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint.Id);
            
            // Add additonal branch to the System Decision point
            process.Process.AddBranchWithSystemTaskToSystemDecisionPoint(systemDecisionPoint,defaultUserTaskOutgoingProcessLink.Orderindex+2,branchEndPoint.Id);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(process, _user);
        }

        [TestCase]
        [Description("Add a new system decision point to the default process. The system decision point gets added with an additonal branch which also contains a system decision point.")]
        public void AddSystemDecisionWithBranchWithSystemDecisionAfterDefaultUserTaskVerifyReturnedProcess()
        {
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find the default UserTask
            var defaultUserTask = process.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the target SystemTask
            var targetSystemTask = process.Process.GetProcessShapeByShapeName(Process.DefaultSystemTaskName);

            // Find the branch end point for system decision points
            var branchEndPoint = process.Process.GetProcessShapeByShapeName(Process.EndName);

            // Find the outgoing process link from the default UserTask
            var defaultUserTaskOutgoingProcessLink = process.Process.GetOutgoingLinkForShape(defaultUserTask);

            // Add System Decision point with a branch merging to branchEndPoint
            process.Process.AddSystemDecisionPointWithBranchBeforeSystemTask(targetSystemTask, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint.Id);

            // Get the link between the system decision point and the System task on the second branch
            var branchingProcessLink = process.Process.Links.Find(l => l.Orderindex.Equals(defaultUserTaskOutgoingProcessLink.Orderindex + 1));

            // Get the system task shape on the second branch for adding the additional System Decision Point
            var systemTaskOnTheSecondBranch = process.Process.GetProcessShapeById(branchingProcessLink.DestinationId);

            // Add the System Decision point on the second branch with the end merging to the same ending point as the first System Decision point
            process.Process.AddSystemDecisionPointWithBranchBeforeSystemTask(systemTaskOnTheSecondBranch, defaultUserTaskOutgoingProcessLink.Orderindex + 2, branchEndPoint.Id);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(process, _user);
        }

        [TestCase]
        [Description("Add a new system decision point to the default process. The system decision point gets added with two additonal branches: " +
            "one contains a system decision point along with branches and system tasks and the other contains just a system task")]
        public void AddSystemDecisionWithBranchesWithSystemDecisionAfterDefaultUserTaskVerifyReturnedProcess()
        {
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find the default UserTask
            var defaultUserTask = process.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the target SystemTask
            var targetSystemTask = process.Process.GetProcessShapeByShapeName(Process.DefaultSystemTaskName);

            // Find the branch end point for system decision points
            var branchEndPoint = process.Process.GetProcessShapeByShapeName(Process.EndName);

            // Find the outgoing process link from the default UserTask
            var defaultUserTaskOutgoingProcessLink = process.Process.GetOutgoingLinkForShape(defaultUserTask);

            // Add a System Decision point (root System Decision point) with a branch merging to branchEndPoint
            var rootSystemDecisionPoint = process.Process.AddSystemDecisionPointWithBranchBeforeSystemTask(targetSystemTask, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint.Id);

            // Get the link between the system decision point and the System task on the second branch
            var branchingProcessLink = process.Process.Links.Find(l => l.Orderindex.Equals(defaultUserTaskOutgoingProcessLink.Orderindex + 1));

            // Get the System Task shape on the second branch for adding the additional System Decision Point
            var systemTaskOnTheSecondBranch = process.Process.GetProcessShapeById(branchingProcessLink.DestinationId);

            // Add a System Decision point on the second branch that merges to branchEndPoint
            process.Process.AddSystemDecisionPointWithBranchBeforeSystemTask(systemTaskOnTheSecondBranch, defaultUserTaskOutgoingProcessLink.Orderindex + 2, branchEndPoint.Id);

            // Add additonal branch on the root System Decision point
            process.Process.AddBranchWithSystemTaskToSystemDecisionPoint(rootSystemDecisionPoint, defaultUserTaskOutgoingProcessLink.Orderindex + 3, branchEndPoint.Id);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(process, _user);
        }

        [TestCase]
        [Description("Add a new system decision point to the default process. The system decision point gets added with two additonal branches: " +
            "each of them contains a system decision point along with branches and system tasks")]
        public void AddSystemDecisionWithTwoBranchesWithSystemDecisionAfterDefaultUserTaskVerifyReturnedProcess()
        {
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find the default UserTask
            var defaultUserTask = process.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the target SystemTask
            var targetSystemTask = process.Process.GetProcessShapeByShapeName(Process.DefaultSystemTaskName);

            // Find the branch end point for system decision points
            var branchEndPoint = process.Process.GetProcessShapeByShapeName(Process.EndName);

            // Find the outgoing process link from the default UserTask
            var defaultUserTaskOutgoingProcessLink = process.Process.GetOutgoingLinkForShape(defaultUserTask);

            // Add a System Decision point with branch (root System Decision point) with a branch merging to branchEndPoint
            var rootSystemDecisionPoint = process.Process.AddSystemDecisionPointWithBranchBeforeSystemTask(targetSystemTask, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint.Id);

            // Get the link between the system decision point and the System task on the second branch
            var branchingProcessLink = process.Process.Links.Find(l => l.Orderindex.Equals(defaultUserTaskOutgoingProcessLink.Orderindex + 1));

            // Get the System Task shape on the second branch for adding the additional System Decision Point
            var systemTaskOnTheSecondBranch = process.Process.GetProcessShapeById(branchingProcessLink.DestinationId);

            // Add a System Decision point on the second branch that merges to branchEndPoint
            process.Process.AddSystemDecisionPointWithBranchBeforeSystemTask(systemTaskOnTheSecondBranch, defaultUserTaskOutgoingProcessLink.Orderindex + 2, branchEndPoint.Id);

            // Add additonal branch on the root System Decision point
            process.Process.AddBranchWithSystemTaskToSystemDecisionPoint(rootSystemDecisionPoint, defaultUserTaskOutgoingProcessLink.Orderindex + 3, branchEndPoint.Id);

            // Get the link between the system decision point and the System task on the second branch
            var secondBranchingProcessLink = process.Process.Links.Find(l => l.Orderindex.Equals(defaultUserTaskOutgoingProcessLink.Orderindex + 3));

            // Get the System Task shape on the second branch for adding the additional System Decision Point
            var systemTaskOnTheThirdBranch = process.Process.GetProcessShapeById(secondBranchingProcessLink.DestinationId);

            // Add a System Decision point on the third branch that merges to branchEndPoint
            process.Process.AddSystemDecisionPointWithBranchBeforeSystemTask(systemTaskOnTheThirdBranch, defaultUserTaskOutgoingProcessLink.Orderindex + 4, branchEndPoint.Id);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(process, _user);
        }

        [TestCase]
        [Description("Add a new system decision point to the default process. The system decision point gets added with two additonal branches: " +
            "each of them contains a system decision point along with branches and system tasks")]
        public void AddTwoSystemDecisionsWithBranchesOnMainBranchVerifyReturnedProcess()
        {
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find the branch end point for system decision points
            var branchEndPoint = process.Process.GetProcessShapeByShapeName(Process.EndName);

            // Find the default UserTask
            var defaultUserTask = process.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the outgoing process link from the default UserTask
            var defaultUserTaskOutgoingProcessLink = process.Process.GetOutgoingLinkForShape(defaultUserTask);

            // Find the default SystemTask
            var defaultSystemTask = process.Process.GetProcessShapeByShapeName(Process.DefaultSystemTaskName);

            // Find the outgoing process link from the default SystemTask
            var defaultSystemTaskOutgoingProcessLink = process.Process.GetOutgoingLinkForShape(defaultSystemTask);

            // Add a new User Task with System Task before the end Point
            var addedUserTask = process.Process.AddUserAndSystemTask(defaultSystemTaskOutgoingProcessLink);

            // Add a System Decision point with branch (first System Decision point) and close the loop before the addedUserTask
            process.Process.AddSystemDecisionPointWithBranchBeforeSystemTask(defaultSystemTask, defaultUserTaskOutgoingProcessLink.Orderindex + 1, addedUserTask.Id);

            // find the outgoing process link from the addedUserTask
            var newUserTaskOutgoingProcessLink = process.Process.GetOutgoingLinkForShape(addedUserTask);

            // Find next system task
            var systemTask = process.Process.GetNextShape(addedUserTask);

            // Add a System Decision point with branch (second System Decision point) and close the loop before the branchEndPoint
            process.Process.AddSystemDecisionPointWithBranchBeforeSystemTask(systemTask, newUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint.Id);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(process, _user);
        }

        [TestCase]
        [Description("Add a new system decision point to the default process. The system decision point gets added with two additonal branches after the default UT. Generate User Story")]
        public void GenerateUserStoryForSystemDecisionWithTwoBranchesAfterDefaultUserTaskVerifyReturnedProcess()
        {
            /*
            You start with this:
            --[UT]--+--[ST]--+--

            It becomes this:
            --[UT]--+--<SD>--+--[ST]--+--
                        |             |
                        +-------[ST]--+
             */

            // Create and get the default process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find the default UserTask
            var defaultUserTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the target SystemTask
            var targetSystemTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultSystemTaskName);

            // Find the branch end point for system decision points
            var branchEndPoint = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Find the outgoing process link from the default UserTask
            var defaultUserTaskOutgoingProcessLink = novaProcess.Process.GetOutgoingLinkForShape(defaultUserTask);

            // Add System Decision point with a branch merging to branchEndPoint
            novaProcess.Process.AddSystemDecisionPointWithBranchBeforeSystemTask(targetSystemTask, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint.Id);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishNovaProcess(novaProcess.NovaProcess, _user);

            // Find number of UserTasks from the published Process
            var returnedNovaProcess = Helper.Storyteller.GetNovaProcess(_user, novaProcess.Id);

            var userTasksOnProcess = returnedNovaProcess.Process.GetProcessShapesByShapeType(ProcessShapeType.UserTask).Count;

            // Generate User Story artfact(s) from the Process artifact
            var userStories = Helper.Storyteller.GenerateUserStories(_user, returnedNovaProcess.Process);

            Logger.WriteDebug("The number of UserStories generated is: {0}", userStories.Count);

            // Assert that the number of UserTasks from the published Process is equal to the number of UserStoryGenerated or Updated
            Assert.That(userStories.Count == userTasksOnProcess, "The number of UserStories generated from the process is {0} but The process has {1} UserTasks.", userStories.Count, userTasksOnProcess);
        }

        [TestCase]
        [Description("Add a new system decision point to the default process. The system decision point gets added with additonal branch. " +
            "Add another system decision point immediately after 1st one (Nested SD). Generate User Story")]
        public void GenerateUserStoryForTwoSystemDecisionsWithBranchesOnMainBranchVerifyReturnedProcess()
        {
            /*
           You start with this:
           --[UT]--+--[ST]--+--

           It becomes this:

            --[UT]--+--<SD>--+--<SD>--+--[ST]--+--[UT]--+--[ST]--+--
                        |        |             |                 |
                        |        +----------------[ST]-----------+
                        |                      |
                        +------[ST]------------+
            */

            // Create and get the default process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find the branch end point for system decision points
            var branchEndPoint = novaProcess.Process.GetProcessShapeByShapeName(Process.EndName);

            // Find the default UserTask
            var defaultUserTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the outgoing process link from the default UserTask
            var defaultUserTaskOutgoingProcessLink = novaProcess.Process.GetOutgoingLinkForShape(defaultUserTask);

            // Find the default SystemTask
            var defaultSystemTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultSystemTaskName);

            // Find the outgoing process link from the default SystemTask
            var defaultSystemTaskOutgoingProcessLink = novaProcess.Process.GetOutgoingLinkForShape(defaultSystemTask);

            // Add a new User Task with System Task before the end Point
            var addedUserTask = novaProcess.Process.AddUserAndSystemTask(defaultSystemTaskOutgoingProcessLink);

            // Add a System Decision point with branch (first System Decision point) and close the loop before the addedUserTask
            novaProcess.Process.AddSystemDecisionPointWithBranchBeforeSystemTask(defaultSystemTask, defaultUserTaskOutgoingProcessLink.Orderindex + 1, addedUserTask.Id);

            // Add a System Decision point with branch (second System Decision point) and close the loop before the branchEndPoint
            novaProcess.Process.AddSystemDecisionPointWithBranchBeforeSystemTask(defaultSystemTask, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint.Id);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishNovaProcess(novaProcess.NovaProcess, _user);

            // Find number of UserTasks from the published Process
            var returnedNovaProcess = Helper.Storyteller.GetNovaProcess(_user, novaProcess.Id);

            var userTasksOnProcess = returnedNovaProcess.Process.GetProcessShapesByShapeType(ProcessShapeType.UserTask).Count;

            // Generate User Story artfact(s) from the Process artifact
            var userStories = Helper.Storyteller.GenerateUserStories(_user, returnedNovaProcess.Process);

            Logger.WriteDebug("The number of UserStories generated is: {0}", userStories.Count);

            // Assert that the number of UserTasks from the published Process is equal to the number of UserStoryGenerated or Updated
            Assert.That(userStories.Count == userTasksOnProcess, "The number of UserStories generated from the process is {0} but The process has {1} UserTasks.", userStories.Count, userTasksOnProcess);
        }

        [TestCase]
        [Description("Add new User Task to the default process. Save, do not publish changes." +
                     "Get discussions for this User Task returns no errors. Regression for bug 178131.")]
        public void GetRaptorDiscussionsForSavedUnpublishedUserTask_ThrowsNoErrors()
        {
            // Create and get the default process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(_project, _user);

            // Find precondition task
            var preconditionTask = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition task
            var preconditionOutgoingLink = novaProcess.Process.GetOutgoingLinkForShape(preconditionTask);

            Assert.IsNotNull(preconditionOutgoingLink, "Process link was not found.");

            // Add user/system Task immediately after the precondition
            novaProcess.Process.AddUserAndSystemTask(preconditionOutgoingLink);

            // Save changes without publishing
            Helper.Storyteller.UpdateNovaProcess(_user, novaProcess.NovaProcess);

            var returnedNovaProcess = Helper.Storyteller.GetNovaProcess(_user, novaProcess.Id);

            // Get newly added User Task
            var unpublishedUserTask = returnedNovaProcess.Process.GetNextShape(preconditionTask);

            Assert.DoesNotThrow(() =>
            {
                var discussions = Helper.SvcComponents.GetRapidReviewDiscussions(_user, unpublishedUserTask.Id, includeDraft: true);

                Assert.That(discussions.ArtifactId == novaProcess.Id, "The ArtifactID must be equal to Process id.");
                Assert.That(discussions.SubArtifactId == unpublishedUserTask.Id, "The SubArtifactID must be equal User Task id.");
            }, "Get Discussions for saved/unpublished User Task shouldn't return an error.");
        }

        /// <summary>
        /// Verifies that number of row from Image table match with expected value
        /// </summary>
        /// <param name="imageId">the image Id that can be used to find image from the image table</param>
        private static void VerifyImageRowsFromDb(string imageId)
        {
            string query = I18NHelper.FormatInvariant("SELECT COUNT (*) as counter FROM dbo.Images WHERE ImageId = {0};", imageId);

            const int expectedImagerowCount = 1;
            int resultCount = DatabaseHelper.ExecuteSingleValueSqlQuery<int>(query);

            Assert.AreEqual(expectedImagerowCount, resultCount, "The total number of rows for the uploaded image is {0} but we expected {1}",
                resultCount, expectedImagerowCount);
        }

        #endregion Tests
    }
}
