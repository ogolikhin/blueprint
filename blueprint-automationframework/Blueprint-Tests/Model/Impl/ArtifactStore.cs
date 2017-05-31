using Common;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.ArtifactModel.Impl.OperationsResults;
using Model.Factories;
using Model.NovaModel.Impl;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Web;
using Model.NovaModel.Reviews;
using Utilities;
using Utilities.Facades;

namespace Model.Impl
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]    // TODO: Maybe refactor later.
    public class ArtifactStore : NovaServiceBase, IArtifactStore
    {
        private IUser _userForFiles = null;

        public List<IFileMetadata> Files { get; } = new List<IFileMetadata>();

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

        /// <seealso cref="IArtifactStore.AddImage(IUser, IFile, List{HttpStatusCode})"/>
        public EmbeddedImageFile AddImage(IUser user, IFile imageFile, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var addedImage = AddImage(Address, user, imageFile, expectedStatusCodes);

            // TODO: Properly manage dispose
            //Files.Add(addedImage);

            // We'll use this user in Dispose() to delete the files.
            if (_userForFiles == null)
            {
                _userForFiles = user;
            }

            return addedImage;
        }

        /// <seealso cref="IArtifactStore.GetImage(IUser, string, List{HttpStatusCode})"/>
        public EmbeddedImageFile GetImage(IUser user, string embeddedImageId, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetImage(Address, user, embeddedImageId, expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.CopyArtifact(IArtifactBase, IArtifactBase, IUser, double?, List{HttpStatusCode})"/>
        public CopyNovaArtifactResultSet CopyArtifact(
            IArtifactBase artifact,
            IArtifactBase newParent,
            IUser user,
            double? orderIndex = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            ThrowIf.ArgumentNull(newParent, nameof(newParent));

            return CopyArtifact(Address, artifact.Id, newParent.Id, user, orderIndex, expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.CreateArtifact(IUser, ArtifactTypePredefined, string, IProject, IArtifactBase, double?, List{HttpStatusCode})"/>
        public INovaArtifactDetails CreateArtifact(IUser user,
            ArtifactTypePredefined baseArtifactType,
            string name,
            IProject project,
            IArtifactBase parentArtifact = null,
            double? orderIndex = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return CreateArtifact(Address, user, (ItemTypePredefined)baseArtifactType, name, project, parentArtifact?.Id, orderIndex, expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.CreateArtifact(IUser, ArtifactTypePredefined, string, IProject, INovaArtifactDetails, double?, List{HttpStatusCode})"/>
        public INovaArtifactDetails CreateArtifact(IUser user,
            ArtifactTypePredefined baseArtifactType,
            string name,
            IProject project,
            INovaArtifactDetails parentArtifact = null,
            double? orderIndex = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return CreateArtifact(Address, user, (ItemTypePredefined)baseArtifactType, name, project, parentArtifact?.Id, orderIndex, expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.CreateArtifact(IUser, ItemTypePredefined, string, IProject, INovaArtifactBase, double?, List{HttpStatusCode})"/>
        public INovaArtifactDetails CreateArtifact(IUser user,
            ItemTypePredefined baseArtifactType,
            string name,
            IProject project,
            INovaArtifactBase parentArtifact = null,
            double? orderIndex = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return CreateArtifact(Address, user, baseArtifactType, name, project, parentArtifact?.Id, orderIndex, expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.UpdateArtifact(IUser, INovaArtifactDetails, List{HttpStatusCode})"/>
        public INovaArtifactDetails UpdateArtifact(IUser user,
            INovaArtifactDetails novaArtifactDetails,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return UpdateArtifact(Address, user, novaArtifactDetails);
        }

        /// <seealso cref="IArtifactStore.DeleteArtifact(IArtifactBase, IUser, List{HttpStatusCode})"/>
        public List<INovaArtifactDetails> DeleteArtifact(IArtifactBase artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            var deletedArtifacts = DeleteArtifact(artifact.Id, user, expectedStatusCodes);

            // Set the IsMarkedForDeletion flag for the artifact that we deleted so the Dispose() works properly.
            foreach (var deletedArtifact in deletedArtifacts)
            {
                Logger.WriteDebug("DeleteArtifact() returned following artifact Id: {0}", deletedArtifact.Id);

                // Hack: This is needed until we can refactor ArtifactBase better.
                var deletedArtifactResult = new OpenApiDeleteArtifactResult
                {
                    ArtifactId = deletedArtifact.Id,
                    ResultCode = HttpStatusCode.OK
                };

                // Add all other artifacts that were deleted as a result of the artifact being deleted.
                var artifaceBaseToDelete = artifact as ArtifactBase;
                artifaceBaseToDelete.DeletedArtifactResults.Add(deletedArtifactResult);

                if (deletedArtifact.Id == artifact.Id)
                {
                    // If the artifact was published, it will require another publish to really delete the artifact.
                    // If the artifact was never published, no other users can see it, so deleting it will permanently delete it.
                    if (artifact.IsPublished)
                    {
                        artifact.IsMarkedForDeletion = true;
                    }
                    else
                    {
                        artifact.IsDeleted = true;
                    }
                }
            }

            return deletedArtifacts;
        }

        /// <seealso cref="IArtifactStore.DeleteArtifact(int, IUser, List{HttpStatusCode})"/>
        public List<INovaArtifactDetails> DeleteArtifact(int artifactId, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.ARTIFACTS_id_, artifactId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            var response = restApi.SendRequestAndDeserializeObject<List<NovaArtifactResponse>>(
                path,
                RestRequestMethod.DELETE,
                expectedStatusCodes: expectedStatusCodes);

            return response?.ConvertAll(o => (INovaArtifactDetails)o);
        }

        /// <seealso cref="IArtifactStore.DiscardAllArtifacts(IUser)"/>
        public INovaArtifactsAndProjectsResponse DiscardAllArtifacts(IUser user)
        {
            return DiscardArtifacts(user, artifactIds: null, all: true);
        }

        /// <seealso cref="IArtifactStore.DiscardArtifact(IUser, int)"/>
        public INovaArtifactsAndProjectsResponse DiscardArtifact(IUser user, int artifactId)
        {
            var artifacts = new List<int> { artifactId };
            return DiscardArtifacts(user, artifacts);
        }

        /// <seealso cref="IArtifactStore.DiscardArtifacts(IUser, IEnumerable{int}, bool?)"/>
        public INovaArtifactsAndProjectsResponse DiscardArtifacts(IUser user, IEnumerable<int> artifactIds, bool? all = null)
        {
            const string path = RestPaths.Svc.ArtifactStore.Artifacts.DISCARD;
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            Dictionary<string, string> queryParams = null;
            if (all != null)
            {
                queryParams = new Dictionary<string, string> { { "all", all.Value.ToString() } };
            }

            return restApi.SendRequestAndDeserializeObject<NovaArtifactsAndProjectsResponse, IEnumerable<int>>(
                path,
                RestRequestMethod.POST,
                artifactIds,
                queryParameters: queryParams);
        }

        /// <seealso cref="IArtifactStore.GetStatus(string, List{HttpStatusCode})"/>
        public string GetStatus(string preAuthorizedKey = null, List<HttpStatusCode> expectedStatusCodes = null)
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
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);

            // Print all returned types for debugging.
            foreach (var artifactType in artifactTypes.ArtifactTypes)
            {
                Logger.WriteDebug("*** Artifact Type - Name: '{0}', BaseType: '{1}', Prefix: '{2}'", artifactType.Name, artifactType.PredefinedType, artifactType.Prefix);
            }

            foreach (var artifactType in artifactTypes.SubArtifactTypes)
            {
                Logger.WriteDebug("*** Sub-Artifact Type - Name: '{0}', BaseType: '{1}', Prefix: '{2}'", artifactType.Name, artifactType.PredefinedType, artifactType.Prefix);
            }

            foreach (var propertyType in artifactTypes.PropertyTypes)
            {
                Logger.WriteDebug("*** Property Type - Name: '{0}', BaseType: '{1}'", propertyType.Name, propertyType.PrimitiveType.ToString());
            }

            return artifactTypes;
        }

        /// <seealso cref="IArtifactStore.GetArtifactChildrenByProjectAndArtifactId(int, int, IUser)"/>
        public List<INovaArtifact> GetArtifactChildrenByProjectAndArtifactId(int projectId, int artifactId, IUser user)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Projects_id_.Artifacts_id_.CHILDREN, projectId, artifactId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            var children = restApi.SendRequestAndDeserializeObject<List<NovaArtifact>>(
                path,
                RestRequestMethod.GET,
                shouldControlJsonChanges: false);

            return children.ConvertAll(a => (INovaArtifact) a);
        }

        /// <seealso cref="IArtifactStore.GetProjectChildrenByProjectId(int, IUser)"/>
        public List<INovaArtifact> GetProjectChildrenByProjectId(int id, IUser user)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Projects_id_.CHILDREN, id);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            var artifacts = restApi.SendRequestAndDeserializeObject<List<NovaArtifact>>(
                path,
                RestRequestMethod.GET,
                shouldControlJsonChanges: false);

            return artifacts.ConvertAll(a => (INovaArtifact) a);
        }

        /// <seealso cref="IArtifactStore.GetExpandedArtifactTree(IUser, IProject, int, bool?, List{HttpStatusCode})"/>
        public List<INovaArtifact> GetExpandedArtifactTree(IUser user,
            IProject project,
            int artifactId,
            bool? includeChildren = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Projects_id_.ARTIFACTS_id_, project.Id, artifactId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            Dictionary<string, string> queryParams = null;

            if (includeChildren != null)
            {
                queryParams = new Dictionary<string, string> { { "includeChildren", includeChildren.Value.ToString() } };
            }

            var novaArtifacts = restApi.SendRequestAndDeserializeObject<List<NovaArtifact>>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);

            var ret = novaArtifacts.ConvertAll(o => (INovaArtifact)o);
            return ret;
        }

        /// <seealso cref="IArtifactStore.GetArtifactDetails(IUser, int, int?, List{HttpStatusCode})"/>
        public NovaArtifactDetails GetArtifactDetails(IUser user, int artifactId, int? versionId = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.ARTIFACTS_id_, artifactId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            Dictionary<string, string> queryParams = null;

            if (versionId != null)
            {
                queryParams = new Dictionary<string, string> { { "versionId", versionId.ToString() } };
            }

            var response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.GET, queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes);

            var artifactDetails = JsonConvert.DeserializeObject<NovaArtifactDetails>(response.Content);

            SerializationUtilities.CheckJson(artifactDetails, response.Content);

            Assert.IsNotNull(artifactDetails.PredefinedType, "PredefinedType shouldn't be null.");

            return ArtifactFactory.ConvertToSpecificArtifact(artifactDetails, response.Content);
        }

        /// <seealso cref="IArtifactStore.GetDiagramArtifact(IUser, int, int?, List{HttpStatusCode})"/>
        public NovaDiagramArtifact GetDiagramArtifact(IUser user, int artifactId, int? versionId = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.DIAGRAM_id_, artifactId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            Dictionary<string, string> queryParams = null;

            if (versionId != null)
            {
                queryParams = new Dictionary<string, string> { { "versionId", versionId.ToString() } };
            }

            return restApi.SendRequestAndDeserializeObject<NovaDiagramArtifact>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);
        }

        /// <seealso cref="IArtifactStore.GetGlossaryArtifact(IUser, int, int?, List{HttpStatusCode})"/>
        public NovaGlossaryArtifact GetGlossaryArtifact(IUser user, int artifactId, int? versionId = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.GLOSSARY_id_, artifactId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            Dictionary<string, string> queryParams = null;

            if (versionId != null)
            {
                queryParams = new Dictionary<string, string> { { "versionId", versionId.ToString() } };
            }

            return restApi.SendRequestAndDeserializeObject<NovaGlossaryArtifact>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);
        }

        /// <seealso cref="IArtifactStore.GetUseCaseArtifact(IUser, int, int?, List{HttpStatusCode})"/>
        public NovaUseCaseArtifact GetUseCaseArtifact(IUser user, int artifactId, int? versionId = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.USECASE_id_, artifactId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            Dictionary<string, string> queryParams = null;

            if (versionId != null)
            {
                queryParams = new Dictionary<string, string> { { "versionId", versionId.ToString() } };
            }

            return restApi.SendRequestAndDeserializeObject<NovaUseCaseArtifact>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);
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

            var restApi = new RestApiFacade(Address, user.Token?.AccessControlToken);

            var artifactHistory = restApi.SendRequestAndDeserializeObject<ArtifactHistory>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes);

            return artifactHistory.ArtifactHistoryVersions;
        }

        /// <seealso cref="IArtifactStore.GetArtifactDiscussions(int, IUser, List{HttpStatusCode})"/>
        public DiscussionResultSet GetArtifactDiscussions(int itemId, IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.DISCUSSIONS, itemId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            var artifactDiscussions = restApi.SendRequestAndDeserializeObject<DiscussionResultSet>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes, shouldControlJsonChanges: true);

            return artifactDiscussions;
        }

        /// <seealso cref="IArtifactStore.GetDiscussionsReplies(Discussion, IUser, List{HttpStatusCode})"/>
        public List<Reply> GetDiscussionsReplies(Discussion comment, IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(comment, nameof(comment));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.Discussions_id_.REPLIES, comment.ItemId, comment.DiscussionId);
            var restApi = new RestApiFacade(Address, user.Token?.AccessControlToken);

            var discussionReplies = restApi.SendRequestAndDeserializeObject<List<Reply>>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes, shouldControlJsonChanges: true);

            return discussionReplies;
        }

        /// <seealso cref="IArtifactStore.GetAttachments(IUser, int, bool?, int?, int?, int?)"/>
        public Attachments GetAttachments(IUser user, int artifactId, bool? addDrafts = null, int? versionId = null,
            int? baselineId = null, int? subArtifactId = null)
        {
            return GetAttachments(Address, artifactId, user, addDrafts, versionId, baselineId, subArtifactId);
        }

        /// <seealso cref="IArtifactStore.GetAttachments(IArtifactBase, IUser, bool?, int?, int?, int?, List{HttpStatusCode})"/>
        public Attachments GetAttachments(IArtifactBase artifact, IUser user, bool? addDrafts = null,
            int? versionId = null, int? baselineId = null, int? subArtifactId = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            
            return GetAttachments(Address, artifact.Id, user, addDrafts, versionId, subArtifactId, baselineId,
                expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.GetRelationships(IUser, IArtifactBase, int?, bool?, int?, int?, List{HttpStatusCode})"/>
        public Relationships GetRelationships(IUser user,
            IArtifactBase artifact,
            int? subArtifactId = null,
            bool? addDrafts = null,
            int? versionId = null,
            int? baselineId = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            return GetRelationships(user, artifact.Id, subArtifactId, addDrafts, versionId, baselineId, expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.GetRelationships(IUser, int, int?, bool?, int?, int?, List{HttpStatusCode})"/>
        public Relationships GetRelationships(
            IUser user,
            int artifactId,
            int? subArtifactId = null,
            bool? addDrafts = null,
            int? versionId = null,
            int? baselineId = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.RELATIONSHIPS, artifactId);
            var queryParameters = new Dictionary<string, string>();

            if (subArtifactId != null)
            {
                queryParameters.Add("subArtifactId", subArtifactId.ToString());
            }

            if (addDrafts != null)
            {
                queryParameters.Add("addDrafts", addDrafts.ToString());
            }

            if (versionId != null)
            {
                queryParameters.Add("versionId", versionId.ToString());
            }

            if (baselineId != null)
            {
                queryParameters.Add("baselineId", baselineId.ToString());
            }

            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            var relationships = restApi.SendRequestAndDeserializeObject<Relationships>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);

            return relationships;
        }

        /// <seealso cref="IArtifactStore.GetRelationshipsDetails(IUser, int})"/>
        public TraceDetails GetRelationshipsDetails(
            IUser user,
            int artifactId)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.RELATIONSHIP_DETAILS, artifactId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            var traceDetails = restApi.SendRequestAndDeserializeObject<TraceDetails>(
                path,
                RestRequestMethod.GET,
                shouldControlJsonChanges: true);

            return traceDetails;
        }

        /// <seealso cref="IArtifactStore.GetSubartifacts(IUser, int, List{HttpStatusCode})"/>
        public List<SubArtifact> GetSubartifacts(IUser user, int artifactId, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetSubartifacts(Address, user, artifactId, expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.GetSubartifact(IUser, int, int, List{HttpStatusCode})"/>
        public NovaSubArtifact GetSubartifact(IUser user, int artifactId, int subArtifactId, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetSubartifact(Address, user, artifactId, subArtifactId, expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.GetUnpublishedChanges(IUser, List{HttpStatusCode})"/>
        public INovaArtifactsAndProjectsResponse GetUnpublishedChanges(IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetUnpublishedChanges(Address, user, expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.GetVersionControlInfo(IUser, int, int?, List{HttpStatusCode})"/>
        public INovaVersionControlArtifactInfo GetVersionControlInfo(IUser user, int itemId, int? baselineId = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts.VERSION_CONTROL_INFO_id_, itemId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            var queryParameters = new Dictionary<string, string>();

            if (baselineId != null)
            {
                queryParameters.Add("baselineId", baselineId.ToString());
            }

            var artifactBaseInfo = restApi.SendRequestAndDeserializeObject<NovaVersionControlArtifactInfo>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes,
                queryParameters: queryParameters,
                shouldControlJsonChanges: true);

            return artifactBaseInfo;
        }

        /// <seealso cref="IArtifactStore.MoveArtifact(IArtifactBase, IArtifactBase, IUser, double?)"/>
        public INovaArtifactDetails MoveArtifact(IArtifactBase artifact,
            IArtifactBase newParent,
            IUser user = null,
            double? orderIndex = null)
        {
            ThrowIf.ArgumentNull(newParent, nameof(newParent));

            return MoveArtifact(artifact, newParent.Id, user, orderIndex);
        }

        /// <seealso cref="IArtifactStore.MoveArtifact(IArtifactBase, int, IUser, double?)"/>
        public INovaArtifactDetails MoveArtifact(
            IArtifactBase artifact,
            int newParentId,
            IUser user,
            double? orderIndex = null)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            var movedArtifact = MoveArtifact(user, artifact.Id, newParentId, orderIndex);

            artifact.IsSaved = true;

            return movedArtifact;
        }

        /// <seealso cref="IArtifactStore.MoveArtifact(IUser, int, int, double?)"/>
        public INovaArtifactDetails MoveArtifact(
            IUser user,
            int artifactId,
            int newParentId,
            double? orderIndex = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.MOVE_TO_id_, artifactId, newParentId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            Dictionary<string, string> queryParams = null;

            if (orderIndex != null)
            {
                queryParams = new Dictionary<string, string> { { "orderIndex", orderIndex.Value.ToStringInvariant() } };
            }

            var movedArtifact = restApi.SendRequestAndDeserializeObject<NovaArtifactDetails>(
                path,
                RestRequestMethod.POST,
                queryParameters: queryParams);

            return movedArtifact;
        }

        /// <seealso cref="IArtifactStore.PublishArtifact(IArtifactBase, IUser, List{HttpStatusCode})"/>
        public INovaArtifactsAndProjectsResponse PublishArtifact(IArtifactBase artifact,
            IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            var artifacts = new List<IArtifactBase> { artifact };

            return PublishArtifacts(artifacts, user, expectedStatusCodes: expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.PublishArtifact(int, IUser, List{HttpStatusCode})"/>
        public INovaArtifactsAndProjectsResponse PublishArtifact(int artifactId,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            var artifacts = new List<int> { artifactId };

            return PublishArtifacts(artifacts, user, expectedStatusCodes: expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.GetNavigationPath(IUser, int, List{HttpStatusCode})"/>
        public List<INovaVersionControlArtifactInfo> GetNavigationPath(IUser user, int itemId, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.NAVIGATION_PATH, itemId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            var artifactBaseInfo = restApi.SendRequestAndDeserializeObject<List<NovaVersionControlArtifactInfo>>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: true);

            return artifactBaseInfo.ConvertAll(o => (INovaVersionControlArtifactInfo)o);
        }

        /// <seealso cref="IArtifactStore.GetAttachmentFile(IUser, int, int, int?, List{HttpStatusCode})"/>
        public IFile GetAttachmentFile(IUser user, int itemId, int fileId, int? versionId = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(itemId, nameof(itemId));
            ThrowIf.ArgumentNull(fileId, nameof(fileId));
            ThrowIf.ArgumentNull(user, nameof(user));

            File file = null;

            string tokenValue = user.Token?.AccessControlToken;
            
            var restApi = new RestApiFacade(Address, tokenValue);
            var path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT_id_, itemId, fileId);

            Dictionary<string, string> queryParams = null;

            if (versionId != null)
            {
                queryParams = new Dictionary<string, string> { { "versionId", versionId.ToString() } };
            }

            var response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.GET, queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes);

            // TODO: implementation copied from FileStore.cs - fix after call will be implemented
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string filename = HttpUtility.UrlDecode(new ContentDisposition(
                            response.Headers.First(h => h.Key == "Content-Disposition").Value.ToString()).FileName);

                file = new File
                {
                    Content = response.RawBytes.ToArray(),
                    LastModifiedDate =
                    DateTime.ParseExact(response.Headers.First(h => h.Key == "Date").Value.ToString(), "r", 
                            null), //'r' allow parse "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'"
                    FileType = response.ContentType,
                    FileName = filename
                };
            }

            return file;
        }

        /// <seealso cref="IArtifactStore.GetCollection(IUser, int, List{HttpStatusCode})"/>
        public Collection GetCollection(IUser user, int collectionId, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.COLLECTION_id_, collectionId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            var collection = restApi.SendRequestAndDeserializeObject<Collection>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: true);

            return collection;
        }

        /// <seealso cref="IArtifactStore.AddArtifactToCollection(IUser, int, int, bool, List{HttpStatusCode})"/>
        public AddToCollectionResult AddArtifactToCollection(IUser user, int artifactId, int collectionId, bool includeDescendants = false,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Collections_id_.CONTENT, collectionId);
            return AddArtifactToBaselineOrCollection<AddToCollectionResult>(user, artifactId, path, includeDescendants, expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.GetActorIcon(IUser, int, int?, List{HttpStatusCode})"/>
        public IFile GetActorIcon(IUser user, int actorArtifactId, int? versionId = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.ACTORICON_id_, actorArtifactId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            var queryParams = new Dictionary<string, string>{{"versionId", versionId?.ToStringInvariant() ??
                string.Empty}};

            queryParams.Add("addDraft", "true");

            File file = null;

            var response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.GET, expectedStatusCodes: expectedStatusCodes);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                file = new File
                {
                    Content = response.RawBytes.ToArray(),
                    FileType = response.ContentType
                };
            }

            return file;
        }

        /// <seealso cref="IArtifactStore.GetBaseline(IUser, int, List{HttpStatusCode})"/>
        public Baseline GetBaseline(IUser user, int baselineId, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.BASELINE_id_, baselineId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            var baseline = restApi.SendRequestAndDeserializeObject<Baseline>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: true);

            return baseline;
        }

        /// <seealso cref="IArtifactStore.AddArtifactToBaseline(IUser, int, int, bool, List{HttpStatusCode})"/>
        public AddToBaselineResult AddArtifactToBaseline(IUser user, int artifactId, int baselineId, bool includeDescendants = false,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            //
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Baselines_id_.CONTENT, baselineId);
            return AddArtifactToBaselineOrCollection<AddToBaselineResult>(user, artifactId, path, includeDescendants, expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.GetArtifactHistory(int, IUser, bool?, int?, int?, List{HttpStatusCode})"/>
        public List<AuthorHistoryItem> GetArtifactsAuthorHistory(List<int> artifactIds,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = RestPaths.Svc.ArtifactStore.Artifacts.AUTHOR_HISTORIES;
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            var historyItems = restApi.SendRequestAndDeserializeObject<List<AuthorHistoryItem>, List<int>>(path,
                RestRequestMethod.POST, artifactIds, expectedStatusCodes: expectedStatusCodes);

            return historyItems;
        }

        /// <seealso cref="IArtifactStore.PublishArtifacts(IEnumerable{int}, IUser, bool?, List{HttpStatusCode})"/>
        public NovaArtifactsAndProjectsResponse PublishArtifacts(
            IEnumerable<int> artifactIds,
            IUser user,
            bool? publishAll = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            const string path = RestPaths.Svc.ArtifactStore.Artifacts.PUBLISH;
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);
            Dictionary<string, string> queryParams = null;

            if (publishAll != null)
            {
                queryParams = new Dictionary<string, string> { { "all", publishAll.Value.ToString() } };
            }

            var publishedArtifacts = restApi.SendRequestAndDeserializeObject<NovaArtifactsAndProjectsResponse, IEnumerable<int>>(
                path,
                RestRequestMethod.POST,
                artifactIds,
                queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: true);

            return publishedArtifacts;
        }

        /// <seealso cref="IArtifactStore.PublishAllArtifacts(IUser))"/>
        public NovaArtifactsAndProjectsResponse PublishAllArtifacts(IUser user)
        {
            return PublishArtifacts(artifactIds: null, user: user, publishAll: true);
        }

        /// <seealso cref="IArtifactStore.PublishArtifacts(List{IArtifactBase}, IUser, bool?, List{HttpStatusCode})"/>
        public INovaArtifactsAndProjectsResponse PublishArtifacts(List<IArtifactBase> artifacts, IUser user = null,
            bool? publishAll = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            artifacts = artifacts ?? new List<IArtifactBase>();

            var publishedArtifacts = PublishArtifacts(artifacts.Select(artifact => artifact.Id), user, publishAll);

            if ((expectedStatusCodes == null) || expectedStatusCodes.Contains(HttpStatusCode.OK))
            {
                var deletedArtifactsList = new List<IArtifactBase>();
                var otherPublishedArtifactsList = new List<INovaArtifactDetails>();

                // Set the IsPublished... flags for the artifact that we deleted so the Dispose() works properly.
                foreach (var publishedArtifactDetails in publishedArtifacts.Artifacts)
                {
                    Logger.WriteDebug("'Publish Artifacts' returned following artifact Id: {0}", publishedArtifactDetails.Id);

                    var publishedArtifact = artifacts.Find(a => a.Id == publishedArtifactDetails.Id);

                    if (publishedArtifact == null)
                    {
                        otherPublishedArtifactsList.Add(publishedArtifactDetails);
                        continue;
                    }

                    publishedArtifact.LockOwner = null;
                    publishedArtifact.IsSaved = false;

                    // If the artifact was marked for deletion, then this publish operation actually deleted the artifact.
                    if (publishedArtifact.IsMarkedForDeletion)
                    {
                        deletedArtifactsList.Add(publishedArtifact);

                        publishedArtifact.IsPublished = false;
                        publishedArtifact.IsDeleted = true;
                    }
                    else
                    {
                        publishedArtifact.IsPublished = true;
                    }
                }

                if (deletedArtifactsList.Any())
                {
                    deletedArtifactsList[0]?.NotifyArtifactDeleted(deletedArtifactsList);
                }

                if (otherPublishedArtifactsList.Any())
                {
                    if (artifacts.Any())
                    {
                        artifacts[0]?.NotifyArtifactPublished(otherPublishedArtifactsList);
                    }

                    Assert.That((publishAll != null) && (publishAll.Value == true),
                        "An artifact that wasn't explicitly passed was published but the 'all=true' parameter wasn't passed!");
                }
            }

            return publishedArtifacts;
        }

        /// <seealso cref="IArtifactStore.GetReviews(int, IUser, List{HttpStatusCode})"/>
        public ReviewRelationshipsResultSet GetReviews(int artifactId, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.REVIEWS, artifactId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            return restApi.SendRequestAndDeserializeObject<ReviewRelationshipsResultSet>(path, RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes, shouldControlJsonChanges: true);
        }

        /// <seealso cref="IArtifactStore.GetBaselineInfo(List{int}, IUser)"/>
        public List<BaselineInfo> GetBaselineInfo(List<int> baselineIds, IUser user)
        {
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            return restApi.SendRequestAndDeserializeObject<List<BaselineInfo>, List<int>>(
                RestPaths.Svc.ArtifactStore.Artifacts.BASELINE_INFO, RestRequestMethod.POST,
                baselineIds, shouldControlJsonChanges: true);
        }

        /// <seealso cref="IArtifactStore.GetReviewArtifacts(IUser, int, int?, int?, int?)"/>
        public ReviewContent GetReviewArtifacts(IUser user, int reviewId, int? offset = 0, int? limit = 50,
            int? versionId = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Reviews_id_.CONTENT, reviewId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            var queryParams = new Dictionary<string, string>();

            if (offset != null)
            {
                queryParams.Add("offset", offset.ToString());
            }

            if (limit != null)
            {
                queryParams.Add("limit", limit.ToString());
            }

            if (versionId != null)
            {
                queryParams.Add("versionId", versionId.ToString());
            }

            return restApi.SendRequestAndDeserializeObject<ReviewContent>(path, RestRequestMethod.GET,
                queryParameters: queryParams, shouldControlJsonChanges: true);
        }

        /// <seealso cref="IArtifactStore.GetReviewContainer(IUser, int, int, int?, int?)"/>
        public ReviewSummary GetReviewContainer(IUser user, int reviewId, int revisionId, int? page = null, int? recordsOnPage = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Containers_id_.TOC, reviewId, revisionId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            var queryParams = new Dictionary<string, string>();

            if (page != null)
            {
                queryParams.Add("offset", page.ToString());
            }

            if (recordsOnPage != null)
            {
                queryParams.Add("limit", recordsOnPage.ToString());
            }

            return restApi.SendRequestAndDeserializeObject<ReviewSummary>(path,
                RestRequestMethod.GET, shouldControlJsonChanges: true);
        }

        /// <seealso cref="IArtifactStore.GetReviewParticipants(IUser, int, int?, int?, int?)"/>
        public ReviewParticipantsContent GetReviewParticipants(IUser user, int reviewId, int? offset = 0, int? limit = 50,
            int? versionId = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Containers_id_.PARTICIPANTS, reviewId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);
            var queryParams = new Dictionary<string, string>();

            if (offset != null)
            {
                queryParams.Add("offset", offset.ToString());
            }

            if (limit != null)
            {
                queryParams.Add("limit", limit.ToString());
            }

            if (versionId != null)
            {
                queryParams.Add("versionId", versionId.ToString());
            }

            return restApi.SendRequestAndDeserializeObject<ReviewParticipantsContent>(path, RestRequestMethod.GET,
                queryParameters: queryParams, shouldControlJsonChanges: true);
        }

        /// <seealso cref="IArtifactStore.GetArtifactStatusesByParticipant(IUser, int, int, int?, int?, int?)"/>
        public ArtifactReviewContent GetArtifactStatusesByParticipant(IUser user, int artifactId, int reviewId,
            int? offset = 0, int? limit = 50, int? versionId = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Containers_id_.ARTIFACT_REVIEWERS, reviewId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);
            var queryParams = new Dictionary<string, string> {{"artifactId", artifactId.ToStringInvariant()}};

            if (offset != null)
            {
                queryParams.Add("offset", offset.ToString());
            }

            if (limit != null)
            {
                queryParams.Add("limit", limit.ToString());
            }

            if (versionId != null)
            {
                queryParams.Add("versionId", versionId.ToString());
            }

            return restApi.SendRequestAndDeserializeObject<ArtifactReviewContent>(path, RestRequestMethod.GET,
                queryParameters: queryParams, shouldControlJsonChanges: true);
        }

        /// <seealso cref="IArtifactStore.AddArtifactsToReview(IUser, int, AddArtifactsParameter)"/>
        public AddArtifactsResult AddArtifactsToReview(IUser user, int reviewId, AddArtifactsParameter content)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Reviews_id_.CONTENT, reviewId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);
            return restApi.SendRequestAndDeserializeObject<AddArtifactsResult, AddArtifactsParameter>(path,
                RestRequestMethod.PUT, content, shouldControlJsonChanges: true);
        }

        #region Process methods

        /// <seealso cref="IArtifactStore.GetNovaProcess(IUser, int, int?, List{HttpStatusCode})"/>
        public INovaProcess GetNovaProcess(
            IUser user,
            int artifactId,
            int? versionIndex = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(ArtifactStore), nameof(GetNovaProcess));

            ThrowIf.ArgumentNull(user, nameof(user));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.PROCESS_id_, artifactId);
            var queryParameters = new Dictionary<string, string>();

            if (versionIndex.HasValue)
            {
                queryParameters.Add("versionId", versionIndex.ToString());
            }

            Logger.WriteInfo("{0} Getting the Process with artifact ID: {1}", nameof(ArtifactStore), artifactId);

            var restApi = new RestApiFacade(Address, user.Token?.AccessControlToken);
            var response = restApi.SendRequestAndDeserializeObject<NovaProcess>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);

            return response;
        }

        /// <seealso cref="IArtifactStore.UpdateNovaProcess(IUser, INovaProcess, List{HttpStatusCode})"/>
        public INovaProcess UpdateNovaProcess(
            IUser user,
            INovaProcess novaProcess,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(ArtifactStore), nameof(UpdateNovaProcess));

            ThrowIf.ArgumentNull(novaProcess, nameof(novaProcess));
            ThrowIf.ArgumentNull(user, nameof(user));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.PROCESSUPDATE_id_, novaProcess.Id);

            Logger.WriteInfo("{0} Updating Process ID: {1}, Name: {2}", nameof(ArtifactStore), novaProcess.Id, novaProcess.Name);

            var restApi = new RestApiFacade(Address, user.Token?.AccessControlToken);
            var restResponse = restApi.SendRequestAndDeserializeObject<NovaProcess, INovaProcess>(
                path,
                RestRequestMethod.PATCH,
                novaProcess,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);

            return restResponse;
        }

        #endregion Process methods

        #endregion Members inherited from IArtifactStore

        #region Private Methods

        /// <summary>
        /// Adds artifact to the baseline or collection
        /// Runs PUT svc/bpartifactstore/baselines/{0}/content or PUT svc/bpartifactstore/collections/{0}/content
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="artifactId">Id of Artifact to add.</param>
        /// <param name="path">The REST path to use.</param>
        /// <param name="includeDescendants">(optional)Pass true to include artifact's children.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request. By default only 200 OK is expected.</param>
        /// <returns>Result of adding artifact to Baseline or Collection</returns>
        /// <exception cref="AssertionException">Throws for unexpected itemType</exception>
        private T AddArtifactToBaselineOrCollection<T>(IUser user, int artifactId, string path, bool includeDescendants = false,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            var collectionContentToAdd = new Dictionary<string, object>();
            collectionContentToAdd.Add("addChildren", includeDescendants);
            collectionContentToAdd.Add("artifactId", artifactId);

            return restApi.SendRequestAndDeserializeObject<T, object> (
                path,
                RestRequestMethod.PUT,
                jsonObject: collectionContentToAdd,
                expectedStatusCodes: expectedStatusCodes);
        }

        #endregion Private Methods

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
                if (Files.Count > 0)
                {
                    Logger.WriteDebug("Deleting all files created by this ArtifactStore instance...");

                    Assert.NotNull(_userForFiles,
                        "It shouldn't be possible for the '{0}' member variable to be null if files were added to ArtifactStore!",
                        nameof(_userForFiles));

                    var fileStore = FileStoreFactory.GetFileStoreFromTestConfig();

                    // Delete all the files that were created.
                    foreach (var file in Files.ToArray())
                    {
                        fileStore.DeleteFile(file.Guid, _userForFiles);
                    }

                    Files.Clear();
                }
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

        #region Static members

        /// <summary>
        /// Uploads a new image to artifact store that could later be embedded in artifacts.
        /// </summary>
        /// <param name="address">The base address of the ArtifactStore.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="imageFile">The image file to upload.  Valid image formats are:  JPG and PNG.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 201 Created is expected.</param>
        /// <returns>The uploaded file with the GUID identification.</returns>
        public static EmbeddedImageFile AddImage(string address, IUser user, IFile imageFile, List<HttpStatusCode> expectedStatusCodes = null)
        {
            const int GUID_SIZE = 36;

            ThrowIf.ArgumentNull(imageFile, nameof(imageFile));
            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.AccessControlToken;
            var additionalHeaders = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(imageFile.FileType))
            {
                additionalHeaders.Add("Content-Type", imageFile.FileType);
            }

            if (!string.IsNullOrEmpty(imageFile.FileName))
            {
                additionalHeaders.Add("filename", imageFile.FileName);
            }

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            }

            var path = RestPaths.Svc.ArtifactStore.IMAGES;
            var restApi = new RestApiFacade(address, tokenValue);

            var response = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.POST,
                imageFile.FileName,
                imageFile.Content.ToArray(),
                imageFile.FileType,
                additionalHeaders: additionalHeaders,
                expectedStatusCodes: expectedStatusCodes);

            string embeddedImageId = System.Text.RegularExpressions.Regex.Replace(response.Content, "{\"guid\":\"", "");
            embeddedImageId = embeddedImageId.Substring(0, GUID_SIZE);
            imageFile.Guid = DatabaseHelper.GetFileStoreIdForEmbeddedImage(embeddedImageId);

            var embeddedImageFile = new EmbeddedImageFile
            {
                ArtifactId = null,
                Content = imageFile.Content.ToArray(),
                EmbeddedImageId = embeddedImageId,
                ExpireTime = null,
                FileName = imageFile.FileName,
                FileType = imageFile.FileType,
                Guid = imageFile.Guid,
                LastModifiedDate = imageFile.LastModifiedDate
            };

            return embeddedImageFile;
        }

        /// <summary>
        /// Gets an image that was uploaded to artifact store.  No authentication is required.
        /// </summary>
        /// <param name="address">The base address of the ArtifactStore.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="embeddedImageId">The GUID of the file you want to retrieve.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>The file that was requested.</returns>
        public static EmbeddedImageFile GetImage(string address, IUser user, string embeddedImageId, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(embeddedImageId, nameof(embeddedImageId));

            EmbeddedImageFile file = null;

            var restApi = new RestApiFacade(address, user?.Token?.AccessControlToken);
            var path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.IMAGES_id_, embeddedImageId);

            var response = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                file = new EmbeddedImageFile
                {
                    Content = response.RawBytes.ToArray(),
                    EmbeddedImageId = embeddedImageId,
                    FileType = response.ContentType,
                    Guid = DatabaseHelper.GetFileStoreIdForEmbeddedImage(embeddedImageId)
                };
            }

            return file;
        }

        /// <summary>
        /// Copies an artifact to a new parent.
        /// (Runs: POST {server}/svc/bpartifactstore/artifacts/{artifactId}/copyTo/{newParentId}?orderIndex={orderIndex})
        /// </summary>
        /// <param name="address">The base address of the ArtifactStore.</param>
        /// <param name="artifactId">The ID of artifact to copy.</param>
        /// <param name="newParentId">The ID of the new parent where this artifact will be copied to.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="orderIndex">(optional) The order index (relative to other artifacts) where this artifact should be copied to.
        ///     By default the artifact is copied to the end (after the last artifact).</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 201 Created is expected.</param>
        /// <returns>The details of the artifact that we copied and the number of artifacts copied.</returns>
        public static CopyNovaArtifactResultSet CopyArtifact(string address,
            int artifactId,
            int newParentId,
            IUser user,
            double? orderIndex = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.COPY_TO_id_, artifactId, newParentId);
            var restApi = new RestApiFacade(address, user?.Token?.AccessControlToken);

            Dictionary<string, string> queryParams = null;

            if (orderIndex != null)
            {
                queryParams = new Dictionary<string, string> { { "orderIndex", orderIndex.Value.ToStringInvariant() } };
            }

            // Set expectedStatusCodes to 201 Created by default if it's null.
            expectedStatusCodes = expectedStatusCodes ?? new List<HttpStatusCode> { HttpStatusCode.Created };

            var copiedArtifact = restApi.SendRequestAndDeserializeObject<CopyNovaArtifactResultSet>(
                path,
                RestRequestMethod.POST,
                queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: true);

            return copiedArtifact;
        }

        /// <summary>
        /// Creates a new Nova artifact.
        /// </summary>
        /// <param name="address">The base address of the ArtifactStore.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="baseArtifactType">The base artifact type (i.e. ItemType) to create.</param>
        /// <param name="name">The name of the new artifact.</param>
        /// <param name="project">The project where the artifact will be created in.</param>
        /// <param name="parentArtifactId">(optional) The ID of the parent of the new artifact.</param>
        /// <param name="orderIndex">(optional) The order index of the new artifact.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 201 Created is expected.</param>
        /// <returns>The new Nova artifact that was created.</returns>
        public static INovaArtifactDetails CreateArtifact(string address,
            IUser user,
            ItemTypePredefined baseArtifactType,
            string name,
            IProject project,
            int? parentArtifactId = null,
            double? orderIndex = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(address, nameof(address));
            ThrowIf.ArgumentNull(project, nameof(project));

            return CreateArtifact(address, 
                user, 
                baseArtifactType, 
                name, 
                project, 
                artifactTypeName: null, 
                parentArtifactId: parentArtifactId, 
                orderIndex: orderIndex, 
                expectedStatusCodes: expectedStatusCodes);
        }

        /// <summary>
        /// Creates a new Nova artifact using named Artifact Type.
        /// </summary>
        /// <param name="address">The base address of the ArtifactStore.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="baseArtifactType">The base artifact type (i.e. ItemType) to create.</param>
        /// <param name="name">The name of the new artifact.</param>
        /// <param name="project">The project where the artifact will be created in.</param>
        /// <param name="artifactTypeName">(optional) Name of the artifact type to be used to create the artifact</param>
        /// <param name="parentArtifactId">(optional) The ID of the parent of the new artifact.</param>
        /// <param name="orderIndex">(optional) The order index of the new artifact.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 201 Created is expected.</param>
        /// <returns>The new Nova artifact that was created.</returns>
        public static INovaArtifactDetails CreateArtifact(string address,
            IUser user,
            ItemTypePredefined baseArtifactType,
            string name,
            IProject project,
            string artifactTypeName = null,
            int? parentArtifactId = null,
            double? orderIndex = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(address, nameof(address));
            ThrowIf.ArgumentNull(project, nameof(project));

            string path = RestPaths.Svc.ArtifactStore.Artifacts.CREATE;
            var restApi = new RestApiFacade(address, user?.Token?.AccessControlToken);

            // Set expectedStatusCodes to 201 Created by default if it's null.
            expectedStatusCodes = expectedStatusCodes ?? new List<HttpStatusCode> { HttpStatusCode.Created };

            // Get the custom artifact type for the project.
            if (project.NovaPropertyTypes.Count == 0)
            {
                project.GetAllNovaArtifactTypes(project.ArtifactStore, user);
            }
            NovaArtifactType itemType;

            if (artifactTypeName == null)
            {
                itemType = project.NovaArtifactTypes.Find(at => at.PredefinedType == baseArtifactType);
            }
            else
            {
                itemType = project.NovaArtifactTypes.Find(at => at.PredefinedType == baseArtifactType && at.Name.Equals(artifactTypeName));
            }

            Assert.NotNull(itemType, "No custom artifact type was found in project '{0}' for ItemTypePredefined: {1}!",
                project.Name, baseArtifactType);

            var jsonBody = new NovaArtifactDetails
            {
                Name = name,
                ProjectId = project.Id,
                ItemTypeId = itemType.Id,
                ParentId = parentArtifactId ?? project.Id,
                OrderIndex = orderIndex
            };

            var newArtifact = restApi.SendRequestAndDeserializeObject<NovaArtifactDetails, INovaArtifactDetails>(
                path,
                RestRequestMethod.POST,
                jsonBody,
                expectedStatusCodes: expectedStatusCodes);

            return newArtifact;
        }

        /// <summary>
        /// Updates a Nova artifact.
        /// </summary>
        /// <param name="address">The base address of the ArtifactStore.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="novaArtifactDetails">The artifact details of the Nova artifact being updated</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>The new Nova artifact that was created.</returns>
        public static INovaArtifactDetails UpdateArtifact(string address, IUser user, INovaArtifactDetails novaArtifactDetails,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(address, nameof(address));
            ThrowIf.ArgumentNull(novaArtifactDetails, nameof(novaArtifactDetails));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.ARTIFACTS_id_, novaArtifactDetails.Id);

            var restApi = new RestApiFacade(address, user?.Token?.AccessControlToken);

            var newArtifact = restApi.SendRequestAndDeserializeObject<NovaArtifactDetails, NovaArtifactDetails>(
                path,
                RestRequestMethod.PATCH,
                (NovaArtifactDetails)novaArtifactDetails,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);

            return newArtifact;
        }

        /// <summary>
        /// Gets attachments for the specified artifact/subartifact
        /// (Runs: GET svc/artifactstore/artifacts/{artifactId}/attachment?addDrafts={addDrafts})
        /// </summary>
        /// <param name="address">The base address of the ArtifactStore.</param>
        /// <param name="artifactId">The ID of the artifact that has the attachment to get.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="addDrafts">(optional) Should include attachments in draft state.  Without addDrafts it works as if addDrafts=true.</param>
        /// <param name="versionId">(optional) The version of the attachment to retrieve.</param>
        /// <param name="baselineId">(optional) The id of baseline to get version of the attachment to retrieve.</param>
        /// <param name="subArtifactId">(optional) The ID of a sub-artifact of this artifact that has the attachment to get.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>Attachment object for the specified artifact/subartifact.</returns>
        public static Attachments GetAttachments(string address,
            int artifactId,
            IUser user,
            bool? addDrafts = null,
            int? versionId = null,
            int? baselineId = null,
            int? subArtifactId = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(address, nameof(address));
            ThrowIf.ArgumentNull(user, nameof(user));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT, artifactId);
            var queryParameters = new Dictionary<string, string>();

            if (addDrafts != null)
            {
                queryParameters.Add("addDrafts", addDrafts.ToString());
            }

            if (versionId != null)
            {
                queryParameters.Add("versionId", versionId.ToString());
            }

            if (baselineId != null)
            {
                queryParameters.Add("baselineId", baselineId.ToString());
            }

            if (subArtifactId != null)
            {
                queryParameters.Add("subArtifactId", subArtifactId.ToString());
            }

            var restApi = new RestApiFacade(address, user.Token?.AccessControlToken);

            try
            {
                var attachment = restApi.SendRequestAndDeserializeObject<Attachments>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);

                return attachment;
            }
            catch (Exception)
            {
                Logger.WriteDebug("Content = '{0}'", restApi.Content);
                throw;
            }
        }

        /// <summary>
        /// Gets list of subartifacts for the artifact with the specified ID.
        /// (Runs: GET svc/artifactstore/artifacts/{artifactId}/subartifacts)
        /// </summary>
        /// <param name="address">The base address of the ArtifactStore.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="artifactId">Id of artifact.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>List of subartifacts.</returns>
        public static List<SubArtifact> GetSubartifacts(string address, IUser user, int artifactId, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.SUBARTIFACTS, artifactId);
            var restApi = new RestApiFacade(address, user?.Token?.AccessControlToken);

            var subartifacts = restApi.SendRequestAndDeserializeObject<List<SubArtifact>>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes, shouldControlJsonChanges: true);

            return subartifacts;
        }

        /// <summary>
        /// Gets subartifact.
        /// (Runs: GET svc/artifactstore/artifacts/{artifactId}/subartifacts/{subArtifactId})
        /// </summary>
        /// <param name="address">The base address of the ArtifactStore.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="artifactId">Id of artifact.</param>
        /// <param name="subArtifactId">Id of the subArtifact</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>The requested subArtifact</returns>
        public static NovaSubArtifact GetSubartifact(string address, IUser user, int artifactId, int subArtifactId, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.SUBARTIFACTS_id_, artifactId, subArtifactId);
            var restApi = new RestApiFacade(address, user?.Token?.AccessControlToken);

            var novaSubArtifact = restApi.SendRequestAndDeserializeObject<NovaSubArtifact>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: true);

            return novaSubArtifact;
        }

        /// <summary>
        /// Gets list of unpublished changes for the specified user.
        /// (Runs: GET svc/bpartifactstore/artifacts/unpublished)
        /// </summary>
        /// <param name="address">The base address of the ArtifactStore.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>List of artifacts and projects for the unpublished changes.</returns>
        public static INovaArtifactsAndProjectsResponse GetUnpublishedChanges(string address, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var restApi = new RestApiFacade(address, user?.Token?.AccessControlToken);

            var unpublishedChanges = restApi.SendRequestAndDeserializeObject<NovaArtifactsAndProjectsResponse>(
                RestPaths.Svc.ArtifactStore.Artifacts.UNPUBLISHED,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: true);

            return unpublishedChanges;
        }

        #endregion Static members

    }
}
