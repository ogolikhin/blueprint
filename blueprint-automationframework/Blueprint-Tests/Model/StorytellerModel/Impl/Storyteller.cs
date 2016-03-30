using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Common;
using Model.Factories;
using Model.Impl;
using Model.OpenApiModel;
using Utilities;
using Utilities.Facades;

namespace Model.StorytellerModel.Impl
{
    public class Storyteller : IStoryteller
    {
        private const string SVC_PATH = "svc/components/storyteller";
        private const string URL_PROJECTS = "projects";
        private const string URL_PROCESSES = "processes";
        private const string URL_USERSTORIES = "userstories";
        private const string URL_ARTIFACTTYPES = "artifacttypes/userstory";
        private const string SessionTokenCookieName = "BLUEPRINT_SESSION_TOKEN";

        private const string SVC_UPLOAD_PATH = "svc/components/filestore/files";

        public string Address { get; }

        public List<IOpenApiArtifact> Artifacts { get; } = new List<IOpenApiArtifact>();

        #region Constructor

        public Storyteller(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            Address = address;
        }

        #endregion Constructor

        #region Implemented from IStoryteller

        public IOpenApiArtifact CreateAndSaveProcessArtifact(IProject project, BaseArtifactType artifactType, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            //Create an artifact with ArtifactType and populate all required values without properties
            var artifact = ArtifactFactory.CreateOpenApiArtifact(Address, user, project, artifactType);

            //Set to add in root of the project
            artifact.ParentId = artifact.ProjectId;

            //add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
            artifact.Save(user);

            // Add artifact to artifacts list
            Artifacts.Add(artifact);

            return artifact;
        }

        public List<IOpenApiArtifact> CreateAndPublishProcessArtifacts(IProject project, IUser user, int numberOfArtifacts)
        {
            var artifacts = new List<IOpenApiArtifact>();

            for (int i = 0; i < numberOfArtifacts; i++)
            {
                var artifact = CreateAndSaveProcessArtifact(project, BaseArtifactType.Process, user);
                artifact.Publish(user);
                artifacts.Add(artifact);
            }

            return artifacts;
        }

        public List<IStorytellerUserStory> GenerateUserStories(IUser user, IProcess process, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            string path = I18NHelper.FormatInvariant("{0}/{1}", SVC_PATH, URL_PROJECTS);

            ThrowIf.ArgumentNull(process, nameof(process));
            path = I18NHelper.FormatInvariant("{0}/{1}/{2}/{3}/{4}", path, process.ProjectId, URL_PROCESSES, process.Id, URL_USERSTORIES);

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            }

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            var additionalHeaders = new Dictionary<string, string>();

            RestApiFacade restApi = new RestApiFacade(Address, user.Username, user.Password, tokenValue);

            var userstoryResults = restApi.SendRequestAndDeserializeObject<List<StorytellerUserStory>>(path, RestRequestMethod.POST, additionalHeaders: additionalHeaders, expectedStatusCodes: expectedStatusCodes);

            return userstoryResults.ConvertAll(o => (IStorytellerUserStory)o);
        }

        public IProcess GetProcess(IUser user, int artifactId, int? versionIndex = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            string path = I18NHelper.FormatInvariant("{0}/processes/{1}", SVC_PATH, artifactId);
            if (versionIndex.HasValue)
            {
                path = I18NHelper.FormatInvariant("{0}/{1}", path, versionIndex);
            }

            var restApi = new RestApiFacade(Address, user.Username, user.Password, tokenValue);

            var response = restApi.SendRequestAndDeserializeObject<Process>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies);

