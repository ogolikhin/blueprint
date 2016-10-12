using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using Model.Impl;
using Model.SearchServiceModel.Impl;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TestCommon;
using Utilities;
using Common;
using Utilities.Factories;

namespace SearchServiceTests
{
    [TestFixture]
    [Category(Categories.SearchService)]
    public class ArtifactsSearchTests : TestBase
    {
        private IUser _adminUser = null;
        private IUser _authorUser = null;
        private IProject _project = null;
        private List<IProject> _projects = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _projects = ProjectFactory.GetAllProjects(_adminUser, shouldRetrievePropertyTypes: true);
            _project = _projects[0];
            _authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [TestCase]
        [TestRail(182900)]
        [Description("Search published artifact by random part of its name, verify search results.")]
        public void SearchArtifact_AllProjects_VerifySearchResult()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.GenericDiagram);
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria(GetRandomSubString(artifact.Name), selectedProjectIds);
            ItemSearchResult results = null;

            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_authorUser, searchCriteria); },
                "no errors");

            // Verify:
            Assert.IsTrue(results.SearchItems.Count > 0, "List of SearchItems shouldn't be empty.");
            Assert.That(results.SearchItems.Exists(si => si.ArtifactId == artifact.Id), "Published artifact must be in search results.");
        }

        /// <summary>
        /// Returns random substring with non-zero length from input string
        /// </summary>
        /// <param name="inputString">string to create substring from</param>
        /// <return>Random substring with non-zero length</return>
        private static string GetRandomSubString(string inputString)
        {
            var initialLength = inputString.Length;

            if (initialLength <= 1)
            {
                return inputString;
            }
            else
            {
                var startIndex = RandomGenerator.RandomNumber(initialLength - 1);
                var length = 1 + RandomGenerator.RandomNumber(initialLength - startIndex - 1);
                Logger.WriteInfo("inputString - {0}, searchString - {1}", inputString, inputString.Substring(startIndex, length));
                return inputString.Substring(startIndex, length);
            }
        }
    }
}
