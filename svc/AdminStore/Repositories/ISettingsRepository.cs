using System.Collections.Generic;
using System.Threading.Tasks;
using AdminStore.Models;

namespace AdminStore.Repositories
{
    public interface IApplicationSettingsRepository
    {
        Task<IEnumerable<ApplicationSetting>> GetSettings();

        Task<T> GetValue<T>(string key, T defaultValue);
    }
}