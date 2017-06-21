using System;

namespace ServiceLibrary.Models.Enums
{
    [Flags]
    public enum FeatureTypes
    {
        None = 0x0,
        HewlettPackardQCIntegration = 0x1,
        MicrosoftTfsIntegration = 0x2,
        BlueprintOpenApi = 0x4,
        Storyteller = 0x8,
        Blueprint = 0x10,
        Workflow = 0x20
    }
}
