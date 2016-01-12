using System.Collections.Generic;
using CustomAttributes;
using Common;
using Model;
using Model.Factories;
using NUnit.Framework;
using Model.Impl;

namespace OpenAPITests
{
    [TestFixture]
    [Category(Categories.OpenAPI)]
    public class ProjectTests
    {
        private IBlueprintServer _server = BlueprintServerFactory.GetBlueprintServerFromTestConfig();
        private IUser _user = null;

        [SetUp]
        public void SetUp()
        {
            _user = UserFactory.CreateUserAndAddToDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            if (_user != null)
            {
                _user.DeleteUser();
                _user = null;
            }
        }

        [Test]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        public void GetProjects()
        {
            // TODO: Create some projects.

            List<IProject> projects = new Project().GetProjects(_server.Address, _user);
            Logger.WriteDebug("Number of projects returned = {0}", projects.Count);

            foreach (IProject project in projects)
            {
                Logger.WriteDebug(project.ToString());
            }

            // TODO: Verify that the projects we created were returned in the list of projects.

            // TODO: Delete the projects we added.
        }

        [Test]
        [Ignore(IgnoreReasons.UnderDevelopment)]
        // TODO: Need to implement Project.Equals(), CreateProject() & DeleteProject().
        public void GetProject()
        {
            // Setup: Create a project and collect the projectID
            IProject project = ProjectFactory.CreateProject();

            try
            {
                // Try to get the project we added.
                IProject foundProject = new Project().GetProject(_server.Address, project.Id, _user);

                // Verify that the projects we created were returned in the list of projects.
                Assert.That(project.Equals(foundProject),
                    "The project we found on the server is not the same as the project we added!\n--> Added project = {0}\n--> Found project = {1}",
                    project.ToString(), foundProject.ToString());
            }
            finally
            {
                // Cleanup: Delete the project we added.
                project.DeleteProject();
            }
        }
    }
}
