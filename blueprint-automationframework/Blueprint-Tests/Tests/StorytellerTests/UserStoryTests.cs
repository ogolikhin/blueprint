using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.ModelHelpers;
using Model.StorytellerModel;
using Model.StorytellerModel.Enums;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TestCommon;
using Utilities;
using Utilities.Facades;

namespace StorytellerTests
{
    public class UserStoryTests : TestBase
    {
        private const string SVC_USERSTORIES_PATH = RestPaths.Svc.Components.Storyteller.Projects_id_.Processes_id_.USERSTORIES;
        private const string SVC_USERSTORYARTIFACTTYPE_PATH = RestPaths.Svc.Components.Storyteller.Projects_id_.ArtifactTypes.USER_STORY;

        private const int NumberOfAdditionalUserTasks = 5;

        private IUser _user;
        private IProject _project;
        private int _defaultUserTaskCount = 1;

        #region SetUp and Teardown

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

        #endregion SetUp and TearDown

        #region Tests

        [TestCase]
        [Description("Verify that total number of generated or updated user stories are equal to total number of user tasks for the default process")]
        public void UserStoryGenerationProcessWithDefaultUserTask_NumberOfUserTasksAndGeneratedUserStoriesAreEqual()
        {
            // Setup: Create and publish a nova process, and find number of UserTasks from the published Process
            // Verify that the number of UserTasks from the default process is same as expected
            var novaProcess = Helper.Storyteller.CreateAndPublishNovaProcessArtifact(_project, _user);
            var userTasksOnProcess = novaProcess.Process.GetProcessShapesByShapeType(ProcessShapeType.UserTask).Count;
            ValidateProcessUserTaskCount(novaProcess, _defaultUserTaskCount, userTasksOnProcess);

            // Execute: Generate User Story artfact(s) from the Process artifact
            List<IStorytellerUserStory> userStories = null;
            Assert.DoesNotThrow(() => userStories = Helper.Storyteller.GenerateUserStories(_user, novaProcess.Process),
                "'POST {0}' should return 200 OK when passed a valid process ID!",
                SVC_USERSTORIES_PATH);

            Logger.WriteDebug("The number of UserStories generated is: {0}", userStories.Count);

            // Verify: Assert that the number of UserTasks from the published Process is equal to the number of UserStoryGenerated or Updated
            Assert.AreEqual(userTasksOnProcess, userStories.Count,
                "The expected number of UserStories generated from the process is {0} but " +
                "The actual number of UserStories generated from the process is {1}.",
                userTasksOnProcess, userStories.Count);
        }

        [TestCase]
        [Description("Verify the contents of generated or updated user stories")]
        public void UserStoryGenerationProcessWithDefaultUserTask_VerifyingContents()
        {
            // Setup: Create and publish a nova process
            var novaProcess = Helper.Storyteller.CreateAndPublishNovaProcessArtifact(_project, _user);

            // Execute:
            // Checking Object: The Process that contains shapes including user task shapes
            // Test Object: Generated User Stories from the Process
            List<IStorytellerUserStory> userStories = null;
            Assert.DoesNotThrow(() => userStories = Helper.Storyteller.GenerateUserStories(_user, novaProcess.Process),
                "'POST {0}' should return 200 OK when passed a valid process ID!",
                SVC_USERSTORIES_PATH);

            // Verify:
            // Assert that there is only one to one maching between UserTask and generated UserStory
            ValidateGeneratedUserStories(novaProcess, userStories);
        }

        [TestCase]
        [Description("Retrieve UserStoryArtifactType if Storyteller Pack or Agile Pack is installed on the target Blueprint")]
        public void GetUserStoryArtifactType_ReceiveUserStoryArtifactType()
        {
            // Setup:

            // Execute:
            OpenApiArtifactType userStoryArtifactType = null;
            Assert.DoesNotThrow(() => userStoryArtifactType = Helper.Storyteller.GetUserStoryArtifactType(_user, _project.Id),
                "'POST {0}' should return 200 OK if Storyteller/Agile Pack is installed on the target Blueprint!",
                SVC_USERSTORYARTIFACTTYPE_PATH);

            // Verify:
            Assert.NotNull(userStoryArtifactType.Id,"UserStoryArtifactType Id is null");
            Assert.NotNull(userStoryArtifactType.Name, "UserStoryArtifactType Name is null");
        }

