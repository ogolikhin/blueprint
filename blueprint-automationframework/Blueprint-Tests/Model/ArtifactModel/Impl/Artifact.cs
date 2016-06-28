using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using Utilities;
using NUnit.Framework;
using Utilities.Facades;
using Common;
using Model.Impl;

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

            foreach (var discardArtifactResult in discardArtifactResults)
            {
                if (discardArtifactResult.ResultCode == HttpStatusCode.OK)
                {
                    IsSaved = false;
                }
            }

            return discardArtifactResults;
        }

        public List<NovaDiscardArtifactResult> NovaDiscard(IUser user = null,
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

            foreach (var discardArtifactResult in discardArtifactResults)
            {
                if (discardArtifactResult.Result == NovaDiscardArtifactResult.ResultCode.Success)
                {
                    IsSaved = false;
                }
            }

            return discardArtifactResults;
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
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            RestApiFacade restApi = new RestApiFacade(Address, tokenValue);
            var path = I18NHelper.FormatInvariant("{0}/{1}", URL_ARTIFACT_INFO, Id);
            var returnedArtifactInfo = restApi.SendRequestAndDeserializeObject<ArtifactInfo>(
                resourcePath: path, method: RestRequestMethod.GET, expectedStatusCodes: expectedStatusCodes);

            return returnedArtifactInfo;
        }

        public RapidReviewDiagram GetDiagramContentForRapidReview(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform GetDiagramContentForRapidReview.");
                user = CreatedBy;
            }

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            var artifactInfo = GetArtifactInfo(user: user);
            string path;
            RestApiFacade restApi = new RestApiFacade(Address, tokenValue);
            switch (artifactInfo.BaseTypePredefined)
            {
                case ItemTypePredefined.BusinessProcess:
                case ItemTypePredefined.DomainDiagram:
                case ItemTypePredefined.GenericDiagram:
                case ItemTypePredefined.Storyboard:
                case ItemTypePredefined.UseCaseDiagram:
                case ItemTypePredefined.UIMockup:
                    path = I18NHelper.FormatInvariant("{0}/{1}", URL_DIAGRAM, Id);
                    break;
                default:
                    throw new ArgumentException("Method works for graphical artifacts only.");
            }
            var diagramContent = restApi.SendRequestAndDeserializeObject<RapidReviewDiagram>(resourcePath: path,
                method: RestRequestMethod.GET, expectedStatusCodes: expectedStatusCodes);
            return diagramContent;
        }

        public RapidReviewUseCase GetUseCaseContentForRapidReview(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform GetUseCaseContentForRapidReview.");
                user = CreatedBy;
            }

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            var artifactInfo = GetArtifactInfo(user: user);
            string path;
            RestApiFacade restApi = new RestApiFacade(Address, tokenValue);
            if (artifactInfo.BaseTypePredefined == ItemTypePredefined.UseCase)
            {
                path = I18NHelper.FormatInvariant("{0}/{1}", URL_USECASE, Id);
                var returnedArtifactContent = restApi.SendRequestAndDeserializeObject<RapidReviewUseCase>(resourcePath: path,
                    method: RestRequestMethod.GET, expectedStatusCodes: expectedStatusCodes);
                return returnedArtifactContent;
            }
            else
            {
                throw new ArgumentException("Method works for UseCase artifacts only.");
            }
        }

        public RapidReviewGlossary GetGlossaryContentForRapidReview(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform GetGlossaryContentForRapidReview.");
                user = CreatedBy;
            }

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            var artifactInfo = GetArtifactInfo(user: user);
            string path;
            RestApiFacade restApi = new RestApiFacade(Address, tokenValue);
            if (artifactInfo.BaseTypePredefined == ItemTypePredefined.Glossary)
            {
                path = I18NHelper.FormatInvariant("{0}/{1}", URL_GLOSSARY, Id);
                var returnedArtifactContent = restApi.SendRequestAndDeserializeObject<RapidReviewGlossary>(resourcePath: path,
                    method: RestRequestMethod.GET, expectedStatusCodes: expectedStatusCodes);
                return returnedArtifactContent;
            }
            else
            {
                throw new ArgumentException("Method works for Glossary artifacts only.");
            }
        }

        public RapidReviewProperties GetPropertiesForRapidReview(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform GetPropertiesForRapidReview.");
                user = CreatedBy;
            }

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            string path = URL_ARTIFACTPROPERTIES;
            var artifactIds = new List<int> { Id };
            RestApiFacade restApi = new RestApiFacade(Address, tokenValue);
            var returnedArtifactProperties = restApi.SendRequestAndDeserializeObject<List<RapidReviewProperties>, List<int>>(resourcePath: path,
                method: RestRequestMethod.POST, jsonObject: artifactIds, expectedStatusCodes: expectedStatusCodes);
            return returnedArtifactProperties[0];
        }

        public NovaPublishArtifactResult NovaPublish(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            return PublishArtifact(artifactToPublish: this, user: user, expectedStatusCodes: expectedStatusCodes,
                sendAuthorizationAsCookie: sendAuthorizationAsCookie);
        }

        public IRaptorComment PostRaptorDiscussions(string discussionsText,
            IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            return PostRaptorDiscussions(Address, Id, discussionsText, user, expectedStatusCodes);
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
        public static List<NovaDiscardArtifactResult> NovaDiscardArtifacts(List<IArtifactBase> artifactsToDiscard,
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
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            RestApiFacade restApi = new RestApiFacade(address, tokenValue);

            var artifactsIds = artifactsToDiscard.Select(artifact => artifact.Id).ToList();
            var artifactResults = restApi.SendRequestAndDeserializeObject<NovaDiscardArtifactResults, List<int>>(
                URL_NOVADISCARD,
                RestRequestMethod.POST,
                artifactsIds,
                expectedStatusCodes: expectedStatusCodes);

            var discardedResultList = artifactResults.DiscardResults;

            // When each artifact is successfully discarded, set IsSaved flag to false
            foreach (var discardedResult in discardedResultList)
            {
                var discardedArtifact = artifactsToDiscard.Find(a => a.Id.Equals(discardedResult.ArtifactId) &&
                    discardedResult.Result == NovaDiscardArtifactResult.ResultCode.Success);

                Logger.WriteDebug("Result Code for the Discarded Artifact {0}: {1}", discardedResult.ArtifactId, (int)discardedResult.Result);

                if (discardedArtifact != null)
                {
                discardedArtifact.IsSaved = false;
                }
            }

            Assert.That(discardedResultList.Count.Equals(artifactsToDiscard.Count),
                "The number of artifacts passed for Discard was {0} but the number of artifacts returned was {1}",
                artifactsToDiscard.Count, discardedResultList.Count);

            return discardedResultList;
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
        /// <returns>RaptorDiscussion for artifact/subartifact</returns>
        public static IRaptorDiscussion GetRaptorDiscussions(string address,
            int itemId,
            bool includeDraft,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            return OpenApiArtifact.GetRaptorDiscussions(
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
        /// <param name="project">The project to search, if project is null search within all available projects.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.</param>
        /// <returns>List of first 10 artifacts with name containing searchSubstring</returns>
        public static IList<IArtifactBase> SearchArtifactsByName(string address,
            IUser user,
            string searchSubstring,
            IProject project = null,
            bool sendAuthorizationAsCookie = false,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return OpenApiArtifact.SearchArtifactsByName(
                address,
                user,
                searchSubstring,
                project,
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
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            var artifactIds = (
                from IArtifactBase artifact in artifactsToLock
                select artifact.Id).ToList();

            var restApi = new RestApiFacade(address, tokenValue);

            var response = restApi.SendRequestAndDeserializeObject<List<LockResultInfo>, List<int>>(
                URL_LOCK,
                RestRequestMethod.POST,
                jsonObject: artifactIds,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies);

            return response;
        }

        /// <summary>
        /// Publish a single artifact on Blueprint server
        /// </summary>
        /// <param name="artifactToPublish">The artifact to publish</param>
        /// <param name="user">The user saving the artifact</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>Resut of Publish operation</returns>
        ///TODO: override the Publish() function in the Artifact class to call this PublishArtifact() function
        public static NovaPublishArtifactResult PublishArtifact(IArtifactBase artifactToPublish,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactToPublish, nameof(artifactToPublish));
            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            const string path = "/svc/shared/artifacts/publish";
            RestApiFacade restApi = new RestApiFacade(artifactToPublish.Address, tokenValue);

            var publishResults = restApi.SendRequestAndDeserializeObject<List<NovaPublishArtifactResult>, List<int>>(path, RestRequestMethod.POST,
                new List<int> { artifactToPublish.Id },
                expectedStatusCodes: expectedStatusCodes);

            if (publishResults[0].StatusCode == NovaPublishArtifactResult.Result.Success)
            {
                artifactToPublish.IsPublished = true;
                artifactToPublish.IsSaved = false;
            }
            return publishResults[0];
        }

        /// <summary>
        /// Creates new discussion for the specified artifact/subartifact using Raptor REST API
        /// </summary>
        /// <param name="address">The base url of the API</param>
        /// <param name="itemId">id of artifact/subartifact</param>
        /// <param name="discussionsText">text for the new discussion</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>Newly created RaptorComment for artifact/subartifact</returns>
        public static IRaptorComment PostRaptorDiscussions(string address,
            int itemId,
            string discussionsText,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return OpenApiArtifact.PostRaptorDiscussion(address, itemId, discussionsText,
                user, expectedStatusCodes);
        }

        /// <summary>
        /// Creates new discussion for the specified artifact/subartifact using Raptor REST API
        /// </summary>
        /// <param name="address">The base url of the Blueprint</param>
        /// <param name="comment">Comment to which we want reply</param>
        /// <param name="replyText">text for the new reply</param>
        /// <param name="user">The user credentials to authenticate with</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>Newly created RaptorReply for artifact/subartifact comment</returns>
        public static IRaptorReply PostRaptorDiscussionReply(string address,
            IRaptorComment comment,
            string replyText,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return OpenApiArtifact.PostRaptorDiscussionReply(address, comment, replyText,
                user, expectedStatusCodes);
        }

        #endregion Static Methods
    }
}
