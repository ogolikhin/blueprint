using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Common;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using NUnit.Framework;
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
            var discardResults = DiscardArtifacts(Address, user, artifactsToDiscard, expectedStatusCodes);

            UpdateStatusOfArtifactsThatWereDiscarded(discardResults, artifactsToDiscard);

            return discardResults;
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

        /// <summary>
        /// Updates the IsSaved and IsMarkedForDeletion flags to false for all artifacts that were successfully discarded.
        /// </summary>
        /// <param name="discardResults">The results returned from the Discard REST call.</param>
        /// <param name="artifactsToDiscard">The list of artifacts that you attempted to discard.</param>
        public static void UpdateStatusOfArtifactsThatWereDiscarded(List<NovaDiscardArtifactResult> discardResults, List<IArtifactBase> artifactsToDiscard)
        {
            ThrowIf.ArgumentNull(artifactsToDiscard, nameof(artifactsToDiscard));
            ThrowIf.ArgumentNull(discardResults, nameof(discardResults));

            // When each artifact is successfully discarded, set IsSaved & IsMarkedForDeletion flags to false.
            foreach (var discardedResult in discardResults)
            {
                var discardedArtifact = artifactsToDiscard.Find(a => (a.Id == discardedResult.ArtifactId) &&
                    (discardedResult.Result == NovaDiscardArtifactResult.ResultCode.Success));

                Logger.WriteDebug("Result Code for the Discarded Artifact {0}: {1}", discardedResult.ArtifactId, (int)discardedResult.Result);

                if (discardedArtifact != null)
                {
                    discardedArtifact.IsSaved = false;
                    discardedArtifact.IsMarkedForDeletion = false;
                }
            }

            Assert.That(discardResults.Count.Equals(artifactsToDiscard.Count),
                "The number of artifacts passed for Discard was {0} but the number of artifacts returned was {1}",
                artifactsToDiscard.Count, discardResults.Count);
        }

        /// <summary>
        /// Lock Artifact(s).  NOTE: The internal IsLocked and LockOwner flags are NOT updated by this function.
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

            var restApi = new RestApiFacade(address, user.Token?.AccessControlToken);

            var lockResults = restApi.SendRequestAndDeserializeObject<List<LockResultInfo>, List<int>>(
                RestPaths.Svc.Shared.Artifacts.LOCK,
                RestRequestMethod.POST,
                jsonObject: artifactIds,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);

            return lockResults;
        }

        /// <summary>
        /// Updates the IsLocked and LockOwner flags to false for all artifacts that were successfully locked.
        /// </summary>
        /// <param name="lockResults">The results returned from the Lock REST call.</param>
        /// <param name="user">The user that performed the Lock operation.</param>
        /// <param name="artifactsToLock">The list of artifacts that you attempted to lock.</param>
        /// <param name="expectedLockResults">(optional) The expected LockResults returned in the JSON body.  This is only checked if StatusCode = 200.
        ///     If null, only Success is expected.</param>
        public static void UpdateStatusOfArtifactsThatWereLocked(List<LockResultInfo> lockResults,
            IUser user,
            List<IArtifactBase> artifactsToLock,
            List<LockResult> expectedLockResults = null)
        {
            ThrowIf.ArgumentNull(artifactsToLock, nameof(artifactsToLock));
            ThrowIf.ArgumentNull(lockResults, nameof(lockResults));

            if (expectedLockResults == null)
            {
                expectedLockResults = new List<LockResult> { LockResult.Success };
            }

            // Update artifacts with lock info.
            foreach (var artifact in artifactsToLock)
            {
                var lockResultInfo = lockResults.Find(x => x.Info.ArtifactId == artifact.Id);

                Assert.NotNull(lockResultInfo, "No LockResultInfo was returned for artifact ID {0} after trying to lock it!", artifact.Id);

                if (lockResultInfo.Result == LockResult.Success)
                {
                    artifact.LockOwner = user;
                    artifact.Status.IsLocked = true;
                }

                Assert.That(expectedLockResults.Contains(lockResultInfo.Result),
                    "We expected the lock Result to be one of: [{0}], but it was: {1}!",
                    string.Join(", ", expectedLockResults), lockResultInfo.Result);
            }
        }

        #endregion Artifacts methods

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
