﻿using System;
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

        #endregion Artifact methods

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
    }
}
