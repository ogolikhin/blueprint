using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLibrary.Models;

namespace AccessControl.Repositories
{
    public interface ILicensesRepository
    {
        Task<IEnumerable<LicenseInfo>> GetActiveLicenses(DateTime now, int licenseLockTimeMinutes);
        Task<int> GetLockedLicenses(int excludeUserId, int licenseLevel, int licenseLockTimeMinutes);
        Task<IEnumerable<LicenseTransaction>> GetLicenseTransactions(DateTime startTime, int consumerType);
        Task<LicenseUsage> GetLicenseUsage(int? month, int? year);
    }
}
