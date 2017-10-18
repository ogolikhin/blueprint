using System;
using System.Collections.Generic;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Models.Licenses
{
    public class FeatureInformation
    {
        public static Dictionary<string, FeatureTypes> Features = new Dictionary<string, FeatureTypes>
        {
            { "HP ALM Adapter", FeatureTypes.HewlettPackardQCIntegration },
            { "Microsoft TFS Adapter", FeatureTypes.MicrosoftTfsIntegration },
            { "Blueprint API", FeatureTypes.BlueprintOpenApi },
            { "Storyteller", FeatureTypes.Storyteller },
            { "Blueprint", FeatureTypes.Blueprint },
            { "Workflow", FeatureTypes.Workflow },
            { "None", FeatureTypes.None }
        };

        private readonly ITimeProvider _timeProvider;

        public FeatureInformation()
        {
            _timeProvider = new TimeProvider();
        }

        public FeatureInformation(string featureName, DateTime expirationDate, ITimeProvider timeProvider = null)
        {
            FeatureName = featureName;
            ExpirationDate = expirationDate;
            _timeProvider = timeProvider ?? new TimeProvider();
        }

        public string FeatureName { get; set; }

        public DateTime ExpirationDate { get; set; }

        public FeatureTypes GetFeatureType()
        {
            if (Features.ContainsKey(FeatureName))
            {
                return Features[FeatureName];
            }
            return FeatureTypes.None;
        }

        public FeatureLicenseStatus GetStatus()
        {
            return ExpirationDate.ToUniversalTime() >= _timeProvider.CurrentUniversalTime ? FeatureLicenseStatus.Active : FeatureLicenseStatus.Expired;
        }
    }
}
