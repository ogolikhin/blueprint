using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Common;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Newtonsoft.Json;
using NUnit.Framework;
using Utilities;
using Utilities.Facades;
using System.Web;
using System.Net.Mime;
using Model.Factories;

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

        /// <seealso cref="IArtifactStore.GetImage(string, List{HttpStatusCode})"/>
        public EmbeddedImageFile GetImage(string embeddedImageId, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetImage(Address, embeddedImageId, expectedStatusCodes);
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

        /// <seealso cref="IArtifactStore.UpdateArtifact(IUser, IProject, NovaArtifactDetails, List{HttpStatusCode})"/>
        public INovaArtifactDetails UpdateArtifact(IUser user,
            IProject project,
            NovaArtifactDetails novaArtifactDetails,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return UpdateArtifact(Address, user, project, novaArtifactDetails);
        }

        /// <seealso cref="IArtifactStore.DeleteArtifact(IArtifactBase, IUser, List{HttpStatusCode})"/>
        public List<INovaArtifactResponse> DeleteArtifact(IArtifactBase artifact, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            var deletedArtifacts = DeleteArtifact(Address, artifact, user, expectedStatusCodes);

            return deletedArtifacts;
        }

        /// <seealso cref="IArtifactStore.DiscardArtifact(IArtifactBase, IUser, bool?, List{HttpStatusCode})"/>
        public INovaArtifactsAndProjectsResponse DiscardArtifact(IArtifactBase artifact, IUser user = null, bool? all = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var artifacts = new List<IArtifactBase> { artifact };
            return DiscardArtifacts(Address, artifacts, user, all, expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.DiscardArtifacts(List{IArtifactBase}, IUser, bool?, List{HttpStatusCode})"/>
        public INovaArtifactsAndProjectsResponse DiscardArtifacts(List<IArtifactBase> artifacts, IUser user = null, bool? all = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return DiscardArtifacts(Address, artifacts, user, all, expectedStatusCodes);
        }

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

        /// <seealso cref="IArtifactStore.GetArtifactChildrenByProjectAndArtifactId(int, int, IUser, List{HttpStatusCode})"/>
        public List<NovaArtifact> GetArtifactChildrenByProjectAndArtifactId(int projectId, int artifactId, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Projects_id_.Artifacts_id_.CHILDREN, projectId, artifactId);
            RestApiFacade restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            return restApi.SendRequestAndDeserializeObject<List<NovaArtifact>>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.GetProjectChildrenByProjectId(int, IUser, List{HttpStatusCode})"/>
        public List<NovaArtifact> GetProjectChildrenByProjectId(int id, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetProjectChildrenByProjectId(Address, id, user, expectedStatusCodes);
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
            RestApiFacade restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            Dictionary<string, string> queryParams = null;

            if (includeChildren != null)
            {
                queryParams = new Dictionary<string, string> { { "includeChildren", includeChildren.Value.ToString() } };
            }

            var novaArtifacts = restApi.SendRequestAndDeserializeObject<List<NovaArtifact>>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes);

            var ret = novaArtifacts.ConvertAll(o => (INovaArtifact)o);
            return ret;
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

            var artifactDetails = restApi.SendRequestAndDeserializeObject<NovaArtifactDetails>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes);

            return artifactDetails;
        }

        /// <seealso cref="IArtifactStore.GetDiagramArtifact(IUser, int, int?, List{HttpStatusCode})"/>
        public NovaDiagramArtifact GetDiagramArtifact(IUser user, int artifactId, int? versionId = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.DIAGRAM_id_, artifactId);
            RestApiFacade restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            Dictionary<string, string> queryParams = null;

            if (versionId != null)
            {
                queryParams = new Dictionary<string, string> { { "versionId", versionId.ToString() } };
            }

            return restApi.SendRequestAndDeserializeObject<NovaDiagramArtifact>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.GetGlossaryArtifact(IUser, int, int?, List{HttpStatusCode})"/>
        public NovaGlossaryArtifact GetGlossaryArtifact(IUser user, int artifactId, int? versionId = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.GLOSSARY_id_, artifactId);
            RestApiFacade restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            Dictionary<string, string> queryParams = null;

            if (versionId != null)
            {
                queryParams = new Dictionary<string, string> { { "versionId", versionId.ToString() } };
            }

            return restApi.SendRequestAndDeserializeObject<NovaGlossaryArtifact>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.GetUseCaseArtifact(IUser, int, int?, List{HttpStatusCode})"/>
        public NovaUseCaseArtifact GetUseCaseArtifact(IUser user, int artifactId, int? versionId = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.USECASE_id_, artifactId);
            RestApiFacade restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            Dictionary<string, string> queryParams = null;

            if (versionId != null)
            {
                queryParams = new Dictionary<string, string> { { "versionId", versionId.ToString() } };
            }

            return restApi.SendRequestAndDeserializeObject<NovaUseCaseArtifact>(
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
                expectedStatusCodes: expectedStatusCodes, shouldControlJsonChanges: true);

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
                expectedStatusCodes: expectedStatusCodes, shouldControlJsonChanges: true);

            return discussionReplies;
        }

        /// <seealso cref="IArtifactStore.GetAttachments(IArtifactBase, IUser, bool?, int?, int?, List{HttpStatusCode}, IServiceErrorMessage)"/>
        public Attachments GetAttachments(IArtifactBase artifact, IUser user, bool? addDrafts = null, int? versionId = null,
            int? subArtifactId = null, List<HttpStatusCode> expectedStatusCodes = null, IServiceErrorMessage expectedServiceErrorMessage = null)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            
            return GetAttachments(Address, artifact.Id, user, addDrafts, versionId, subArtifactId,
                expectedStatusCodes, expectedServiceErrorMessage);
        }

        /// <seealso cref="IArtifactStore.GetRelationships(IUser, IArtifactBase, int?, bool?, int?, List{HttpStatusCode})"/>
        public Relationships GetRelationships(IUser user,
            IArtifactBase artifact,
            int? subArtifactId = null,
            bool? addDrafts = null,
            int? versionId = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            return GetRelationships(Address, user, artifact.Id, subArtifactId, addDrafts, versionId, expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.GetRelationshipsDetails(IUser, IArtifactBase, List{HttpStatusCode})"/>
        public TraceDetails GetRelationshipsDetails(IUser user,
            IArtifactBase artifact,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            return GetRelationshipsDetails(Address, user, artifact.Id, expectedStatusCodes);
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

        /// <seealso cref="IArtifactStore.GetVersionControlInfo(IUser, int, List{HttpStatusCode})"/>
        public INovaVersionControlArtifactInfo GetVersionControlInfo(IUser user, int itemId, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts.VERSION_CONTROL_INFO_id_, itemId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            var artifactBaseInfo = restApi.SendRequestAndDeserializeObject<NovaVersionControlArtifactInfo>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: true);

            return artifactBaseInfo;
        }

        /// <seealso cref="IArtifactStore.MoveArtifact(IArtifactBase, IArtifactBase, IUser, double?, List{HttpStatusCode})"/>
        public INovaArtifactDetails MoveArtifact(IArtifactBase artifact,
            IArtifactBase newParent,
            IUser user = null,
            double? orderIndex = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(newParent, nameof(newParent));

            return MoveArtifact(Address, artifact, newParent.Id, user, orderIndex, expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.PublishArtifact(IArtifactBase, IUser, List{HttpStatusCode})"/>
        public INovaArtifactsAndProjectsResponse PublishArtifact(IArtifactBase artifact,
            IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            var artifacts = new List<IArtifactBase> { artifact };

            return PublishArtifacts(artifacts, user, expectedStatusCodes: expectedStatusCodes);
        }

        /// <seealso cref="IArtifactStore.PublishArtifacts(List{IArtifactBase}, IUser, bool?, List{HttpStatusCode})"/>
        public INovaArtifactsAndProjectsResponse PublishArtifacts(List<IArtifactBase> artifacts,
            IUser user = null,
            bool? all = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return PublishArtifacts(Address, artifacts, user, all, expectedStatusCodes);
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
        /// <param name="embeddedImageId">The GUID of the file you want to retrieve.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>The file that was requested.</returns>
        public static EmbeddedImageFile GetImage(string address, string embeddedImageId, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(embeddedImageId, nameof(embeddedImageId));

            EmbeddedImageFile file = null;

            var restApi = new RestApiFacade(address);
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
                    Guid = DatabaseHelper.GetFileStoreIdForEmbeddedImage(embeddedImageId),
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
            RestApiFacade restApi = new RestApiFacade(address, user?.Token?.AccessControlToken);

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
            RestApiFacade restApi = new RestApiFacade(address, user?.Token?.AccessControlToken);

            // Set expectedStatusCodes to 201 Created by default if it's null.
            expectedStatusCodes = expectedStatusCodes ?? new List<HttpStatusCode> { HttpStatusCode.Created };

            // Get the custom artifact type for the project.
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

            var newArtifact = restApi.SendRequestAndDeserializeObject<NovaArtifactDetails, NovaArtifactDetails>(
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
        /// <param name="project">The project containing the artifact to be updated.</param>
        /// <param name="novaArtifactDetails">The artifact details of the Nova artifact being updated</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>The new Nova artifact that was created.</returns>
        public static INovaArtifactDetails UpdateArtifact(string address, IUser user, IProject project, NovaArtifactDetails novaArtifactDetails,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(address, nameof(address));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(novaArtifactDetails, nameof(novaArtifactDetails));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.ARTIFACTS_id_, novaArtifactDetails.Id);

            RestApiFacade restApi = new RestApiFacade(address, user?.Token?.AccessControlToken);

            var newArtifact = restApi.SendRequestAndDeserializeObject<NovaArtifactDetails, NovaArtifactDetails>(
                path,
                RestRequestMethod.PATCH,
                novaArtifactDetails,
                expectedStatusCodes: expectedStatusCodes);

            return newArtifact;
        }

        /// <summary>
        /// Deletes the specified artifact and any children/traces/links/attachments belonging to the artifact.
        /// </summary>
        /// <param name="address">The base address of the ArtifactStore.</param>
        /// <param name="artifact">The artifact to delete.</param>
        /// <param name="user">(optional) The user to authenticate with.  By default it uses the user that created the artifact.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>A list of artifacts that were deleted.</returns>
        public static List<INovaArtifactResponse> DeleteArtifact(string address, IArtifactBase artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(address, nameof(address));
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.ARTIFACTS_id_, artifact.Id);
            RestApiFacade restApi = new RestApiFacade(address, user?.Token?.AccessControlToken);

            var deletedArtifacts = restApi.SendRequestAndDeserializeObject<List<NovaArtifactResponse>>(
                path,
                RestRequestMethod.DELETE,
                expectedStatusCodes: expectedStatusCodes);

            var deletedArtifactsToReturn = deletedArtifacts.ConvertAll(o => (INovaArtifactResponse)o);

            if (restApi.StatusCode == HttpStatusCode.OK)
            {
                // Set the IsMarkedForDeletion flag for the artifact that we deleted so the Dispose() works properly.
                foreach (INovaArtifactResponse deletedArtifact in deletedArtifacts)
                {
                    Logger.WriteDebug("'DELETE {0}' returned following artifact Id: {1}",
                        path, deletedArtifact.Id);

                    ArtifactBase artifaceBaseToDelete = artifact as ArtifactBase;

                    // Hack: This is needed until we can refactor ArtifactBase better.
                    DeleteArtifactResult deletedArtifactResult = new DeleteArtifactResult
                    {
                        ArtifactId = deletedArtifact.Id,
                        ResultCode = HttpStatusCode.OK
                    };

                    artifaceBaseToDelete.DeletedArtifactResults.Add(deletedArtifactResult);

                    if (deletedArtifact.Id == artifact.Id)
                    {
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
            }

            return deletedArtifactsToReturn;
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
        /// <param name="subArtifactId">(optional) The ID of a sub-artifact of this artifact that has the attachment to get.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <param name="expectedServiceErrorMessage">(optional) Expected error message for the request.</param>
        /// <returns>Attachment object for the specified artifact/subartifact.</returns>
        public static Attachments GetAttachments(string address,
            int artifactId,
            IUser user,
            bool? addDrafts = null,
            int? versionId = null,
            int? subArtifactId = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            IServiceErrorMessage expectedServiceErrorMessage = null)
        {
            ThrowIf.ArgumentNull(address, nameof(address));
            ThrowIf.ArgumentNull(user, nameof(user));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.ATTACHMENT, artifactId);
            Dictionary<string, string> queryParameters = new Dictionary<string, string>();

            if (addDrafts != null)
            {
                queryParameters.Add("addDrafts", addDrafts.ToString());
            }

            if (versionId != null)
            {
                queryParameters.Add("versionId", versionId.ToString());
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
                expectedStatusCodes: expectedStatusCodes);

                return attachment;
            }
            catch (Exception)
            {
                Logger.WriteDebug("Content = '{0}'", restApi.Content);

                if (expectedServiceErrorMessage != null)
                {
                    var serviceErrorMessage = JsonConvert.DeserializeObject<ServiceErrorMessage>(restApi.Content);
                    serviceErrorMessage.AssertEquals(expectedServiceErrorMessage);
                }

                throw;
            }
        }

        /// <summary>
        /// Gets all children artifacts for specified by id project.
        /// (Runs: GET /projects/{projectId}/children)
        /// </summary>
        /// <param name="address">The base address of the ArtifactStore.</param>
        /// <param name="id">The id of specified project.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>A list of all artifacts in the specified project.</returns>
        public static List<NovaArtifact> GetProjectChildrenByProjectId(string address,
            int id,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Projects_id_.CHILDREN, id);
            RestApiFacade restApi = new RestApiFacade(address, user?.Token?.AccessControlToken);

            return restApi.SendRequestAndDeserializeObject<List<NovaArtifact>>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);
        }

        /// <summary>
        /// Gets relationships for the specified artifact/subartifact
        /// (Runs: GET svc/artifactstore/artifacts/{itemId}/relationships)
        /// </summary>
        /// <param name="address">The base address of the ArtifactStore.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="artifactId">The ID of the artifact containing the relationship to get.</param>
        /// <param name="subArtifactId">(optional) ID of the sub-artifact.</param>
        /// <param name="addDrafts">(optional) Should include attachments in draft state.  Without addDrafts it works as if addDrafts=true</param>
        /// <param name="versionId">(optional) The version of the artifact whose relationships you want to get. null = latest version.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>Relationships object for the specified artifact/subartifact.</returns>
        public static Relationships GetRelationships(string address,
            IUser user,
            int artifactId,
            int? subArtifactId = null,
            bool? addDrafts = null,
            int? versionId = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

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

            var restApi = new RestApiFacade(address, user.Token?.AccessControlToken);

            var relationships = restApi.SendRequestAndDeserializeObject<Relationships>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes);

            return relationships;
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

        /// <summary>
        /// Moves an artifact to a different parent.
        /// (Runs: POST {server}/svc/bpartifactstore/artifacts/{artifactId}/moveTo/{newParentId}?orderIndex={orderIndex})
        /// </summary>
        /// <param name="address">The base address of the ArtifactStore.</param>
        /// <param name="artifact">The artifact to move.</param>
        /// <param name="newParentId">The ID of the new parent where this artifact will be moved to.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="orderIndex">(optional) The order index (relative to other artifacts) where this artifact should be moved to.
        ///     By default the artifact is moved to the end (after the last artifact).</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>The details of the artifact that we moved.</returns>
        public static INovaArtifactDetails MoveArtifact(string address,
            IArtifactBase artifact,
            int newParentId,
            IUser user,
            double? orderIndex = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(address, nameof(address));
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.MOVE_TO_id_, artifact.Id, newParentId);
            RestApiFacade restApi = new RestApiFacade(address, user?.Token?.AccessControlToken);

            Dictionary<string, string> queryParams = null;

            if (orderIndex != null)
            {
                queryParams = new Dictionary<string, string> { { "orderIndex", orderIndex.Value.ToStringInvariant() } };
            }

            var movedArtifact = restApi.SendRequestAndDeserializeObject<NovaArtifactDetails>(
                path,
                RestRequestMethod.POST,
                queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: true);

            if (restApi.StatusCode == HttpStatusCode.OK)
            {
                // Set the IsSaved flag for the artifact so the Dispose() works properly.
                artifact.IsSaved = true;
            }

            return movedArtifact;
        }

        /// <summary>
        /// Publishes a list of artifacts.
        /// </summary>
        /// <param name="address">The base address of the ArtifactStore.</param>
        /// <param name="artifacts">The artifacts to publish.  This can be null if the 'all' parameter is true.</param>
        /// <param name="user">(optional) The user to authenticate with.  By default it uses the user that created the artifact.</param>
        /// <param name="all">(optional) Pass true to publish all artifacts created by the user that have changes.  In this case, you don't need to specify the artifacts to publish.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>An object containing a list of artifacts that were published and their projects.</returns>
        public static INovaArtifactsAndProjectsResponse PublishArtifacts(string address,
            List<IArtifactBase> artifacts,
            IUser user = null,
            bool? all = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            if (artifacts == null)
            {
                artifacts = new List<IArtifactBase>();
            }

            const string path = RestPaths.Svc.ArtifactStore.Artifacts.PUBLISH;
            RestApiFacade restApi = new RestApiFacade(address, user?.Token?.AccessControlToken);
            var artifactIds = artifacts.Select(artifact => artifact.Id).ToList();
            Dictionary<string, string> queryParams = null;

            if (all != null)
            {
                queryParams = new Dictionary<string, string> { { "all", all.Value.ToString() } };
            }

            var publishedArtifacts = restApi.SendRequestAndDeserializeObject<NovaArtifactsAndProjectsResponse, List<int>>(
                path,
                RestRequestMethod.POST,
                artifactIds,
                queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: true);

            if (restApi.StatusCode == HttpStatusCode.OK)
            {
                var deletedArtifactsList = new List<IArtifactBase>();
                var otherPublishedArtifactsList = new List<INovaArtifactResponse>();

                // Set the IsPublished... flags for the artifact that we deleted so the Dispose() works properly.
                foreach (var publishedArtifactDetails in publishedArtifacts.Artifacts)
                {
                    Logger.WriteDebug("'POST {0}' returned following artifact Id: {1}",
                        path, publishedArtifactDetails.Id);

                    IArtifactBase publishedArtifact = artifacts.Find(a => a.Id == publishedArtifactDetails.Id);

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
                    deletedArtifactsList[0]?.NotifyArtifactDeletion(deletedArtifactsList);
                }

                if (otherPublishedArtifactsList.Any())
                {
                    if (artifacts.Any())
                    {
                        artifacts[0]?.NotifyArtifactPublish(otherPublishedArtifactsList);
                    }

                    Assert.That((all != null) && (all.Value == true),
                        "An artifact that wasn't explicitly passed was published but the 'all=true' parameter wasn't passed!");
                }
            }

            return publishedArtifacts;
        }

        /// <summary>
        /// Discards a list of artifacts.
        /// </summary>
        /// <param name="address">The base address of the ArtifactStore.</param>
        /// <param name="artifacts">The artifacts to discard.  This can be null if the 'all' parameter is true.</param>
        /// <param name="user">(optional) The user to authenticate with.  By default it uses the user that created the artifact.</param>
        /// <param name="all">(optional) Pass true to discard all artifacts created by the user that have changes.  In this case, you don't need to specify the artifacts to discard.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>An object containing a list of artifacts that were discarded and their projects.</returns>
        public static INovaArtifactsAndProjectsResponse DiscardArtifacts(string address,
            List<IArtifactBase> artifacts,
            IUser user = null,
            bool? all = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            if (artifacts == null)
            {
                artifacts = new List<IArtifactBase>();
            }

            const string path = RestPaths.Svc.ArtifactStore.Artifacts.DISCARD;
            RestApiFacade restApi = new RestApiFacade(address, user?.Token?.AccessControlToken);
            var artifactIds = artifacts.Select(artifact => artifact.Id).ToList();
            Dictionary<string, string> queryParams = null;

            if (all != null)
            {
                queryParams = new Dictionary<string, string> { { "all", all.Value.ToString() } };
            }

            var discardedArtifactResponse = restApi.SendRequestAndDeserializeObject<NovaArtifactsAndProjectsResponse, List<int>>(
                path,
                RestRequestMethod.POST,
                artifactIds,
                queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes);

            if (restApi.StatusCode == HttpStatusCode.OK)
            {
                // Set the IsSaved flags for the artifact that we discarded so the Dispose() works properly.
                foreach (var discardedArtifacts in discardedArtifactResponse.Artifacts)
                {
                    Logger.WriteDebug("'POST {0}' returned following artifact Id: {1}",
                        path, discardedArtifacts.Id);

                    if (artifacts.Count > 0)
                    {
                        IArtifactBase discardedArtifact = artifacts.Find(a => a.Id == discardedArtifacts.Id);

                        if (discardedArtifact != null)
                        {
                            discardedArtifact.IsSaved = false;
                        }
                    }
                }
            }

            return discardedArtifactResponse;
        }

        /// <summary>
        /// Gets traceDetails for the specified artifact/subartifact
        /// (Runs: GET svc/artifactstore/artifacts/{artifactId}/relationshipdetails)
        /// </summary>
        /// <param name="address">The base address of the ArtifactStore.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="artifactId">The artifact ID containing the relationship to get.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>RelationshipsDetails object for the specified artifact/subartifact.</returns>
        public static TraceDetails GetRelationshipsDetails(string address,
            IUser user,
            int artifactId,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(address, nameof(address));
            ThrowIf.ArgumentNull(user, nameof(user));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.Artifacts_id_.RELATIONSHIP_DETAILS, artifactId);
            var restApi = new RestApiFacade(address, user.Token?.AccessControlToken);

            var traceDetails = restApi.SendRequestAndDeserializeObject<TraceDetails>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: true);

            return traceDetails;
        }

        #endregion Static members
    }
}
