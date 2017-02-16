using Common;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Impl;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Model.OpenApiModel.Services;
using Model.SearchServiceModel.Impl;
using Utilities;
using Utilities.Factories;

namespace Helper
{
    public static class SearchServiceTestHelper
    {
        private const int DEFAULT_TIMEOUT_FOR_SEARCH_INDEXER_UPDATE_IN_MS = 300000;

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
        public static List<IArtifactBase> SetupFullTextSearchData(List<IProject> projects, IUser user, TestHelper testHelper,
            List<BaseArtifactType> selectedBaseArtifactTypes = null,
            int timeoutInMilliseconds = DEFAULT_TIMEOUT_FOR_SEARCH_INDEXER_UPDATE_IN_MS)
        {
            ThrowIf.ArgumentNull(projects, nameof(projects));
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(testHelper, nameof(testHelper));

            Logger.WriteTrace("{0}.{1} called.", nameof(SearchServiceTestHelper), nameof(SetupFullTextSearchData));

            var baseArtifactTypes = selectedBaseArtifactTypes ?? TestCaseSources.AllArtifactTypesForOpenApiRestMethods;

            var artifacts = new List<IArtifactBase>();

            var projectsSubset = new List<IProject> {projects[0], projects[1]};

            // This keeps the artifact description constant for all created artifacts
            var randomArtifactDescription = "Description " + RandomGenerator.RandomAlphaNumericUpperAndLowerCaseAndSpecialCharactersWithSpaces();

            foreach (var artifactType in baseArtifactTypes)
            {
                var randomArtifactName = "Artifact_" + RandomGenerator.RandomAlphaNumericUpperAndLowerCaseAndSpecialCharactersWithSpaces();

                foreach (var project in projectsSubset)
                {
                    // Create artifact in first project with random Name & Description
                    var artifact = testHelper.CreateAndSaveArtifact(project, user, artifactType);

                    var propertiesToUpdate = new Dictionary<string, object>
                    {
                        {"Name", randomArtifactName},
                        {"Description", WebUtility.HtmlEncode(randomArtifactDescription)}
                    };

                    UpdateArtifactProperties(testHelper, user, project, artifact, artifactType, propertiesToUpdate);

                    artifacts.Add(artifact);
                }
            }

            ArtifactBase.PublishArtifacts(artifacts, artifacts.First().Address, user);

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

            var timeout = DateTime.Now.AddMilliseconds(timeoutInMilliseconds);

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


            var errorMessage = I18NHelper.FormatInvariant(
                    "Created artifact count of {0} does not match expected artifact count of {1} after {2} seconds.",
                    fullTextSearchMetaDataResult.TotalCount,
                    artifactCount,
                    timeoutInMilliseconds / 1000);

            if (!fullTextSearchMetaDataResult.TotalCount.Equals(artifactCount))
            {
                Logger.WriteError(errorMessage);
            }

            Assert.That(fullTextSearchMetaDataResult.TotalCount.Equals(artifactCount), errorMessage);
        }

        /// <summary>
        /// Gets the list of all Item Type Ids for a list of projects and base artifact types.
        /// </summary>
        /// <param name="projects"></param>
        /// <param name="baseArtifactTypes"></param>
        /// <returns>List of ItemTypeId</returns>
        public static List<int> GetItemTypeIdsForBaseArtifactTypes(List<IProject> projects,
            List<BaseArtifactType> baseArtifactTypes)
        {
            ThrowIf.ArgumentNull(projects, nameof(projects));
            ThrowIf.ArgumentNull(baseArtifactTypes, nameof(baseArtifactTypes));

            var itemTypeIds = new List<int>();

            foreach (var baseArtifactType in baseArtifactTypes)
            {
                foreach (var project in projects)
                {
                    var itemTypeId = GetItemTypeIdForBaseArtifactType(project.ArtifactTypes, baseArtifactType);

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
            List<OpenApiArtifactType> artifactTypes,
            BaseArtifactType baseArtifactType,
            string artifactTypeName = null)
        {
            ThrowIf.ArgumentNull(artifactTypes, nameof(artifactTypes));

            OpenApiArtifactType artifactType;

            if (artifactTypeName == null)
            {
                artifactType = artifactTypes.Find(t => t.BaseArtifactType == baseArtifactType);
            }
            else
            {
                artifactType = artifactTypes.Find(t => t.BaseArtifactType == baseArtifactType && t.Name == artifactTypeName);
            }

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

        /// <summary>
        /// Updates artifact properties
        /// </summary>
        /// <param name="testHelper">An instance of TestHelper</param>
        /// <param name="user">The user updating the artifact</param>
        /// <param name="project">The project containing the artifact</param>
        /// <param name="artifact">The artifact to update.</param>
        /// <param name="artifactType">The type of artifact.</param>
        /// <param name="propertiesToUpdate">Dictionary of properties to update (Key: property name; Value: property value</param>
        public static void UpdateArtifactProperties<T>(TestHelper testHelper, IUser user, IProject project, IArtifact artifact, BaseArtifactType artifactType, Dictionary<string, T> propertiesToUpdate)
        {
            ThrowIf.ArgumentNull(testHelper, nameof(testHelper));
            ThrowIf.ArgumentNull(propertiesToUpdate, nameof(propertiesToUpdate));

            var artifactDetails = testHelper.ArtifactStore.GetArtifactDetails(user, artifact.Id);

            foreach (var kvp in propertiesToUpdate)
            {
                CSharpUtilities.SetProperty(kvp.Key, kvp.Value, artifactDetails);
            }

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