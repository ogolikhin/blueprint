using Common;
using Model.Common.Enums;
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
 /*
        public ImpactAnalysisResult GetImpactAnalysis(IUser user, int sourceId, int level, string format = "", List<HttpStatusCode> expectedStatusCodes = null)
        {
            var providedSupportTypes = _httpHeaderHelper.GetCompatibleSupportTypes(Request);

                        //Get corresponding accept type for the defined accept type
                        var acceptType = GetEffectiveSupportedAcceptType(format, providedSupportTypes);
            /*
                        var request = CreateImpactAnalysisRequest(sourceId, level, acceptType);

                                    switch (acceptType)
                        {
                            case SupportedAcceptTypes.Json:
                                return GetImpactAnalysisResult(request);
                            case SupportedAcceptTypes.Excel:
                                return GetImpactAnalysisReportResult(request);
                        }
                        throw new NotSupportedException();
                         *

            string path = I18NHelper.FormatInvariant(RestPaths.ImpactAnalysis.IMPACT_ANALYSIS, sourceId, level, format);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            return restApi.SendRequestAndDeserializeObject<ImpactAnalysisResult>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);
        }
*/
        // Taken from blueprint-current/Source/BluePrintSys.RC.Web.Internal/Components/ImpactAnalysis/Web/IHttpHeaderHelper.cs
        public SupportedAcceptTypes GetEffectiveSupportedAcceptType(string format, HashSet<SupportedAcceptTypes> providedAcceptTypes)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                if (providedAcceptTypes.Contains(SupportedAcceptTypes.Json))
                {
                    return SupportedAcceptTypes.Json;
                }
                if (providedAcceptTypes.Contains(SupportedAcceptTypes.Excel))
                {
                    return SupportedAcceptTypes.Excel;
                }
            }
            switch (format.ToLower())
            {
                case "text":
                case "plain":
                case "json":
                    if (providedAcceptTypes.Contains(SupportedAcceptTypes.Json))
                    {
                        return SupportedAcceptTypes.Json;
                    }
                    break;
                case "xlsx":
                    if (providedAcceptTypes.Contains(SupportedAcceptTypes.Excel))
                    {
                        return SupportedAcceptTypes.Excel;
                    }
                    break;
            }
            return SupportedAcceptTypes.Json;
        }
    }
}
