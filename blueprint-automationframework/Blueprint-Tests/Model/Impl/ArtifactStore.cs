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
        public List<Artifact> GetArtifactChildrenByProjectAndArtifactId(int projectId, int artifactId, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Projects_id_.Artifacts_id_.CHILDREN, projectId, artifactId);
            RestApiFacade restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            return restApi.SendRequestAndDeserializeObject<List<Artifact>>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.GetProjectChildrenByProjectId(int, IUser, List{HttpStatusCode})"/>
        public List<Artifact> GetProjectChildrenByProjectId(int id, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Projects_id_.CHILDREN, id);
            RestApiFacade restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            return restApi.SendRequestAndDeserializeObject<List<Artifact>>(
                path,
                RestRequestMethod.GET,
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
                expectedStatusCodes: expectedStatusCodes);

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
                expectedStatusCodes: expectedStatusCodes);

            return discussionReplies;
        }

        /// <seealso cref="IArtifactStore.GetItemsAttachment(int, IUser, bool?, List{HttpStatusCode})"/>
        public Attachment GetItemsAttachment(int itemId, IUser user, bool? addDrafts = true,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT, itemId);
            Dictionary<string, string> queryParameters = null;

            if (addDrafts != null)
            {
                queryParameters = new Dictionary<string, string> { { "addDrafts", addDrafts.ToString() } };
            }

            var restApi = new RestApiFacade(Address, user.Token?.AccessControlToken);

            var attachment = restApi.SendRequestAndDeserializeObject<Attachment>(
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
            var restApi = new RestApiFacade(Address, user.Token?.AccessControlToken);

            var relationships = restApi.SendRequestAndDeserializeObject<Relationships>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);

            return relationships;
        }

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
