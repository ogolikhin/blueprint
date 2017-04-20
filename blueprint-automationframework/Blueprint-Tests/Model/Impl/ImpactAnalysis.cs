using Common;
using Model.NovaModel.Components.ImpactAnalysisService;
using System.Collections.Generic;
using System.Net;
using Utilities;
using Utilities.Facades;

namespace Model.Impl
{
    public class ImpactAnalysis : NovaServiceBase
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The base URI of the ImpactAnalysis service.</param>
        public ImpactAnalysis(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            Address = address;
        }

        /// <summary>
        /// Creates GET call returns Gets impact analysys information
        /// {base_blueprint_url}/ImpactAnalysis/api/{sourceId}/{level}/{format}
        /// </summary>
        /// <param name="user">User to authenticate with.</param>
        /// <param name="address">The base address of the ArtifactStore.</param>
        /// <param name="sourceId">Artifact Id from which impact analysis starts.</param>
        /// <param name="level">Number of levels in artifact chain to return.</param>
        /// <param name="format">(optional)Xml or json format. By default format is taken from request header.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request. By default only 200 OK is expected.<</param>
        /// <returns>ImpactAnalysisResult object</returns>
        public static ImpactAnalysisResult GetImpactAnalysis(IUser user, string address, int sourceId, int level, string format = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.ImpactAnalysis.IMPACT_ANALYSIS, sourceId, level);
            if (!string.IsNullOrEmpty(format))
            {
                path += "/" + format;
            }

            var restApi = new RestApiFacade(address, user?.Token?.AccessControlToken);

            return restApi.SendRequestAndDeserializeObject<ImpactAnalysisResult>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);
        }
    }
}
