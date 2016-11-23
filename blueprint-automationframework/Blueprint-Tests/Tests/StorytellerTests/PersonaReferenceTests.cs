using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using TestCommon;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class PersonaReferenceTests : TestBase
    {
        private IUser _adminUser;
        private IUser _authorFullAccess;
        private IProject _project;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();

            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_adminUser);
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _adminUser);
            _authorFullAccess = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region Tests

        [TestCase(Process.DefaultUserTaskName)]
        [TestCase(Process.DefaultSystemTaskName)]
        [Description("Add a persona reference to a Process artifact task. Verify the persona reference was added.")]
        public void PersonaReference_AddReferenceToTask_VerifyPersonaAdded(string taskName)
        {
            // Setup:
            // Create and get the default process
            var process = StorytellerTestHelper.CreateAndGetDefaultProcess(Helper.Storyteller, _project, _authorFullAccess);

            AddPersonaReferenceToTask(taskName, process, _authorFullAccess);

            // Execute & Verify:
            StorytellerTestHelper.UpdateAndVerifyProcess(process, Helper.Storyteller, _authorFullAccess);
        }

        #endregion Tests

        #region Private Methods

        /// <summary>
        /// Creates an actor artifact and adds it to a task as a persona reference
        /// </summary>
        /// <param name="taskName">The name of the task that will contain the persona reference.</param>
        /// <param name="process">The process containing the task </param>
        /// <param name="user">The user that will create the actor artifact.</param>
        private void AddPersonaReferenceToTask(string taskName, IProcess process, IUser user)
        {
            // Create actor for persona reference
            var actor = Helper.CreateAndPublishArtifact(_project, user, BaseArtifactType.Actor);

            var actorDetails = Helper.ArtifactStore.GetArtifactDetails(user, actor.Id);

            // Add include to default user task
            var defaultUserTask = process.GetProcessShapeByShapeName(taskName);
            defaultUserTask.AddPersonaReference(actorDetails);
        }

        #endregion
    }
}
