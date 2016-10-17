using System;
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
using Utilities.Factories;

namespace Model.ArtifactModel.Impl
{
    //TODO  Remove "sendAuthorizationAsCookie" since this does not apply to OpenAPI
    public class OpenApiArtifact : ArtifactBase, IOpenApiArtifact
    {
        #region Enums

        public enum ArtifactTraceType
        {
            None,
            All,
            Parent,
            Child,
            Manual,
            Reuse,
            Other
        }

        #endregion Enums

        #region Constructors

        /// <summary>
        /// Constructor needed to deserialize it as generic type.
        /// </summary>
        public OpenApiArtifact()
        {
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

        /// <seealso cref="IOpenApiArtifact.GetArtifact(IProject,IUser,Nullable{bool},Nullable{bool},System.Nullable{Model.ArtifactModel.Impl.OpenApiArtifact.ArtifactTraceType},Nullable{bool},Nullable{bool},Nullable{bool},Nullable{bool},System.Collections.Generic.List{System.Net.HttpStatusCode})"/>
        public IOpenApiArtifact GetArtifact(IProject project,
            IUser user,
            bool? getStatus = null,
            bool? getComments = null,
            ArtifactTraceType? getTraces = null,
            bool? getAttachments = null,
            bool? richTextAsPlain = null,
            bool? getInlineCSS = null,
            bool? getContent = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetArtifact(Address, project, Id, user,
                getAttachments: getAttachments,
                getComments: getComments,
                getContent: getContent,
                getInlineCSS: getInlineCSS,
                getStatus: getStatus,
                getTraces: getTraces,
                richTextAsPlain: richTextAsPlain);
        }

        /// <seealso cref="IOpenApiArtifact.GetVersion(IUser, List{HttpStatusCode}, bool)" />
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

        /// <seealso cref="IOpenApiArtifact.AddTrace(IUser, IArtifactBase, TraceDirection, TraceTypes, bool, int?, bool?, List{HttpStatusCode})" />
        public List<OpenApiTrace> AddTrace(IUser user,
            IArtifactBase targetArtifact,
            TraceDirection traceDirection,
            TraceTypes traceType = TraceTypes.Manual,
            bool isSuspect = false,
            int? subArtifactId = null,
            bool? reconcileWithTwoWay = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return AddTrace(Address, this, targetArtifact, traceDirection, user, traceType, isSuspect, subArtifactId, reconcileWithTwoWay, expectedStatusCodes);
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
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            string path = I18NHelper.FormatInvariant(RestPaths.OpenApi.Projects_id_.ARTIFACTS, artifactToSave.ProjectId);

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { artifactToSave.Id == 0 ? HttpStatusCode.Created : HttpStatusCode.OK };
            }

            if (restRequestMethod == RestRequestMethod.POST)
            {
                RestApiFacade restApi = new RestApiFacade(artifactToSave.Address, tokenValue);

                var artifactResult = restApi.SendRequestAndDeserializeObject<OpenApiAddArtifactResult, ArtifactBase>(
                    path,
                    restRequestMethod,
                    artifactToSave as ArtifactBase,
                    expectedStatusCodes: expectedStatusCodes);

                ReplacePropertiesWithPropertiesFromSourceArtifact(artifactResult.Artifact, artifactToSave);

                // Artifact was successfully created so IsSaved is set to true
                if (artifactResult.ResultCode == HttpStatusCode.Created)
                {
                    artifactToSave.IsSaved = true;
                }

                Logger.WriteDebug("{0} {1} returned the following: Message: {2}, ResultCode: {3}", restRequestMethod.ToString(), path, artifactResult.Message, artifactResult.ResultCode);
                Logger.WriteDebug("The Artifact Returned: {0}", artifactResult.Artifact);

                if (expectedStatusCodes.Contains(HttpStatusCode.OK) || expectedStatusCodes.Contains(HttpStatusCode.Created))
                {
                    Assert.That(artifactResult.ResultCode == HttpStatusCode.Created,
                        "The returned ResultCode was '{0}' but '{1}' was expected",
                        artifactResult.ResultCode,
                        ((int) HttpStatusCode.Created).ToString(CultureInfo.InvariantCulture));

                    Assert.That(artifactResult.Message == "Success",
                        "The returned Message was '{0}' but 'Success' was expected",
                        artifactResult.Message);

                    Assert.IsFalse(artifactResult.Artifact.Status.IsLocked, "Status.IsLocked should always be false for new artifacts!");
                    Assert.IsFalse(artifactResult.Artifact.Status.IsReadOnly, "Status.IsReadOnly should always be false for new artifacts!");
                }
            }
            else if (restRequestMethod == RestRequestMethod.PATCH)
            {
                UpdateArtifact(artifactToSave, user, expectedStatusCodes, sendAuthorizationAsCookie);
            }
            else
            {
                throw new InvalidOperationException("Only POST or PATCH methods are supported for saving artifacts!");
            }
        }

