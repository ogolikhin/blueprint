using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using Common;
using Model.Impl;
using Newtonsoft.Json;
using NUnit.Framework;
using Utilities;
using Utilities.Facades;

namespace Model.ArtifactModel.Impl
{
    //TODO  Remove "sendAuthorizationAsCookie" since this does not apply to OpenAPI
    public class OpenApiArtifact : ArtifactBase, IOpenApiArtifact
    {
        #region Constants

        public const string SVC_PATH = "api/v1/projects";
        public const string URL_PUBLISH = "api/v1/vc/publish";
        public const string URL_DISCARD = "api/v1/vc/discard";


        #endregion Constants

        #region Constructors
        
        /// <summary>
        /// Constructor in order to use it as generic type
        /// </summary>
        public OpenApiArtifact()
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
        /// <param name="address">The URI address of the Open API</param>
        public OpenApiArtifact(string address) : base(address)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="address">The base url of the Open API</param>
        /// <param name="id">The artifact id</param>
        /// <param name="projectId">The project containing the artifact</param>
        public OpenApiArtifact(string address, int id, int projectId) : base(address, id, projectId)
        {
        }

        #endregion Constructors

        #region Methods

        public void Save(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform Save.");
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

            PublishArtifacts(artifactToPublish, Address, user, shouldKeepLock, expectedStatusCodes, sendAuthorizationAsCookie);
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

            var discardArtifactResults = DiscardArtifacts(artifactToDiscard, Address, user, expectedStatusCodes, sendAuthorizationAsCookie);

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

        #endregion Methods

        #region Static Methods

        /// <summary>
        /// Save a single artifact to Blueprint
        /// </summary>
        /// <param name="artifactToSave">The artifact to save</param>
        /// <param name="user">The user saving the artifact</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected. Also, if the request method is
        /// POST, the expected status code is 201; If the request method is PATCH, the expected status code is 200.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        public static void SaveArtifact(IArtifactBase artifactToSave, 
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactToSave, nameof(artifactToSave));

            // Use POST only if this is creating the artifact, otherwise use PATCH
            var restRequestMethod = artifactToSave.Id == 0 ? RestRequestMethod.POST : RestRequestMethod.PATCH;

            string tokenValue = user.Token?.OpenApiToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            string path = I18NHelper.FormatInvariant("{0}/{1}/artifacts", SVC_PATH, artifactToSave.ProjectId);

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { artifactToSave.Id == 0 ? HttpStatusCode.Created : HttpStatusCode.OK };
            }

            RestApiFacade restApi = new RestApiFacade(artifactToSave.Address, user.Username, user.Password, tokenValue);
            var artifactResult = restApi.SendRequestAndDeserializeObject<ArtifactResult, ArtifactBase>(
                path, restRequestMethod, artifactToSave as ArtifactBase, expectedStatusCodes: expectedStatusCodes);

            
            artifactToSave.ReplacePropertiesWithPropertiesFromSourceArtifact(artifactResult.Artifact);

            if (artifactResult.ResultCode == HttpStatusCode.Created || artifactResult.ResultCode == HttpStatusCode.OK)
            {
                artifactToSave.IsSaved = true;
            }

            Logger.WriteDebug("{0} {1} returned followings: Message: {2}, ResultCode: {3}", restRequestMethod.ToString(), path, artifactResult.Message, artifactResult.ResultCode);
            Logger.WriteDebug("The Artifact Returned: {0}", artifactResult.Artifact);

            Assert.That(artifactResult.Message == "Success", "The returned Message was '{0}' but 'Success' was expected", artifactResult.Message);

