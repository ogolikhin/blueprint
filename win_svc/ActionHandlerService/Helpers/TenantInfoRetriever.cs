using System;
using System.Collections.Generic;
using ActionHandlerService.Models;
using ActionHandlerService.Models.Enums;
using ServiceLibrary.Helpers;

namespace ActionHandlerService.Helpers
{
    public static class TenantInfoRetriever
    {
        private const string DefaultTentantId = "tenant0";
        private const string DefaultTenantSettings = "settings";

        private static readonly CacheHelper<Dictionary<string, TenantInformation>> TenantInfoCache = new CacheHelper<Dictionary<string, TenantInformation>>(TimeSpan.FromMinutes(ConfigHelper.CacheExpirationMinutes), GetTenantInfoForCache);

        public static Dictionary<string, TenantInformation> GetTenants()
        {
            return TenantInfoCache.Get();
        }

        private static Dictionary<string, TenantInformation> GetTenantInfoForCache()
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
                    break;
            }
            return tenants;
        }
    }
}
