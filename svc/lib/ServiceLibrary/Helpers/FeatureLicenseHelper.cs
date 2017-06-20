using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Licenses;
using ServiceLibrary.Repositories;

namespace ServiceLibrary.Helpers
{
    public class FeatureLicenseHelper
    {
        private const int ExpirationTime = 20; //minutes

        private static FeatureTypes _validFeatures = FeatureTypes.None;
        private static FeatureTypes _expiredFeatures = FeatureTypes.None;

        private static CacheHelper<FeatureTypes> _validBlueprintLicenseFeaturesCache = new CacheHelper<FeatureTypes>(
            TimeSpan.FromMinutes(ExpirationTime),
            () => InternalGetValidBlueprintLicenseFeatures()
        );
        public static FeatureTypes GetValidBlueprintLicenseFeatures()
        {
            return _validBlueprintLicenseFeaturesCache.Get();
        }

        private static FeatureTypes InternalGetValidBlueprintLicenseFeatures()
        {
            CalculateValidIntegrationFeatures();
            return _validFeatures;
        }

        public static FeatureTypes GetExpiredLicenseFeatures()
        {
            return _expiredFeatures;
        }

        private static CacheHelper<IDictionary<FeatureTypes, FeatureInformation>> _allLicenseFeaturesCache =
            new CacheHelper<IDictionary<FeatureTypes, FeatureInformation>>(
                TimeSpan.FromMinutes(ExpirationTime),
                () => new ReadOnlyDictionary<FeatureTypes, FeatureInformation>(
                    GetLicensesFromDatabase()
                )
            );
        public static IDictionary<FeatureTypes, FeatureInformation> GetAllLicenseFeatures()
        {
            return _allLicenseFeaturesCache.Get();
        }

        private static void CalculateValidIntegrationFeatures()
        {
            _validFeatures = FeatureTypes.None;
            var validLicenses = GetLicensesFromDatabase();
            foreach (var lic in validLicenses.Keys)
            {
                if (validLicenses[lic] != null)
                {
                    if (validLicenses[lic].GetStatus() == FeatureLicenseStatus.Active)
                    {
                        if (_validFeatures == FeatureTypes.None)
                        {
                            _validFeatures = lic;
                        }
                        else
                        {
                            _validFeatures |= lic;
                        }
                    }
                    else if (validLicenses[lic].GetStatus() == FeatureLicenseStatus.Expired)
                    {
                        _expiredFeatures |= lic;
                    }
                }
            }
        }

        private static Dictionary<FeatureTypes, FeatureInformation> GetLicensesFromDatabase()
        {
            var licenseRepository = new LicenseRepository();
            var licenseSettings = licenseRepository.GetLicenseInfo();
            if (licenseSettings.Count != 1 || string.IsNullOrWhiteSpace(licenseSettings.Single().Value))
            {
                return new Dictionary<FeatureTypes, FeatureInformation>();
            }
            var licenses = DecryptLicenses(licenseSettings.Single().Value);
            return licenses;
        }

        private static Dictionary<FeatureTypes, FeatureInformation> DecryptLicenses(string xml)
        {
            var serializableLicenses = SerializationHelper.FromXml<FeatureInformation[]>(xml);
            return serializableLicenses.ToDictionary(f => f.GetFeatureType(), f => f);
        }
    }
}
