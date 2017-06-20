using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceLibrary.Helpers
{
    [Flags]
    public enum FeatureTypes
    {
        None = 0x0,
        HewlettPackardQCIntegration = 0x1,
        MicrosoftTfsIntegration = 0x2,
        BlueprintOpenApi = 0x4,
        Storyteller = 0x8,
        Blueprint = 0x10
    }

    public enum BlueprintFeatureLicenseStatus
    {
        None,
        Active,
        Expired
    }

    public static class BlueprintFeatureLicenseTypeExt
    {
        public const string InishTechHewlettPackardQCIntegrationName = "HP ALM Adapter";
        public const string InishTechMicrosoftTfsName = "Microsoft TFS Adapter";
        public const string InishTechOpenApiName = "Blueprint API";
        public const string InishTechStorytellerName = "Storyteller";
        public const string InishTechBlueprintName = "Blueprint";

        public static IDictionary<FeatureTypes, string> BlueprintLicenseToInishTechLicense = new Dictionary<FeatureTypes, string>
        {
            { FeatureTypes.HewlettPackardQCIntegration, InishTechHewlettPackardQCIntegrationName},
            { FeatureTypes.MicrosoftTfsIntegration, InishTechMicrosoftTfsName},
            { FeatureTypes.BlueprintOpenApi, InishTechOpenApiName},
            { FeatureTypes.Storyteller, InishTechStorytellerName },
            { FeatureTypes.Blueprint, InishTechBlueprintName },
            { FeatureTypes.None, LicenseType.None.ToLicenseName()},
        };

        public static IDictionary<string, FeatureTypes> InishTechLicenseToBlueprintLicense = new Dictionary<string, FeatureTypes>
        {
            { InishTechHewlettPackardQCIntegrationName, FeatureTypes.HewlettPackardQCIntegration},
            { InishTechMicrosoftTfsName, FeatureTypes.MicrosoftTfsIntegration},
            { InishTechOpenApiName, FeatureTypes.BlueprintOpenApi},
            { InishTechStorytellerName, FeatureTypes.Storyteller },
            { InishTechBlueprintName, FeatureTypes.Blueprint },
            { LicenseType.None.ToLicenseName(), FeatureTypes.None },
        };

        public static IList<FeatureTypes> ValidLicenses
        {
            get
            {
                return (from FeatureTypes l in Enum.GetValues(typeof(FeatureTypes))
                    where l != FeatureTypes.None
                    select l).ToList();
            }
        }

        public static IEnumerable<FeatureTypes> GetLicenseTypeListToDisplay()
        {
            return ValidLicenses;
        }
    }
}
