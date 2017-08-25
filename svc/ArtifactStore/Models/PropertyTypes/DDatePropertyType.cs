using System;
using ServiceLibrary.Models;
using ServiceLibrary.Models.PropertyType;

namespace ArtifactStore.Models.PropertyTypes
{
    public class DDatePropertyType : DPropertyType
    {
        public bool IsValidate { get; set; }
        public Range<DateTime?> Range { get; set; }
        public DateTime? DefaultValue { get; set; }
    }
}
