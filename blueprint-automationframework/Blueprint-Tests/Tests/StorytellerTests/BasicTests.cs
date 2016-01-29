using Common;
using CustomAttributes;
using Model;
using Model.Factories;
using NUnit.Framework;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    public class BasicTests
    {
        private IAdminStore _adminStore;
        private IStoryteller _storyteller;
        private IUser _user;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _storyteller = StorytellerFactory.GetStorytellerFromTestConfig();
            _user = UserFactory.CreateUserAndAddToDatabase();

            // Get a valid token for the user.
            ISession session = _adminStore.AddSession(_user.Username, _user.Password);
            _user.SetToken(session.SessionId);

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.AccessControlToken), "The user didn't get an Access Control token!");
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
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

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase(12, 5, 2, ProcessType.BusinessProcess)]
        public void GetDefaultProcess_VerifyReturnedProcess(int id, int defaultShapesLength, int defaultLinksLength, ProcessType processType)
        {
            var process = _storyteller.GetProcess(_user, id);

            Assert.IsNotNull(process, "The returned process was null.");
            Assert.That(process.Id == id, I18NHelper.FormatInvariant("The ID of the returned process was '{0}', but '{1}' was expected.", process.Id, id));
            Assert.That(process.Shapes.Length == defaultShapesLength, I18NHelper.FormatInvariant("The number of shapes in a default process is {0} but {1} shapes were returned.", defaultShapesLength, process.Shapes.Length));
            Assert.That(process.Links.Length == defaultLinksLength, I18NHelper.FormatInvariant("The number of links in a default process is {0} but {1} links were returned.", defaultLinksLength, process.Links.Length));
            Assert.That(process.Type == processType, I18NHelper.FormatInvariant("The process type returned was '{0}', but '{1}' was expected", process.Type.ToString(), processType.ToString()));
            Assert.That(process.Shapes[0].Name == ProcessShapeType.Start.ToString(), I18NHelper.FormatInvariant("The shape returned was named '{0}', but '{1}' was expected", process.Shapes[0].Name, ProcessShapeType.Start.ToString()));
            Assert.That(process.Shapes[0].ShapeType == ProcessShapeType.Start, I18NHelper.FormatInvariant("The shape returned was of type '{0}', but '{1}' was expected", process.Shapes[0].ShapeType.ToString(), ProcessShapeType.Start.ToString()));
            Assert.That(process.Shapes[1].Name == ProcessShapeType.UserTask.ToString(), I18NHelper.FormatInvariant("The shape returned was named '{0}' but '{1}' was expected", process.Shapes[1].Name, ProcessShapeType.UserTask.ToString()));
            Assert.That(process.Shapes[1].ShapeType == ProcessShapeType.UserTask, I18NHelper.FormatInvariant("The shape returned was of type '{0}' but '{1}' was expected", process.Shapes[1].ShapeType.ToString(), ProcessShapeType.UserTask.ToString()));
            Assert.That(process.Shapes[2].Name == ProcessShapeType.End.ToString(), I18NHelper.FormatInvariant("The shape returned was named '{0}' but '{1}' was expected", process.Shapes[2].Name, ProcessShapeType.End.ToString()));
            Assert.That(process.Shapes[2].ShapeType == ProcessShapeType.End, I18NHelper.FormatInvariant("The shape returned was of type '{0}' but '{1}' was expected", process.Shapes[2].ShapeType.ToString(), ProcessShapeType.End.ToString()));
        }
    }
}
