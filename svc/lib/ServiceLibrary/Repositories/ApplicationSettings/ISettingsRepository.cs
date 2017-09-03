using ServiceLibrary.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
    public interface IApplicationSettingsRepository
    {
        Task<IEnumerable<ApplicationSetting>> GetSettingsAsync(bool returnNonRestrictedOnly);

        Task<T> GetValue<T>(string key, T defaultValue);

        Task<TenantInfo> GetTenantInfo(IDbTransaction transaction = null);
    }
}