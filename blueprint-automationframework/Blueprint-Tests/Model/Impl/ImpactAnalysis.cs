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

        public ImpactAnalysisResult GetImpactAnalysis(IUser user, int sourceId, int level, string format = "", List<HttpStatusCode> expectedStatusCodes = null)
        { 
            var acceptType = GetEffectiveSupportedAcceptType(format);

            string path = I18NHelper.FormatInvariant(RestPaths.ImpactAnalysis.IMPACT_ANALYSIS, sourceId, level, acceptType);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            return restApi.SendRequestAndDeserializeObject<ImpactAnalysisResult>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);
        }

        public static SupportedAcceptType GetEffectiveSupportedAcceptType(string format)
        {
            ThrowIf.ArgumentNull(format, nameof(format));

            if (format.ToLower(CultureInfo.CurrentCulture).Equals("xlsx"))
            {
                return SupportedAcceptType.Excel;
            }
            else
            {
                return SupportedAcceptType.Json;
            }
        }
    }
}
