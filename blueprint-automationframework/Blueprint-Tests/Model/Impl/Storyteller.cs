﻿using Common;
using System;
using System.Collections.Generic;
using System.Net;
using Utilities;
using Utilities.Facades;

namespace Model.Impl
{
    public class Storyteller : IStoryteller
    {
        private const string SVC_PATH = "svc/components/storyteller";
        private const string SessionTokenCookieName = "BLUEPRINT_SESSION_TOKEN";

        private IArtifact _artifact;
        private readonly string _address;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="address">The URI address of the Storyteller REST API</param>
        public Storyteller(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            _address = address;
            _artifact = new Artifact(_address);
        }

        #region Inherited from IStoryteller

        public IArtifact AddProcessArtifact(IArtifact process, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return _artifact.AddArtifact(process, user);
        }

        public IArtifactResult DeleteProcessArtifact(IArtifact process, IUser user)
        {
            return _artifact.DeleteArtifact(process, user);
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
            string processTypeName = "Process";//replace with enum
            return project.GetArtifactTypeId(address: _address, user: user, baseArtifactTypeName: processTypeName,
                projectId: project.Id, expectedStatusCodes: expectedStatusCodes);
        }

        #endregion Inherited from IStoryteller
    }
}
