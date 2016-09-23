using Common;
using CustomAttributes;
using Model;
using Model.Factories;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Model.ArtifactModel;
using System.Linq;
using Helper;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using TestCommon;
using Utilities;

namespace StorytellerTests
{
    public class UserStoryTests : TestBase
    {
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
            // Create and publish a process artifact
            var processArtifact = Helper.Storyteller.CreateAndPublishProcessArtifact(_project, _user);

            // Find number of UserTasks from the published Process
            var process = Helper.Storyteller.GetProcess(_user, processArtifact.Id);

            var userTasksOnProcess = process.GetProcessShapesByShapeType(ProcessShapeType.UserTask).Count;

            // Assert that the number of UserTasks the published process is equal to the number of UserTasks returned from GetProcess call
            Assert.That(userTasksOnProcess == _defaultUserTaskCount,
                "The default number of UserTasks for the new Process is {0} but The number of UserTasks returned from GetProcess call is {1}.",
                _defaultUserTaskCount, userTasksOnProcess);

            Logger.WriteDebug("The number of UserTasks inside of Process is: {0}", userTasksOnProcess);

            // Generate User Story artfact(s) from the Process artifact
            List<IStorytellerUserStory> userStories = Helper.Storyteller.GenerateUserStories(_user, process);

            Logger.WriteDebug("The number of UserStories generated is: {0}", userStories.Count);

            // Assert that the number of UserTasks from the published Process is equal to the number of UserStoryGenerated or Updated
            Assert.That(userStories.Count == userTasksOnProcess,
                "The number of UserStories generated from the process is {0} but The process has {1} UserTasks.",
                userStories.Count, userTasksOnProcess);
        }

        [TestCase]
        [Description("Verify the contents of generated or updated user stories")]
        public void UserStoryGenerationProcessWithDefaultUserTask_VerifyingContents()
        {
            // Create and publish a process artifact
            var processArtifact = Helper.Storyteller.CreateAndPublishProcessArtifact(_project, _user);

            // Checking Object: The Process that contains shapes including user task shapes
            var process = Helper.Storyteller.GetProcess(_user, processArtifact.Id);

            // Test Object: Generated User Stories from the Process
            List<IStorytellerUserStory> userStories = Helper.Storyteller.GenerateUserStories(_user, process);

            // Assert that there is only one to one maching between UserTask and generated UserStory
            foreach (IProcessShape shape in process.GetProcessShapesByShapeType(ProcessShapeType.UserTask))
            {
                var userStoryCounter = 0;

                foreach (IStorytellerUserStory us in userStories.Where(us => us.ProcessTaskId.Equals(shape.Id)))
                {
                    userStoryCounter++;

                    // -- Verifying userStory contents -- 
                    Assert.That(us.Name.Equals(shape.Name),"Generated US name {0} doesn't match with the source UT name {1}", us.Name, shape.Name);

                    // TODO Assert that UserStory ID == 
                    //Assert.That(userStory.Id.Equals(processShape.PropertyValues["storyLinks"]), "Generated US name {0} doesn't match with the source UT name {1}", userStory.Name, processShape.Name);

                    // Assert that UserStory Property's Name value with Shape's Name Value 
                    Assert.That(us.SystemProperties.Find(s => s.Name.Equals("Name")).Value.Equals(shape.Name),
                        "Generated US's Property Name {0} doesn't match with the source UT name {1}",
                        us.SystemProperties.Find(s => s.Name.Equals("Name")).Value, shape.Name);
                        
                    // Assert that UserStory ST-Title ==
                    // Assert that UserStory ST-Acceptance Criteria ==
                }

                Assert.That(!userStoryCounter.Equals(0), "No UserStory matches with the UserTask whose ID: {0} is created", shape.Id);
                Assert.That(userStoryCounter.Equals(1), "More than one UserStories are generated for the UserTask whose ID: {0}.", shape.Id);
            }
        }

        [TestCase]
        [Description("Retrieve UserStoryArtifactType if Storyteller Pack is installed on the target Blueprint")]
        public void GetUserStoryArtifactType_ReceiveUserStoryArtifactType()
        {
            var userStoryArtifactType = Helper.Storyteller.GetUserStoryArtifactType(_user, _project.Id);

            Assert.NotNull(userStoryArtifactType.Id,"UserStoryArtifactType Id is null");
            Assert.NotNull(userStoryArtifactType.Name, "UserStoryArtifactType Name is null");
        }

