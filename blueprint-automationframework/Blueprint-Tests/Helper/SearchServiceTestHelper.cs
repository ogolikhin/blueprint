using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.FullTextSearchModel.Impl;
using Model.Impl;
using NUnit.Framework;
using Utilities;
using Utilities.Factories;

namespace Helper
{
    public static class SearchServiceTestHelper
    {
        public static List<IArtifactBase> SetupSearchData(List<IProject> projects, IUser user, TestHelper testHelper)
        {
            ThrowIf.ArgumentNull(projects, nameof(projects));
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(testHelper, nameof(testHelper));

            var baseArtifactTypes = new List<BaseArtifactType>()
            {
                BaseArtifactType.Actor,
                BaseArtifactType.BusinessProcess,
                BaseArtifactType.Document,
                BaseArtifactType.DomainDiagram,
                BaseArtifactType.GenericDiagram,
                BaseArtifactType.Glossary,
                BaseArtifactType.Process,
                BaseArtifactType.Storyboard,
                BaseArtifactType.TextualRequirement,
                BaseArtifactType.UIMockup,
                BaseArtifactType.UseCase,
                BaseArtifactType.UseCaseDiagram
            };

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

                artifact.Publish();
                artifacts.Add(artifact);

                // Create artifact in last project with same Name and Description
                artifact = testHelper.CreateAndPublishArtifact(projects.Last(), user, artifactType);
                artifact.Lock();

                UpdateArtifactProperty(testHelper, user, projects.Last(), artifact, artifactType, "Name", randomArtifactName);

                UpdateArtifactProperty(testHelper, user, projects.Last(), artifact, artifactType, "Description", randomArtifactDescription);

                artifact.Publish();
                artifacts.Add(artifact);
            }

            var openApiProperty = artifacts.First().Properties.FirstOrDefault(p => p.Name == "Description");

            Assert.That(openApiProperty != null, "Description property for artifact could not be found!");

            // Search for Description property value which is common to all artifacts
            var searchTerm = openApiProperty.TextOrChoiceValue;

            // Setup: 
            var searchCriteria = new FullTextSearchCriteria(searchTerm, projects.Select(p => p.Id));

            WaitForSearchIndexerToUpdate(user, testHelper, searchCriteria, artifacts.Count);

            // Return the full artifact list
            return artifacts;
        }

        /// <summary>
        /// Waits for expected search criteria to be met with SearchMetadata method (timeout specified in milliseconds)
        /// </summary>
        /// <param name="user">The user performing the search</param>
        /// <param name="testHelper">An instance of TestHelper</param>
        /// <param name="searchCriteria">The full text search criteria</param>
        /// <param name="artifactCount"></param>
        /// <param name="waitForArtifactsToDisappear"></param>
        /// <param name="timeoutInMilliseconds">(optional) Timeout in milliseconds after which search will terminate 
        /// if not successful </param>
        /// <returns>True if the search criteria was met within the timeout. False if not.</returns>
        public static bool WaitForSearchIndexerToUpdate(IUser user, TestHelper testHelper,
            FullTextSearchCriteria searchCriteria, int artifactCount, bool waitForArtifactsToDisappear = false, int? timeoutInMilliseconds = null)
        {
            ThrowIf.ArgumentNull(searchCriteria, nameof(searchCriteria));
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(testHelper, nameof(testHelper));

            // Default wait of 5 seconds if timeout is not set
            int waitForSearchIndexerMilliseconds = timeoutInMilliseconds ?? 5000;

            var timeout = DateTime.Now.AddMilliseconds(waitForSearchIndexerMilliseconds);

            FullTextSearchMetaDataResult fullTextSearchMetaDataResult = null;
            do
            {
                Assert.DoesNotThrow(() => fullTextSearchMetaDataResult =
                    testHelper.FullTextSearch.SearchMetaData(user, searchCriteria),
                    "SearchMetaData() call failed when using following search term: {0}!",
                    searchCriteria.Query);

            } while ((!waitForArtifactsToDisappear && DateTime.Now < timeout && fullTextSearchMetaDataResult.TotalCount < artifactCount ) ||
                    waitForArtifactsToDisappear && DateTime.Now < timeout && fullTextSearchMetaDataResult.TotalCount > artifactCount);

            return fullTextSearchMetaDataResult.TotalCount == artifactCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="artifactTypes"></param>
        /// <param name="baseArtifactType"></param>
        /// <param name="artifactTypeName"></param>
        /// <returns></returns>
        public static int GetItemTypeIdForBaseArtifactType(List<OpenApiArtifactType> artifactTypes,
            BaseArtifactType baseArtifactType, string artifactTypeName = null)
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
        /// 
        /// </summary>
        /// <param name="projects"></param>
        /// <param name="baseArtifactTypes"></param>
        /// <returns></returns>
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

        #region private methods

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
        private static void UpdateArtifactProperty<T>(TestHelper testHelper, IUser user, IProject project, IArtifact artifact, BaseArtifactType artifactType, string propertyToChange, T value)
        {
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

        /// <summary>
        /// Set one primary property to specific value.
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