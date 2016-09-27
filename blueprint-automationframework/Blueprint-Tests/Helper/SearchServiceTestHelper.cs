using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.FullTextSearchModel.Impl;
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

            foreach (var artifactType in baseArtifactTypes)
            {
                var randomArtifactName = "Artifact_" + RandomGenerator.RandomAlphaNumericUpperAndLowerCaseAndSpecialCharactersWithSpaces();
                var randomArtifactDescription = "Description " + RandomGenerator.RandomAlphaNumericUpperAndLowerCaseAndSpecialCharactersWithSpaces();

                // Create artifact in first project with random Name & DEscription
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

            // Get Name of last artifact published
            var searchTerm = artifacts.Last().Name;

            // Setup: 
            var searchCriteria = new FullTextSearchCriteria(searchTerm, projects.Select(p => p.Id));

            WaitForSearchIndexerToUpdate(user, testHelper, searchCriteria);

            // Return the full artifact list
            return artifacts;
        }

        /// <summary>
        /// Waits for expected search criteria to be met with SearchMetadata method (timeout specified in milliseconds)
        /// </summary>
        /// <param name="user">The user performing the search</param>
        /// <param name="testHelper">An instance of TestHelper</param>
        /// <param name="searchCriteria">The full text search criteria</param>
        /// <param name="timeoutInMilliseconds">(optional) Timeout in milliseconds after which search will terminate 
        /// if not successful </param>
        /// <returns>True if the search criteria was met within the timeout. False if not.</returns>
        public static bool WaitForSearchIndexerToUpdate(IUser user, TestHelper testHelper,
            FullTextSearchCriteria searchCriteria, int? timeoutInMilliseconds = null)
        {
            ThrowIf.ArgumentNull(searchCriteria, nameof(searchCriteria));
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(testHelper, nameof(testHelper));

            // Default wait of 5 seconds if timeout is not set
            int waitForSearchIndexerMilliseconds = timeoutInMilliseconds ?? 5000;

            var timeout = DateTime.Now.AddSeconds(waitForSearchIndexerMilliseconds);

            FullTextSearchMetaDataResult fullTestSearchMetadataResult = null;
            do
            {
                fullTestSearchMetadataResult = testHelper.FullTextSearch.SearchMetaData(user, searchCriteria);
            } while (fullTestSearchMetadataResult.TotalCount < 1 && DateTime.Now < timeout);

            return fullTestSearchMetadataResult.TotalCount >= 1;
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