using CustomAttributes;
using Model;
using Model.Factories;
using NUnit.Framework;

namespace CommonServiceTests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    [TestFixture]
    [Category(Categories.ArtifactVersion)]
    [Explicit(IgnoreReasons.UnderDevelopment)]
    class ArtifactVersionTests
    {
        private IAdminStore _adminStore;
        private IBlueprintServer _blueprintServer;
        private IUser _user;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private IProject _project;


        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _blueprintServer = BlueprintServerFactory.GetBlueprintServerFromTestConfig();
            _user = UserFactory.CreateUserAndAddToDatabase();
            _project = ProjectFactory.GetProject(_user);

            // Get a valid Access Control token for the user (for the new Storyteller REST calls).
            ISession session = _adminStore.AddSession(_user.Username, _user.Password);
            _user.SetToken(session.SessionId);

            Assert.IsFalse(
                string.IsNullOrWhiteSpace(_user.Token.AccessControlToken),
                "The user didn't get an Access Control token!");

            // Get a valid OpenApi token for the user (for the OpenApi artifact REST calls).
            _blueprintServer.LoginUsingBasicAuthorization(_user, string.Empty);

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.OpenApiToken), "The user didn't get an OpenApi token!");
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            //if (_artifactVersion. != null)
            //{
            //    // Delete or Discard all the artifacts that were added.
            //    var savedArtifactsList = new List<IOpenApiArtifact>();
            //    foreach (var artifact in _storyteller.Artifacts.ToArray())
            //    {
            //        if (artifact.IsPublished)
            //        {
            //            _storyteller.DeleteProcessArtifact(artifact, deleteChildren: true);
            //        }
            //        else
            //        {
            //            savedArtifactsList.Add(artifact);
            //        }
            //    }
            //    if (savedArtifactsList.Any())
            //    {
            //        Storyteller.DiscardProcessArtifacts(savedArtifactsList, _blueprintServer.Address, _user);
            //    }
            //}

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [TestCase]
        [Description("")]
        public void FirstTest()
        {

        }
    }
}
