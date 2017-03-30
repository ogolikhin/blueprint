using System.Collections.Generic;
using System.Linq;
using System.Net;
using Common;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Impl;
using Model.JobModel;
using Model.JobModel.Enums;
using Model.JobModel.Impl;
using Model.OpenApiModel.UserModel;
using Model.OpenApiModel.UserModel.Results;
using Newtonsoft.Json;
using NUnit.Framework;
using Utilities;
using Utilities.Facades;

namespace Model.OpenApiModel.Services
{
    /// <summary>
    /// This class contains OpenAPI REST functions.
    /// </summary>
    public class OpenApi : IOpenApi
    {
        public OpenApi(string address)
        {
            Address = address;
        }

        #region Properties and member variables.

        private string _address = null;

        /// <summary>
        /// Gets/sets the URL address of the server.  Note: any trailing '/' characters will be removed.
        /// </summary>
        public string Address
        {
            get { return _address; }
            protected set { _address = value?.TrimEnd('/'); }
        }

        #endregion Properties and member variables.

        #region Project methods

        /// <sumary>
        /// Get a project based on the project ID on the Blueprint server.
        /// (Runs:  'GET api/v1/projects/{projectId}')
        /// </sumary>
        /// <param name="address">The base Uri address of the Blueprint server.</param>
        /// <param name="projectId">The ID of the project need to be retrieved.</param>
        /// <param name="user">(optional) The user to authenticate to the server with.  Default to use no authentication.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>A project associated with the projectId provided with the request.</returns>
        public static IProject GetProject(string address, int projectId, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var restApi = new RestApiFacade(address, user?.Token.OpenApiToken);
            string path = I18NHelper.FormatInvariant(RestPaths.OpenApi.PROJECTS_id_, projectId);
            var project = restApi.SendRequestAndDeserializeObject<Project>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);

            return project;
        }

        /// <summary>
        /// Gets a list of all projects on the Blueprint server.
        /// (Runs:  'GET api/v1/projects')
        /// </summary>
        /// <param name="address">The base Uri address of the Blueprint server.</param>
        /// <param name="user">(optional) The user to authenticate to the server with.  Defaults to no authentication.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' or '206 PartialContent' are expected.</param>
        /// <returns>A list of all projects on the Blueprint server.</returns>
        public static List<IProject> GetProjects(string address, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var restApi = new RestApiFacade(address, user?.Token?.OpenApiToken);
            const string path = RestPaths.OpenApi.PROJECTS;

            // TODO: If PartialContent is returned, make additional calls to get the rest of the contents.  Big sites like Today1 get a 206 PartialContent here.
            expectedStatusCodes = expectedStatusCodes ?? new List<HttpStatusCode> { HttpStatusCode.OK, HttpStatusCode.PartialContent };

            var projects = restApi.SendRequestAndDeserializeObject<List<Project>>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);

