using System;

namespace ServiceLibrary.Models.Enums
{
    [Flags]
    public enum PropertyTypeReuseTemplateSettings
    {
        None = 0x0,
        ReadOnly = 0x1,
        ChangesIgnored = 0x2
    }
}
