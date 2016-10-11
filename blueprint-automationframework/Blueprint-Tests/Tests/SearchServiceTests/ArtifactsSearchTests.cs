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
            _project = ProjectFactory.GetProject(_adminUser);
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
            CompareArtifactWithSearchItem(artifact, results.SearchItems[0]);
        }

        [TestCase]
        [TestRail(1)]
        [Description("Search published artifact by full name, verify search results.")]
        public void SearchArtifactByFullName_AllProjects_VerifySearchResult()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.GenericDiagram);
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria(artifact.Name, selectedProjectIds);
            ItemSearchResult results = null;

            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_authorUser, searchCriteria); },
                "no errors");

            // Verify:
            Assert.IsTrue(results.SearchItems.Count > 0, "List of SearchItems shouldn't be empty.");
            CompareArtifactWithSearchItem(artifact, results.SearchItems[0]);
        }

        [TestCase]
        [TestRail(2)]
        [Description("Search published artifact by full name, verify search results.")]
        public void SearchArtifactNonExistingName_AllProjects_VerifyEmptyResult()
        {
            // Setup:
            Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.GenericDiagram);
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria(RandomGenerator.RandomLowerCase(50), selectedProjectIds);
            ItemSearchResult results = null;

            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_authorUser, searchCriteria); },
                "no errors");

            // Verify:
            Assert.IsTrue(results.SearchItems.Count == 0, "List of SearchItems should be empty.");
        }

        [TestCase]
        [TestRail(3)]
        [Description("Search published artifact by full name, verify search results.")]
        public void SearchDraftArtifactByFullName_AllProjects_VerifySearchResult()
        {
            // Setup:
            var artifact = Helper.CreateAndSaveArtifact(_project, _adminUser, BaseArtifactType.GenericDiagram);
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria(artifact.Name, selectedProjectIds);
            ItemSearchResult results = null;

            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_authorUser, searchCriteria); },
                "no errors");

            // Verify:
            Assert.IsTrue(results.SearchItems.Count > 0, "List of SearchItems shouldn't be empty.");
            CompareArtifactWithSearchItem(artifact, results.SearchItems[0]);
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
                return inputString.Substring(startIndex, length);
            }
        }

        /// <summary>
        /// Compares Artifact with SearchItem
        /// </summary>
        /// <param name="artifact">artifact to compare</param>
        /// <param name="searchItem">searchItem to compare</param>
        private static void CompareArtifactWithSearchItem(IArtifact artifact, SearchItem searchItem)
        {
            Assert.AreEqual(searchItem.ArtifactId, artifact.Id, "");
            Assert.AreEqual(searchItem.Name, artifact.Name, ".");
            Assert.AreEqual(searchItem.ProjectId, artifact.ProjectId, "..");
            Assert.AreEqual(searchItem.TypeName, artifact.ArtifactTypeName, "..");
        }
    }
}
