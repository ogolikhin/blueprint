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
        private IProject _projectTest = null;
        private List<IProject> _projects = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _projects = ProjectFactory.GetAllProjects(_adminUser, shouldRetrievePropertyTypes: true);
            _projectTest = _projects[0];
            _authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _projectTest);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [TestCase]
        [TestRail(182900)]
        [Description("Search published artifact across 2 projects by random part of its name, verify search results.")]
        public void SearchArtifact_2Projects_VerifySearchResult()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.Actor);
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria(GetRandomSubString(artifact.Name), selectedProjectIds);
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
            var artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.Document);
            var artifact2 = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.Storyboard);
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
            Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.Glossary);
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
            var artifact = Helper.CreateAndSaveArtifact(_projectTest, _authorUser, BaseArtifactType.Process);
            var searchCriteria = new FullTextSearchCriteria(artifact.Name, _projectTest.Id);
            ItemSearchResult results = null;

            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_authorUser, searchCriteria); },
                "SearchItems should throw no errors.");

            // Verify:
            Assert.AreEqual(0, results.PageItemCount, "For empty list PageItemCount should be 0.");
            Assert.IsFalse(results.Items.Exists(si => DoesSearchItemCorrespondsToArtifact(artifact, si)), "Search shouldn't return draft never published artifact.");
        }

        [TestCase]
        [TestRail(183350)]
        [Description("Search deleted artifact by full name, verify that search results are empty.")]
        public void SearchDeletedArtifactByFullName_AllProjects_VerifySearchResult()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.UseCaseDiagram);
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            artifact.Delete(_adminUser);
            artifact.Publish(_adminUser);
            var searchCriteria = new FullTextSearchCriteria(artifact.Name, selectedProjectIds);
            ItemSearchResult results = null;

            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_adminUser, searchCriteria); },
                "SearchItems should throw no errors.");

            // Verify:
            Assert.AreEqual(0, results.Items.Count, "List of SearchItems should be empty.");
            Assert.AreEqual(0, results.PageItemCount, "For empty list PageItemCount should be 0.");
        }

        #region Custom data tests

        [TestCase]
        [Category(Categories.CustomData)]
        [TestRail(183347)]
        [Description("Search published artifact by full name in 2 projects, user has access to both projects, verify that artifact could be found in both projects.")]
        public void SearchArtifactByName_2Projects_VerifyArtifactCanBeFoundInBoth()
        {
            // Setup:
            IProject _projectCustomData = ProjectFactory.GetProject(_adminUser, "Custom Data");
            string existingArtifactName = "InheritedFromParentActor"; //existing artifact name in Custom Data project
            var artifact = Helper.CreateAndSaveArtifact(_projectTest, _adminUser, BaseArtifactType.UIMockup, name: existingArtifactName);
            artifact.Publish(_adminUser);
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria(artifact.Name, selectedProjectIds);
            ItemSearchResult results = null;

            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_adminUser, searchCriteria); },
                "SearchItems should throw no errors.");

            // Verify:
            Assert.IsTrue(results.PageItemCount > 0, "For non-empty list PageItemCount shouldn't be 0.");
            Assert.IsTrue(results.Items.Exists(si => DoesSearchItemCorrespondsToArtifact(artifact, si)), "Search should return published artifact.");
            Assert.IsTrue(results.Items.Exists(si => si.ProjectId == _projectCustomData.Id), "Artifact should be found in Custom Data project.");
            Assert.IsTrue(results.Items.Exists(si => si.ProjectId == _projectTest.Id), "Artifact should be found in Test project.");
        }

        [TestCase]
        [Category(Categories.CustomData)]
        [TestRail(183348)]
        [Description("Search published artifact by full name in 2 projects, user has access to one project, verify that search results has accessible project only.")]
        public void SearchArtifactByName_2ProjectsUserHasAccessTo1Project_VerifyResults()
        {
            // Setup:
            IProject _projectCustomData = ProjectFactory.GetProject(_adminUser, "Custom Data");
            string existingArtifactName = "InheritedFromParentActor";//existing artifact name in Custom Data project
            var artifact = Helper.CreateAndSaveArtifact(_projectTest, _adminUser, BaseArtifactType.UseCase, name: existingArtifactName);
            artifact.Publish(_adminUser);
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new FullTextSearchCriteria(artifact.Name, selectedProjectIds);
            ItemSearchResult results = null;

            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_authorUser, searchCriteria); },
                "SearchItems should throw no errors.");

            // Verify:
            Assert.IsTrue(results.PageItemCount > 0, "For non-empty list PageItemCount shouldn't be 0.");
            Assert.IsTrue(results.Items.Exists(si => DoesSearchItemCorrespondsToArtifact(artifact, si)), "Search should return published artifact.");
            Assert.IsTrue(results.Items.Exists(si => si.ProjectId == _projectTest.Id), "Artifact should be found in Test project.");
            Assert.IsFalse(results.Items.Exists(si => si.ProjectId == _projectCustomData.Id), "Artifact shouldn't be found in Custom Data project to which user has no access.");
        }

        [TestCase]
        [TestRail(183349)]
        [Description("Search published artifact by name and ItemTypeId in 2 projects, user has access to both projects, verify that only expected artifact could be found.")]
        public void SearchArtifactByNameAndItemTypeId_AllProjects_VerifyResults()
        {
            // Setup:
            Assert.IsTrue(_projects.Count >= 2, "This test expect that Blueprint server has at least 2 projects.");
            string artifactName = RandomGenerator.RandomAlphaNumeric(12);
            List<IArtifact> artifacts = new List<IArtifact>();

            // create and publish Storyboard and Actor in each project. All artifacts have the same name
            foreach (var pr in _projects)
            {
                artifacts.Add(Helper.CreateAndSaveArtifact(pr, _adminUser, BaseArtifactType.Storyboard, name: artifactName));
                artifacts.Add(Helper.CreateAndSaveArtifact(pr, _adminUser, BaseArtifactType.Actor, name: artifactName));
            }
            foreach (var a in artifacts)
            {
                Helper.ArtifactStore.PublishArtifact(a, _adminUser);
            }

            Assert.AreEqual(2 * _projects.Count, artifacts.Count, "Expected number of artifacts is number of projects times 2.");
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            //create list of TypeId for search criteria, TypeId depends from Project; TypeId == ArtifactTypeId
            List<int> actorTypeIds = (artifacts.FindAll(a => a.BaseArtifactType == BaseArtifactType.Actor)).ConvertAll(a =>a.ArtifactTypeId);
            var nameSearchCriteria = new FullTextSearchCriteria(artifactName, selectedProjectIds);//search by name across all projects
            var itemTypeIdSearchCriteria = new FullTextSearchCriteria(artifactName, selectedProjectIds, actorTypeIds);//search by name and TypeId across all projects

            ItemSearchResult nameSearchResult = Helper.SearchService.SearchItems(_adminUser, nameSearchCriteria);
            Assert.AreEqual(artifacts.Count, nameSearchResult.Items.Count, "Search by name across all projects should return all artifacts with the artifactName name.");

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
                Assert.IsTrue(nameAndTypeIdSearchResult.Items.Exists(si => si.ProjectId == p.Id), "Artifact of specified name and type should be found each project.");
            }
            foreach (var si in nameAndTypeIdSearchResult.Items)
            {
                Assert.IsTrue(artifacts.Exists(a => DoesSearchItemCorrespondsToArtifact(a, si)), "Search results must include all expected artifacts.");
            }
        }

        #endregion Custom data tests

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

        /// <summary>
        /// Returns true if SearchItem contains information about Artifact and false otherwise
        /// </summary>
        /// <param name="artifact">artifact to compare</param>
        /// <param name="searchItem">searchItem to compare</param>
        private static bool DoesSearchItemCorrespondsToArtifact(IArtifact artifact, SearchItem searchItem)
        {
            return ((searchItem.ArtifactId == artifact.Id) &&
            (searchItem.Name == artifact.Name) &&
            (searchItem.ProjectId == artifact.ProjectId) &&
            (searchItem.TypeName == artifact.ArtifactTypeName) &&
            (searchItem.ItemTypeId == artifact.ArtifactTypeId));
        }
    }
}
