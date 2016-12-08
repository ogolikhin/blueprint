using System.Collections.Generic;
using CustomAttributes;
using Common;
using Helper;
using Model;
using Model.Factories;
using NUnit.Framework;
using Model.Impl;
using TestCommon;

namespace OpenAPITests
{
    [TestFixture]
    [Category(Categories.OpenApi)]
    public class ProjectTests : TestBase
    {
        private IBlueprintServer _server = BlueprintServerFactory.GetBlueprintServerFromTestConfig();
        private IUser _user = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.OpenApiToken);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [TestCase]
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
        }

        [TestCase]
        [Ignore(IgnoreReasons.UnderDevelopment)]    // TODO: Need to implement Project.Equals(), CreateProject() & DeleteProject().
        public void GetProject()
        {
            // Setup: Create a project and collect the projectID
            IProject project = Helper.CreateProject();

            // Try to get the project we added.
            IProject foundProject = new Project().GetProject(_server.Address, project.Id, _user);

            // Verify that the projects we created were returned in the list of projects.
            Assert.That(project.Equals(foundProject),
                "The project we found on the server is not the same as the project we added!\n--> Added project = {0}\n--> Found project = {1}",
                project.ToString(), foundProject.ToString());
        }
    }
}
