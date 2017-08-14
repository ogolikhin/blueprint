using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Helpers.Cache;
using ServiceLibrary.Repositories.ApplicationSettings;

namespace ServiceLibrary.Services
{
    public class FeaturesService : IFeaturesService
    {
        private static readonly string CacheKey = "FeaturesServiceCache";
        private static readonly TimeSpan CacheAbsoluteExpirationTime = TimeSpan.FromMinutes(20);

        private IFeaturesRepository _featuresRepository;
        private IAsyncCache _cache;
        private IFeatureLicenseHelper _licenseHelper;

        public FeaturesService(IFeaturesRepository featuresRepository, IFeatureLicenseHelper licenseHelper, IAsyncCache cache)
        {
            if (featuresRepository == null)
            {
                throw new ArgumentNullException(nameof(featuresRepository));
            }
            _featuresRepository = featuresRepository;

            if (licenseHelper == null)
            {
                throw new ArgumentNullException(nameof(licenseHelper));
            }
            _licenseHelper = licenseHelper;

            if (cache == null)
            {
                throw new ArgumentNullException(nameof(cache));
            }
            _cache = cache;
        }

        public FeaturesService() : this(new SqlFeaturesRepository(), FeatureLicenseHelper.Instance, AsyncCache.Default)
        {
        }

        public Task<IDictionary<string, bool>> GetFeaturesAsync()
        {
            return _cache.AddOrGetExistingAsync(CacheKey, LoadFeaturesAsync, DateTimeOffset.UtcNow.Add(CacheAbsoluteExpirationTime));
        }

        private async Task<IDictionary<string, bool>> LoadFeaturesAsync()
        {
            var features = (await _featuresRepository.GetFeaturesAsync())
                .ToDictionary(f => f.Name, f => f.Enabled);

            var isWorkflowEnabled = (_licenseHelper.GetValidBlueprintLicenseFeatures() & FeatureTypes.Workflow) != 0;
            features[ServiceConstants.WorkflowFeatureKey] = isWorkflowEnabled;

            return features;
        }
    }
}
