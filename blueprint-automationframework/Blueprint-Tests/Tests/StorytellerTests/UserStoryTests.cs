using Common;
using CustomAttributes;
using Model;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using System.Collections.Generic;

namespace StorytellerTests
{
    [Explicit(IgnoreReasons.UnderDevelopment)]
    public class UserStoryTests
    {
        private IAdminStore _adminStore;
        private IStoryteller _storyteller;
        private IUser _user;
        private IProject _project;
        private IOpenApiArtifact _processArtifact;
        private ISession _session;

        #region SetUp
        [SetUp]
        public void SetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _storyteller = StorytellerFactory.GetStorytellerFromTestConfig();
            _user = UserFactory.CreateUserAndAddToDatabase();
            _project = ProjectFactory.GetProject(_user);
        }
        #endregion SetUp

        #region TearDown
        [TearDown]
        public void TearDown()
        {
            if (_user != null)
            {
                //_user.DeleteUser(deleteFromDatabase: true);
                _user = null;
            }
        }
        #endregion TearDown

        #region Tests
        [Test]
        public void GenerateUserStories()
        {
            // Create an Process artifact
            _processArtifact = _storyteller.CreateProcessArtifact(project: _project, user: _user, artifactType: BaseArtifactType.Process);
            
            // Publish the Process artifact - using RestApi
            _storyteller.PublishProcessArtifacts(_user);

            // Set session for StoryTeller Interal Api Operation 
            _session = _adminStore.AddSession(_user.Username, _user.Password);
            _user.SetToken(_session.SessionId);

            // Generate User Story artfact(s) from the Process artifact
            List<OpenApiUserStoryArtifact> userStories = _storyteller.GenerateUserStories(_user, _processArtifact);

            Logger.WriteDebug("Total number of UserStoryGenerated/Updated is: {0}", userStories.Count);
        }
        #endregion Tests
    }
}
