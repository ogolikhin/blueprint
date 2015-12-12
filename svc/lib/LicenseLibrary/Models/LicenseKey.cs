using System;
using System.Collections.Generic;
using System.Linq;
using Sp.Agent.Licensing;

namespace LicenseLibrary.Models
{
    public class LicenseKey
    {
        public ProductFeature ProductFeature { get; }
        public int? MaximumLicenses { get; }
        public DateTime ExpirationDate { get; }
        public string ActivationKey { get; }

        public LicenseKey(ProductFeature productFeature, int? maximumLicenses, DateTime expirationDate, string activationKey)
        {
            ProductFeature = productFeature;
            MaximumLicenses = maximumLicenses;
            ExpirationDate = expirationDate;
            ActivationKey = activationKey;
        }

        internal static LicenseKey Get(ILicense license, ProductFeature productFeature)
        {
            var featureName = productFeature.GetFeatureName();
            if (featureName == null)
            {
                return new LicenseKey(productFeature, license.Advanced.ConcurrentUsageLimit, license.ValidUntil, license.ActivationKey);
            }

            IFeature feature = license.Advanced.AllFeatures().Where(kvp => kvp.Key == featureName).Select(kvp => kvp.Value).FirstOrDefault();
            if (feature != null && feature.ValidUntil >= DateTime.Now)
            {
                return new LicenseKey(productFeature, feature.ConcurrentUsageLimit,
                    license.ValidUntil > feature.ValidUntil ? feature.ValidUntil : license.ValidUntil, // Minimum of the two
                    license.ActivationKey);
            }
            return null;
        }

        internal static LicenseKey Aggregate(IEnumerable<LicenseKey> keys)
        {
            var list = keys.Where(k => k != null).ToList();
            if (list.Any())
            {
                int? maximumLicenses;
                try
                {
                    maximumLicenses = list.Any(i => i.MaximumLicenses == null) ? null : list.Sum(i => i.MaximumLicenses);
                }
                catch (OverflowException)
                {
                    maximumLicenses = null;
                }
                return new LicenseKey(list.Select(i => i.ProductFeature).First(), maximumLicenses,
                    list.Max(k => k.ExpirationDate), list.Select(k => k.ActivationKey).First());
            }
            return null;
        }
    }
}
