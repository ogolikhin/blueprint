using Common;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.ModelHelpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Model.OpenApiModel.Services;
using Model.SearchServiceModel.Impl;
using Utilities;
using Utilities.Factories;
using Model.ArtifactModel.Enums;

namespace Helper
{
    public static class SearchServiceTestHelper
    {
        private const int DEFAULT_TIMEOUT_FOR_SEARCH_INDEXER_UPDATE_IN_MS = 600000;

        /// <summary>
        /// Sets up artifact data for Full Text Search Service tests
        /// </summary>
        /// <param name="projects">The projects in which the artifacts will be created</param>
        /// <param name="user">The user creating the artifacts</param>
        /// <param name="testHelper">An instance of TestHelper</param>
        /// <param name="selectedBaseArtifactTypes">(optional) list of seletedBaseAritfactTypes will be used to setup search data</param>
        /// <returns>List of created artifacts</returns>
        /// <param name="timeoutInMilliseconds">(optional) Timeout in milliseconds after which search will terminate 
        /// if not successful </param>
        public static List<ArtifactWrapper> SetupFullTextSearchData(List<IProject> projects, IUser user, TestHelper testHelper,
            List<ItemTypePredefined> selectedBaseArtifactTypes = null,
            int timeoutInMilliseconds = DEFAULT_TIMEOUT_FOR_SEARCH_INDEXER_UPDATE_IN_MS)
        {
            ThrowIf.ArgumentNull(projects, nameof(projects));
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(testHelper, nameof(testHelper));

            Logger.WriteTrace("{0}.{1} called.", nameof(SearchServiceTestHelper), nameof(SetupFullTextSearchData));

            var artifacts = new List<ArtifactWrapper>();

            var projectsSubset = new List<IProject> { projects[0], projects[1] };

            // This keeps the artifact description constant for all created artifacts
            var randomArtifactDescription = "Description " + RandomGenerator.RandomAlphaNumericUpperAndLowerCaseAndSpecialCharactersWithSpaces();

            if (selectedBaseArtifactTypes == null)
            {
                var randomBaselineName = "Artifact_" + RandomGenerator.RandomAlphaNumericUpperAndLowerCaseAndSpecialCharactersWithSpaces();
                var randomCollectionName = "Artifact_" + RandomGenerator.RandomAlphaNumericUpperAndLowerCaseAndSpecialCharactersWithSpaces();
                foreach (var project in projectsSubset)
                    {
                        var artifactBaseline = testHelper.CreateBaseline(user, project, name: randomBaselineName);
                        artifactBaseline.SaveWithNewDescription(user, randomArtifactDescription);

                        var artifactCollection = testHelper.CreateUnpublishedCollection(project, user, name: randomCollectionName);
                        artifactCollection.SaveWithNewDescription(user, randomArtifactDescription);

                        artifacts.Add(artifactBaseline);
                        artifacts.Add(artifactCollection);
                    }

                selectedBaseArtifactTypes = new List<ItemTypePredefined>();
                foreach (var tp in TestCaseSources.AllArtifactTypesForNovaRestMethods)
                {
                    var itm = (ItemTypePredefined)tp;
                    selectedBaseArtifactTypes.Add(itm);
                }
            }

            var baseArtifactTypes = selectedBaseArtifactTypes ??
                (TestCaseSources.AllArtifactTypesForNovaRestMethods).Select(artifactType =>
                (ItemTypePredefined)artifactType);

            foreach (var artifactType in baseArtifactTypes)
            {
                var randomArtifactName = "Artifact_" + RandomGenerator.RandomAlphaNumericUpperAndLowerCaseAndSpecialCharactersWithSpaces();

                foreach (var project in projectsSubset)
                {
                    // Create artifact in the project with random Name & Description
                    var artifact = testHelper.CreateNovaArtifact(user, project, artifactType, name: randomArtifactName);
                    artifact.SaveWithNewDescription(user, randomArtifactDescription);

                    artifacts.Add(artifact);
                }
            }

            artifacts.ForEach(a => a.Publish(user));
            artifacts.ForEach(a => a.RefreshArtifactFromServer(user));

            // Wait for all artifacts to be available to the search service
            var searchCriteria = new FullTextSearchCriteria(randomArtifactDescription, projectIds: projectsSubset.Select(p => p.Id));
            WaitForFullTextSearchIndexerToUpdate(user, testHelper, searchCriteria, artifacts.Count, timeoutInMilliseconds: timeoutInMilliseconds);

            Logger.WriteInfo("{0} {1} artifacts created.", nameof(SearchServiceTestHelper), artifacts.Count);
            Logger.WriteTrace("{0}.{1} finished.", nameof(SearchServiceTestHelper), nameof(SetupFullTextSearchData));

            // Return the full artifact list
            return artifacts;
        }

