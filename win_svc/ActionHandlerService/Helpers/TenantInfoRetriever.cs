using System;
using System.Collections.Generic;
using ActionHandlerService.Models;
using ActionHandlerService.Models.Enums;
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
        }

        private IConfigHelper ConfigHelper { get; }
        private CacheHelper<Dictionary<string, TenantInformation>> TenantInfoCache { get; }
        private const string DefaultTentantId = "tenant0";
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
