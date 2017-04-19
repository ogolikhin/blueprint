﻿using System.Collections.Generic;
using System.Linq;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
using Utilities;
using Common;
using Model.ArtifactModel.Enums;
using Model.ModelHelpers;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class UnpublishedChangesTests : TestBase
    {
        private const string SVC_PATH = RestPaths.Svc.ArtifactStore.Artifacts.UNPUBLISHED;

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

        #region 200 OK tests

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182259)]
        [Description("Create & save an artifact.  GetUnpublishedChanges.  Verify the saved artifact is returned.")]
        public void GetUnpublishedChanges_SavedArtifact_ReturnsArtifactDetails(BaseArtifactType artifactType)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            var artifact = Helper.CreateAndSaveArtifact(_project, author, artifactType);
            INovaArtifactsAndProjectsResponse unpublishedChanges = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                unpublishedChanges = Helper.ArtifactStore.GetUnpublishedChanges(author);
            }, "'GET {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.AssertOnlyExpectedProjectWasReturned(unpublishedChanges.Projects, _project);
            Assert.AreEqual(1, unpublishedChanges.Artifacts.Count, "There should be 1 artifact in the list of unpublished changes!");
            ArtifactStoreHelper.AssertNovaArtifactResponsePropertiesMatchWithArtifactSkipVersion(unpublishedChanges.Artifacts.First(), artifact);
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182260)]
        [Description("Create & publish an artifact, then change & save it.  GetUnpublishedChanges.  Verify the draft artifact is returned.")]
        public void GetUnpublishedChanges_PublishedArtifactWithDraft_ReturnsArtifactDetails(BaseArtifactType artifactType)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            var artifact = Helper.CreateAndPublishArtifact(_project, author, artifactType);
            artifact.Save(author);
            INovaArtifactsAndProjectsResponse unpublishedChanges = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                unpublishedChanges = Helper.ArtifactStore.GetUnpublishedChanges(author);
            }, "'GET {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.AssertOnlyExpectedProjectWasReturned(unpublishedChanges.Projects, _project);
            Assert.AreEqual(1, unpublishedChanges.Artifacts.Count, "There should be 1 artifact in the list of unpublished changes!");
            ArtifactStoreHelper.AssertNovaArtifactResponsePropertiesMatchWithArtifact(unpublishedChanges.Artifacts.First(), artifact, expectedVersion: 1);
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182335)]
        [Description("Create & publish an artifact.  GetUnpublishedChanges.  Verify an empty list of changes is returned.")]
        public void GetUnpublishedChanges_PublishedArtifact_ReturnsEmptyList(BaseArtifactType artifactType)
        {
            // Setup:
            Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            INovaArtifactsAndProjectsResponse unpublishedChanges = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                unpublishedChanges = Helper.ArtifactStore.GetUnpublishedChanges(_user);
            }, "'GET {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            Assert.AreEqual(0, unpublishedChanges.Artifacts.Count, "There should be no artifacts in the list of unpublished changes!");
            Assert.AreEqual(0, unpublishedChanges.Projects.Count, "There should be no projects in the list of unpublished changes!");
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182337)]
        [Description("Create & publish an artifact, then delete it.  GetUnpublishedChanges.  Verify the delete artifact change is returned.")]
        public void GetUnpublishedChanges_PublishedArtifactThenDeleted_ReturnsArtifactDetails(BaseArtifactType artifactType)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);

            var artifact = Helper.CreateAndPublishArtifact(_project, author, artifactType);
            artifact.Delete(author);
            INovaArtifactsAndProjectsResponse unpublishedChanges = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                unpublishedChanges = Helper.ArtifactStore.GetUnpublishedChanges(author);
            }, "'GET {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.AssertOnlyExpectedProjectWasReturned(unpublishedChanges.Projects, _project);
            Assert.AreEqual(1, unpublishedChanges.Artifacts.Count, "There should be 1 artifact in the list of unpublished changes!");
            ArtifactStoreHelper.AssertNovaArtifactResponsePropertiesMatchWithArtifact(unpublishedChanges.Artifacts.First(), artifact, expectedVersion: 1);
        }

        [TestCase(BaseArtifactType.Process)]
        [TestRail(182345)]
        [Description("As user1 create & publish an artifact, then as user2 change & save it.  GetUnpublishedChanges as user1.  Verify an empty list is returned.")]
        public void GetUnpublishedChanges_PublishedArtifactModifiedByDifferentUser_ReturnsEmptyList(BaseArtifactType artifactType)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, artifactType);
            var user2 = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            artifact.Save(user2);
            INovaArtifactsAndProjectsResponse unpublishedChanges = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                unpublishedChanges = Helper.ArtifactStore.GetUnpublishedChanges(_user);
            }, "'GET {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            Assert.AreEqual(0, unpublishedChanges.Artifacts.Count, "There should be no artifacts in the list of unpublished changes!");
            Assert.AreEqual(0, unpublishedChanges.Projects.Count, "There should be no projects in the list of unpublished changes!");
        }

        [TestCase]
        [TestRail(182338)]
        [Description("Call GetUnpublishedChanges by a user without permission to any projects.  Verify an empty list is returned.")]
        public void GetUnpublishedChanges_UserWithNoPermissions_ReturnsEmptyList()
        {
            // Setup:
            var userWithNoPermissions = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken, instanceAdminRole: null);
            INovaArtifactsAndProjectsResponse unpublishedChanges = null;

            // Execute:
            Assert.DoesNotThrow(() => unpublishedChanges = Helper.ArtifactStore.GetUnpublishedChanges(userWithNoPermissions),
                "'GET {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            Assert.AreEqual(0, unpublishedChanges.Artifacts.Count, "There should be no artifacts in the list of unpublished changes!");
            Assert.AreEqual(0, unpublishedChanges.Projects.Count, "There should be no projects in the list of unpublished changes!");
        }

        [TestCase(ItemTypePredefined.Process, 2)]
        [TestRail(182336)]
        [Description("Create & save artifacts in different projects.  GetUnpublishedChanges.  Verify the saved artifacts and the specified projects are returned " +
                     "and that the artifacts are ordered by project name, then by artifact ID.")]
        public void GetUnpublishedChanges_ArtifactsSavedInDifferentProjects_ReturnsArtifactDetailsOrderedByProjectNameThenArtifactId(
            ItemTypePredefined artifactType, int numberOfProjects)
        {
            // Setup:
            var projects = ProjectFactory.GetProjects(_user, numberOfProjects);

            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, projects);

            var artifacts = new List<ArtifactWrapper>();

            // Create artifacts in different projects.
            foreach (var project in projects)
            {
                var artifact = Helper.CreateNovaArtifact(author, project, artifactType);
                artifacts.Add(artifact);
            }

            // Now create artifacts in projects in the opposite order.
            var projectsInReverse = new List<IProject>();
            projectsInReverse.AddRange(projects);
            projectsInReverse.Reverse();

            foreach (var project in projectsInReverse)
            {
                var artifact = Helper.CreateNovaArtifact(author, project, artifactType);
                artifacts.Add(artifact);
            }

            INovaArtifactsAndProjectsResponse unpublishedChanges = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                unpublishedChanges = Helper.ArtifactStore.GetUnpublishedChanges(author);
            }, "'GET {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.AssertAllExpectedProjectsWereReturned(unpublishedChanges.Projects, projects);
            Assert.AreEqual(artifacts.Count, unpublishedChanges.Artifacts.Count,
                "There should be {0} artifact in the list of unpublished changes!", artifacts.Count);
            ArtifactStoreHelper.AssertArtifactsAndProjectsResponseContainsAllArtifactsInList(unpublishedChanges, artifacts.ConvertAll(a => (INovaArtifactDetails)a));

            // Verify - Order by the project name and then by the artifact integer Id.
            AssertArtifactsAndProjectsResponseIsOrderedByProjectNameThenByArtifactId(unpublishedChanges);
        }

        [Explicit(IgnoreReasons.UnderDevelopmentDev)]  // We can't move artifacts from one project to another yet.  https://trello.com/c/V3AdEzXK
        [TestCase(ItemTypePredefined.Process, 2)]
        [TestRail(000)]     // TODO: Add to TestRail once this test is working.
        [Description("Create & publish an artifact, then move it to a different project.  GetUnpublishedChanges.  " +
                     "Verify the artifact and the specified projects are returned.")]
        public void GetUnpublishedChanges_ArtifactMovedToDifferentProject_ReturnsArtifactDetails(ItemTypePredefined artifactType, int numberOfProjects)
        {
            // Setup:
            var projects = ProjectFactory.GetProjects(_user, numberOfProjects);

            // Create artifacts in different projects.
            var artifact = Helper.CreateAndPublishNovaArtifact(_user, projects[0], artifactType);

            var artifacts = new List<ArtifactWrapper> { artifact };

            // Move the artifact to a different project.
            artifact.ProjectId = projects[1].Id;
            artifact.ArtifactState.Project = projects[1];

            artifact.Lock(_user);
            artifact.Update(_user, artifact);

            INovaArtifactsAndProjectsResponse unpublishedChanges = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                unpublishedChanges = Helper.ArtifactStore.GetUnpublishedChanges(_user);
            }, "'GET {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.AssertAllExpectedProjectsWereReturned(unpublishedChanges.Projects, projects);
            Assert.AreEqual(artifacts.Count, unpublishedChanges.Artifacts.Count,
                "There should be {0} artifact in the list of unpublished changes!", artifacts.Count);
            ArtifactStoreHelper.AssertArtifactsAndProjectsResponseContainsAllArtifactsInList(
                unpublishedChanges, artifacts.ConvertAll(a => (INovaArtifactDetails)a));

            // Verify - Order by the project name and then by the artifact integer Id.
            AssertArtifactsAndProjectsResponseIsOrderedByProjectNameThenByArtifactId(unpublishedChanges);
        }

        [TestCase(3)]
        [TestRail(191164)]
        [Description("Create & save multiple artifacts. Change user permissions to None to one of them. GetUnpublishedChanges.  Verify the draft artifacts are returned.")]
        public void GetUnpublishedChanges_PublishedMultipleArtifacts_UserLosesPermissionsToOneOfThem_ReturnsArtifactDetails(int numberOfArtifacts)
        {
            // Setup:
            var user = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            var artifacts = Helper.CreateAndSaveMultipleArtifacts(_project, user, BaseArtifactType.Process, numberOfArtifacts);

            Helper.AssignProjectRolePermissionsToUser(user, TestHelper.ProjectRole.None, _project, artifacts.Last());

            INovaArtifactsAndProjectsResponse unpublishedChanges = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                unpublishedChanges = Helper.ArtifactStore.GetUnpublishedChanges(user);
            }, "'GET {0}' should return 200 OK when called with a valid token!", SVC_PATH);

            // Verify:
            ArtifactStoreHelper.AssertOnlyExpectedProjectWasReturned(unpublishedChanges.Projects, _project);
            Assert.AreEqual(3, unpublishedChanges.Artifacts.Count, I18NHelper.FormatInvariant("There should be {0} artifacts in the list of unpublished changes!", numberOfArtifacts));
            ArtifactStoreHelper.AssertArtifactsAndProjectsResponseContainsAllArtifactsInListAndHasExpectedVersion(unpublishedChanges, artifacts, expectedVersion: 1);
        }

        #endregion 200 OK tests

        #region 401 Unauthorized tests

        [TestCase]
        [TestRail(182339)]
        [Description("Call GetUnpublishedChanges with an unauthorized token.  Verify it returns 401 Unauthorized.")]
        public void GetUnpublishedChanges_UnauthorizedUser_401Unauthorized()
        {
            // Setup:
            var unauthorizedUser = Helper.CreateUserWithInvalidToken(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Execute:
            Assert.Throws<Http401UnauthorizedException>(() => Helper.ArtifactStore.GetUnpublishedChanges(unauthorizedUser),
                "'GET {0}' should return 401 Unauthorized when called with a bad token!", SVC_PATH);
        }

        #endregion 401 Unauthorized tests

        #region Private functions

        private static void AssertArtifactsAndProjectsResponseIsOrderedByProjectNameThenByArtifactId(
            INovaArtifactsAndProjectsResponse unpublishedChanges)
        {
            var projects = unpublishedChanges.Projects;
            var sortedProjects = projects.OrderBy(p => p.Name).ToList();

            // XXX: Check with Alex F. if the Projects list should also be sorted or just the Artifacts list?
//            Assert.That(projectNames.SequenceEqual(sortedProjects), "The Projects are not sorted alphabetically by Name!");

            var sortedProjectIds = sortedProjects.Select(p => p.Id).ToList();
            int lastArtifactId = 0;
            int lastProjectIdIndex = 0;

            foreach (var artifact in unpublishedChanges.Artifacts)
            {
                // If we move from one group of artifacts by project to another, update project index and reset the last artifact ID.
                if (artifact.ProjectId != sortedProjectIds[lastProjectIdIndex])
                {
                    ++lastProjectIdIndex;
                    lastArtifactId = 0;
                }

                Assert.AreEqual(sortedProjectIds[lastProjectIdIndex], artifact.ProjectId,
                    "The Project ID of artifact ID {0} is not ordered correctly by Project Name!");
                Assert.Less(lastArtifactId, artifact.Id, "The Artifacts are not ordered correctly by Artifact ID!");

                lastArtifactId = artifact.Id;
            }
        }

        #endregion Private functions
    }
}
