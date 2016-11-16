using System.Collections.Generic;
using System.Linq;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
using Utilities;
using Newtonsoft.Json;
using Model.Impl;
using Utilities.Factories;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class PublishArtifactTests : TestBase
    {
        const string PUBLISH_PATH = RestPaths.Svc.ArtifactStore.Artifacts.PUBLISH;

        private IUser _user = null;
        private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region 200 OK Tests

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(165856)]
        [Description("Create & save a single artifact.  Publish the artifact.  Verify publish is successful and that artifact version is now 1.")]
        public void PublishArtifact_SingleSavedArtifact_ArtifactHasVersion1(BaseArtifactType artifactType)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, author, artifactType);
            var artifactHistoryBefore = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, author);
            Assert.AreEqual(int.MaxValue, artifactHistoryBefore[0].VersionId, "Version ID before publish should be {0}!", int.MaxValue);

            // Execute:
            INovaArtifactsAndProjectsResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifact(artifact, author),
                "'POST {0}' should return 200 OK if a valid artifact ID is sent!", PUBLISH_PATH);

            // Verify:
            ArtifactStoreHelper.AssertOnlyExpectedProjectWasReturned(publishResponse.Projects, _project);
            Assert.AreEqual(1, publishResponse.Artifacts.Count, "There should only be 1 published artifact returned!");
            ArtifactStoreHelper.AssertNovaArtifactResponsePropertiesMatchWithArtifact(publishResponse.Artifacts.First(), artifact, expectedVersion: 1);

            var artifactHistoryAfter = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, author);
            Assert.AreEqual(1, artifactHistoryAfter[0].VersionId, "Version ID after publish should be 1!");
        }

        [TestCase(BaseArtifactType.Actor, 2)]
        [TestCase(BaseArtifactType.Process, 3)]
        [TestRail(165968)]
        [Description("Create & publish a single artifact several times, then save to create a draft.  Publish the artifact." +
            "Verify publish is successful and that artifact has the expected version.")]
        public void PublishArtifact_SinglePublishedArtifactWithMultipleVersionsWithDraft_ArtifactHasExpectedVersion(BaseArtifactType artifactType, int numberOfVersions)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            IArtifact artifactWithMultipleVersions = Helper.CreateAndPublishArtifact(_project, author, artifactType, numberOfVersions: numberOfVersions);

            var artifactHistoryBefore = Helper.ArtifactStore.GetArtifactHistory(artifactWithMultipleVersions.Id, author);
            Assert.AreEqual(numberOfVersions, artifactHistoryBefore[0].VersionId, "Version ID before Nova publish should be {0}!", numberOfVersions);

            artifactWithMultipleVersions.Save();    // Now save to make a draft.

            // Execute:
            INovaArtifactsAndProjectsResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifact(artifactWithMultipleVersions, author),
                "'POST {0}' should return 200 OK if a valid artifact ID is sent!", PUBLISH_PATH);

            // Verify:
            int expectedVersion;

            checked  // This checked section is needed to fix warning:  CA2233: Operations should not overflow
            {
                expectedVersion = numberOfVersions + 1;
            }

            ArtifactStoreHelper.AssertOnlyExpectedProjectWasReturned(publishResponse.Projects, _project);
            Assert.AreEqual(1, publishResponse.Artifacts.Count, "There should only be 1 published artifact returned!");
            ArtifactStoreHelper.AssertNovaArtifactResponsePropertiesMatchWithArtifact(publishResponse.Artifacts.First(), artifactWithMultipleVersions, expectedVersion);

            var artifactHistoryAfter = Helper.ArtifactStore.GetArtifactHistory(artifactWithMultipleVersions.Id, _user);

            Assert.AreEqual(expectedVersion, artifactHistoryAfter[0].VersionId, "Version ID after publish should be {0}!", expectedVersion);
        }

        [TestCase(BaseArtifactType.Process, 3)]
        [TestRail(165956)]
        [Description("Create & save multiple artifacts.  Publish all the artifacts.  Verify publish is successful and that the version of the artifacts is now 1.")]
        public void PublishArtifact_MultipleSavedArtifacts_ArtifactsHaveVersion1(BaseArtifactType artifactType, int numberOfArtifacts)
        {
            // Setup:
            var artifacts = Helper.CreateAndSaveMultipleArtifacts(_project, _user, artifactType, numberOfArtifacts);
            var artifactHistoryBefore = Helper.ArtifactStore.GetArtifactHistory(artifacts[0].Id, _user);
            Assert.AreEqual(int.MaxValue, artifactHistoryBefore[0].VersionId, "Version ID before publish should be {0}!", int.MaxValue);

            // Execute:
            INovaArtifactsAndProjectsResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifacts(artifacts, _user),
                "'POST {0}' should return 200 OK if a valid list of artifact IDs is sent!", PUBLISH_PATH);

            // Verify:
            ArtifactStoreHelper.AssertOnlyExpectedProjectWasReturned(publishResponse.Projects, _project);
            Assert.AreEqual(numberOfArtifacts, publishResponse.Artifacts.Count, "There should only be {0} published artifact returned!", numberOfArtifacts);

            AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(
                publishResponse, artifacts, expectedVersion: 1);
        }

        [TestCase(2, BaseArtifactType.Actor, BaseArtifactType.Document, BaseArtifactType.Glossary)]
        [TestCase(3, BaseArtifactType.Process, BaseArtifactType.TextualRequirement, BaseArtifactType.UseCase)]
        [TestRail(165969)]
        [Description("Create & publish a multiple artifacts several times and save to create drafts.  Publish the artifacts." +
            "Verify publish is successful and that the artifacts have the expected versions.")]
        public void PublishArtifact_MultiplePublishedArtifactsWithMultipleVersionsWithDraft_ArtifactHasExpectedVersion(int numberOfVersions, params BaseArtifactType[] artifactTypes)
        {
            ThrowIf.ArgumentNull(artifactTypes, nameof(artifactTypes));

            // Setup:
            var artifactsWithMultipleVersions = new List<IArtifactBase>();

            foreach (var artifactType in artifactTypes)
            {
                IArtifact artifactWithMultipleVersions = Helper.CreateAndPublishArtifact(_project, _user, artifactType, numberOfVersions: numberOfVersions);
                artifactsWithMultipleVersions.Add(artifactWithMultipleVersions);

                var artifactHistoryBefore = Helper.ArtifactStore.GetArtifactHistory(artifactWithMultipleVersions.Id, _user);
                Assert.AreEqual(numberOfVersions, artifactHistoryBefore[0].VersionId, "Version ID before Nova publish should be {0}!", numberOfVersions);

                artifactWithMultipleVersions.Save();    // Now save to make a draft.
            }

            // Execute:
            INovaArtifactsAndProjectsResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifacts(artifactsWithMultipleVersions, _user),
                "'POST {0}' should return 200 OK if a valid list of artifact IDs is sent!", PUBLISH_PATH);

            // Verify:
            ArtifactStoreHelper.AssertOnlyExpectedProjectWasReturned(publishResponse.Projects, _project);
            Assert.AreEqual(artifactsWithMultipleVersions.Count, publishResponse.Artifacts.Count,
                "There should only be {0} published artifact returned!", artifactsWithMultipleVersions.Count);

            int expectedVersion;

            checked  // This checked section is needed to fix warning:  CA2233: Operations should not overflow
            {
                expectedVersion = numberOfVersions + 1;
            }

            AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(
                publishResponse, artifactsWithMultipleVersions, expectedVersion);
        }

        [TestCase(BaseArtifactType.Actor, 2, BaseArtifactType.Process, 3, BaseArtifactType.UseCase, 2)]
        [TestRail(165970)]
        [Description("Create multiple artifacts (some saved, some published & some published with drafts).  Publish all the artifacts with unpublished changes." +
            "Verify publish is successful and that the version of the artifacts have the expected versions.")]
        public void PublishArtifact_MultipleSavedAndPublishedArtifactsSomeWithDrafts_PublishArtifactsWithUnpublishedChanges_ArtifactsHaveExpectedVersions(
            BaseArtifactType savedArtifactType, int numberOfSavedArtifacts,
            BaseArtifactType publishedWithDraftArtifactType, int numberOfPublishedWithDraftArtifacts,
            BaseArtifactType publishedArtifactType, int numberOfPublishedArtifacts)
        {
            // Setup:
            var savedArtifacts = Helper.CreateAndSaveMultipleArtifacts(_project, _user, savedArtifactType, numberOfSavedArtifacts);
            var publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _user, publishedArtifactType, numberOfPublishedArtifacts);
            var publishedWithDraftArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, _user, publishedWithDraftArtifactType, numberOfPublishedWithDraftArtifacts);

            Artifact.LockArtifacts(publishedWithDraftArtifacts, publishedWithDraftArtifacts.First().Address, _user);

            foreach (var artifact in publishedWithDraftArtifacts)
            {
                Artifact.UpdateArtifact(artifact, _user);
            }

            var artifactsToPublish = new List<IArtifactBase>();
            artifactsToPublish.AddRange(savedArtifacts);
            artifactsToPublish.AddRange(publishedWithDraftArtifacts);

            // Execute:
            INovaArtifactsAndProjectsResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifacts(artifactsToPublish, _user),
                "'POST {0}' should return 200 OK if a valid list of artifact IDs is sent!", PUBLISH_PATH);

            // Verify:
            ArtifactStoreHelper.AssertOnlyExpectedProjectWasReturned(publishResponse.Projects, _project);
            Assert.AreEqual(artifactsToPublish.Count, publishResponse.Artifacts.Count,
                "There should only be {0} published artifact returned!", artifactsToPublish.Count);

            AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(publishResponse, savedArtifacts, expectedVersion: 1);
            AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(publishResponse, publishedWithDraftArtifacts, expectedVersion: 2);

            // Verify the published artifacts without drafts didn't get published again.
            AssertArtifactsVersionEquals(publishedArtifacts, expectedVersion: 1);
        }

        [TestCase(BaseArtifactType.Process, 3, 2, null)]
        [TestCase(BaseArtifactType.UseCase, 2, 3, false)]
        [TestRail(165977)]
        [Description("Create & save multiple artifacts.  Publish some of the artifacts (pass all=false or don't pass the all parameter)." +
            "Verify publish is successful and that only the artifacts we wanted to publish got published.")]
        public void PublishArtifactWithAllNullOrFalse_MultipleSavedArtifacts_OnlyPublishSome_ArtifactsHaveExpectedVersion(
            BaseArtifactType artifactType, int numberOfArtifactsToPublish, int numberOfArtifactsToNotPublish, bool? all)
        {
            // Setup:
            var artifactsToPublish = Helper.CreateAndSaveMultipleArtifacts(_project, _user, artifactType, numberOfArtifactsToPublish);
            var artifactsNotToPublish = Helper.CreateAndSaveMultipleArtifacts(_project, _user, artifactType, numberOfArtifactsToNotPublish);

            // Execute:
            INovaArtifactsAndProjectsResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifacts(artifactsToPublish, _user, all),
                "'POST {0}{1}' should return 200 OK if a valid list of artifact IDs is sent!",
                PUBLISH_PATH, (all == null) ? string.Empty : I18NHelper.FormatInvariant("?all={0}", all.Value.ToString()));

            // Verify:
            ArtifactStoreHelper.AssertOnlyExpectedProjectWasReturned(publishResponse.Projects, _project);
            Assert.AreEqual(numberOfArtifactsToPublish, publishResponse.Artifacts.Count,
                "There should only be {0} published artifact returned!", numberOfArtifactsToPublish);

            AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(
                publishResponse, artifactsToPublish, expectedVersion: 1);

            AssertArtifactsWereNotPublished(artifactsNotToPublish);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
        [TestRail(165978)]
        [Description("Create a single published artifact.  Delete the artifact, then publish it.  Verify the artifact is deleted by trying to get it with another user.")]
        public void PublishArtifact_SingleDeletedArtifact_ArtifactIsDeleted(BaseArtifactType artifactType)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, author, artifactType);
            artifact.Delete(author);

            // Execute:
            INovaArtifactsAndProjectsResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifact(artifact, author),
                "'POST {0}' should return 200 OK if a valid artifact ID is sent!", PUBLISH_PATH);

            // Verify:
            ArtifactStoreHelper.AssertOnlyExpectedProjectWasReturned(publishResponse.Projects, _project);
            Assert.AreEqual(1, publishResponse.Artifacts.Count, "There should only be 1 published artifact returned!");

            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);

            Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetArtifactDetails(viewer, artifact.Id),
                "After publishing a deleted artifact, other users should not be able to get the deleted artifact!");
        }

        [TestCase(BaseArtifactType.UseCase, 2, 3)]
        [TestRail(165979)]
        [Description("Create & save multiple artifacts.  Publish with all=true but only pass some of the artifacts." +
            "Verify publish is successful and that all the artifacts we created were published.")]
        public void PublishArtifactWithAllTrue_MultipleSavedArtifacts_OnlyPublishSome_ArtifactsHaveExpectedVersion(
            BaseArtifactType artifactType, int numberOfArtifactsToPublish, int numberOfArtifactsToNotPublish)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var artifactsPassedToPublish = Helper.CreateAndSaveMultipleArtifacts(_project, author, artifactType, numberOfArtifactsToPublish);
            var artifactsNotPassedToPublish = Helper.CreateAndSaveMultipleArtifacts(_project, author, artifactType, numberOfArtifactsToNotPublish);

            // Execute:
            INovaArtifactsAndProjectsResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifacts(artifactsPassedToPublish, author, all: true),
                "'POST {0}?all=true' should return 200 OK if a valid list of artifact IDs is sent!", PUBLISH_PATH);

            // Verify:
            var allArtifacts = new List<IArtifactBase>();
            allArtifacts.AddRange(artifactsPassedToPublish);
            allArtifacts.AddRange(artifactsNotPassedToPublish);

            ArtifactStoreHelper.AssertOnlyExpectedProjectWasReturned(publishResponse.Projects, _project);
            Assert.AreEqual(allArtifacts.Count, publishResponse.Artifacts.Count,
                "There should only be {0} published artifact returned!", allArtifacts.Count);

            AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(
                publishResponse, allArtifacts, expectedVersion: 1);
        }

        [TestCase(BaseArtifactType.Process, 3)]
        [TestRail(165980)]
        [Description("Create & save multiple artifacts.  Publish with all=true but don't pass any of the artifacts." +
            "Verify publish is successful and that all the artifacts we created were published.")]
        public void PublishArtifactWithAllTrue_MultipleSavedArtifacts_SendEmptyListToPublish_ArtifactsHaveExpectedVersion(
            BaseArtifactType artifactType, int numberOfArtifacts)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var allArtifacts = Helper.CreateAndSaveMultipleArtifacts(_project, author, artifactType, numberOfArtifacts);

            // Execute:
            INovaArtifactsAndProjectsResponse publishResponse = null;

            try
            {
                Assert.DoesNotThrow(
                    () => publishResponse = Helper.ArtifactStore.PublishArtifacts(new List<IArtifactBase>(), author, all: true),
                    "'POST {0}?all=true' should return 200 OK if an empty list of artifact IDs is sent!", PUBLISH_PATH);

                // Verify:
                ArtifactStoreHelper.AssertOnlyExpectedProjectWasReturned(publishResponse.Projects, _project);
                Assert.AreEqual(allArtifacts.Count, publishResponse.Artifacts.Count,
                    "There should only be {0} published artifact returned!", allArtifacts.Count);

                AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(
                    publishResponse, allArtifacts, expectedVersion: 1);
            }
            finally
            {
                // This is needed so the Dispose() in the TearDown doesn't fail.
                if (publishResponse != null)
                {
                    allArtifacts.First().NotifyArtifactPublish(publishResponse.Artifacts);
                }
            }
        }

        [TestCase(BaseArtifactType.Process, 3, BaseArtifactType.UseCase, 2)]
        [TestRail(166018)]
        [Description("Create multiple artifacts (some saved and others published).  Publish with all=true but don't pass any of the artifacts." +
            "Verify publish is successful and that only the unpublished artifacts we created were published.")]
        public void PublishArtifactWithAllTrue_MultipleSavedArtifactsAndPublishedArtifacts_SendEmptyListToPublish_ArtifactsHaveExpectedVersion(
            BaseArtifactType savedArtifactType, int numberOfSavedArtifacts, BaseArtifactType publishedArtifactType, int numberOfPublishedArtifacts)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
                
            var savedArtifacts = Helper.CreateAndSaveMultipleArtifacts(_project, author, savedArtifactType, numberOfSavedArtifacts);
            var publishedArtifacts = Helper.CreateAndPublishMultipleArtifacts(_project, author, publishedArtifactType, numberOfPublishedArtifacts);

            // Execute:
            INovaArtifactsAndProjectsResponse publishResponse = null;

            try
            {
                Assert.DoesNotThrow(
                    () => publishResponse = Helper.ArtifactStore.PublishArtifacts(new List<IArtifactBase>(), author, all: true),
                    "'POST {0}?all=true' should return 200 OK if an empty list of artifact IDs is sent!", PUBLISH_PATH);

                // Verify:
                ArtifactStoreHelper.AssertOnlyExpectedProjectWasReturned(publishResponse.Projects, _project);
                Assert.AreEqual(savedArtifacts.Count, publishResponse.Artifacts.Count,
                    "There should only be {0} published artifact returned!", savedArtifacts.Count);

                AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(
                    publishResponse, savedArtifacts, expectedVersion: 1);

                AssertArtifactsVersionEquals(publishedArtifacts, expectedVersion: 1);
            }
            finally
            {
                // This is needed so the Dispose() in the TearDown doesn't fail.
                if (publishResponse != null)
                {
                    savedArtifacts.First().NotifyArtifactPublish(publishResponse.Artifacts);
                }
            }
        }

        [TestCase(BaseArtifactType.Process, 3)]
        [TestRail(166019)]
        [Description("Create & save artifacts in multiple projects.  Publish all the artifacts.  Verify publish is successful and that the version of the artifacts is now 1.")]
        public void PublishArtifact_ArtifactsSavedInMultipleProjects_ArtifactsHaveVersion1(BaseArtifactType artifactType, int numberOfArtifacts)
        {
            // Setup:
            var projects = ProjectFactory.GetAllProjects(_user, shouldRetrievePropertyTypes: true);
            Assert.GreaterOrEqual(projects.Count, 2, "This test requires at least 2 projects to exist!");

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projects);

            IProject firstProject = projects[0];
            IProject secondProject = projects[1];

            var artifactsInFirstProject = Helper.CreateAndSaveMultipleArtifacts(firstProject, author, artifactType, numberOfArtifacts);
            var artifactsInSecondProject = Helper.CreateAndSaveMultipleArtifacts(secondProject, author, artifactType, numberOfArtifacts);

            var allArtifacts = new List<IArtifactBase>();
            allArtifacts.AddRange(artifactsInFirstProject);
            allArtifacts.AddRange(artifactsInSecondProject);

            // Execute:
            INovaArtifactsAndProjectsResponse publishResponse = null;

            Assert.DoesNotThrow(() => publishResponse = Helper.ArtifactStore.PublishArtifacts(allArtifacts, author),
                "'POST {0}' should return 200 OK if a valid list of artifact IDs is sent!", PUBLISH_PATH);

            // Verify:
            var expectedProjects = new List<IProject>();
            expectedProjects.Add(firstProject);
            expectedProjects.Add(secondProject);

            ArtifactStoreHelper.AssertAllExpectedProjectsWereReturned(publishResponse.Projects, expectedProjects);
            Assert.AreEqual(allArtifacts.Count, publishResponse.Artifacts.Count,
                "There should be {0} published artifacts returned!", allArtifacts.Count);

            AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(
                publishResponse, artifactsInFirstProject, expectedVersion: 1);
        }

        [TestCase(BaseArtifactType.Process, 3)]
        [TestRail(166020)]
        [Description("Create & save artifacts in multiple projects.  Publish all the artifacts.  Verify publish is successful and that the version of the artifacts is now 1.")]
        public void PublishArtifactWithAllTrue_ArtifactsSavedInMultipleProjects_SendEmptyListToPublish_ArtifactsHaveVersion1(BaseArtifactType artifactType, int numberOfArtifacts)
        {
            // Setup:
            var projects = ProjectFactory.GetAllProjects(_user, shouldRetrievePropertyTypes: true);
            Assert.GreaterOrEqual(projects.Count, 2, "This test requires at least 2 projects to exist!");

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, projects);

            IProject firstProject = projects[0];
            IProject secondProject = projects[1];

            var artifactsInFirstProject = Helper.CreateAndSaveMultipleArtifacts(firstProject, author, artifactType, numberOfArtifacts);
            var artifactsInSecondProject = Helper.CreateAndSaveMultipleArtifacts(secondProject, author, artifactType, numberOfArtifacts);

            var allArtifacts = new List<IArtifactBase>();
            allArtifacts.AddRange(artifactsInFirstProject);
            allArtifacts.AddRange(artifactsInSecondProject);

            // Execute:
            INovaArtifactsAndProjectsResponse publishResponse = null;

            try
            {
                Assert.DoesNotThrow(
                    () => publishResponse = Helper.ArtifactStore.PublishArtifacts(new List<IArtifactBase>(), author, all: true),
                    "'POST {0}?all=true' should return 200 OK if an empty list of artifact IDs is sent!", PUBLISH_PATH);

                // Verify:
                var expectedProjects = new List<IProject>();
                expectedProjects.Add(firstProject);
                expectedProjects.Add(secondProject);

                ArtifactStoreHelper.AssertAllExpectedProjectsWereReturned(publishResponse.Projects, expectedProjects);
                Assert.AreEqual(allArtifacts.Count, publishResponse.Artifacts.Count,
                    "There should be {0} published artifacts returned!", allArtifacts.Count);

                AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(
                    publishResponse, artifactsInFirstProject, expectedVersion: 1);
            }
            finally
            {
                // This is needed so the Dispose() in the TearDown doesn't fail.
                if (publishResponse != null)
                {
                    allArtifacts.First().NotifyArtifactPublish(publishResponse.Artifacts);
                }
            }
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(191079)]
        [Description("Create, publish & save artifacts in a couple of projects.  Publish all the artifacts.  User has permissions only for one artifact in 2nd project.  Verify publish is successful.")]
        public void PublishArtifactWithAllTrue_ArtifactsSavedInCoupleOfProjects_UserHasPermissionsOnlyToOneArtifactInSecondProjects(BaseArtifactType artifactType)
        {
            // Setup:
            var projects = ProjectFactory.GetProjects(_user, numberOfProjects: 2);

            // User has author rights for first project but none permissions for the second project
            var user = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, projects[0]);
            Helper.AssignProjectRolePermissionsToUser(user, TestHelper.ProjectRole.None, projects[1]);

            var artifactInProject1 = Helper.CreateAndPublishArtifact(projects[0], user, artifactType);
            artifactInProject1.Save(user);

            // Create & publish artifact with user that has permissions to the project
            var artifactInProject2 = Helper.CreateAndPublishArtifact(projects[1], _user, artifactType);
            // Allow editing for previously created & published artifact with user that does not have permissions for that project
            Helper.AssignProjectRolePermissionsToUser(user, TestHelper.ProjectRole.Author, projects[1], artifactInProject2);

            artifactInProject2.Save(user);

            var allArtifacts = new List<IArtifactBase>();
            allArtifacts.Add(artifactInProject1);
            allArtifacts.Add(artifactInProject2);

            INovaArtifactsAndProjectsResponse publishResponse = null;

            try
            {
                // Execute:
                Assert.DoesNotThrow(
                    () => publishResponse = Helper.ArtifactStore.PublishArtifacts(new List<IArtifactBase>(), user, all: true),
                    "'POST {0}?all=true' should return 200 OK if an empty list of artifact IDs is sent!", PUBLISH_PATH);

                // Verify:
                var expectedProjects = new List<IProject>();
                expectedProjects.Add(projects[0]);
                expectedProjects.Add(projects[1]);

                ArtifactStoreHelper.AssertAllExpectedProjectsWereReturned(publishResponse.Projects, expectedProjects);
                Assert.AreEqual(allArtifacts.Count, publishResponse.Artifacts.Count,
                    "There should be {0} published artifacts returned!", allArtifacts.Count);

                AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(
                    publishResponse, allArtifacts, expectedVersion: 2);
            }
            finally
            {
                // This is needed so the Dispose() in the TearDown doesn't fail.
                if (publishResponse != null)
                {
                    allArtifacts.First().NotifyArtifactPublish(publishResponse.Artifacts);
                }
            }
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(191154)]
        [Description("Create collection artifact or collection folder.  Publish it.  Verify the published collection artifact or collection folder is returned with proper content.")]
        public void PublishArtifact_CollectionOrCollectionFolder_ReturnsPublishedArtifact(ItemTypePredefined artifactType)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            INovaArtifact defaultCollectionFolder = _project.GetDefaultCollectionFolder(Helper.ArtifactStore.Address, author);

            var novaArtifact = ArtifactStore.CreateArtifact(Helper.ArtifactStore.Address, author, artifactType, RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10),
                _project, defaultCollectionFolder.Id);

            IArtifact artifact = Helper.WrapNovaArtifact(novaArtifact, _project, author, BaseArtifactType.PrimitiveFolder);

            INovaArtifactsAndProjectsResponse publishResponse = null;

            // Execute:
            Assert.DoesNotThrow(
                    () => publishResponse = Helper.ArtifactStore.PublishArtifact(artifact, author),
                    "'POST {0} should return 200 OK if an empty list of artifact IDs is sent!", PUBLISH_PATH);

            // Verify:
            var expectedProjects = new List<IProject>();
            expectedProjects.Add(_project);

            ArtifactStoreHelper.AssertAllExpectedProjectsWereReturned(publishResponse.Projects, expectedProjects);
            INovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(publishResponse.Artifacts[0], artifactDetails);
        }

        [TestCase()]
        [TestRail(191155)]
        [Description("Create collection artifact and collection folder.  Publish them with all=true.  " + 
            "Verify the published collection artifact and collection folder have been returned with proper content.")]
        public void PublishAllArtifacts_CollectionOrCollectionFolder_ReturnsPublishedArtifact()
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            INovaArtifact defaultCollectionFolder = _project.GetDefaultCollectionFolder(Helper.ArtifactStore.Address, author);

            var collectionFolder = ArtifactStore.CreateArtifact(Helper.ArtifactStore.Address, author, ItemTypePredefined.CollectionFolder,
                RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10), _project, defaultCollectionFolder.Id);
            var collectionArtifact = ArtifactStore.CreateArtifact(Helper.ArtifactStore.Address, author, ItemTypePredefined.ArtifactCollection,
                RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10), _project, collectionFolder.Id);

            INovaArtifactsAndProjectsResponse publishResponse = null;

            // Execute:
            Assert.DoesNotThrow(
                    () => publishResponse = Helper.ArtifactStore.PublishArtifacts(new List<IArtifactBase>(), author, all: true),
                    "'POST {0}?all=true' should return 200 OK if an empty list of artifact IDs is sent!", PUBLISH_PATH);

            // Verify:
            var expectedProjects = new List<IProject>();
            expectedProjects.Add(_project);

            ArtifactStoreHelper.AssertAllExpectedProjectsWereReturned(publishResponse.Projects, expectedProjects);
            INovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, collectionFolder.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(publishResponse.Artifacts[0], artifactDetails);
            artifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, collectionArtifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(publishResponse.Artifacts[1], artifactDetails);
        }

        [TestCase(BaselineAndCollectionTypePredefined.ArtifactCollection)]
        [TestCase(BaselineAndCollectionTypePredefined.CollectionFolder)]
        [TestRail(191156)]
        [Description("Create & Save collection artifact or collection folder.  Publish it.  Verify the published collection artifact or collection folder is returned with proper content.")]
        public void PublishArtifact_SaveCollectionOrCollectionFolder_ReturnsPublishedArtifact(ItemTypePredefined artifactType)
        {
            // Setup:
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            INovaArtifact defaultCollectionFolder = _project.GetDefaultCollectionFolder(Helper.ArtifactStore.Address, author);

            var novaArtifact = ArtifactStore.CreateArtifact(Helper.ArtifactStore.Address, author, artifactType, RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10),
                _project, defaultCollectionFolder.Id);

            IArtifact artifact = Helper.WrapNovaArtifact(novaArtifact, _project, author, BaseArtifactType.PrimitiveFolder);

            novaArtifact.GetType().GetProperty("Name").SetValue(novaArtifact, "Changed", null);

            Artifact.UpdateArtifact(artifact, author, (NovaArtifactDetails)novaArtifact);

            INovaArtifactsAndProjectsResponse publishResponse = null;

            // Execute:
            Assert.DoesNotThrow(
                    () => publishResponse = Helper.ArtifactStore.PublishArtifact(artifact, author),
                    "'POST {0} should return 200 OK if an empty list of artifact IDs is sent!", PUBLISH_PATH);

            // Verify:
            var expectedProjects = new List<IProject>();
            expectedProjects.Add(_project);

            ArtifactStoreHelper.AssertAllExpectedProjectsWereReturned(publishResponse.Projects, expectedProjects);
            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(author, artifact.Id);
            ArtifactStoreHelper.AssertArtifactsEqual(publishResponse.Artifacts[0], artifactDetails);
        }

        #endregion 200 OK Tests

        #region 400 Bad Request tests
        [TestCase]
        [TestRail(165971)]
        [Description("Send empty list of artifacts, checks returned result is 400 Bad Request.")]
        public void PublishArtifact_EmptyArtifactList_BadRequest()
        {
            // Setup:
            List<IArtifactBase> artifacts = new List<IArtifactBase>();

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.ArtifactStore.PublishArtifacts(artifacts, _user),
            "'POST {0}' should return 400 Bad Request if body of the request does not have any artifact ids!", PUBLISH_PATH);

            // Verify:
            string expectedExceptionMessage = "The list of artifact Ids is empty.";
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.IncorrectInputParameters, expectedExceptionMessage);
        }

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(165972)]
        [Description("Create, save, publish Actor artifact, checks returned result is 400 Bad Request for artifact that is already published")]
        public void PublishArtifact_SinglePublishedArtifact_BadRequest(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);

            // Execute:
            var ex = Assert.Throws<Http400BadRequestException>(() => Helper.ArtifactStore.PublishArtifact(artifact, _user),
                "'POST {0}' should return 400 Bad Request if an artifact already published!", PUBLISH_PATH);

            // Verify:
            string expectedExceptionMessage = I18NHelper.FormatInvariant("Artifact with ID {0} has nothing to publish.", artifact.Id);
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ValidationFailed, expectedExceptionMessage);
        }

        #endregion 400 Bad Request tests

        #region 401 Unauthorized tests

        [TestCase(BaseArtifactType.Actor)]
        [TestRail(165975)]
        [Description("Create & save a single artifact.  Publish the artifact with wrong token.  Verify publish returns code 401 Unauthorized.")]
        public void PublishArtifact_InvalidToken_Unauthorized(BaseArtifactType artifactType)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_project, _user, artifactType);

            IUser userWithBadToken = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute:
            var ex = Assert.Throws<Http401UnauthorizedException>(() => Helper.ArtifactStore.PublishArtifact(artifact, userWithBadToken),
                "'POST {0}' should return 401 Unauthorized if a token is invalid!", PUBLISH_PATH);
            
            // Verify:
            string jsonBody = JsonConvert.DeserializeObject<string>(ex.RestResponse.Content);
            const string expectedMessage = "Unauthorized call";
            Assert.AreEqual(expectedMessage, jsonBody, "The JSON body should contain '{0}' when an unauthorized token is passed!", expectedMessage);
        }

        #endregion 401 Unauthorized tests

        #region 404 Not Found tests

        [TestCase(BaseArtifactType.Process)]
        [TestRail(165973)]
        [Description("Create, save, publish, delete Process artifact by another user, checks returned result is 404 Not Found.")]
        public void PublishArtifact_PublishedArtifactDeletedByAnotherUser_NotFound(BaseArtifactType artifactType)
        {
            // Setup:
            IUser anotherUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            var artifact = Helper.CreateAndPublishArtifact(_project, anotherUser, artifactType);

            artifact.Delete(anotherUser);
            artifact.Publish(anotherUser);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.PublishArtifact(artifact, _user),
                "'POST {0}' should return 404 Not Found if the Artifact ID doesn't exist!", PUBLISH_PATH);

            // Verify:
            string expectedExceptionMessage = I18NHelper.FormatInvariant("Artifact with ID {0} is deleted.", artifact.Id);
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound, expectedExceptionMessage);
        }

        [TestCase(int.MaxValue)]
        [TestRail(165976)]
        [Description("Try to publish an artifact with a non-existent Artifact ID.  Verify 404 Not Found is returned.")]
        public void PublishArtifact_NonExistentArtifactId_NotFound(int nonExistentArtifactId)
        {
            // Setup:
            IArtifact artifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Process);

            // Replace ProjectId with a fake ID that shouldn't exist.
            artifact.Id = nonExistentArtifactId;

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.PublishArtifact(artifact, _user),
                "'POST {0}' should return 404 Not Found if the Artifact ID doesn't exist!", PUBLISH_PATH);

            // Verify:
            string expectedExceptionMessage = I18NHelper.FormatInvariant("Item with ID {0} is not found.", artifact.Id);
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ItemNotFound, expectedExceptionMessage);
        }

        #endregion 404 Not Found tests

        #region 409 Conflict tests

        [TestCase(BaseArtifactType.Process, 1)]
        [TestCase(BaseArtifactType.Process, 2)]
        [TestRail(165974)]
        [Description("Create, save, parent artifact with two children, publish child artifact, checks returned result is 409 Conflict.")]
        public void PublishArtifact_ParentAndChildArtifacts_OnlyPublishChild_Conflict(BaseArtifactType artifactType, int index)
        {
            // Setup:
            List<IArtifact> artifactList = CreateParentAndTwoChildrenArtifactsAndGetAllArtifacts(artifactType);
            IArtifact childArtifact = artifactList[index];

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.PublishArtifact(childArtifact, _user),
                "'POST {0}' should return 409 Conflict if the Artifact has parent artifact which is not published!", PUBLISH_PATH);

            // Verify:
            // TODO: Also verify the 'errorContent' property that contains the dependent artifacts.
            string expectedExceptionMessage = "Specified artifacts have dependent artifacts to publish.";
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotPublishOverDependencies, expectedExceptionMessage);
        }

        #region Custom data tests

        [Category(Categories.CustomData)]
        [TestCase("value\":10.0", "value\":999.0")] //Insert value into Numeric field which is out of range
        [TestCase("value\":\"20", "value\":\"21")] //Insert value into Date field which is out of range
        [TestRail(166007)]
        [Description("Try to publish an artifact with a value of property that out of its permitted range. Verify 409 Conflict is returned.")]
        public void PublishArtifact_PropertyOutOfRange_Conflict(string toChange, string changeTo)
        {
            // Setup:
            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            IArtifact artifact = Helper.CreateAndPublishArtifact(projectCustomData, _user, BaseArtifactType.Actor);
            artifact.Lock();

            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);

            //This is needed to suppress 501 error
            artifactDetails.ItemTypeId = null;

            string requestBody = JsonConvert.SerializeObject(artifactDetails);

            requestBody = requestBody.Replace(toChange, changeTo);

            Assert.DoesNotThrow(() => ArtifactStoreHelper.UpdateInvalidArtifact(Helper.BlueprintServer.Address, requestBody, artifact.Id, _user),
                "'PATCH {0}' should return 200 OK if properties are out of range!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.PublishArtifact(artifact, _user),
                "'POST {0}' should return 409 Conflict if an artifact already published!", PUBLISH_PATH);

            // Verify:
            string expectedExceptionMessage = I18NHelper.FormatInvariant("Artifact with ID {0} has validation errors.", artifact.Id);
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotPublishOverValidationErrors, expectedExceptionMessage);
        }

        [TestCase("value\":10.0", "value\":999.0", BaseArtifactType.Actor, 0)] //Insert value into Numeric field which is out of range in grandparent artifact
        [TestCase("value\":10.0", "value\":999.0", BaseArtifactType.Actor, 1)] //Insert value into Numeric field which is out of range in parent artifact
        [TestCase("value\":10.0", "value\":999.0", BaseArtifactType.Actor, 2)] //Insert value into Numeric field which is out of range in child artifact
        [TestCase("value\":\"20", "value\":\"21", BaseArtifactType.Actor, 0)] //Insert value into Date field which is out of range in grandparent artifact
        [TestCase("value\":\"20", "value\":\"21", BaseArtifactType.Actor, 1)] //Insert value into Date field which is out of range in parent artifact
        [TestCase("value\":\"20", "value\":\"21", BaseArtifactType.Actor, 2)] //Insert value into Date field which is out of range in child artifact
        [Category(Categories.CustomData)]
        [TestRail(166129)]
        [Description("Try to publish an artifact with a value of property that out of its permitted range. Verify 409 Conflict is returned.")]
        public void PublishAllArtifacts_PropertyOutOfRange_Conflict(string toChange, string changeTo, BaseArtifactType artifactType, int index)
        {
            // Setup:
            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);

            var artifactTypes = new BaseArtifactType[] { artifactType, artifactType, artifactType };
            List<IArtifact> artifactList = Helper.CreatePublishedArtifactChain(projectCustomData, _user, artifactTypes);
            artifactList[index].Lock();

            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifactList[index].Id);

            //This is needed to suppress 501 error
            artifactDetails.ItemTypeId = null;

            string requestBody = JsonConvert.SerializeObject(artifactDetails);

            requestBody = requestBody.Replace(toChange, changeTo);

            Assert.DoesNotThrow(() => ArtifactStoreHelper.UpdateInvalidArtifact(Helper.BlueprintServer.Address, requestBody, artifactList[index].Id, _user),
                "'PATCH {0}' should return 200 OK if properties are out of range!",
                RestPaths.Svc.ArtifactStore.ARTIFACTS_id_);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => Helper.ArtifactStore.PublishArtifacts(artifactList.ConvertAll(o => (IArtifactBase)o), _user, all: true),
                "'POST {0}' should return 409 Conflict if an artifact already published!", PUBLISH_PATH);

            // Verify:
            string expectedExceptionMessage = I18NHelper.FormatInvariant("Artifact with ID {0} has validation errors.", artifactList[index].Id);
            ArtifactStoreHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotPublishOverValidationErrors, expectedExceptionMessage);
        }
    
        #endregion Custom data tests

        #endregion 409 Conflict tests

        #region Private functions

        /// <summary>
        /// Asserts that the version of all the artifacts in the list still have the expected version.
        /// </summary>
        /// <param name="artifacts">The list of artifacts whose version you want to check.</param>
        /// <param name="expectedVersion">The expected version of all the artifacts.</param>
        private void AssertArtifactsVersionEquals(List<IArtifactBase> artifacts, int expectedVersion)
        {
            foreach (var artifact in artifacts)
            {
                var artifactHistoryAfter = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user);
                Assert.AreEqual(expectedVersion, artifactHistoryAfter[0].VersionId,
                    "Version ID after publish should be {0}!", expectedVersion);
            }
        }

        /// <summary>
        /// Asserts that the specified list of artifacts were NOT published by getting the artifacts and comparing against
        /// the list that was passed in.
        /// </summary>
        /// <param name="artifacts">The list of artifacts to verify.</param>
        private void AssertArtifactsWereNotPublished(List<IArtifactBase> artifacts)
        {
            foreach (var artifact in artifacts)
            {
                var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, artifact.Id);
                ArtifactStoreHelper.AssertArtifactsEqual(artifactDetails, artifact);
            }
        }

        /// <summary>
        /// Asserts that the response from the publish call contains all the specified artifacts and that they now have the correct version.
        /// </summary>
        /// <param name="publishResponse">The response from the publish call.</param>
        /// <param name="artifactsToPublish">The OpenApi artifacts that we sent to the publish call.</param>
        /// <param name="expectedVersion">The version expected in the publishedArtifact.</param>
        private void AssertPublishedArtifactResponseContainsAllArtifactsInListAndHasExpectedVersion(
            INovaArtifactsAndProjectsResponse publishResponse,
            List<IArtifactBase> artifactsToPublish,
            int expectedVersion)
        {
            ArtifactStoreHelper.AssertArtifactsAndProjectsResponseContainsAllArtifactsInListAndHasExpectedVersion(
                publishResponse, artifactsToPublish, expectedVersion);

            foreach (var artifact in artifactsToPublish)
            {
                var artifactHistoryAfter = Helper.ArtifactStore.GetArtifactHistory(artifact.Id, _user);
                Assert.AreEqual(expectedVersion, artifactHistoryAfter[0].VersionId, "Version ID after publish should be {0}!", expectedVersion);
            }
        }

        /// <summary>
        /// Creates chain of tree artifacts. Grandparent, parent and child
        /// </summary>
        /// <param name="artifactType">The type of created artifacts</param>
        /// <returns>List of created artifacts.</returns>
        private List<IArtifact> CreateParentAndTwoChildrenArtifactsAndGetAllArtifacts(BaseArtifactType artifactType)
        {
            var artifactTypes = new BaseArtifactType[] { artifactType, artifactType, artifactType };
            var artifactChain = Helper.CreateSavedArtifactChain(_project, _user, artifactTypes);
            return artifactChain;
        }


        #endregion Private functions
    }
}