        [TestCase(NumberOfAdditionalUserTasks)]
        [Description("Verify that total number of generated or updated user stories are equal to total number of user tasks for the process with multi user tasks")]
        public void UserStoryGenerationProcessWithMultipleUserTasks_NumberOfUserTasksAndUserStoriesAreEqual(int iteration)
        {
            // Setup:
            // Create a nova process and get the updated nova process with additional user tasks
            // Find number of UserTasks from the published Process
            // Assert that the number of UserTasks the published process is equal to the number of UserTasks returned from GetProcess call
            int userTaskExpectedCount = iteration + _defaultUserTaskCount;

            if (userTaskExpectedCount == int.MaxValue)
            {
                throw new OverflowException("overflow exception");
            }

            var novaProcess = Helper.Storyteller.CreateAndSaveNovaProcessArtifact(_project, _user);
            var returnedNovaProcess = GetNovaProcessWithAdditionalShapes(novaProcess, iteration);
            var userTasksOnProcess = returnedNovaProcess.Process.GetProcessShapesByShapeType(ProcessShapeType.UserTask).Count;
            ValidateProcessUserTaskCount(novaProcess, userTaskExpectedCount, userTasksOnProcess);

            // Execute: 
            // Generate User Story artfact(s) from the Process artifact
            List<IStorytellerUserStory> userStories = null;
            Assert.DoesNotThrow(() => userStories = Helper.Storyteller.GenerateUserStories(_user, returnedNovaProcess.Process),
                "'POST {0}' should return 200 OK when passed a valid process ID!", SVC_USERSTORIES_PATH);

            Logger.WriteDebug("The number of UserStories generated is: {0}", userStories.Count);

            // Verify:
            // Assert that the number of UserTasks from the published Process is equal to the number of UserStoryGenerated or Updated
            ValidateGeneratedUserStories(returnedNovaProcess, userStories);
        }

        [TestCase(NumberOfAdditionalUserTasks)]
        [Description("Verify that every generated or updated user stories are mapped to user tasks for the process with multi user tasks")]
        public void UserStoryGenerationProcessWithMultipleUserTasks_UserTaskUserStoryMapping(int iteration)
        {
            // Setup: Create a nova process and get the updated nova process with additional user tasks
            var novaProcess = Helper.Storyteller.CreateAndSaveNovaProcessArtifact(_project, _user);
            var returnedNovaProcess = GetNovaProcessWithAdditionalShapes(novaProcess, iteration);

            // Execute: 
            // Test Object: Generated User Stories from the Process
            List<IStorytellerUserStory> userStories = null;
            Assert.DoesNotThrow(() => userStories = Helper.Storyteller.GenerateUserStories(_user, returnedNovaProcess.Process),
                "'POST {0}' should return 200 OK when passed a valid process ID!", SVC_USERSTORIES_PATH);

            // Verify:
            // Assert that there is one to one maching between UserTask and generated UserStory
            ValidateGeneratedUserStories(returnedNovaProcess, userStories);
        }