        /// <summary>
        /// Update an Artifact with Property Changes
        /// </summary>
        /// <param name="artifactToUpdate">The artifact to be updated</param>
        /// <param name="user">The user updating the artifact</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        public static void UpdateArtifact(IArtifactBase artifactToUpdate,
        IUser user,
        List<HttpStatusCode> expectedStatusCodes = null,
        bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactToUpdate, nameof(artifactToUpdate));

            Assert.That(artifactToUpdate.Id != 0, "Artifact Id cannot be 0 to perform an update.");

            string tokenValue = user.Token?.OpenApiToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            string path = I18NHelper.FormatInvariant(RestPaths.OpenApi.Projects_id_.ARTIFACTS, artifactToUpdate.ProjectId);

            //TODO: Remove this when solution to have the property to update configurable
            var propertyToUpdate = artifactToUpdate.Properties.First(p => p.Name == "Description");

            // TODO: Expand this to have the properties to update configurable
            // Create a copy of the artifact to update that only includes the properties to be updated
            var artifactWithPropertyToUpdate = new OpenApiArtifactForUpdate
            {
                Id = artifactToUpdate.Id,
                Properties = new List<OpenApiPropertyForUpdate>
                {
                    new OpenApiPropertyForUpdate
                    {
                        PropertyTypeId = propertyToUpdate.PropertyTypeId,
                        TextOrChoiceValue = "NewDescription_"+ RandomGenerator.RandomAlphaNumeric(5)
                    }
                }
            };

            var artifactsToUpdate = new List<OpenApiArtifactForUpdate> { artifactWithPropertyToUpdate };

            RestApiFacade restApi = new RestApiFacade(artifactToUpdate.Address, tokenValue);
            var updateResultList = restApi.SendRequestAndDeserializeObject<List<OpenApiUpdateArtifactResult>, List<OpenApiArtifactForUpdate>>(
                path, RestRequestMethod.PATCH, artifactsToUpdate, expectedStatusCodes: expectedStatusCodes);

            Assert.IsNotEmpty(updateResultList, "No artifact results were returned");
            Assert.That(updateResultList.Count == 1, "Only a single artifact was updated, but multiple artifact results were returned");

            // Get the updated artifact from the result list
            var updateResult = updateResultList.Find(a => a.ArtifactId == artifactToUpdate.Id);

