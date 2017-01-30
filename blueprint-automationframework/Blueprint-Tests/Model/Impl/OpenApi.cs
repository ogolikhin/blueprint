using System.Collections.Generic;
using System.Net;
using Common;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Newtonsoft.Json;
using NUnit.Framework;
using Utilities;
using Utilities.Facades;

namespace Model.Impl
{
    /// <summary>
    /// This class contains OpenAPI REST functions.
    /// </summary>
    public static class OpenApi
    {
        private const string SessionTokenCookieName = "BLUEPRINT_SESSION_TOKEN";

        #region Project methods

        /// <sumary>
        /// Get a project based on the project ID on the Blueprint server.
        /// </sumary>
        /// <param name="address">The base Uri address of the Blueprint server.</param>
        /// <param name="projectId">The ID of the project need to be retrieved.</param>
        /// <param name="user">(optional) The user to authenticate to the server with.  Default to use no authentication.</param>
        /// <returns>A project associated with the projectId provided with the request.</returns>
        public static IProject GetProject(string address, int projectId, IUser user = null)
        {
            var restApi = new RestApiFacade(address, user?.Token.OpenApiToken);
            string path = I18NHelper.FormatInvariant(RestPaths.OpenApi.PROJECTS_id_, projectId);
            var project = restApi.SendRequestAndDeserializeObject<Project>(path, RestRequestMethod.GET);

            return project;
        }

        /// <summary>
        /// Gets a list of all projects on the Blueprint server.
        /// </summary>
        /// <param name="address">The base Uri address of the Blueprint server.</param>
        /// <param name="user">(optional) The user to authenticate to the server with.  Defaults to no authentication.</param>
        /// <returns>A list of all projects on the Blueprint server.</returns>
        public static List<IProject> GetProjects(string address, IUser user = null)
        {
            var restApi = new RestApiFacade(address, user?.Token?.OpenApiToken);
            const string path = RestPaths.OpenApi.PROJECTS;

            var projects = restApi.SendRequestAndDeserializeObject<List<Project>>(path, RestRequestMethod.GET);

            // VS Can't automatically convert List<Project> to List<IProject>, so we need to do it manually.
            return projects.ConvertAll(o => (IProject)o);
        }

        #endregion Project methods

        #region MetaData methods

        /// <summary>
        /// Get the all Artifact Types for the specified project.
        /// Runs 'GET api/v1/projects/projectId/metadata/artifactTypes' with optional 'PropertyTypes' parameter.
        /// </summary>
        /// <param name="address">The base Uri address of the Blueprint server.</param>
        /// <param name="projectId">The ID of the project whose artifact types you want to get.</param>
        /// <param name="user">The user to authenticate to the the server with.  Defaults to no authentication.</param>
        /// <param name="shouldRetrievePropertyTypes">(optional) Defines whether or not to include property types.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>A list of artifact types which was retrieved for the project.</returns>
        public static List<OpenApiArtifactType> GetAllArtifactTypes(
            string address,
            int projectId,
            IUser user,
            bool shouldRetrievePropertyTypes = false,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            var path = I18NHelper.FormatInvariant(RestPaths.OpenApi.Projects_id_.MetaData.ARTIFACT_TYPES, projectId);
            var queryParameters = new Dictionary<string, string>();

            if (shouldRetrievePropertyTypes)
            {
                queryParameters.Add("PropertyTypes", "true");
            }

            // Retrieve the artifact type list for the project 
            var restApi = new RestApiFacade(address, user.Token?.OpenApiToken);
            var artifactTypes = restApi.SendRequestAndDeserializeObject<List<OpenApiArtifactType>>(path, RestRequestMethod.GET,
                queryParameters: queryParameters, expectedStatusCodes: expectedStatusCodes);

            return artifactTypes;
        }

        #endregion MetaData methods

        #region Artifact methods

        /// <summary>
        /// Delete a single artifact on Blueprint server.
        /// To delete artifact permanently, Publish must be called after the Delete, otherwise the deletion can be discarded.
        /// </summary>
        /// <param name="address">The base address of the Blueprint server.</param>
        /// <param name="artifactToDelete">The list of artifacts to delete</param>
        /// <param name="user">The user deleting the artifact. If null, attempts to delete using the credentials
        /// of the user that created the artifact.</param>
        /// <param name="deleteChildren">(optional) Specifies whether or not to also delete all child artifacts of the specified artifact</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>The DeletedArtifactResult list after delete artifact call</returns>
        public static List<DeleteArtifactResult> DeleteArtifact(string address,
            IArtifactBase artifactToDelete,
            IUser user,
            bool? deleteChildren = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactToDelete, nameof(artifactToDelete));