        [TestCase(NumberOfAdditionalUserTasks)]
        [Description("Verify that Genearate UserStories updates user stories if there are existing user stories for user tasks for the process with multi user tasks")]
        public void UserStoryGenerationProcessWithMultipleUserTasks_VerifyingUpdateFlagsForExistingUserStories(int iteration)
        {
            // Setup:
            var initialUserTaskExpectedCount = iteration / 2 + _defaultUserTaskCount;
            var additionalUserTaskExpectedCount = iteration - (iteration/2);

            // Create a nova process
            var novaProcess = Helper.Storyteller.CreateAndSaveNovaProcessArtifact(_project, _user);

            // Execute:
            // Get the updated nova process with additional UserTasks
            // (InitialUserTaskExpected - DEFAULTUSERTASK_COUNT since default UT counts), 1st version
            var returnedNovaProcessFirstBatch = GetNovaProcessWithAdditionalShapes(novaProcess, initialUserTaskExpectedCount - _defaultUserTaskCount);
            // User Stories from the updated Process (1st version)
            List<IStorytellerUserStory> userStoriesFirstBatch = null;
            Assert.DoesNotThrow(() => userStoriesFirstBatch = Helper.Storyteller.GenerateUserStories(_user, returnedNovaProcessFirstBatch.Process),
                "'POST {0}' should return 200 OK when passed a valid process ID!", SVC_USERSTORIES_PATH);

            Logger.WriteDebug("The number of user stories generated is: {0}", userStoriesFirstBatch.Count);

            // Get the updated nova process with additional UserTasks
            // (AdditionalUserTaskExpected), 2nd version
            var returnedNovaProcessSecondBatch = GetNovaProcessWithAdditionalShapes(returnedNovaProcessFirstBatch, additionalUserTaskExpectedCount);
            // User Stories from the updated Process (2nd version)
            List<IStorytellerUserStory> userStoriesSecondBatch = null;
            Assert.DoesNotThrow(() => userStoriesSecondBatch = Helper.Storyteller.GenerateUserStories(_user, returnedNovaProcessSecondBatch.Process),
                "'POST {0}' should return 200 OK when passed a valid process ID!", SVC_USERSTORIES_PATH);

            Logger.WriteDebug("The number of user stories generated or updated is: {0}", userStoriesSecondBatch.Count);

            // Verify:
            //Assert that the count of generated user stories from first batch is equal to the count of updated user stories from the second batch
            ValidateIsNewFlagForUserStories(userStoriesFirstBatch, userStoriesSecondBatch);
        }

        [TestCase]
        [Description("Publish process, generate user story for it, update Nonfunctional requirement" +
            "field with inline trace to deleted artifact. Response must return error message.")]
        public void UpdateNonfunctionalRequirementsWithInlineTrace_VerifyReturnedMessage()
        {
            // Setup:
            var novaProcess = Helper.Storyteller.CreateAndPublishNovaProcessArtifact(_project, _user);

            // Create target artifact for inline trace
            var linkedArtifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Actor);

            string inlineTraceText;
            int linkedArtifactId = linkedArtifact.Id;

            try
            {
                //get text with inline trace to the specified artifact
                inlineTraceText = GetTextForInlineTrace(new List<ArtifactWrapper>() {linkedArtifact});
            }
            finally
            {
                //delete artifact which is target for inline trace
                linkedArtifact.Delete(_user);
                linkedArtifact.Lock(_user);
                linkedArtifact.Publish(_user);
            }

            // Generate User Stories from the Process
            List<IStorytellerUserStory> userStories = null;
            Assert.DoesNotThrow(() => userStories = Helper.Storyteller.GenerateUserStories(_user, novaProcess.Process),
                "'POST {0}' should return 200 OK when passed a valid process ID!", SVC_USERSTORIES_PATH);

            // Execute:
            var updatePropertyResult = userStories[0].UpdateNonFunctionalRequirements(_user, inlineTraceText);

            // Verify:
            Assert.That(updatePropertyResult.Messages.Count() == 1,
                "Result of create inline trace must return one error message, but returns {0}",
                updatePropertyResult.Messages.Count());

