using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TestCommon;
using Utilities;
using Model.ArtifactModel.Impl;
using Model.StorytellerModel.Enums;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class UpdateNovaProcessTests : TestBase
    {
        private IUser _user;
        private IProject _project;
        private List<IProject> _allProjects = null;
        private const string INVALID_PROCESS_MESSAGE = "The artifact cannot be saved. Please ensure all values are correct.";

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _allProjects = ProjectFactory.GetAllProjects(_user);
            _project = _allProjects.First();
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            if (Helper?.Storyteller != null)
            {
                foreach (var novaProcess in Helper.Storyteller.NovaProcesses)
                {
                    Helper.Storyteller.DeleteNovaProcessArtifact(_user, novaProcess);
                }
            }

            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region Tests

        [TestCase]
        [TestRail(211705)]
        [Description("Update the process type of the process and verify that the returned Nova process " +
                     "has the updated process type. This verifies that the process type toggle in the" +
                     "process diagram is working.")]
        public void UpdateNovaProcess_ModifyProcessType_VerifyReturnedNovaProcess()
        {
            // Setup:
            // Create and get the default Nova process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(Helper.Storyteller, _project, _user);
            var process = novaProcess.Process;
            var processType = process.ProcessType;

            Assert.IsTrue(processType == ProcessType.BusinessProcess, "Process type should be Business Process but is not!");

            // Modify default process Type
            process.ProcessType = ProcessType.UserToSystemProcess;

            // Execute & Verify:
            // Update and Verify the modified process
            var updatedNovaProcess = StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess, Helper.Storyteller, _user);

            Assert.IsTrue(updatedNovaProcess.Process.ProcessType == ProcessType.UserToSystemProcess,
                "Process Type was not updated to UserToSystemProcess!");
        }

        [TestCase]
        [TestRail(211706)]
        [Description("Add a new user task after the precondition and verify that the returned Nova process " +
                     "has the new user task in the correct position and has the correct properties.")]
        public void UpdateNovaProcess_AddUserTaskAfterPrecondition_VerifyReturnedNovaProcess()
        {
            // Setup:
            // Create and get the default Nova process
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(Helper.Storyteller, _project, _user);
            var process = novaProcess.Process;

            // Find precondition task
            var preconditionTask = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition task
            var preconditionOutgoingLink = process.GetOutgoingLinkForShape(preconditionTask);
                
            Assert.IsNotNull(preconditionOutgoingLink, "Process link was not found.");

            // Add user/system Task immediately after the precondition
            process.AddUserAndSystemTask(preconditionOutgoingLink);

            // Execute & Verify:
            // Update and Verify the modified Nova process
            StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess, Helper.Storyteller, _user);
        }

        #endregion Tests

        #region Process Validation Tests

        [Category(Categories.CustomData)]
        [TestCase("Std-Choice-Required-HasDefault")]
        [TestCase("Std-Date-Required-HasDefault")]
        [TestCase("Std-Date-Required-Validated-Min-Max-HasDefault")]
        [TestCase("Std-Number-Required-Validated-DecPlaces-Min-Max-HasDefault")]
        [TestCase("Std-Text-Required-HasDefault")]
        [TestCase("Std-Text-Required-RT-Multi-HasDefault")]
        [TestCase("Std-User-Required-HasDefault-User")]
        [TestRail(234610)]
        [Description("Create Process, try to update it - set user task required property with default value to null, " +
                     "verify returned error message and shape Id.")]
        public void UpdateNovaProcess_SetRequiredShapePropertyToNull_VerifyReturnedError(string customPropertyName)
        {
            // Setup:
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(Helper.Storyteller, _project, _user);
            var process = novaProcess.Process;
            int shapeIndex = 2;

            var userTaskSubArtifact = Helper.ArtifactStore.GetSubartifact(_user, process.Id, process.Shapes[shapeIndex].Id);
            var customPropertyNullValue = ArtifactStoreHelper.SetCustomPropertyToNull(userTaskSubArtifact.CustomPropertyValues,
                customPropertyName);

            userTaskSubArtifact = TestHelper.CreateSubArtifactChangeSet(userTaskSubArtifact, customPropertyNullValue);
            novaProcess.SubArtifacts = new List<NovaSubArtifact> { userTaskSubArtifact };

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess,
                Helper.Storyteller, _user), "Process Update should throw 409 exception when shapes have invalid Custom Property values.");

            // Verify:
            TestHelper.ValidateProcessValidationError(ex.RestResponse, InternalApiErrorCodes.ProcessValidationFailed,
                INVALID_PROCESS_MESSAGE, new List<int> { process.Shapes[shapeIndex].Id });
        }

        [Category(Categories.CustomData)]
        [TestCase("Std-Number-Required-Validated-DecPlaces-Min-Max-HasDefault", 14)]
        [TestCase("Std-Date-Required-Validated-Min-Max-HasDefault", 14)]
        [TestRail(234611)]
        [Description("Create Process, try to update it - set user task required, validated property with default value " +
                     "to invalid value, verify returned error message and shape Id.")]
        public void UpdateNovaProcess_SetValidatedRequiredShapePropertyToInvalidValue_VerifyReturnedError(string customPropertyName, int invalidValue)
        {
            // Setup:
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(Helper.Storyteller, _project, _user);
            var process = novaProcess.Process;

            int shapeIndex = 2;

            var userTaskSubArtifact = Helper.ArtifactStore.GetSubartifact(_user, process.Id, process.Shapes[shapeIndex].Id);
            var customPropertyInvalidValue = ArtifactStoreHelper.SetCustomProperty(userTaskSubArtifact.CustomPropertyValues,
                _project, PropertyPrimitiveType.Number, customPropertyName, invalidValue);

            userTaskSubArtifact = TestHelper.CreateSubArtifactChangeSet(userTaskSubArtifact, customPropertyInvalidValue);
            novaProcess.SubArtifacts = new List<NovaSubArtifact> { userTaskSubArtifact };

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess, Helper.Storyteller, _user),
                "Process Update should throw 409 exception when shapes have invalid Custom Property values.");

            // Verify:
            TestHelper.ValidateProcessValidationError(ex.RestResponse, InternalApiErrorCodes.ProcessValidationFailed,
                INVALID_PROCESS_MESSAGE, new List<int> { process.Shapes[shapeIndex].Id });
        }

        [Category(Categories.CustomData)]
        [TestCase]
        [TestRail(234612)]
        [Description("Create Process, try to update it - set first shape's required, validated property with default value " +
                     "to null, set second shape's required, validated property with default value to invalid value, " +
                     "set third shape's required, validated property with default value to valid value, verify returned error message and shape Ids.")]
        public void UpdateNovaProcess_SetSomeShapesPropertyToInvalidValueOtherToValidValues_VerifyReturnedError()
        {
            // Setup:
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcess(Helper.Storyteller, _project, _user);
            var process = novaProcess.Process;

            string customPropertyName = "Std-Number-Required-Validated-DecPlaces-Min-Max-HasDefault";
            
            const int invalidNumber = 14;
            const int validNumber = 2;

            List<NovaSubArtifact> shapeSubArtifacts = new List<NovaSubArtifact>();
            for (int i = 0; i < 3; i++)
            {
                
                var shapeSubArtifact = Helper.ArtifactStore.GetSubartifact(_user, process.Id, process.Shapes[i].Id);
                shapeSubArtifacts.Add(shapeSubArtifact);
            }

            var customPropertyNullValue = ArtifactStoreHelper.SetCustomPropertyToNull(shapeSubArtifacts[0].CustomPropertyValues,
                customPropertyName);
            shapeSubArtifacts[0] = TestHelper.CreateSubArtifactChangeSet(shapeSubArtifacts[0], customPropertyNullValue);

            var customPropertyNewValue = ArtifactStoreHelper.SetCustomProperty(shapeSubArtifacts[1].CustomPropertyValues,
                _project, PropertyPrimitiveType.Number, customPropertyName, invalidNumber);
            shapeSubArtifacts[1] = TestHelper.CreateSubArtifactChangeSet(shapeSubArtifacts[1], customPropertyNewValue);

            customPropertyNewValue = ArtifactStoreHelper.SetCustomProperty(shapeSubArtifacts[2].CustomPropertyValues,
                _project, PropertyPrimitiveType.Number, customPropertyName, validNumber);
            shapeSubArtifacts[2] = TestHelper.CreateSubArtifactChangeSet(shapeSubArtifacts[2], customPropertyNewValue);

            novaProcess.SubArtifacts = new List<NovaSubArtifact> { shapeSubArtifacts[0],
                shapeSubArtifacts[1], shapeSubArtifacts[2] };

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => StorytellerTestHelper.UpdateAndVerifyNovaProcess(novaProcess, Helper.Storyteller, _user),
                "Process Update should throw 409 exception when shapes have invalid Custom Property values.");

            // Verify:
            TestHelper.ValidateProcessValidationError(ex.RestResponse, InternalApiErrorCodes.ProcessValidationFailed,
                INVALID_PROCESS_MESSAGE, new List<int> { process.Shapes[0].Id, process.Shapes[1].Id });
        }

        #endregion Process Validation Tests
    }
}
