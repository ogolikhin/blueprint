using System;
using System.Collections.Generic;
using ServiceLibrary.Helpers;

namespace ActionHandlerService
{
    public static class TenantInfoRetriever
    {
        private static readonly CacheHelper<Dictionary<int, TenantInfo>> TenantInfoCache = new CacheHelper<Dictionary<int, TenantInfo>>(TimeSpan.FromMinutes(ConfigHelper.CacheExpirationMinutes), GetTenantInfoForCache);

        public static Dictionary<int, TenantInfo> GetTenants()
        {
            return TenantInfoCache.Get();
        }

        private static Dictionary<int, TenantInfo> GetTenantInfoForCache()
        {
            var tenants = new Dictionary<int, TenantInfo>();
            var tenancy = ConfigHelper.Tenancy;
            if (tenancy == Tenancy.Single)
            {
                var tenant = new TenantInfo {TenantId = 0, TenantConnectionString = ConfigHelper.SingleTenancyConnectionString, TenantSettings = string.Empty};
                tenants.Add(tenant.TenantId, tenant);
            }
            else if (tenancy == Tenancy.Multiple)
            {
            }
            return tenants;
        }
    }

    public class TenantInfo
    {
        public int TenantId { get; set; }
        public string TenantConnectionString { get; set; }
        public string TenantSettings { get; set; }
    }
}
