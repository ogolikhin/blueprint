using System;
using System.Collections.Generic;
using ActionHandlerService.Models;
using ActionHandlerService.Models.Enums;
using ServiceLibrary.Helpers;

namespace ActionHandlerService.Helpers
{
    public static class TenantInfoRetriever
    {
        private static readonly CacheHelper<Dictionary<int, TenantInformation>> TenantInfoCache = new CacheHelper<Dictionary<int, TenantInformation>>(TimeSpan.FromMinutes(ConfigHelper.CacheExpirationMinutes), GetTenantInfoForCache);

        public static Dictionary<int, TenantInformation> GetTenants()
        {
            return TenantInfoCache.Get();
        }

        private static Dictionary<int, TenantInformation> GetTenantInfoForCache()
        {
            var tenants = new Dictionary<int, TenantInformation>();
            var tenancy = ConfigHelper.Tenancy;
            switch (tenancy)
            {
                case Tenancy.Single:
                    var tenant = new TenantInformation {Id = 0, ConnectionString = ConfigHelper.SingleTenancyConnectionString, Settings = string.Empty};
                    tenants.Add(tenant.Id, tenant);
                    break;
                case Tenancy.Multiple:
                    break;
            }
            return tenants;
        }
    }
}