            string path = I18NHelper.FormatInvariant(RestPaths.OpenApi.Projects_id_.ARTIFACTS_id_, artifactToDelete.ProjectId,
                artifactToDelete.Id);

            var queryparameters = new Dictionary<string, string>();

            if (deleteChildren ?? artifactToDelete.ShouldDeleteChildren)
            {
                Logger.WriteDebug("*** Recursively deleting children for artifact ID: {0}.", artifactToDelete.Id);
                queryparameters.Add("Recursively", "true");
            }

            var restApi = new RestApiFacade(address, user.Token?.OpenApiToken);
            var response = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.DELETE,
                queryParameters: queryparameters,
                expectedStatusCodes: expectedStatusCodes);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var artifactResults = JsonConvert.DeserializeObject<List<DeleteArtifactResult>>(response.Content);

            // For all artifacts that were deleted, update their internal flags to indicate they're marked for deletion.
            UpdateStateOfDeletedArtifacts(artifactResults, artifactToDelete, user, path);

            return artifactResults;
        }

        /// <summary>
        /// Updates the internal flags of all artifacts affected by the deleted artifact to indicate they are now marked for deletion.
        /// </summary>
        /// <param name="artifactResults">The results of the OpenAPI delete artifact call.</param>
        /// <param name="artifactToDelete">The main artifact that was deleted.</param>
        /// <param name="user">The user who deleted the artifact(s).</param>
        /// <param name="path">The path of the delete REST call.</param>
        private static void UpdateStateOfDeletedArtifacts(List<DeleteArtifactResult> artifactResults,
            IArtifactBase artifactToDelete,
            IUser user,
            string path)
        {
            var artifaceBaseToDelete = artifactToDelete as ArtifactBase;

            foreach (var deletedArtifactResult in artifactResults)
            {
                Logger.WriteDebug("DELETE {0} returned following: ArtifactId: {1} Message: {2}, ResultCode: {3}",
                    path, deletedArtifactResult.ArtifactId, deletedArtifactResult.Message, deletedArtifactResult.ResultCode);

                if (deletedArtifactResult.ResultCode == HttpStatusCode.OK)
                {
                    artifaceBaseToDelete.DeletedArtifactResults.Add(deletedArtifactResult);

                    if (deletedArtifactResult.ArtifactId == artifactToDelete.Id)
                    {
                        if (artifactToDelete.IsPublished)
                        {
                            artifactToDelete.IsMarkedForDeletion = true;
                            artifaceBaseToDelete.LockOwner = user;
                        }
                        else
                        {
                            artifactToDelete.IsDeleted = true;
                        }
                    }
                }
            }
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
            OpenApiTraceTypes? getTraces = null,
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

            if (getAttachments != null)
            { queryParameters.Add("Attachments", getAttachments.ToString()); }

            if (getComments != null)
            { queryParameters.Add("Comments", getComments.ToString()); }

            if (getContent != null)
            { queryParameters.Add("Content", getContent.ToString()); }

            if (getInlineCSS != null)
            { queryParameters.Add("InlineCSS", getInlineCSS.ToString()); }

            if (getStatus != null)
            { queryParameters.Add("Status", getStatus.ToString()); }

            if (getTraces != null)
            { queryParameters.Add("Traces", getTraces.ToString()); }

            if (richTextAsPlain != null)
            { queryParameters.Add("RichTextAsPlain", richTextAsPlain.ToString()); }

            var restApi = new RestApiFacade(baseAddress, user.Token?.OpenApiToken);
            var path = I18NHelper.FormatInvariant(RestPaths.OpenApi.Projects_id_.ARTIFACTS_id_, project.Id, artifactId);

            var returnedArtifact = restApi.SendRequestAndDeserializeObject<OpenApiArtifact>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes);

            returnedArtifact.Address = baseAddress;

            return returnedArtifact;
        }

        /// <summary>
        /// Gets the Version property of an Artifact via OpenAPI call
        /// </summary>
        /// <param name="address">The base address of the Blueprint server.</param>
        /// <param name="artifact">The artifact</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>The historical version of the artifact.</returns>
        public static int GetArtifactVersion(string address,
            IArtifactBase artifact,
            IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            if (user == null)
            {
                Assert.NotNull(artifact.CreatedBy, "No user is available to perform GetVersion.");
                user = artifact.CreatedBy;
            }

            var restApi = new RestApiFacade(address, user.Token?.OpenApiToken);
            var path = I18NHelper.FormatInvariant(RestPaths.OpenApi.Projects_id_.ARTIFACTS_id_, artifact.ProjectId, artifact.Id);

            var returnedArtifact = restApi.SendRequestAndDeserializeObject<ArtifactBase>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);

            return returnedArtifact.Version;
        }

        #endregion Artifact methods

    }
}
