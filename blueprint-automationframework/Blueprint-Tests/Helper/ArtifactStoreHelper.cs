using Common;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Utilities;
using Utilities.Facades;
using Model.NovaModel;
using Model.ArtifactModel.Impl;

namespace Helper
{
    public static class ArtifactStoreHelper
    {
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
        /// <param name="requestBody">The request body (i.e. artifact to be updated).</param>
        /// <param name="artifactId">The ID of the artifact to save.</param>
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
        /// Attaches file to the artifact (Save changes).
        /// </summary>
        /// <param name="user">User to perform an operation.</param>
        /// <param name="artifact">Artifact.</param>
        /// <param name="files">List of files to attach.</param>
        /// <param name="artifactStore">IArtifactStore.</param>
        public static void AddArtifactAttachmentAndSave(IUser user, IArtifact artifact, List<INovaFile> files, IArtifactStore artifactStore)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(files, nameof(files));
            ThrowIf.ArgumentNull(artifactStore, nameof(artifactStore));

            artifact.Lock(user);
            NovaArtifactDetails artifactDetails = artifactStore.GetArtifactDetails(user, artifact.Id);
            foreach (var file in files)
            {
                artifactDetails.AttachmentValues.Add(new AttachmentValue(user, file));
            }   

            Artifact.UpdateArtifact(artifact, user, artifactDetails, artifactStore.Address);
            var attachment = artifactStore.GetAttachments(artifact, user);
            Assert.IsTrue(attachment.AttachedFiles.Count > 0, "Artifact should have at least one attachment.");
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

            Artifact.UpdateArtifact(artifact, user, artifactDetails, artifactStore.Address);
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

            inlineTraceText = I18NHelper.FormatInvariant("<html><head></head><body style=\"padding: 1px 0px 0px; font-family: 'Portable User Interface'; font-size: 10.67px\"><div style=\"padding: 0px\"><p style=\"margin: 0px\">&#x200b;<a linkassemblyqualifiedname=\"BluePrintSys.RC.Client.SL.RichText.RichTextArtifactLink, BluePrintSys.RC.Client.SL.RichText, Version=7.4.0.0, Culture=neutral, PublicKeyToken=null\" canclick=\"True\" isvalid=\"True\" href=\"{0}?ArtifactId={1}\" target=\"_blank\" artifactid=\"{1}\" style=\"font-family: 'Portable User Interface'; font-size: 11px; font-style: normal; font-weight: normal; text-decoration: underline; color: #0000FF\" title=\"Project: akim_project\"><span style=\"font-family: 'Portable User Interface'; font-size: 11px; font-style: normal; font-weight: normal; text-decoration: underline; color: #0000FF\">{2}{1}: {3}</span></a><span style=\"-c1-editable: true; font-family: 'Portable User Interface'; font-size: 10.67px; font-style: normal; font-weight: normal; color: Black\">&#x200b;</span></p></div></body></html>",
                inlineTraceArtifact.Address, inlineTraceArtifact.Id, inlineTraceArtifactDetails.Prefix, inlineTraceArtifactDetails.Name);

            return inlineTraceText;
        }

        /// <summary>
        /// Check if the inline trace link is valid or not.
        /// </summary>
        /// <param name="inlineTraceLink">The inlinetrace link to validate</param>
        /// <returns> returnes true if the inline trace link is valid inline trace link</returns>
        private static bool IsValidInlineTrace(string inlineTraceLink)
        {
            string validTag = "isValid=\"True\"";

            return inlineTraceLink.Contains(validTag);
        }

        /// <summary>
        /// Validates inline trace link returned from artifact details
        /// </summary>
        /// <param name="artifactdetails">the artifact details with the inline trace link which needs validation</param>
        /// <param name="inlineTraceArtifact">the inline trace artifact the inline trace link pointing to</param>
        /// <param name="validInlineTraceLink">the boolean represents if the link is to be valid or not</param>
        public static void ValidateInlineTraceLinkFromArtifactDetails(NovaArtifactDetails artifactdetails, IArtifactBase inlineTraceArtifact, bool validInlineTraceLink)
        {
            ThrowIf.ArgumentNull(artifactdetails, nameof(artifactdetails));
            ThrowIf.ArgumentNull(inlineTraceArtifact, nameof(inlineTraceArtifact));

            // Validation: Verify that the artifactDeatils' description field which contain inline trace link contains the valid inline trace information (name of the inline trace artifact)
            Assert.That(artifactdetails.Description.Contains(inlineTraceArtifact.Name), "Expected outcome should not contains {0} on returned artifactdetails. Returned inline trace content is {1}.", inlineTraceArtifact.Name, artifactdetails.Description);

            if (!validInlineTraceLink)
            {
                // Validation: Verify that the artifactdetails' description contains invalid inline trace link
                Assert.IsFalse(IsValidInlineTrace(artifactdetails.Description), "Expected invalid inlineTraceLink from returned artifactdetails. Returned valid inline trace content. The returned inlinetrace link is {0}.", artifactdetails.Description);
            }
        }
    }
}
