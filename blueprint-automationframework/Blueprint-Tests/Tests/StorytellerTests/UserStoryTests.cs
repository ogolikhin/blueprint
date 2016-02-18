using CustomAttributes;
using Model;
using Model.Factories;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorytellerTests
{
    class UserStoryTests
    {
        private IAdminStore _adminStore;
        private IUser _user;
        private IProject _project;
        private IArtifact _artifact;

        #region Setup
        [SetUp]
        public void SetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _user = UserFactory.CreateUserAndAddToDatabase();
            _project = ProjectFactory.GetProject();
            // Get a valid token for the user.
            ISession session = _adminStore.AddSession(_user.Username, _user.Password);
            _user.SetToken(session.SessionId);
            // Create an process artifact
            _artifact = ArtifactFactory.CreateArtifact(_project, ArtifactType.Process);

        }
        #endregion Setup

        #region TearDown
        [TearDown]
        public void TearDown()
        {
            if (_user != null)
            {
                _user.DeleteUser(deleteFromDatabase: true);
                _user = null;
            }
        }
        #endregion TearDown

        #region Tests
        [Test]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        public void GetUserStories()
        {

        }
        #endregion Tests
    }
}
