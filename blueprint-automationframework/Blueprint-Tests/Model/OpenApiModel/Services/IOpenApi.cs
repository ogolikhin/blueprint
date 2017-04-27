using System.Collections.Generic;
using System.Net;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Impl;
using Model.JobModel;
using Model.OpenApiModel.UserModel;
using Model.OpenApiModel.UserModel.Results;

namespace Model.OpenApiModel.Services
{
    public interface IOpenApi
    {
        string Address { get; }

        #region ALM Jobs methods

        /// <summary>
        /// Add ALM ChangeSummary Job using OpenAPI.
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="project">The project to have the ALM ChangeSummary job.</param>
        /// <param name="baselineOrReviewId">The baseline or review artifact ID.</param>
        /// <param name="almTarget">The ALM target.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.</param>
        /// <returns>OpenAPIJob that contains information for ALM ChangeSummary Job.</returns>
        IOpenAPIJob AddAlmChangeSummaryJob(
            IUser user,
            IProject project,
            int baselineOrReviewId,
            IAlmTarget almTarget,
            List<HttpStatusCode> expectedStatusCodes = null);

        #endregion ALM Jobs methods

        #region Artifact methods

        /// <summary>
        /// Retrieves a single artifact by Project ID and Artifact ID and returns information about the artifact.
        /// (Runs:  'GET /api/v1/projects/{projectId}/artifacts/{artifactId}'  with the following optional query parameters:
        /// status={status}, comments={comments}, traces={traces}, attachments={attachments}, richtextasplain={richtextasplain}, inlinecss={inlinecss}, content={content})
        /// </summary>
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
        IOpenApiArtifact GetArtifact(
            IProject project,
            int artifactId,
            IUser user,
            bool? getStatus = null,
            bool? getComments = null,
            OpenApiTraceTypes? getTraces = null,
            bool? getAttachments = null,
            bool? richTextAsPlain = null,
            bool? getInlineCSS = null,
            bool? getContent = null,
            List<HttpStatusCode> expectedStatusCodes = null);

        #endregion Artifact methods

        #region Attachment methods

        /// <summary>
        /// Add attachment to the specified artifact.
        /// (Runs:  'POST /api/v1/projects/{projectId}/artifacts/{artifactId}/attachments')
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="projectId">Id of project containing artifact to add attachment.</param>
        /// <param name="artifactId">Id of artifact to add attachment.</param>
        /// <param name="file">File to attach.</param>
        /// <returns>OpenApiAttachment object.</returns>
        OpenApiAttachment AddArtifactAttachment(
            IUser user,
            int projectId,
            int artifactId,
            IFile file);

        /// <summary>
        /// Add attachment to the specified sub-artifact.
        /// (Runs:  'POST /api/v1/projects/{projectId}/artifacts/{artifactId}/subartifacts/{subArtifactId}/attachments')
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="projectId">Id of project containing artifact to add attachment.</param>
        /// <param name="artifactId">Id of artifact to add attachment.</param>
        /// <param name="subArtifactId">Id of subartifact to attach file.</param>
        /// <param name="file">File to attach.</param>
        /// <returns>OpenApiAttachment object.</returns>
        OpenApiAttachment AddSubArtifactAttachment(
            IUser user,
            int projectId,
            int artifactId,
            int subArtifactId,
            IFile file);

        #endregion Attachment methods

        #region User methods

        /// <summary>

        /// Create one or more users with specified user properties.
        /// (Runs:  'POST /api/v1/users')
        /// </summary>
        /// <param name="userToAuthenticate">A user that has permission to create users.</param>
        /// <param name="usersToCreate">List of users to create.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '201 Created' is expected.</param>
        /// <returns>Collection of users which were created with returned http code and message.</returns>
        UserCallResultCollection CreateUsers(
            IUser userToAuthenticate,
            List<UserDataModel> usersToCreate,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Delete a list of users with specific usernames.
        /// (Runs:  'DELETE /api/v1/users')
        /// </summary>
        /// <param name="userToAuthenticate">A user that has permission to delete users.</param>
        /// <param name="usernamesToDelete">Usernames of users to delete.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>List of usernames with their error codes and messages that was created together with global HTTP code.</returns>
        UserCallResultCollection DeleteUsers(
            IUser userToAuthenticate,
            List<string> usernamesToDelete,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Get a user with the specified ID.
        /// (Runs:  'GET /api/v1/users/{id}')
        /// </summary>
        /// <param name="userToAuthenticate">A user that has permission to get users.</param>
        /// <param name="userId">ID of the user to get.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>The details for the specified user.</returns>
        GetUserResult GetUser(
            IUser userToAuthenticate,
            int userId,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Updates a list of users with specific usernames.
        /// (Runs:  'PATCH /api/v1/users')
        /// </summary>
        /// <param name="userToAuthenticate">A user that has permission to update users.</param>
        /// <param name="usersToUpdate">Users to update (only Type, Username and the properties being updated are required).</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>List of usernames with their error codes and messages.</returns>
        UserCallResultCollection UpdateUsers(
            IUser userToAuthenticate,
            List<UserDataModel> usersToUpdate,
            List<HttpStatusCode> expectedStatusCodes = null);

        #endregion User methods
    }
}
