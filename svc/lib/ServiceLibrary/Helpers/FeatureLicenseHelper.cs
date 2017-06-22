using System;
using System.Collections.Generic;
using System.Linq;
using ServiceLibrary.Helpers.Security;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Licenses;
using ServiceLibrary.Repositories;

namespace ServiceLibrary.Helpers
{
    public interface IFeatureLicenseHelper
    {
        FeatureTypes GetValidBlueprintLicenseFeatures();
    }

    public class FeatureLicenseHelper : IFeatureLicenseHelper
    {
        private const int CacheExpirationTime = 20; //minutes
        private static readonly CacheHelper<FeatureTypes> ValidBlueprintLicenseFeaturesCache = new CacheHelper<FeatureTypes>(TimeSpan.FromMinutes(CacheExpirationTime), InternalGetValidBlueprintLicenseFeatures);

        private FeatureLicenseHelper()
        {
        }

        public static IFeatureLicenseHelper Instance { get; } = new FeatureLicenseHelper();

        public FeatureTypes GetValidBlueprintLicenseFeatures()
        {
            return ValidBlueprintLicenseFeaturesCache.Get();
        }

        private static FeatureTypes InternalGetValidBlueprintLicenseFeatures()
        {
            var validFeatures = FeatureTypes.None;
            var validLicenses = GetValidLicenses();
            foreach (var lic in validLicenses.Keys)
            {
                if (validLicenses[lic] == null || validLicenses[lic].GetStatus() != FeatureLicenseStatus.Active)
                {
                    continue;
                }
                if (validFeatures == FeatureTypes.None)
                {
                    validFeatures = lic;
                }
                else
                {
                    validFeatures |= lic;
                }
            }
            return validFeatures;
        }


        private static Dictionary<FeatureTypes, FeatureInformation> GetValidLicenses()
        {
#if DEBUG
            return GetVirtualLicenses();
#else
            return GetLicensesFromDatabase();
#endif
        }

        private static Dictionary<FeatureTypes, FeatureInformation> GetLicensesFromDatabase()
        {
            var licenseRepository = new LicenseRepository();
            var licenseSettings = licenseRepository.GetLicenseInfo();
            if (licenseSettings.Count != 1 || string.IsNullOrWhiteSpace(licenseSettings.Single().Value))
            {
                return new Dictionary<FeatureTypes, FeatureInformation>();
            }
            var decryptedXml = SystemEncryptions.Decrypt(licenseSettings.Single().Value);
            var featureInfos = SerializationHelper.FromXml<FeatureInformation[]>(decryptedXml);
            return featureInfos.ToDictionary(f => f.GetFeatureType(), f => f);
        }

        private static Dictionary<FeatureTypes, FeatureInformation> GetVirtualLicenses()
        {
            return new Dictionary<FeatureTypes, FeatureInformation>
            {
                {FeatureTypes.Workflow, new FeatureInformation(FeatureTypes.Workflow.ToStringInvariant(), DateTime.MaxValue)}
            };
        }
    }
}
