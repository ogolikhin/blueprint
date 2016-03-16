using System;
using System.Globalization;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using Utilities;
using Utilities.Factories;
using System.Data.SqlClient;
using Common;
using System.Data;

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
        private bool deleteChildren = false;

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
                    _storyteller.DeleteProcessArtifact(artifact, _user, deleteChildren: deleteChildren);
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
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
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
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
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
            var processLink = returnedProcess.GetOutgoingLinkForShape(preconditionTask.Id);
                
            Assert.IsNotNull(processLink, "Process link was not found.");

            // Add user/system Task immediately after the precondition
            returnedProcess.AddUserTask(processLink);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
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
            var processLink = returnedProcess.GetIncomingLinkForShape(endShape.Id);

            Assert.IsNotNull(processLink, "Process link was not found.");

            // Add a user/system task immediately before the end shape
            returnedProcess.AddUserTask(processLink);

            // Updatea and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Add a user task after an existing user task and verify that the returned process" +
                     "has the new user task in the correct position and has the correct properties.")]
        public void AddUserTaskAfterUserTask_VerifyReturnedProcess()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Get the default user task
            var defaultUserTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the incoming link for the end shape
            var processLink = returnedProcess.GetOutgoingLinkForShape(defaultUserTask.Id);

            Assert.IsNotNull(processLink, "Process link was not found.");

            // Add a user/system task immediately before the end shape
            returnedProcess.AddUserTask(processLink);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
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
            var processLink = returnedProcess.GetOutgoingLinkForShape(preconditionTask.Id);

            var branchEndPoint = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Add Decision point with branch to end
            returnedProcess.AddUserDecisionPointWithBranchAfterShape(preconditionTask.Id, processLink.Orderindex + 1, branchEndPoint.Id);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
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
            var processLink = returnedProcess.GetIncomingLinkForShape(endShape.Id);

            // Add Decision point with branch and 2 user tasks
            returnedProcess.AddUserDecisionPointWithBranchBeforeShape(endShape.Id, processLink.Orderindex + 1);

            // Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Add a new user task before a user decision point and verify that the returned process" +
                     "has the new user task in the correct position and has the correct properties.")]
        public void AddUserTaskBeforeUserDecision_VerifyReturnedProcess()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find the default user task
            var defaultUserTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find incoming process link for the default user task shape
            var processLink = returnedProcess.GetIncomingLinkForShape(defaultUserTask.Id);

            // Add Decision point with branch before default user task
            var userDecisionPoint = returnedProcess.AddUserDecisionPointWithBranchBeforeShape(defaultUserTask.Id, processLink.Orderindex + 1);

            // Find the incoming link for the user decision task
             processLink = returnedProcess.GetIncomingLinkForShape(userDecisionPoint.Id);

            // Add a user/system task immediately before the user decision point
            returnedProcess.AddUserTask(processLink);

            // Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Add new user decision point between 2 user tasks and verify that the returned process" +
                     "has the new user decision and branch in the correct position and has the correct properties.")]
        public void AddUserDecisionBetweenTwoUserTasks_VerifyReturnedProcess()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find the end shape
            var defaultUserTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the outgoing link for the default user task
            var processLink = returnedProcess.GetOutgoingLinkForShape(defaultUserTask.Id);

            // Add a user/system task immediately bafter the default user task
            returnedProcess.AddUserTask(processLink);

            // Add a user decision point between 2 user/system tasks
            returnedProcess.AddUserDecisionPointWithBranchAfterShape(defaultUserTask.Id, processLink.Orderindex + 1);

            // Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
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
            var processLink = returnedProcess.GetOutgoingLinkForShape(preconditionTask.Id);

            var branchEndPoint = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Add Decision point with branch (make order index to to give room for internal branch
            returnedProcess.AddUserDecisionPointWithBranchAfterShape(preconditionTask.Id, processLink.Orderindex + 1, branchEndPoint.Id);

            // Find the default user task
            var defaultUserTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the outgoing link for the default user task
            processLink = returnedProcess.GetOutgoingLinkForShape(defaultUserTask.Id);

            // Add a user decision point after the default user task and before the end point
            returnedProcess.AddUserDecisionPointWithBranchAfterShape(defaultUserTask.Id, processLink.Orderindex + 1, branchEndPoint.Id);

            // Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
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
            var processLink = returnedProcess.GetOutgoingLinkForShape(preconditionTask.Id);

            // Find the endpoint for the new branch
            var branchEndPoint = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Add decision point with branch to end
            returnedProcess.AddUserDecisionPointWithBranchAfterShape(preconditionTask.Id, processLink.Orderindex + 1, branchEndPoint.Id);

            // Add branch to decision point with task
            returnedProcess.AddBranchWithUserTaskToUserDecisionPoint(preconditionTask.Id, processLink.Orderindex + 1, branchEndPoint.Id); 

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Add a new user decision point to a branch and verify that the returned process" +
                     "conatins the new decision point, branch and 2 user tasks and has the correct properties.")]
        public void AddUserDecisionToBranch_VerifyReturnedProcess()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find precondition task
            var preconditionTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition
            var processLink = returnedProcess.GetOutgoingLinkForShape(preconditionTask.Id);

            // Determine the branch endpoint
            var branchEndPoint = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Add branch to decision point with task
            var userTask = returnedProcess.AddBranchWithUserTaskToUserDecisionPoint(preconditionTask.Id, processLink.Orderindex + 1, branchEndPoint.Id);

            // Add decision point with branch to end
            returnedProcess.AddUserDecisionPointWithBranchAfterShape(userTask.Id, processLink.Orderindex + 1, branchEndPoint.Id);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Add a user task after a merge point and verify that the returned process" +
                     "has the new user task and the correct properties.")]
        public void AddUserTaskAfterMergePoint_VerifyReturnedProcess()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user);

            // Find the default user task
            var defaultUserTask = returnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName);

            // Find the outgoing link for the default user task
            var processLink = returnedProcess.GetOutgoingLinkForShape(defaultUserTask.Id);

            Assert.IsNotNull(processLink, "Process link was not found.");

            // Add a user/system task immediately before the end shape
            var userTask = returnedProcess.AddUserTask(processLink);

            // Add branch with merge point between 2 user tasks
            returnedProcess.AddUserDecisionPointWithBranchBeforeShape(defaultUserTask.Id, processLink.Orderindex + 1, userTask.Id);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
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
            var processLink = returnedProcess.GetOutgoingLinkForShape(preconditionTask.Id);

            // Determine the branch endpoint
            var branchEndPoint = returnedProcess.GetProcessShapeByShapeName(Process.EndName);

            // Add Decision point with branch to end
            var userDecisionPoint = returnedProcess.AddUserDecisionPointWithBranchAfterShape(preconditionTask.Id, processLink.Orderindex + 1, branchEndPoint.Id);

            // Add decision point before decision point; will have same branch order index as previous added branch
            returnedProcess.AddUserDecisionPointWithBranchBeforeShape(userDecisionPoint.Id, processLink.Orderindex + 1);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
        }

        [TestCase]
        [Description("Add an include to a user task and verify that the returned process contains the added include.")]
        public void AddIncludeToUserTask_VerifyReturnedProcess()
        {
            // Create and get the default process
            var returnedProcess = StorytellerTestHelper.CreateAndGetDefaultProcess(_storyteller, _project, _user); ;

            // Modify default process Name
            returnedProcess.Name = RandomGenerator.RandomValueWithPrefix("returnedProcess", 4);
            returnedProcess.ArtifactPathLinks[0].Name = returnedProcess.Name;

            // Create and publish process artifact to be used as include; enable recursive delete flag
            var includedProcessArtifact = _storyteller.CreateProcessArtifact(_project, BaseArtifactType.Process, _user);
            includedProcessArtifact.Publish(_user);
            deleteChildren = true;

            // Add include to default user task
            returnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName).AddAssociatedArtifact(includedProcessArtifact);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
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
            var includedProcessArtifact = _storyteller.CreateProcessArtifact(_project, BaseArtifactType.Process, _user);
            includedProcessArtifact.Publish(_user);
            deleteChildren = true;

            // Add include to default user task
            returnedProcess.GetProcessShapeByShapeName(Process.DefaultSystemTaskName).AddAssociatedArtifact(includedProcessArtifact);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);
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
            var includedProcessArtifact = _storyteller.CreateProcessArtifact(_project, BaseArtifactType.Process, _user);
            includedProcessArtifact.Publish(_user);
            deleteChildren = true;

            // Add include to default user task
            returnedProcess.GetProcessShapeByShapeName(Process.DefaultUserTaskName).AddAssociatedArtifact(includedProcessArtifact);

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(returnedProcess, _storyteller, _user);

            // Get the process using GetProcess
            var processReturnedFromGet = _storyteller.GetProcess(_user, returnedProcess.Id);

            // Remove the include from the default user task
            processReturnedFromGet.GetProcessShapeByShapeName(Process.DefaultUserTaskName).AssociatedArtifact = null;

            // Update and Verify the modified process
            StorytellerTestHelper.UpdateAndVerifyProcess(processReturnedFromGet, _storyteller, _user);
        }

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)4096, "4KB_File.jpg", "application/json;charset=utf-8")]
        [Description("Upload an Image file to Default Precondition and verify returned process model")]
        public void UploadImageToDefaultPrecondition_VerifyImage(uint fileSize, string fakeFileName, string fileType)
        {
            // Create a Process artifact
            var addedProcessArtifact = _storyteller.CreateProcessArtifact(project: _project, user: _user, artifactType: BaseArtifactType.Process);

            // Get default process
            var returnedProcess = _storyteller.GetProcess(_user, addedProcessArtifact.Id);

            // Setup: create a file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            /// Uploading the file
            var uploadResult = _storyteller.UploadFile(_user, file, DateTime.Now.AddDays(1));

            var deserialzedUploadResult = Deserialization.DeserializeObject<Storyteller.UploadResult>(uploadResult);

            // Update the default precondition properties in the retrieved process model with guid and uriToFile
            var defaultPreconditionShape = returnedProcess.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            defaultPreconditionShape.PropertyValues[PropertyTypeName.associatedImageUrl.ToString()].Value = deserialzedUploadResult.uriToFile;
            defaultPreconditionShape.PropertyValues[PropertyTypeName.imageId.ToString()].Value = deserialzedUploadResult.guid;

            // Save the process with the updated properties; enable recursive delete flag
            returnedProcess = _storyteller.UpdateProcess(_user, returnedProcess);
            defaultPreconditionShape = returnedProcess.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            addedProcessArtifact.Publish(_user);
            deleteChildren = true;

            // Assert that the Default Precondition SystemTask contains value
            string updatedAssociatedImageUrl = defaultPreconditionShape.PropertyValues[PropertyTypeName.associatedImageUrl.ToString()].Value.ToString();
            string updatedImageId = defaultPreconditionShape.PropertyValues[PropertyTypeName.imageId.ToString()].Value.ToString();

            Assert.That(updatedAssociatedImageUrl.Contains("/svc/components/RapidReview/diagram/image/"), "The updated associatedImageUri of The precondition contains {0}", updatedAssociatedImageUrl);

            Assert.IsNotNull(updatedImageId, "The updated ImageId of The precondition contains nothing");

            // Assert that there is a row of data available on image table
            int expectedImageRow = 1;
            VerifyImageRowsFromDB(expectedImageRow, updatedImageId);
        }

        /// <summary>
        /// Verifies that number of row from Image table match with expected value
        /// </summary>
        /// <param name="expectedCount">the expected total count of images from the image table</param>
        /// <param name="imageId">the image Id that can be used to find image from the image table</param>
        private static void VerifyImageRowsFromDB(int expectedCount, string imageId)
        {
            int resultCount = 0;
            string query = null;
            SqlDataReader reader;

            using (IDatabase database = DatabaseFactory.CreateDatabase())
            {
                query = "SELECT COUNT (*) as counter FROM dbo.Images WHERE ImageId = @Image_Id;";
                Logger.WriteDebug("Running: {0}", query);
                using (SqlCommand cmd = database.CreateSqlCommand(query))
                {
                    database.Open();
                    cmd.Parameters.Add("@Image_Id", SqlDbType.Int).Value = imageId;
                    cmd.CommandType = CommandType.Text;

                    try
                    {
                        using (reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                            }
                            resultCount = Int32.Parse(reader["counter"].ToString(), CultureInfo.InvariantCulture);
                        }
                    }
                    catch (System.InvalidOperationException ex)
                    {
                        Logger.WriteError("Upload Image didn't create a data entry. Exception details = {0}", ex);
                    }
                }
            }
            Assert.That(resultCount.Equals(expectedCount), "The total number of rows for the uploaded image is {0} but we expected {1}", resultCount, expectedCount);
        }

        #endregion Tests
    }
}
