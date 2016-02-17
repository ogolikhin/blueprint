using Common;
using System.Linq;
using System.Collections;
using CustomAttributes;
using Model;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using System.Collections.Generic;

namespace StorytellerTests
{
    [TestFixture]
    [Category(Categories.Storyteller)]
    [Explicit(IgnoreReasons.DeploymentNotReady)]
    public class BasicTests
    {
        private IAdminStore _adminStore;
        private IStoryteller _storyteller;
        private IUser _user;
        private IArtifact _artifact;
        private IProject _project;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _storyteller = StorytellerFactory.GetStorytellerFromTestConfig();
            _user = UserFactory.CreateUserAndAddToDatabase();
            int projectId = 1; // using default project id: 1
            _project = ProjectFactory.CreateProject(id: projectId);

            // Get a valid token for the user.
            ISession session = _adminStore.AddSession(_user.Username, _user.Password);
            _user.SetToken(session.SessionId);
            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.AccessControlToken), "The user didn't get an Access Control token!");

            var process = new Artifact()
            {
                Id = 0,
                Name = "Test Process",
                ParentId = projectId, //we can use Project as a parent
                ProjectId = _project.Id,
                ArtifactTypeId = _storyteller.GetProcessTypeId(user: _user, project: _project)
                //ArtifactTypeId = 369 // Need to find a way to determine the artifact type id from server-side
            };
            

            _artifact = _storyteller.AddProcessArtifact(process, _user);
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            if (_artifact != null)
            {
                _storyteller.DeleteProcessArtifact(_artifact, _user);
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

        [Explicit(IgnoreReasons.UnderDevelopment)]
        [TestCase(5, 4, ProcessType.BusinessProcess)]
        public void GetDefaultProcess_VerifyReturnedProcess(int defaultShapesLength, int defaultLinksLength, ProcessType processType)
        {
            var process = _storyteller.GetProcess(_user, _artifact.Id);
            Assert.IsNotNull(process, "The returned process was null.");
            Assert.That(process.Id == _artifact.Id, I18NHelper.FormatInvariant("The ID of the returned process was '{0}', but '{1}' was expected.", process.Id, _artifact.Id));
            Assert.That(process.Shapes.Length == defaultShapesLength, I18NHelper.FormatInvariant("The number of shapes in a default process is {0} but {1} shapes were returned.", defaultShapesLength, process.Shapes.Length));
            Assert.That(process.Links.Length == defaultLinksLength, I18NHelper.FormatInvariant("The number of links in a default process is {0} but {1} links were returned.", defaultLinksLength, process.Links.Length));
            Assert.That(process.Type == processType, I18NHelper.FormatInvariant("The process type returned was '{0}', but '{1}' was expected", process.Type.ToString(), processType.ToString()));
            Assert.That(process.Shapes[0].Name == ProcessShapeType.Start.ToString(), I18NHelper.FormatInvariant("The shape returned was named '{0}', but '{1}' was expected", process.Shapes[0].Name, ProcessShapeType.Start.ToString()));
            Assert.That(process.Shapes[0].ShapeType == ProcessShapeType.Start, I18NHelper.FormatInvariant("The shape returned was of type '{0}', but '{1}' was expected", process.Shapes[0].ShapeType.ToString(), ProcessShapeType.Start.ToString()));
            Assert.That(process.Shapes[1].Name == Process.DefaulPreconditionName, I18NHelper.FormatInvariant("The shape returned was named '{0}' but '{1}' was expected", process.Shapes[1].Name, Process.DefaulPreconditionName));
            Assert.That(process.Shapes[1].ShapeType == ProcessShapeType.PreconditionSystemTask, I18NHelper.FormatInvariant("The shape returned was of type '{0}' but '{1}' was expected", process.Shapes[1].ShapeType.ToString(), ProcessShapeType.PreconditionSystemTask.ToString()));
            Assert.That(process.Shapes[2].Name == Process.DefaultUserTaskName, I18NHelper.FormatInvariant("The shape returned was named '{0}' but '{1}' was expected", process.Shapes[2].Name, Process.DefaultUserTaskName));
            Assert.That(process.Shapes[2].ShapeType == ProcessShapeType.UserTask, I18NHelper.FormatInvariant("The shape returned was of type '{0}' but '{1}' was expected", process.Shapes[2].ShapeType.ToString(), ProcessShapeType.UserTask.ToString()));
            Assert.That(process.Shapes[3].Name == Process.DefaultSystemTaskName, I18NHelper.FormatInvariant("The shape returned was named '{0}' but '{1}' was expected", process.Shapes[3].Name, Process.DefaultSystemTaskName));
            Assert.That(process.Shapes[3].ShapeType == ProcessShapeType.SystemTask, I18NHelper.FormatInvariant("The shape returned was of type '{0}' but '{1}' was expected", process.Shapes[3].ShapeType.ToString(), ProcessShapeType.SystemTask.ToString()));
            Assert.That(process.Shapes[4].Name == ProcessShapeType.End.ToString(), I18NHelper.FormatInvariant("The shape returned was named '{0}' but '{1}' was expected", process.Shapes[4].Name, ProcessShapeType.End.ToString()));
            Assert.That(process.Shapes[4].ShapeType == ProcessShapeType.End, I18NHelper.FormatInvariant("The shape returned was of type '{0}' but '{1}' was expected", process.Shapes[4].ShapeType.ToString(), ProcessShapeType.End.ToString()));
        }

        [Test]
        public void GetProcesses_ReturnedListContainsCreatedProcess()
        {
            IList<IProcess> processList = null;
            Assert.DoesNotThrow(() =>
            {
                processList = _storyteller.GetProcesses(_user, 1);
            }, "GetProcesses must not return an error.");
            var results = processList.Where(p => (p.Name == _artifact.Name && p.TypePreffix == "SP")).ToList();
            Assert.IsTrue(results.Count > 0, "List of processes must have newly created process, but it doesn't.");
        }
    }
}