            if (updateResult.ResultCode == HttpStatusCode.OK)
            {
                Logger.WriteDebug("Result Code for the Saved Artifact {0}: {1}, Message: {2}", updateResult.ArtifactId, updateResult.ResultCode, updateResult.Message);

                // Copy updated property into original artifact
                propertyToUpdate.TextOrChoiceValue = artifactWithPropertyToUpdate.Properties.First(p => p.PropertyTypeId == propertyToUpdate.PropertyTypeId).TextOrChoiceValue;

                artifactToUpdate.IsSaved = true;

                Assert.AreEqual("Success", updateResult.Message,
                        "The returned Message was '{0}' but 'Success' was expected", updateResult.Message);
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
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            // TODO Why do we need to make copies of artifacts here?  Add comment

            var artifactObjectList = artifactsToDiscard.Select(artifact =>
                new ArtifactBase(artifact.Address, artifact.Id, artifact.ProjectId)).ToList();

            RestApiFacade restApi = new RestApiFacade(address, tokenValue);

            var artifactResults = restApi.SendRequestAndDeserializeObject<List<DiscardArtifactResult>, List<ArtifactBase>>(
                RestPaths.OpenApi.VersionControl.DISCARD,
                RestRequestMethod.POST,
                artifactObjectList,
                expectedStatusCodes: expectedStatusCodes);

            var discardedResultList = artifactResults.FindAll(result => result.ResultCode.Equals(HttpStatusCode.OK));

            // When each artifact is successfully discarded, set IsSaved & IsMarkedForDeletion flags to false.
            foreach (var discardedResult in discardedResultList)
            {
                var discardedArtifact = artifactObjectList.Find(a => a.Id.Equals(discardedResult.ArtifactId));
                discardedArtifact.IsSaved = false;
                discardedArtifact.IsMarkedForDeletion = false;

                Logger.WriteDebug("Result Code for the Discarded Artifact {0}: {1}", discardedResult.ArtifactId, discardedResult.ResultCode);
            }

            Assert.That(discardedResultList.Count.Equals(artifactObjectList.Count),
                "The number of artifacts passed for Discard was {0} but the number of artifacts returned was {1}",
                artifactObjectList.Count, discardedResultList.Count);

            return artifactResults.ConvertAll(o => (DiscardArtifactResult)o);
        }

        /// <summary>
        /// Retrieves a single artifact by Project ID and Artifact ID and returns information about the artifact.
        /// (Runs:  /api/v1/projects/{projectId}/artifacts/{artifactId}  with the following optional query parameters:
        /// status={status}, comments={comments}, traces={traces}, attachments={attachments}, richtextasplain={richtextasplain}, inlinecss={inlinecss}, content={content})
        /// </summary>
        /// <param name="baseAddress">The base address of the Blueprint server.</param>
        /// <param name="project">The project where the artifact exists.</param>
        /// <param name="artifactId">The ID of the artifact.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="getStatus">(optional) Defines whether or not the status of the artifact should be loaded.  The default is false if not specified.
        /// The default is true if the parameter is included in the URI with no given value.</param>
        /// <param name="getComments">(optional) Indicates whether to retreive comments of the artifact.  The default is false if not specified.
        /// The default is true if the parameter is included in the URI with no given value.</param>
        /// <param name="getTraces">(optional) Indicates whether to retrieve traces of the artifact.  The default is None if not specified.
        /// The default is All if the parameter is included in the URI with no given value.</param>
        /// <param name="getAttachments">(optional) Indicates whether to retrieve information about the attachments of the artifact.  The default is false if not specified.
        /// The default is true if the parameter is included in the URI with no given value.</param>
        /// <param name="richTextAsPlain">(optional) Defines whether or not to retrieve all rich-text properties as Plain Text instead of HTML.  The default is false if not specified.
        /// The default is true if the parameter is included in the URI with no given value.</param>
        /// <param name="getInlineCSS">(optional) Defines whether or not to retrieve all rich-text properties with locally defined or inline styles.  The default is false if not specified.
        /// The default is true if the parameter is included in the URI with no given value.  When this parameter is set to false, rich-text properties return internal styles that are defined
        /// within the &lt;head&gt; section of the HTML.</param>
        /// <param name="getContent">(optional) Defines whether or not to retrieve the artifact's content.  The default is false if not specified.
        /// Defines whether or not to retrieve the artifact's content. This parameter can be set to true or false. The default is false if not specified. The default is true if the parameter is included in the URI with no given value.
        /// The default is true if the parameter is included in the URI with no given value.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only 200 OK is expected.</param>
        /// <returns>The artifact with all the additional details you requested.</returns>
        public static IOpenApiArtifact GetArtifact(string baseAddress,
            IProject project,
            int artifactId,
            IUser user,
            bool? getStatus = null,
            bool? getComments = null,
            ArtifactTraceType? getTraces = null,
            bool? getAttachments = null,
            bool? richTextAsPlain = null,
            bool? getInlineCSS = null,
            bool? getContent = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(baseAddress, nameof(baseAddress));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            var queryParameters = new Dictionary<string, string>();

            if (getAttachments != null) { queryParameters.Add("Attachments", getAttachments.ToString()); }
            if (getComments != null) { queryParameters.Add("Comments", getComments.ToString()); }
            if (getContent != null) { queryParameters.Add("Content", getContent.ToString()); }
            if (getInlineCSS != null) { queryParameters.Add("InlineCSS", getInlineCSS.ToString()); }
            if (getStatus != null) { queryParameters.Add("Status", getStatus.ToString()); }
            if (getTraces != null) { queryParameters.Add("Traces", getTraces.ToString()); }
            if (richTextAsPlain != null) { queryParameters.Add("RichTextAsPlain", richTextAsPlain.ToString()); }

            RestApiFacade restApi = new RestApiFacade(baseAddress, user.Token?.OpenApiToken);
            var path = I18NHelper.FormatInvariant(RestPaths.OpenApi.Projects_id_.ARTIFACTS_id_, project.Id, artifactId);

            var returnedArtifact = restApi.SendRequestAndDeserializeObject<OpenApiArtifact>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);

