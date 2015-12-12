using LicenseLibrary.Models;

namespace LicenseLibrary.Repositories
{
    public interface ILicenseManager
    {
        LicenseKey GetLicenseKey(ProductFeature feature);
    }
}
