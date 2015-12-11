using System.Collections.Generic;
using LicenseLibrary.Models;

namespace LicenseLibrary
{
    interface ILicenseProvider
    {
        IEnumerable<LicenseWrapper> GetBlueprintLicenses();
        IEnumerable<LicenseWrapper> GetDataAnalyticsLicenses();
    }
}
