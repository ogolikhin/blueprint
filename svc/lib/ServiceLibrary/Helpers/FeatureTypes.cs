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
        //public const string INISHTECH_HP_QC_LICENSE_NAME_2 = "HP ALM Adaptor";
        //public const string INISHTECH_MSF_TFS_LICENSE_NAME_2 = "Microsoft TFS Adaptor";

        public static IDictionary<FeatureTypes, string> BlueprintLicenseToInishTechLicense = new Dictionary<FeatureTypes, string>
        {
            { FeatureTypes.HewlettPackardQCIntegration, InishTechHewlettPackardQCIntegrationName},//INISHTECH_HP_QC_LICENSE_NAME_2} },
            { FeatureTypes.MicrosoftTfsIntegration, InishTechMicrosoftTfsName},//INISHTECH_MSF_TFS_LICENSE_NAME_2}},
            { FeatureTypes.BlueprintOpenApi, InishTechOpenApiName},
            { FeatureTypes.Storyteller, InishTechStorytellerName },
            { FeatureTypes.Blueprint, InishTechBlueprintName },
            { FeatureTypes.None, LicenseType.None.ToLicenseName()},
        };

        public static IDictionary<string, FeatureTypes> InishTechLicenseToBlueprintLicense = new Dictionary<string, FeatureTypes>
        {
            { InishTechHewlettPackardQCIntegrationName, FeatureTypes.HewlettPackardQCIntegration},
            //{INISHTECH_HP_QC_LICENSE_NAME_2, FeatureTypes.HewletPackardQCIntegration },
            { InishTechMicrosoftTfsName, FeatureTypes.MicrosoftTfsIntegration},
            //{INISHTECH_MSF_TFS_LICENSE_NAME_2, FeatureTypes.MicrosoftTfsIntegration},
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
