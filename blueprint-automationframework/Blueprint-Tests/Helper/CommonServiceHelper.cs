using Common;
using Model;
using Model.NavigationModel;
using Model.OpenApiModel;
using Model.OpenApiModel.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using Utilities;

namespace Helper
{

    public static class CommonServiceHelper
    {
        private const string NAVIGATION_BASE_URL = "/Web/#/Storyteller/";
        private const string INACCESSIBLE_ARTIFACT_NAME = "<Inaccessible>";
        private const int NONEXISTENT_ARTIFACT_ID = 99999999;

        /// <summary>
        /// Verifies that the JSON content returned by a 'GET /status' call has the expected fields.
        /// </summary>
        /// <param name="content">The content returned from a GET /status call.</param>
        /// <exception cref="AssertionException">If any expected fields are not found.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]   // The first assert already validates for null.
        public static void ValidateStatusResponseContent(string content)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(content), "GET /status returned no content!");

            Logger.WriteDebug("GET /status returned: '{0}'", content);

            var stringsToFind = new List<string> { "ServiceName", "AccessInfo", "AssemblyFileVersion", "NoErrors", "Errors", "StatusResponses", "AccessControlEndpoint", "ConfigControlEndpoint" };

            foreach (string tag in stringsToFind)
            {
                Assert.That(content.Contains(tag), "The content returned from GET /status should contain '{0}'!", tag);
            }
        }

        /// <summary>
        /// Verifies that the JSON content returned by a 'GET /svc/shared/navigation/{artifactId}' call has the expected fields.
        /// </summary>
        /// <param name="content">The content returned from a GET /svc/shared/navigation/{artifactId} call.</param>
        /// <exception cref="AssertionException">If any expected fields are not found.</exception>
        public static void VerifyNavigation(
            IProject project,
            IUser user,
            List<IArtifactReference> resultArtifactReferenceList,
            INavigation navigation)
        {
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(resultArtifactReferenceList, nameof(resultArtifactReferenceList));
            ThrowIf.ArgumentNull(navigation, nameof(navigation));

            Assert.That(resultArtifactReferenceList.Count.Equals(navigation.Artifacts.Count),
                "The expected number of ArtifactRerences from GetNavigation is {0} but the call returned {1}.",
                navigation.Artifacts.Count, resultArtifactReferenceList.Count);

            //Validations for accessible artifact references
            var accessibleSourceArtifactList = navigation.Artifacts.FindAll(
                artifact => artifact.CreatedBy.Token.Equals(user.Token) && !artifact.Id.Equals(NONEXISTENT_ARTIFACT_ID));
            var accessibleResultArtifactReferenceList = resultArtifactReferenceList.FindAll(ar => accessibleSourceArtifactList.ConvertAll(a => a.Id).Contains(ar.Id));
            var accessibleSourceArtifact = new OpenApiArtifact();

            var sourceArtifactType = new Model.Impl.ArtifactType();

            foreach (var accessibleResultArtifactReference in accessibleResultArtifactReferenceList)
            {
                accessibleSourceArtifact = (OpenApiArtifact)accessibleSourceArtifactList.Find(a => a.Id.Equals(accessibleResultArtifactReference.Id));

                sourceArtifactType = project.ArtifactTypes.Find(at => at.BaseArtifactType.ToString().Equals(
                    accessibleResultArtifactReference.BaseItemTypePredefined.ToString()));

                Assert.IsTrue(accessibleResultArtifactReference.Name.Equals(accessibleSourceArtifact.Name),
                    "The name value for the accessible artifact (Id: {0}) on artifact reference should be {1} but returned name value is {2}.",
                    accessibleResultArtifactReference.Id, accessibleSourceArtifact.Name, accessibleResultArtifactReference.Name);

                Assert.IsTrue(accessibleResultArtifactReference.Id.Equals(accessibleSourceArtifact.Id),
                    "The ID for the accessible artifact (Id: {0}) on artifact reference should be {1} but returned ID is {2}.",
                    accessibleResultArtifactReference.Id, accessibleSourceArtifact.Id, accessibleResultArtifactReference.Id);

                Assert.IsTrue(accessibleResultArtifactReference.BaseItemTypePredefined.ToString().Equals(accessibleSourceArtifact.BaseArtifactType.ToString()),
                    "The baseItemTypePredefined for the accessible artifact (Id: {0}) on artifact reference should be {1} but returned baseItemTypePredefined is {2}.",
                    accessibleResultArtifactReference.Id, accessibleSourceArtifact.BaseArtifactType.ToString(),
                    accessibleResultArtifactReference.BaseItemTypePredefined.ToString());

                Assert.IsTrue(accessibleResultArtifactReference.ProjectId.Equals(accessibleSourceArtifact.ProjectId),
                    "The projectId for the accessible artifact (Id: {0}) on artifact reference should be {1} but returned projectId is {2}.",
                    accessibleResultArtifactReference.Id, accessibleSourceArtifact.ProjectId, accessibleResultArtifactReference.ProjectId);

                Assert.IsTrue(accessibleResultArtifactReference.TypePrefix.Equals(sourceArtifactType.Prefix),
                    "The typePrefix for the accessible artifact (Id: {0}) on artifact reference should be {1} but returned typePrefix is {2}.",
                    accessibleResultArtifactReference.Id, sourceArtifactType.Prefix, accessibleResultArtifactReference.TypePrefix);
            }

            //Validations for non-existance or inaccessible artifact references
            var nonExistentSourceArtifactList = navigation.Artifacts.FindAll(
                artifact => !artifact.CreatedBy.Token.Equals(user.Token) || artifact.Id.Equals(NONEXISTENT_ARTIFACT_ID));
            var nonExistentArtifactReferenceList = resultArtifactReferenceList.FindAll(ar => nonExistentSourceArtifactList.ConvertAll(a => a.Id).Contains(ar.Id));

            foreach (var nonExistentArtifactReference in nonExistentArtifactReferenceList)
            {
                Assert.IsTrue(nonExistentArtifactReference.Name.Equals(INACCESSIBLE_ARTIFACT_NAME),
                    "The name value for the non-existent/inaccessible artifact (Id: {0}) on artifact reference should be {1} but returned name value is {2}.",
                    nonExistentArtifactReference.Id, INACCESSIBLE_ARTIFACT_NAME, nonExistentArtifactReference.Name);

                Assert.IsTrue(nonExistentArtifactReference.Link == null,
                    "The link for the non-existent/inaccessible artifact (Id: {0}) on artifact reference should be null but returned link is {1}.",
                    nonExistentArtifactReference.Id, nonExistentArtifactReference.Link);

                Assert.IsTrue(nonExistentArtifactReference.BaseItemTypePredefined.Equals(ItemTypePredefined.None),
                    "The baseItemTypePredefined for the non-existent/inaccessible artifact (Id: {0}) on artifact reference should be {1} but returned ",
                    "baseItemTypePredefined is {2}.",
                    nonExistentArtifactReference.Id, ItemTypePredefined.None, nonExistentArtifactReference.BaseItemTypePredefined);

                Assert.IsTrue(nonExistentArtifactReference.ProjectId.Equals(0),
                    "The projectId for the non-existent/inaccessible artifact (Id: {0}) on artifact reference should be {1} but returned projectId is {2}.",
                    nonExistentArtifactReference.Id, 0, nonExistentArtifactReference.ProjectId);

                Assert.IsTrue(nonExistentArtifactReference.TypePrefix == null,
                    "The typePrefix for the non-existent/inaccessible artifact (Id: {0}) on artifact reference should be null but returned typePrefix is {1}.",
                    nonExistentArtifactReference.Id, nonExistentArtifactReference.TypePrefix);
            }

            //Validation for links in resultArtifactReferenceList
            string linkPath = NAVIGATION_BASE_URL;
            foreach (var artifactReference in resultArtifactReferenceList)
            {
                linkPath = I18NHelper.FormatInvariant("{0}{1}/", linkPath, artifactReference.Id);
                if (artifactReference.Id.Equals(NONEXISTENT_ARTIFACT_ID) || artifactReference.Name.Equals(INACCESSIBLE_ARTIFACT_NAME))
                {
                    Assert.That(artifactReference.Link == null,
                        "The expected link value is null but returned link value is {0}", artifactReference.Link);
                }
                else
                {
                    Assert.That(artifactReference.Link.Equals(linkPath),
                        "The expected link value is {0} but the returned link value is {1}.",
                        linkPath, artifactReference.Link);
                }
            }
        }
    }
}
