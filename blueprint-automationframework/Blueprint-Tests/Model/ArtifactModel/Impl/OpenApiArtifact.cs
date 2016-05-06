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
    public class OpenApiArtifact : ArtifactBase, IOpenApiArtifact
    {
        #region Properties

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof (Deserialization.ConcreteConverter<List<OpenApiProperty>>))]
        public List<OpenApiProperty> Properties { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof (Deserialization.ConcreteConverter<List<OpenApiComment>>))]
        public List<OpenApiComment> Comments { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof (Deserialization.ConcreteConverter<List<OpenApiTrace>>))]
        public List<OpenApiTrace> Traces { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof (Deserialization.ConcreteConverter<List<OpenApiAttachment>>))]
        public List<OpenApiAttachment> Attachments { get; set; }
        
        #endregion Properties

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
        /// <param name="address">The URI address of the artifact.</param>
        public OpenApiArtifact(string address) : base(address)
        {
        }

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

            PublishArtifacts(artifactToPublish, Address, user, expectedStatusCodes, shouldKeepLock, sendAuthorizationAsCookie);
        }

        public List<IDiscardArtifactResult> Discard(IUser user = null,
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

        public List<IDeleteArtifactResult> Delete(IUser user = null,
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

            string path = I18NHelper.FormatInvariant("{0}/{1}/{2}", SVC_PATH, artifactToSave.ProjectId, URL_ARTIFACTS);

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            }

            RestApiFacade restApi = new RestApiFacade(artifactToSave.Address, user.Username, user.Password, tokenValue);
            var artifactResult = restApi.SendRequestAndDeserializeObject<ArtifactResult, ArtifactBase>(
                path, restRequestMethod, artifactToSave as ArtifactBase, expectedStatusCodes: expectedStatusCodes);

            if (artifactResult.ResultCode == HttpStatusCode.Created)
            {
                artifactToSave.IsSaved = true;
            }

            Logger.WriteDebug("POST {0} returned followings: Message: {1}, ResultCode: {2}", path, artifactResult.Message, artifactResult.ResultCode);
            Logger.WriteDebug("The Artifact Returned: {0}", artifactResult.Artifact);

            artifactToSave.Id = artifactResult.Artifact.Id;

            const string expectedMsg = "Success";
            Assert.That(artifactResult.Message == expectedMsg, "The returned Message was '{0}' but '{1}' was expected", artifactResult.Message, expectedMsg);

            Assert.That(artifactResult.ResultCode == HttpStatusCode.Created,
                "The returned ResultCode was '{0}' but '{1}' was expected",
                artifactResult.ResultCode, ((int)HttpStatusCode.Created).ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Discard the added artifact(s) from Blueprint
        /// </summary>
        /// <param name="artifactsToDiscard">The artifact(s) to be discarded.</param>
        /// <param name="address">The base url of the Open API</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The list of ArtifactResult objects created by the dicard artifacts request</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        public static List<IDiscardArtifactResult> DiscardArtifacts(List<IArtifactBase> artifactsToDiscard,
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

            return artifactResults.ConvertAll(o => (IDiscardArtifactResult)o);
        }

        /// <summary>
        /// Publish Artifact(s) (Used when publishing a single artifact OR a list of artifacts)
        /// </summary>
        /// <param name="artifactsToPublish">The list of artifacts to publish</param>
        /// <param name="address">The base url of the Open API</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="shouldKeepLock">(optional) Boolean parameter which defines whether or not to keep the lock after publishing the artfacts</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The list of PublishArtifactResult objects created by the publish artifacts request</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        public static List<IPublishArtifactResult> PublishArtifacts(List<IArtifactBase> artifactsToPublish,
            string address,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool shouldKeepLock = false,
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

            var artifactObjectList = (
                from IArtifactBase artifact in artifactsToPublish
                select new ArtifactBase(artifact.Address, artifact.Id, artifact.ProjectId)).ToList();

            RestApiFacade restApi = new RestApiFacade(address, user.Username, user.Password, tokenValue);
            var artifactResults = restApi.SendRequestAndDeserializeObject<List<PublishArtifactResult>, List<ArtifactBase>>(
                URL_PUBLISH, 
                RestRequestMethod.POST, 
                artifactObjectList, 
                additionalHeaders: additionalHeaders, 
                expectedStatusCodes: expectedStatusCodes);

            var publishedResultList = artifactResults.FindAll(result => result.ResultCode.Equals(HttpStatusCode.OK));

            // When each artifact is successfully published, set IsSaved flag to false since there are no longer saved changes
            foreach (var publishedResult in publishedResultList)
            {
                var publishedArtifact = artifactObjectList.Find(a => a.Id.Equals(publishedResult.ArtifactId));
                publishedArtifact.IsSaved = false;
                publishedArtifact.IsPublished = true;
                Logger.WriteDebug("Result Code for the Published Artifact {0}: {1}", publishedResult.ArtifactId, publishedResult.ResultCode);
            }

            Assert.That(publishedResultList.Count.Equals(artifactObjectList.Count),
                "The number of artifacts passed for Publish was {0} but the number of artifacts returned was {1}",
                artifactObjectList.Count, publishedResultList.Count);

            return artifactResults.ConvertAll(o => (IPublishArtifactResult)o);
        }

        public static List<IDeleteArtifactResult> DeleteArtifact(IArtifactBase artifactToDiscard, 
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false,
            bool deleteChildren = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactToDiscard, nameof(artifactToDiscard));

            string tokenValue = user.Token?.OpenApiToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            string path = I18NHelper.FormatInvariant("{0}/{1}/{2}/{3}", SVC_PATH, artifactToDiscard.ProjectId, URL_ARTIFACTS, artifactToDiscard.Id);

            if (deleteChildren)
            {
                path = I18NHelper.FormatInvariant("{0}?Recursively=True", path);
            }

            RestApiFacade restApi = new RestApiFacade(artifactToDiscard.Address, user.Username, user.Password, tokenValue);
            var artifactResults = restApi.SendRequestAndDeserializeObject<List<DeleteArtifactResult>>(
                path,
                RestRequestMethod.DELETE,
                expectedStatusCodes: expectedStatusCodes);

            foreach (var deletedArtifact in artifactResults)
            {
                Logger.WriteDebug("DELETE {0} returned following: ArtifactId: {1} Message: {2}, ResultCode: {3}", 
                    path, deletedArtifact.ArtifactId, deletedArtifact.Message, deletedArtifact.ResultCode);
            }

            return artifactResults.ConvertAll(o => (IDeleteArtifactResult)o);
        }

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
            var path = I18NHelper.FormatInvariant("{0}/{1}/{2}/{3}", SVC_PATH, artifact.ProjectId, URL_ARTIFACTS, artifact.Id);
            var returnedArtifact = restApi.SendRequestAndDeserializeObject<ArtifactBase>(
                resourcePath: path, 
                method: RestRequestMethod.GET, 
                expectedStatusCodes: expectedStatusCodes);

            return returnedArtifact.Version;
        }

        /// <summary>
        /// Get discussions for the specified artifact/subartifact
        /// </summary>
        /// <param name="address">server address</param>
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
        /// <param name="address">server address</param>
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