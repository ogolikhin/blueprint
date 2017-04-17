using Common;
using Model.Common.Enums;
using System.Collections.Generic;
using System.Globalization;
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
        /// <param name="sourceId"></param>
        /// <param name="level"></param>
        /// <param name="format"></param>
        /// <param name="expectedStatusCodes"></param>
        /// <returns></returns>
        public ImpactAnalysisResult GetImpactAnalysis(IUser user, int sourceId, int level, string format = "", List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(format, nameof(format));

            string path = I18NHelper.FormatInvariant(RestPaths.ImpactAnalysis.IMPACT_ANALYSIS, sourceId, level);
            if (!string.IsNullOrEmpty(format))
            {
                path += "/" + format;
            }

            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            return restApi.SendRequestAndDeserializeObject<ImpactAnalysisResult>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);
        }
    }
}
