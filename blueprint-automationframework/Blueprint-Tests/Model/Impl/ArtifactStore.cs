using System;
using System.Collections.Generic;
using System.Net;
using Common;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Utilities;
using Utilities.Facades;

namespace Model.Impl
{
    public class ArtifactStore : NovaServiceBase, IArtifactStore
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The base URI of the ArtifactStore service.</param>
        public ArtifactStore(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            Address = address;
        }

        #region Members inherited from IArtifactStore

        /// <seealso cref="IArtifactStore.GetStatus(string, List{HttpStatusCode})"/>
        public string GetStatus(string preAuthorizedKey = CommonConstants.PreAuthorizedKeyForStatus, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetStatus(RestPaths.Svc.ArtifactStore.STATUS, preAuthorizedKey, expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.GetStatusUpcheck(List{HttpStatusCode})"/>
        public HttpStatusCode GetStatusUpcheck(List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetStatusUpcheck(RestPaths.Svc.ArtifactStore.Status.UPCHECK, expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.GetCustomArtifactTypes(IProject, IUser, List{HttpStatusCode})"/>
        public ProjectCustomArtifactTypesResult GetCustomArtifactTypes(IProject project, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));

            Logger.WriteInfo("Getting artifact types for project ID: {0}.", project.Id);

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Projects_id_.Meta.CUSTOM_TYPES, project.Id);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            var artifactTypes = restApi.SendRequestAndDeserializeObject<ProjectCustomArtifactTypesResult>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);

            // Print all returned types for debugging.
            foreach (var artifactType in artifactTypes.ArtifactTypes)
            {
                Logger.WriteDebug("*** Artifact Type - Name: '{0}', BaseType: '{1}', Prefix: '{2}'", artifactType.Name, artifactType.BaseType, artifactType.Prefix);
            }

            foreach (var artifactType in artifactTypes.SubArtifactTypes)
            {
                Logger.WriteDebug("*** Sub-Artifact Type - Name: '{0}', BaseType: '{1}', Prefix: '{2}'", artifactType.Name, artifactType.BaseType, artifactType.Prefix);
            }

            foreach (var propertyType in artifactTypes.PropertyTypes)
            {
                Logger.WriteDebug("*** Property Type - Name: '{0}', BaseType: '{1}'", propertyType.Name, propertyType.PrimitiveType.ToString());
            }

            return artifactTypes;
        }

        /// <seealso cref="IArtifactStore.GetArtifactChildrenByProjectAndArtifactId(int, int, IUser, List{HttpStatusCode})"/>
        public List<NovaArtifact> GetArtifactChildrenByProjectAndArtifactId(int projectId, int artifactId, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Projects_id_.Artifacts_id_.CHILDREN, projectId, artifactId);
            RestApiFacade restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            return restApi.SendRequestAndDeserializeObject<List<NovaArtifact>>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.GetProjectChildrenByProjectId(int, IUser, List{HttpStatusCode})"/>
        public List<NovaArtifact> GetProjectChildrenByProjectId(int id, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Projects_id_.CHILDREN, id);
            RestApiFacade restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            return restApi.SendRequestAndDeserializeObject<List<NovaArtifact>>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.GetArtifactDetails(IUser, int, int?, List{HttpStatusCode})"/>
        public NovaArtifactDetails GetArtifactDetails(IUser user, int artifactId, int? versionId = null, List<HttpStatusCode> expectedStatusCodes = null)
        { 
        string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.ARTIFACTS_id_, artifactId);
            RestApiFacade restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            Dictionary<string, string> queryParams = null;

            if (versionId != null)
            {
                queryParams = new Dictionary<string, string> { { "versionId", versionId.ToString() } };
            }

              return restApi.SendRequestAndDeserializeObject<NovaArtifactDetails>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.GetArtifactHistory(int, IUser, bool?, int?, int?, List{HttpStatusCode})"/>
        public List<ArtifactHistoryVersion> GetArtifactHistory(int artifactId, IUser user, 
            bool? sortByDateAsc = null, int? limit = null, int? offset = null, 
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.VERSION, artifactId);
            Dictionary<string, string> queryParameters = null;

            if ((sortByDateAsc != null) || (limit != null) || (offset != null))
            {
                queryParameters = new Dictionary<string, string>();

                if (sortByDateAsc != null)
                {
                    queryParameters.Add("asc", sortByDateAsc.ToString());
                }

                if (limit != null)
                {
                    queryParameters.Add("limit", limit.ToString());
                }

                if (offset != null)
                {
                    queryParameters.Add("offset", offset.ToString());
                }
            }

            RestApiFacade restApi = new RestApiFacade(Address, user.Token?.AccessControlToken);

