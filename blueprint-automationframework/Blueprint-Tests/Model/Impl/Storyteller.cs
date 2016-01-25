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

        private static string _address;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="address">The URI address of the Storyteller REST API</param>
        public Storyteller(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            _address = address;
        }

        #region Inherited from IStoryteller

        public List<IProcess> Processes { get; } = new List<IProcess>();

        public IProcess GetProcess(IUser user, int Id, int? versionIndex = default(int?), List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(Id, nameof(Id));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            var path = I18NHelper.FormatInvariant("/processes/{id}/{versionId}", SVC_PATH, Id);
            var restApi = new RestApiFacade(_address, user.Username, user.Password, tokenValue);

            var response = restApi.SendRequestAndDeserializeObject<Process>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies);

            return response;
        }

        public void UpdateProcess(IUser user, Process Process, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            throw new NotImplementedException();
        }

        #endregion Inherited from IStoryteller
    }
}
