using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Common;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Utilities;
using Utilities.Facades;

namespace Model.Impl
{
    public class SvcShared : NovaServiceBase, ISvcShared
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The base URI of the svc/shared service.</param>
        public SvcShared(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            Address = address;
        }

        #region Members inherited from ISvcShared

        #region Artifacts methods

        /// <seealso cref="ISvcShared.DiscardArtifacts(IUser, List{IArtifactBase}, List{HttpStatusCode})"/>
        public List<NovaDiscardArtifactResult> DiscardArtifacts(IUser user,
            List<IArtifactBase> artifactsToDiscard,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return DiscardArtifacts(Address, user, artifactsToDiscard, expectedStatusCodes);
        }

        /// <summary>
        /// Discard changes to artifact(s) on Blueprint server using NOVA endpoint.
        /// NOTE: The internal status flags of the artifacts are NOT updated.
        /// (Runs:  'POST /svc/shared/artifacts/discard')
        /// </summary>
        /// <param name="address">The base URL of the Blueprint server.</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="artifactsToDiscard">The artifact(s) having changes to be discarded.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>The list of ArtifactResult objects created by the dicard artifacts request.</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        public static List<NovaDiscardArtifactResult> DiscardArtifacts(string address,
            IUser user,
            List<IArtifactBase> artifactsToDiscard,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactsToDiscard, nameof(artifactsToDiscard));

            var artifactsIds = artifactsToDiscard.Select(artifact => artifact.Id).ToList();
            var restApi = new RestApiFacade(address, user.Token?.AccessControlToken);

            var artifactResults = restApi.SendRequestAndDeserializeObject<NovaDiscardArtifactResults, List<int>>(
                RestPaths.Svc.Shared.Artifacts.DISCARD,
                RestRequestMethod.POST,
                artifactsIds,
                expectedStatusCodes: expectedStatusCodes);

            return artifactResults.DiscardResults;
        }

        /// <seealso cref="ISvcShared.LockArtifacts(IUser, List{IArtifactBase}, List{HttpStatusCode})"/>
        public List<LockResultInfo> LockArtifacts(IUser user,
            List<IArtifactBase> artifactsToLock,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return LockArtifacts(Address, user, artifactsToLock, expectedStatusCodes);
        }

        /// <summary>
        /// Lock Artifact(s).
        /// NOTE: The internal IsLocked and LockOwner flags are NOT updated by this function.
        /// (Runs:  'POST /svc/shared/artifacts/lock'  with artifact IDs in the request body)
        /// </summary>
        /// <param name="address">The base URL of the Blueprint server.</param>
        /// <param name="user">The user locking the artifact.</param>
        /// <param name="artifactsToLock">The list of artifacts to lock.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>List of LockResultInfo for the locked artifacts.</returns>
        public static List<LockResultInfo> LockArtifacts(string address,
            IUser user,
            List<IArtifactBase> artifactsToLock,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactsToLock, nameof(artifactsToLock));

            var artifactIds = (
                from IArtifactBase artifact in artifactsToLock
                select artifact.Id).ToList();

            return LockArtifacts(address, user, artifactIds, expectedStatusCodes);
        }

        /// <summary>
        /// Lock Artifact(s).
        /// NOTE: The internal IsLocked and LockOwner flags are NOT updated by this function.
        /// (Runs:  'POST /svc/shared/artifacts/lock'  with artifact IDs in the request body)
        /// </summary>
        /// <param name="address">The base URL of the Blueprint server.</param>
        /// <param name="user">The user locking the artifact.</param>
        /// <param name="artifactIds">The Ids of artifacts to lock.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>List of LockResultInfo for the locked artifacts.</returns>
        public static List<LockResultInfo> LockArtifacts(string address, IUser user, List<int> artifactIds,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            var restApi = new RestApiFacade(address, user.Token?.AccessControlToken);

            var lockResults = restApi.SendRequestAndDeserializeObject<List<LockResultInfo>, List<int>>(
                RestPaths.Svc.Shared.Artifacts.LOCK,
                RestRequestMethod.POST,
                jsonObject: artifactIds,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);

            return lockResults;
        }

        /// <seealso cref="ISvcShared.PublishArtifacts(IUser, List{int}, List{HttpStatusCode})"/>
        public List<NovaPublishArtifactResult> PublishArtifacts(IUser user,
            List<int> artifactsToPublish,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return PublishArtifacts(Address, user, artifactsToPublish, expectedStatusCodes);
        }

        /// <summary>
        /// Publish a list of artifacts on Blueprint server.  This is only used in Storyteller.
        /// NOTE: The internal IsSaved and IsPublished flags are NOT updated by this function.
        /// (Runs: 'POST /svc/shared/artifacts/publish')
        /// </summary>
        /// <param name="address">The base URL of the Blueprint server.</param>
        /// <param name="user">The user saving the artifact.</param>
        /// <param name="artifactsToPublish">The IDs of the artifacts to publish.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>Results of Publish operation.</returns>
        public static List<NovaPublishArtifactResult> PublishArtifacts(string address,
            IUser user,
            List<int> artifactsToPublish,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactsToPublish, nameof(artifactsToPublish));

            const string path = RestPaths.Svc.Shared.Artifacts.PUBLISH;
            var restApi = new RestApiFacade(address, user.Token?.AccessControlToken);

            var publishResults = restApi.SendRequestAndDeserializeObject<List<NovaPublishArtifactResult>, List<int>>(
                path,
                RestRequestMethod.POST,
                artifactsToPublish,
                expectedStatusCodes: expectedStatusCodes);

            return publishResults;
        }

        /// <seealso cref="ISvcShared.SearchArtifactsByName(IUser, string, IProject, List{HttpStatusCode})"/>
        public IList<IArtifactBase> SearchArtifactsByName(IUser user,
            string searchSubstring,
            IProject project = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return SearchArtifactsByName(Address, user, searchSubstring, project, expectedStatusCodes);
        }

        /// <summary>
        /// Search artifact by a substring in its name on Blueprint server.  Among published artifacts only.
        /// (Runs:  'GET svc/shared/artifacts/search')
        /// </summary>
        /// <param name="address">The base URL of the Blueprint server.</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="searchSubstring">The substring (case insensitive) to search.</param>
        /// <param name="project">The project to search, if project is null search within all available projects.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.</param>
        /// <returns>List of first 10 artifacts with name containing searchSubstring.</returns>
        public static IList<IArtifactBase> SearchArtifactsByName(string address,
            IUser user,
            string searchSubstring,
            IProject project = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            var queryParameters = new Dictionary<string, string>
            {
                { "name", searchSubstring }
            };

            if (project != null)
            {
                queryParameters.Add("projectId", project.Id.ToStringInvariant());
            }

            //showBusyIndicator doesn't affect server side, it is added to make call similar to call from HTML
            queryParameters.Add("showBusyIndicator", "false");

            var restApi = new RestApiFacade(address, user.Token?.AccessControlToken);

            var response = restApi.SendRequestAndDeserializeObject<List<ArtifactBase>>(
                RestPaths.Svc.Shared.Artifacts.SEARCH,
                RestRequestMethod.GET,
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);

            Logger.WriteDebug("Response for search artifact by name: {0}", response);

            return response.ConvertAll(o => (IArtifactBase)o);
        }

        #endregion Artifacts methods

        #region Navigation methods

        /// <seealso cref="ISvcShared.GetNavigation(IUser, List{IArtifact}, int?, int?, int?, bool?, List{HttpStatusCode})"/>
        public List<ArtifactReference> GetNavigation(
            IUser user,
            List<IArtifact> artifacts,
            int? versionId = null,
            int? revisionId = null,
            int? baselineId = null,
            bool? readOnly = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetNavigation(Address, user, artifacts,
                versionId: versionId,
                revisionId: revisionId,
                baselineId: baselineId,
                readOnly: readOnly,
                expectedStatusCodes: expectedStatusCodes);
        }

        /// <summary>
        /// Get ArtifactReference list which is used to represent breadcrumb navigation.
        /// (Runs:  'GET /svc/shared/navigation/{id1}/{id2}...')
        /// </summary>
        /// <param name="address">The base URL of the Blueprint server.</param>
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        public static List<ArtifactReference> GetNavigation(string address,
            IUser user,
            List<IArtifact> artifacts,
            int? versionId = null,
            int? revisionId = null,
            int? baselineId = null,
            bool? readOnly = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifacts, nameof(artifacts));

            //Get list of artifacts which were created.
            var artifactIds = artifacts.Select(artifact => artifact.Id).ToList();
            var path = I18NHelper.FormatInvariant(RestPaths.Svc.Shared.NAVIGATION_ids_, string.Join("/", artifactIds));

            var queryParameters = new Dictionary<string, string>();

            if (versionId != null)
            {
                queryParameters.Add("versionId", versionId.Value.ToStringInvariant());
            }

            if (revisionId != null)
            {
                queryParameters.Add("revisionId", revisionId.Value.ToStringInvariant());
            }

            if (baselineId != null)
            {
                queryParameters.Add("baselineId", baselineId.Value.ToStringInvariant());
            }

            if (readOnly != null)
            {
                queryParameters.Add("readOnly", readOnly.Value.ToString().ToLowerInvariant());
            }

            var restApi = new RestApiFacade(address, user.Token?.AccessControlToken);

            var response = restApi.SendRequestAndDeserializeObject<List<ArtifactReference>>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);

            return response;
        }

        #endregion Navigation methods

        #region User and Group methods

        /// <seealso cref="ISvcShared.FindUserOrGroup(IUser, string, bool?, int?, bool?, List{HttpStatusCode})"/>
        public List<UserOrGroupInfo> FindUserOrGroup(IUser user, 
            string search = null,
            bool? allowEmptyEmail = null,
            int? limit = null,
            bool? includeGuests = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            // GET: <base_url>/svc/shared/users/search?search={search}&emailDiscussions={emailDiscussions}&limit={limit}&includeGuests={includeGuests}
            // See:  https://github.com/BlueprintSys/blueprint-current/blob/develop/Source/BluePrintSys.RC.Web.Internal/Shared/Metadata/UsersAndGroupsController.cs

            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);
            var queryParams = new Dictionary<string, string>();

            if (search != null)
            {
                queryParams.Add("search", search);
            }

            if (allowEmptyEmail != null)
            {
                queryParams.Add("emailDiscussions", allowEmptyEmail.Value.ToString());
            }

            if (limit != null)
            {
                queryParams.Add("limit", limit.Value.ToStringInvariant());
            }

            if (includeGuests != null)
            {
                queryParams.Add("includeGuests", includeGuests.Value.ToString());
            }

            return restApi.SendRequestAndDeserializeObject<List<UserOrGroupInfo>>(
                RestPaths.Svc.Shared.Users.SEARCH,
                RestRequestMethod.GET,
                queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);
        }

        #endregion User and Group methods

        #endregion Members inherited from ISvcShared

        #region Members inherited from IDisposable

        private bool _isDisposed = false;

        /// <summary>
        /// Disposes this object by deleting all objects that were created.
        /// </summary>
        /// <param name="disposing">Pass true if explicitly disposing or false if called from the destructor.</param>
        protected virtual void Dispose(bool disposing)
        {
            Logger.WriteTrace("{0}.{1} called.", nameof(SvcShared), nameof(Dispose));

            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // TODO: Delete anything created by this class.
            }

            _isDisposed = true;
        }

        /// <summary>
        /// Disposes this object by deleting all objects that were created.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion Members inherited from IDisposable
    }
}