            string expectedMessage = I18NHelper.FormatInvariant("Artifact with ID {0} was inaccessible. A manual trace was not created.", linkedArtifactId);
            Assert.AreEqual(expectedMessage, updatePropertyResult.Messages.ElementAt(0).Message, "Returned message must be '{0}', but it is '{1}'",
                expectedMessage, updatePropertyResult.Messages.ElementAt(0).Message);
            Assert.AreEqual(updatePropertyResult.Messages.ElementAt(0).ItemId, linkedArtifactId, "Returned ID must be {0}, but it is {1}",
                linkedArtifactId, updatePropertyResult.Messages.ElementAt(0).ItemId);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, novaProcess.Id, expectedIndicatorFlags: null);
        }

        [TestCase]
        [Description("Publish process, generate user story for it, update Nonfunctional requirement" +
            "field with inline trace to process artifact. Response must not return an error.")]
        public void UpdateNonfunctionalRequirementsWithInlineTrace_VerifySuccess()
        {
            // Setup:
            var novaProcess = Helper.Storyteller.CreateAndPublishNovaProcessArtifact(_project, _user);
            var wrappedProcess = Helper.WrapArtifact(novaProcess, _project, _user);

            //get text with inline trace to the specified artifact
            var inlineTraceText = GetTextForInlineTrace(new List<ArtifactWrapper>() { wrappedProcess });

            // Generate User Stories from the Process
            var userStories = Helper.Storyteller.GenerateUserStories(_user, novaProcess.Process);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                userStories[0].UpdateNonFunctionalRequirements(_user, inlineTraceText);
            }, "Update Nonfunctional Requirements must not return an error.");

            // Verify:
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, novaProcess.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
        }

        [TestCase]
        [Description("Create and publish process, generate user story, check that user story has expected title.")]
        [TestRail(125529)]
        public void UserStoryGenerationProcessWithDefaultUserTask_VerifyingSTTitle()
        {
            // Create and publish a process artifact
            var novaProcess = Helper.Storyteller.CreateAndPublishNovaProcessArtifact(_project, _user);
            
            var userTasksOnProcess = novaProcess.Process.GetProcessShapesByShapeType(ProcessShapeType.UserTask);
            // Get value of the persona property
            var persona = userTasksOnProcess[0].PropertyValues["persona"].Value;
            // Get value of the User Task name
            var taskName = userTasksOnProcess[0].Name;
            // User story title must be 'As a <persona> I want to <taskName>'
            const string css_font_normal = "<span style=\"font-weight: normal;color: #565656;";
            const string css_font_bold = "<span style=\"font-weight: bold;color: #565656;";
            string expectedStoryTitle = I18NHelper.FormatInvariant("<html>\r\n<body>\r\n<p style=\"margin: 0px;\">{0}\">As a </span><span style=\"font-weight: bold;color: #565656;\">{2}</span>{0}\">, I want to </span>{1}\">{3}</span></p>\r\n</body>\r\n</html>\r\n",
                css_font_normal, css_font_bold, persona.ToString(), taskName);

            // Generated User Stories from the Process
            var userStories = Helper.Storyteller.GenerateUserStories(_user, novaProcess.Process);
            // Check that ST-Title property has expected value
            var stTitleProperty = GetPropertyByName(userStories[0].CustomProperties, "ST-Title");
            Assert.AreEqual(expectedStoryTitle, stTitleProperty.Value);
        }

        #endregion Tests

        #region 400 Bad Request

        [TestRail(246537)]
        [TestCase("9999999999", "The request is invalid.")]
        [TestCase("&amp;", "A potentially dangerous Request.Path value was detected from the client (&).")]
        [Description("Create a rest path that tries to generate user stories for a process with an invalid artifact Id. " +
                     "Attempt to  generate the user stories. Verify that HTTP 400 Bad Request exception is thrown.")]
        public void GenerateUserStories_InvalidArtifactId_400BadRequest(string artifactId, string expectedErrorMessage)
        {
            // Setup:
            string path = I18NHelper.FormatInvariant(SVC_USERSTORIES_PATH, _project.Id, artifactId);

            var restApi = new RestApiFacade(Helper.ArtifactStore.Address, _user?.Token?.AccessControlToken);

            // Execute & Verify:
            var ex = Assert.Throws<Http400BadRequestException>(() => restApi.SendRequestAndDeserializeObject<NovaArtifactResponse, object>(
               path,
               RestRequestMethod.POST,
               jsonObject: null),
                "We should get a 400 Bad Request when the artifact Id is invalid!");

            // Verify:
            TestHelper.ValidateServiceErrorMessage(ex.RestResponse, expectedErrorMessage);
        }

        #endregion 400 Bad Request

        #region private functions

        /// <summary>
        /// Gets the StorytellerProperty from the list that has the specified Name.
        /// </summary>
        /// <param name="storytellerProperties">The list of properties to search through.</param>
        /// <param name="name">The name of the property to find.</param>
        /// <returns>The StorytellerProperty that it found.</returns>
        private static StorytellerProperty GetPropertyByName(List<StorytellerProperty> storytellerProperties, string name)
        {
            ThrowIf.ArgumentNull(storytellerProperties, nameof(storytellerProperties));

            return storytellerProperties.Find(p => p.Name.StartsWithOrdinal(name)); // Use StartsWith() instead of == because it might have "(Agile Pack)" on the end of the name.
        }

        /// <summary>
        /// Creates text with inline traces for provided artifacts. For use with RTF properties.
        /// </summary>
        /// <param name="artifacts">list of target artifacts for inline traces</param>
        /// <returns>Text with inline traces</returns>
        private static string GetTextForInlineTrace(List<ArtifactWrapper> artifacts)
        {
            var text = string.Empty;

            foreach (var artifact in artifacts)
            {
                text = text + I18NHelper.FormatInvariant("<a " +
                "href=\"{0}?ArtifactId={1}\" target=\"\" artifactid=\"{1}\"" +
                " linkassemblyqualifiedname=\"BluePrintSys.RC.Client.SL.RichText.RichTextArtifactLink, BluePrintSys.RC.Client.SL.RichText, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\"" +
                " text=\"{1}: {2}\" canclick=\"True\" canedit=\"False\" isvalid=\"True\"><span style=\"text-decoration: underline;\">{1}: {2}</span></a>&nbsp;",
                artifact.ArtifactStore.Address, artifact.Id, artifact.Name);
            }

            return "<p>"+text+"</p>";
        }

        /// <summary>
        /// Validate number of user task shapes for the process
        /// </summary>
        /// <param name="novaProcess">the nova process artifact to validate</param>
        /// <param name="expectedProcessShapeCount">expected user task process shape count</param>
        /// <param name="actualProcessUserTaskShapeCount">actual user task process shape count</param>
        private static void ValidateProcessUserTaskCount(NovaProcess novaProcess, int expectedProcessUserTaskShapeCount, int actualProcessUserTaskShapeCount)
        {
            ThrowIf.ArgumentNull(novaProcess, nameof(novaProcess));

            Assert.AreEqual(expectedProcessUserTaskShapeCount, actualProcessUserTaskShapeCount,
                "The expected number of UserTasks for the Process is {0} but The actual number of UserTasks returned is {1}.",
                expectedProcessUserTaskShapeCount, actualProcessUserTaskShapeCount);

            Logger.WriteDebug("The number of UserTasks inside of the Process is: {0}", actualProcessUserTaskShapeCount);
        }

        /// <summary>
        /// Validate Contents of generated user stories for the process
        /// </summary>
        /// <param name="novaProcess">the nova process artifact to validate</param>
        /// <param name="userStories">userstories generated from the nova process artifact</param>
        private static void ValidateGeneratedUserStories (NovaProcess novaProcess, List<IStorytellerUserStory> userStories)
        {
            ThrowIf.ArgumentNull(novaProcess, nameof(novaProcess));
            ThrowIf.ArgumentNull(userStories, nameof(userStories));

            var UserTaskShapesInNovaProcess = novaProcess.Process.GetProcessShapesByShapeType(ProcessShapeType.UserTask);

            foreach (var shape in UserTaskShapesInNovaProcess)
            {
                var userStoryCounter = 0;

                foreach (var us in userStories.Where(us => us.ProcessTaskId.Equals(shape.Id)))
                {
                    userStoryCounter++;

                    // -- Verifying userStory contents -- 
                    Assert.That(us.Name.Equals(shape.Name), "Generated US name {0} doesn't match with the source UT name {1}", us.Name, shape.Name);

                    // TODO Assert that UserStory ID == 
                    //Assert.That(userStory.Id.Equals(processShape.PropertyValues["storyLinks"]), "Generated US name {0} doesn't match with the source UT name {1}", userStory.Name, processShape.Name);

                    // Assert that UserStory Property's Name value with Shape's Name Value 
                    Assert.That(us.SystemProperties.Find(s => s.Name.Equals("Name")).Value.Equals(shape.Name),
                        "Generated US's Property Name {0} doesn't match with the source UT name {1}",
                        us.SystemProperties.Find(s => s.Name.Equals("Name")).Value, shape.Name);

                    // Assert that UserStory ST-Title ==
                    // Assert that UserStory ST-Acceptance Criteria ==
                }

                Assert.AreNotEqual(0, userStoryCounter, "No UserStory matches with the UserTask whose ID: {0} is created", shape.Id);
                Assert.AreEqual(1,userStoryCounter, "More than one UserStories are generated for the UserTask whose ID: {0}.", shape.Id);
            }

            Assert.AreEqual(UserTaskShapesInNovaProcess.Count, userStories.Count,
                "The expected number of generated UserStories is {0} but The actual number of generated UserStories is {1}.",
                UserTaskShapesInNovaProcess.Count, userStories.Count);
        }

        /// <summary>
        /// Validate IsNew Flags from generated userstories
        /// </summary>
        /// <param name="userStoriesFirstBatch">user stories generated from first generateUserStories</param>
        /// <param name="userStoriesSecondBatch">user stories generated from second generateUserStories</param>
        private static void ValidateIsNewFlagForUserStories(List<IStorytellerUserStory> userStoriesFirstBatch, List<IStorytellerUserStory> userStoriesSecondBatch)
        {
            ThrowIf.ArgumentNull(userStoriesFirstBatch, nameof(userStoriesFirstBatch));
            ThrowIf.ArgumentNull(userStoriesSecondBatch, nameof(userStoriesSecondBatch));

            var createdUserStoriesFirstBatchCount = userStoriesFirstBatch.Count;
            var totalUserStoriesSecondBatchCount = userStoriesSecondBatch.Count;
            var createdUserStoriesSecondBatchCount = userStoriesSecondBatch.FindAll(u => u.IsNew.Equals(true)).Count;
            var updatedUserStoriesSecondBatchCount = userStoriesSecondBatch.FindAll(u => u.IsNew.Equals(false)).Count;

            Assert.AreEqual(totalUserStoriesSecondBatchCount, createdUserStoriesSecondBatchCount + updatedUserStoriesSecondBatchCount,
                "The user stories either updated or created: {0} should be equal to addition of the created: {1} and updated: {2}",
                totalUserStoriesSecondBatchCount, createdUserStoriesSecondBatchCount, updatedUserStoriesSecondBatchCount);
            Assert.AreEqual(createdUserStoriesFirstBatchCount, updatedUserStoriesSecondBatchCount,
                "The expected number of user stories from UserStoryGeneration call is {0} but {1} are updated.",
                createdUserStoriesFirstBatchCount, updatedUserStoriesSecondBatchCount);
        }

        /// <summary>
        /// Add UserTasksToProcess
        /// </summary>
        /// <param name="novaProcess">the nova process artifact to update</param>
        /// <param name="additionalUserTasks">the number of UserTasks to add</param>
        private static void AddUserTasksToNovaProcess(NovaProcess novaProcess, int additionalUserTasks)
        {
            ThrowIf.ArgumentNull(novaProcess, nameof(novaProcess));

            // Get the process artifact
            // Add UserTasks - iteration
            var precondition = novaProcess.Process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition task
            var processLink = novaProcess.Process.GetOutgoingLinkForShape(precondition);

            for (int i = 0; i < additionalUserTasks; i++)
            {
                var userTask = novaProcess.Process.AddUserAndSystemTask(processLink);
                var processShape = novaProcess.Process.GetNextShape(userTask);

                processLink = novaProcess.Process.GetOutgoingLinkForShape(processShape);
            }
        }

        /// <summary>
        /// Get the updated NovaProcess with additional UserTasks
        /// </summary>
        /// <param name="novaProcess">the nova process to update</param>
        /// <param name="additionalUserTasks">the number of UserTasks to add</param>
        /// <returns>the updated nova process with additonal user tasks</returns>
        private NovaProcess GetNovaProcessWithAdditionalShapes(NovaProcess novaProcess, int additionalUserTasks)
        {
            ThrowIf.ArgumentNull(novaProcess, nameof(novaProcess));

            AddUserTasksToNovaProcess(novaProcess, additionalUserTasks);
            // Publish the updated Process
            var updatedNovaProcess = Helper.Storyteller.UpdateNovaProcess(_user, novaProcess);
            Helper.Storyteller.PublishNovaProcess(_user, updatedNovaProcess);

            // Get the updated nova process
            return Helper.Storyteller.GetNovaProcess(_user, novaProcess.Id);
        }

        #endregion private functions
    }
}
