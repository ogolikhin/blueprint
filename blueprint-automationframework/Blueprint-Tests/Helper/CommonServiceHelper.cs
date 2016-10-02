using Common;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using Model.ArtifactModel.Enums;
using Utilities;

namespace Helper
{

    public static class CommonServiceHelper
    {
        private const string NAVIGATION_BASE_URL = "/Web/#/Storyteller/";
        private const string INACCESSIBLE_ARTIFACT_NAME = "<Inaccessible>";
        // TODO This will need to be updated with the value that cannot does not exist in the system 
        //Non-existence artifact Id sample
        public const int NONEXISTENT_ARTIFACT_ID = 99999999;

        /// <summary>
        /// Verifies that the JSON content returned by a 'GET /status' call has the expected fields.
        /// </summary>
        /// <param name="content">The content returned from a GET /status call.</param>
        /// <param name="extraExpectedStrings">(optional) A list of additional strings to search for in the returned JSON content.</param>
        /// <exception cref="AssertionException">If any expected fields are not found.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]   // The first assert already validates for null.
        public static void ValidateStatusResponseContent(string content, IList<string> extraExpectedStrings = null)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(content), "GET /status returned no content!");

            Logger.WriteDebug("GET /status returned: '{0}'", content);

            // TODO: See if we can do more verification beyond just looking for keywords.
            var stringsToFind = new List<string> { "serviceName", "accessInfo", "assemblyFileVersion", "noErrors", "errors", "statusResponses" };

            if (extraExpectedStrings != null)
            {
                stringsToFind.AddRange(extraExpectedStrings);
            }

            foreach (string tag in stringsToFind)
            {
                Assert.That(content.ContainsIgnoreCase(tag), "The content returned from GET /status should contain '{0}'!", tag);
            }
        }

        /// <summary>
        /// Verifies that the JSON content returned by a 'GET /svc/shared/navigation/{artifactId}' call has the expected fields.
        /// </summary>
        /// <param name="project">The project where accessible artifacts resides for the user</param>
        /// <param name="user">The user who have access to the project</param>
        /// <param name="resultArtifactReferenceList">The returned artifact reference list from GET navigation call</param>
        /// <param name="artifactList">The list of artifacts for the navigation</param>
        /// <param name="readOnly">(optional) Indicator that returning artifact reference links are readOnly format</param>
        /// <exception cref="AssertionException">If any expected fields are not found.</exception>
        public static void VerifyNavigation(
            IProject project,
            IUser user,
            List<ArtifactReference> resultArtifactReferenceList,
            List<IArtifact> artifactList,
            bool readOnly = false
            )
        {
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(resultArtifactReferenceList, nameof(resultArtifactReferenceList));
            ThrowIf.ArgumentNull(artifactList, nameof(artifactList));

            Assert.That(resultArtifactReferenceList.Count.Equals(artifactList.Count),
                "The expected number of ArtifactRerences from GetNavigation is {0} but the call returned {1}.",
                artifactList.Count, resultArtifactReferenceList.Count);

            //Validations for accessible artifact references

            //accessibleSourceArtifactList is list of artifacts which are either created by the same user or doesn't contain NONEXISTETNT_ARTIFACT_ID
            var accessibleSourceArtifactList = artifactList.FindAll(
                artifact => artifact.CreatedBy.Token.Equals(user.Token) && !artifact.Id.Equals(NONEXISTENT_ARTIFACT_ID));

            List<int> accessibleSourceArtifactdIdList = accessibleSourceArtifactList.ConvertAll(a => a.Id);

            //accessibleResultArtifactReferenceList is list of artifact references whose Ids exist in accessibleSourceArtifactdIdList
            var accessibleResultArtifactReferenceList = resultArtifactReferenceList.FindAll(ar => accessibleSourceArtifactdIdList.Contains(ar.Id));

            foreach (var accessibleResultArtifactReference in accessibleResultArtifactReferenceList)
            {
                //var accessibleSourceArtifact = accessibleSourceArtifactList.Find(artifact => artifact.Id.Equals(accessibleResultArtifactReference.Id));
                var accessibleSourceArtifact = accessibleSourceArtifactList[accessibleResultArtifactReferenceList.IndexOf(accessibleResultArtifactReference)];

                var sourceArtifactType = project.ArtifactTypes.Find(artifactType => artifactType.BaseArtifactType.ToString().Equals(
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

            //nonExistentSourceArtifactList is list of artifacts which are either created by the different user or contain NONEXISTETNT_ARTIFACT_ID
            var nonExistentSourceArtifactList = artifactList.FindAll(
                artifact => !artifact.CreatedBy.Token.Equals(user.Token) || artifact.Id.Equals(NONEXISTENT_ARTIFACT_ID));

            var nonExistentSourceArtifactIdList = nonExistentSourceArtifactList.ConvertAll(a => a.Id);

            //nonExistentArtifactReferenceList is list of artifact references whose Ids exist in nonExistentSourceArtifactIdList
            var nonExistentArtifactReferenceList = resultArtifactReferenceList.FindAll(ar => nonExistentSourceArtifactIdList.Contains(ar.Id));

            foreach (var nonExistentArtifactReference in nonExistentArtifactReferenceList)
            {
                Assert.IsTrue(nonExistentArtifactReference.Name.Equals(INACCESSIBLE_ARTIFACT_NAME),
                    "The name value for the non-existent/inaccessible artifact (Id: {0}) on artifact reference should be {1} but returned name value is {2}.",
                    nonExistentArtifactReference.Id, INACCESSIBLE_ARTIFACT_NAME, nonExistentArtifactReference.Name);

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
                linkPath = readOnly ? I18NHelper.FormatInvariant("{0}{1}/?readOnly=1", linkPath, artifactReference.Id)
                    : I18NHelper.FormatInvariant("{0}{1}/", linkPath, artifactReference.Id);
                if (artifactReference.Id.Equals(NONEXISTENT_ARTIFACT_ID) || artifactReference.Name.Equals(INACCESSIBLE_ARTIFACT_NAME))
                {
                    Assert.That(artifactReference.Link == null,
                        "The expected link value for the artifact (Id: {0}) is null but returned link value is {1}",
                        artifactReference.Id, artifactReference.Link);
                }
                else
                {
                    Assert.That(artifactReference.Link.Equals(linkPath),
                        "The expected link value for the artifact (Id: {0}) is {1} but the returned link value is {2}.",
                        artifactReference.Id, linkPath, artifactReference.Link);
                }
            }
        }
    }
}