            if (restRequestMethod == RestRequestMethod.POST)
        {
                Assert.That(
                    artifactResult.ResultCode == HttpStatusCode.Created,
                    "The returned ResultCode was '{0}' but '{1}' was expected",
                    artifactResult.ResultCode,
                    ((int) HttpStatusCode.Created).ToString(CultureInfo.InvariantCulture));
            }
            else if (restRequestMethod == RestRequestMethod.PATCH)
            {
                Assert.That(
                    artifactResult.ResultCode == HttpStatusCode.OK,
                    "The returned ResultCode was '{0}' but '{1}' was expected",
                    artifactResult.ResultCode,
                    ((int)HttpStatusCode.OK).ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                Assert.True(restRequestMethod != RestRequestMethod.POST && restRequestMethod != RestRequestMethod.PATCH, "Only POST or PATCH methods are supported!" );
            }
        }

        /// <summary>
        /// Discard changes to artifact(s) from Blueprint
        /// </summary>
        /// <param name="artifactsToDiscard">The artifact(s) to be discarded.</param>
        /// <param name="address">The base url of the Open API</param>
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
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactsToDiscard, nameof(artifactsToDiscard));

            string tokenValue = user.Token?.OpenApiToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            // TODO Why do we need to make copies of artifacts here?  Add comment

            var artifactObjectList = artifactsToDiscard.Select(artifact => 
                new ArtifactBase(artifact.Address, artifact.Id, artifact.ProjectId)).ToList();

            RestApiFacade restApi = new RestApiFacade(address, user.Username, user.Password, tokenValue);

            var artifactResults = restApi.SendRequestAndDeserializeObject<List<DiscardArtifactResult>, List<ArtifactBase>>(
                URL_DISCARD, 
                RestRequestMethod.POST, 
                artifactObjectList, 
                expectedStatusCodes: expectedStatusCodes);

            var discardedResultList = artifactResults.FindAll(result => result.ResultCode.Equals(HttpStatusCode.OK));

            // When each artifact is successfully discarded, set IsSaved flag to false
            foreach (var discardedResult in discardedResultList)
            {
                var discardedArtifact = artifactObjectList.Find(a => a.Id.Equals(discardedResult.ArtifactId));
                discardedArtifact.IsSaved = false;
                Logger.WriteDebug("Result Code for the Discarded Artifact {0}: {1}", discardedResult.ArtifactId, discardedResult.ResultCode);
            }

            Assert.That(discardedResultList.Count.Equals(artifactObjectList.Count),
                "The number of artifacts passed for Discard was {0} but the number of artifacts returned was {1}",
                artifactObjectList.Count, discardedResultList.Count);

            return artifactResults.ConvertAll(o => (DiscardArtifactResult)o);
        }

        /// <summary>
        /// Publish Artifact(s) (Used when publishing a single artifact OR a list of artifacts)
        /// </summary>
        /// <param name="artifactsToPublish">The list of artifacts to publish</param>
        /// <param name="address">The base url of the Open API</param>
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
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactsToPublish, nameof(artifactsToPublish));

            string tokenValue = user.Token?.OpenApiToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            var additionalHeaders = new Dictionary<string, string>();

            if (shouldKeepLock)
            {
                additionalHeaders.Add("KeepLock", "true");
            }

            // 
            //var artifactObjectList = (
            //    from IArtifactBase artifact in artifactsToPublish
            //    select new ArtifactBase(artifact.Address, artifact.Id, artifact.ProjectId)).ToList();


            RestApiFacade restApi = new RestApiFacade(address, user.Username, user.Password, tokenValue);
            var artifactResults = restApi.SendRequestAndDeserializeObject<List<PublishArtifactResult>, List<IArtifactBase>>(
                URL_PUBLISH, 
                RestRequestMethod.POST,
                artifactsToPublish, 
                additionalHeaders: additionalHeaders, 
                expectedStatusCodes: expectedStatusCodes);

            var publishedResultList = artifactResults.FindAll(result => result.ResultCode.Equals(HttpStatusCode.OK));

            // When each artifact is successfully published, set IsSaved flag to false since there are no longer saved changes
            foreach (var publishedResult in publishedResultList)
            {
                var publishedArtifact = artifactsToPublish.Find(a => a.Id.Equals(publishedResult.ArtifactId));
                publishedArtifact.IsSaved = false;
                publishedArtifact.IsPublished = true;
                publishedArtifact.Version++;
                Logger.WriteDebug("Result Code for the Published Artifact {0}: {1}", publishedResult.ArtifactId, publishedResult.ResultCode);
            }

            Assert.That(publishedResultList.Count.Equals(artifactsToPublish.Count),
                "The number of artifacts passed for Publish was {0} but the number of artifacts returned was {1}",
                artifactsToPublish.Count, publishedResultList.Count);

            return artifactResults;
        }

        /// <summary>
        /// Delete a single artifact on Blueprint server.
        /// To delete artifact permanently, Publish must be called after the Delete, otherwise the deletion can be discarded.
        /// </summary>
        /// <param name="artifactToDelete">The list of artifacts to delete</param>
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
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactToDelete, nameof(artifactToDelete));

            string tokenValue = user.Token?.OpenApiToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            string path = I18NHelper.FormatInvariant("{0}/{1}/artifacts/{2}", SVC_PATH, artifactToDelete.ProjectId, artifactToDelete.Id);

            var queryparameters = new Dictionary<string, string>();

            if (deleteChildren)
            {
                queryparameters.Add("Recursively", "true");
            }

            RestApiFacade restApi = new RestApiFacade(artifactToDelete.Address, user.Username, user.Password, tokenValue);
            var artifactResults = restApi.SendRequestAndDeserializeObject<List<DeleteArtifactResult>>(
                path,
                RestRequestMethod.DELETE,
                queryParameters: queryparameters,
                expectedStatusCodes: expectedStatusCodes);

            foreach (var deletedArtifact in artifactResults)
            {
                Logger.WriteDebug("DELETE {0} returned following: ArtifactId: {1} Message: {2}, ResultCode: {3}", 
                    path, deletedArtifact.ArtifactId, deletedArtifact.Message, deletedArtifact.ResultCode);
            }

            return artifactResults;
        }

        /// <summary>
        /// Gets the Version property of an Artifact via OpenAPI call
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
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            if (user == null)
            {
                Assert.NotNull(artifact.CreatedBy, "No user is available to perform GetVersion.");
                user = artifact.CreatedBy;
            }

            string tokenValue = user.Token?.OpenApiToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            RestApiFacade restApi = new RestApiFacade(artifact.Address, user.Username, user.Password, tokenValue);
            var path = I18NHelper.FormatInvariant("{0}/{1}/artifacts/{2}", SVC_PATH, artifact.ProjectId, artifact.Id);
            var returnedArtifact = restApi.SendRequestAndDeserializeObject<ArtifactBase>(
                resourcePath: path, 
                method: RestRequestMethod.GET, 
                expectedStatusCodes: expectedStatusCodes);

            return returnedArtifact.Version;
        }


        //TODO Investigate if we can use IArtifact instead of ItemId

        /// <summary>
        /// Get discussions for the specified artifact/subartifact
        /// </summary>
        /// <param name="address">The base url of the Open API</param>
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
            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            var queryParameters = new Dictionary<string, string> {
                { "includeDraft", includeDraft.ToString() }
            };

            string path = I18NHelper.FormatInvariant(URL_DISCUSSIONS, itemId);
            var restApi = new RestApiFacade(address, user.Username, user.Password, tokenValue);
            var response = restApi.SendRequestAndDeserializeObject<Discussion>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies);

            return response;
        }

        /// <summary>
        /// Search artifact by a substring in its name on Blueprint server. Among published artifacts only.
        /// </summary>
        /// <param name="address">The base url of the Open API</param>
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
            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            var queryParameters = new Dictionary<string, string> {
                { "name", searchSubstring }
            };

            var restApi = new RestApiFacade(address, user.Username, user.Password, tokenValue);

            var response = restApi.SendRequestAndDeserializeObject<List<ArtifactBase>>(
                URL_SEARCH,
                RestRequestMethod.GET,
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies);
            Logger.WriteDebug("Response for search artifact by name: {0}", response);

            return response.ConvertAll(o => (IArtifactBase)o);
        }

        #endregion Static Methods
    }
}
