﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Models;
using BlueprintSys.RC.Services.Repositories;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.CrossCutting.Logging;
using ServiceLibrary.Helpers;

namespace BlueprintSys.RC.Services.Helpers
{
    public interface ITenantInfoRetriever
    {
        Task<Dictionary<string, TenantInformation>> GetTenants();
    }

    public class TenantInfoRetriever : ITenantInfoRetriever
    {
        public TenantInfoRetriever(IActionHandlerServiceRepository actionHandlerServiceRepository, IConfigHelper configHelper)
        {
            var expirationTime = TimeSpan.FromMinutes(configHelper.CacheExpirationMinutes);
            _tenantInfoCache = new CacheHelper<Task<Dictionary<string, TenantInformation>>>(expirationTime, GetTenantInfoForCache);
            _actionHandlerServiceRepository = actionHandlerServiceRepository;
        }

        public TenantInfoRetriever() : this(new ActionHandlerServiceRepository(new ConfigHelper().TenantsDatabase), new ConfigHelper())
        {
        }

        private readonly IActionHandlerServiceRepository _actionHandlerServiceRepository;
        private readonly CacheHelper<Task<Dictionary<string, TenantInformation>>> _tenantInfoCache;

        public Task<Dictionary<string, TenantInformation>> GetTenants()
        {
            return _tenantInfoCache.Get();
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