            return response;
        }

        public IList<IProcess> GetProcesses(IUser user, int projectId, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            string path = I18NHelper.FormatInvariant("{0}/projects/{1}/processes", SVC_PATH, projectId);

            var restApi = new RestApiFacade(Address, user.Username, user.Password, tokenValue);

            var response = restApi.SendRequestAndDeserializeObject<List<Process>>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies);

            return response.ConvertAll(o => (IProcess)o);
        }

        public int GetProcessArtifactTypeId(IUser user, IProject project)
        {
            ThrowIf.ArgumentNull(project, nameof(project));
            BaseArtifactType processTypeName = BaseArtifactType.Process;

            return project.GetArtifactTypeId(address: Address, user: user, baseArtifactTypeName: processTypeName,
                projectId: project.Id);
        }

        public IProcess GetProcessWithBreadcrumb(IUser user, List<int> artifactIds, int? versionIndex = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactIds, nameof(artifactIds));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            string path = I18NHelper.FormatInvariant("{0}/processes", SVC_PATH);

            foreach (var id in artifactIds)
            {
                path = I18NHelper.FormatInvariant("{0}/{1}", path, id);
            }

            if (versionIndex.HasValue)
            {
                path = I18NHelper.FormatInvariant("{0}/{1}", path, versionIndex);
            }

            var restApi = new RestApiFacade(Address, user.Username, user.Password, tokenValue);

            var response = restApi.SendRequestAndDeserializeObject<Process>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies);

            return response;
        }

        public IArtifactType GetUserStoryArtifactType(IUser user, int projectId, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            }

            string path = I18NHelper.FormatInvariant("{0}/{1}/{2}/{3}", SVC_PATH, URL_PROJECTS, projectId, URL_ARTIFACTTYPES);

            var restApi = new RestApiFacade(Address, user.Username, user.Password, tokenValue);

            var response = restApi.SendRequestAndDeserializeObject<ArtifactType>(path, RestRequestMethod.GET, expectedStatusCodes: expectedStatusCodes, cookies: cookies);

            return response;
        }

        public IProcess UpdateProcess(IUser user, IProcess process, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(process, nameof(process));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            string path = I18NHelper.FormatInvariant("{0}/processes/{1}", SVC_PATH, process.Id);

            var restApi = new RestApiFacade(Address, user.Username, user.Password, tokenValue);

            var updateProcessResult = restApi.SendRequestAndDeserializeObject<UpdateResult<Process>, Process>(
                path,
                RestRequestMethod.PATCH,
                (Process)process,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies);

            // Mark artifact in artifact list as saved
            MarkArtifactAsSaved(process.Id);

            return updateProcessResult.Result;
        }

        public string UpdateProcessReturnResponseOnly(IUser user, IProcess process, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(process, nameof(process));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            string path = I18NHelper.FormatInvariant("{0}/processes/{1}", SVC_PATH, process.Id);

            var restApi = new RestApiFacade(Address, user.Username, user.Password, tokenValue);

            var restResponse = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.PATCH,
                bodyObject: (Process)process,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies);

            // Mark artifact in artifact list as saved
            MarkArtifactAsSaved(process.Id);

            return restResponse.Content;
        }

        public string UploadFile(IUser user, IFile file, DateTime? expireDate = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(file, nameof(file));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            }

            var additionalHeaders = new Dictionary<string, string>();

            string path = I18NHelper.FormatInvariant("{0}/{1}", SVC_UPLOAD_PATH, file.FileName);
            if (expireDate != null)
            {
                DateTime time = (DateTime)expireDate;
                path = I18NHelper.FormatInvariant("{0}/?expired={1}", path, time.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'", CultureInfo.InvariantCulture));
            }

            byte[] bytes = file.Content.ToArray();

            RestApiFacade restApi = new RestApiFacade(Address, user.Username, user.Password, tokenValue);
            var artifactResult = restApi.SendRequestAndGetResponse(path, RestRequestMethod.POST, fileName: file.FileName, fileContent: bytes, contentType: "application/json;charset=utf8", additionalHeaders: additionalHeaders, expectedStatusCodes: expectedStatusCodes, cookies: cookies);

            return artifactResult.Content;
        }

        public string PublishProcess(IUser user, IProcess process, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(process, nameof(process));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            }

            string path = I18NHelper.FormatInvariant("{0}/{1}/{2}", SVC_PATH, URL_PROCESSES, process.Id);

            RestApiFacade restApi = new RestApiFacade(Address, user.Username, user.Password, tokenValue);

            var publishProcessResult = restApi.SendRequestAndGetResponse(
                path, 
                RestRequestMethod.POST, 
                expectedStatusCodes: expectedStatusCodes);

            // Mark artifact in artifact list as published
            MarkArtifactAsPublished(process.Id);

            return publishProcessResult.Content;
        }

        public List<IDeleteArtifactResult> DeleteProcessArtifact(IOpenApiArtifact artifact, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false, bool deleteChildren = false)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            Artifacts.Remove(Artifacts.First(i => i.Id.Equals(artifact.Id)));
            return artifact.Delete(artifact.CreatedBy, expectedStatusCodes, sendAuthorizationAsCookie: sendAuthorizationAsCookie, deleteChildren: deleteChildren);
        }

        #endregion Implemented from IStoryteller

        #region Private Methods

        /// <summary>
        /// Mark the Artifact as Published (Indicates artifact has no pending changes)
        /// </summary>
        /// <param name="artifactId">The id of the artifact to be published</param>
        private void MarkArtifactAsPublished(int artifactId)
        {
            var publishedArtifact = Artifacts.Find(artifact => artifact.Id == artifactId);
            publishedArtifact.IsSaved = false;
            publishedArtifact.IsPublished = true;
        }

        /// <summary>
        /// Mark the Artifact as Saved (Indicates artifact has pending changes)
        /// </summary>
        /// <param name="artifactId">The id of the artifact to be saved</param>
        private void MarkArtifactAsSaved(int artifactId)
        {
            var publishedArtifact = Artifacts.Find(artifact => artifact.Id == artifactId);
            publishedArtifact.IsSaved = true;
        }

        #endregion Private Methods

    }
}
