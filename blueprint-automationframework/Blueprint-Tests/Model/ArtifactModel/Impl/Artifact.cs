using System.Net;
using System.Collections.Generic;
using System.Linq;
using Utilities;
using NUnit.Framework;
using Utilities.Facades;
using Common;

namespace Model.ArtifactModel.Impl
{
    public class Artifact : ArtifactBase, IArtifact
    {
        #region Constructors

        /// <summary>
        /// Constructor in order to use it as generic type
        /// </summary>
        public Artifact()
        {
            //Required for deserializing OpenApiArtifact
            Properties = new List<OpenApiProperty>();
            Comments = new List<OpenApiComment>();
            Traces = new List<OpenApiTrace>();
            Attachments = new List<OpenApiAttachment>();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The base url of the API</param>
        public Artifact(string address) : base(address)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="address">The base url of the API</param>
        /// <param name="id">The artifact id</param>
        /// <param name="projectId">The project containing the artifact</param>
        public Artifact(string address, int id, int projectId) : base(address, id, projectId)
        {
        }

        #endregion Constructors

        #region Methods

        public void Save(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            // If CreatedBy is null, then this save is adding the artifact.  User must not be null.
            if (CreatedBy == null)
            {
                Assert.NotNull(user, "No user is available to add the artifact.");
                CreatedBy = user;
            }

            // If user is null, attempt to save using the CreatedBy user.  CreatedBy must not be null.
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to save the artifact.");
                user = CreatedBy;
            }

            SaveArtifact(this, user, expectedStatusCodes, sendAuthorizationAsCookie);
        }

        public void Publish(IUser user = null,
            bool shouldKeepLock = false,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform Publish.");
                user = CreatedBy;
            }

            var artifactToPublish = new List<IArtifactBase> { this };

            PublishArtifacts(
                artifactToPublish, 
                Address, 
                user,
                shouldKeepLock,
                expectedStatusCodes, 
                sendAuthorizationAsCookie);
        }

        public List<DiscardArtifactResult> Discard(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform Discard.");
                user = CreatedBy;
            }

            var artifactToDiscard = new List<IArtifactBase> { this };

            var discardArtifactResults = DiscardArtifacts(
                artifactToDiscard, 
                Address, 
                user, 
                expectedStatusCodes, 
                sendAuthorizationAsCookie);

