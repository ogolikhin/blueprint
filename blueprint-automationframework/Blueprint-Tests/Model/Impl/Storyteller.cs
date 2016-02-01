using Common;
using System;
using System.Collections.Generic;
using System.Net;
using Utilities;
using Utilities.Facades;

namespace Model.Impl
{
    public class Storyteller : IStoryteller
    {
        private const string SVC_PATH = "Storyteller/api";
        private const string SessionTokenCookieName = "BLUEPRINT_SESSION_TOKEN";

        private IArtifactStore _artifactStore;
        private readonly string _address;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="address">The URI address of the Storyteller REST API</param>
        public Storyteller(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            _address = address;
            _artifactStore = new ArtifactStore(_address);
        }

        #region Inherited from IStoryteller

        public IArtifact CreateProcessArtifact(IUser user, string processName, int projectId, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var artifactToCreate = new Artifact()
            {
                Id = 0,
                Name = processName,
                ParentId = projectId,
                ProjectId = projectId,
                ArtifactTypeId = 77 // Need to find a way to determine the artifact type id from server-side
            };
            return _artifactStore.AddArtifact(artifactToCreate, user);
        }

        public IArtifactResult DeleteProcessArtifact(IArtifact artifact, IUser user)
        {
            return _artifactStore.DeleteArtifact(artifact, user);
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

        #endregion Inherited from IStoryteller
    }
}
