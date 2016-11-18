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
using Model.ArtifactModel.Enums;

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
            _projects = ProjectFactory.GetProjects(_adminUser, numberOfProjects: 2, shouldRetrievePropertyTypes: true);

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

            var searchCriteria = new ItemNameSearchCriteria(partialName, selectedProjectIds);
            ItemSearchResult results = null;

            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_authorUser, searchCriteria); },
                "SearchItems should throw no errors.");

            // Verify:
            Assert.AreEqual(1, results.Items.Count, "List of SearchItems should have 1 item.");
            Assert.AreEqual(1, results.PageItemCount, "PageItemCount should be 1.");
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
            var searchCriteria = new ItemNameSearchCriteria(artifact.Name, selectedProjectIds);
            ItemSearchResult results = null;

            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_authorUser, searchCriteria); },
                "SearchItems should throw no errors.");

            // Verify:
            Assert.AreEqual(1, results.Items.Count, "List of SearchItems should have 1 item.");
            Assert.AreEqual(1, results.PageItemCount, "PageItemCount should be 1.");
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
            var searchCriteria = new ItemNameSearchCriteria(nonExistingArtifactName, selectedProjectIds);
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
            var searchCriteria = new ItemNameSearchCriteria(artifact.Name, _firstProject.Id);
            ItemSearchResult results = null;

            string separatorString = " > ";

            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_authorUser, searchCriteria, separatorString: separatorString); },
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
            var searchCriteria = new ItemNameSearchCriteria(artifact.Name, selectedProjectIds);
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
            var searchCriteria = new ItemNameSearchCriteria(artifact.Name, selectedProjectIds);
            ItemSearchResult results = null;

            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_adminUser, searchCriteria); },
                "SearchItems should throw no errors.");

            // Verify:
            Assert.AreEqual(2, results.Items.Count, "List of SearchItems should have 2 items.");
            Assert.AreEqual(2, results.PageItemCount, "PageItemCount should be 2.");
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
            var searchCriteria = new ItemNameSearchCriteria(artifact.Name, selectedProjectIds);
            ItemSearchResult results = null;

            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_authorUser, searchCriteria); },
                "SearchItems should throw no errors.");

            // Verify:
            Assert.AreEqual(1, results.Items.Count, "List of SearchItems should have 1 item.");
            Assert.AreEqual(1, results.PageItemCount, "PageItemCount should be 1.");
            Assert.IsTrue(results.Items.Exists(si => DoesSearchItemCorrespondsToArtifact(artifact, si)), "Search should return published artifact.");
            Assert.IsTrue(results.Items.Exists(si => si.ProjectId == _firstProject.Id), "Artifact should be found in first project.");
            Assert.IsFalse(results.Items.Exists(si => si.ProjectId == _secondProject.Id), "Artifact shouldn't be found in second project to which user has no access.");
        }

        [TestCase]
        [TestRail(183349)]
        [Description("Search published artifact by name and ItemTypeId in 2 projects, user has access to both projects, verify that only expected artifact could be found.")]
        public void SearchArtifactByNameAndItemTypePredefinedId_AllProjects_VerifyResults()
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
            var nameSearchCriteria       = new ItemNameSearchCriteria(artifactName, selectedProjectIds);                // Search by name across all projects.
            var itemTypeIdSearchCriteria = new ItemNameSearchCriteria(artifactName, selectedProjectIds, (int)ItemTypePredefined.Actor);  // Search by name and TypeId across all projects.

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

        [TestCase]
        [TestRail(191041)]
        [Description("Search published artifact by full name within 2 projects, verify search item has expected version.")]
        public void SearchArtifactByFullName_2Projects_VerifyVersionInfo()
        {
            // Setup:
            int numberOfVersions = 3;
            var artifact = Helper.CreateAndPublishArtifact(_firstProject, _adminUser, BaseArtifactType.Document,
                numberOfVersions: numberOfVersions);

            artifact.Save(_authorUser);

            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new ItemNameSearchCriteria(artifact.Name, selectedProjectIds);
            searchCriteria.IncludeArtifactPath = true;
            ItemSearchResult results = null;

            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_authorUser, searchCriteria); },
                "SearchItems should throw no errors.");

            // Verify:
            Assert.AreEqual(1, results.Items.Count, "List of SearchItems should have 1 item.");
            Assert.AreEqual(1, results.PageItemCount, "PageItemCount should be 1.");
            Assert.That(results.Items.Exists(si => DoesSearchItemCorrespondsToArtifact(artifact, si)), "Published artifact must be in search results.");

            Assert.AreEqual(numberOfVersions, results.Items[0].Version, "Version should have expected value.");
        }

        [TestCase]
        [TestRail(191042)]
        [Description("Search published artifact by full name within 2 projects, verify search item has expected Path.")]
        public void SearchArtifactByFullName_2Projects_VerifyPath()
        {
            // Setup:
            var parentFolder = Helper.CreateAndPublishArtifact(_firstProject, _adminUser, BaseArtifactType.PrimitiveFolder);
            var parentArtifact = Helper.CreateAndPublishArtifact(_firstProject, _adminUser, BaseArtifactType.UseCase, parentFolder);
            var artifact = Helper.CreateAndPublishArtifact(_firstProject, _adminUser, BaseArtifactType.Actor, parentArtifact);

            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new ItemNameSearchCriteria(artifact.Name, selectedProjectIds);
            searchCriteria.IncludeArtifactPath = true;
            ItemSearchResult results = null;

            string separatorString = " > ";
            string expectedPath = string.Concat(_firstProject.Name, separatorString, parentFolder.Name, separatorString,
                parentArtifact.Name);

            // Execute:
            Assert.DoesNotThrow(() => {
                results = Helper.SearchService.SearchItems(_authorUser, searchCriteria,
                separatorString: separatorString); }, "SearchItems should throw no errors.");

            // Verify:
            Assert.AreEqual(1, results.Items.Count, "List of SearchItems should have 1 item.");
            Assert.AreEqual(1, results.PageItemCount, "PageItemCount should be 1.");
            Assert.That(results.Items.Exists(si => DoesSearchItemCorrespondsToArtifact(artifact, si)), "Published artifact must be in search results.");
            StringAssert.AreEqualIgnoringCase(expectedPath, results.Items[0].Path, "Returned Path should have expected value");
        }

        [TestCase]
        [TestRail(191045)]
        [Description("Create and publish 11 artifacts, search published artifact by full name in all projects, verify that PageItemCount is 10.")]
        public void SearchArtifactByName_2Projects_VerifyPageSizeDefaultValue10()
        {
            // Setup:
            string artifactName = RandomGenerator.RandomAlphaNumeric(12);
            int numberOfArtifacts = 11;
            for (int i = 0; i < numberOfArtifacts; i++)
            {
                Helper.CreateAndPublishArtifact(_firstProject, _adminUser, BaseArtifactType.UIMockup, name: artifactName);
            }
            
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new ItemNameSearchCriteria(artifactName, selectedProjectIds);
            ItemSearchResult results = null;

            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_adminUser, searchCriteria); },
                "SearchItems should throw no errors.");

            // Verify:
            Assert.AreEqual(10, results.PageItemCount, "Default value of PageItemCount should be 10.");
        }

        [TestCase]
        [TestRail(191046)]
        [Description("Search published artifact by full name in 2 projects with PageSize, verify that number of search result is limited by PageSize.")]
        public void SearchArtifactByName_SetPageSize_VerifyPageSizeHasExpectedValue()
        {
            // Setup:
            string artifactName = RandomGenerator.RandomAlphaNumeric(12);
            int numberOfArtifacts = 5;
            for (int i = 0; i < numberOfArtifacts; i++)
            {
                Helper.CreateAndPublishArtifact(_firstProject, _adminUser, BaseArtifactType.UIMockup, name: artifactName);
            }
            
            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new ItemNameSearchCriteria(artifactName, selectedProjectIds);
            ItemSearchResult results = null;
            int pageSize = 3;
            
            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_adminUser, searchCriteria, pageSize: pageSize); },
                "SearchItems should throw no errors.");

            // Verify:
            Assert.AreEqual(pageSize, results.PageItemCount, "Default value of PageItemCount should be {0}.", pageSize);
            Assert.AreEqual(pageSize, results.Items.Count, "When expected number of search results is more than PageItemCount, number of returned items should be PageItemCount.");
        }

        [TestCase]
        [TestRail(191054)]
        [Description("Search published artifact by full name within all available projects with IncludeArtifactPath = false, verify search item has Path with null value.")]
        public void SearchArtifactByFullName_IncludeArtifactPathFalse_VerifyPathIsNull()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_firstProject, _adminUser, BaseArtifactType.DomainDiagram);

            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new ItemNameSearchCriteria(artifact.Name, selectedProjectIds);
            searchCriteria.IncludeArtifactPath = false;
            ItemSearchResult results = null;

            // Execute:
            Assert.DoesNotThrow(() => {
                results = Helper.SearchService.SearchItems(_authorUser, searchCriteria);
            }, "SearchItems should throw no errors.");

            // Verify:
            Assert.AreEqual(1, results.Items.Count, "List of SearchItems should have 1 item.");
            Assert.AreEqual(1, results.PageItemCount, "PageItemCount should be 1.");
            Assert.That(results.Items.Exists(si => DoesSearchItemCorrespondsToArtifact(artifact, si)), "Published artifact must be in search results.");
            Assert.IsNull(results.Items[0].Path, "Path should be null when IncludeArtifactPath is false");
        }

        [TestCase]
        [TestRail(191206)]
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
            int actorTypeIdsInFirstProject = (artifacts.Find(a => (a.BaseArtifactType == BaseArtifactType.Actor &&
            a.ProjectId == _projects[0].Id))).ArtifactTypeId;
            var itemTypeIdSearchCriteria = new ItemNameSearchCriteria(artifactName, selectedProjectIds, new List<int> { actorTypeIdsInFirstProject });
                //itemTypeId: actorTypeIdsInFirstProject); // Search by name and TypeId across all projects.

            var nameSearchCriteria = new ItemNameSearchCriteria(artifactName, selectedProjectIds); // Search by name across all projects.

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
            Assert.AreEqual(1, nameAndTypeIdSearchResult.Items.Count, "List of SearchItems should have 1 item.");
            Assert.AreEqual(1, nameAndTypeIdSearchResult.PageItemCount, "PageItemCount should be 1.");
            //Assert.IsTrue(nameAndTypeIdSearchResult.PageItemCount > 0, "For non-empty list PageItemCount shouldn't be 0.");

            Assert.IsTrue(nameAndTypeIdSearchResult.Items.Exists(si => si.ProjectId == _projects[0].Id),
                    "Artifact of specified name and type should be found in the first project.");
            Assert.IsFalse(nameAndTypeIdSearchResult.Items.Exists(si => si.ProjectId == _projects[1].Id),
                    "Artifact of specified name and type shouldn't be found in the second project.");

            foreach (var si in nameAndTypeIdSearchResult.Items)
            {
                Assert.IsTrue(artifacts.Exists(a => DoesSearchItemCorrespondsToArtifact(a, si)),
                    "Search results must include all expected artifacts.");
            }
        }

        [TestCase]
        [TestRail(191210)]
        [Explicit(IgnoreReasons.ProductBug)] // https://trello.com/c/VLdz6frC SearchItems doesn't return HasChildren
        [Description("Search published artifact by full name within 2 projects, verify search item has expected HasChildren.")]
        public void SearchArtifactByFullName_2ProjectsArtifactWithChild_VerifyHasChildren()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_firstProject, _adminUser, BaseArtifactType.UseCase);
            var childArtifact = Helper.CreateAndPublishArtifact(_firstProject, _adminUser, BaseArtifactType.Process, artifact);

            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new ItemNameSearchCriteria(artifact.Name, selectedProjectIds);
            searchCriteria.IncludeArtifactPath = true;
            ItemSearchResult results = null;

            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_authorUser, searchCriteria); },
                "SearchItems should throw no errors.");

            // Verify:
            Assert.AreEqual(1, results.Items.Count, "List of SearchItems should have 1 item.");
            Assert.AreEqual(1, results.PageItemCount, "PageItemCount should be 1.");
            Assert.That(results.Items.Exists(si => DoesSearchItemCorrespondsToArtifact(artifact, si)), "Published artifact must be in search results.");
            Assert.IsTrue(results.Items[0].HasChildren, "Artifact in search results should have HasChildren set to true.");
            Assert.AreEqual(childArtifact.Id, results.Items[0].Id);
        }

        [TestCase]
        [TestRail(191211)]
        [Description("Search published artifact by full name within 2 projects, verify search item has expected LockedByUser value.")]
        public void SearchArtifactByFullName_2ProjectsArtifactLockedByOtherUser_VerifyLockedBy()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_firstProject, _adminUser, BaseArtifactType.UseCase);

            var selectedProjectIds = _projects.ConvertAll(project => project.Id);
            var searchCriteria = new ItemNameSearchCriteria(artifact.Name, selectedProjectIds);
            searchCriteria.IncludeArtifactPath = true;
            ItemSearchResult results = null;

            artifact.Lock(_adminUser);

            // Execute:
            Assert.DoesNotThrow(() => { results = Helper.SearchService.SearchItems(_authorUser, searchCriteria); },
                "SearchItems should throw no errors.");

            // Verify:
            Assert.AreEqual(1, results.Items.Count, "List of SearchItems should have 1 item.");
            Assert.AreEqual(1, results.PageItemCount, "PageItemCount should be 1.");
            Assert.That(results.Items.Exists(si => DoesSearchItemCorrespondsToArtifact(artifact, si)), "Published artifact must be in search results.");
            Assert.IsNotNull(results.Items[0].LockedByUser, "LockedByUser shouldn't be null.");
            Assert.AreEqual(_adminUser.Id, results.Items[0].LockedByUser.Id, "User id should have expected value.");
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
            (searchItem.ItemTypeId == artifact.ArtifactTypeId));
        }
    }
}