            return discardArtifactResults;
        }

        public List<DiscardArtifactResult> NovaDiscard(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform Discard.");
                user = CreatedBy;
            }

            var artifactsToDiscard = new List<IArtifactBase> { this };

            var discardArtifactResults = NovaDiscardArtifacts(
                artifactsToDiscard,
                Address,
                user,
                expectedStatusCodes,
                sendAuthorizationAsCookie);

            return discardArtifactResults;
        }
        public List<DeleteArtifactResult> Delete(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false,
            bool deleteChildren = false)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform Delete.");
                user = CreatedBy;
            }

            var deleteArtifactResults = DeleteArtifact(
                this,
                user,
                expectedStatusCodes,
                sendAuthorizationAsCookie,
                deleteChildren);

            return deleteArtifactResults;
        }

        public int GetVersion(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform GetVersion.");
                user = CreatedBy;
            }

            int artifactVersion = GetVersion(this, user, expectedStatusCodes, sendAuthorizationAsCookie);

            return artifactVersion;
        }

        public LockResultInfo Lock(
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to lock the artifact.");
                user = CreatedBy;
            }

            var artifactToLock = new List<IArtifactBase> { this };

            var artifactLockResults = LockArtifacts(
                artifactToLock,
                Address,
                user,
                expectedStatusCodes,
                sendAuthorizationAsCookie);

            Assert.That(artifactLockResults.Count == 1, "Multiple lock artifact results were returned when 1 was expected.");

            var artifactLockResult = artifactLockResults.First();

            return artifactLockResult;
        }

        public ArtifactInfo GetArtifactInfo(IUser user = null,
           List<HttpStatusCode> expectedStatusCodes = null,
           bool sendAuthorizationAsCookie = false)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform GetArtifactInfo.");
                user = CreatedBy;
            }

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            RestApiFacade restApi = new RestApiFacade(Address, user.Username, user.Password, tokenValue);
            var path = I18NHelper.FormatInvariant("{0}/{1}", URL_ARTIFACT_INFO, Id);
            var returnedArtifactInfo = restApi.SendRequestAndDeserializeObject<ArtifactInfo>(
                resourcePath: path, method: RestRequestMethod.GET, expectedStatusCodes: expectedStatusCodes);

            return returnedArtifactInfo;
        }

        #endregion Methods

        #region Static Methods

        /// <summary>
        /// Save a single artifact to Blueprint
        /// </summary>
        /// <param name="artifactToSave">The artifact to save</param>
        /// <param name="user">The user saving the artifact</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        public static void SaveArtifact(IArtifactBase artifactToSave,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            OpenApiArtifact.SaveArtifact(artifactToSave, user, expectedStatusCodes, sendAuthorizationAsCookie);
        }

        /// <summary>
        /// Discard changes to artifact(s) from Blueprint
        /// </summary>
        /// <param name="artifactsToDiscard">The artifact(s) having changes to be discarded.</param>
        /// <param name="address">The base url of the API</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The list of ArtifactResult objects created by the dicard artifacts request</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        public static List<DiscardArtifactResult> DiscardArtifacts(List<IArtifactBase> artifactsToDiscard,
            string address,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            return OpenApiArtifact.DiscardArtifacts(
                artifactsToDiscard,
                address,
                user,
                expectedStatusCodes,
                sendAuthorizationAsCookie);
        }

        /// <summary>
        /// Discard changes to artifact(s) on Blueprint server using NOVA endpoint(not OpenAPI).
        /// </summary>
        /// <param name="artifactsToDiscard">The artifact(s) having changes to be discarded.</param>
        /// <param name="address">The base url of the API</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The list of ArtifactResult objects created by the dicard artifacts request</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        public static List<DiscardArtifactResult> NovaDiscardArtifacts(List<IArtifactBase> artifactsToDiscard,
            string address,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactsToDiscard, nameof(artifactsToDiscard));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            RestApiFacade restApi = new RestApiFacade(address, user.Username, user.Password, tokenValue);

            var artifactsIds = artifactsToDiscard.Select(artifact => artifact.Id).ToList();
            var artifactResults = restApi.SendRequestAndDeserializeObject<NovaDiscardArtifactResult, List<int>>(
                URL_NOVADISCARD,
                RestRequestMethod.POST,
                artifactsIds,
                expectedStatusCodes: expectedStatusCodes);

            var discardedResultList = artifactResults.DiscardResults;

            // When each artifact is successfully discarded, set IsSaved flag to false
            foreach (var discardedResult in discardedResultList)
            {
                var discardedArtifact = artifactsToDiscard.Find(a => (a.Id.Equals(discardedResult.ArtifactId)) &&
                discardedResult.ResultCode == 0);
                discardedArtifact.IsSaved = false;
                Logger.WriteDebug("Result Code for the Discarded Artifact {0}: {1}", discardedResult.ArtifactId, discardedResult.ResultCode);
            }

            Assert.That(discardedResultList.Count.Equals(artifactsToDiscard.Count),
                "The number of artifacts passed for Discard was {0} but the number of artifacts returned was {1}",
                artifactsToDiscard.Count, discardedResultList.Count);

            return discardedResultList;
        }
        
        /// <summary>
        /// Publish Artifact(s) (Used when publishing a single artifact OR a list of artifacts)
        /// </summary>
        /// <param name="artifactsToPublish">The list of artifacts to publish</param>
        /// <param name="address">The base url of the API</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="shouldKeepLock">(optional) Boolean parameter which defines whether or not to keep the lock after publishing the artfacts</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The list of PublishArtifactResult objects created by the publish artifacts request</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        public static List<PublishArtifactResult> PublishArtifacts(List<IArtifactBase> artifactsToPublish,
            string address,
            IUser user,
            bool shouldKeepLock = false,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            return OpenApiArtifact.PublishArtifacts(
                artifactsToPublish,
                address,
                user,
                shouldKeepLock,
                expectedStatusCodes,
                sendAuthorizationAsCookie);
        }

        /// <summary>
        /// Delete a single artifact on Blueprint server.
        /// To delete artifact permanently, Publish must be called after the Delete, otherwise the deletion can be discarded.
        /// </summary>
        /// <param name="artifactToDelete">The artifact to delete</param>
        /// <param name="user">The user deleting the artifact. If null, attempts to delete using the credentials
        /// of the user that created the artifact.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <param name="deleteChildren">(optional) Specifies whether or not to also delete all child artifacts of the specified artifact</param>
        /// <returns>The DeletedArtifactResult list after delete artifact call</returns>
        public static List<DeleteArtifactResult> DeleteArtifact(IArtifactBase artifactToDelete,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false,
            bool deleteChildren = false)
        {
            return OpenApiArtifact.DeleteArtifact(
                artifactToDelete,
                user,
                expectedStatusCodes,
                sendAuthorizationAsCookie,
                deleteChildren);
        }

        /// <summary>
        /// Gets the Version property of an Artifact via API call
        /// </summary>
        /// <param name="artifact">The artifact</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The historical version of the artifact.</returns>
        public static int GetVersion(IArtifactBase artifact,
            IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            return OpenApiArtifact.GetVersion(artifact, user, expectedStatusCodes, sendAuthorizationAsCookie);
        }

        /// <summary>
        /// Get discussions for the specified artifact/subartifact
        /// </summary>
        /// <param name="address">The base url of the API</param>
        /// <param name="itemId">id of artifact/subartifact</param>
        /// <param name="includeDraft">false gets discussions for the last published version, true works with draft</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>Discussion for artifact/subartifact</returns>
        public static IDiscussion GetDiscussions(string address,
            int itemId,
            bool includeDraft,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            return OpenApiArtifact.GetDiscussions(
                address,
                itemId,
                includeDraft,
                user,
                expectedStatusCodes,
                sendAuthorizationAsCookie);
        }

        // TODO Move this to a common area, not in artifact/openapiartifact class

        /// <summary>
        /// Search artifact by a substring in its name on Blueprint server. Among published artifacts only.
        /// </summary>
        /// <param name="address">The base url of the API</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="searchSubstring">The substring(case insensitive) to search.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.</param>
        /// <returns>List of first 10 artifacts with name containing searchSubstring</returns>
        public static IList<IArtifactBase> SearchArtifactsByName(string address,
            IUser user,
            string searchSubstring,
            bool sendAuthorizationAsCookie = false,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return OpenApiArtifact.SearchArtifactsByName(
                address,
                user,
                searchSubstring,
                sendAuthorizationAsCookie,
                expectedStatusCodes);
        }

        /// <summary>
        /// Lock Artifact(s) 
        /// </summary>
        /// <param name="artifactsToLock">The list of artifacts to lock</param>
        /// <param name="address">The base url of the API</param>
        /// <param name="user">The user locking the artifact</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>List of LockResultInfo for the locked artifacts</returns>
        public static List<LockResultInfo> LockArtifacts(List<IArtifactBase> artifactsToLock,
            string address,
            IUser user, 
            List<HttpStatusCode> expectedStatusCodes = null, 
            bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactsToLock, nameof(artifactsToLock));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            var artifactIds = (
                from IArtifactBase artifact in artifactsToLock
                select artifact.Id).ToList();

            var restApi = new RestApiFacade(address, user.Username, user.Password, tokenValue);

            var response = restApi.SendRequestAndDeserializeObject<List<LockResultInfo>, List<int>>(
                URL_LOCK,
                RestRequestMethod.POST,
                jsonObject: artifactIds,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies);

            return response;
        }

        #endregion Static Methods
    }
}
