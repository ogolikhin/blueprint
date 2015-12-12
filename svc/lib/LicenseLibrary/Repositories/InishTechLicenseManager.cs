using System.Linq;
using System.Web.Hosting;
using LicenseLibrary.Helpers;
using LicenseLibrary.Models;
using Sp.Agent.Configuration;
using Sp.Agent.Configuration.Internal;

namespace LicenseLibrary.Repositories
{
    public class InishTechLicenseManager : ILicenseManager
    {
        private static readonly object SyncRoot = new object();

        private readonly IAgentContext _agentContext;

        public InishTechLicenseManager()
        {
            lock (SyncRoot)
            {
                _agentContext = AgentContext.For(LicenseConstants.PermutationShortId);
                _agentContext.Configure(x => x.WithHttpApplicationIdStore(LicenseConstants.LicenseFolderFullPath,
                    HostingEnvironment.ApplicationID).CompleteWithDefaults());
            }
        }

        internal InishTechLicenseManager(IAgentContext agentContext)
        {
            lock (SyncRoot)
            {
                _agentContext = agentContext;
            }
        }

        #region ILicenseManager

        public LicenseKey GetLicenseKey(ProductFeature feature)
        {
            lock (SyncRoot)
            {
                var productContext = _agentContext.ProductContextFor(feature.GetProductName(), feature.GetProductVersion());

                return LicenseKey.Aggregate(productContext.Licenses.Valid().Select(l => LicenseKey.Get(l, feature)));
            }
        }

        #endregion ILicenseManager
    }
}