        [TestCase(NumberOfAdditionalUserTasks)]
        [Description("Verify that total number of generated or updated user stories are equal to total number of user tasks for the process with multi user tasks")]
        public void UserStoryGenerationProcessWithMultipleUserTasks_NumberOfUserTasksAndUserStoriesAreEqual(int iteration)
        {
            int userTaskExpectedCount = iteration + _defaultUserTaskCount;

            if (userTaskExpectedCount == int.MaxValue)
            {
                throw new OverflowException("overflow exception");
            }

            // Create an Process artifact
            var processArtifact = Helper.Storyteller.CreateAndSaveProcessArtifact(project: _project, user: _user);

            // Get the process artifact
            var process = Helper.Storyteller.GetProcess(_user, processArtifact.Id);

            // Add UserTasks - iteration
            var precondition = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition task
            var processLink = process.GetOutgoingLinkForShape(precondition);

            for (int i = 0; i < iteration; i++)
            {
                var userTask = process.AddUserAndSystemTask(processLink);
                var processShape = process.GetNextShape(userTask);

                processLink = process.GetOutgoingLinkForShape(processShape);
            }

            // Update the process
             var updatedProcess = Helper.Storyteller.UpdateProcess(_user, process);

            // Publish the Process artifact
            Helper.Storyteller.PublishProcess(_user, updatedProcess);

            // Find number of UserTasks from the published Process
            process = Helper.Storyteller.GetProcess(_user, processArtifact.Id);

            var userTasksOnProcess = process.GetProcessShapesByShapeType(ProcessShapeType.UserTask).Count;

            // Assert that the number of UserTasks the published process is equal to the number of UserTasks returned from GetProcess call
            Assert.That(userTasksOnProcess == userTaskExpectedCount,
                "The number of UserTasks expected for the Process is {0} but The number of UserTasks returned from GetProcess call is {1}.",
                userTaskExpectedCount, userTasksOnProcess);

            Logger.WriteDebug("The number of UserTasks inside of Process is: {0}", userTasksOnProcess);

            // Generate User Story artfact(s) from the Process artifact
            List<IStorytellerUserStory> userStories = Helper.Storyteller.GenerateUserStories(_user, process);

            Logger.WriteDebug("The number of UserStories generated is: {0}", userStories.Count);

            // Assert that the number of UserTasks from the published Process is equal to the number of UserStoryGenerated or Updated
            Assert.That(userStories.Count == userTasksOnProcess,
                "The number of UserStories generated from the process is {0} but The process has {1} UserTasks.",
                userStories.Count, userTasksOnProcess);
        }

        [TestCase(NumberOfAdditionalUserTasks)]
        [Description("Verify that every generated or updated user stories are mapped to user tasks for the process with multi user tasks")]
        public void UserStoryGenerationProcessWithMultipleUserTasks_UserTaskUserStoryMapping(int iteration)
        {
            // Create an Process artifact
            var processArtifact = Helper.Storyteller.CreateAndSaveProcessArtifact(project: _project, user: _user);

            // Get the process artifact
            var process = Helper.Storyteller.GetProcess(_user, processArtifact.Id);

            // Add UserTasks - iteration
            var precondition = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition task
            var processLink = process.GetOutgoingLinkForShape(precondition);

            for (int i = 0; i < iteration; i++)
            {
                var userTask = process.AddUserAndSystemTask(processLink);
                var processShape = process.GetNextShape(userTask);

                processLink = process.GetOutgoingLinkForShape(processShape);
            }

            // Update the process
            var updatedProcess = Helper.Storyteller.UpdateProcess(_user, process);

            // Publish the Process artifact; enable recursive delete flag
            Helper.Storyteller.PublishProcess(_user, updatedProcess);

            // Checking Object: The Process that contains shapes including user task shapes
            process = Helper.Storyteller.GetProcess(_user, processArtifact.Id);

            // Test Object: Generated User Stories from the Process
            List<IStorytellerUserStory> userStories = Helper.Storyteller.GenerateUserStories(_user, process);

            // Assert that there is one to one maching between UserTask and generated UserStory
            foreach (IProcessShape shape in process.GetProcessShapesByShapeType(ProcessShapeType.UserTask))
            {
                var userStoryCounter = userStories.Count(us => us.ProcessTaskId.Equals(shape.Id));

                Assert.That(!userStoryCounter.Equals(0), "No UserStory matches with the UserTask whose ID: {0} is created", shape.Id);
                Assert.That(userStoryCounter.Equals(1), "More than one UserStories are generated for the UserTask whose ID: {0}.", shape.Id);
            }
        }

