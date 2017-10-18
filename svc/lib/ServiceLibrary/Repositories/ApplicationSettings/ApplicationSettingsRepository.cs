using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Repositories.ApplicationSettings
{
    public sealed class ApplicationSettingsRepository : IApplicationSettingsRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        public ApplicationSettingsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        public ApplicationSettingsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<TenantInfo> GetTenantInfo(IDbTransaction transaction = null)
        {
            if (transaction != null)
            {
                return (await transaction.Connection.QueryAsync<TenantInfo>("[dbo].[GetTenantInfo]", transaction: transaction, commandType: CommandType.StoredProcedure))
                .FirstOrDefault();
            }
            return (await _connectionWrapper.QueryAsync<TenantInfo>("[dbo].[GetTenantInfo]", commandType: CommandType.StoredProcedure))
                .FirstOrDefault();
        }

        public async Task<IEnumerable<ApplicationSetting>> GetSettingsAsync(bool returnNonRestrictedOnly)
        {
            var prm = new DynamicParameters();
            prm.Add("@returnNonRestrictedOnly", returnNonRestrictedOnly);
            var settings = (await _connectionWrapper.QueryAsync<ApplicationSetting>("GetApplicationSettings", prm, commandType: CommandType.StoredProcedure)).ToList();

            // decrypt the license information
            bool workflowFeatureEnabled = false;
            var licenseInfo = settings.FirstOrDefault(s => s.Key == ServiceConstants.LicenseInfoApplicationSettingKey);
            if (licenseInfo != null)
            {
                var licenses = FeatureLicenseHelper.DecryptLicenses(licenseInfo.Value);
                var workflowLicense = licenses?.FirstOrDefault(f => f.GetFeatureType() == FeatureTypes.Workflow);
                if (workflowLicense?.GetStatus() == FeatureLicenseStatus.Active)
                {
                    workflowFeatureEnabled = true;
                }
                settings.Remove(licenseInfo);
            }

            var tenantInfo = await GetTenantInfo();

            settings.AddRange(new[]
            {
                new ApplicationSetting
                {
                    Key = ServiceConstants.ForgotPasswordUrlConfigKey,
                    Value = ServiceConstants.ForgotPasswordUrl
                },
                new ApplicationSetting
                {
                    Key = ServiceConstants.WorkflowFeatureKey,
                    Value = workflowFeatureEnabled.ToString()
                },
                new ApplicationSetting
                {
                    Key = ServiceConstants.TenantIdKey,
                    Value = tenantInfo?.TenantId
                },
                new ApplicationSetting
                {
                    Key = ServiceConstants.PackageNameKey,
                    Value = tenantInfo?.PackageName
                }
            });

            return settings;
        }

        public async Task<T> GetValue<T>(string key, T defaultValue)
        {
            var applicationSettings = await GetSettingsAsync(false);

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
            else if (typeof(T) == typeof(bool))
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
