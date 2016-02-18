using Common;
using System;
using System.Collections.Generic;
using System.Net;
using Model.Factories;
using Utilities;
using Utilities.Facades;

namespace Model.Impl
{
    public class Storyteller : IStoryteller
    {
        private const string SVC_PATH = "svc/components/storyteller";
        private const string SessionTokenCookieName = "BLUEPRINT_SESSION_TOKEN";

        private IOpenApiArtifact _artifact;
        private readonly string _address;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="address">The URI address of the Storyteller REST API</param>
        public Storyteller(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            _address = address;
            _artifact = new OpenApiArtifact(_address);
        }

        #region Implemented from IStoryteller

        public IOpenApiArtifact CreateProcessArtifact(IProject project, BaseArtifactType artifactType, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            //Create an artifact with ArtifactType and populate all required values without properties
            _artifact = ArtifactFactory.CreateOpenApiArtifact(_address, project, artifactType);

            //Create Description property
            List<IOpenApiProperty> properties = new List<IOpenApiProperty>();
            IOpenApiProperty property = new OpenApiProperty();
            properties.Add(property.GetProperty(project, "Description", "DescriptionValue"));

            //Set to add in root of the project
            _artifact.ParentId = _artifact.ProjectId;

            //Set the artifact properties 
            _artifact.SetProperties(properties);

            //add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
            return _artifact.AddArtifact(_artifact, user);
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
                path = I18NHelper.FormatInvariant(path + "/{0}", versionIndex);
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

        public int GetProcessTypeId(IUser user, IProject project, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));
            string processTypeName = nameof(BaseArtifactType.Process);
            return project.GetArtifactTypeId(address: _address, user: user, baseArtifactTypeName: processTypeName,
                projectId: project.Id, expectedStatusCodes: expectedStatusCodes);
        }

        public IArtifactResult<IOpenApiArtifact> DeleteProcessArtifact(IOpenApiArtifact process, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return _artifact.DeleteArtifact(process, user);
        }

        #endregion Inherited from IStoryteller
    }
}
