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
        private List<IProject> _projects = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _projects = ProjectFactory.GetAllProjects(_adminUser, shouldRetrievePropertyTypes: true);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [TestCase]
        [TestRail(1)]
        [Description("Searching with optional parameter, page. Execute Search - Must return SearchResult that uses the page value.")]
        public void SearchArtifact_AllProjects_VerifySearchResult()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_projects[0], _adminUser, BaseArtifactType.GenericDiagram);
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria(GetRandomSubString(artifact.Name), selectedProjectIds);
            ItemSearchResult results = null;

            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_adminUser, searchCriteria); },
                "no errors");

            // Verify:
            Assert.IsTrue(results.SearchItems.Count > 0, "non-empty list");
            Assert.IsTrue(artifact.Id == results.SearchItems[0].ArtifactId, "must have expected artifact.");
        }

        private static string GetRandomSubString(string name)
        {
            var initialLength = name.Length;

            if (initialLength <= 1)
            {
                return name;
            }
            else
            {
                var startIndex = RandomGenerator.RandomNumber(initialLength - 1);
                var length = 1 + RandomGenerator.RandomNumber(initialLength - startIndex - 1);
                return name.Substring(startIndex, length);
            }
        }
    }
}