            // VS Can't automatically convert List<Project> to List<IProject>, so we need to do it manually.
            return projects.ConvertAll(o => (IProject)o);
        }

        #endregion Project methods

        #region MetaData methods

        /// <summary>
        /// Get the all Artifact Types for the specified project.
        /// (Runs 'GET api/v1/projects/projectId/metadata/artifactTypes' with optional 'PropertyTypes' parameter.)
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
            var artifactTypes = restApi.SendRequestAndDeserializeObject<List<OpenApiArtifactType>>(
                path, 
                RestRequestMethod.GET,
                queryParameters: queryParameters, 
                expectedStatusCodes: expectedStatusCodes, 
                shouldControlJsonChanges: false);

            return artifactTypes;
        }

        #endregion MetaData methods

        #region Artifact methods

        /// <summary>
        /// Delete a single artifact on Blueprint server.  The state of the artifactToDelete object isn't updated.
        /// To delete artifact permanently, Publish must be called after the Delete, otherwise the deletion can be discarded.
        /// (Runs:  'DELETE api/v1/projects/{projectId}/artifacts/{artifactId}')
        /// </summary>
        /// <param name="address">The base address of the Blueprint server.</param>
        /// <param name="artifactToDelete">The list of artifacts to delete</param>
        /// <param name="user">The user deleting the artifact. If null, attempts to delete using the credentials
        /// of the user that created the artifact.</param>
        /// <param name="deleteChildren">(optional) Specifies whether or not to also delete all child artifacts of the specified artifact</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only '200 OK' is expected.</param>
        /// <returns>The DeletedArtifactResult list after delete artifact call</returns>
        public static List<OpenApiDeleteArtifactResult> DeleteArtifact(string address,
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
                return new List<OpenApiDeleteArtifactResult>();
            }

            // NOTE: We have to deserialize separately instead of calling restApi.SendRequestAndDeserializeObject() because if a test gets a 404 in
            // the Dispose(), it'll fail to deserialize and throw an exception.
            // TODO: Fix this limitation in the Dispose() method.
            var deleteArtifactResults = JsonConvert.DeserializeObject<List<OpenApiDeleteArtifactResult>>(response.Content);

            return deleteArtifactResults;
        }

        /// <summary>
        /// Creates a new OpenAPI artifact.  The state of the artifactToSave object isn't updated.
        /// (Runs:  'POST api/v1/projects/{projectId}/artifacts')
        /// </summary>
        /// <param name="artifactToSave">The artifact to save.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '201 Created' is expected.</param>
        /// <returns>The OpenAPI result which includes the newly created artifact as well as success/failure info.</returns>
        public static OpenApiAddArtifactResult CreateArtifact(IArtifactBase artifactToSave,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactToSave, nameof(artifactToSave));

            string path = I18NHelper.FormatInvariant(RestPaths.OpenApi.Projects_id_.ARTIFACTS, artifactToSave.ProjectId);

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            }

            var restApi = new RestApiFacade(artifactToSave.Address, user.Token?.OpenApiToken);
            var artifactResult = restApi.SendRequestAndDeserializeObject<OpenApiAddArtifactResult, ArtifactBase>(
                path,
                RestRequestMethod.POST,
                artifactToSave as ArtifactBase,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);

            return artifactResult;
        }

        /// <summary>
        /// Saves (updates) an existing OpenAPI artifact.  The state of the artifactToUpdate object isn't updated.
        /// (Runs:  'PATCH api/v1/projects/{0}/artifacts')
        /// </summary>
        /// <param name="artifactToUpdate">The artifact to update.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="propertiesToUpdate">A list of properties to be updated with their new values.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>The OpenAPI result which includes the newly created artifact as well as success/failure info.</returns>
        public static List<OpenApiUpdateArtifactResult> UpdateArtifact(IArtifactBase artifactToUpdate,
            IUser user,
            List<OpenApiPropertyForUpdate> propertiesToUpdate,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactToUpdate, nameof(artifactToUpdate));
            ThrowIf.ArgumentNull(propertiesToUpdate, nameof(propertiesToUpdate));

            string path = I18NHelper.FormatInvariant(RestPaths.OpenApi.Projects_id_.ARTIFACTS, artifactToUpdate.ProjectId);

            // Create a copy of the artifact to update that only includes the properties to be updated
            var artifactWithPropertyToUpdate = new OpenApiArtifactForUpdate
            {
                Id = artifactToUpdate.Id,
                Properties = propertiesToUpdate
            };

            var artifactsToUpdate = new List<OpenApiArtifactForUpdate> { artifactWithPropertyToUpdate };

            var restApi = new RestApiFacade(artifactToUpdate.Address, user.Token?.OpenApiToken);
            var updateResultList = restApi.SendRequestAndDeserializeObject<List<OpenApiUpdateArtifactResult>, List<OpenApiArtifactForUpdate>>(
                path,
                RestRequestMethod.PATCH,
                artifactsToUpdate,
                expectedStatusCodes: expectedStatusCodes);

            return updateResultList;
        }

        /// <seealso cref="IOpenApi.GetArtifact(IProject, int, IUser, bool?, bool?, OpenApiTraceTypes?, bool?, bool?, bool?, bool?, List{HttpStatusCode})"/>
        public IOpenApiArtifact GetArtifact(
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
            return GetArtifact(Address, project, artifactId, user,
                getStatus: getStatus,
                getComments: getComments,
                getTraces: getTraces,
                getAttachments: getAttachments,
                richTextAsPlain: richTextAsPlain,
                getInlineCSS: getInlineCSS,
                getContent: getContent,
                expectedStatusCodes: expectedStatusCodes);
        }

        /// <summary>
        /// Retrieves a single artifact by Project ID and Artifact ID and returns information about the artifact.
        /// (Runs:  'GET /api/v1/projects/{projectId}/artifacts/{artifactId}'  with the following optional query parameters:
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
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);

            returnedArtifact.Address = baseAddress;

            return returnedArtifact;
        }

        /// <summary>
        /// Gets the Version property of an Artifact via OpenAPI call.
        /// (Runs:  'GET /api/v1/projects/{projectId}/artifacts/{artifactId}')
        /// </summary>
        /// <param name="address">The base address of the Blueprint server.</param>
        /// <param name="artifact">The artifact</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
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

            var returnedArtifact = GetArtifact(address, artifact.Project, artifact.Id, user, expectedStatusCodes: expectedStatusCodes);

            return returnedArtifact.Version;
        }

        #endregion Artifact methods

        #region Attachment methods

        /// <summary>
        /// Add attachment to the specified artifact.
        /// (Runs:  'POST /api/v1/projects/{projectId}/artifacts/{artifactId}/attachments')
        /// </summary>
        /// <param name="address">The base URL of the Blueprint server.</param>
        /// <param name="projectId">Id of project containing artifact to add attachment.</param>
        /// <param name="artifactId">Id of artifact to add attachment.</param>
        /// <param name="file">File to attach.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '201 Created' is expected.</param>
        /// <returns>OpenApiAttachment object.</returns>
        public static OpenApiAttachment AddArtifactAttachment(string address,
            int projectId,
            int artifactId,
            IFile file,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(file, nameof(file));

            string path = I18NHelper.FormatInvariant(RestPaths.OpenApi.Projects_id_.Artifacts_id_.ATTACHMENTS, projectId, artifactId);

            return AddItemAttachment(address, path, file, user, expectedStatusCodes);
        }

        /// <summary>
        /// Add attachment to the specified sub-artifact.
        /// (Runs:  'POST /api/v1/projects/{projectId}/artifacts/{artifactId}/subartifacts/{subArtifactId}/attachments')
        /// </summary>
        /// <param name="address">The base URL of the Blueprint server.</param>
        /// <param name="projectId">Id of project containing artifact to add attachment.</param>
        /// <param name="artifactId">Id of artifact to add attachment.</param>
        /// <param name="subArtifactId">Id of subartifact to attach file.</param>
        /// <param name="file">File to attach.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '201 Created' is expected.</param>
        /// <returns>OpenApiAttachment object.</returns>
        public static OpenApiAttachment AddSubArtifactAttachment(string address,
            int projectId,
            int artifactId,
            int subArtifactId,
            IFile file,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(file, nameof(file));

            string path = I18NHelper.FormatInvariant(RestPaths.OpenApi.Projects_id_.Artifacts_id_.SubArtifacts_id_.ATTACHMENTS,
                projectId, artifactId, subArtifactId);

            return AddItemAttachment(address, path, file, user, expectedStatusCodes);
        }

        /// <summary>
        /// Add attachment to the specified artifact/subartifact.
        /// </summary>
        /// <param name="address">The base URL of the Blueprint server.</param>
        /// <param name="path">Path to add attachment.</param>
        /// <param name="file">File to attach.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '201 Created' is expected.</param>
        /// <returns>OpenApiAttachment object.</returns>
        private static OpenApiAttachment AddItemAttachment(string address,
            string path,
            IFile file,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(file, nameof(file));

            var restApi = new RestApiFacade(address, user.Token?.OpenApiToken);
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

        #endregion Attachment methods

        #region Trace methods

        /// <summary>
        /// Add trace between two artifacts (or artifact and sub-artifact) with specified properties.
        /// (Runs:  'POST /api/v1/projects/{projectId}/artifacts/{artifactId}/traces')
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
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '201 Created' is expected.</param>
        /// <returns>List of OpenApiTrace objects for all traces that were added.</returns>
        public static List<OpenApiTrace> AddTrace(string address,
            IArtifactBase sourceArtifact,
            IArtifactBase targetArtifact,   // TODO: Create an AddTrace() that takes a list of target artifacts.
            TraceDirection traceDirection,
            IUser user,
            OpenApiTraceTypes traceType = OpenApiTraceTypes.Manual,
            bool isSuspect = false,
            int? subArtifactId = null,
            bool? reconcileWithTwoWay = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(sourceArtifact, nameof(sourceArtifact));
            ThrowIf.ArgumentNull(targetArtifact, nameof(targetArtifact));

            string path = I18NHelper.FormatInvariant(RestPaths.OpenApi.Projects_id_.Artifacts_id_.TRACES,
                sourceArtifact.ProjectId, sourceArtifact.Id);

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            }

            Dictionary<string, string> queryParameters = null;

            if (reconcileWithTwoWay != null)
            {
                queryParameters = new Dictionary<string, string> { { "reconcilewithtwoway", reconcileWithTwoWay.ToString() } };
            }

            var traceToCreate = new OpenApiTrace(targetArtifact.ProjectId, targetArtifact,
                traceDirection, traceType, isSuspect, subArtifactId);

            var restApi = new RestApiFacade(address, user.Token?.OpenApiToken);

            var openApiTraces = restApi.SendRequestAndDeserializeObject<List<OpenApiTrace>, List<OpenApiTrace>>(
                path,
                RestRequestMethod.POST,
                new List<OpenApiTrace> { traceToCreate },
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes);

            return openApiTraces;
        }

        /// <summary>
        /// Delete trace between two artifacts (or artifact and sub-artifact) with specified properties.
        /// (Runs:  'DELETE /api/v1/projects/{projectId}/artifacts/{artifactId}/traces')
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
        ///     the request does not return an error; instead, the trace Type is set to TwoWay.  The default is null and acts the same as false.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only '201' is expected.</param>
        /// <returns>List of OpenApiTrace objects for all traces that were deleted.</returns>
        public static List<OpenApiTrace> DeleteTrace(string address,
            IArtifactBase sourceArtifact,
            IArtifactBase targetArtifact,   // TODO: Create an DeleteTrace() that takes a list of target artifacts.
            TraceDirection traceDirection,
            IUser user,
            OpenApiTraceTypes traceType = OpenApiTraceTypes.Manual,
            bool isSuspect = false,
            int? subArtifactId = null,
            bool? reconcileWithTwoWay = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(sourceArtifact, nameof(sourceArtifact));
            ThrowIf.ArgumentNull(targetArtifact, nameof(targetArtifact));

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

            var traceToDelete = new OpenApiTrace(targetArtifact.ProjectId, targetArtifact,
                traceDirection, traceType, isSuspect, subArtifactId);

            var restApi = new RestApiFacade(address, user.Token?.OpenApiToken);

            var openApiTraces = restApi.SendRequestAndDeserializeObject<List<OpenApiTrace>, List<OpenApiTrace>>(
                path,
                RestRequestMethod.DELETE,
                new List<OpenApiTrace> { traceToDelete },
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);

            return openApiTraces;
        }

        #endregion Trace methods

        #region User methods

        /// <seealso cref="IOpenApi.CreateUsers(IUser, List{UserDataModel}, List{HttpStatusCode})"/>
        public UserCallResultCollection CreateUsers(
            IUser userToAuthenticate,
            List<UserDataModel> usersToCreate,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(usersToCreate, nameof(usersToCreate));

            expectedStatusCodes = expectedStatusCodes ?? new List<HttpStatusCode> { HttpStatusCode.Created };

            var restApi = new RestApiFacade(Address, userToAuthenticate?.Token?.OpenApiToken);
            string path = RestPaths.OpenApi.USERS;

            return restApi.SendRequestAndDeserializeObject<UserCallResultCollection, List<UserDataModel>>(
                path,
                RestRequestMethod.POST,
                usersToCreate,
                expectedStatusCodes: expectedStatusCodes);
        }

        /// <seealso cref="IOpenApi.DeleteUsers(IUser, List{string}, List{HttpStatusCode})"/>
        public UserCallResultCollection DeleteUsers(
            IUser userToAuthenticate,
            List<string> usernamesToDelete,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(usernamesToDelete, nameof(usernamesToDelete));

            var restApi = new RestApiFacade(Address, userToAuthenticate?.Token?.OpenApiToken);
            string path = RestPaths.OpenApi.USERS;

            return restApi.SendRequestAndDeserializeObject<UserCallResultCollection, List<string>>(
                path,
                RestRequestMethod.DELETE,
                usernamesToDelete,
                expectedStatusCodes: expectedStatusCodes);
        }

        /// <seealso cref="IOpenApi.GetUser(IUser, int, List{HttpStatusCode})"/>
        public GetUserResult GetUser(
            IUser userToAuthenticate,
            int userId,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            var restApi = new RestApiFacade(Address, userToAuthenticate?.Token?.OpenApiToken);
            string path = I18NHelper.FormatInvariant(RestPaths.OpenApi.Users.GET_id_, userId);

            return restApi.SendRequestAndDeserializeObject<GetUserResult>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);
        }

        /// <seealso cref="IOpenApi.UpdateUsers(IUser, List{UserDataModel}, List{HttpStatusCode})"/>
        public UserCallResultCollection UpdateUsers(
            IUser userToAuthenticate,
            List<UserDataModel> usersToUpdate,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(usersToUpdate, nameof(usersToUpdate));

            var restApi = new RestApiFacade(Address, userToAuthenticate?.Token?.OpenApiToken);
            string path = RestPaths.OpenApi.USERS;

            return restApi.SendRequestAndDeserializeObject<UserCallResultCollection, List<UserDataModel>>(
                path,
                RestRequestMethod.PATCH,
                usersToUpdate,
                expectedStatusCodes: expectedStatusCodes);
        }

        #endregion User methods

        #region Version Control methods

        /// <summary>
        /// Discard changes to artifact(s) from Blueprint
        /// </summary>
        /// <param name="artifactsToDiscard">The artifact(s) to be discarded.</param>
        /// <param name="address">The base url of the Open API</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>The list of ArtifactResult objects created by the dicard artifacts request</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        public static List<DiscardArtifactResult> DiscardArtifacts(List<IArtifactBase> artifactsToDiscard,
            string address,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactsToDiscard, nameof(artifactsToDiscard));

            // Create a list of OpenApiVersionControlRequest from the artifacts to publish with the minimum required properties.
            var discardRequestArtifacts = new List<OpenApiVersionControlRequest>();
            artifactsToDiscard.ForEach(a => discardRequestArtifacts.Add(new OpenApiVersionControlRequest(a)));

            var restApi = new RestApiFacade(address, user.Token?.OpenApiToken);
            var artifactResults = restApi.SendRequestAndDeserializeObject<List<DiscardArtifactResult>, List<OpenApiVersionControlRequest>>(
                RestPaths.OpenApi.VersionControl.DISCARD,
                RestRequestMethod.POST,
                discardRequestArtifacts,
                expectedStatusCodes: expectedStatusCodes);

            return artifactResults;
        }

        /// <summary>
        /// Publish Artifact(s) (Used when publishing a single artifact OR a list of artifacts).
        /// NOTE: This function won't update the internal status flags used by automation.
        /// (Runs:  'POST api/v1/vc/publish')
        /// </summary>
        /// <param name="artifactsToPublish">The list of artifacts to publish</param>
        /// <param name="address">The base url of the Open API</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="shouldKeepLock">(optional) Boolean parameter which defines whether or not to keep the lock after publishing the artfacts</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only '200 OK' is expected.</param>
        /// <returns>The list of OpenApiPublishArtifactResult objects created by the publish artifacts request.</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        public static List<OpenApiPublishArtifactResult> PublishArtifacts(List<IArtifactBase> artifactsToPublish,
            string address,
            IUser user,
            bool shouldKeepLock = false,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactsToPublish, nameof(artifactsToPublish));

            var additionalHeaders = new Dictionary<string, string>();

            if (shouldKeepLock)
            {
                additionalHeaders.Add("KeepLock", "true");
            }

            // Create a list of OpenApiVersionControlRequest from the artifacts to publish with the minimum required properties.
            var publishRequestArtifacts = new List<OpenApiVersionControlRequest>();
            artifactsToPublish.ForEach(a => publishRequestArtifacts.Add(new OpenApiVersionControlRequest(a)));

            var restApi = new RestApiFacade(address, user.Token?.OpenApiToken);
            var publishedResultList = restApi.SendRequestAndDeserializeObject<List<OpenApiPublishArtifactResult>, List<OpenApiVersionControlRequest>>(
                RestPaths.OpenApi.VersionControl.PUBLISH,
                RestRequestMethod.POST,
                publishRequestArtifacts,
                additionalHeaders: additionalHeaders,
                expectedStatusCodes: expectedStatusCodes);

            return publishedResultList;
        }

        #endregion Version Control methods

        #region ALM Jobs methods

        /// <seealso cref="IOpenApi.AddAlmChangeSummaryJob(IUser, IProject, int, IAlmTarget, List{HttpStatusCode})"/>
        public IOpenAPIJob AddAlmChangeSummaryJob(
            IUser user,
            IProject project,
            int baselineOrReviewId,
            IAlmTarget almTarget,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(almTarget, nameof(almTarget));

            string path = I18NHelper.FormatInvariant(RestPaths.OpenApi.Projects_id_.ALM.Targets_id_.JOBS, project.Id, almTarget.Id);
            var almJob = new AlmJob(AlmJobType.ChangeSummary, baselineOrReviewId);

            var restApi = new RestApiFacade(Address, user?.Token?.OpenApiToken);
            var returnedAlmChangeSummaryJob = restApi.SendRequestAndDeserializeObject<OpenAPIJob, AlmJob>(
                path,
                RestRequestMethod.POST,
                almJob,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);

            return returnedAlmChangeSummaryJob;
        }

        #endregion ALM Jobs methods
    }
}
