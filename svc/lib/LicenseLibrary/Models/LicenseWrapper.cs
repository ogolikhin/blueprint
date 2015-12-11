using System;
using System.Collections.Generic;
using System.Linq;
using Sp.Agent.Licensing;

namespace LicenseLibrary.Models
{
    public class LicenseWrapper
    {
        public string FeatureName { get; }
        public int? MaximumLicenses { get; }
        public DateTime ExpirationDate { get; }
        public string ActivationKey { get; }

        public LicenseWrapper(string featureName, ILicense license, IFeature feature = null)
        {
            int? concurrentUsageLimit = feature == null ? license.Advanced.ConcurrentUsageLimit : feature.ConcurrentUsageLimit;
            DateTime? featureValidUntil = feature == null ? (DateTime?)null : feature.ValidUntil;

            FeatureName = featureName;
            MaximumLicenses = concurrentUsageLimit.GetValueOrDefault(int.MaxValue);
            ExpirationDate = license.ValidUntil > featureValidUntil ? featureValidUntil.Value : license.ValidUntil; // Minimum of the two
            ActivationKey = license.ActivationKey;
        }

        internal LicenseWrapper(IEnumerable<LicenseWrapper> wrappers)
        {
            var list = wrappers.ToList();
            FeatureName = list.Select(w => w.FeatureName).FirstOrDefault();
            MaximumLicenses = list.Any(w => w.MaximumLicenses == int.MaxValue) ? int.MaxValue : list.Sum(w => w.MaximumLicenses);
            ExpirationDate = list.Max(w => w.ExpirationDate);
            ActivationKey = list.Select(w => w.ActivationKey).FirstOrDefault();
        }

        internal static IEnumerable<LicenseWrapper> ValidFeatures(ILicense license, string feature = null)
        {
            if (feature == null)
            {
                return license.Advanced.AllFeatures()
                    .Where(kvp => kvp.Value.ValidUntil >= DateTime.Now)
                    .Select(kvp => new LicenseWrapper(kvp.Key, license, kvp.Value));
            }

            return new[] { new LicenseWrapper(feature, license) };
        }
    }
}