        [TestCase(NumberOfAdditionalUserTasks)]
        [Description("Verify that Genearate UserStories updates user stories if there are existing user stories for user tasks for the process with multi user tasks")]
        public void UserStoryGenerationProcessWithMultipleUserTasks_VerifyingUpdateFlagsForExistingUserStories(int iteration)
        {
            var initialUserTaskExpectedCount = iteration / 2 + _defaultUserTaskCount;
            var additionalUserTaskExpectedCount = iteration - (iteration/2);

            // Create an Process artifact
            var processArtifact = Helper.Storyteller.CreateAndSaveProcessArtifact(project: _project, user: _user);

            // Get the process artifact
            var process = Helper.Storyteller.GetProcess(_user, processArtifact.Id);

            // Add UserTasks - InitialUserTaskExpected - DEFAULTUSERTASK_COUNT since default UT counts
            var precondition = process.GetProcessShapeByShapeName(Process.DefaultPreconditionName);

            // Find outgoing process link for precondition task
            var processLink = process.GetOutgoingLinkForShape(precondition);

            for (int i = 0; i < initialUserTaskExpectedCount - _defaultUserTaskCount; i++)
            {
                var userTask = process.AddUserAndSystemTask(processLink);
                var processShape = process.GetNextShape(userTask);

                processLink = process.GetOutgoingLinkForShape(processShape);
            }

            // Update the process
            var updatedProcess = Helper.Storyteller.UpdateProcess(_user, process);

            // Publish the Process artifact
            Helper.Storyteller.PublishProcess(_user, updatedProcess);

            // Get the process artifact
            var returnedProcess = Helper.Storyteller.GetProcess(_user, processArtifact.Id);

            // User Stories from the Process artifact
            List<IStorytellerUserStory> userStoriesFirstBatch = Helper.Storyteller.GenerateUserStories(_user, returnedProcess);

            Logger.WriteDebug("The number of user stories generated is: {0}", userStoriesFirstBatch.Count);

            // Add UserTasks - AdditionalUserTaskExpected
            precondition = returnedProcess.Shapes.Find(p => p.Name.Equals(Process.DefaultPreconditionName));

            // Find outgoing process link for precondition task
            processLink = returnedProcess.GetOutgoingLinkForShape(precondition);

            for (int i = 0; i < additionalUserTaskExpectedCount; i++)
            {
                var userTask = returnedProcess.AddUserAndSystemTask(processLink);
                var processShape = returnedProcess.GetNextShape(userTask);

                processLink = returnedProcess.GetOutgoingLinkForShape(processShape);
            }

            // Update the process
            updatedProcess = Helper.Storyteller.UpdateProcess(_user, returnedProcess);

            // Publish the Process artifact
            Helper.Storyteller.PublishProcess(_user, updatedProcess);

            // Get the process artifact
            returnedProcess = Helper.Storyteller.GetProcess(_user, processArtifact.Id);

            // User Stories from the Process artifact
            List<IStorytellerUserStory> userStoriesSecondBatch = Helper.Storyteller.GenerateUserStories(_user, returnedProcess);

            Logger.WriteDebug("The number of user stories generated or updated is: {0}", userStoriesSecondBatch.Count);

            //Assert that the count of generated user stories from first batch is equal to the count of updated user stories from the second batch
            var createdUserStoriesFirstBatchCount = userStoriesFirstBatch.Count;
            var totalUserStoriesSecondBatchCount = userStoriesSecondBatch.Count;
            var createdUserStoriesSecondBatchCount = userStoriesSecondBatch.FindAll(u => u.IsNew.Equals(true)).Count;
            var updatedUserStoriesSecondBatchCount = userStoriesSecondBatch.FindAll(u => u.IsNew.Equals(false)).Count;

            Assert.That(totalUserStoriesSecondBatchCount == createdUserStoriesSecondBatchCount + updatedUserStoriesSecondBatchCount,
                "The user stories either updated or created: {0} should be equal to addition of the created: {1} and updated: {2}",
                totalUserStoriesSecondBatchCount, createdUserStoriesSecondBatchCount, updatedUserStoriesSecondBatchCount);
            Assert.That(createdUserStoriesFirstBatchCount == updatedUserStoriesSecondBatchCount,
                "The expected number of user stories from UserStoryGeneration call is {0} but {1} are updated.",
                createdUserStoriesFirstBatchCount, updatedUserStoriesSecondBatchCount);
        }

