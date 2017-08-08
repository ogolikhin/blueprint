using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActionHandlerService.Models;
using ActionHandlerService.Models.Enums;
using ActionHandlerService.Repositories;
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
            TenantInfoCache = new CacheHelper<Dictionary<string, TenantInformation>>(expirationTime, GetTenantInfoForCache);
            _actionHandlerServiceRepository = actionHandlerServiceRepository;
        }

        public TenantInfoRetriever() : this(new ActionHandlerServiceRepository(new ConfigHelper().SingleTenancyConnectionString), new ConfigHelper())
        {
        }

        private readonly IActionHandlerServiceRepository _actionHandlerServiceRepository;
        private IConfigHelper ConfigHelper { get; }
        private CacheHelper<Dictionary<string, TenantInformation>> TenantInfoCache { get; }
        private string _defaultTentantId;
        private const string DefaultTenantSettings = "settings";

        public async Task<Dictionary<string, TenantInformation>> GetTenants()
        {
            if (string.IsNullOrEmpty(_defaultTentantId))
            {
                //TODO: remove once we get the tenant db ready
                _defaultTentantId = await _actionHandlerServiceRepository.GetTenantId();
                Log.Info($"Retrieved default tenant id: {_defaultTentantId}");
            }
            return TenantInfoCache.Get();
        }

        private Dictionary<string, TenantInformation> GetTenantInfoForCache()
        {
            var tenants = new Dictionary<string, TenantInformation>();
            var tenancy = ConfigHelper.Tenancy;
            switch (tenancy)
            {
                case Tenancy.Single:
                    Log.Info("Retrieving single tenant.");
                    var tenant = new TenantInformation {Id = _defaultTentantId, ConnectionString = ConfigHelper.SingleTenancyConnectionString, Settings = DefaultTenantSettings};
                    tenants.Add(tenant.Id, tenant);
                    break;
                case Tenancy.Multiple:
                    Log.Info("Retrieving multiple tenants.");
                    //TODO: get multiple tenants
                    break;
            }
            Log.Info($"Retrieved {tenants.Count} tenants: {string.Join(", ", tenants.Select(p => p.Key))}");
            return tenants;
        }
    }
}
