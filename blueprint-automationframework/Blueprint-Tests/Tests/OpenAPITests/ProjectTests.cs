﻿using CustomAttributes;
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
        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)] // Needs more asserts and custom data tag.
        public void GetProjects()
        {
            // TODO: Create some projects.

            var projects = new Project().GetProjects(_server.Address, _user);
            Logger.WriteDebug("Number of projects returned = {0}", projects.Count);

            foreach (var project in projects)
            {
                Logger.WriteDebug(project.ToString());
            }

            // TODO: Verify that the projects we created were returned in the list of projects.
        }

        [TestCase]
        [Ignore(IgnoreReasons.UnderDevelopmentQaDev)]    // TODO: Need to implement Project.Equals(), CreateProject() & DeleteProject(), or use CustomData.
        public void GetProject()
        {
            // Setup: Create a project and collect the projectID
            var project = Helper.CreateProject();

            // Try to get the project we added.
            var foundProject = new Project().GetProject(_server.Address, project.Id, _user);

            // Verify that the projects we created were returned in the list of projects.
            Assert.That(project.Equals(foundProject),
                "The project we found on the server is not the same as the project we added!\n--> Added project = {0}\n--> Found project = {1}",
                project.ToString(), foundProject.ToString());
        }
    }
}
