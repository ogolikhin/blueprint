using System;
using System.Collections.Generic;
using ServiceLibrary.Models.Enums;

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
            const string inishTechHewlettPackardQcIntegrationName = "HP ALM Adapter";
            const string inishTechMicrosoftTfsName = "Microsoft TFS Adapter";
            const string inishTechOpenApiName = "Blueprint API";
            const string inishTechStorytellerName = "Storyteller";
            const string inishTechBlueprintName = "Blueprint";
            const string licenseTypeNone = "None";
            var inishTechLicenseToBlueprintLicense = new Dictionary<string, FeatureTypes>
            {
                {inishTechHewlettPackardQcIntegrationName, FeatureTypes.HewlettPackardQCIntegration},
                {inishTechMicrosoftTfsName, FeatureTypes.MicrosoftTfsIntegration},
                {inishTechOpenApiName, FeatureTypes.BlueprintOpenApi},
                {inishTechStorytellerName, FeatureTypes.Storyteller},
                {inishTechBlueprintName, FeatureTypes.Blueprint},
                {licenseTypeNone, FeatureTypes.None}
            };
            return inishTechLicenseToBlueprintLicense[FeatureName];
        }

        public FeatureLicenseStatus GetStatus()
        {
            return ExpirationDate.ToUniversalTime() >= DateTime.Now.ToUniversalTime() ? FeatureLicenseStatus.Active : FeatureLicenseStatus.Expired;
        }
    }
}
