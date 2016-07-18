using System.Collections.Generic;
using System.Net;
using Common;
using Utilities.Facades;

namespace Model.Impl
{
    public class NovaServiceBase
    {
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

        /// <summary>
        /// Checks if the service is ready for operation and returns a JSON string with service details.
        /// (Runs: GET svc/{component}/status)
        /// </summary>
        /// <param name="path">The full service path (ex. svc/filestore/status).</param>
        /// <param name="preAuthorizedKey">(optional) The pre-authorized key to use for authentication.  Defaults to a valid key.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>A JSON structure containing the status of this service and its dependent services.</returns>
        protected string GetStatus(string path, string preAuthorizedKey = CommonConstants.PreAuthorizedKeyForStatus, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var restApi = new RestApiFacade(Address);

            Dictionary<string, string> queryParameters = null;

            if (preAuthorizedKey != null)
            {
                queryParameters = new Dictionary<string, string> { { "preAuthorizedKey", preAuthorizedKey} };
            }

            Logger.WriteInfo("Getting {0} ...", path);
            var response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.GET, queryParameters: queryParameters, expectedStatusCodes: expectedStatusCodes);
            return response.Content;
        }

        /// <summary>
        /// Checks if the service is ready for operation.
        /// (Runs: GET /svc/{component}/status/upcheck)
        /// </summary>
        /// <param name="path">The full service path (ex. svc/filestore/status/upcheck).</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>The status code returned by ArtifactStore.</returns>
        protected HttpStatusCode GetStatusUpcheck(string path, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var restApi = new RestApiFacade(Address);

            Logger.WriteInfo("Getting {0} ...", path);
            var response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.GET, expectedStatusCodes: expectedStatusCodes);
            return response.StatusCode;
        }
    }
}
