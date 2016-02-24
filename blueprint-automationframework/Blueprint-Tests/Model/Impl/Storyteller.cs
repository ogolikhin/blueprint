using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Model.Factories;
using Utilities;
using Utilities.Facades;

namespace Model.Impl
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

        private IOpenApiArtifact _artifact;
        private readonly string _address;

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

        public IOpenApiArtifact CreateProcessArtifact(IProject project, BaseArtifactType artifactType, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            //Create an artifact with ArtifactType and populate all required values without properties
            _artifact = ArtifactFactory.CreateOpenApiArtifact(_address, user, project, artifactType);
            _artifact.BaseArtifactType = artifactType;
            //Set to add in root of the project
            _artifact.ParentId = _artifact.ProjectId;

            var artifact = _artifact.AddArtifact(_artifact, user);
            _artifact.Id = artifact.Id;

            Artifacts.Add(_artifact);

            //add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
            return artifact;
        }

        public List<IOpenApiArtifact> CreateProcessArtifacts(IStoryteller storyteller, IProject project, IUser user, int numberOfArtifacts)
        {
            ThrowIf.ArgumentNull(storyteller, nameof(storyteller));
            var artifacts = new List<IOpenApiArtifact>();

            for (int i = 0; i < numberOfArtifacts; i++)
            {
                artifacts.Add(storyteller.CreateProcessArtifact(project, BaseArtifactType.Process, user));
            }

            return artifacts;
        }

        public IProcess GetProcess(IUser user, int id, int? versionIndex = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            var path = I18NHelper.FormatInvariant("{0}/processes/{1}", SVC_PATH, id);
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

        public IProcess GetProcessWithBreadcrumb(IUser user, List<int> ids, int? versionIndex = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(ids, nameof(ids));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            var path = I18NHelper.FormatInvariant("{0}/processes", SVC_PATH);

            foreach (var id in ids)
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

        public void UpdateProcess(IUser user, IProcess process, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            throw new NotImplementedException();
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

        public int GetProcessTypeId(IUser user, IProject project)
        {
            ThrowIf.ArgumentNull(project, nameof(project));
            BaseArtifactType processTypeName = BaseArtifactType.Process;
            return project.GetArtifactTypeId(address: _address, user: user, baseArtifactTypeName: processTypeName,
                projectId: project.Id);
        }

        public List<PublishArtifactResult> PublishProcessArtifacts(IUser user, bool isKeepLock = false, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            Dictionary<string, string> additionalHeaders = new Dictionary<string, string>();

            additionalHeaders.Add("Accept", "application/json");

            if (isKeepLock)
            {
                additionalHeaders.Add("KeepLock", "true");
            }
            string path = URL_PUBLISH;

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            }

            OpenApiArtifact artifactElement;
            List<OpenApiArtifact> artifactObjectList = new List<OpenApiArtifact>();
            foreach (IOpenApiArtifact artifact in Artifacts)
            {
                artifactElement = new OpenApiArtifact(artifact.Address);
                artifactElement.Id = artifact.Id;
                artifactElement.ProjectId = artifact.ProjectId;
                artifactObjectList.Add(artifactElement);
            }

            RestApiFacade restApi = new RestApiFacade(_address, user.Username, user.Password, tokenValue);

            List<PublishArtifactResult> artifactResults = restApi.SendRequestAndDeserializeObject<List<PublishArtifactResult>, List<OpenApiArtifact>>(path, RestRequestMethod.POST, artifactObjectList, additionalHeaders: additionalHeaders, expectedStatusCodes: expectedStatusCodes);

            return artifactResults;
        }

        public IArtifactResult<IOpenApiArtifact> DeleteProcessArtifact(IOpenApiArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            Artifacts.Remove(Artifacts.First(i => i.Id == artifact.Id));

            return artifact.DeleteArtifact(artifact, user, expectedStatusCodes);
        }

        public List<OpenApiUserStoryArtifact> GenerateUserStories(IUser user, IOpenApiArtifact processArtifact, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            var path = SVC_PATH + "/" + URL_PROJECTS;

            ThrowIf.ArgumentNull(processArtifact, nameof(processArtifact));
            path = I18NHelper.FormatInvariant("{0}/{1}", path, processArtifact.ProjectId);
            path = I18NHelper.FormatInvariant("{0}/{1}", path, URL_PROCESSES);
            path = I18NHelper.FormatInvariant("{0}/{1}", path, processArtifact.Id);
            path = I18NHelper.FormatInvariant("{0}/{1}", path, URL_USERSTORIES);

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
            additionalHeaders.Add("Accept", "application/json");

            RestApiFacade restApi = new RestApiFacade(_address, user.Username, user.Password, tokenValue);

            List<OpenApiUserStoryArtifact> userstoryResults = restApi.SendRequestAndDeserializeObject<List<OpenApiUserStoryArtifact>>(path, RestRequestMethod.POST, additionalHeaders: additionalHeaders, expectedStatusCodes: expectedStatusCodes);

            return userstoryResults;
        }

        #endregion Implemented from IStoryteller
    }
}
