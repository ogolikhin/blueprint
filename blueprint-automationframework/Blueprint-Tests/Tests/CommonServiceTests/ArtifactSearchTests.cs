using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.ModelHelpers;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommon;

namespace CommonServiceTests
{
    public class ArtifactSearchTests : TestBase
    {
        private IUser _adminUser;
        private IProject _project;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_adminUser);
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForNovaRestMethods))]
        [TestRail(102883)]
        [Description("Create artifact, save and publish it. Search created artifact by name within all projects. Search must return created artifact.")]
        public void GetSearchArtifactResultsAllProjects_ReturnedListContainsCreatedArtifact(ItemTypePredefined artifactType)
        {
            // Setup:
            //Create an artifact with ArtifactType and populate all required values without properties
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, artifactType);

            // Execute:
            IList<IArtifactBase> artifactsList = null;
            Assert.DoesNotThrow(() =>
            {
                artifactsList = Helper.SvcShared.SearchArtifactsByName(user: _adminUser, searchSubstring: artifact.Name);
            }, "{0}.{1}() shouldn't throw an exception when passed valid parameters!", nameof(Helper.SvcShared),
            nameof(Helper.SvcShared.SearchArtifactsByName));

            // Verify:
            Assert.IsTrue(artifactsList.Count > 0, "No artifacts were found after adding an artifact!");
        }

        [TestCase]
        [TestRail(102884)]
        [Description("Check that search artifact by name returns 10 artifacts only.")]
        public void GetSearchArtifactResults_ReturnedListHasExpectedLength()
        {
            // Setup: Create an artifact with ArtifactType and populate all required values without properties
            var artifactList = new List<ArtifactWrapper>();

            for (int i = 0; i < 12; i++)
            {
                var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Actor);
                artifactList.Add(artifact);
            }

            // Execute: Implementation of CreateArtifact uses Artifact_ prefix to name artifacts
            string searchString = "Artifact_";
            IList<IArtifactBase> searchResultList = null;

            Assert.DoesNotThrow(() =>
            {
                searchResultList = Helper.SvcShared.SearchArtifactsByName(user: _adminUser, searchSubstring: searchString);
            }, "{0}.{1}() shouldn't throw an exception when passed valid parameters!", nameof(Helper.SvcShared),
            nameof(Helper.SvcShared.SearchArtifactsByName));

            // Verify: 
            Assert.IsTrue(searchResultList.Count == 10, "Search results must have 10 artifacts, but they have '{0}'.", searchResultList.Count);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForNovaRestMethods))]
        [TestRail(123257)]
        [Description("Create artifact, save and publish it. Search created artifact by name within the project where artifact was created. Search must return created artifact.")]
        public void GetSearchArtifactResultsForOneProject_ReturnedListContainsCreatedArtifact(ItemTypePredefined artifactType)
        {
            // Setup: Create an artifact with ArtifactType and populate all required values without properties
            var artifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, artifactType);

            // Execute:
            IList<IArtifactBase> searchResultList = null;

            Assert.DoesNotThrow(() =>
            {
                searchResultList = Helper.SvcShared.SearchArtifactsByName(user: _adminUser, searchSubstring: artifact.Name,
                    project: _project);
            }, "{0}.{1}() shouldn't throw an exception when passed valid parameters!", nameof(Helper.SvcShared),
            nameof(Helper.SvcShared.SearchArtifactsByName));

            // Verify:
            Assert.IsTrue(searchResultList.Count > 0, "No artifacts were found after adding an artifact!");
        }
    }
}
