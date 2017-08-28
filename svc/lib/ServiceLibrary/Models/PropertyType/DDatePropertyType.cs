using System;

namespace ServiceLibrary.Models.PropertyType
{
    public class DDatePropertyType : DPropertyType
    {
        public bool IsValidate { get; set; }
        public Range<DateTime?> Range { get; set; }
        public DateTime? DefaultValue { get; set; }
    }
}
