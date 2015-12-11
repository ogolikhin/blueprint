using System.Collections.Generic;
using System.Linq;
using LicenseLibrary.Models;
using Sp.Agent.Configuration;
using Sp.Agent.Configuration.Internal;

namespace LicenseLibrary
{
    public class InishTechLicenseProvider : ILicenseProvider
    {
        internal const string ProductName = "Blueprint";
        internal const string ProductVersion = "5.0";
        internal const string DataAnalyticsProductName = "Blueprint Data Analytics";
        internal const string DataAnalyticsProductVersion = "5.0";

        #region Singleton

        private static volatile InishTechLicenseProvider _instance;
        private static readonly object syncRoot = new object();

        public static InishTechLicenseProvider GetInstance()
        {
            string webApplicationId = System.Web.Hosting.HostingEnvironment.ApplicationID;
            var agentContext = AgentContext.For(LicenseConstants.PermutationShortId);
            agentContext.Configure(x => x.WithHttpApplicationIdStore(LicenseConstants.LicenseFolderFullPath, webApplicationId).CompleteWithDefaults());
            return GetInstance(agentContext);
        }

        internal static InishTechLicenseProvider GetInstance(IAgentContext agentContext)
        {
            if (_instance == null)
            {
                lock (syncRoot)
                {
                    if (_instance == null)
                    {
                        _instance = new InishTechLicenseProvider(agentContext);
                    }
                }
            }

            return _instance;
        }

        private readonly IAgentContext _agentContext;

        private InishTechLicenseProvider(IAgentContext agentContext)
        {
            _agentContext = agentContext;
        }

        #endregion Singleton

        #region GetLicenses

        public IEnumerable<LicenseWrapper> GetBlueprintLicenses()
        {
            return GetLicenses(ProductName, ProductVersion);
        }

        public IEnumerable<LicenseWrapper> GetDataAnalyticsLicenses()
        {
            return GetLicenses(DataAnalyticsProductName, DataAnalyticsProductVersion, "Data Analytics");
        }

        #endregion GetLicenses

        private IEnumerable<LicenseWrapper> GetLicenses(string productName, string productVersion, string feature = null)
        {
            lock (syncRoot)
            {
                var productContext = _agentContext.ProductContextFor(productName, productVersion);

                return productContext.Licenses.Valid()
                    .SelectMany(l => LicenseWrapper.ValidFeatures(l, feature))
                    .GroupBy(w => w.FeatureName, (n, w) => new LicenseWrapper(w));
            }
        }
    }
}
