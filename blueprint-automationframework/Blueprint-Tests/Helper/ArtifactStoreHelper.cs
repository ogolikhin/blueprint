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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Utilities;
using Utilities.Facades;

namespace Helper
{
    public static class ArtifactStoreHelper
    {
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
                INovaProject novaProject = returnedProjects.Find(p => p.Id == expectedProject.Id);

                Assert.NotNull(novaProject, "Project ID {0} was not found in the list of returned projects!", expectedProject.Id);
                Assert.AreEqual(expectedProject.Name, novaProject.Name,
                    "Returned project ID {0} should have Name: '{1}'!", expectedProject.Id, expectedProject.Name);
                Assert.IsNull(novaProject.Description, "The returned project Description should always be null!");
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
        /// <exception cref="AssertionException">If any of the properties are different.</exception>
        public static void AssertArtifactsEqual(IArtifactBase expectedArtifactBase, INovaArtifactBase actualNovaArtifactBase, bool skipIdAndVersion = false)
        {
            ThrowIf.ArgumentNull(actualNovaArtifactBase, nameof(actualNovaArtifactBase));
            ThrowIf.ArgumentNull(expectedArtifactBase, nameof(expectedArtifactBase));

            if (!skipIdAndVersion)
            {
                Assert.AreEqual(expectedArtifactBase.Id, actualNovaArtifactBase.Id, "The Id parameters don't match!");
                Assert.AreEqual(expectedArtifactBase.Version, actualNovaArtifactBase.Version, "The Version  parameters don't match!");
            }

            Assert.AreEqual(expectedArtifactBase.Name, actualNovaArtifactBase.Name, "The Name  parameters don't match!");
            Assert.AreEqual(expectedArtifactBase.ParentId, actualNovaArtifactBase.ParentId, "The ParentId  parameters don't match!");
            Assert.AreEqual(expectedArtifactBase.ArtifactTypeId, actualNovaArtifactBase.ItemTypeId, "The ItemTypeId  parameters don't match!");
            Assert.AreEqual(expectedArtifactBase.ProjectId, actualNovaArtifactBase.ProjectId, "The ProjectId  parameters don't match!");
        }

        /// <summary>
        /// Asserts that the specified INovaArtifactBase object is equal to the specified IArtifactBase.
        /// </summary>
        /// <param name="novaArtifactBase1">The INovaArtifactBase to compare against.</param>
        /// <param name="novaArtifactBase2">The IArtifactBase to compare against.</param>
        /// <exception cref="AssertionException">If any of the properties are different.</exception>
        public static void AssertArtifactsEqual(INovaArtifactBase novaArtifactBase1, INovaArtifactBase novaArtifactBase2)
        {
            ThrowIf.ArgumentNull(novaArtifactBase1, nameof(novaArtifactBase1));
            ThrowIf.ArgumentNull(novaArtifactBase2, nameof(novaArtifactBase2));

            Assert.AreEqual(novaArtifactBase1.Id, novaArtifactBase2.Id, "The Id parameters don't match!");
            Assert.AreEqual(novaArtifactBase1.Name, novaArtifactBase2.Name, "The Name  parameters don't match!");
            Assert.AreEqual(novaArtifactBase1.ParentId, novaArtifactBase2.ParentId, "The ParentId  parameters don't match!");
            Assert.AreEqual(novaArtifactBase1.ItemTypeId, novaArtifactBase2.ItemTypeId, "The ItemTypeId  parameters don't match!");
            Assert.AreEqual(novaArtifactBase1.ProjectId, novaArtifactBase2.ProjectId, "The ProjectId  parameters don't match!");
            Assert.AreEqual(novaArtifactBase1.Version, novaArtifactBase2.Version, "The Version  parameters don't match!");
        }