            return returnedArtifact;
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
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            RestApiFacade restApi = new RestApiFacade(artifact.Address, tokenValue);
            var path = I18NHelper.FormatInvariant(RestPaths.OpenApi.Projects_id_.ARTIFACTS_id_, artifact.ProjectId, artifact.Id);

            var returnedArtifact = restApi.SendRequestAndDeserializeObject<ArtifactBase>(
                path,
                RestRequestMethod.GET,
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
        /// <returns>RaptorDiscussion for artifact/subartifact</returns>
        public static IRaptorDiscussion GetRaptorDiscussions(string address,
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
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            var queryParameters = new Dictionary<string, string> {
                { "includeDraft", includeDraft.ToString() }
            };

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.RapidReview.Artifacts_id_.DISCUSSIONS, itemId);
            var restApi = new RestApiFacade(address, tokenValue);

            var response = restApi.SendRequestAndDeserializeObject<RaptorDiscussion>(
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
            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            var queryParameters = new Dictionary<string, string> {
                { "name", searchSubstring }
            };

            if (project != null)
            {
                queryParameters.Add("projectId", I18NHelper.ToStringInvariant(project.Id));
            }

            //showBusyIndicator doesn't affect server side, it is added to make call similar to call from HTML
            queryParameters.Add("showBusyIndicator", "false");

            var restApi = new RestApiFacade(address, tokenValue);

            var response = restApi.SendRequestAndDeserializeObject<List<ArtifactBase>>(
                RestPaths.Svc.Shared.Artifacts.SEARCH,
                RestRequestMethod.GET,
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies);
            Logger.WriteDebug("Response for search artifact by name: {0}", response);

            return response.ConvertAll(o => (IArtifactBase)o);
        }

        /// <summary>
        /// POST discussion for the specified artifact.
        /// (Runs: /svc/components/RapidReview/artifacts/{artifactId}/discussions)
        /// </summary>
        /// <param name="address">The base url of the Open API</param>
        /// <param name="itemId">id of artifact</param>
        /// <param name="discussionText">text for new comment</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>RaptorDiscussion for artifact/subartifact</returns>
        public static IRaptorComment PostRaptorDiscussion(string address, int itemId, 
            string discussionText, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.AccessControlToken;
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.RapidReview.Artifacts_id_.DISCUSSIONS, itemId);
            var restApi = new RestApiFacade(address, tokenValue);

            var response = restApi.SendRequestAndGetResponse<string>(path, RestRequestMethod.POST,
                bodyObject: discussionText, expectedStatusCodes: expectedStatusCodes);
            
            // Derialization
            var result = JsonConvert.DeserializeObject<RaptorComment>(response.Content);

            return result;
        }

