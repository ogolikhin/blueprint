using System;
using System.Collections.Generic;
using System.Net;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;

namespace Model
{
    public interface ISvcShared : IDisposable
    {
        #region Artifact methods

        /// <summary>
        /// Discard changes to artifact(s) on Blueprint server using NOVA endpoint.
        /// NOTE: The internal status flags of the artifacts are NOT updated.
        /// (Runs:  'POST /svc/shared/artifacts/discard')
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="artifactsToDiscard">The artifact(s) having changes to be discarded.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>The list of ArtifactResult objects created by the dicard artifacts request.</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        List<NovaDiscardArtifactResult> DiscardArtifacts(IUser user,
            List<IArtifactBase> artifactsToDiscard,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Lock Artifact(s).
        /// NOTE: The internal IsLocked and LockOwner flags are NOT updated by this function.
        /// (Runs:  'POST /svc/shared/artifacts/lock'  with artifact IDs in the request body)
        /// </summary>
        /// <param name="user">The user locking the artifact.</param>
        /// <param name="artifactsToLock">The list of artifacts to lock.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>List of LockResultInfo for the locked artifacts.</returns>
        List<LockResultInfo> LockArtifacts(IUser user,
            List<IArtifactBase> artifactsToLock,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Publish a list of artifacts on Blueprint server.  This is only used in Storyteller.
        /// NOTE: The internal IsSaved and IsPublished flags are NOT updated by this function.
        /// (Runs: 'POST /svc/shared/artifacts/publish')
        /// </summary>
        /// <param name="user">The user saving the artifact.</param>
        /// <param name="artifactsToPublish">The IDs of the artifacts to publish.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>Results of Publish operation.</returns>
        List<NovaPublishArtifactResult> PublishArtifacts(IUser user,
            List<int> artifactsToPublish,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Search artifact by a substring in its name on Blueprint server.  Among published artifacts only.
        /// (Runs:  'GET svc/shared/artifacts/search')
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="searchSubstring">The substring (case insensitive) to search.</param>
        /// <param name="project">The project to search, if project is null search within all available projects.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.</param>
        /// <returns>List of first 10 artifacts with name containing searchSubstring.</returns>
        IList<IArtifactBase> SearchArtifactsByName(IUser user,
            string searchSubstring,
            IProject project = null,
            List<HttpStatusCode> expectedStatusCodes = null);

        #endregion Artifact methods

        #region Navigation methods

        /// <summary>
        /// Get ArtifactReference list which is used to represent breadcrumb navigation.
        /// (Runs:  'GET /svc/shared/navigation/{id1}/{id2}...')
        /// </summary>
        /// <param name="user">The user credentials for breadcrumb navigation.</param>
        /// <param name="artifacts">The list of artifacts used for breadcrumb navigation.</param>
        /// <param name="versionId">(optional) The Version ID??</param>
        /// <param name="revisionId">(optional) The Revision ID??</param>
        /// <param name="baselineId">(optional) The Baseline ID??</param>
        /// <param name="readOnly">(optional) Indicator which determines if returning artifact references are readOnly or not.
        ///     By default, readOnly is set to false.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.</param>
        /// <returns>The List of ArtifactReferences after the get navigation call.</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        List<ArtifactReference> GetNavigation(
            IUser user,
            List<IArtifact> artifacts,
            int? versionId = null,
            int? revisionId = null,
            int? baselineId = null,
            bool? readOnly = null,
            List<HttpStatusCode> expectedStatusCodes = null);

        #endregion Navigation methods

        #region User and Group methods

        /// <summary>
        /// Searches for users or groups that match the search criteria.
        /// </summary>
        /// <param name="user">The user to authenticate the REST call.</param>
        /// <param name="search">(optional) The sub-string to search for in the display name or E-mail address of the user or group.</param>
        /// <param name="allowEmptyEmail">(optional) If false, do not return users and groups with an empty E-mail address.  Default (null) = false.</param>
        /// <param name="limit">(optional) The max number of results to return.  Default (null) = 5.  The min is 1 and max is 500.</param>
        /// <param name="includeGuests">(optional) If false, do not return guests.  Default (null) = true.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>The list of UserOrGroupInfo that was returned.</returns>
        List<UserOrGroupInfo> FindUserOrGroup(IUser user,
            string search = null,
            bool? allowEmptyEmail = null,
            int? limit = null,
            bool? includeGuests = null,
            List<HttpStatusCode> expectedStatusCodes = null);

        #endregion User and Group methods
    }
}