        [TestCase]
        [Description("Publish process, generate user story for it, update Nonfunctional requirement" +
            "field with inline trace to deleted artifact. Response must return error message.")]
        public void UpdateNonfunctionalRequirementsWithInlineTrace_VerifyReturnedMessage()
        {
            // Create and publish a process artifact
            var processArtifact = Helper.Storyteller.CreateAndPublishProcessArtifact(_project, _user);

            var process = Helper.Storyteller.GetProcess(_user, processArtifact.Id);

            // Create target artifact for inline trace
            IArtifact linkedArtifact = ArtifactFactory.CreateArtifact(_project, _user, BaseArtifactType.Actor);     // TODO:  Change ArtifactFactory to Helper.
            linkedArtifact.Save();
            linkedArtifact.Publish();
            string inlineTraceText;
            int linkedArtifactId = linkedArtifact.Id;

            try
            {
                //get text with inline trace to the specified artifact
                inlineTraceText = GetTextForInlineTrace(new List<IArtifact>() {linkedArtifact});
            }
            finally
            {
                //delete artifact which is target for inline trace
                linkedArtifact.Delete();
                linkedArtifact.Publish();
            }

            // Generate User Stories from the Process
            List<IStorytellerUserStory> userStories = Helper.Storyteller.GenerateUserStories(_user, process);

            // update Nonfunctional Requirements field with inline trace
            var updatePropertyResult = userStories[0].UpdateNonfunctionalRequirements(Helper.Storyteller.Address, _user, inlineTraceText);
            Assert.That(updatePropertyResult.Messages.Count() == 1,
                "Result of create inline trace must return one error message, but returns {0}",
                updatePropertyResult.Messages.Count());

            string expectedMessage = I18NHelper.FormatInvariant("Artifact with ID {0} was inaccessible. A manual trace was not created.", linkedArtifactId);
            Assert.That(updatePropertyResult.Messages.ElementAt(0).Message.Equals(expectedMessage), "Returned message must be '{0}', but it is '{1}'",
                expectedMessage, updatePropertyResult.Messages.ElementAt(0).Message);
            Assert.AreEqual(updatePropertyResult.Messages.ElementAt(0).ItemId, linkedArtifactId, "Returned ID must be {0}, but it is {1}",
                linkedArtifactId, updatePropertyResult.Messages.ElementAt(0).ItemId);
        }

        [TestCase]
        [Description("Publish process, generate user story for it, update Nonfunctional requirement" +
            "field with inline trace to process artifact. Response must not return an error.")]
        public void UpdateNonfunctionalRequirementsWithInlineTrace_VerifySuccess()
        {
            // Create and publish a process artifact
            var processArtifact = Helper.Storyteller.CreateAndPublishProcessArtifact(_project, _user);

            var process = Helper.Storyteller.GetProcess(_user, processArtifact.Id);

            //get text with inline trace to the specified artifact
            var inlineTraceText = GetTextForInlineTrace(new List<IArtifact>() { processArtifact });

            // Generate User Stories from the Process
            List<IStorytellerUserStory> userStories = Helper.Storyteller.GenerateUserStories(_user, process);

            // update Nonfunctional Requirements field with inline trace
            Assert.DoesNotThrow(() =>
            {
                userStories[0].UpdateNonfunctionalRequirements(Helper.Storyteller.Address, _user, inlineTraceText);
            }, "Update Nonfunctional Requirements must not return an error.");
        }

        [TestCase]
        [Description("Create and publish process, generate user story, check that user story has expected title.")]
        [TestRail(125529)]
        public void UserStoryGenerationProcessWithDefaultUserTask_VerifyingSTTitle()
        {
            // Create and publish a process artifact
            var processArtifact = Helper.Storyteller.CreateAndPublishProcessArtifact(_project, _user);

            // Get process
            var process = Helper.Storyteller.GetProcess(_user, processArtifact.Id);
            
            var userTasksOnProcess = process.GetProcessShapesByShapeType(ProcessShapeType.UserTask);
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
            List <IStorytellerUserStory> userStories = Helper.Storyteller.GenerateUserStories(_user, process);
            // Check that ST-Title property has expected value
            var stTitleProperty = GetPropertyByName(userStories[0].CustomProperties, "ST-Title");
            Assert.AreEqual(expectedStoryTitle, stTitleProperty.Value);
        }

        #endregion Tests

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
        private static string GetTextForInlineTrace(List<IArtifact> artifacts)
        {
            var text = string.Empty;

            foreach (var artifact in artifacts)
            {
                text = text + I18NHelper.FormatInvariant("<a " +
                "href=\"{0}?ArtifactId={1}\" target=\"\" artifactid=\"{1}\"" +
                " linkassemblyqualifiedname=\"BluePrintSys.RC.Client.SL.RichText.RichTextArtifactLink, BluePrintSys.RC.Client.SL.RichText, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\"" +
                " text=\"{1}: {2}\" canclick=\"True\" canedit=\"False\" isvalid=\"True\"><span style=\"text-decoration: underline;\">{1}: {2}</span></a>&nbsp;",
                artifact.Address, artifact.Id, artifact.Name);
            }

            return "<p>"+text+"</p>";
        }
    }
}
