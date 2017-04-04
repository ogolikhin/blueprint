using Common;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.Impl;
using Model.NovaModel;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Model.ModelHelpers;
using Utilities;
using Utilities.Facades;
using Utilities.Factories;

namespace Helper
{
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]    // This is expected in a Helper class.
    public static class ArtifactStoreHelper
    {
        private const string DEFAULT_COLLECTIONS_ROOT_NAME = "Collections";
        private const int DEFAULT_COLLECTIONS_ROOT_ORDERINDEX = -1;
        private const int DEFAULT_COLLECTIONS_ROOT_PREDEFINEDTYPE = (int)ItemTypePredefined.CollectionFolder;
        private const string DEFAULT_COLLECTIONS_ROOT_PREFIX = "_CFL";

        private const string DEFAULT_BASELINES_AND_REVIEWS_ROOT_NAME = "Baselines and Reviews";
        private const int DEFAULT_BASELINES_AND_REVIEWS_ROOT_ORDERINDEX = -1;
        private const int DEFAULT_BASELINES_AND_REVIEWS_ROOT_PREDEFINEDTYPE = (int)ItemTypePredefined.BaselineFolder;
        private const string DEFAULT_BASELINES_AND_REVIEWS_ROOT_PREFIX = "_BFL";

        #region Custom Asserts

        /// <summary>
        /// Asserts that the list of returned projects contains only the expected project.
        /// </summary>
        /// <param name="returnedProjects">The list of returned projects.</param>
        /// <param name="expectedProject">The expected project.</param>
        public static void AssertOnlyExpectedProjectWasReturned(List<INovaProject> returnedProjects, IProject expectedProject)
        {
            AssertAllExpectedProjectsWereReturned(returnedProjects, new List<IProject> { expectedProject });
        }

        /// <summary>
        /// Asserts that the list of returned Nova projects contains all the expected projects.
        /// </summary>
        /// <param name="returnedProjects">The list of returned projects.</param>
        /// <param name="expectedProjects">The list of expected projects.</param>
        /// <param name="assertNoUnexpectedProjectsWereReturned">(optional) Also verifies that no projects other than the expected projects were returned.
        ///     Pass false to disable this check.</param>
        public static void AssertAllExpectedProjectsWereReturned(
            List<INovaProject> returnedProjects,
            List<IProject> expectedProjects,
            bool assertNoUnexpectedProjectsWereReturned = true)
        {
            ThrowIf.ArgumentNull(expectedProjects, nameof(expectedProjects));
            ThrowIf.ArgumentNull(returnedProjects, nameof(returnedProjects));

            if (assertNoUnexpectedProjectsWereReturned)
            {
                Assert.AreEqual(expectedProjects.Count, returnedProjects.Count,
                    "There should be {0} projects returned!", expectedProjects.Count);
            }

            foreach (var expectedProject in expectedProjects)
            {
                var novaProject = returnedProjects.Find(p => p.Id == expectedProject.Id);

                Assert.NotNull(novaProject, "Project ID {0} was not found in the list of returned projects!", expectedProject.Id);
                Assert.AreEqual(expectedProject.Name, novaProject.Name,
                    "Returned project ID {0} should have Name: '{1}'!", expectedProject.Id, expectedProject.Name);
                Assert.IsNull(novaProject.Description, "The returned project Description should always be null!");
            }
        }

        /// <summary>
        /// Asserts that expected and actulal indicator flags are the same 
        /// </summary>
        /// <param name="actualIndicatorFlags">Actual indicators flags</param>
        /// <param name="expectedIndicatorFlags">Expected indicator flags</param>
        public static void AssertIndicatorFlagsBitsAreEnabled(ItemIndicatorFlags? actualIndicatorFlags, ItemIndicatorFlags? expectedIndicatorFlags)
        {
            if ((actualIndicatorFlags == null) || (expectedIndicatorFlags == null))
            {
                Assert.AreEqual(expectedIndicatorFlags, actualIndicatorFlags, "Both indicator flags must be null!");
            }
            else
            {
                Assert.AreEqual(expectedIndicatorFlags, (expectedIndicatorFlags & actualIndicatorFlags),
                    "Indicator '{0}' was expected but '{1}' was returned.", expectedIndicatorFlags, actualIndicatorFlags);
            }
        }

        /// <summary>
        /// Verifies that actual and expected indicator flags are the same
        /// </summary>
        /// <param name="helper">A TestHelper object</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="artifactId">Artifact Id</param>
        /// <param name="subArtifactId">(optional)Sub-artifact Id. By default artifact indicatorFlags is asserted</param>
        /// <param name="expectedIndicatorFlags">Expected indicator value</param>
        public static void VerifyIndicatorFlags(TestHelper helper, IUser user, int artifactId, ItemIndicatorFlags? expectedIndicatorFlags, int subArtifactId = 0)
        {
            const string ARTIFACT_ID_PATH = RestPaths.Svc.ArtifactStore.ARTIFACTS_id_;
            const string SUB_ARTIFACT_ID_PATH = RestPaths.Svc.ArtifactStore.Artifacts_id_.SUBARTIFACTS_id_;

            if (subArtifactId == 0)
            {
                NovaArtifactDetails artifact = null;
                Assert.DoesNotThrow(() =>
                {
                    artifact = helper.ArtifactStore.GetArtifactDetails(user, artifactId);
                }, "'GET {0}' should return 200 OK when passed a valid artifact ID!", ARTIFACT_ID_PATH);

                AssertIndicatorFlagsBitsAreEnabled(artifact.IndicatorFlags, expectedIndicatorFlags);
            }
            else
            {
                NovaSubArtifact subArtifact = null;
                Assert.DoesNotThrow(() =>
                {
                    subArtifact = helper.ArtifactStore.GetSubartifact(user, artifactId, subArtifactId);
                }, "'GET {0}' should return 200 OK when passed a valid artifact and sub-artifact ID!", SUB_ARTIFACT_ID_PATH);

                AssertIndicatorFlagsBitsAreEnabled(subArtifact.IndicatorFlags, expectedIndicatorFlags);
            }
        }

        /// <summary>
        /// Asserts that the properties of the NovaArtifactResponse match with the specified artifact.  Some properties are expected to be null.
        /// </summary>
        /// <param name="novaArtifactResponse">The artifact returned by the Nova call.</param>
        /// <param name="artifact">The artifact to compare against.</param>
        /// <param name="expectedVersion">The version expected in the NovaArtifactResponse.</param>
        public static void AssertNovaArtifactResponsePropertiesMatchWithArtifact(
            INovaArtifactResponse novaArtifactResponse,
            IArtifactBase artifact,
            int expectedVersion)
        {
            ThrowIf.ArgumentNull(novaArtifactResponse, nameof(novaArtifactResponse));

            AssertNovaArtifactResponsePropertiesMatchWithArtifactSkipVersion(novaArtifactResponse, artifact);
            Assert.AreEqual(expectedVersion, novaArtifactResponse.Version, "The Version properties of the artifacts don't match!");
        }

        /// <summary>
        /// Asserts that the properties of the Nova artifact response match with the specified artifact (but don't check the versions).
        /// Some properties are expected to be null.
        /// </summary>
        /// <param name="novaArtifactResponse">The artifact returned by the Nova call.</param>
        /// <param name="artifact">The artifact to compare against.</param>
        public static void AssertNovaArtifactResponsePropertiesMatchWithArtifactSkipVersion(
            INovaArtifactResponse novaArtifactResponse,
            IArtifactBase artifact)
        {
            ThrowIf.ArgumentNull(novaArtifactResponse, nameof(novaArtifactResponse));
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            Assert.AreEqual(artifact.Id, novaArtifactResponse.Id, "The Id properties of the artifacts don't match!");
            Assert.AreEqual(artifact.ArtifactTypeId, novaArtifactResponse.ItemTypeId, "The ItemTypeId properties of the artifacts don't match!");
            Assert.AreEqual(artifact.Name, novaArtifactResponse.Name, "The Name properties of the artifacts don't match!");
            Assert.AreEqual(artifact.ParentId, novaArtifactResponse.ParentId, "The ParentId properties of the artifacts don't match!");
            Assert.AreEqual(artifact.ProjectId, novaArtifactResponse.ProjectId, "The ProjectId properties of the artifacts don't match!");

            // These properties should always be null:
            Assert.IsNull(novaArtifactResponse.CreatedBy, "The CreatedBy property of the Nova artifact response should always be null!");
            Assert.IsNull(novaArtifactResponse.CreatedOn, "The CreatedOn property of the Nova artifact response should always be null!");
            Assert.IsNull(novaArtifactResponse.Description, "The Description property of the Nova artifact response should always be null!");
            Assert.IsNull(novaArtifactResponse.LastEditedBy, "The LastEditedBy property of the Nova artifact response should always be null!");
            Assert.IsNull(novaArtifactResponse.LastEditedOn, "The LastEditedOn property of the Nova artifact response should always be null!");

            // OpenAPI doesn't have these properties, so they can't be compared:  OrderIndex, PredefinedType, Prefix
        }

        /// <summary>
        /// Asserts that the properties of the Nova artifact response match with the specified artifact.
        /// Some properties are expected to be null.
        /// </summary>
        /// <param name="novaArtifactResponse">The artifact returned by the Nova call.</param>
        /// <param name="artifact">The artifact to compare against.</param>
        /// <param name="expectedVersion">(optional) The version expected in the NovaArtifactResponse.
        ///     By default it compares the version of the NovaArtifactResponse with the artifact.</param>
        public static void AssertNovaArtifactResponsePropertiesMatchWithArtifact(
            INovaArtifactResponse novaArtifactResponse,
            INovaArtifactDetails artifact,
            int? expectedVersion = null)
        {
            ThrowIf.ArgumentNull(novaArtifactResponse, nameof(novaArtifactResponse));
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            Assert.AreEqual(artifact.Id, novaArtifactResponse.Id, "The {0} properties of the artifacts don't match!", nameof(artifact.Id));
            Assert.AreEqual(artifact.ItemTypeIconId, novaArtifactResponse.ItemTypeIconId, "The {0} properties of the artifacts don't match!", nameof(artifact.ItemTypeIconId));
            Assert.AreEqual(artifact.ItemTypeId, novaArtifactResponse.ItemTypeId, "The {0} properties of the artifacts don't match!", nameof(artifact.ItemTypeId));
            Assert.AreEqual(artifact.Name, novaArtifactResponse.Name, "The {0} properties of the artifacts don't match!", nameof(artifact.Name));
            Assert.AreEqual(artifact.ParentId, novaArtifactResponse.ParentId, "The {0} properties of the artifacts don't match!", nameof(artifact.ParentId));
            Assert.AreEqual(artifact.OrderIndex, novaArtifactResponse.OrderIndex, "The {0} properties of the artifacts don't match!", nameof(artifact.OrderIndex));
            Assert.AreEqual(artifact.PredefinedType, novaArtifactResponse.PredefinedType, "The {0} properties of the artifacts don't match!", nameof(artifact.PredefinedType));
            Assert.AreEqual(artifact.Prefix, novaArtifactResponse.Prefix, "The {0} properties of the artifacts don't match!", nameof(artifact.Prefix));
            Assert.AreEqual(artifact.ProjectId, novaArtifactResponse.ProjectId, "The {0} properties of the artifacts don't match!", nameof(artifact.ProjectId));
            Assert.AreEqual(expectedVersion ?? artifact.Version, novaArtifactResponse.Version, "The {0} properties of the artifacts don't match!", nameof(artifact.Version));

            // These properties should always be null:
            Assert.IsNull(novaArtifactResponse.CreatedBy, "The {0} property of the Nova artifact response should always be null!", nameof(artifact.CreatedBy));
            Assert.IsNull(novaArtifactResponse.CreatedOn, "The {0} property of the Nova artifact response should always be null!", nameof(artifact.CreatedOn));
            Assert.IsNull(novaArtifactResponse.Description, "The {0} property of the Nova artifact response should always be null!", nameof(artifact.Description));
            Assert.IsNull(novaArtifactResponse.LastEditedBy, "The {0} property of the Nova artifact response should always be null!", nameof(artifact.LastEditedBy));
            Assert.IsNull(novaArtifactResponse.LastEditedOn, "The {0} property of the Nova artifact response should always be null!", nameof(artifact.LastEditedOn));
        }

        /// <summary>
        /// Asserts that the response from the Nova call contains all the specified artifacts and that they now have the correct version.
        /// </summary>
        /// <param name="artifactAndProjectResponse">The response from the Nova call.</param>
        /// <param name="artifacts">The OpenApi artifacts that we sent to the Nova call.</param>
        /// <param name="expectedVersion">The version expected in the artifacts.</param>
        public static void AssertArtifactsAndProjectsResponseContainsAllArtifactsInListAndHasExpectedVersion(
            INovaArtifactsAndProjectsResponse artifactAndProjectResponse,
            List<IArtifactBase> artifacts,
            int expectedVersion)
        {
            ThrowIf.ArgumentNull(artifactAndProjectResponse, nameof(artifactAndProjectResponse));
            ThrowIf.ArgumentNull(artifacts, nameof(artifacts));

            foreach (var artifact in artifacts)
            {
                var novaArtifactResponse = artifactAndProjectResponse.Artifacts.Find(a => a.Id == artifact.Id);
                Assert.NotNull(novaArtifactResponse, "Couldn't find artifact ID {0} in the list of artifacts!");

                // The artifact doesn't have a version before it's published at least once, so we can't compare version of unpublished artifacts.
                if (artifact.IsPublished)
                {
                    AssertNovaArtifactResponsePropertiesMatchWithArtifact(novaArtifactResponse, artifact, expectedVersion);
                }
                else
                {
                    AssertNovaArtifactResponsePropertiesMatchWithArtifactSkipVersion(novaArtifactResponse, artifact);
                }
            }
        }

        /// <summary>
        /// Asserts that the response from the Nova call contains all the specified artifacts and that they now have the correct version.
        /// </summary>
        /// <param name="artifactAndProjectResponse">The response from the Nova call.</param>
        /// <param name="artifacts">The artifacts that we sent to the Nova call.</param>
        /// <param name="expectedVersion">The version expected in the artifacts.</param>
        public static void AssertArtifactsAndProjectsResponseContainsAllArtifactsInListAndHasExpectedVersion(
            INovaArtifactsAndProjectsResponse artifactAndProjectResponse,
            List<ArtifactWrapper> artifacts,
            int expectedVersion)
        {
            ThrowIf.ArgumentNull(artifactAndProjectResponse, nameof(artifactAndProjectResponse));
            ThrowIf.ArgumentNull(artifacts, nameof(artifacts));

            foreach (var artifact in artifacts)
            {
                var novaArtifactResponse = artifactAndProjectResponse.Artifacts.Find(a => a.Id == artifact.Id);
                Assert.NotNull(novaArtifactResponse, "Couldn't find artifact ID {0} in the list of artifacts!");

                // The artifact doesn't have a version before it's published at least once, so we can't compare version of unpublished artifacts.
                if (artifact.ArtifactState.IsPublished)
                {
                    AssertNovaArtifactResponsePropertiesMatchWithArtifact(novaArtifactResponse, artifact.Artifact, expectedVersion);
                }
                else
                {
                    AssertNovaArtifactResponsePropertiesMatchWithArtifact(novaArtifactResponse, artifact.Artifact);
                }
            }
        }

        /// <summary>
        /// Asserts that the response from the Nova call contains all the specified artifacts.
        /// </summary>
        /// <param name="artifactAndProjectResponse">The response from the Nova call.</param>
        /// <param name="artifacts">The OpenApi artifacts that we sent to the Nova call.</param>
        public static void AssertArtifactsAndProjectsResponseContainsAllArtifactsInList(
            INovaArtifactsAndProjectsResponse artifactAndProjectResponse,
            List<IArtifactBase> artifacts)
        {
            ThrowIf.ArgumentNull(artifactAndProjectResponse, nameof(artifactAndProjectResponse));
            ThrowIf.ArgumentNull(artifacts, nameof(artifacts));

            foreach (var artifact in artifacts)
            {
                var novaArtifactResponse = artifactAndProjectResponse.Artifacts.Find(a => a.Id == artifact.Id);
                Assert.NotNull(novaArtifactResponse, "Couldn't find artifact ID {0} in the list of artifacts!");

                AssertNovaArtifactResponsePropertiesMatchWithArtifactSkipVersion(novaArtifactResponse, artifact);
            }
        }

        /// <summary>
        /// Asserts that the specified INovaArtifactBase object is equal to the specified IArtifactBase.
        /// </summary>
        /// <param name="expectedNovaArtifactBase">The INovaArtifactBase containing the expected properties.</param>
        /// <param name="actualArtifactBase">The IArtifactBase containing the actual properties to compare against.</param>
        /// <param name="skipIdAndVersion">(optional) Pass true to skip comparison of the Id and Version properties.</param>
        /// <exception cref="AssertionException">If any of the properties are different.</exception>
        public static void AssertArtifactsEqual(INovaArtifactBase expectedNovaArtifactBase, IArtifactBase actualArtifactBase, bool skipIdAndVersion = false)
        {
            ThrowIf.ArgumentNull(expectedNovaArtifactBase, nameof(expectedNovaArtifactBase));
            ThrowIf.ArgumentNull(actualArtifactBase, nameof(actualArtifactBase));

            if (!skipIdAndVersion)
            {
                Assert.AreEqual(expectedNovaArtifactBase.Id, actualArtifactBase.Id, "The Id parameters don't match!");
                Assert.AreEqual(expectedNovaArtifactBase.Version, actualArtifactBase.Version, "The Version  parameters don't match!");
            }

            Assert.AreEqual(expectedNovaArtifactBase.Name, actualArtifactBase.Name, "The Name  parameters don't match!");
            Assert.AreEqual(expectedNovaArtifactBase.ParentId, actualArtifactBase.ParentId, "The ParentId  parameters don't match!");
            Assert.AreEqual(expectedNovaArtifactBase.ItemTypeId, actualArtifactBase.ArtifactTypeId, "The ItemTypeId  parameters don't match!");
            Assert.AreEqual(expectedNovaArtifactBase.ProjectId, actualArtifactBase.ProjectId, "The ProjectId  parameters don't match!");
        }

        /// <summary>
        /// Asserts that the specified INovaArtifactBase object is equal to the specified IArtifactBase.
        /// </summary>
        /// <param name="expectedArtifactBase">The IArtifactBase containing the expected properties.</param>
        /// <param name="actualNovaArtifactBase">The INovaArtifactBase containing the actual properties to compare against.</param>
        /// <param name="skipIdAndVersion">(optional) Pass true to skip comparison of the Id and Version properties.</param>
        /// <param name="skipParentIds">(optional) Pass true to skip comparison of the ParentId properties.</param>
        /// <exception cref="AssertionException">If any of the properties are different.</exception>
        public static void AssertArtifactsEqual(IArtifactBase expectedArtifactBase,
            INovaArtifactBase actualNovaArtifactBase,
            bool skipIdAndVersion = false,
            bool skipParentIds = false)
        {
            ThrowIf.ArgumentNull(actualNovaArtifactBase, nameof(actualNovaArtifactBase));
            ThrowIf.ArgumentNull(expectedArtifactBase, nameof(expectedArtifactBase));

            if (!skipIdAndVersion)
            {
                Assert.AreEqual(expectedArtifactBase.Id, actualNovaArtifactBase.Id, "The Id parameters don't match!");
                Assert.AreEqual(expectedArtifactBase.Version, actualNovaArtifactBase.Version, "The Version  parameters don't match!");
            }

            if (!skipParentIds)
            {
                Assert.AreEqual(expectedArtifactBase.ParentId, actualNovaArtifactBase.ParentId, "The ParentId  parameters don't match!");
            }

            Assert.AreEqual(expectedArtifactBase.Name, actualNovaArtifactBase.Name, "The Name  parameters don't match!");
            Assert.AreEqual(expectedArtifactBase.ArtifactTypeId, actualNovaArtifactBase.ItemTypeId, "The ItemTypeId  parameters don't match!");
            Assert.AreEqual(expectedArtifactBase.ProjectId, actualNovaArtifactBase.ProjectId, "The ProjectId  parameters don't match!");
        }

        
        /// <summary>
        /// Asserts that both INovaArtifactDetails objects are equal.
        /// </summary>
        /// <param name="artifact1">The first INovaArtifactDetails to compare against.</param>
        /// <param name="artifact2">The second INovaArtifactDetails to compare against.</param>
        /// <param name="skipIdAndVersion">(optional) Pass true to skip comparison of the Id and Version properties.</param>
        /// <param name="skipParentId">(optional) Pass true to skip comparison of the ParentId properties.</param>
        /// <param name="skipOrderIndex">(optional) Pass true to skip comparison of the OrderIndex properties.</param>
        /// <param name="skipCreatedBy">(optional) Pass true to skip comparison of the CreatedBy properties.</param>
        /// <param name="skipPublishedProperties">(optional) Pass true to skip comparison of properties that only published artifacts have.</param>
        /// <param name="skipPermissions">(optional) Pass true to skip comparison of the Permissions properties.</param>
        /// <param name="skipDescription">(optional) Pass true to skip comparison of the Description properties.</param>
        /// <exception cref="AssertionException">If any of the properties are different.</exception>
        public static void AssertArtifactsEqual(INovaArtifactDetails artifact1, INovaArtifactDetails artifact2,
            bool skipIdAndVersion = false, bool skipParentId = false, bool skipOrderIndex = false, bool skipCreatedBy = false,
            bool skipPublishedProperties = false, bool skipPermissions = false, bool skipDescription = false)
        {
            ThrowIf.ArgumentNull(artifact1, nameof(artifact1));
            ThrowIf.ArgumentNull(artifact2, nameof(artifact2));

            if (!skipIdAndVersion)
            {
                Assert.AreEqual(artifact1.Id, artifact2.Id, "The Id parameters don't match!");
                Assert.AreEqual(artifact1.Version, artifact2.Version, "The Version parameters don't match!");
            }

            if (!skipParentId)
            {
                Assert.AreEqual(artifact1.ParentId, artifact2.ParentId, "The ParentId parameters don't match!");
            }

            if (!skipOrderIndex)
            {
                Assert.AreEqual(artifact1.OrderIndex, artifact2.OrderIndex, "The OrderIndex parameters don't match!");
            }

            if (!skipCreatedBy)
            {
                Identification.AssertEquals(artifact1.CreatedBy, artifact2.CreatedBy);
            }

            if (!skipPermissions)
            {
                Assert.AreEqual(artifact1.Permissions, artifact2.Permissions, "The Permissions parameters don't match!");
            }

            if (!skipPublishedProperties)
            {
                Assert.AreEqual(artifact1.CreatedOn, artifact2.CreatedOn, "The CreatedOn parameters don't match!");
                Assert.AreEqual(artifact1.LastEditedOn, artifact2.LastEditedOn, "The LastEditedOn parameters don't match!");
                Assert.AreEqual(artifact1.LockedDateTime, artifact2.LockedDateTime, "The LockedDateTime parameters don't match!");
                Identification.AssertEquals(artifact1.LastEditedBy, artifact2.LastEditedBy);
                Identification.AssertEquals(artifact1.LockedByUser, artifact2.LockedByUser);
            }

            if (!skipDescription)
            {
                Assert.AreEqual(artifact1.Description, artifact2.Description, "The Description parameters don't match!");
            }

            Assert.AreEqual(artifact1.Name, artifact2.Name, "The Name parameters don't match!");
            Assert.AreEqual(artifact1.ItemTypeId, artifact2.ItemTypeId, "The ItemTypeId parameters don't match!");
            Assert.AreEqual(artifact1.ItemTypeVersionId, artifact2.ItemTypeVersionId, "The ItemTypeVersionId parameters don't match!");
            Assert.AreEqual(artifact1.ProjectId, artifact2.ProjectId, "The ProjectId parameters don't match!");
            Assert.AreEqual(artifact1.LastSaveInvalid, artifact2.LastSaveInvalid, "The LastSaveInvalid parameters don't match!");

            Assert.AreEqual(artifact1.CustomPropertyValues.Count, artifact2.CustomPropertyValues.Count, "The number of Custom Properties is different!");
            Assert.AreEqual(artifact1.SpecificPropertyValues.Count, artifact2.SpecificPropertyValues.Count, "The number of Specific Property Values is different!");

            // Now compare each property in CustomProperties & SpecificPropertyValues.
            foreach (var property in artifact1.CustomPropertyValues)
            {
                Assert.That(artifact2.CustomPropertyValues.Exists(p => p.Name == property.Name),
                "Couldn't find a CustomProperty named '{0}'!", property.Name);
            }

            foreach (var property in artifact1.SpecificPropertyValues)
            {
                Assert.That(artifact2.SpecificPropertyValues.Exists(p => p.Name == property.Name),
                "Couldn't find a SpecificPropertyValue named '{0}'!", property.Name);
            }
        }

        /// <summary>
        /// Asserts that the specified INovaArtifactDetails object is equal to the specified INovaArtifactResponse.
        /// </summary>
        /// <param name="artifact1">The first INovaArtifactDetails to compare against.</param>
        /// <param name="artifact2">The second INovaArtifactResponse to compare against.</param>
        /// <param name="skipDatesAndDescription">(optional) Pass true to skip comparing the Created*, LastEdited* and Description properties.
        ///     This is needed when comparing the response of the GetUnpublishedChanges REST call which always returns null for those fields.</param>
        /// <exception cref="AssertionException">If any of the properties are different.</exception>
        public static void AssertArtifactsEqual(INovaArtifactDetails artifact1, INovaArtifactResponse artifact2, bool skipDatesAndDescription = false)
        {
            ThrowIf.ArgumentNull(artifact1, nameof(artifact1));
            ThrowIf.ArgumentNull(artifact2, nameof(artifact2));

            Assert.AreEqual(artifact1.Id, artifact2.Id, "The Id parameters don't match!");
            Assert.AreEqual(artifact1.Name, artifact2.Name, "The Name  parameters don't match!");
            Assert.AreEqual(artifact1.ParentId, artifact2.ParentId, "The ParentId  parameters don't match!");
            Assert.AreEqual(artifact1.OrderIndex, artifact2.OrderIndex, "The OrderIndex  parameters don't match!");
            Assert.AreEqual(artifact1.ItemTypeId, artifact2.ItemTypeId, "The ItemTypeId  parameters don't match!");
            Assert.AreEqual(artifact1.ProjectId, artifact2.ProjectId, "The ProjectId  parameters don't match!");
            Assert.AreEqual(artifact1.Version, artifact2.Version, "The Version  parameters don't match!");

            if (!skipDatesAndDescription)
            {
                Assert.AreEqual(artifact1.Description, artifact2.Description, "The Description  parameters don't match!");
                Assert.AreEqual(artifact1.CreatedOn, artifact2.CreatedOn, "The CreatedOn  parameters don't match!");
                Assert.AreEqual(artifact1.LastEditedOn, artifact2.LastEditedOn, "The LastEditedOn  parameters don't match!");

                Identification.AssertEquals(artifact1.CreatedBy, artifact2.CreatedBy);
                Identification.AssertEquals(artifact1.LastEditedBy, artifact2.LastEditedBy);
            }
        }

        /// <summary>
        /// Asserts that the INovaArtifactDetails & INovaVersionControlArtifactInfo objects are equal.
        /// </summary>
        /// <param name="artifact1">The first INovaArtifactDetails to compare against.</param>
        /// <param name="artifact2">The second INovaVersionControlArtifactInfo to compare against.</param>
        /// <param name="compareVersions">(optional) Pass false to skip version comparison.  Versions will never be compared if the Version of artifact2 is null.</param>
        /// <param name="compareLockInfo">(optional) Pass false to skip the LockedByUser and LockedDateTime comparisons.</param>
        /// <exception cref="AssertionException">If any of the properties are different.</exception>
        public static void AssertArtifactsEqual(INovaArtifactDetails artifact1,
            INovaVersionControlArtifactInfo artifact2,
            bool compareVersions = true,
            bool compareLockInfo = true)
        {
            ThrowIf.ArgumentNull(artifact1, nameof(artifact1));
            ThrowIf.ArgumentNull(artifact2, nameof(artifact2));

            Assert.AreEqual(artifact1.Id, artifact2.Id, "The Id parameters don't match!");
            Assert.AreEqual(artifact1.ItemTypeId, artifact2.ItemTypeId, "The ItemTypeId  parameters don't match!");
            Assert.AreEqual(artifact1.Name, artifact2.Name, "The Name  parameters don't match!");
            Assert.AreEqual(artifact1.OrderIndex, artifact2.OrderIndex, "The OrderIndex  parameters don't match!");
            Assert.AreEqual(artifact1.ParentId, artifact2.ParentId, "The ParentId  parameters don't match!");
            Assert.AreEqual((int?)artifact1.Permissions, artifact2.Permissions, "The Permissions  parameters don't match!");
            Assert.AreEqual(artifact1.ProjectId, artifact2.ProjectId, "The ProjectId  parameters don't match!");
            Assert.AreEqual(artifact1.PredefinedType, artifact2.PredefinedType, "The PredefinedType  parameters don't match!");
            Assert.AreEqual(artifact1.Prefix, artifact2.Prefix, "The Prefix  parameters don't match!");

            // The Version property in VersionControlInfo is always null until the artifact is deleted.
            if (compareVersions && (artifact2.Version != null))
            {
                Assert.AreEqual(artifact1.Version, artifact2.Version, "The Version  parameters don't match!");
            }

            if (compareLockInfo)
            {
                Assert.AreEqual(artifact1.LockedDateTime, artifact2.LockedDateTime, "The LockedDateTime  parameters don't match!");
                Identification.AssertEquals(artifact1.LockedByUser, artifact2.LockedByUser);
            }
        }

        /// <summary>
        /// Asserts that both NovaSubArtifact objects are equal.
        /// </summary>
        /// <param name="expectedSubArtifact">The expected NovaSubArtifact.</param>
        /// <param name="actualSubArtifact">The actual NovaSubArtifact to compare against the expected NovaSubArtifact.</param>
        /// <param name="artifactStore">An ArtifactStore to make REST calls to.</param>
        /// <param name="user">User to authenticate with.</param>
        /// <param name="expectedParentId">(optional) Pass the expected ParentId property of the actualSubArtifact or leave null if the 2 NovaSubArtifacts
        ///     should have the same ParentId.</param>
        /// <param name="propertyCompareOptions">(optional) Specifies which properties to compare.  By default, all properties are compared.</param>
        /// <param name="attachmentCompareOptions">(optional) Specifies which Attachments properties to compare.  By default, all properties are compared.</param>
        /// <exception cref="AssertionException">If any of the properties are different.</exception>
        public static void AssertSubArtifactsAreEqual(NovaSubArtifact expectedSubArtifact, NovaSubArtifact actualSubArtifact, IArtifactStore artifactStore, IUser user,
            int? expectedParentId = null, NovaItem.PropertyCompareOptions propertyCompareOptions = null, Attachments.CompareOptions attachmentCompareOptions = null)
        {
            ThrowIf.ArgumentNull(expectedSubArtifact, nameof(expectedSubArtifact));
            ThrowIf.ArgumentNull(actualSubArtifact, nameof(actualSubArtifact));
            ThrowIf.ArgumentNull(artifactStore, nameof(artifactStore));
            ThrowIf.ArgumentNull(propertyCompareOptions, nameof(propertyCompareOptions));

            Assert.AreEqual(expectedSubArtifact.IsDeleted, actualSubArtifact.IsDeleted, "The IsDeleted parameters don't match!");

            if (propertyCompareOptions.CompareArtifactIds)
            {
                Assert.AreEqual(expectedSubArtifact.Id, actualSubArtifact.Id, "The Id parameters don't match!");
            }

            expectedParentId = expectedParentId ?? expectedSubArtifact.ParentId;
            Assert.AreEqual(expectedParentId, actualSubArtifact.ParentId, "The ParentId parameters don't match!");

            if (propertyCompareOptions.CompareOrderIndeces)
            {
                Assert.AreEqual(expectedSubArtifact.OrderIndex, actualSubArtifact.OrderIndex, "The OrderIndex parameters don't match!");
            }

            Assert.AreEqual(expectedSubArtifact.Name, actualSubArtifact.Name, "The Name parameters don't match!");

            if (propertyCompareOptions.CompareDescriptions)
            {
                Assert.AreEqual(expectedSubArtifact.Description, actualSubArtifact.Description, "The Description parameters don't match!");
            }
            Assert.AreEqual(expectedSubArtifact.ItemTypeId, actualSubArtifact.ItemTypeId, "The ItemTypeId parameters don't match!");
            Assert.AreEqual(expectedSubArtifact.ItemTypeName, actualSubArtifact.ItemTypeName, "The ItemTypeName parameters don't match!");

            Assert.AreEqual(expectedSubArtifact.ItemTypeVersionId, actualSubArtifact.ItemTypeVersionId, "The ItemTypeVersionId parameters don't match!");

            Assert.AreEqual(expectedSubArtifact.ItemTypeIconId, actualSubArtifact.ItemTypeIconId, "The ItemTypeIconId parameters don't match!");
            Assert.AreEqual(expectedSubArtifact.Prefix, actualSubArtifact.Prefix, "The Prefix parameters don't match!");
            Assert.AreEqual(expectedSubArtifact.PredefinedType, actualSubArtifact.PredefinedType, "The PredefinedType parameters don't match!");

            if (propertyCompareOptions.CompareCustomProperties)
            {
                ComparePropertyValuesLists(expectedSubArtifact.CustomPropertyValues, actualSubArtifact.CustomPropertyValues);
            }

            if (propertyCompareOptions.CompareSpecificPropertyValues)
            {
                ComparePropertyValuesLists(expectedSubArtifact.SpecificPropertyValues, actualSubArtifact.SpecificPropertyValues,
                    skipVirtualProperties: true);
            }

            // NOTE: Currently, NovaSubArtifacts don't return any Attachments, DocReferences or Traces.  You need to make separate calls to get those.
            Assert.AreEqual(0, expectedSubArtifact?.AttachmentValues.Count, "AttachmentValues should always be empty!");
            Assert.AreEqual(0, expectedSubArtifact?.DocRefValues.Count, "DocRefValues should always be empty!");
            Assert.AreEqual(0, expectedSubArtifact?.Traces.Count, "Traces should always be empty!");
            Assert.AreEqual(expectedSubArtifact?.AttachmentValues.Count, actualSubArtifact?.AttachmentValues.Count, "The number of Sub-Artifact Attachments don't match!");
            Assert.AreEqual(expectedSubArtifact?.DocRefValues.Count, actualSubArtifact?.DocRefValues.Count, "The number of Sub-Artifact Document References don't match!");
            Assert.AreEqual(expectedSubArtifact?.Traces.Count, actualSubArtifact?.Traces.Count, "The number of Sub-Artifact Traces don't match!");

            // Get and compare sub-artifact Attachments & DocumentReferences.
            var expectedAttachments = ArtifactStore.GetAttachments(artifactStore.Address, expectedSubArtifact.ParentId.Value, user, subArtifactId: expectedSubArtifact.Id);
            var actualAttachments = ArtifactStore.GetAttachments(artifactStore.Address, actualSubArtifact.ParentId.Value, user, subArtifactId: actualSubArtifact.Id);

            Attachments.AssertAreEqual(expectedAttachments, actualAttachments, attachmentCompareOptions);

            // Get and compare sub-artifact Traces.
            if (propertyCompareOptions.CompareTraces)
            {
                var expectedRelationships = artifactStore.GetRelationships(user, expectedSubArtifact.ParentId.Value, expectedSubArtifact.Id.Value);
                var actualRelationships = artifactStore.GetRelationships(user, actualSubArtifact.ParentId.Value, actualSubArtifact.Id.Value);

                Relationships.AssertRelationshipsAreEqual(expectedRelationships, actualRelationships);
            }
        }

        /// <summary>
        /// Compares two lists of Custom properties.
        /// </summary>
        /// <param name="expectedPropertyValues">List of expected Custom Properties</param>
        /// <param name="actualPropertyValues">List of actual Custom Properties</param>
        /// <param name="skipVirtualProperties">(Optional) Set true to skip comparing properties with empty 'name' or 'typeid' -1.
        ///     Set 'true' when comparing Specific properties.</param>
        private static void ComparePropertyValuesLists(List<CustomProperty> expectedPropertyValues,
            List<CustomProperty> actualPropertyValues, bool skipVirtualProperties = false)
        {
            Assert.AreEqual(expectedPropertyValues.Count, actualPropertyValues.Count,
                    "The number of Custom Properties is different!");

            // Compare each property in CustomProperties.
            foreach (var expectedProperty in expectedPropertyValues)
            {
                Assert.That(actualPropertyValues.Exists(p => p.Name == expectedProperty.Name),
                "Couldn't find a Custom Property named '{0}'!", expectedProperty.Name);

                CustomProperty actualProperty = null;
                if (skipVirtualProperties)
                {
                    if ((expectedProperty.Name != null) || (expectedProperty.PropertyTypeId != -1))
                        actualProperty = actualPropertyValues.Find(cp => cp.Name == expectedProperty.Name);
                }
                else
                {
                    actualProperty = actualPropertyValues.Find(cp => cp.Name == expectedProperty.Name);
                }

                if (actualProperty != null)
                {
                    AssertCustomPropertiesAreEqual(expectedProperty, actualProperty);
                }
            }
        }

        /// <summary>
        /// Compares Two Custom Properties for Equality
        /// </summary>
        /// <param name="expectedProperty">The expected custom property.</param>
        /// <param name="actualProperty">The actual custom property to be compared with the expected custom property.</param>
        public static void AssertCustomPropertiesAreEqual(CustomProperty expectedProperty, CustomProperty actualProperty)
        {
            ThrowIf.ArgumentNull(expectedProperty, nameof(expectedProperty));
            ThrowIf.ArgumentNull(actualProperty, nameof(actualProperty));

            Assert.AreEqual(expectedProperty.IsMultipleAllowed, actualProperty.IsMultipleAllowed, "The IsMultipleAllowed properties don't match!");
            Assert.AreEqual(expectedProperty.IsReuseReadOnly, actualProperty.IsReuseReadOnly, "The IsReuseReadOnly properties don't match!");
            Assert.AreEqual(expectedProperty.IsRichText, actualProperty.IsRichText, "The IsRichText properties don't match!");
            Assert.AreEqual(expectedProperty.Name, actualProperty.Name, "The Name properties don't match!");
            Assert.AreEqual(expectedProperty.PrimitiveType, actualProperty.PrimitiveType, "The PrimitiveType properties don't match!");
            Assert.AreEqual(expectedProperty.PropertyType, actualProperty.PropertyType, "The PropertyType properties don't match!");
            Assert.AreEqual(expectedProperty.PropertyTypeId, actualProperty.PropertyTypeId, "The PropertyTypeId properties don't match!");
            Assert.AreEqual(expectedProperty.PropertyTypeVersionId, actualProperty.PropertyTypeVersionId, "The PropertyTypeVersionId properties don't match!");

            if (expectedProperty.PrimitiveType == null)
            {
                string expectedPropertyString = expectedProperty?.CustomPropertyValue?.ToString();
                string actualPropertyString = actualProperty?.CustomPropertyValue?.ToString();
                Assert.AreEqual(expectedPropertyString, actualPropertyString, "The CustomPropertyValues don't match!");
                return;
            }

            var primitiveType = (PropertyPrimitiveType)expectedProperty.PrimitiveType;

            switch (primitiveType)
            {
                case PropertyPrimitiveType.Text:
                case PropertyPrimitiveType.Number:
                    Assert.AreEqual(expectedProperty.CustomPropertyValue, actualProperty.CustomPropertyValue, "The custom {0} properties do not match.", primitiveType);
                    break;

                case PropertyPrimitiveType.Date:
                    DateTime firstCustomPropertyValue;
                    DateTime secondCustomPropertyValue;

                    if (expectedProperty.CustomPropertyValue is DateTime)
                    {
                        firstCustomPropertyValue = (DateTime)expectedProperty.CustomPropertyValue;
                    }
                    else
                    {
                        firstCustomPropertyValue = DateTime.Parse(SerializationUtilities.CastOrDeserialize<string>(expectedProperty.CustomPropertyValue), CultureInfo.InvariantCulture);
                    }

                    if (actualProperty.CustomPropertyValue is DateTime)
                    {
                        secondCustomPropertyValue = (DateTime)actualProperty.CustomPropertyValue;
                    }
                    else
                    {
                        secondCustomPropertyValue = DateTime.Parse(SerializationUtilities.CastOrDeserialize<string>(actualProperty.CustomPropertyValue), CultureInfo.InvariantCulture);
                    }

                    Assert.AreEqual(firstCustomPropertyValue, secondCustomPropertyValue, "The custom {0} properties do not match.", primitiveType);
                    break;

                case PropertyPrimitiveType.Choice:
                {
                    var expectedCustomProperty = SerializationUtilities.CastOrDeserialize<ChoiceValues>(expectedProperty.CustomPropertyValue);
                    var actualCustomProperty = SerializationUtilities.CastOrDeserialize<ChoiceValues>(actualProperty.CustomPropertyValue);

                    Assert.AreEqual(expectedCustomProperty.ValidValues.Count, actualCustomProperty.ValidValues.Count,
                        "The custom {0} property counts are not equal.", primitiveType);

                    for (int i = 0; i < expectedCustomProperty.ValidValues.Count; i++)
                    {
                        var choiceValue1 = expectedCustomProperty.ValidValues[i];
                        var choiceValue2 = actualCustomProperty.ValidValues[i];

                        Assert.AreEqual(choiceValue1.Id, choiceValue2.Id, "The custom {0} property Ids are not equal.", primitiveType);
                        Assert.AreEqual(choiceValue1.Value, choiceValue2.Value, "The custom {0} property choice values are not equal.",
                            primitiveType);
                    }

                    if (!string.IsNullOrEmpty(expectedCustomProperty.CustomValue))
                    {
                        var customValue1 = actualCustomProperty.CustomValue;
                        var customValue2 = actualCustomProperty.CustomValue;

                        Assert.AreEqual(customValue1, customValue2, "The custom {0} property CustomValues are not equal.", primitiveType);
                    }
                    else if (string.IsNullOrEmpty(expectedCustomProperty.CustomValue) &&
                             !string.IsNullOrEmpty(actualCustomProperty.CustomValue))
                    {
                        Assert.Fail("The custom {0} property CustomValue was null for the expected property but the CustomValue " +
                                    "for the actual property was not null.", primitiveType);
                    }

                    break;
                }
                case PropertyPrimitiveType.User:
                {
                    var expectedCustomProperty = SerializationUtilities.CastOrDeserialize<UserGroupValues>(expectedProperty.CustomPropertyValue);
                    var actualCustomProperty = SerializationUtilities.CastOrDeserialize<UserGroupValues>(actualProperty.CustomPropertyValue);

                    Assert.AreEqual(expectedCustomProperty.UsersGroups.Count, actualCustomProperty.UsersGroups.Count,
                        "The custom {0} property counts are not equal.", primitiveType);

                    for (int i = 0; i < expectedCustomProperty.UsersGroups.Count; i++)
                    {
                        var userGroupValue1 = expectedCustomProperty.UsersGroups[i];
                        var userGroupValue2 = actualCustomProperty.UsersGroups[i];

                        Assert.AreEqual(userGroupValue1.Id, userGroupValue2.Id, "The custom {0} property Ids are not equal.", primitiveType);
                        Assert.AreEqual(userGroupValue1.DisplayName, userGroupValue2.DisplayName,
                            "The custom {0} Display Names are not equal.", primitiveType);

                        if ((userGroupValue1.IsGroup != null) || (userGroupValue2.IsGroup != null))
                        {
                            Assert.AreEqual(userGroupValue1.IsGroup, userGroupValue2.IsGroup,
                                "The custom {0} property IsGroup flags are not equal.", primitiveType);
                        }
                    }

                    if (!string.IsNullOrEmpty(expectedCustomProperty.Label))
                    {
                        var customValue1 = expectedCustomProperty.Label;
                        var customValue2 = actualCustomProperty.Label;

                        Assert.AreEqual(customValue1, customValue2, "The custom {0} property Labels are not equal.", primitiveType);
                    }
                    else if (string.IsNullOrEmpty(expectedCustomProperty.Label) &&
                             !string.IsNullOrEmpty(actualCustomProperty.Label))
                    {
                        Assert.Fail("The custom {0} property Label was null for the expected property but the Label " +
                                    "for the actual property was not null.", primitiveType);
                    }
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(I18NHelper.FormatInvariant("The primitive type: {0} was not expected", primitiveType.ToString()));
            }
        }

        /// <summary>
        /// Compares Two Attachments for Equality
        /// </summary>
        /// <param name="expectedAttachment">The expected attachment.</param>
        /// <param name="actualAttachment">The actual attachment to be compared with the expected attachment.</param>
        public static void AssertAttachmentsAreEqual(AttachmentValue expectedAttachment, AttachmentValue actualAttachment)
        {
            ThrowIf.ArgumentNull(expectedAttachment, nameof(expectedAttachment));
            ThrowIf.ArgumentNull(actualAttachment, nameof(actualAttachment));

            Assert.AreEqual(expectedAttachment.AttachmentId, actualAttachment.AttachmentId, "The AttachmentId values do not match!");
            Assert.AreEqual(expectedAttachment.Guid, actualAttachment.Guid, "The attachment GUID values do not match!");

            // TODO: Investigate the impact of this change type assertion
            // Assert.AreEqual(expectedAttachment.ChangeType, actualAttachment.ChangeType);

            Assert.AreEqual(expectedAttachment.FileName, actualAttachment.FileName, "The attachment FileName values do not match!");
            Assert.AreEqual(expectedAttachment.FileType, actualAttachment.FileType, "The attachment FileType values do not match!");
            Assert.AreEqual(expectedAttachment.UploadedDate, actualAttachment.UploadedDate, "The attachment UploadedDate values do not match!");
            Assert.AreEqual(expectedAttachment.UserId, actualAttachment.UserId, "The attachment UserId values do not match!");
            Assert.AreEqual(expectedAttachment.UserName, actualAttachment.UserName, "The attachment UserName values do not match!");
        }

        /// <summary>
        /// Compares traces and asserts that each of their properties are equal.
        /// </summary>
        /// <param name="expectedArtifactTrace">The expected trace.</param>
        /// <param name="actualArtifactTrace">The actual trace.</param>
        /// <param name="checkDirection">(optional) Pass false if you don't want to compare the Direction properties of the traces.</param>
        public static void AssertTracesAreEqual(ITrace expectedArtifactTrace, ITrace actualArtifactTrace, bool checkDirection = true)
        {
            ThrowIf.ArgumentNull(expectedArtifactTrace, nameof(expectedArtifactTrace));
            ThrowIf.ArgumentNull(actualArtifactTrace, nameof(actualArtifactTrace));

            Assert.AreEqual(expectedArtifactTrace.ProjectId, actualArtifactTrace.ProjectId, "The Project IDs of the traces don't match!");
            Assert.AreEqual(expectedArtifactTrace.ArtifactId, actualArtifactTrace.ArtifactId, "The Artifact IDs of the traces don't match!");
            Assert.AreEqual(expectedArtifactTrace.IsSuspect, actualArtifactTrace.IsSuspect, "One trace is marked suspect but the other isn't!");

            if (checkDirection)
            {
                Assert.AreEqual(expectedArtifactTrace.Direction, actualArtifactTrace.Direction, "The Trace Directions don't match!");
            }
        }

        /// <summary>
        /// Compare expected artifact with actual collection item
        /// </summary>
        /// <param name="expectedArtifact">The expected artifact</param>
        /// <param name="actualCollectionItem">The actual collection item</param>
        public static void AssertAreEqual(IArtifactBase expectedArtifact, CollectionItem actualCollectionItem)
        {
            ThrowIf.ArgumentNull(expectedArtifact, nameof(expectedArtifact));
            ThrowIf.ArgumentNull(actualCollectionItem, nameof(actualCollectionItem));

            Assert.AreEqual(expectedArtifact.Id, actualCollectionItem.Id);
            Assert.AreEqual(expectedArtifact.Name, actualCollectionItem.Name);
            Assert.AreEqual(expectedArtifact.ArtifactTypeId, actualCollectionItem.ItemTypeId);
            Assert.AreEqual(expectedArtifact.BaseArtifactType.ToItemTypePredefined(), actualCollectionItem.ItemTypePredefined);
        }

        #endregion Custom Asserts

        public enum ImageType
        {
            JPEG,
            PNG,
            GIF,
            TIFF
        }

        #region Image Functions

        private static Dictionary<ImageType, ImageFormat> ImageFormatMap = new Dictionary<ImageType, ImageFormat>
        {
            { ImageType.JPEG, ImageFormat.Jpeg },
            { ImageType.PNG, ImageFormat.Png },
            { ImageType.GIF, ImageFormat.Gif },
            { ImageType.TIFF, ImageFormat.Tiff }
        };

        /// <summary>
        /// Creates a random image and adds it to a property of the specified artifact. Artifact should be locked. It will be saved.
        /// </summary>
        /// <param name="artifact">The artifact where the image will be embedded.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="artifactStore">An ArtifactStore instance.</param>
        /// <param name="width">(optional) The image width.</param>
        /// <param name="height">(optional) The image height.</param>
        /// <param name="imageType">(optional) The image type.</param>
        /// <param name="contentType">(optional) The Content-Type.</param>
        /// <param name="propertyName">(optional) The name of the artifact property where the image should be embedded.</param>
        /// <param name="numberOfImagesToAdd">(optional) The number of images to embed in the property.</param>
        /// <returns>The INovaArtifactDetails after saving the artifact.</returns>
        public static INovaArtifactDetails AddRandomImageToArtifactProperty(IArtifactBase artifact,
            IUser user,
            IArtifactStore artifactStore,
            int width = 100,
            int height = 100,
            ImageType imageType = ImageType.JPEG,
            string contentType = "image/jpeg",
            string propertyName = nameof(NovaArtifactDetails.Description),
            int numberOfImagesToAdd = 1)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(artifactStore, nameof(artifactStore));

            var artifactDetails = artifactStore.GetArtifactDetails(user, artifact.Id);

            return AddRandomImageToArtifactProperty(artifactDetails, user, artifactStore,
                width, height, imageType, contentType, propertyName, numberOfImagesToAdd);
        }

        /// <summary>
        /// Creates a random image and adds it to a property of the specified artifact. Artifact should be locked. It will be saved.
        /// NOTE: This function will first search for a top-level property with the specified name, then if not found it will
        /// look in the CustomPropertyValues and then SpecificPropertyValues.
        /// </summary>
        /// <param name="artifactDetails">The artifact where the image will be embedded.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="artifactStore">An ArtifactStore instance.</param>
        /// <param name="width">(optional) The image width.</param>
        /// <param name="height">(optional) The image height.</param>
        /// <param name="imageType">(optional) The image type.</param>
        /// <param name="contentType">(optional) The Content-Type.</param>
        /// <param name="propertyName">(optional) The name of the artifact property where the image should be embedded.</param>
        /// <param name="numberOfImagesToAdd">(optional) The number of images to embed in the property.</param>
        /// <returns>The INovaArtifactDetails after saving the artifact.</returns>
        public static INovaArtifactDetails AddRandomImageToArtifactProperty(NovaArtifactDetails artifactDetails,
            IUser user,
            IArtifactStore artifactStore,
            int width = 100,
            int height = 100,
            ImageType imageType = ImageType.JPEG,
            string contentType = "image/jpeg",
            string propertyName = nameof(NovaArtifactDetails.Description),
            int numberOfImagesToAdd = 1)
        {
            ThrowIf.ArgumentNull(artifactDetails, nameof(artifactDetails));
            ThrowIf.ArgumentNull(artifactStore, nameof(artifactStore));

            var imageTags = new List<string>();

            for (int i = 0; i < numberOfImagesToAdd; ++i)
            {
                var imageFile = CreateRandomImageFile(width, height, imageType, contentType);
                var addedFile = artifactStore.AddImage(user, imageFile);
                string imageTag = CreateEmbeddedImageHtml(addedFile.EmbeddedImageId);
                imageTags.Add(imageTag);
            }

            string propertyContent = string.Join("<br/>", imageTags);

            return SetArtifactTextProperty(artifactDetails, user, artifactStore, propertyContent, propertyName);
        }

        /// <summary>
        /// Creates a random image file of the specified type and size.
        /// </summary>
        /// <param name="width">The image width.</param>
        /// <param name="height">The image height.</param>
        /// <param name="imageType">The type of image to create (ex. jpeg, png).</param>
        /// <param name="contentType">The MIME Content-Type.</param>
        /// <returns>The random image file.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]  // I want lowercase, not uppercase!
        public static IFile CreateRandomImageFile(int width = 300, int height = 100, ImageType imageType = ImageType.JPEG,
            string contentType = "image/jpeg")
        {
            byte[] imageBytes = ImageUtilities.GenerateRandomImage(width, height, ImageFormatMap[imageType]);
            string randomName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);
            string filename = I18NHelper.FormatInvariant("{0}.{1}", randomName, imageType.ToStringInvariant().ToLowerInvariant());

            return FileFactory.CreateFile(filename, contentType, DateTime.Now, imageBytes);
        }

        /// <summary>
        /// Creates Embedded Image html for the artifact property
        /// </summary>
        /// <param name="imageGUID">Image GUID for embedded image</param>
        /// <returns>Html string</returns>
        public static string CreateEmbeddedImageHtml(string imageGUID)
        {
            Assert.IsNotNullOrEmpty(imageGUID, "Image GUID should not be null or empty!");

            return I18NHelper.FormatInvariant("<p><img src=\"/svc/bpartifactstore/images/{0}\" /></p>", imageGUID);
        }

        #endregion Image Functions

        #region Collection Methods

        /// <summary>
        /// Validate Collection contents by comparing with the expected artifact list
        /// </summary>
        /// <param name="collection">collection returned from get collection call</param>
        /// <param name="artifactList">list of artifact that represents expected artifacts from the returned collection call</param>
        public static void ValidateCollection(NovaCollectionBase collection, List<IArtifactBase> artifactList)
        {
            ThrowIf.ArgumentNull(collection, nameof(collection));
            ThrowIf.ArgumentNull(artifactList, nameof(artifactList));

            Assert.AreEqual(artifactList.Count(), collection.Artifacts.Count(),
                "{0} artifacts are expected from collection but {1} are returned.",
                artifactList.Count(), collection.Artifacts.Count());

            if (artifactList.Any())
            {
                foreach (var artifact in artifactList)
                {
                    var collectionItem = collection.Artifacts.Find(a => a.Id.Equals(artifact.Id));
                    AssertAreEqual(artifact, collectionItem);
                }
            }
        }

        #endregion Collection Methods

        #region NovaArtifact Validations

        /// <summary>
        /// Validate total number of artifact, currently used from get project/artifact children.
        /// </summary>
        /// <param name="novaArtifacts">The list of artifacts returned.</param>
        /// <param name="expectedNumberOfArtifacts">expected number of artifacts.</param>
        public static void ValidateNovaArtifactsCount(List<NovaArtifact> novaArtifacts, int expectedNumberOfArtifacts)
        {
            ThrowIf.ArgumentNull(novaArtifacts, nameof(novaArtifacts));

            Assert.AreEqual(expectedNumberOfArtifacts, novaArtifacts.Count(),
                "expected number of nova artifacts is {0} but {1} is returned.",
                expectedNumberOfArtifacts, novaArtifacts.Count());
        }
        
        /// <summary>
        /// Validate nova artifact contents
        /// </summary>
        /// <param name="project">The project where artifact resides.</param>
        /// <param name="artifact">The nova artifact returned from get project/artifact childen.</param>
        /// <param name="parentArtifactId">parent artifact Id for get project/artifact children.</param>
        public static void ValidateNovaArtifact(IProject project, NovaArtifact artifact, int parentArtifactId)
        {
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            var novaArtifactTypeForArtifact = project.NovaArtifactTypes.Find(nat => ((int)nat.PredefinedType).Equals((int)artifact.PredefinedType));

            Assert.IsNotNull(artifact.HasChildren, "{0} should not be null!", nameof(artifact.HasChildren));

            Assert.IsNotNull(artifact.Id, "{0} should not be null!", nameof(artifact.Id));

            Assert.IsFalse(string.IsNullOrEmpty(artifact.Name), "name should not be empty but it's {0}", artifact.Name);

            switch (artifact.PredefinedType)
            {
                case DEFAULT_COLLECTIONS_ROOT_PREDEFINEDTYPE:
                    Assert.AreEqual(DEFAULT_COLLECTIONS_ROOT_NAME, artifact.Name, "name should be {0} for the Collections default folder.",
                    DEFAULT_COLLECTIONS_ROOT_NAME);

                    Assert.AreEqual(DEFAULT_COLLECTIONS_ROOT_ORDERINDEX, artifact.OrderIndex, "orderIndex should be {0} for the Collections default folder.",
                        DEFAULT_COLLECTIONS_ROOT_ORDERINDEX);

                    Assert.AreEqual(DEFAULT_COLLECTIONS_ROOT_PREFIX, artifact.Prefix, "prefix should be {0} for the Collections default folder.",
                        DEFAULT_COLLECTIONS_ROOT_PREFIX);
                    break;

                case DEFAULT_BASELINES_AND_REVIEWS_ROOT_PREDEFINEDTYPE:
                    Assert.AreEqual(DEFAULT_BASELINES_AND_REVIEWS_ROOT_NAME, artifact.Name, "name should be {0} for the Baselinens default folder.",
                    DEFAULT_BASELINES_AND_REVIEWS_ROOT_NAME);

                    Assert.AreEqual(DEFAULT_BASELINES_AND_REVIEWS_ROOT_ORDERINDEX, artifact.OrderIndex, "orderIndex should be {0} for the Baselinens default folder.",
                        DEFAULT_BASELINES_AND_REVIEWS_ROOT_ORDERINDEX);

                    Assert.AreEqual(DEFAULT_BASELINES_AND_REVIEWS_ROOT_PREFIX, artifact.Prefix, "prefix should be {0} for the Baselinens default folder.",
                        DEFAULT_BASELINES_AND_REVIEWS_ROOT_PREFIX);
                    break;

                default:
                    Assert.AreEqual(novaArtifactTypeForArtifact.Id, artifact.ItemTypeId,
                    "itemTypeId {0} for the artifact {1} doesn't exist on the project {2}",
                    artifact.ItemTypeId, artifact.Name, project.Name);

                    Assert.GreaterOrEqual(artifact.OrderIndex, 0,
                        "orderIndex for the artifact {0} should be greater than or equal to zero but returned {1}.",
                        artifact.Name, artifact.OrderIndex);

                    Assert.AreEqual(novaArtifactTypeForArtifact.Prefix, artifact.Prefix,
                        "prefix {0} for the artifact {1} doesn't exist on the project {2}",
                        artifact.Prefix, artifact.Name, project.Name);
                    break;
            }

            Assert.AreEqual(parentArtifactId, artifact.ParentId, "parentId for the artifact {0} should be {1} but {2} is returned.",
                    artifact.Name, parentArtifactId, artifact.ParentId);

            Assert.IsNotNull(artifact.Permissions, "{0} should not be null!", nameof(artifact.Permissions));

            Assert.AreEqual((int)novaArtifactTypeForArtifact.PredefinedType, (int)artifact.PredefinedType,
                "predefinedType {0} for the artifact {1} doesn't exist on the project {2}",
                artifact.PredefinedType.ToString(), artifact.Name, project.Name);

            Assert.AreEqual(project.Id, artifact.ProjectId, "projectId for the artifact {0} should be {1} but {2} is returned",
                artifact.Name, project.Id, artifact.ProjectId);

            Assert.That(artifact.Version >= 1, "version should be non-zero positive!");
        }

        // TODO: Update content validation portion when more information is available
        /// <summary>
        /// Validate list of nova artifacts' contents
        /// </summary>
        /// <param name="project">The project where artifacts reside.</param>
        /// <param name="novaArtifacts">list of nova artifacts, currently used from get project/artifact childen.</param>
        /// <param name="parentArtifact">(optional) parent artifact.
        /// If null, validation work for the nova artifact list.</param>
        public static void ValidateNovaArtifactsContents(IProject project, List<NovaArtifact> novaArtifacts, IArtifactBase parentArtifact = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(novaArtifacts, nameof(novaArtifacts));

            var parentArtifactId = (parentArtifact == null) ? project.Id : parentArtifact.Id;

            foreach (var artifact in novaArtifacts)
            {
                ValidateNovaArtifact(project, artifact, parentArtifactId);
            }
        }

        /// <summary>
        /// Validate list of nova artifacts, currently used from get project/artifact children.
        /// </summary>
        /// <param name="project">The project where artifacts reside.</param>
        /// <param name="novaArtifacts">list of nova artifacts returned.</param>
        /// <param name="parentArtifact">(optional) parent artifact.
        /// If null, validation work with project Id used as parent Id.</param>
        /// <param name="expectedNumberOfArtifacts">
        /// (optional) expected number of artifacts.
        /// If null, only validate returned artifacts contents</param>
        public static void ValidateNovaArtifacts(
            IProject project,
            List<NovaArtifact> novaArtifacts,
            IArtifactBase parentArtifact = null,
            int expectedNumberOfArtifacts = 0
            )
        {
            ThrowIf.ArgumentNull(project, nameof(project));

            if (expectedNumberOfArtifacts != 0)
            {
                ValidateNovaArtifactsCount(novaArtifacts, expectedNumberOfArtifacts);
            }

            ValidateNovaArtifactsContents(project, novaArtifacts, parentArtifact);
        }

        #endregion NovaArtifact Validations

        /// <summary>
        /// Gets Inline Image Id from html of the artifact rich text property.
        /// </summary>
        /// <returns>Guid string, empty string if no guids were found.</returns>
        public static string GetInlineImageId(string richTextProperty)
        {
            if (richTextProperty == null)
            {
                return string.Empty;
            }

            var guidString = Regex.Match(richTextProperty, @"[0-9a-f]{8}[-]([0-9a-f]{4}[-]){3}[0-9a-f]{12}");
            return guidString.Value;
        }

        /// <summary>
        /// Gets the custom data project.
        /// </summary>
        /// <returns>The custom data project.</returns>
        public static IProject GetCustomDataProject(IUser user)
        {
            var allProjects = ProjectFactory.GetAllProjects(user);

            const string customDataProjectName = TestHelper.GoldenDataProject.CustomData;

            Assert.That(allProjects.Exists(p => (p.Name == customDataProjectName)),
                "No project was found named '{0}'!", customDataProjectName);

            var projectCustomData = allProjects.First(p => (p.Name == customDataProjectName));
            projectCustomData.GetAllOpenApiArtifactTypes(ProjectFactory.Address, user);

            return projectCustomData;
        }

        /// <summary>
        /// Gets all details for all the sub-artifacts passed in.
        /// </summary>
        /// <param name="artifactStore">A reference to an instance of artifact store</param>
        /// <param name="artifact">The artifact to which the sub-artifacts belong.</param>
        /// <param name="subArtifacts">The list of sub-artifacts to get more details for.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <returns>A list of NovaSubArtifacts.</returns>
        public static List<NovaSubArtifact> GetDetailsForAllSubArtifacts(IArtifactStore artifactStore, IArtifactBase artifact, List<SubArtifact> subArtifacts, IUser user)
        {
            ThrowIf.ArgumentNull(artifactStore, nameof(artifactStore));
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(subArtifacts, nameof(subArtifacts));

            var subArtifactDetailsList = new List<NovaSubArtifact>();

            foreach (var subArtifact in subArtifacts)
            {
                var subArtifactDetails = artifactStore.GetSubartifact(user, artifact.Id, subArtifact.Id);
                subArtifactDetailsList.Add(subArtifactDetails);
            }

            return subArtifactDetailsList;
        }

        /// <summary>
        /// Sets the specified custom property of the artifact to the new value.  NOTE: This function doesn't update the artifact on the server, only in memory.
        /// The caller is responsible for locking, saving & publishing the artifact.
        /// </summary>
        /// <typeparam name="T">The new value type.</typeparam>
        /// <param name="artifactDetails">The artifact details containing the custom property to set.</param>
        /// <param name="project">The project where the artifact exists.</param>
        /// <param name="propertyType">The type of property to be set.</param>
        /// <param name="propertyName">The name of the custom property to set.</param>
        /// <param name="newValue">The new value to assign to the custom property.
        ///     For Choice & Text property types, pass a string.
        ///     For Number & Date property types, pass an integer (for Date, it means 'Now + newValue').
        ///     For User property types, pass an IUser.</param>
        /// <returns>The custom property that was set.</returns>
        public static CustomProperty SetArtifactCustomProperty<T>(INovaArtifactDetails artifactDetails,
            IProject project,
            PropertyPrimitiveType propertyType,
            string propertyName,
            T newValue)
        {
            ThrowIf.ArgumentNull(artifactDetails, nameof(artifactDetails));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(newValue, nameof(newValue));

            var customProperties = artifactDetails.CustomPropertyValues;

            return SetCustomProperty(customProperties, project, propertyType, propertyName, newValue);
        }

        /// <summary>
        /// Sets the specified custom property of the subartifact to the new value.  NOTE: This function doesn't update the artifact on the server, only in memory.
        /// The caller is responsible for locking, saving & publishing the artifact.
        /// </summary>
        /// <typeparam name="T">The new value type.</typeparam>
        /// <param name="subArtifactDetails">The subartifact details containing the custom property to set.</param>
        /// <param name="project">The project where the artifact exists.</param>
        /// <param name="propertyType">The type of property to be set.</param>
        /// <param name="propertyName">The name of the custom property to set.</param>
        /// <param name="newValue">The new value to assign to the custom property.
        ///     For Choice & Text property types, pass a string.
        ///     For Number & Date property types, pass an integer (for Date, it means 'Now + newValue').
        ///     For User property types, pass an IUser.</param>
        /// <returns>The custom property that was set.</returns>
        public static CustomProperty SetSubArtifactCustomProperty<T>(NovaItem subArtifactDetails,
            IProject project,
            PropertyPrimitiveType propertyType,
            string propertyName,
            T newValue)
        {
            ThrowIf.ArgumentNull(subArtifactDetails, nameof(subArtifactDetails));
            ThrowIf.ArgumentNull(project, nameof(project));

            var customProperties = subArtifactDetails.CustomPropertyValues;

            return SetCustomProperty(customProperties, project, propertyType, propertyName, newValue);
        }

        /// <summary>
        /// Sets the specified custom property to the new value.  NOTE: This function doesn't update the artifact on the server, only in memory.
        /// The caller is responsible for locking, saving & publishing the artifact.
        /// </summary>
        /// <typeparam name="T">The new value type.</typeparam>
        /// <param name="customProperties">The list of custom properties to set.</param>
        /// <param name="project">The project where the artifact exists.</param>
        /// <param name="propertyType">The type of property to be set.</param>
        /// <param name="propertyName">The name of the custom property to set.</param>
        /// <param name="newValue">The new value to assign to the custom property.
        ///     For Choice & Text property types, pass a string.
        ///     For Number & Date property types, pass an integer (for Date, it means 'Now + newValue').
        ///     For User property types, pass an IUser.</param>
        /// <returns>The custom property that was set.</returns>
        public static CustomProperty SetCustomProperty<T>(List<CustomProperty> customProperties,
            IProject project,
            PropertyPrimitiveType propertyType,
            string propertyName,
            T newValue)
        {
            ThrowIf.ArgumentNull(customProperties, nameof(customProperties));
            ThrowIf.ArgumentNull(project, nameof(project));

            var property = customProperties.Find(p => p.Name == propertyName);

            Assert.IsNotNull(property, "Property does not exist!");

            switch (propertyType)
            {
                case PropertyPrimitiveType.Choice:
                    var novaPropertyType = project.NovaPropertyTypes.Find(pt => pt.Name.EqualsOrdinalIgnoreCase(propertyName));
                    var choicePropertyValidValues = novaPropertyType.ValidValues;

                    string[] values;

                    if (newValue.GetType().IsArray)
                    {
                        values = ((IEnumerable) newValue)
                            .Cast<object>()
                            .Select(x => x.ToString())
                            .ToArray();
                    }
                    else
                    {
                        values = new[] { newValue.ToString() };
                    }

                    var validValues = new List<NovaPropertyType.ValidValue>();
                    var customValue = string.Empty;

                    foreach (string value in values)
                    {
                        var newPropertyValue = choicePropertyValidValues.Find(vv => vv.Value.Equals(value));

                        // Change custom property choice value
                        if (newPropertyValue != null)
                        {
                            validValues.Add(newPropertyValue);
                        }
                        else
                        {
                            // Add as custom value if not found in valid values
                            customValue = newValue.ToString();
                        }
                    }

                    if (validValues.Count > 0)
                    {
                        property.CustomPropertyValue = new ChoiceValues { ValidValues = validValues };
                    }

                    if (!string.IsNullOrEmpty(customValue))
                    {
                        property.CustomPropertyValue = new ChoiceValues { CustomValue = customValue };
                    }
                    break;
                case PropertyPrimitiveType.Date:
                case PropertyPrimitiveType.Number:
                    property.CustomPropertyValue = newValue;
                    break;
                case PropertyPrimitiveType.Text:
                    // Change custom property text value
                    property.CustomPropertyValue = StringUtilities.WrapInHTML(WebUtility.HtmlEncode(newValue.ToString()));
                    break;
                case PropertyPrimitiveType.User:
                    var user = (IUser)newValue;

                    var newIdentification = new Identification { DisplayName = user.DisplayName, Id = user.Id };
                    var newUserPropertyValue = new List<Identification> { newIdentification };

                    // Change custom property user value
                    property.CustomPropertyValue = new UserGroupValues { UsersGroups = newUserPropertyValue };
                    break;
                default:
                    Assert.Fail("Unsupported PropertyPrimitiveType '{0}' was passed to this test!", propertyType);
                    break;
            }

            return property;
        }

        /// <summary>
        /// Sets the specified custom property to null.  NOTE: This function doesn't update the artifact on the server, only in memory.
        /// The caller is responsible for locking, saving & publishing the artifact.
        /// </summary>
        /// <param name="customProperties">The list of custom properties.</param>
        /// <param name="propertyName">The name of the custom property to set.</param>
        /// <returns>The custom property that was set.</returns>
        public static CustomProperty SetCustomPropertyToNull(List<CustomProperty> customProperties, string propertyName)
        {
            ThrowIf.ArgumentNull(customProperties, nameof(customProperties));

            var property = customProperties.Find(p => p.Name == propertyName);

            Assert.IsNotNull(property, "Property does not exist!");

            if (property.PrimitiveType == (int) PropertyPrimitiveType.User)
            {
                property.CustomPropertyValue = new UserGroupValues { UsersGroups = null };
            }
            else
            {
                property.CustomPropertyValue = null;
            }

            return property;
        }

        /// <summary>
        /// Update an artifact custom property and save.
        /// </summary>
        /// <typeparam name="T">The property value type</typeparam>
        /// <param name="artifact">The artifact to update</param>
        /// <param name="user">The user updating the artifact</param>
        /// <param name="project">The project where the artifact exists</param>
        /// <param name="propertyType">The primitive property type of the property</param>
        /// <param name="propertyName">The name of the artifact property to be updated</param>
        /// <param name="propertyValue">The new value for the subartifact property</param>
        /// <param name="artifactStore">A reference to an instance of artifact store</param>
        /// <returns>The updated property</returns>
        public static CustomProperty UpdateArtifactCustomProperty<T>(IArtifact artifact, IUser user, IProject project,
            PropertyPrimitiveType propertyType, string propertyName, T propertyValue, IArtifactStore artifactStore)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(artifactStore, nameof(artifactStore));

            var artifactDetails = artifactStore.GetArtifactDetails(user, artifact.Id);

            // Set custom property in artifact.
            var property = SetArtifactCustomProperty(artifactDetails, project,
                propertyType, propertyName, propertyValue);

            var artifactDetailsChangeset = TestHelper.CreateArtifactChangeSet(artifactDetails, customProperty: property);

            artifact.Lock(user);
            artifactStore.UpdateArtifact(user, (NovaArtifactDetails) artifactDetailsChangeset);

            return property;
        }

        /// <summary>
        /// Update a subartifact custom property and save
        /// </summary>
        /// <typeparam name="T">The property value type</typeparam>
        /// <param name="artifact">The artifact to update</param>
        /// <param name="subArtifact">The subartifact to update</param>
        /// <param name="user">The user updating the subartifact</param>
        /// <param name="project">The project where the artifact exists</param>
        /// <param name="propertyType">The primitive type of the property</param>
        /// <param name="propertyName">The name of the subartifact property to be updated</param>
        /// <param name="propertyValue">The new value for the subartifact property</param>
        /// <param name="artifactStore">A reference to an instance of artifact store</param>
        /// <returns>The updated property</returns>
        public static CustomProperty UpdateSubArtifactCustomProperty<T>(IArtifact artifact, NovaSubArtifact subArtifact, IUser user, IProject project,
            PropertyPrimitiveType propertyType, string propertyName, T propertyValue, IArtifactStore artifactStore)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(subArtifact, nameof(subArtifact));
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(artifactStore, nameof(artifactStore));

            var artifactDetails = artifactStore.GetArtifactDetails(user, artifact.Id);

            // Set custom property in subartifact.
            var property = SetSubArtifactCustomProperty(subArtifact, project,
                propertyType, propertyName, propertyValue);

            var subArtifactChangeSet = TestHelper.CreateSubArtifactChangeSet(subArtifact, customProperty: property);
            var artifactDetailsChangeset = TestHelper.CreateArtifactChangeSet(artifactDetails, subArtifact: subArtifactChangeSet);

            artifact.Lock(user);
            artifactStore.UpdateArtifact(user, (NovaArtifactDetails)artifactDetailsChangeset);

            return property;
        }

        /// <summary>
        /// Try to update an invalid Artifact with Property Changes.  Use this for testing cases where the save is expected to fail.
        /// </summary>
        /// <param name="address">The base address used for the REST call.</param>
        /// <param name="requestBody">The request body (i.e. artifact to be updated).</param>
        /// <param name="artifactId">The ID of the artifact to save.</param>
        /// <param name="user">The user updating the artifact.</param>
        /// <returns>The body content returned from ArtifactStore.</returns>
        public static string UpdateInvalidArtifact(string address, string requestBody,
            int artifactId, IUser user)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.AccessControlToken;

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.ARTIFACTS_id_, artifactId);
            RestApiFacade restApi = new RestApiFacade(address, tokenValue);
            const string contentType = "application/json";

            var response = restApi.SendRequestBodyAndGetResponse(
                path,
                RestRequestMethod.PATCH,
                requestBody,
                contentType);

            return response.Content;
        }

        /// <summary>
        /// Attaches files to the artifact (Save changes).
        /// </summary>
        /// <param name="user">User to perform an operation.</param>
        /// <param name="artifact">Artifact.</param>
        /// <param name="files">List of files to attach.</param>
        /// <param name="artifactStore">IArtifactStore.</param>
        /// <param name="shouldLockArtifact">(optional) Pass false if you already locked the artifact.
        ///     By default this function will lock the artifact.</param>
        /// <param name="expectedLockResult">(optional) The expected LockResult returned in the JSON body.  This is only checked if StatusCode = 200.
        ///     If null, only Success is expected.</param>
        /// <returns>The attachments that were added.</returns>
        public static Attachments AddArtifactAttachmentsAndSave(
            IUser user,
            IArtifact artifact,
            List<INovaFile> files,
            IArtifactStore artifactStore,
            bool shouldLockArtifact = true,
            LockResult expectedLockResult = LockResult.Success)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(files, nameof(files));
            ThrowIf.ArgumentNull(artifactStore, nameof(artifactStore));

            if (shouldLockArtifact)
            {
                artifact.Lock(user, expectedLockResult: expectedLockResult);
            }

            var artifactDetails = artifactStore.GetArtifactDetails(user, artifact.Id);

            foreach (var file in files)
            {
                artifactDetails.AttachmentValues.Add(new AttachmentValue(user, file));
            }

            Artifact.UpdateArtifact(artifact, user, artifactDetails, address: artifactStore.Address);
            var attachments = artifactStore.GetAttachments(artifact, user);
            Assert.IsTrue(attachments.AttachedFiles.Count >= files.Count, "All attachments should be added.");

            return attachments;
        }

        /// <summary>
        /// Attaches file to the artifact (Save changes).
        /// </summary>
        /// <param name="user">User to perform an operation.</param>
        /// <param name="artifact">Artifact.</param>
        /// <param name="file">The file to attach.</param>
        /// <param name="artifactStore">IArtifactStore.</param>
        /// <param name="shouldLockArtifact">(optional) Pass false if you already locked the artifact.
        ///     By default this function will lock the artifact.</param>
        /// <param name="expectedAttachedFilesCount">(optional) The expected number of attached files after adding the attachment.</param>
        /// <param name="expectedLockResult">(optional) The expected LockResult returned in the JSON body.  This is only checked if StatusCode = 200.
        ///     If null, only Success is expected.</param>
        /// <returns>The attachments that were added.</returns>
        public static Attachments AddArtifactAttachmentAndSave(
            IUser user,
            IArtifact artifact,
            INovaFile file,
            IArtifactStore artifactStore,
            bool shouldLockArtifact = true,
            int expectedAttachedFilesCount = 1,
            LockResult expectedLockResult = LockResult.Success)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(file, nameof(file));
            ThrowIf.ArgumentNull(artifactStore, nameof(artifactStore));

            var attachments = AddArtifactAttachmentsAndSave(user, artifact, new List<INovaFile> { file }, artifactStore, shouldLockArtifact, expectedLockResult);
            Assert.AreEqual(expectedAttachedFilesCount, attachments.AttachedFiles.Count, "The attachment should be added.");

            return attachments;
        }

        /// <summary>
        /// Attaches file to the subartifact (Save changes).
        /// </summary>
        /// <param name="user">User to perform an operation.</param>
        /// <param name="artifact">Artifact.</param>
        /// <param name="subArtifact">SubArtifact.</param>
        /// <param name="files">List of files to attach.</param>
        /// <param name="artifactStore">IArtifactStore.</param>
        public static void AddSubArtifactAttachmentAndSave(IUser user, IArtifact artifact, NovaItem subArtifact,
            List<INovaFile> files, IArtifactStore artifactStore)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(subArtifact, nameof(subArtifact));
            ThrowIf.ArgumentNull(files, nameof(files));
            ThrowIf.ArgumentNull(artifactStore, nameof(artifactStore));
            Assert.AreEqual(artifact.Id, subArtifact.ParentId, "subArtifact should belong to Artifact");

            artifact.Lock(user);
            var artifactDetails = artifactStore.GetArtifactDetails(user, artifact.Id);

            var subArtifactToAdd = new NovaSubArtifact();
            subArtifactToAdd.Id = subArtifact.Id;

            foreach (var file in files)
            {
                subArtifactToAdd.AttachmentValues.Add(new AttachmentValue(user, file));
            }

            var subArtifacts = new List<NovaSubArtifact> { subArtifactToAdd };

            artifactDetails.SubArtifacts = subArtifacts;

            Artifact.UpdateArtifact(artifact, user, artifactDetails, address: artifactStore.Address);
            var attachment = artifactStore.GetAttachments(artifact, user, subArtifactId: subArtifact.Id);
            Assert.IsTrue(attachment.AttachedFiles.Count >= files.Count, "All attachments should be added.");
        }

        /// <summary>
        /// Deletes file from the artifact (Save changes).
        /// </summary>
        /// <param name="user">User to perform an operation.</param>
        /// <param name="artifact">Artifact.</param>
        /// <param name="fileId">Id of the file to delete. File must be attached to the artifact.</param>
        /// <param name="artifactStore">IArtifactStore.</param>
        public static void DeleteArtifactAttachmentAndSave(IUser user, IArtifact artifact, int fileId, IArtifactStore artifactStore)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(artifactStore, nameof(artifactStore));

            var attachments = artifactStore.GetAttachments(artifact, user);
            Assert.IsNotNull(attachments, "Getattachments shouldn't return null.");
            Assert.IsTrue(attachments.AttachedFiles.Count > 0, "Artifact should have at least one attachment.");

            var fileToDelete = attachments.AttachedFiles.FirstOrDefault(f => f.AttachmentId == fileId);
            Assert.NotNull(fileToDelete, "Couldn't find an Attachment with ID: '{0}'.", fileId);
            Assert.AreEqual(fileId, fileToDelete.AttachmentId, "Attachments must contain file with fileId.");

            artifact.Lock(user);

            var artifactDetails = artifactStore.GetArtifactDetails(user, artifact.Id);
            artifactDetails.AttachmentValues.Add(new AttachmentValue(fileToDelete.AttachmentId));

            Artifact.UpdateArtifact(artifact, user, artifactDetails, address: artifactStore.Address);
        }

        /// <summary>
        /// Deletes file from the SubArtifact (Save changes).
        /// </summary>
        /// <param name="user">User to perform an operation.</param>
        /// <param name="artifact">Artifact.</param>
        /// <param name="subArtifact">SubArtifact.</param>
        /// <param name="fileId">Id of the file to delete. File must be attached to the artifact.</param>
        /// <param name="artifactStore">IArtifactStore.</param>
        public static void DeleteSubArtifactAttachmentAndSave(IUser user, IArtifact artifact, NovaItem subArtifact,
            int fileId, IArtifactStore artifactStore)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(subArtifact, nameof(subArtifact));
            ThrowIf.ArgumentNull(artifactStore, nameof(artifactStore));
            Assert.AreEqual(artifact.Id, subArtifact.ParentId, "Subartifact should belong to artifact.");

            var attachment = artifactStore.GetAttachments(artifact, user, subArtifactId: subArtifact.Id);
            Assert.IsNotNull(attachment, "Getattachments shouldn't return null.");
            Assert.IsTrue(attachment.AttachedFiles.Count > 0, "Artifact should have at least one attachment.");

            var fileToDelete = attachment.AttachedFiles.FirstOrDefault(f => f.AttachmentId == fileId);
            Assert.NotNull(fileToDelete, "Couldn't find an Attachment with ID: '{0}'.", fileId);
            Assert.AreEqual(fileId, fileToDelete.AttachmentId, "Attachments must contain file with fileId.");

            artifact.Lock(user);

            var artifactDetails = artifactStore.GetArtifactDetails(user, artifact.Id);
            artifactDetails.AttachmentValues.Add(new AttachmentValue(fileToDelete.AttachmentId));

            var subArtifactToAdd = new NovaSubArtifact();
            subArtifactToAdd.Id = subArtifact.Id;
            subArtifactToAdd.AttachmentValues.Add(new AttachmentValue(fileToDelete.AttachmentId));

            var subArtifacts = new List<NovaSubArtifact> { subArtifactToAdd };

            artifactDetails.SubArtifacts = subArtifacts;
            Artifact.UpdateArtifact(artifact, user, artifactDetails, address: artifactStore.Address);
        }

        /// <summary>
        /// Creates a new INovaArtifactDetails with the minimal properties required for updating an artifact.
        /// </summary>
        /// <param name="artifact">The artifact which contains properties that NovaArtiactDetails refers to.</param>
        /// <returns>The INovaArtifactDetails with the minimal required properties for update.</returns>
        public static INovaArtifactDetails CreateNovaArtifactDetailsForUpdate(INovaArtifactBase artifact)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            var novaArtifactDetails = new NovaArtifactDetails
            {
                Id = artifact.Id,
                ProjectId = artifact.ProjectId
            };
            return novaArtifactDetails;
        }

        /// <summary>
        /// Creates inline trace text for the provided artifact. For use with RTF properties.
        /// </summary>
        /// <param name="inlineTraceArtifact">The target artifact for inline traces.</param>
        /// <param name="blueprintServerAddress">The base address of the Blueprint server.</param>
        /// <returns>The inline trace text.</returns>
        public static string CreateArtifactInlineTraceValue(
            ArtifactWrapper inlineTraceArtifact,
            string blueprintServerAddress)
        {
            ThrowIf.ArgumentNull(inlineTraceArtifact, nameof(inlineTraceArtifact));

            string inlineTraceText = I18NHelper.FormatInvariant(
                "<html><head></head><body style=\"padding: 1px 0px 0px; font-family: 'Portable User Interface'; font-size: 10.67px\">" +
                "<div style=\"padding: 0px\"><p style=\"margin: 0px\">&#x200b;<a linkassemblyqualifiedname=\"BluePrintSys.RC.Client.SL.RichText.RichTextArtifactLink, " +
                "BluePrintSys.RC.Client.SL.RichText, Version=7.4.0.0, Culture=neutral, PublicKeyToken=null\" canclick=\"True\" isvalid=\"True\" href=\"{0}?ArtifactId={1}\" " +
                "target=\"_blank\" artifactid=\"{1}\" style=\"font-family: 'Portable User Interface'; font-size: 11px; font-style: normal; font-weight: normal; " +
                "text-decoration: underline; color: #0000FF\" title=\"Project: {4}\"><span style=\"font-family: 'Portable User Interface'; font-size: 11px; " +
                "font-style: normal; font-weight: normal; text-decoration: underline; color: #0000FF\">{2}{1}: {3}</span></a><span " +
                "style=\"-c1-editable: true; font-family: 'Portable User Interface'; font-size: 10.67px; font-style: normal; font-weight: normal; color: Black\">" +
                "&#x200b;</span></p></div></body></html>",
                blueprintServerAddress, inlineTraceArtifact.Id, inlineTraceArtifact.Prefix, inlineTraceArtifact.Name, inlineTraceArtifact.ArtifactState.Project.Name);

            return inlineTraceText;
        }

        /// <summary>
        /// Creates and saves (or publishes) a new artifact and attaches the specified file to it.  The attachment is not published.
        /// </summary>
        /// <param name="helper">A TestHelper instance.</param>
        /// <param name="project">The project where the artifact will be created.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="artifactType">The type of artifact to create.</param>
        /// <param name="file">The file to attach.</param>
        /// <param name="shouldPublishArtifact">(optional) Pass true to publish the artifact before adding the attachment.  Default is no publish.</param>
        /// <returns>The new artifact.</returns>
        public static IArtifact CreateArtifactWithAttachment(TestHelper helper,
            IProject project,
            IUser user,
            BaseArtifactType artifactType,
            IFileMetadata file,
            bool shouldPublishArtifact = false)
        {
            ThrowIf.ArgumentNull(helper, nameof(helper));
            ThrowIf.ArgumentNull(file, nameof(file));

            var artifact = helper.CreateAndSaveArtifact(project, user, artifactType);

            if (shouldPublishArtifact)
            {
                artifact.Publish();
            }

            // Create & add attachment to the artifact.
            DateTime defaultExpireTime = DateTime.Now.AddDays(2);   // Currently Nova set ExpireTime 2 days from today for newly uploaded file.

            var novaAttachmentFile = FileStoreTestHelper.UploadNovaFileToFileStore(user, file.FileName, file.FileType,
                defaultExpireTime, helper.FileStore);

            AddArtifactAttachmentAndSave(user, artifact, novaAttachmentFile, helper.ArtifactStore);

            return artifact;
        }

        /// <summary>
        /// Checks if the inline trace link is valid or not.
        /// </summary>
        /// <param name="inlineTraceLink">The inline trace link to validate</param>
        /// <returns> True if the inline trace link is a valid inline trace link, otherwise returns false.</returns>
        private static bool IsValidInlineTrace(string inlineTraceLink)
        {
            const string validTag = "isValid=\"True\"";

            return inlineTraceLink.ToUpper(CultureInfo.InvariantCulture).Contains(validTag.ToUpper(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Validates that the NovaTrace from the source artifact has the correct properties to point to the target artifact.
        /// </summary>
        /// <param name="sourceArtifactTrace">The Nova trace obtained from the source artifact.</param>
        /// <param name="targetArtifact">The target artifact of the trace.</param>
        /// <exception cref="AssertionException">If any properties of the trace don't match the target artifact.</exception>
        public static void ValidateTrace(INovaTrace sourceArtifactTrace, IArtifactBase targetArtifact)
        {
            ThrowIf.ArgumentNull(sourceArtifactTrace, nameof(sourceArtifactTrace));
            ThrowIf.ArgumentNull(targetArtifact, nameof(targetArtifact));

            Assert.AreEqual(sourceArtifactTrace.ArtifactId, targetArtifact.Id, "Id from trace and artifact should be equal to each other.");
            Assert.AreEqual(sourceArtifactTrace.ArtifactName, targetArtifact.Name, "Name from trace and artifact should be equal to each other.");
            Assert.AreEqual(sourceArtifactTrace.ItemId, targetArtifact.Id, "itemId from trace and artifact should be equal to each other.");
            Assert.AreEqual(sourceArtifactTrace.ProjectId, targetArtifact.ProjectId, "ProjectId from trace and artifact should be equal to each other.");
            Assert.AreEqual(sourceArtifactTrace.ProjectName, targetArtifact.Project.Name, "ProjectName from trace and artifact should be equal to each other.");
        }

        /// <summary>
        /// Validates inline trace link returned from artifact details
        /// </summary>
        /// <param name="artifactDetails">The artifact details containing the inline trace link which needs validation</param>
        /// <param name="inlineTraceArtifact">The artifact contained within the inline trace link</param>
        /// <param name="validInlineTraceLink">A flag indicating whether the inline trace link is expected to be valid or not</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static void ValidateInlineTraceLinkFromArtifactDetails(
            INovaArtifactDetails artifactDetails,
            INovaArtifactDetails inlineTraceArtifact,
            bool validInlineTraceLink)
        {
            ThrowIf.ArgumentNull(artifactDetails, nameof(artifactDetails));
            ThrowIf.ArgumentNull(inlineTraceArtifact, nameof(inlineTraceArtifact));

            // Validation: Verify that the artifactDetails' description field which contain inline trace link contains the valid
            // inline trace information (name of the inline trace artifact).
            Assert.That(artifactDetails.Description.Contains(inlineTraceArtifact.Name),
                "Expected outcome should not contains {0} on returned artifactdetails. Returned inline trace content is {1}.",
                inlineTraceArtifact.Name,
                artifactDetails.Description);

            Assert.AreEqual(validInlineTraceLink, IsValidInlineTrace(artifactDetails.Description),
                "Expected {0} for valid inline trace but {1} was returned. The returned inlinetrace link is {2}.",
                validInlineTraceLink,
                !validInlineTraceLink,
                artifactDetails.Description);
        }

        /// <summary>
        /// Validates inline trace link returned from subartifact
        /// </summary>
        /// <param name="subArtifact">The subartifact containing the inline trace link which needs validation</param>
        /// <param name="inlineTraceArtifact">The artifact contained within the inline trace link</param>
        /// <param name="validInlineTraceLink">A flag indicating whether the inline trace link is expected to be valid or not</param>
        public static void ValidateInlineTraceLinkFromSubArtifactDetails(
            NovaItem subArtifact,
            ArtifactWrapper inlineTraceArtifact,
            bool validInlineTraceLink)
        {
            ThrowIf.ArgumentNull(subArtifact, nameof(subArtifact));
            ThrowIf.ArgumentNull(inlineTraceArtifact, nameof(inlineTraceArtifact));

            // Validation: Verify that the subArtifactDetails' description field which contain inline trace link contains the valid
            // inline trace information (name of the inline trace artifact).
            Assert.That(subArtifact.Description.Contains(inlineTraceArtifact.Name),
                "Expected outcome does not contain {0} on returned artifactdetails. Returned inline trace content is {1}.",
                inlineTraceArtifact.Name,
                subArtifact.Description);

            Assert.AreEqual(validInlineTraceLink, IsValidInlineTrace(subArtifact.Description),
                "Expected {0} for valid inline trace but {1} was returned. The returned inlinetrace link is {2}.",
                validInlineTraceLink,
                !validInlineTraceLink,
                subArtifact.Description);
        }

        /// <summary>
        /// Verifies that the content returned in the rest response contains the specified ErrorCode and Message.
        /// </summary>
        /// <param name="restResponse">The RestResponse that was returned.</param>
        /// <param name="expectedErrorCode">The expected error code.</param>
        /// <param name="expectedErrorMessage">The expected error message.</param>
        public static void ValidateServiceError(RestResponse restResponse, int expectedErrorCode, string expectedErrorMessage)
        {
            IServiceErrorMessage serviceError = null;

            Assert.DoesNotThrow(() =>
            {
                serviceError = JsonConvert.DeserializeObject<ServiceErrorMessage>(restResponse.Content);
            }, "Failed to deserialize the content of the REST response into a ServiceErrorMessage object!");

            var expectedError = ServiceErrorMessageFactory.CreateServiceErrorMessage(
                expectedErrorCode,
                expectedErrorMessage);

            serviceError.AssertEquals(expectedError);
        }

        /// <summary>
        /// Verifies that the content returned in the rest response contains the specified Message.
        /// </summary>
        /// <param name="restResponse">The RestResponse that was returned.</param>
        /// <param name="expectedErrorMessage">The expected error message.</param>
        public static void ValidateServiceError(RestResponse restResponse, string expectedErrorMessage)
        {
            string errorMessage = null;

            Assert.DoesNotThrow(() =>
            {
                errorMessage = JsonConvert.DeserializeObject<string>(restResponse.Content);
            }, "Failed to deserialize the content of the REST response into a string!");

            Assert.AreEqual(expectedErrorMessage, errorMessage, "The error message received doesn't match what we expected!");
        }

        /// <summary>
        /// Creates new rich text that includes inline trace(s).
        /// </summary>
        /// <param name="artifacts">The artifacts being added as inline trace(s).</param>
        /// <param name="blueprintBaseAddress">The base address of the Blueprint server.</param>
        /// <returns>A formatted rich text string with inline traces(s).</returns>
        public static string CreateTextForProcessInlineTrace(IList<ArtifactWrapper> artifacts, string blueprintBaseAddress)
        {
            ThrowIf.ArgumentNull(artifacts, nameof(artifacts));

            var text = string.Empty;

            foreach (var artifact in artifacts)
            {
                text = text + I18NHelper.FormatInvariant("<a href=\"{0}/?/ArtifactId={1}\" target=\"\" artifactid=\"{1}\"" +
                    " linkassemblyqualifiedname=\"BluePrintSys.RC.Client.SL.RichText.RichTextArtifactLink, BluePrintSys.RC.Client.SL.RichText, " +
                    "Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\" canclick=\"True\" isvalid=\"True\" title=\"Project: {3}\">" +
                    "<span style=\"text-decoration: underline; color: #0000ff\">{4}{1}: {2}</span></a>",
                    blueprintBaseAddress, artifact.Id, artifact.Name, artifact.ArtifactState.Project.Name, artifact.Prefix);
            }

            Assert.IsFalse(string.IsNullOrWhiteSpace(text), "Text for inline trace was null or whitespace!");

            return I18NHelper.FormatInvariant(StringUtilities.WrapInHTML("<p>" + text + "</p>"));
        }

        /// <summary>
        /// Adds trace to the artifact (and saves changes).
        /// </summary>
        /// <param name="user">User to perform an operation.</param>
        /// <param name="artifact">Artifact to add trace.</param>
        /// <param name="traceTarget">Trace's target.</param>
        /// <param name="traceDirection">Trace direction.</param>
        /// <param name="changeType">ChangeType enum - Add, Update or Delete trace</param>
        /// <param name="artifactStore">IArtifactStore.</param>
        /// <param name="isSuspect">(optional)isSuspect, true for suspect trace, false otherwise.</param>
        /// <param name="targetSubArtifact">(optional)subArtifact for trace target(creates trace with subartifact).</param>
        public static void UpdateManualArtifactTraceAndSave(IUser user,
            IArtifact artifact,
            IArtifactBase traceTarget,
            ChangeType changeType,
            IArtifactStore artifactStore,
            TraceDirection traceDirection = TraceDirection.From,
            bool? isSuspect = null,
            NovaItem targetSubArtifact = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(traceTarget, nameof(traceTarget));
            ThrowIf.ArgumentNull(traceDirection, nameof(traceDirection));
            ThrowIf.ArgumentNull(artifactStore, nameof(artifactStore));

            artifact.Lock(user);
            var artifactDetails = artifactStore.GetArtifactDetails(user, artifact.Id);

            var traceToCreate = new NovaTrace();
            traceToCreate.ArtifactId = traceTarget.Id;
            traceToCreate.ProjectId = traceTarget.ProjectId;
            traceToCreate.Direction = traceDirection;
            traceToCreate.TraceType = TraceType.Manual;
            traceToCreate.ItemId = targetSubArtifact?.Id ?? traceTarget.Id;
            traceToCreate.ChangeType = changeType;
            traceToCreate.IsSuspect = isSuspect ?? false;

            var updatedTraces = new List<NovaTrace> { traceToCreate };

            artifactDetails.Traces = updatedTraces;

            Artifact.UpdateArtifact(artifact, user, artifactDetails, address: artifactStore.Address);
            // TODO: add assertions about changed traces
        }

        //TODO: Refactor and add to ItemTypePredefinedExtensions.ca
        /// <summary>
        /// Gets the Standard Pack Artifact Type that matches the given ItemTypePredefined
        /// </summary>
        /// <param name="itemType">The Nova base ItemType to create.</param>
        /// <returns>A string indicating the name of the Standard Pack artifact name for the predefined item type.</returns>
        public static string GetStandardPackArtifactTypeName(ItemTypePredefined itemType)
        {
            ThrowIf.ArgumentNull(itemType, nameof(itemType));

            return GetArtifactTypeName(itemType, "(Standard Pack)");
        }

        /// <summary>
        /// Gets the Custom Artifact Type that matches the given ItemTypePredefined
        /// </summary>
        /// <param name="itemType">The Nova base ItemType to create.</param>
        /// <returns>A string indicating the name of the Custom artifact name for the predefined item type.</returns>
        public static string GetCustomArtifactTypeName(ItemTypePredefined itemType)
        {
            ThrowIf.ArgumentNull(itemType, nameof(itemType));

            return GetArtifactTypeName(itemType, "(Custom Test)");
        }

        /// <summary>
        /// Gets the Artifact Type that matches the given ItemTypePredefined with artifact type suffix
        /// </summary>
        /// <param name="itemType">The Nova base ItemType to create.</param>
        /// <param name="artifactTypeSuffix">The suffix to add the the artifcat type name</param>
        /// <returns>A string indicating the artifact type name for the predefined item type.</returns>
        private static string GetArtifactTypeName(ItemTypePredefined itemType, string artifactTypeSuffix)
        {
            ThrowIf.ArgumentNull(itemType, nameof(itemType));

            string artifactTypeNameBase;

            switch (itemType)
            {
                case ItemTypePredefined.Actor:
                    artifactTypeNameBase = "Actor";
                    break;

                case ItemTypePredefined.BusinessProcess:
                    artifactTypeNameBase = "Business Process";
                    break;

                case ItemTypePredefined.Document:
                    artifactTypeNameBase = "Document";
                    break;

                case ItemTypePredefined.DomainDiagram:
                    artifactTypeNameBase = "Domain Diagram";
                    break;

                case ItemTypePredefined.GenericDiagram:
                    artifactTypeNameBase = "Generic Diagram";
                    break;

                case ItemTypePredefined.Glossary:
                    artifactTypeNameBase = "Glossary";
                    break;

                case ItemTypePredefined.PrimitiveFolder:
                    artifactTypeNameBase = "Folder";
                    break;

                case ItemTypePredefined.Process:
                    artifactTypeNameBase = "Process";
                    break;

                case ItemTypePredefined.Storyboard:
                    artifactTypeNameBase = "Storyboard";
                    break;

                case ItemTypePredefined.TextualRequirement:
                    artifactTypeNameBase = "Textual Requirement";
                    break;

                case ItemTypePredefined.UIMockup:
                    artifactTypeNameBase = "UI Mockup";
                    break;

                case ItemTypePredefined.UseCase:
                    artifactTypeNameBase = "Use Case";
                    break;

                case ItemTypePredefined.UseCaseDiagram:
                    artifactTypeNameBase = "Use Case Diagram";
                    break;

                default:
                    artifactTypeNameBase = "";
                    break;
            }

            return I18NHelper.FormatInvariant("{0}{1}", artifactTypeNameBase, artifactTypeSuffix);
        }

        /// <summary>
        /// Updates text property of the specified artifact. Artifact should be locked by the user. It will be saved.
        /// NOTE: This function will first search for a top-level property with the specified name, then if not found it will
        /// look in the CustomPropertyValues.
        /// </summary>
        /// <param name="artifactDetails">The artifact where the image will be embedded.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="artifactStore">An ArtifactStore instance.</param>
        /// <param name="text">The new text that will be set in the property.</param>
        /// <param name="propertyName">(optional) The name of the artifact property to update. By default it is Description.</param>
        /// <returns>The INovaArtifactDetails after saving the artifact.</returns>
        public static INovaArtifactDetails SetArtifactTextProperty(NovaArtifactDetails artifactDetails,
            IUser user,
            IArtifactStore artifactStore,
            string text,
            string propertyName = nameof(NovaArtifactDetails.Description))
        {
            ThrowIf.ArgumentNull(artifactDetails, nameof(artifactDetails));
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactStore, nameof(artifactStore));
            
            if (artifactDetails.GetType().GetProperty(propertyName) != null)
            {
                CSharpUtilities.SetProperty(propertyName, text, artifactDetails);
            }
            else if (artifactDetails.CustomPropertyValues.Exists(p => p.Name == propertyName))
            {
                var property = artifactDetails.CustomPropertyValues.Find(p => p.Name == propertyName);
                property.CustomPropertyValue = text;
            }
            else
            {
                Assert.Fail("No property named '{0}' was found in artifact ID: {1}!", propertyName, artifactDetails.Id);
            }

            artifactStore.UpdateArtifact(user, artifactDetails);
            return artifactStore.GetArtifactDetails(user, artifactDetails.Id);
        }

        public class ChoiceValues
        {
            [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
            [JsonProperty("validValues")]
            public List<NovaPropertyType.ValidValue> ValidValues { get; set; }

            [JsonProperty("customValue")]
            public string CustomValue { get; set; }
        }

        public class UserGroupValues
        {
            [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
            [JsonProperty("usersGroups")]
            public List<Identification> UsersGroups { get; set; }

            [JsonProperty("label")]
            public string Label { get; set; }
        }
    }

    public static class CustomPropertyName
    {
        public const string TextRequiredRTMultiHasDefault = "Std-Text-Required-RT-Multi-HasDefault";
        public const string NumberRequiredValidatedDecPlacesMinMaxHasDefault = "Std-Number-Required-Validated-DecPlaces-Min-Max-HasDefault";
        public const string DateRequiredValidatedMinMaxHasDefault = "Std-Date-Required-Validated-Min-Max-HasDefault";
        public const string ChoiceRequiredAllowMultipleDefaultValue = "Std-Choice-Required-AllowMultiple-DefaultValue";
        public const string UserRequiredHasDefaultUser = "Std-User-Required-HasDefault-User";
    }
}
