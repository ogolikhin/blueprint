using System;
using LicenseLibrary.Models;

namespace LicenseLibrary.Repositories
{
    public class DebugLicenseManager : ILicenseManager
    {
        public LicenseInfo GetLicenseInfo(ProductFeature feature)
        {
            return new LicenseInfo(feature, null, DateTime.MaxValue, null);
        }
    }
}
