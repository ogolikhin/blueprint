using System.Collections.Generic;

namespace LicenseLibrary
{
    interface ILicenseProvider
    {
        IEnumerable<LicenseWrapper> GetBlueprintLicenses();
        IEnumerable<LicenseWrapper> GetDataAnalyticsLicenses();
    }
}
