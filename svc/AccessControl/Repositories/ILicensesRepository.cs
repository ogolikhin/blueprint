using System.Collections;
using System.Collections.Generic;
using ServiceLibrary.Models;
using System.Threading.Tasks;

namespace AccessControl.Repositories
{
    public interface ILicensesRepository
    {
		Task<IEnumerable<LicenseInfo>> GetLicensesStatus(int licenseLockTimeMinutes);

		Task<int> GetActiveLicenses(int excludeUserId, int licenseLevel, int licenseLockTimeMinutes);
	}
}
