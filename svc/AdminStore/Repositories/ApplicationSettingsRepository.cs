using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Models;
using ServiceLibrary.Repositories;
using ServiceLibrary.Helpers;

namespace AdminStore.Repositories
{
    public class ApplicationSettingsRepository : IApplicationSettingsRepository
    {
        internal readonly ISqlConnectionWrapper _connectionWrapper;

        public ApplicationSettingsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal ApplicationSettingsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public virtual async Task<IEnumerable<ApplicationSetting>> GetSettingsAsync()
        {
            var settings = (await _connectionWrapper.QueryAsync<ApplicationSetting>("GetApplicationSettings", null, commandType: CommandType.StoredProcedure)).ToList();

            settings.Add
            (
                new ApplicationSetting
                {
                    Key = ServiceConstants.ForgotPasswordUrlConfigKey,
                    Value = ServiceConstants.ForgotPasswordUrl
                }
            );

            return settings;
        }

        public async Task<T> GetValue<T>(string key, T defaultValue)
        {
            var applicationSettings = await GetSettingsAsync();

            var matchingSetting = applicationSettings.FirstOrDefault(s => s.Key == key);
            if (matchingSetting == null)
            {
                return defaultValue;
            }

            string rawValue = matchingSetting.Value;
            T resultValue;
            
            if (typeof(T) == typeof(int))
            {
                int value;
                if (!int.TryParse(rawValue, out value))
                {
                    return defaultValue;
                }
                resultValue = (T)Convert.ChangeType(value, typeof(T));
            }
            else if (typeof (T) == typeof (bool))
            {
                bool value;
                if (!bool.TryParse(rawValue, out value))
                {
                    return defaultValue;
                }
                resultValue = (T)Convert.ChangeType(value, typeof(T));
            }
            else
            {
                resultValue = (T)Convert.ChangeType(rawValue, typeof(T));
            }

            return resultValue;
        }
    }
}