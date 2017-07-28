using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ActionHandlerService.Models;
using ActionHandlerService.Models.Enums;
using ActionHandlerService.Repositories;
using ServiceLibrary.Helpers;

namespace ActionHandlerService.Helpers
{
    public interface ITenantInfoRetriever
    {
        Dictionary<string, TenantInformation> GetTenants();
    }

    public class TenantInfoRetriever : ITenantInfoRetriever
    {
        public TenantInfoRetriever(IConfigHelper configHelper = null)
        {
            ConfigHelper = configHelper ?? new ConfigHelper();
            var expirationTime = TimeSpan.FromMinutes(ConfigHelper.CacheExpirationMinutes);
            TenantInfoCache = new CacheHelper<Dictionary<string, TenantInformation>>(expirationTime, GetTenantInfoForCache);

            //TODO: remove once we get the tenant db ready
            actionHandlerServiceRepository = new ActionHandlerServiceRepository(ConfigHelper.SingleTenancyConnectionString);
            var task = Task.Run(async () => { DefaultTentantId = await actionHandlerServiceRepository.GetTenantId(); });
            task.Wait();
        }

        private IActionHandlerServiceRepository actionHandlerServiceRepository;
        private IConfigHelper ConfigHelper { get; }
        private CacheHelper<Dictionary<string, TenantInformation>> TenantInfoCache { get; }
        private string DefaultTentantId;
        private const string DefaultTenantSettings = "settings";

        public Dictionary<string, TenantInformation> GetTenants()
        {
            return TenantInfoCache.Get();
        }

        private Dictionary<string, TenantInformation> GetTenantInfoForCache()
        {
            var tenants = new Dictionary<string, TenantInformation>();
            var tenancy = ConfigHelper.Tenancy;
            switch (tenancy)
            {
                case Tenancy.Single:
                    var tenant = new TenantInformation {Id = DefaultTentantId, ConnectionString = ConfigHelper.SingleTenancyConnectionString, Settings = DefaultTenantSettings};
                    tenants.Add(tenant.Id, tenant);
                    break;
                case Tenancy.Multiple:
                    //TODO: get multiple tenants
                    break;
            }
            return tenants;
        }
    }
}