        /// <summary>
        /// Waits for expected search criteria to be met with SearchMetadata method (timeout specified in milliseconds)
        /// </summary>
        /// <param name="user">The user performing the search</param>
        /// <param name="testHelper">An instance of TestHelper</param>
        /// <param name="searchCriteria">The full text search criteria</param>
        /// <param name="artifactCount">The number of artifacts that were created</param>
        /// <param name="waitForArtifactsToDisappear">(optional) Flag to indicate whether to wait for the artifacts to disappear. 
        /// (Default is False => Wait for artifacts to appear instead of disappear)</param>
        /// <param name="timeoutInMilliseconds">(optional) Timeout in milliseconds after which search will terminate 
        /// if not successful </param>
        /// <param name="sleepIntervalInMilliseconds">(optional) The amount of time in milliseconds to sleep between retries.</param>
        public static void WaitForFullTextSearchIndexerToUpdate(
            IUser user,
            TestHelper testHelper,
            FullTextSearchCriteria searchCriteria,
            int artifactCount,
            bool waitForArtifactsToDisappear = false,
            int timeoutInMilliseconds = DEFAULT_TIMEOUT_FOR_SEARCH_INDEXER_UPDATE_IN_MS,
            int sleepIntervalInMilliseconds = 1000)
        {
            ThrowIf.ArgumentNull(searchCriteria, nameof(searchCriteria));
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(testHelper, nameof(testHelper));

            DateTime timeout = DateTime.Now.AddMilliseconds(timeoutInMilliseconds);
            DateTime startTime = DateTime.Now;

            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;
            do
            {
                Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                    testHelper.SearchService.FullTextSearchMetaData(user, searchCriteria),
                    "SearchMetaData() call failed when using following search term: {0}!",
                    searchCriteria.Query);

                Thread.Sleep(sleepIntervalInMilliseconds); // Sleep betweeen retries so we don't DoS attack ourselves.
                Logger.WriteDebug("** waitForArtifactsToDisappear = {0}", waitForArtifactsToDisappear);
                Logger.WriteDebug("** fullTextSearchMetaDataResult.TotalCount = {0}", fullTextSearchMetaDataResult.TotalCount);

            } while ((DateTime.Now < timeout) && (
                    (!waitForArtifactsToDisappear && fullTextSearchMetaDataResult.TotalCount < artifactCount) ||
                    (waitForArtifactsToDisappear && fullTextSearchMetaDataResult.TotalCount > artifactCount)
                    ));

            var secondsSpentWaiting = (DateTime.Now - startTime).TotalSeconds;

            var errorMessage = I18NHelper.FormatInvariant(
                    "Created artifact count of {0} does not match expected artifact count of {1} after {2} seconds.",
                    fullTextSearchMetaDataResult.TotalCount,
                    artifactCount,
                    secondsSpentWaiting);

            Assert.AreEqual(artifactCount, fullTextSearchMetaDataResult.TotalCount, errorMessage);
        }

        /// <summary>
        /// Gets the list of all Item Type Ids for a list of projects and base artifact types.
        /// </summary>
        /// <param name="projects">List of projects</param>
        /// <param name="baseArtifactTypes">List of Artifact Types</param>
        /// <returns>List of ItemTypeId</returns>
        public static List<int> GetItemTypeIdsForBaseArtifactTypes(List<IProject> projects,
            List<ItemTypePredefined> artifactTypes)
        {
            ThrowIf.ArgumentNull(projects, nameof(projects));
            ThrowIf.ArgumentNull(artifactTypes, nameof(artifactTypes));

            var itemTypeIds = new List<int>();

            foreach (var baseArtifactType in artifactTypes)
            {
                foreach (var project in projects)
                {
                    var itemTypeId = GetItemTypeIdForBaseArtifactType(project.NovaArtifactTypes, baseArtifactType);

                    itemTypeIds.Add(itemTypeId);
                }
            }

            return itemTypeIds;
        }

        /// <summary>
        /// Gets the Item Type Id for a base artifact type from a list of artifact types
        /// </summary>
        /// <param name="artifactTypes"></param>
        /// <param name="baseArtifactType"></param>
        /// <param name="artifactTypeName"></param>
        /// <returns>An ItemTypeId</returns>
        public static int GetItemTypeIdForBaseArtifactType(
            List<NovaArtifactType> artifactTypes,
            ItemTypePredefined baseArtifactType)
        {
            ThrowIf.ArgumentNull(artifactTypes, nameof(artifactTypes));

            var artifactType = artifactTypes.Find(t => t.PredefinedType == baseArtifactType);

            return artifactType.Id;
        }


        /// <summary>
        /// Updates an artifact property
        /// </summary>
        /// <param name="testHelper">An instance of TestHelper</param>
        /// <param name="user">The user updating the artifact</param>
        /// <param name="project">The project containing the artifact</param>
        /// <param name="artifact">The artifact to update.</param>
        /// <param name="artifactType">The type of artifact.</param>
        /// <param name="propertyToUpdate">Property to update.</param>
        /// <param name="value">The value to what property will be updated</param>
        public static void UpdateArtifactProperty<T>(TestHelper testHelper, IUser user, IProject project, IArtifact artifact, BaseArtifactType artifactType, string propertyToUpdate, T value)
        {
            ThrowIf.ArgumentNull(testHelper, nameof(testHelper));

            var artifactDetails = testHelper.ArtifactStore.GetArtifactDetails(user, artifact.Id);

            CSharpUtilities.SetProperty(propertyToUpdate, value, artifactDetails);

            INovaArtifactDetails updateResult = null;

            Assert.DoesNotThrow(() => updateResult = Artifact.UpdateArtifact(artifact, user, artifactDetails, address: testHelper.BlueprintServer.Address),
                "Exception caught while trying to update an artifact of type: '{0}'!", artifactType);

            Assert.AreEqual(artifactDetails.CreatedBy?.DisplayName, updateResult.CreatedBy?.DisplayName, "The CreatedBy properties don't match!");

            var openApiArtifact = OpenApi.GetArtifact(testHelper.BlueprintServer.Address, project, artifact.Id, user);
            ArtifactStoreHelper.AssertArtifactsEqual(updateResult, artifactDetails);

            TestHelper.AssertArtifactsAreEqual(artifact, openApiArtifact);
        }
    }
}