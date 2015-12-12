using System;
using LicenseLibrary.Models;

namespace LicenseLibrary.Repositories
{
    public class DebugLicenseManager : ILicenseManager
    {
        #region ILicenseManager

        public LicenseKey GetLicenseKey(ProductFeature feature)
        {
            return new LicenseKey(feature, null, DateTime.MaxValue, null);
        }

        #endregion ILicenseManager
    }
}
