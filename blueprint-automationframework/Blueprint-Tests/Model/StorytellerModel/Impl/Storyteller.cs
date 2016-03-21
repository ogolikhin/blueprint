using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Common;
using Model.Factories;
using Model.Impl;
using Model.OpenApiModel;
using Model.OpenApiModel.Impl;
using Utilities;
using Utilities.Facades;

namespace Model.StorytellerModel.Impl
{
    public class Storyteller : IStoryteller
    {
        private const string SVC_PATH = "svc/components/storyteller";
        private const string URL_PUBLISH = "api/v1/vc/publish";
        private const string URL_PROJECTS = "projects";
        private const string URL_PROCESSES = "processes";
        private const string URL_USERSTORIES = "userstories";
        private const string URL_ARTIFACTTYPES = "artifacttypes/userstory";
        private const string SessionTokenCookieName = "BLUEPRINT_SESSION_TOKEN";

        private const string SVC_UPLOAD_PATH = "svc/components/filestore/files";

        private IOpenApiArtifact _artifact;
        private readonly string _address;
        public string Address
        {
            get
            {
                return _address;
            }
        }
        public List<IOpenApiArtifact> Artifacts { get; } = new List<IOpenApiArtifact>();

        #region Constructor

        public Storyteller(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            _address = address;
            _artifact = new OpenApiArtifact(_address);
            _artifact.BaseArtifactType = BaseArtifactType.Process;
        }

        #endregion Constructor

        #region Implemented from IStoryteller

