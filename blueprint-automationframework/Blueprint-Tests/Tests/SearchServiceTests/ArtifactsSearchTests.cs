using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using Model.SearchServiceModel.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommon;
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
        private IProject _firstProject = null;
        private IProject _secondProject = null;
        private List<IProject> _projects = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _projects = ProjectFactory.GetAllProjects(_adminUser, shouldRetrievePropertyTypes: true);

            Assert.IsTrue(_projects.Count >= 2, "These tests expect that Blueprint server has at least 2 projects.");

            _firstProject = _projects[0];
            _secondProject = _projects[1];
            _authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _firstProject);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [TestCase(0, 2)]
        [TestCase(2, 0)]
        [TestCase(2, 2)]
        [TestRail(182900)]
        [Description("Search published artifact across 2 projects by a partial part of its name.  Verify search results.")]
        public void SearchArtifactByPartialName_2Projects_VerifySearchResult(int startCharsToRemove, int endCharsToRemove)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_firstProject, _adminUser, BaseArtifactType.Actor);
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            string partialName = artifact.Name;

            if (startCharsToRemove > 0)
            {
                partialName = partialName.Remove(0, startCharsToRemove);
            }

            if (endCharsToRemove > 0)
            {
                partialName = partialName.Remove(partialName.Length - endCharsToRemove);
            }

            var searchCriteria = new FullTextSearchCriteria(partialName, selectedProjectIds);
            ItemSearchResult results = null;

            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_authorUser, searchCriteria); },
                "SearchItems should throw no errors.");

            // Verify:
            Assert.IsTrue(results.Items.Count > 0, "List of SearchItems shouldn't be empty.");
            Assert.IsTrue(results.PageItemCount > 0, "For non-empty list PageItemCount shouldn't be 0.");
            Assert.That(results.Items.Exists(si => DoesSearchItemCorrespondsToArtifact(artifact, si)), "Published artifact must be in search results.");
        }

        [TestCase]
        [TestRail(183272)]
        [Description("Search published artifact by full name within 2 projects, verify search results.")]
        public void SearchArtifactByFullName_2Projects_VerifySearchResult()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_firstProject, _adminUser, BaseArtifactType.Document);
            var artifact2 = Helper.CreateAndPublishArtifact(_firstProject, _adminUser, BaseArtifactType.Storyboard);

            Assert.AreNotEqual(artifact.Name, artifact2.Name, "Random artifacts should have different names.");
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria(artifact.Name, selectedProjectIds);
            ItemSearchResult results = null;

            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_authorUser, searchCriteria); },
                "SearchItems should throw no errors.");

            // Verify:
            Assert.IsTrue(results.Items.Count > 0, "List of SearchItems shouldn't be empty.");
            Assert.IsTrue(results.PageItemCount > 0, "For non-empty list PageItemCount shouldn't be 0.");
            Assert.That(results.Items.Exists(si => DoesSearchItemCorrespondsToArtifact(artifact, si)), "Published artifact must be in search results.");
            Assert.IsFalse(results.Items.Exists(si => DoesSearchItemCorrespondsToArtifact(artifact2, si)), "Search shouldn't return artifact with different name.");
        }

        [TestCase]
        [TestRail(183273)]
        [Description("Search artifact by non-existing name within 2 projects, verify that search results are empty.")]
        public void SearchArtifactNonExistingName_2Projects_VerifyEmptyResult()
        {
            // Setup:
            Helper.CreateAndPublishArtifact(_firstProject, _adminUser, BaseArtifactType.Glossary);

            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            string nonExistingArtifactName = RandomGenerator.RandomLowerCase(50);
            var searchCriteria = new FullTextSearchCriteria(nonExistingArtifactName, selectedProjectIds);
            ItemSearchResult results = null;

            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_authorUser, searchCriteria); },
                "SearchItems should throw no errors.");

            // Verify:
            Assert.AreEqual(0, results.Items.Count, "List of SearchItems should be empty.");
            Assert.AreEqual(0, results.PageItemCount, "For empty list PageItemCount should be empty.");
        }

        [TestCase]
        [TestRail(183346)]
        [Description("Search draft, never published artifact by full name, verify that search results are empty.")]
        public void SearchDraftNeverPublishedArtifactByName_ParentProject_VerifyEmptySearchResult()
        {
            // Setup:
            var artifact = Helper.CreateAndSaveArtifact(_firstProject, _authorUser, BaseArtifactType.Process);
            var searchCriteria = new FullTextSearchCriteria(artifact.Name, _firstProject.Id);
            ItemSearchResult results = null;

            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_authorUser, searchCriteria); },
                "SearchItems should throw no errors.");

            // Verify:
            Assert.AreEqual(0, results.PageItemCount, "For empty list PageItemCount should be 0.");
            Assert.IsFalse(results.Items.Exists(si => DoesSearchItemCorrespondsToArtifact(artifact, si)),
                "Search shouldn't return draft never published artifact.");
        }

        [TestCase]
        [TestRail(183350)]
        [Description("Search deleted artifact by full name, verify that search results are empty.")]
        public void SearchDeletedArtifactByFullName_AllProjects_VerifySearchResult()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_firstProject, _adminUser, BaseArtifactType.UseCaseDiagram);
            artifact.Delete(_adminUser);
            artifact.Publish(_adminUser);

            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria(artifact.Name, selectedProjectIds);
            ItemSearchResult results = null;

            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_adminUser, searchCriteria); },
                "SearchItems should throw no errors.");

            // Verify:
            Assert.AreEqual(0, results.Items.Count, "List of SearchItems should be empty.");
            Assert.AreEqual(0, results.PageItemCount, "For empty list PageItemCount should be 0.");
        }

        [TestCase]
        [TestRail(183347)]
        [Description("Search published artifact by full name in 2 projects, user has access to both projects, verify that artifact could be found in both projects.")]
        public void SearchArtifactByName_2Projects_VerifyArtifactCanBeFoundInBoth()
        {
            // Setup:
            string artifactName = RandomGenerator.RandomAlphaNumeric(12);
            var artifact = Helper.CreateAndPublishArtifact(_firstProject, _adminUser, BaseArtifactType.UIMockup, name: artifactName);
            Helper.CreateAndPublishArtifact(_secondProject, _adminUser, BaseArtifactType.UIMockup, name: artifactName);

            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria(artifact.Name, selectedProjectIds);
            ItemSearchResult results = null;

            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_adminUser, searchCriteria); },
                "SearchItems should throw no errors.");

            // Verify:
            Assert.IsTrue(results.PageItemCount > 0, "For non-empty list PageItemCount shouldn't be 0.");
            Assert.IsTrue(results.Items.Exists(si => DoesSearchItemCorrespondsToArtifact(artifact, si)), "Search should return published artifact.");
            Assert.IsTrue(results.Items.Exists(si => si.ProjectId == _firstProject.Id), "Artifact should be found in first project.");
            Assert.IsTrue(results.Items.Exists(si => si.ProjectId == _secondProject.Id), "Artifact should be found in second project.");
        }

        [TestCase]
        [TestRail(183348)]
        [Description("Search published artifact by full name in 2 projects, user has access to one project, verify that search results has accessible project only.")]
        public void SearchArtifactByName_2ProjectsUserHasAccessTo1Project_VerifyResults()
        {
            // Setup:
            string artifactName = RandomGenerator.RandomAlphaNumeric(12);
            var artifact = Helper.CreateAndPublishArtifact(_firstProject, _adminUser, BaseArtifactType.UseCase, name: artifactName);
            Helper.CreateAndPublishArtifact(_secondProject, _adminUser, BaseArtifactType.UIMockup, name: artifactName);

            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria(artifact.Name, selectedProjectIds);
            ItemSearchResult results = null;

            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_authorUser, searchCriteria); },
                "SearchItems should throw no errors.");

            // Verify:
            Assert.IsTrue(results.PageItemCount > 0, "For non-empty list PageItemCount shouldn't be 0.");
            Assert.IsTrue(results.Items.Exists(si => DoesSearchItemCorrespondsToArtifact(artifact, si)), "Search should return published artifact.");
            Assert.IsTrue(results.Items.Exists(si => si.ProjectId == _firstProject.Id), "Artifact should be found in first project.");
            Assert.IsFalse(results.Items.Exists(si => si.ProjectId == _secondProject.Id), "Artifact shouldn't be found in second project to which user has no access.");
        }

        [TestCase]
        [TestRail(183349)]
        [Description("Search published artifact by name and ItemTypeId in 2 projects, user has access to both projects, verify that only expected artifact could be found.")]
        public void SearchArtifactByNameAndItemTypeId_AllProjects_VerifyResults()
        {
            // Setup:
            string artifactName = RandomGenerator.RandomAlphaNumeric(12);
            List<IArtifact> artifacts = new List<IArtifact>();

            // Create and publish Storyboard and Actor in each project.  All artifacts have the same name.
            foreach (var pr in _projects)
            {
                artifacts.Add(Helper.CreateAndPublishArtifact(pr, _adminUser, BaseArtifactType.Storyboard, name: artifactName));
                artifacts.Add(Helper.CreateAndPublishArtifact(pr, _adminUser, BaseArtifactType.Actor, name: artifactName));
            }

            Assert.AreEqual(2 * _projects.Count, artifacts.Count, "Expected number of artifacts is number of projects times 2.");
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);

            // Create list of TypeId for search criteria, TypeId depends from Project; TypeId == ArtifactTypeId.
            List<int> actorTypeIds = (artifacts.FindAll(a => a.BaseArtifactType == BaseArtifactType.Actor)).ConvertAll(a =>a.ArtifactTypeId);
            var nameSearchCriteria       = new FullTextSearchCriteria(artifactName, selectedProjectIds);                // Search by name across all projects.
            var itemTypeIdSearchCriteria = new FullTextSearchCriteria(artifactName, selectedProjectIds, actorTypeIds);  // Search by name and TypeId across all projects.

            ItemSearchResult nameSearchResult = Helper.SearchService.SearchItems(_adminUser, nameSearchCriteria);
            Assert.AreEqual(artifacts.Count, nameSearchResult.Items.Count,
                "Search by name across all projects should return all artifacts with the artifactName name.");

            ItemSearchResult nameAndTypeIdSearchResult = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                nameAndTypeIdSearchResult = Helper.SearchService.SearchItems(_adminUser, itemTypeIdSearchCriteria);
            }, "SearchItems should throw no errors.");

            // Verify:
            Assert.IsTrue(nameSearchResult.PageItemCount > 0, "For non-empty list PageItemCount shouldn't be 0.");

            foreach (var p in _projects)
            {
                Assert.IsTrue(nameAndTypeIdSearchResult.Items.Exists(si => si.ProjectId == p.Id),
                    "Artifact of specified name and type should be found each project.");
            }

            foreach (var si in nameAndTypeIdSearchResult.Items)
            {
                Assert.IsTrue(artifacts.Exists(a => DoesSearchItemCorrespondsToArtifact(a, si)),
                    "Search results must include all expected artifacts.");
            }
        }

        /// <summary>
        /// Returns true if SearchItem contains information about Artifact and false otherwise
        /// </summary>
        /// <param name="artifact">artifact to compare</param>
        /// <param name="searchItem">searchItem to compare</param>
        private static bool DoesSearchItemCorrespondsToArtifact(IArtifact artifact, SearchItem searchItem)
        {
            return ((searchItem.Id == artifact.Id) &&
            (searchItem.Name == artifact.Name) &&
            (searchItem.ProjectId == artifact.ProjectId) &&
            //(searchItem.TypeName == artifact.ArtifactTypeName) &&
            (searchItem.ItemTypeId == artifact.ArtifactTypeId));
        }
    }
}
