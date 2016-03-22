using System;
using System.Globalization;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.OpenApiModel;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using Utilities;
using Utilities.Factories;
using System.Data.SqlClient;
using Common;
using System.Data;
using System.Linq;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class UpdateProcessTests
    {
        private IAdminStore _adminStore;
        private IStoryteller _storyteller;
        private IUser _user;
        private IProject _project;
        private IFileStore _filestore;
        private bool _deleteChildren = true;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _storyteller = StorytellerFactory.GetStorytellerFromTestConfig();
            _user = UserFactory.CreateUserAndAddToDatabase();
            _project = ProjectFactory.GetProject(_user);
            _filestore = FileStoreFactory.GetFileStoreFromTestConfig();

            // Get a valid token for the user.
            ISession session = _adminStore.AddSession(_user.Username, _user.Password);
            _user.SetToken(session.SessionId);

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.AccessControlToken), "The user didn't get an Access Control token!");
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            if (_filestore != null)
            {
                // Delete all the files that were added.
                foreach (var file in _filestore.Files.ToArray())
                {
                    _filestore.DeleteFile(file.Id, _user);
                }
            }

             if (_storyteller.Artifacts != null)
            {
                // Delete all the artifacts that were added.
                foreach (var artifact in _storyteller.Artifacts.ToArray())
                {
                    _storyteller.DeleteProcessArtifact(artifact, _user, deleteChildren: _deleteChildren);
                }
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
        }

        #endregion Setup and Cleanup

        #region Tests

        [TestCase]
        [Description("Update the name of process and verify that the returned process has the" +
                     "modified name.")]
        public void ModifyReturnedProcessName_VerifyReturnedProcess()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Modify default process Name
            returnedProcess.Name = RandomGenerator.RandomValueWithPrefix("returnedProcess", 4);
            returnedProcess.ArtifactPathLinks[0].Name = returnedProcess.Name;

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Update the process type of the process and verify that the returned process" +
                     "has the updated process type. This verifies that the process type toggle in the" +
                     "process diagram is working.")]
        public void ModifyProcessType_VerifyReturnedProcess()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Must lower the case of the first character to create lower case property name
            var clientTypePropertyName = PropertyTypePredefined.ClientType.ToString().LowerCaseFirstCharacter();

            var processType = Convert.ToInt32(returnedProcess.PropertyValues[clientTypePropertyName].Value, CultureInfo.InvariantCulture);

            Assert.That(processType == (int)ProcessType.BusinessProcess);

            // Modify default process Type
            returnedProcess.PropertyValues[clientTypePropertyName].Value = (int)ProcessType.UserToSystemProcess;

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Add a new user task after the precondition and verify that the returned process" +
                     "has the new user task in the correct position and has the correct properties.")]
        public void AddUserTaskAfterPrecondition_VerifyReturnedProcess()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition task
            var preconditionOutgoingLink = returnedProcess.GetOutgoingLinkForShape(preconditionTask.Id);
                
            Assert.IsNotNull(preconditionOutgoingLink, "Process link was not found.");

            // Add user/system Task immediately after the precondition
            returnedProcess.AddUserAndSystemTask(preconditionOutgoingLink);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Add a new user task before the end shape and verify that the returned process" +
                     "has the new user task in the correct position and has the correct properties.")]
        public void AddUserTaskBeforeEnd_VerifyReturnedProcess()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find the end shape
            var endShape = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Find the incoming link for the end shape
            var endIncomingLink = returnedProcess.GetIncomingLinkForShape(endShape.Id);

            Assert.IsNotNull(endIncomingLink, "Process link was not found.");

            // Add a user/system task immediately before the end shape
            returnedProcess.AddUserAndSystemTask(endIncomingLink);

            // Updatea and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Add a user task after an existing user task and verify that the returned process" +
                     "has the new user task in the correct position and has the correct properties.")]
        public void AddUserTaskAfterUserTask_VerifyReturnedProcess()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find the end shape
            var endShape = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Find the incoming link for the end shape
            var endIncomingLink = returnedProcess.GetIncomingLinkForShape(endShape.Id);

            Assert.IsNotNull(endIncomingLink, "Process link was not found.");

            // Add a user/system task immediately before the end shape
            returnedProcess.AddUserAndSystemTask(endIncomingLink);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Add a new user decision with branch after precondition and verify that the returned " +
                     "process has the new user decision and user task in the correct position and has the " +
                     "correct properties.")]
        public void AddUserDecisionWithBranchAfterPrecondition_VerifyReturnedProcess()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = returnedProcess.GetOutgoingLinkForShape(preconditionTask.Id);

            // Get the branch end point
            var branchEndPoint = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Add Decision point with branch to end
            returnedProcess.AddUserDecisionPointWithBranchAfterShape(preconditionTask.Id, preconditionOutgoingLink.Orderindex + 1, branchEndPoint.Id);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Add a new user decision with branch before the end shape and verify that the returned " +
                     "process has the new user decision and branch with 2 new user tasks in the correct position " +
                     "and has the correct properties.")]
        public void AddUserDecisionWithBranchBeforeEnd_VerifyReturnedProcess()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find the end shape
            var endShape = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Find incoming process link for end shape
            var endIncomingLink = returnedProcess.GetIncomingLinkForShape(endShape.Id);

            // Add Decision point with branch and 2 user tasks
            returnedProcess.AddUserDecisionPointWithBranchBeforeShape(endShape.Id, endIncomingLink.Orderindex + 1);

            // Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Add new user decision point between 2 user tasks and verify that the returned process" +
                     "has the new user decision and branch in the correct position and has the correct properties.")]
        public void AddUserDecisionBetweenTwoUserTasks_VerifyReturnedProcess()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find the end shape
            var endShape = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Find the incoming link for the end shape
            var endIncomingLink = returnedProcess.GetIncomingLinkForShape(endShape.Id);

            // Add a user/system task immediately before the end shape
            var newUserTask = returnedProcess.AddUserAndSystemTask(endIncomingLink);

            // Add a user decision point between 2 user/system tasks
            returnedProcess.AddUserDecisionPointWithBranchBeforeShape(newUserTask.Id, endIncomingLink.Orderindex + 1, endShape.Id);

            // Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Add new user decision point before a merge point and verify that returned process" +
                     "has the new user decision point, branch and 2 new user tasks and has the correct " +
                     "properties.")]
        public void AddUserDecisionWithinMainBranchBeforeMergePoint_VerifyReturnedProcess()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find the outgoing link for the precondition
            var preconditionOutgoingLink = returnedProcess.GetOutgoingLinkForShape(preconditionTask.Id);

            // Add Decision point with branch (make order index to to give room for internal branch
            returnedProcess.AddUserDecisionPointWithBranchAfterShape(preconditionTask.Id, preconditionOutgoingLink.Orderindex + 1);

            // Find the updated outgoing link for the precondition
            preconditionOutgoingLink = returnedProcess.GetOutgoingLinkForShape(preconditionTask.Id);

            // Add user and system task before new user decision point
            returnedProcess.AddUserAndSystemTask(preconditionOutgoingLink);

            // Find the updated outgoing link for the precondition
            preconditionOutgoingLink = returnedProcess.GetOutgoingLinkForShape(preconditionTask.Id);

            // Add a user decision point after the precondition and before the new user/system task
            returnedProcess.AddUserDecisionPointWithBranchAfterShape(preconditionTask.Id, preconditionOutgoingLink.Orderindex + 1);

            // Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Add a second branch to a user decision point and verify that the returned process" +
                     "has the new branch along with a new user task in the the correct position and with" +
                     "the correct properties.")]
        public void AddSecondBranchToUserDecision_VerifyReturnedProcess()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = returnedProcess.GetOutgoingLinkForShape(preconditionTask.Id);

            // Find the endpoint for the new branch
            var branchEndPoint = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Add decision point with branch to end
            var userDecision = returnedProcess.AddUserDecisionPointWithBranchAfterShape(preconditionTask.Id, preconditionOutgoingLink.Orderindex + 1);

            // Add branch to decision point with task
            returnedProcess.AddBranchWithUserAndSystemTaskToUserDecisionPoint(userDecision.Id, preconditionOutgoingLink.Orderindex + 2, branchEndPoint.Id); 

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Add a new user decision point to a branch and verify that the returned process" +
                     "has the new decision point, branch and 2 user tasks and has the correct properties.")]
        public void AddUserDecisionToBranch_VerifyReturnedProcess()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = returnedProcess.GetOutgoingLinkForShape(preconditionTask.Id);

            // Determine the branch endpoint
            var branchEndPoint = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Add decision point with branch after precondition
            returnedProcess.AddUserDecisionPointWithBranchAfterShape(preconditionTask.Id, preconditionOutgoingLink.Orderindex + 1, branchEndPoint.Id);

            // Add branch to decision point with task
            var newUserTask = returnedProcess.AddBranchWithUserAndSystemTaskToUserDecisionPoint(preconditionTask.Id, preconditionOutgoingLink.Orderindex + 1, branchEndPoint.Id);

            var newSystemTask = returnedProcess.GetNextShape(newUserTask);

            // Add decision point with branch to end
            returnedProcess.AddUserDecisionPointWithBranchAfterShape(newSystemTask.Id, preconditionOutgoingLink.Orderindex + 1, branchEndPoint.Id);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Add a user task after a merge point and verify that the returned process" +
                     "has the new user task and the correct properties.")]
        public void AddUserTaskAfterMergePoint_VerifyReturnedProcess()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find the end shape
            var endShape = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Find the incoming link for the end shape
            var endIncomingLink = returnedProcess.GetIncomingLinkForShape(endShape.Id);

            Assert.IsNotNull(endIncomingLink, "Process link was not found.");

            var newUserTask = returnedProcess.AddUserAndSystemTask(endIncomingLink);

            // Find the precondition task
            var precondition = returnedProcess.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find the outgoing link for the precondition shape
            var preconditionOutgoingLink = returnedProcess.GetOutgoingLinkForShape(precondition.Id);

            Assert.IsNotNull(preconditionOutgoingLink, "Process link was not found.");

            // Add user decision with branch and merge point after precondition
            returnedProcess.AddUserDecisionPointWithBranchAfterShape(precondition.Id, preconditionOutgoingLink.Orderindex + 1, newUserTask.Id);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Add a user decision point after a merge point and verify that the returned process" +
                     "has the new decision point with branch and 2 new user tasks and has correct properties.")]
        public void AddUserDecisionPointAfterMergePoint_VerifyReturnedProcess()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var preconditionOutgoingLink = returnedProcess.GetOutgoingLinkForShape(preconditionTask.Id);

            // Determine the branch endpoint
            var branchEndPoint = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Add Decision point with branch to end
            var userDecisionPoint = returnedProcess.AddUserDecisionPointWithBranchAfterShape(preconditionTask.Id, preconditionOutgoingLink.Orderindex + 1, branchEndPoint.Id);

            // Add decision point before decision point; will have same branch order index as previous added branch
            returnedProcess.AddUserDecisionPointWithBranchBeforeShape(userDecisionPoint.Id, preconditionOutgoingLink.Orderindex + 1);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Add an include to a user task and verify that the returned process contains the added include.")]
        public void AddIncludeToUserTask_VerifyReturnedProcess()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Modify default process Name
            returnedProcess.Name = RandomGenerator.RandomValueWithPrefix("returnedProcess", 4);
            returnedProcess.ArtifactPathLinks[0].Name = returnedProcess.Name;

            // Create and publish process artifact to be used as include; enable recursive delete flag
            var includedProcessArtifact = _storyteller.CreateAndSaveProcessArtifact(_project, BaseArtifactType.Process, _user);
            includedProcessArtifact.Publish(_user);
            _deleteChildren = true;

            // Add include to default user task
            returnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName).AddAssociatedArtifact(includedProcessArtifact);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Add an include to system task and verify that the returned process contains the added include.")]
        public void AddIncludeToSystemTask_VerifyReturnedProcess()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Modify default process Name
            returnedProcess.Name = RandomGenerator.RandomValueWithPrefix("returnedProcess", 4);
            returnedProcess.ArtifactPathLinks[0].Name = returnedProcess.Name;

            // Create and publish process artifact to be used as include; enable recursive delete flag
            var includedProcessArtifact = _storyteller.CreateAndSaveProcessArtifact(_project, BaseArtifactType.Process, _user);
            includedProcessArtifact.Publish(_user);
            _deleteChildren = true;

            // Add include to default user task
            returnedProcess.GetProcessShapeByShapeName(Process.DefaultSystemTaskName).AddAssociatedArtifact(includedProcessArtifact);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Delete an include from a user task and verify that the returned process does not contain the include.")]
        public void DeleteIncludeFromUserTask_VerifyReturnedProcess()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Modify default process Name
            returnedProcess.Name = RandomGenerator.RandomValueWithPrefix("returnedProcess", 4);
            returnedProcess.ArtifactPathLinks[0].Name = returnedProcess.Name;

            // Create and publish process artifact to be used as include; enable recursive delete flag
            var includedProcessArtifact = _storyteller.CreateAndSaveProcessArtifact(_project, BaseArtifactType.Process, _user);
            includedProcessArtifact.Publish(_user);
            _deleteChildren = true;

            // Add include to default user task
            returnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName).AddAssociatedArtifact(includedProcessArtifact);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(returnedProcess, _storyteller, _user);

            // Get the process using GetProcess
            var processReturnedFromGet = _storyteller.GetProcess(_user, returnedProcess.Id);

            // Remove the include from the default user task
            processReturnedFromGet.GetProcessShapeByShapeName(Process.DefaultUserTaskName).AssociatedArtifact = null;

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(processReturnedFromGet, _storyteller, _user);
        }

        [TestCase((uint)4096, "4KB_File.jpg", "application/json;charset=utf-8")]
        [Description("Upload an Image file to Default Precondition and verify returned process model")]
        public void UploadImageToDefaultPrecondition_VerifyImage(uint fileSize, string fakeFileName, string fileType)
        {
            // Create a Process artifact
            var addedProcessArtifact = _storyteller.CreateAndSaveProcessArtifact(project: _project, user: _user, artifactType: BaseArtifactType.Process);

            // Get default process
            var returnedProcess = _storyteller.GetProcess(_user, addedProcessArtifact.Id);

            // Setup: create a file with a random byte array.
            var file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Uploading the file
            var uploadResult = _storyteller.UploadFile(_user, file, DateTime.Now.AddDays(1));

            var deserialzedUploadResult = Deserialization.DeserializeObject<UploadResult>(uploadResult);

            // Update the default precondition properties in the retrieved process model with Guid and UriToFile
            var defaultPreconditionShape = returnedProcess.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            defaultPreconditionShape.PropertyValues[PropertyTypeName.associatedImageUrl.ToString()].Value = deserialzedUploadResult.UriToFile;
            defaultPreconditionShape.PropertyValues[PropertyTypeName.imageId.ToString()].Value = deserialzedUploadResult.Guid;

            // Save the process with the updated properties
            returnedProcess = _storyteller.UpdateProcess(_user, returnedProcess);
            defaultPreconditionShape = returnedProcess.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Publish the process and enables recursive delete flag
            addedProcessArtifact.Publish(_user);
            _deleteChildren = true;

            // Assert that the Default Precondition SystemTask contains value
            var updatedAssociatedImageUrl = defaultPreconditionShape.PropertyValues[PropertyTypeName.associatedImageUrl.ToString()].Value.ToString();
            var updatedImageId = defaultPreconditionShape.PropertyValues[PropertyTypeName.imageId.ToString()].Value.ToString();

            Assert.That(updatedAssociatedImageUrl.Contains("/svc/components/RapidReview/diagram/image/"), "The updated associatedImageUri of The precondition contains {0}", updatedAssociatedImageUrl);

            Assert.IsNotNull(updatedImageId, "The updated ImageId of The precondition contains nothing");

            // Assert that there is a row of data available on image table
            VerifyImageRowsFromDb(updatedImageId);
        }

        [Test]
        [Description("Add a new system decision point to the default process. The new system decision point added after the default UT.")]
        public void AddSystemDecisionWithBranchAfterDefaultUserTask_VerifyReturnedProcess()
        {
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find the default UserTask
            var defaultUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the target SystemTask
            var targetSystemTask = process.GetProcessShapeByShapeName(Process.DefaultSystemTaskName);

            // Find the branch end point for the system decision point
            var branchEndPoint = process.GetProcessShapeByShapeName(Process.EndName);
            
            // Find the outgoing process link from the default UserTask
            var defaultUserTaskOutgoingProcessLink = process.GetOutgoingLinkForShape(defaultUserTask.Id);

            // Add System Decision point with branch to end
            process.AddSystemDecisionPointWithBranchBeforeSystemTask(targetSystemTask.Id, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint.Id);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, _storyteller, _user);
        }

        [Test]
        [Description("Add two new system decision points to the default process. The two system decision points added one after the other after the default UT.")]
        public void AddSystemDecisionsWithBranchAfterDefaultUserTask_VerifyReturnedProcess()
        {
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find the default UserTask
            var defaultUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the default SystemTask
            var defaultSystemTask = process.GetProcessShapeByShapeName(Process.DefaultSystemTaskName);

            // Find the branch end point for system decision points
            var branchEndPoint = process.GetProcessShapeByShapeName(Process.EndName);
            
            // Find the outgoing process link from the default UserTask
            var defaultUserTaskOutgoingProcessLink = process.GetOutgoingLinkForShape(defaultUserTask.Id);

            // Add System Decision point with branch to end
            process.AddSystemDecisionPointWithBranchBeforeSystemTask(defaultUserTaskOutgoingProcessLink.DestinationId, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint.Id);

            // Get the updated default outgoing process link from the  default UserTask
            defaultUserTaskOutgoingProcessLink = process.GetIncomingLinkForShape(defaultSystemTask.Id);

            // Add System Decision point with branch to end
            process.AddSystemDecisionPointWithBranchBeforeSystemTask(defaultUserTaskOutgoingProcessLink.DestinationId, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint.Id);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, _storyteller, _user);
        }

        [Test]
        [Description("Add a new system decision point to the default process. The system decision point gets added with two additonal branches after the default UT.")]
        public void AddSystemDecisionWithBranchesAfterDefaultUserTask_VerifyReturnedProcess()
        {
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find the default UserTask
            var defaultUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the target SystemTask
            var targetSystemTask = process.GetProcessShapeByShapeName(Process.DefaultSystemTaskName);

            // Find the branch end point for system decision points
            var branchEndPoint = process.GetProcessShapeByShapeName(Process.EndName);

            // Find the outgoing process link from the default UserTask
            var defaultUserTaskOutgoingProcessLink = process.GetOutgoingLinkForShape(defaultUserTask.Id);

            // Add System Decision point with branch to end
            var systemDecisionPoint = process.AddSystemDecisionPointWithBranchBeforeSystemTask(targetSystemTask.Id, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint.Id);
            
            // Add additonal branch to the System Decision point
            process.AddBranchWithSystemTaskToSystemDecisionPoint(systemDecisionPoint.Id,defaultUserTaskOutgoingProcessLink.Orderindex+2,branchEndPoint.Id);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, _storyteller, _user);
        }

        [Test]
        [Description("Add a new system decision point to the default process. The system decision point gets added with an additonal branch which also contains a system decision point."
            )]
        public void AddSystemDecisionWithBranchWithSystemDecisionAfterDefaultUserTask_VerifyReturnedProcess()
        {
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find the default UserTask
            var defaultUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the target SystemTask
            var targetSystemTask = process.GetProcessShapeByShapeName(Process.DefaultSystemTaskName);

            // Find the branch end point for system decision points
            var branchEndPoint = process.GetProcessShapeByShapeName(Process.EndName);

            // Find the outgoing process link from the default UserTask
            var defaultUserTaskOutgoingProcessLink = process.GetOutgoingLinkForShape(defaultUserTask.Id);

            // Add System Decision point with branch to end
            process.AddSystemDecisionPointWithBranchBeforeSystemTask(targetSystemTask.Id, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint.Id);

            // Get the system task shape on the second branch for adding the additional System Decision Point
            var systemTaskOnTheSecondBranch = process.GetProcessShapeById(process.Links.Find(l => l.Orderindex.Equals(defaultUserTaskOutgoingProcessLink.Orderindex + 1)).DestinationId);

            // Add the System Decision point with branch to end
            process.AddSystemDecisionPointWithBranchBeforeSystemTask(systemTaskOnTheSecondBranch.Id, defaultUserTaskOutgoingProcessLink.Orderindex + 2, branchEndPoint.Id);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, _storyteller, _user);
        }

        [Test]
        [Description("Add a new system decision point to the default process. The system decision point gets added with two additonal branches: one contains a system decision point along with branches and system tasks and the other contain just a system task"
            )]
        public void AddSystemDecisionWithBranchesWithSystemDecisionAfterDefaultUserTask_VerifyReturnedProcess()
        {
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find the default UserTask
            var defaultUserTask = process.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the target SystemTask
            var targetSystemTask = process.GetProcessShapeByShapeName(Process.DefaultSystemTaskName);

            // Find the branch end point for system decision points
            var branchEndPoint = process.GetProcessShapeByShapeName(Process.EndName);

            // Find the outgoing process link from the default UserTask
            var defaultUserTaskOutgoingProcessLink = process.GetOutgoingLinkForShape(defaultUserTask.Id);

            // Add System Decision point with branch to end
            var rootSystemDecisionPoint = process.AddSystemDecisionPointWithBranchBeforeSystemTask(targetSystemTask.Id, defaultUserTaskOutgoingProcessLink.Orderindex + 1, branchEndPoint.Id);

            // Get the system task shape on the second branch for adding the additional System Decision Point
            var systemTaskOnTheSecondBranch = process.GetProcessShapeById(process.Links.Find(l => l.Orderindex.Equals(defaultUserTaskOutgoingProcessLink.Orderindex + 1)).DestinationId);

            // Add System Decision point with branch to end
            process.AddSystemDecisionPointWithBranchBeforeSystemTask(systemTaskOnTheSecondBranch.Id, defaultUserTaskOutgoingProcessLink.Orderindex + 2, branchEndPoint.Id);

            process.AddBranchWithSystemTaskToSystemDecisionPoint(rootSystemDecisionPoint.Id, defaultUserTaskOutgoingProcessLink.Orderindex + 3, branchEndPoint.Id);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, _storyteller, _user);
        }

        /// <summary>
        /// Verifies that number of row from Image table match with expected value
        /// </summary>
        /// <param name="imageId">the image Id that can be used to find image from the image table</param>
        private static void VerifyImageRowsFromDb(string imageId)
        {
            const int expectedImagerowCount = 1;
            var resultCount = 0;

            using (var database = DatabaseFactory.CreateDatabase())
            {
                const string query = "SELECT COUNT (*) as counter FROM dbo.Images WHERE ImageId = @Image_Id;";
                Logger.WriteDebug("Running: {0}", query);
                using (var cmd = database.CreateSqlCommand(query))
                {
                    database.Open();
                    cmd.Parameters.Add("@Image_Id", SqlDbType.Int).Value = imageId;
                    cmd.CommandType = CommandType.Text;

                    try
                    {
                        SqlDataReader reader;
                        using (reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                            }
                            resultCount = Int32.Parse(reader["counter"].ToString(), CultureInfo.InvariantCulture);
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        Logger.WriteError("Upload Image didn't create a data entry. Exception details = {0}", ex);
                    }
                }
            }
            Assert.That(resultCount.Equals(expectedImagerowCount), "The total number of rows for the uploaded image is {0} but we expected {1}", resultCount, expectedImagerowCount);
        }

        #endregion Tests
    }
}