        /// <summary>
        /// Updates the specified comment.
        /// (Runs: PATCH /svc/components/RapidReview/artifacts/{itemId}/discussions/{discussionId})
        /// </summary>
        /// <param name="address">The base url of the Open API</param>
        /// <param name="itemId">id of artifact</param>
        /// <param name="commentToUpdate">comment to update</param>
        /// <param name="discussionText">new text for discussion</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>updated RaptorDiscussion</returns>
        public static IRaptorComment UpdateRaptorDiscussion(string address, int itemId, IRaptorComment commentToUpdate,
            string discussionText, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(commentToUpdate, nameof(commentToUpdate));

            string tokenValue = user.Token?.AccessControlToken;
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.RapidReview.Artifacts_id_.Discussions_id_.COMMENT, itemId, commentToUpdate.DiscussionId);
            var restApi = new RestApiFacade(address, tokenValue);

            var response = restApi.SendRequestAndGetResponse<string>(path, RestRequestMethod.PATCH,
                bodyObject: discussionText, expectedStatusCodes: expectedStatusCodes);

            // Derialization
            var result = JsonConvert.DeserializeObject<RaptorComment>(response.Content);

            return result;
        }

        /// <summary>
        /// Deletes the specified comment.
        /// (Runs: POST /svc/components/RapidReview/artifacts/{itemId}/deletethread/{discussionId})
        /// </summary>
        /// <param name="address">The base url of the Open API</param>
        /// <param name="itemId">id of artifact</param>
        /// <param name="commentToDelete">comment to update</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>message</returns>
        public static string DeleteRaptorDiscussion(string address, int itemId, IRaptorComment commentToDelete,
            IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(commentToDelete, nameof(commentToDelete));

            string tokenValue = user.Token?.AccessControlToken;
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.RapidReview.Artifacts_id_.DELETE_THREAD_ID, itemId, commentToDelete.DiscussionId);
            var restApi = new RestApiFacade(address, tokenValue);

            var response = restApi.SendRequestAndGetResponse<string>(path, RestRequestMethod.DELETE,
                expectedStatusCodes: expectedStatusCodes);

            // Derialization
            var resultMessage = JsonConvert.DeserializeObject<string>(response.Content);

            return resultMessage;
        }

        /// <summary>
        /// POST reply for the specified discussion
        /// </summary>
        /// <param name="address">The base url of the Blueprint server</param>
        /// <param name="comment">Comment to reply</param>
        /// <param name="discussionText">Text for replying</param>
        /// <param name="user">The user to authenticate with</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>Newly created RaptorReply for artifact/subartifact comment</returns>
        public static IRaptorReply PostRaptorDiscussionReply(string address,
            IRaptorComment comment, string discussionText, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(comment, nameof(comment));

            string tokenValue = user.Token?.AccessControlToken;
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.RapidReview.Artifacts_id_.Discussions_id_.REPLY, comment.ItemId, comment.DiscussionId);
            var restApi = new RestApiFacade(address, tokenValue);

            var response = restApi.SendRequestAndGetResponse<string>(path, RestRequestMethod.POST,
                bodyObject: discussionText, expectedStatusCodes: expectedStatusCodes);

            // Derialization
            var result = JsonConvert.DeserializeObject<RaptorReply>(response.Content);

            return result;
        }

        /// <summary>
        /// Updates the specified reply.
        /// (Runs: PATCH /svc/components/RapidReview/artifacts/{itemId}/discussions/{discussionId}/reply/{replyId})
        /// </summary>
        /// <param name="address">The base url of the Open API</param>
        /// <param name="itemId">id of artifact/subartifact</param>
        /// <param name="comment">comment containing reply to update</param>
        /// <param name="replyToUpdate">reply to update</param>
        /// <param name="discussionText">new text for discussion</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>updated RaptorDiscussion</returns>
        public static IRaptorReply UpdateRaptorDiscussionReply(string address, int itemId, IRaptorComment comment,
            IRaptorReply replyToUpdate, string discussionText, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(comment, nameof(comment));
            ThrowIf.ArgumentNull(replyToUpdate, nameof(replyToUpdate));

            string tokenValue = user.Token?.AccessControlToken;
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.RapidReview.Artifacts_id_.Discussions_id_.REPLY_ID,
                itemId, comment.DiscussionId, replyToUpdate.ReplyId);
            var restApi = new RestApiFacade(address, tokenValue);

            var response = restApi.SendRequestAndGetResponse<string>(path, RestRequestMethod.PATCH,
                bodyObject: discussionText, expectedStatusCodes: expectedStatusCodes);

            // Derialization
            var result = JsonConvert.DeserializeObject<RaptorReply>(response.Content);

            return result;
        }

        /// <summary>
        /// Deletes the specified reply.
        /// (Runs: POST /svc/components/RapidReview/artifacts/{itemId}/deletecomment/{replyId})
        /// </summary>
        /// <param name="address">The base url of the Open API</param>
        /// <param name="itemId">id of artifact</param>
        /// <param name="replyToDelete">comment to update</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>message</returns>
        public static string DeleteRaptorReply(string address, int itemId, IRaptorReply replyToDelete,
            IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(replyToDelete, nameof(replyToDelete));

            string tokenValue = user.Token?.AccessControlToken;
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.RapidReview.Artifacts_id_.DELETE_COMMENT_ID, itemId, replyToDelete.ReplyId);
            var restApi = new RestApiFacade(address, tokenValue);

            var response = restApi.SendRequestAndGetResponse<string>(path, RestRequestMethod.DELETE,
                expectedStatusCodes: expectedStatusCodes);

            // Derialization
            var resultMessage = JsonConvert.DeserializeObject<string>(response.Content);

            return resultMessage;
        }

        /// <summary>
        /// add attachment to the specified artifact
        /// </summary>
        /// <param name="address">The base url of the Blueprint server</param>
        /// <param name="projectId">Id of project containing artifact to add attachment</param>
        /// <param name="artifactId">Id of artifact to add attachment</param>
        /// <param name="file">File to attach</param>
        /// <param name="user">The user to authenticate with</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only '201' is expected.</param>
        /// <returns>OpenApiAttachment object</returns>
        public static OpenApiAttachment AddArtifactAttachment(string address,
            int projectId, int artifactId, IFile file, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(file, nameof(file));

            string path = I18NHelper.FormatInvariant(RestPaths.OpenApi.Projects_id_.Artifacts_id_.ATTACHMENTS, projectId, artifactId);

            return AddItemAttachment(address, path, file, user, expectedStatusCodes);
        }

        /// <summary>
        /// add attachment to the specified sub-artifact
        /// </summary>
        /// <param name="address">The base url of the Blueprint server</param>
        /// <param name="projectId">Id of project containing artifact to add attachment</param>
        /// <param name="artifactId">Id of artifact to add attachment</param>
        /// <param name="subArtifactId">Id of subartifact to attach file</param>
        /// <param name="file">File to attach</param>
        /// <param name="user">The user to authenticate with</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only '201' is expected.</param>
        /// <returns>OpenApiAttachment object</returns>
        public static OpenApiAttachment AddSubArtifactAttachment(string address,
            int projectId, int artifactId, int subArtifactId, IFile file, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(file, nameof(file));

            string path = I18NHelper.FormatInvariant(RestPaths.OpenApi.Projects_id_.Artifacts_id_.SubArtifacts_id_.ATTACHMENTS,
                projectId, artifactId, subArtifactId);

            return AddItemAttachment(address, path, file, user, expectedStatusCodes);
        }

        /// <summary>
        /// add attachment to the specified artifact/subartifact
        /// </summary>
        /// <param name="address">The base url of the Blueprint server</param>
        /// <param name="path">Path to add attachment</param>
        /// <param name="file">File to attach</param>
        /// <param name="user">The user to authenticate with</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only '201' is expected.</param>
        /// <returns>OpenApiAttachment object</returns>
        private static OpenApiAttachment AddItemAttachment(string address,
            string path, IFile file, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(file, nameof(file));

            string tokenValue = user.Token?.OpenApiToken;
            var restApi = new RestApiFacade(address, tokenValue);
            var additionalHeaders = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(file.FileType))
            {
                additionalHeaders.Add("Content-Type", file.FileType);
            }

            if (!string.IsNullOrEmpty(file.FileName))
            {
                additionalHeaders.Add("Content-Disposition",
                    I18NHelper.FormatInvariant("form-data; name=attachment; filename=\"{0}\"",
                        System.Web.HttpUtility.UrlPathEncode(file.FileName)));
            }

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            }

            var response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.POST,
                fileName: file.FileName,
                fileContent: file.Content.ToArray(),
                contentType: file.FileType,
                additionalHeaders: additionalHeaders,
                expectedStatusCodes: expectedStatusCodes);

            return JsonConvert.DeserializeObject<OpenApiAttachment>(response.Content);
        }

        /// <summary>
        /// Add trace between two artifacts (or artifact and sub-artifact) with specified properties.
        /// </summary>
        /// <param name="address">The base URL of the Blueprint server.</param>
        /// <param name="sourceArtifact">The first artifact to which the call adds a trace.</param>
        /// <param name="targetArtifact">The second artifact to which the call adds a trace.</param>
        /// <param name="traceDirection">The direction of the trace 'To', 'From', 'Both'.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="traceType">(optional) The type of the trace - default is: 'Manual'.</param>
        /// <param name="isSuspect">(optional) Should trace be marked as suspected.</param>
        /// <param name="subArtifactId">(optional) The ID of a sub-artifact of the target artifact to which the trace should be added.</param>
        /// <param name="reconcileWithTwoWay">(optional) Indicates how to handle the existence of an inverse trace.  If set to true, and an inverse trace already exists,
        ///   the request does not return an error; instead, the trace Type is set to TwoWay.  The default is null and acts the same as false.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only '201' is expected.</param>
        /// <returns>List of OpenApiTrace objects for all traces that were added.</returns>
        public static List<OpenApiTrace> AddTrace(string address,
            IArtifactBase sourceArtifact,
            IArtifactBase targetArtifact,   // TODO: Create an AddTrace() that takes a list of target artifacts.
            TraceDirection traceDirection,
            IUser user,
            TraceTypes traceType = TraceTypes.Manual,
            bool isSuspect = false,
            int? subArtifactId = null,
            bool? reconcileWithTwoWay = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(sourceArtifact, nameof(sourceArtifact));
            ThrowIf.ArgumentNull(targetArtifact, nameof(targetArtifact));

            string tokenValue = user.Token?.OpenApiToken;
            string path = I18NHelper.FormatInvariant(RestPaths.OpenApi.Projects_id_.Artifacts_id_.TRACES,
                sourceArtifact.ProjectId, sourceArtifact.Id);

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            }

            Dictionary<string, string> queryParameters = null;

            if (reconcileWithTwoWay != null)
            {
                queryParameters = new Dictionary<string, string> { {"reconcilewithtwoway", reconcileWithTwoWay.ToString() } };
            }

            OpenApiTrace traceToCreate = new OpenApiTrace(targetArtifact.ProjectId, targetArtifact,
                traceDirection, traceType, isSuspect, subArtifactId);

            var restApi = new RestApiFacade(address, tokenValue);

            var openApiTraces = restApi.SendRequestAndDeserializeObject<List<OpenApiTrace>, List<OpenApiTrace>>(
                path,
                RestRequestMethod.POST,
                new List<OpenApiTrace> { traceToCreate },
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes);

            if (expectedStatusCodes.Contains(HttpStatusCode.Created))
            {
                Assert.AreEqual(1, openApiTraces.Count);
                Assert.AreEqual((int)HttpStatusCode.Created, openApiTraces[0].ResultCode);

                string traceCreatedMessage = I18NHelper.FormatInvariant("Trace between {0} and {1} added successfully.",
                    sourceArtifact.Id, subArtifactId ?? targetArtifact.Id);

                Assert.AreEqual(traceCreatedMessage, openApiTraces[0].Message);
            }

            return openApiTraces;
        }

        /// <summary>
        /// Delete trace between two artifacts (or artifact and sub-artifact) with specified properties.
        /// </summary>
        /// <param name="address">The base URL of the Blueprint server.</param>
        /// <param name="sourceArtifact">The first artifact to which the call deletes a trace.</param>
        /// <param name="targetArtifact">The second artifact to which the call deletes a trace.</param>
        /// <param name="traceDirection">The direction of the trace 'To', 'From', 'Both'.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="traceType">(optional) The type of the trace - default is: 'Manual'.</param>
        /// <param name="isSuspect">(optional) Should trace be marked as suspected.</param>
        /// <param name="subArtifactId">(optional) The ID of a sub-artifact of the target artifact to which the trace should be deleted.</param>
        /// <param name="reconcileWithTwoWay">(optional) Indicates how to handle the existence of an inverse trace.  If set to true, and an inverse trace already exists,
        ///   the request does not return an error; instead, the trace Type is set to TwoWay.  The default is null and acts the same as false.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only '201' is expected.</param>
        /// <returns>List of OpenApiTrace objects for all traces that were deleted.</returns>
        public static List<OpenApiTrace> DeleteTrace(string address,
            IArtifactBase sourceArtifact,
            IArtifactBase targetArtifact,   // TODO: Create an DeleteTrace() that takes a list of target artifacts.
            TraceDirection traceDirection,
            IUser user,
            TraceTypes traceType = TraceTypes.Manual,
            bool isSuspect = false,
            int? subArtifactId = null,
            bool? reconcileWithTwoWay = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(sourceArtifact, nameof(sourceArtifact));
            ThrowIf.ArgumentNull(targetArtifact, nameof(targetArtifact));

            string tokenValue = user.Token?.OpenApiToken;
            string path = I18NHelper.FormatInvariant(RestPaths.OpenApi.Projects_id_.Artifacts_id_.TRACES,
                sourceArtifact.ProjectId, sourceArtifact.Id);

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            }

            Dictionary<string, string> queryParameters = null;

            if (reconcileWithTwoWay != null)
            {
                queryParameters = new Dictionary<string, string> { { "reconcilewithtwoway", reconcileWithTwoWay.ToString() } };
            }

            OpenApiTrace traceToDelete = new OpenApiTrace(targetArtifact.ProjectId, targetArtifact,
                traceDirection, traceType, isSuspect, subArtifactId);

            var restApi = new RestApiFacade(address, tokenValue);

            var openApiTraces = restApi.SendRequestAndDeserializeObject<List<OpenApiTrace>, List<OpenApiTrace>>(
                path,
                RestRequestMethod.DELETE,
                new List<OpenApiTrace> { traceToDelete },
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes);

            if (expectedStatusCodes.Contains(HttpStatusCode.OK))
            {
                Assert.AreEqual(1, openApiTraces.Count);
                Assert.AreEqual((int)HttpStatusCode.OK, openApiTraces[0].ResultCode);

                string traceDeletedMessage = I18NHelper.FormatInvariant("Trace has been successfully deleted.");

                Assert.AreEqual(traceDeletedMessage, openApiTraces[0].Message);
            }

            return openApiTraces;
        }

        #endregion Static Methods
    }

    public class OpenApiArtifactForUpdate
    {
        public int Id { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public List<OpenApiPropertyForUpdate> Properties { get; set; }
    }

    public class OpenApiPropertyForUpdate
    {
        public int PropertyTypeId { get; set; }
        public string TextOrChoiceValue { get; set; }
    }
}
