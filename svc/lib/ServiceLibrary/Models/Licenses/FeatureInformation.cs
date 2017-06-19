using System;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Models.Licenses
{
    public class FeatureInformation
    {
        public FeatureInformation()
        {
        }

        public FeatureInformation(string featureName, DateTime expirationDate)
        {
            FeatureName = featureName;
            ExpirationDate = expirationDate;
        }

        public string FeatureName { get; set; }

        public DateTime ExpirationDate { get; set; }

        public FeatureTypes GetFeatureType()
        {
            return BlueprintFeatureLicenseTypeExt.InishTechLicenseToBlueprintLicense[FeatureName];
        }

        public BlueprintFeatureLicenseStatus GetStatus()
        {
            return ExpirationDate.ToUniversalTime() >= DateTime.Now.ToUniversalTime() ? BlueprintFeatureLicenseStatus.Active : BlueprintFeatureLicenseStatus.Expired;
        }
    }
}
