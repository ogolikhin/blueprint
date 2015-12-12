using LicenseLibrary.Models;

namespace LicenseLibrary.Repositories
{
    public interface ILicenseManager
    {
        LicenseInfo GetLicenseInfo(ProductFeature feature);
    }
}