            var artifactHistory = restApi.SendRequestAndDeserializeObject<ArtifactHistory>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes);

            return artifactHistory.ArtifactHistoryVersions;
        }

        /// <seealso cref="IArtifactStore.GetArtifactDiscussions(int, IUser, List{HttpStatusCode})"/>
        public Discussions GetArtifactDiscussions(int itemId, IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.DISCUSSIONS, itemId);
            var restApi = new RestApiFacade(Address, user.Token?.AccessControlToken);

            var artifactDiscussions = restApi.SendRequestAndDeserializeObject<Discussions>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes, shouldControlJsonChange: true);

            return artifactDiscussions;
        }

        /// <seealso cref="IArtifactStore.GetDiscussionsReplies(Comment, IUser, List{HttpStatusCode})"/>
        public List<Reply> GetDiscussionsReplies(Comment comment, IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(comment, nameof(comment));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.Discussions_id_.REPLIES, comment.ItemId, comment.DiscussionId);
            var restApi = new RestApiFacade(Address, user.Token?.AccessControlToken);

            var discussionReplies = restApi.SendRequestAndDeserializeObject<List<Reply>>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes, shouldControlJsonChange: true);

            return discussionReplies;
        }

        /// <seealso cref="IArtifactStore.GetAttachments(IArtifactBase, IUser, bool?, int?, List{HttpStatusCode})"/>
        public Attachments GetAttachments(IArtifactBase artifact, IUser user, bool? addDrafts = null, int? subArtifactId = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(user, nameof(user));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT, artifact.Id);
            Dictionary<string, string> queryParameters = new Dictionary<string, string>();

            if (addDrafts != null)
            {
                queryParameters.Add("addDrafts", addDrafts.ToString());
            }

            if (subArtifactId != null)
            {
                queryParameters.Add("subArtifactId", subArtifactId.ToString());
            }

            var restApi = new RestApiFacade(Address, user.Token?.AccessControlToken);

            var attachment = restApi.SendRequestAndDeserializeObject<Attachments>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes);

            return attachment;
        }

        /// <seealso cref="IArtifactStore.GetRelationships(IUser, IArtifactBase, int?, bool?, List{HttpStatusCode})"/>
        public Relationships GetRelationships(IUser user,
            IArtifactBase artifact,
            int? subArtifactId = null,
            bool? addDrafts = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.RELATIONSHIPS, artifact.Id);
            var queryParameters = new Dictionary<string, string>();

            if (subArtifactId != null)
            {
                queryParameters.Add("subArtifactId", subArtifactId.ToString());
            }

            if (addDrafts != null)
            {
                queryParameters.Add("addDrafts", addDrafts.ToString());
            }

            var restApi = new RestApiFacade(Address, user.Token?.AccessControlToken);

            var relationships = restApi.SendRequestAndDeserializeObject<Relationships>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes);

            return relationships;
        }

        /// <seealso cref="IArtifactStore.GetRelationshipsDetails(IUser, IArtifactBase, bool?, List{HttpStatusCode})"/>
        public TraceDetails GetRelationshipsDetails(IUser user,
            IArtifactBase artifact,
            bool? addDrafts = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.RELATIONSHIP_DETAILS, artifact.Id);
            var queryParameters = new Dictionary<string, string>();

            if (addDrafts != null)
            {
                queryParameters.Add("addDrafts", addDrafts.ToString());
            }

            var restApi = new RestApiFacade(Address, user.Token?.AccessControlToken);

            var traceDetails = restApi.SendRequestAndDeserializeObject<TraceDetails>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes);

            return traceDetails;
        }

        /*    Commented out because this is still in development.
        /// <summary>
        /// Save a single artifact to ArtifactStore.
        /// </summary>
        /// <param name="artifactToSave">The artifact to save.</param>
        /// <param name="user">The user saving the artifact.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false).</param>
        public static void PostArtifact(IArtifactBase artifactToSave,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactToSave, nameof(artifactToSave));

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { artifactToSave.Id == 0 ? HttpStatusCode.Created : HttpStatusCode.OK };
            }

            string tokenValue = user.Token?.AccessControlToken;
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.ARTIFACTS, artifactToSave.ProjectId);  // TODO: Update REST path to include projectID.

            RestApiFacade restApi = new RestApiFacade(artifactToSave.Address, tokenValue);

            var artifactResult = restApi.SendRequestAndDeserializeObject<UpdateArtifactResult, ArtifactBase>(
                path,
                RestRequestMethod.POST,
                artifactToSave as ArtifactBase,
                expectedStatusCodes: expectedStatusCodes);

            ReplacePropertiesWithPropertiesFromSourceArtifact(artifactResult.Artifact, artifactToSave);

            // Artifact was successfully created so IsSaved is set to true
            if (artifactResult.ResultCode == HttpStatusCode.Created)
            {
                artifactToSave.IsSaved = true;
            }

            Logger.WriteDebug("POST {0} returned the following: Message: {1}, ResultCode: {2}",
                path, artifactResult.Message, artifactResult.ResultCode);
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
            }
        }
        */

        #endregion Members inherited from IArtifactStore

        #region Members inherited from IDisposable

        private bool _isDisposed = false;

        /// <summary>
        /// Disposes this object by deleting all artifacts that were created.
        /// </summary>
        /// <param name="disposing">Pass true if explicitly disposing or false if called from the destructor.</param>
        protected virtual void Dispose(bool disposing)
        {
            Logger.WriteTrace("{0}.{1} called.", nameof(ArtifactStore), nameof(Dispose));

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
        /// Disposes this object by deleting all artifacts that were created.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion Members inherited from IDisposable
    }
}
