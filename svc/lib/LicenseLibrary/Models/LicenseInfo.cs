using System;
using System.Collections.Generic;
using System.Linq;
using Sp.Agent.Licensing;

namespace LicenseLibrary.Models
{
    public class LicenseInfo
    {
        public ProductFeature ProductFeature { get; }
        public int? MaximumLicenses { get; }
        public DateTime ExpirationDate { get; }
        public string ActivationKey { get; }

        internal LicenseInfo(ProductFeature productFeature, int? maximumLicenses, DateTime expirationDate, string activationKey)
        {
            ProductFeature = productFeature;
            MaximumLicenses = maximumLicenses;
            ExpirationDate = expirationDate;
            ActivationKey = activationKey;
        }

        internal static LicenseInfo Get(ILicense license, ProductFeature productFeature)
        {
            var featureName = productFeature.GetFeatureName();
            if (featureName == null)
            {
                return new LicenseInfo(productFeature, license.Advanced.ConcurrentUsageLimit, license.ValidUntil, license.ActivationKey);
            }

            IFeature feature = license.Advanced.AllFeatures().Where(kvp => kvp.Key == featureName).Select(kvp => kvp.Value).FirstOrDefault();
            if (feature != null && feature.ValidUntil >= DateTime.Now)
            {
                return new LicenseInfo(productFeature, feature.ConcurrentUsageLimit,
                    license.ValidUntil > feature.ValidUntil ? feature.ValidUntil : license.ValidUntil, // Minimum of the two
                    license.ActivationKey);
            }
            return null;
        }

        internal static LicenseInfo Aggregate(IEnumerable<LicenseInfo> infos)
        {
            var list = infos.Where(i => i != null).ToList();
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
                return new LicenseInfo(list.Select(i => i.ProductFeature).First(), maximumLicenses,
                    list.Max(i => i.ExpirationDate), list.Select(i => i.ActivationKey).First());
            }
            return null;
        }
    }
}
