using System.Collections.Generic;
using System.Net;
using Common;
using Utilities;
using Utilities.Facades;

namespace Model.Impl
{
    public class ArtifactStore : IArtifactStore
    {
        private const string SVC_PATH = "svc/artifactstore";

        private string _address = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The base URI of the ArtifactStore service.</param>
        public ArtifactStore(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            _address = address;
        }

        #region Members inherited from IArtifactStore

        /// <seealso cref="IArtifactStore.GetStatus"/>
        public string GetStatus(string preAuthorizedKey = CommonConstants.PreAuthorizedKeyForStatus, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var restApi = new RestApiFacade(_address, string.Empty);
            string path = I18NHelper.FormatInvariant("{0}/status", SVC_PATH);

            var queryParameters = new Dictionary<string, string>();

            if (preAuthorizedKey != null)
            {
                queryParameters.Add("preAuthorizedKey", preAuthorizedKey);
            }

            Logger.WriteInfo("Getting ArtifactStore status...");
            var response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.GET, queryParameters: queryParameters, expectedStatusCodes: expectedStatusCodes);
            return response.Content;
        }

        /// <seealso cref="IArtifactStore.GetStatusUpcheck"/>
        public HttpStatusCode GetStatusUpcheck(List<HttpStatusCode> expectedStatusCodes = null)
        {
            var restApi = new RestApiFacade(_address, string.Empty);
            string path = I18NHelper.FormatInvariant("{0}/status/upcheck", SVC_PATH);

            Logger.WriteInfo("Getting ArtifactStore status upcheck...");
            var response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.GET, expectedStatusCodes: expectedStatusCodes);
            return response.StatusCode;
        }

        #endregion Members inherited from IArtifactStore
    }
}
