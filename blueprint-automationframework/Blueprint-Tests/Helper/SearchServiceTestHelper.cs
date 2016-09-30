using Common;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.FullTextSearchModel.Impl;
using Model.Impl;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;
using Utilities.Factories;

namespace Helper
{
    public static class SearchServiceTestHelper
    {
        private const int DEFAULT_TIMEOUT_FOR_SEARCH_INDEXER_UPDATE_IN_MS = 30000;

        /// <summary>
        /// Sets up artifact data for Search Service tests
        /// </summary>
        /// <param name="projects">The projects in which the artifacts will be created</param>
        /// <param name="user">The user creating the artifacts</param>
        /// <param name="testHelper">An instance of TestHelper</param>
        /// <param name="selectedBaseArtifactTypes">(optional) list of seletedBaseAritfactTypes will be used to setup search data</param>
        /// <returns>List of created artifacts</returns>
        public static List<IArtifactBase> SetupSearchData(List<IProject> projects, IUser user, TestHelper testHelper,
            List<BaseArtifactType> selectedBasedArtifactTypes = null)
        {
            ThrowIf.ArgumentNull(projects, nameof(projects));
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(testHelper, nameof(testHelper));

            Logger.WriteTrace("{0}.{1} called.", nameof(SearchServiceTestHelper), nameof(SetupSearchData));

            var baseArtifactTypes = selectedBasedArtifactTypes ?? TestCaseSources.AllArtifactTypesForOpenApiRestMethods;

            var artifacts = new List<IArtifactBase>();

            // This keeps the artifact description constant for all created artifacts
            var randomArtifactDescription = "Description " + RandomGenerator.RandomAlphaNumericUpperAndLowerCaseAndSpecialCharactersWithSpaces();

            foreach (var artifactType in baseArtifactTypes)
            {
                var randomArtifactName = "Artifact_" + RandomGenerator.RandomAlphaNumericUpperAndLowerCaseAndSpecialCharactersWithSpaces();

                // Create artifact in first project with random Name & Description
                var artifact = testHelper.CreateAndPublishArtifact(projects.First(), user, artifactType);
                artifact.Lock();

                UpdateArtifactProperty(testHelper, user, projects.First(), artifact, artifactType, "Name", randomArtifactName );
                UpdateArtifactProperty(testHelper, user, projects.First(), artifact, artifactType, "Description", randomArtifactDescription);

                artifacts.Add(artifact);

                // Create artifact in last project with same Name and Description
                artifact = testHelper.CreateAndPublishArtifact(projects.Last(), user, artifactType);
                artifact.Lock();

                UpdateArtifactProperty(testHelper, user, projects.Last(), artifact, artifactType, "Name", randomArtifactName);
                UpdateArtifactProperty(testHelper, user, projects.Last(), artifact, artifactType, "Description", randomArtifactDescription);

                artifacts.Add(artifact);
            }

            ArtifactBase.PublishArtifacts(artifacts, artifacts.First().Address, user);

            // Wait for all artifacts to be available to the search service
            var searchCriteria = new FullTextSearchCriteria(randomArtifactDescription, projects.Select(p => p.Id));
            WaitForSearchIndexerToUpdate(user, testHelper, searchCriteria, artifacts.Count);

            Logger.WriteInfo("{0} {1} artifacts created.", nameof(SearchServiceTestHelper), artifacts.Count);
            Logger.WriteTrace("{0}.{1} finished.", nameof(SearchServiceTestHelper), nameof(SetupSearchData));

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
        public static void  WaitForSearchIndexerToUpdate(
            IUser user, 
            TestHelper testHelper,
            FullTextSearchCriteria searchCriteria, 
            int artifactCount, 
            bool waitForArtifactsToDisappear = false, 
            int timeoutInMilliseconds = DEFAULT_TIMEOUT_FOR_SEARCH_INDEXER_UPDATE_IN_MS)
        {
            ThrowIf.ArgumentNull(searchCriteria, nameof(searchCriteria));
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(testHelper, nameof(testHelper));

            var timeout = DateTime.Now.AddMilliseconds(timeoutInMilliseconds);

            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;
            do
            {
                Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                    testHelper.FullTextSearch.SearchMetaData(user, searchCriteria),
                    "SearchMetaData() call failed when using following search term: {0}!",
                    searchCriteria.Query);

            } while ((!waitForArtifactsToDisappear && DateTime.Now < timeout && fullTextSearchMetaDataResult.TotalCount < artifactCount ) ||
                    waitForArtifactsToDisappear && DateTime.Now < timeout && fullTextSearchMetaDataResult.TotalCount > artifactCount);


            var errorMessage = I18NHelper.FormatInvariant(
                    "Created artifact count of {0} does not match expected artifact count of {1} after {2} seconds.",
                    fullTextSearchMetaDataResult.TotalCount,
                    artifactCount,
                    timeoutInMilliseconds/1000);

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
        /// <param name="propertyToChange">Property to change.</param>
        /// <param name="value">The value to what property will be changed</param>
        public static void UpdateArtifactProperty<T>(TestHelper testHelper, IUser user, IProject project, IArtifact artifact, BaseArtifactType artifactType, string propertyToChange, T value)
        {
            ThrowIf.ArgumentNull(testHelper, nameof(testHelper));

            var artifactDetails = testHelper.ArtifactStore.GetArtifactDetails(user, artifact.Id);

            SetProperty(propertyToChange, value, ref artifactDetails);

            NovaArtifactDetails updateResult = null;

            Assert.DoesNotThrow(() => updateResult = Artifact.UpdateArtifact(artifact, user, artifactDetails, testHelper.BlueprintServer.Address),
                "Exception caught while trying to update an artifact of type: '{0}'!", artifactType);

            Assert.AreEqual(artifactDetails.CreatedBy?.DisplayName, updateResult.CreatedBy?.DisplayName, "The CreatedBy properties don't match!");

            var openApiArtifact = OpenApiArtifact.GetArtifact(testHelper.BlueprintServer.Address, project, artifact.Id, user);
            updateResult.AssertEquals(artifactDetails);

            TestHelper.AssertArtifactsAreEqual(artifact, openApiArtifact);
        }

        #region private methods

        /// <summary>
        /// Set one property to a specific value.
        /// </summary>
        /// <param name="propertyName">Name of the property in which value will be changed.</param>
        /// <param name="propertyValue">The value to set the property to.</param>
        /// <param name="objectToUpdate">Object that contains the property to be changed.</param>
        private static void SetProperty<T>(string propertyName, T propertyValue, ref NovaArtifactDetails objectToUpdate)
        {
            objectToUpdate.GetType().GetProperty(propertyName).SetValue(objectToUpdate, propertyValue, null);
        }

        #endregion private methods
    }
}