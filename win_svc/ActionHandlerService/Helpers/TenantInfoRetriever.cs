using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActionHandlerService.Models;
using ActionHandlerService.Repositories;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.CrossCutting.Logging;
using ServiceLibrary.Helpers;

namespace ActionHandlerService.Helpers
{
    public interface ITenantInfoRetriever
    {
        Task<Dictionary<string, TenantInformation>> GetTenants();
    }

    public class TenantInfoRetriever : ITenantInfoRetriever
    {
        public TenantInfoRetriever(IActionHandlerServiceRepository actionHandlerServiceRepository, IConfigHelper configHelper)
        {
            ConfigHelper = configHelper;
            var expirationTime = TimeSpan.FromMinutes(ConfigHelper.CacheExpirationMinutes);
            TenantInfoCache = new CacheHelper<Task<Dictionary<string, TenantInformation>>>(expirationTime, GetTenantInfoForCache);
            _actionHandlerServiceRepository = actionHandlerServiceRepository;
        }

        public TenantInfoRetriever() : this(new ActionHandlerServiceRepository(new ConfigHelper().TenantsDatabase), new ConfigHelper())
        {
        }

        private readonly IActionHandlerServiceRepository _actionHandlerServiceRepository;
        private IConfigHelper ConfigHelper { get; }
        private CacheHelper<Task<Dictionary<string, TenantInformation>>> TenantInfoCache { get; }

        public Task<Dictionary<string, TenantInformation>> GetTenants()
        {
            return TenantInfoCache.Get();
        }

        private async Task<Dictionary<string, TenantInformation>> GetTenantInfoForCache()
        {
            Log.Debug("Retrieving tenants");
            var sqlTenants = await _actionHandlerServiceRepository.GetTenantsFromTenantsDb();
            var tenants = sqlTenants.ToDictionary(tenant => tenant.TenantId);
            Log.Debug($"Retrieved {tenants.Count} tenants: {string.Join(", ", tenants.Select(p => p.Key))}");
            return tenants;
        }
    }
}
