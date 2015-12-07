
using System.Collections.Generic;
using CustomAttributes;
using Helper.Factories;
using Logging;
using Model;
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
        public void GetProject()
        {
            // TODO: Create some projects and collect the projectID
            // TODO: Verify that the projects we created were returned in the list of projects.
            IProject projects = new Project().GetProject(_server.Address, 1, _user);
            // TODO: Delete the projects we added.
        }
    }
}