        /// <summary>
        /// Asserts that both INovaArtifactDetails objects are equal.
        /// </summary>
        /// <param name="artifact1">The first INovaArtifactDetails to compare against.</param>
        /// <param name="artifact2">The second INovaArtifactDetails to compare against.</param>
        /// <exception cref="AssertionException">If any of the properties are different.</exception>
        public static void AssertArtifactsEqual(INovaArtifactDetails artifact1, INovaArtifactDetails artifact2)
        {
            ThrowIf.ArgumentNull(artifact1, nameof(artifact1));
            ThrowIf.ArgumentNull(artifact2, nameof(artifact2));

            Assert.AreEqual(artifact1.Id, artifact2.Id, "The Id parameters don't match!");
            Assert.AreEqual(artifact1.Name, artifact2.Name, "The Name  parameters don't match!");
            Assert.AreEqual(artifact1.Description, artifact2.Description, "The Description  parameters don't match!");
            Assert.AreEqual(artifact1.ParentId, artifact2.ParentId, "The ParentId  parameters don't match!");
            Assert.AreEqual(artifact1.Permissions, artifact2.Permissions, "The Permissions  parameters don't match!");
            Assert.AreEqual(artifact1.OrderIndex, artifact2.OrderIndex, "The OrderIndex  parameters don't match!");
            Assert.AreEqual(artifact1.ItemTypeId, artifact2.ItemTypeId, "The ItemTypeId  parameters don't match!");
            Assert.AreEqual(artifact1.ItemTypeVersionId, artifact2.ItemTypeVersionId, "The ItemTypeVersionId  parameters don't match!");
            Assert.AreEqual(artifact1.LockedDateTime, artifact2.LockedDateTime, "The LockedDateTime  parameters don't match!");
            Assert.AreEqual(artifact1.ProjectId, artifact2.ProjectId, "The ProjectId  parameters don't match!");
            Assert.AreEqual(artifact1.Version, artifact2.Version, "The Version  parameters don't match!");
            Assert.AreEqual(artifact1.CreatedOn, artifact2.CreatedOn, "The CreatedOn  parameters don't match!");
            Assert.AreEqual(artifact1.LastEditedOn, artifact2.LastEditedOn, "The LastEditedOn  parameters don't match!");

            Identification.AssertEquals(artifact1.CreatedBy, artifact2.CreatedBy);
            Identification.AssertEquals(artifact1.LastEditedBy, artifact2.LastEditedBy);
            Identification.AssertEquals(artifact1.LockedByUser, artifact2.LockedByUser);

            Assert.AreEqual(artifact1.CustomPropertyValues.Count, artifact2.CustomPropertyValues.Count, "The number of Custom Properties is different!");
            Assert.AreEqual(artifact1.SpecificPropertyValues.Count, artifact2.SpecificPropertyValues.Count, "The number of Specific Property Values is different!");

            // Now compare each property in CustomProperties & SpecificPropertyValues.
            foreach (CustomProperty property in artifact1.CustomPropertyValues)
            {
                Assert.That(artifact2.CustomPropertyValues.Exists(p => p.Name == property.Name),
                "Couldn't find a CustomProperty named '{0}'!", property.Name);
            }

            foreach (CustomProperty property in artifact1.SpecificPropertyValues)
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
            Assert.AreEqual(artifact1.Permissions, artifact2.Permissions, "The Permissions  parameters don't match!");
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
        /// Compares Two Custom Properties for Equality
        /// </summary>
        /// <param name="expectedProperty">The first custom property to compare.</param>
        /// <param name="actualProperty">The second custom property to compare.</param>
        public static void AssertCustomPropertiesAreEqual(CustomProperty expectedProperty, CustomProperty actualProperty)
        {
            ThrowIf.ArgumentNull(expectedProperty,nameof(expectedProperty));
            ThrowIf.ArgumentNull(actualProperty, nameof(actualProperty));

            Assert.IsNotNull(expectedProperty.PrimitiveType, "The primitive type for the first custom property was not present!");
            Assert.IsNotNull(actualProperty.PrimitiveType, "The primitive type for the second custom property was not present!");
            Assert.AreEqual(expectedProperty.PrimitiveType, actualProperty.PrimitiveType, "PrimitiveType properties don't match!");

            var primitiveType = (PropertyPrimitiveType)expectedProperty.PrimitiveType;

            switch (primitiveType)
            {
                case PropertyPrimitiveType.Text:
                case PropertyPrimitiveType.Number:
                    Assert.AreEqual(expectedProperty.CustomPropertyValue, actualProperty.CustomPropertyValue, "The custom {0} properties do not match.", primitiveType);
                    break;

                //TODO:  Investigate if the DateTimeUtility can be applied to both properties
                case PropertyPrimitiveType.Date:
                    var secondCustomPropertyValue = DateTimeUtilities.ConvertDateTimeToSortableDateTime((DateTime)actualProperty.CustomPropertyValue);
                    Assert.AreEqual(expectedProperty.CustomPropertyValue, secondCustomPropertyValue, "The custom {0} properties do not match.", primitiveType);
                    break;

                //TODO: Investigate if the JsonConvert could be done for both
                case PropertyPrimitiveType.Choice:
                    var validValues1 = ((ChoiceValues)expectedProperty.CustomPropertyValue).ValidValues;
                    var validValues2 = JsonConvert.DeserializeObject<ChoiceValues>(actualProperty.CustomPropertyValue.ToString()).ValidValues;

                    Assert.AreEqual(validValues1.Count, validValues2.Count, "The custom {0} property counts are not equal.", primitiveType);

                    for (int i = 0; i < validValues1.Count; i++)
                    {
                        var choiceValue1 = validValues1[i];
                        var choiceValue2 = validValues2[i];

                        Assert.AreEqual(choiceValue1.Id, choiceValue2.Id, "The custom {0} property Ids are not equal.", primitiveType);
                        Assert.AreEqual(choiceValue1.Value, choiceValue2.Value, "The custom {0} property choice values are not equal.", primitiveType);
                    }

                    if (!string.IsNullOrEmpty(((ChoiceValues) expectedProperty.CustomPropertyValue).CustomValue))
                    {
                        var customValue1 = ((ChoiceValues)expectedProperty.CustomPropertyValue).CustomValue;
                        var customValue2 = JsonConvert.DeserializeObject<ChoiceValues>(actualProperty.CustomPropertyValue.ToString()).CustomValue;

                        Assert.AreEqual(customValue1, customValue2, "The custom {0} property CustomValues are not equal.", primitiveType);
                    }
                    break;

                case PropertyPrimitiveType.User:
                    var userGroups1 = ((UserGroupValues)expectedProperty.CustomPropertyValue).UsersGroups;
                    var userGroups2 = JsonConvert.DeserializeObject<UserGroupValues>(actualProperty.CustomPropertyValue.ToString()).UsersGroups;

                    Assert.AreEqual(userGroups1.Count, userGroups2.Count, "The custom {0} property counts are not equal.", primitiveType);

                    for (int i = 0; i < userGroups1.Count; i++)
                    {
                        var userGroupValue1 = userGroups1[i];
                        var userGroupValue2 = userGroups2[i];

                        Assert.AreEqual(userGroupValue1.Id, userGroupValue2.Id, "The custom {0} property Ids are not equal.", primitiveType);
                        Assert.AreEqual(userGroupValue1.DisplayName, userGroupValue2.DisplayName, "The custom {0} Display Names are not equal.", primitiveType);

                        if (userGroupValue1.IsGroup != null)
                        {
                            Assert.AreEqual(userGroupValue1.IsGroup, userGroupValue2.IsGroup, "The custom {0} property IsGroup flags are not equal.", primitiveType);
                        }
                    }

                    if (!string.IsNullOrEmpty(((UserGroupValues)expectedProperty.CustomPropertyValue).Label))
                    {
                        var customValue1 = ((UserGroupValues)expectedProperty.CustomPropertyValue).Label;
                        var customValue2 = JsonConvert.DeserializeObject<UserGroupValues>(actualProperty.CustomPropertyValue.ToString()).Label;

                        Assert.AreEqual(customValue1, customValue2, "The custom {0} property Labels are not equal.", primitiveType);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(I18NHelper.FormatInvariant("The primitive type: {0} was not expected", primitiveType.ToString()));
            }
        }

        #endregion Custom Asserts

        /// <summary>
        /// Gets the custom data project.
        /// </summary>
        /// <returns>The custom data project.</returns>
        public static IProject GetCustomDataProject(IUser user)
        {
            List<IProject> allProjects = null;
            allProjects = ProjectFactory.GetAllProjects(user);

            const string customDataProjectName = "Custom Data";

            Assert.That(allProjects.Exists(p => (p.Name == customDataProjectName)),
                "No project was found named '{0}'!", customDataProjectName);

            var projectCustomData = allProjects.First(p => (p.Name == customDataProjectName));
            projectCustomData.GetAllArtifactTypes(ProjectFactory.Address, user);

            return projectCustomData;
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
        /// <returns>The attachments that were added.</returns>
        public static Attachments AddArtifactAttachmentsAndSave(
            IUser user,
            IArtifact artifact,
            List<INovaFile> files,
            IArtifactStore artifactStore,
            bool shouldLockArtifact = true)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(files, nameof(files));
            ThrowIf.ArgumentNull(artifactStore, nameof(artifactStore));

            if (shouldLockArtifact)
            {
                artifact.Lock(user);
            }

            NovaArtifactDetails artifactDetails = artifactStore.GetArtifactDetails(user, artifact.Id);

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
        /// <returns>The attachments that were added.</returns>
        public static Attachments AddArtifactAttachmentAndSave(
            IUser user,
            IArtifact artifact,
            INovaFile file,
            IArtifactStore artifactStore,
            bool shouldLockArtifact = true,
            int expectedAttachedFilesCount = 1)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(file, nameof(file));
            ThrowIf.ArgumentNull(artifactStore, nameof(artifactStore));

            var attachments = AddArtifactAttachmentsAndSave(user, artifact, new List<INovaFile> { file }, artifactStore, shouldLockArtifact);
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
        public static void AddSubArtifactAttachmentAndSave(IUser user, IArtifact artifact, INovaSubArtifact subArtifact,
            List<INovaFile> files, IArtifactStore artifactStore)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(subArtifact, nameof(subArtifact));
            ThrowIf.ArgumentNull(files, nameof(files));
            ThrowIf.ArgumentNull(artifactStore, nameof(artifactStore));
            Assert.AreEqual(artifact.Id, subArtifact.ParentId, "subArtifact should belong to Artifact");

            artifact.Lock(user);
            NovaArtifactDetails artifactDetails = artifactStore.GetArtifactDetails(user, artifact.Id);

            NovaSubArtifact subArtifactToAdd = new NovaSubArtifact();
            subArtifactToAdd.Id = subArtifact.Id;
            foreach (var file in files)
            {
                subArtifactToAdd.AttachmentValues.Add(new AttachmentValue(user, file));
            }

            List<INovaSubArtifact> subArtifacts = new List<INovaSubArtifact> { subArtifactToAdd };

            artifactDetails.SubArtifacts = subArtifacts;

            Artifact.UpdateArtifact(artifact, user, artifactDetails, address: artifactStore.Address);
            var attachment = artifactStore.GetAttachments(artifact, user, subArtifactId: subArtifact.Id);
            Assert.IsTrue(attachment.AttachedFiles.Count >= files.Count, "All attachments should be added.");
        }

        /// <summary>
        /// deletes file from the artifact (Save changes).
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

            var attachment = artifactStore.GetAttachments(artifact, user);
            Assert.IsNotNull(attachment, "Getattachments shouldn't return null.");
            Assert.IsTrue(attachment.AttachedFiles.Count > 0, "Artifact should have at least one attachment.");
            var fileToDelete = attachment.AttachedFiles.FirstOrDefault(f => f.AttachmentId == fileId);
            Assert.AreEqual(fileId, fileToDelete.AttachmentId, "Attachments must contain file with fileId.");

            artifact.Lock(user);
            NovaArtifactDetails artifactDetails = artifactStore.GetArtifactDetails(user, artifact.Id);
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
        public static void DeleteSubArtifactAttachmentAndSave(IUser user, IArtifact artifact, INovaSubArtifact subArtifact,
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
            Assert.AreEqual(fileId, fileToDelete.AttachmentId, "Attachments must contain file with fileId.");

            artifact.Lock(user);
            NovaArtifactDetails artifactDetails = artifactStore.GetArtifactDetails(user, artifact.Id);
            artifactDetails.AttachmentValues.Add(new AttachmentValue(fileToDelete.AttachmentId));

            NovaSubArtifact subArtifactToAdd = new NovaSubArtifact();
            subArtifactToAdd.Id = subArtifact.Id;
            subArtifactToAdd.AttachmentValues.Add(new AttachmentValue(fileToDelete.AttachmentId));

            List<INovaSubArtifact> subArtifacts = new List<INovaSubArtifact> { subArtifactToAdd };

            artifactDetails.SubArtifacts = subArtifacts;
            Artifact.UpdateArtifact(artifact, user, artifactDetails, address: artifactStore.Address);
        }

        /// <summary>
        /// Creates a new NovaArtifactDetails with the published artifact
        /// </summary>
        /// <param name="artifact">The artifact which contains properties that NovaArtiactDetails refers to</param>
        /// <returns>NovaArtifactDetails</returns>
        public static NovaArtifactDetails CreateNovaArtifactDetailsWithArtifact(IArtifactBase artifact)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            NovaArtifactDetails novaArtifactDetails = new NovaArtifactDetails
            {
                Id = artifact.Id,
                ProjectId = artifact.ProjectId,
                ParentId = artifact.ParentId,
                Version = artifact.Version,
            };
            return novaArtifactDetails;
        }

        /// <summary>
        /// Creates inline trace text for the provided artifact. For use with RTF properties.
        /// </summary>
        /// <param name="inlineTraceArtifact">target artifact for inline traces</param>
        /// <param name="inlineTraceArtifactDetails">target artifactDetails for inline traces</param>
        /// <returns>inline trace text</returns>
        public static string CreateArtifactInlineTraceValue(IArtifactBase inlineTraceArtifact, INovaArtifactDetails inlineTraceArtifactDetails)
        {
            ThrowIf.ArgumentNull(inlineTraceArtifact, nameof(inlineTraceArtifact));
            ThrowIf.ArgumentNull(inlineTraceArtifactDetails, nameof(inlineTraceArtifactDetails));

            string inlineTraceText = null;

            inlineTraceText = I18NHelper.FormatInvariant("<html><head></head><body style=\"padding: 1px 0px 0px; font-family: 'Portable User Interface'; font-size: 10.67px\">" +
                "<div style=\"padding: 0px\"><p style=\"margin: 0px\">&#x200b;<a linkassemblyqualifiedname=\"BluePrintSys.RC.Client.SL.RichText.RichTextArtifactLink, " +
                "BluePrintSys.RC.Client.SL.RichText, Version=7.4.0.0, Culture=neutral, PublicKeyToken=null\" canclick=\"True\" isvalid=\"True\" href=\"{0}?ArtifactId={1}\" " +
                "target=\"_blank\" artifactid=\"{1}\" style=\"font-family: 'Portable User Interface'; font-size: 11px; font-style: normal; font-weight: normal; " +
                "text-decoration: underline; color: #0000FF\" title=\"Project: {4}\"><span style=\"font-family: 'Portable User Interface'; font-size: 11px; " +
                "font-style: normal; font-weight: normal; text-decoration: underline; color: #0000FF\">{2}{1}: {3}</span></a><span " +
                "style=\"-c1-editable: true; font-family: 'Portable User Interface'; font-size: 10.67px; font-style: normal; font-weight: normal; color: Black\">" +
                "&#x200b;</span></p></div></body></html>",
                inlineTraceArtifact.Address, inlineTraceArtifact.Id, inlineTraceArtifactDetails.Prefix, inlineTraceArtifactDetails.Name, inlineTraceArtifact.Project.Name);

            return inlineTraceText;
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
        public static void ValidateTrace(NovaTrace sourceArtifactTrace, IArtifactBase targetArtifact)
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
        /// <param name="artifactdetails">The artifact details containing the inline trace link which needs validation</param>
        /// <param name="inlineTraceArtifact">The artifact contained within the inline trace link</param>
        /// <param name="validInlineTraceLink">A flag indicating whether the inline trace link is expected to be valid or not</param>
        public static void ValidateInlineTraceLinkFromArtifactDetails(NovaArtifactDetails artifactdetails,
            IArtifactBase inlineTraceArtifact,
            bool validInlineTraceLink)
        {
            ThrowIf.ArgumentNull(artifactdetails, nameof(artifactdetails));
            ThrowIf.ArgumentNull(inlineTraceArtifact, nameof(inlineTraceArtifact));

            // Validation: Verify that the artifactDetails' description field which contain inline trace link contains the valid
            // inline trace information (name of the inline trace artifact).
            Assert.That(artifactdetails.Description.Contains(inlineTraceArtifact.Name),
                "Expected outcome should not contains {0} on returned artifactdetails. Returned inline trace content is {1}.",
                inlineTraceArtifact.Name,
                artifactdetails.Description);

            Assert.AreEqual(validInlineTraceLink, IsValidInlineTrace(artifactdetails.Description),
                "Expected {0} for valid inline trace but {1} was returned. The returned inlinetrace link is {2}.",
                validInlineTraceLink,
                !validInlineTraceLink,
                artifactdetails.Description);
        }

        /// <summary>
        /// Validates inline trace link returned from subartifact
        /// </summary>
        /// <param name="subArtifact">The subartifact containing the inline trace link which needs validation</param>
        /// <param name="inlineTraceArtifact">The artifact contained within the inline trace link</param>
        /// <param name="validInlineTraceLink">A flag indicating whether the inline trace link is expected to be valid or not</param>
        public static void ValidateInlineTraceLinkFromSubArtifactDetails(NovaSubArtifact subArtifact,
            IArtifactBase inlineTraceArtifact,
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

            IServiceErrorMessage expectedError = ServiceErrorMessageFactory.CreateServiceErrorMessage(
                expectedErrorCode,
                expectedErrorMessage);

            serviceError.AssertEquals(expectedError);
        }

        /// <summary>
        /// Creates new rich text that includes inline trace(s)
        /// </summary>
        /// <param name="artifacts">The artifacts being added as inline trace(s)</param>
        /// <returns>A formatted rich text string with inline traces(s)</returns>
        public static string CreateTextForProcessInlineTrace(IList<IArtifact> artifacts)
        {
            ThrowIf.ArgumentNull(artifacts, nameof(artifacts));

            var text = string.Empty;

            foreach (var artifact in artifacts)
            {
                var openApiProperty = artifact.Properties.FirstOrDefault(p => p.Name == "ID");
                if (openApiProperty != null)
                {
                    text = text + I18NHelper.FormatInvariant("<a href=\"{0}/?/ArtifactId={1}\" target=\"\" artifactid=\"{1}\"" +
                        " linkassemblyqualifiedname=\"BluePrintSys.RC.Client.SL.RichText.RichTextArtifactLink, BluePrintSys.RC.Client.SL.RichText, " +
                        "Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\" canclick=\"True\" isvalid=\"True\" title=\"Project: {3}\">" +
                        "<span style=\"text-decoration: underline; color: #0000ff\">{4}: {2}</span></a>",
                        artifact.Address, artifact.Id, artifact.Name, artifact.Project.Name,
                        openApiProperty.TextOrChoiceValue);
                }
            }

            Assert.IsFalse(string.IsNullOrWhiteSpace(text), "Text for inline trace was null or whitespace!");

            return I18NHelper.FormatInvariant("<p>{0}</p>", text);
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
        /// <param name="expectedErrorMessage">(optional)Expected error message.</param>
        public static void UpdateManualArtifactTraceAndSave(IUser user, IArtifact artifact, IArtifactBase traceTarget,
            ArtifactUpdateChangeType changeType, IArtifactStore artifactStore, TraceDirection traceDirection = TraceDirection.From,
            bool? isSuspect = null, INovaSubArtifact targetSubArtifact = null, IServiceErrorMessage expectedErrorMessage = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(traceTarget, nameof(traceTarget));
            ThrowIf.ArgumentNull(traceDirection, nameof(traceDirection));
            ThrowIf.ArgumentNull(artifactStore, nameof(artifactStore));

            artifact.Lock(user);
            NovaArtifactDetails artifactDetails = artifactStore.GetArtifactDetails(user, artifact.Id);

            NovaTrace traceToCreate = new NovaTrace();
            traceToCreate.ArtifactId = traceTarget.Id;
            traceToCreate.ProjectId = traceTarget.ProjectId;
            traceToCreate.Direction = traceDirection;
            traceToCreate.TraceType = TraceTypes.Manual;
            traceToCreate.ItemId = targetSubArtifact?.Id ?? traceTarget.Id;
            traceToCreate.ChangeType = changeType;
            traceToCreate.IsSuspect = isSuspect ?? false;

            List<NovaTrace> updatedTraces = new List<NovaTrace> { traceToCreate };

            artifactDetails.Traces = updatedTraces;

            Artifact.UpdateArtifact(artifact, user, artifactDetails, address: artifactStore.Address,
                expectedServiceErrorMessage: expectedErrorMessage);
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

            return I18NHelper.FormatInvariant("{0}(Standard Pack)", artifactTypeNameBase);
        }

        public class ChoiceValues
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
            [JsonProperty("validValues")]
            public List<NovaPropertyType.ValidValue> ValidValues { get; set; }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
            [JsonProperty("customValue")]
            public string CustomValue { get; set; }
        }

        public class UserGroupValues
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
            [JsonProperty("usersGroups")]
            public List<Identification> UsersGroups { get; set; }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
            [JsonProperty("label")]
            public string Label { get; set; }
        }
    }
}