        public IOpenApiArtifact CreateAndSaveProcessArtifact(IProject project, BaseArtifactType artifactType, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            //Create an artifact with ArtifactType and populate all required values without properties
            _artifact = ArtifactFactory.CreateOpenApiArtifact(_address, user, project, artifactType);

            //Set to add in root of the project
            _artifact.ParentId = _artifact.ProjectId;

            //add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
            var artifact = _artifact.AddArtifact(_artifact, user);
            _artifact.Id = artifact.Id;

            // Add artifact to artifacts list
            Artifacts.Add(_artifact);

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
            var path = I18NHelper.FormatInvariant("{0}/{1}", SVC_PATH, URL_PROJECTS);

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

            Dictionary<string, string> additionalHeaders = new Dictionary<string, string>();

            RestApiFacade restApi = new RestApiFacade(_address, user.Username, user.Password, tokenValue);

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

            var path = I18NHelper.FormatInvariant("{0}/processes/{1}", SVC_PATH, artifactId);
            if (versionIndex.HasValue)
            {
                path = I18NHelper.FormatInvariant("{0}/{1}", path, versionIndex);
            }

            var restApi = new RestApiFacade(_address, user.Username, user.Password, tokenValue);

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

            var path = I18NHelper.FormatInvariant("{0}/projects/{1}/processes", SVC_PATH, projectId);

            var restApi = new RestApiFacade(_address, user.Username, user.Password, tokenValue);

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
            return project.GetArtifactTypeId(address: _address, user: user, baseArtifactTypeName: processTypeName,
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

            var path = I18NHelper.FormatInvariant("{0}/processes", SVC_PATH);

            foreach (var id in artifactIds)
            {
                path = I18NHelper.FormatInvariant("{0}/{1}", path, id);
            }

            if (versionIndex.HasValue)
            {
                path = I18NHelper.FormatInvariant("{0}/{1}", path, versionIndex);
            }

            var restApi = new RestApiFacade(_address, user.Username, user.Password, tokenValue);

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

            var path = I18NHelper.FormatInvariant("{0}/{1}/{2}/{3}", SVC_PATH, URL_PROJECTS, projectId, URL_ARTIFACTTYPES);

            var restApi = new RestApiFacade(_address, user.Username, user.Password, tokenValue);

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

            var path = I18NHelper.FormatInvariant("{0}/processes/{1}", SVC_PATH, process.Id);

            var restApi = new RestApiFacade(_address, user.Username, user.Password, tokenValue);

            var updateProcessResult = restApi.SendRequestAndDeserializeObject<UpdateResult<Process>, Process>(
                path,
                RestRequestMethod.PATCH,
                (Process)process,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies);

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

            var path = I18NHelper.FormatInvariant("{0}/processes/{1}", SVC_PATH, process.Id);

            var restApi = new RestApiFacade(_address, user.Username, user.Password, tokenValue);

            var restResponse = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.PATCH,
                bodyObject: (Process)process,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies);

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

            Dictionary<string, string> additionalHeaders = new Dictionary<string, string>();

            additionalHeaders.Add("Accept", "application/json");

            string path = I18NHelper.FormatInvariant("{0}/{1}", SVC_UPLOAD_PATH, file.FileName);
            if (expireDate != null)
            {
                DateTime time = (DateTime)expireDate;
                path = I18NHelper.FormatInvariant("{0}/?expired={1}", path, time.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'", CultureInfo.InvariantCulture));
            }

            byte[] bytes = file.Content.ToArray<byte>();

            RestApiFacade restApi = new RestApiFacade(_address, user.Username, user.Password, tokenValue);
            var artifactResult = restApi.SendRequestAndGetResponse(path, RestRequestMethod.POST, fileName: file.FileName, fileContent: bytes, contentType: "application/json;charset=utf8", additionalHeaders: additionalHeaders, expectedStatusCodes: expectedStatusCodes, cookies: cookies);

            return artifactResult.Content;
        }

        public string PublishProcessArtifact(IUser user, IProcess process, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
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

            List<OpenApiArtifact> artifactObjectList = new List<OpenApiArtifact>();
            foreach (IOpenApiArtifact artifact in Artifacts)
            {
                var artifactElement = new OpenApiArtifact(artifact.Address, artifact.Id, artifact.ProjectId);
                artifactObjectList.Add(artifactElement);
            }

            string path = I18NHelper.FormatInvariant("{0}/{1}/{2}", SVC_PATH, URL_PROCESSES, process.Id);

            RestApiFacade restApi = new RestApiFacade(_address, user.Username, user.Password, tokenValue);

            var artifactResult = restApi.SendRequestAndGetResponse(path, RestRequestMethod.POST, expectedStatusCodes: expectedStatusCodes);

            return artifactResult.Content;
        }

        public List<IPublishArtifactResult> PublishProcessArtifacts(IUser user, bool shouldKeepLock = false, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.OpenApiToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            Dictionary<string, string> additionalHeaders = new Dictionary<string, string>();

            if (shouldKeepLock)
            {
                additionalHeaders.Add("KeepLock", "true");
            }

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            }

            List<OpenApiArtifact> artifactObjectList = new List<OpenApiArtifact>();
            foreach (IOpenApiArtifact artifact in Artifacts)
            {
                var artifactElement = new OpenApiArtifact(artifact.Address, artifact.Id, artifact.ProjectId);
                artifactObjectList.Add(artifactElement);
            }

            string path = URL_PUBLISH;
            RestApiFacade restApi = new RestApiFacade(_address, user.Username, user.Password, tokenValue);
            var artifactResults = restApi.SendRequestAndDeserializeObject<List<PublishArtifactResult>, List<OpenApiArtifact>>(path, RestRequestMethod.POST, artifactObjectList, additionalHeaders: additionalHeaders, expectedStatusCodes: expectedStatusCodes);

            return artifactResults.ConvertAll(o => (IPublishArtifactResult)o);
        }

        public List<IDeleteArtifactResult> DeleteProcessArtifact(IOpenApiArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null, bool deleteChildren = false)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));
            Artifacts.Remove(Artifacts.First(i => i.Id.Equals(artifact.Id)));
            return artifact.DeleteArtifact(artifact, user, expectedStatusCodes, deleteChildren);
        }

        #endregion Implemented from IStoryteller
    }
}
