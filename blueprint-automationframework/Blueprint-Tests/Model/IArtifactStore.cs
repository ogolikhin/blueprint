using Model.Impl;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using System;
using System.Collections.Generic;
using System.Net;

namespace Model
{
    /// <summary>
    /// This is the data returned by:  GET /projects/{projectId}/meta/customtypes  which contains all the custom artifact/sub-artifact and property types in a project.
    /// </summary>
    public class ProjectCustomArtifactTypesResult
    {
        public List<NovaArtifactType> ArtifactTypes { get; } = new List<NovaArtifactType>();
        public List<NovaArtifactType> SubArtifactTypes { get; } = new List<NovaArtifactType>();
        public List<NovaPropertyType> PropertyTypes { get; } = new List<NovaPropertyType>();
    }

    public interface IArtifactStore : IDisposable
    {
        /// <summary>
        /// Creates a new Nova artifact.
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="artifactType">The artifact type (i.e. ItemType) to create.</param>
        /// <param name="name">The name of the new artifact.</param>
        /// <param name="project">The project where the artifact will be created in.</param>
        /// <param name="parentArtifact">(optional) The parent of the new artifact.</param>
        /// <param name="orderIndex">(optional) The order index of the new artifact.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 201 Created is expected.</param>
        /// <returns>The new Nova artifact that was created.</returns>
        INovaArtifactDetails CreateArtifact(IUser user, 
            BaseArtifactType artifactType,
            string name,
            IProject project,
            INovaArtifactDetails parentArtifact = null,
            double? orderIndex = null,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Deletes the specified artifact and any children/traces/links/attachments belonging to the artifact.
        /// (Runs: DELETE {server}/svc/bpartifactstore/artifacts/{artifactId})
        /// </summary>
        /// <param name="artifact">The artifact to delete.</param>
        /// <param name="user">(optional) The user to authenticate with.  By default it uses the user that created the artifact.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>A list of artifacts that were deleted.</returns>
        List<INovaArtifactResponse> DeleteArtifact(IArtifactBase artifact, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Discard a single artifact.
        /// </summary>
        /// <param name="artifact">The artifact to discard.  This can be null if the 'all' parameter is true.</param>
        /// <param name="user">(optional) The user to authenticate with.  By default it uses the user that created the artifact.</param>
        /// <param name="all">(optional) Pass true to discard all artifacts saved by the user that have changes.  In this case, you don't need to specify the artifact to discard.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>An object containing a list of artifacts that were discarded and their projects.</returns>
        INovaArtifactsAndProjectsResponse DiscardArtifact(IArtifactBase artifact,
            IUser user = null,
            bool? all = null,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Discard a list of artifacts.
        /// </summary>
        /// <param name="artifacts">The artifacts to discard.  This can be null if the 'all' parameter is true.</param>
        /// <param name="user">(optional) The user to authenticate with.  By default it uses the user that created the artifact.</param>
        /// <param name="all">(optional) Pass true to discard all artifacts saved by the user that have changes.  In this case, you don't need to specify the artifacts to discard.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>An object containing a list of artifacts that were discarded and their projects.</returns>
        INovaArtifactsAndProjectsResponse DiscardArtifacts(List<IArtifactBase> artifacts,
            IUser user = null,
            bool? all = null,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Checks if the ArtifactStore service is ready for operation.
        /// (Runs: GET /status)
        /// </summary>
        /// <param name="preAuthorizedKey">(optional) The pre-authorized key to use for authentication.  Defaults to a valid key.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>A JSON structure containing the status of this service and its dependent services.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        string GetStatus(string preAuthorizedKey = CommonConstants.PreAuthorizedKeyForStatus, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Checks if the ArtifactStore service is ready for operation.
        /// (Runs: GET /status/upcheck)
        /// </summary>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>The status code returned by ArtifactStore.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        HttpStatusCode GetStatusUpcheck(List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Returns all custom artifact, sub-artifact and property types of the specified project.
        /// (Runs: GET /projects/{projectId}/meta/customtypes)
        /// </summary>
        /// <param name="project">The project whose types you are interested in.</param>
        /// <param name="user">(optional) The user to authenticate with.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>An object containing a list of custom artifact, sub-artifact and property types for the specified project.</returns>
        ProjectCustomArtifactTypesResult GetCustomArtifactTypes(IProject project, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets all children artifacts for specified by id project.
        /// (Runs: GET /projects/{projectId}/children)
        /// </summary>
        /// <param name="id">The id of specified project.</param>
        /// <param name="user">(optional) The user to authenticate with.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>A list of all artifacts in the specified project.</returns>
        List<NovaArtifact> GetProjectChildrenByProjectId(int id, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets all children artifacts by project and artifact id.
        /// (Runs: GET /projects/{projectId}/artifacts/{artifactId})
        /// </summary>
        /// <param name="projectId">The id of specific project.</param>
        /// <param name="artifactId">The id of specific artifact.</param>
        /// <param name="user">(optional) The user to authenticate with.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>A list of all sub-artifacts of the specified artifact.</returns>
        List<NovaArtifact> GetArtifactChildrenByProjectAndArtifactId(int projectId,
            int artifactId,
            IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets the artifact tree expanded to the specified artifact.
        /// (Runs: GET /projects/{projectId}/artifacts/{artifactId}/[includeChildren={bool}])
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="project">The project containing the artifact.</param>
        /// <param name="artifactId">The Id of artifact whose hierarchy you are getting.</param>
        /// <param name="includeChildren">(optional) Pass true to also get children of the specified artifact.  By default no children are retrieved.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>A list of artifacts (top-level siblings), and one artifact that contains a tree of children down to the specified artifact.</returns>
        List<INovaArtifact> GetExpandedArtifactTree(IUser user, 
            IProject project,
            int artifactId,
            bool? includeChildren = null,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets artifacts history by artifact id.
        /// (Runs: GET /svc/ArtifactStore/artifacts/{artifactId}/version)
        /// </summary>
        /// <param name="artifactId">The id of artifact.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="sortByDateAsc">(optional) False - the latest version comes first, true the latest version comes last. Without this param call return versions as for sortByDateAsc=false.</param>
        /// <param name="limit">(optional) The maximum number of history items returned in the request. Without this param call return 10 versions.</param>
        /// <param name="offset">(optional) The offset for the pagination.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>List of artifacts versions.</returns>
        List<ArtifactHistoryVersion> GetArtifactHistory(int artifactId, IUser user,
            bool? sortByDateAsc = null, int? limit = null, int? offset = null,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets discussions for the artifact or subartifact with specified id
        /// (Runs: GET svc/ArtifactStore/artifacts/{itemId}/discussions)
        /// </summary>
        /// <param name="itemId">id of the artifact/subartifact</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>Discussion for the artifact/subartifact with specified id.</returns>
        Discussions GetArtifactDiscussions(int itemId, IUser user,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets replies for the specified comment
        /// (Runs: GET svc/ArtifactStore/artifacts/{comment.itemId}/discussions/{comment.discussionId}/replies)
        /// </summary>
        /// <param name="comment">Comment to get replies</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>Replies for the specified comment.</returns>
        List<Reply> GetDiscussionsReplies(Comment comment,  IUser user,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets attachments for the specified artifact/subartifact
        /// (Runs: GET svc/artifactstore/artifacts/{artifactId}/attachment?addDrafts={addDrafts})
        /// </summary>
        /// <param name="artifact">The artifact that has the attachment to get.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="addDrafts">(optional) Should include attachments in draft state.  Without addDrafts it works as if addDrafts=true.</param>
        /// <param name="subArtifactId">(optional) The ID of a sub-artifact of this artifact that has the attachment to get.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>Attachment object for the specified artifact/subartifact.</returns>
        Attachments GetAttachments(IArtifactBase artifact, IUser user, bool? addDrafts = null, int? subArtifactId = null,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets relationships for the specified artifact/subartifact
        /// (Runs: GET svc/artifactstore/artifacts/{itemId}/relationships)
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="artifact">The artifact containing the relationship to get.</param>
        /// <param name="subArtifactId">(optional) ID of the sub-artifact.</param>
        /// <param name="addDrafts">(optional) Should include attachments in draft state.  Without addDrafts it works as if addDrafts=true</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>Relationships object for the specified artifact/subartifact.</returns>
        Relationships GetRelationships(IUser user, IArtifactBase artifact, int? subArtifactId = null, bool? addDrafts = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets artifact details by specifying its ID.
        /// (Runs: GET svc/bpartifactstore/artifacts/{artifactId})
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="artifactId">Id of artifact.</param>
        /// <param name="versionId">(optional) The version of the artifact whose details you want to get.  null = latest version.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>Artifact details.</returns>
        NovaArtifactDetails GetArtifactDetails(IUser user, int artifactId, int? versionId = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets relationshipsdetails for the specified artifact/subartifact
        /// (Runs: GET svc/artifactstore/artifacts/{itemId}/relationshipdetails)
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="artifact">The artifact containing the relationship to get.</param>
        /// <param name="addDrafts">(optional) Should include attachments in draft state.  Without addDrafts it works as if addDrafts=true</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>RelationshipsDetails object for the specified artifact/subartifact.</returns>
        TraceDetails GetRelationshipsDetails(IUser user, IArtifactBase artifact, bool? addDrafts = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets list of subartifacts for the artifact with the specified ID.
        /// (Runs: GET svc/artifactstore/artifacts/{artifactId}/subartifacts)
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="artifactId">Id of artifact.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>List of subartifacts.</returns>
        List<INovaSubArtifact> GetSubartifacts(IUser user, int artifactId, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets list of unpublished changes for the specified user.
        /// (Runs: GET svc/bpartifactstore/artifacts/unpublished)
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>List of artifacts and projects for the unpublished changes.</returns>
        INovaArtifactsAndProjectsResponse GetUnpublishedChanges(IUser user, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Moves an artifact to a different parent.
        /// </summary>
        /// <param name="artifact">The artifact to move.</param>
        /// <param name="newParent">The new parent where this artifact will move to.</param>
        /// <param name="user">(optional) The user to authenticate with.  By default it uses the user that created the artifact.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>The details of the artifact that we moved.</returns>
        INovaArtifactDetails MoveArtifact(IArtifactBase artifact,
            IArtifactBase newParent,
            IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Publishes an artifact.
        /// </summary>
        /// <param name="artifact">The artifact to publish.</param>
        /// <param name="user">(optional) The user to authenticate with.  By default it uses the user that created the artifact.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>An object containing a list of artifacts that were published and their projects.</returns>
        INovaArtifactsAndProjectsResponse PublishArtifact(IArtifactBase artifact, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Publishes a list of artifacts.
        /// </summary>
        /// <param name="artifacts">The artifacts to publish.  This can be null if the 'all' parameter is true.</param>
        /// <param name="user">(optional) The user to authenticate with.  By default it uses the user that created the artifact.</param>
        /// <param name="all">(optional) Pass true to publish all artifacts created by the user that have changes.  In this case, you don't need to specify the artifacts to publish.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>An object containing a list of artifacts that were published and their projects.</returns>
        INovaArtifactsAndProjectsResponse PublishArtifacts(List<IArtifactBase> artifacts, IUser user = null, bool? all = null, List<HttpStatusCode> expectedStatusCodes = null);
    }
}
